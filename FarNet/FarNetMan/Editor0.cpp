#include "StdAfx.h"
#include "Editor0.h"
#include "Editor.h"
#include "Wrappers.h"

namespace FarNet
{
//! CopyTo() is not used because of different return type.
//! But ToArray() seems to work.
array<IEditor^>^ Editor0::Editors()
{
	return _editors.ToArray();
}

//! For external use.
Editor^ Editor0::GetCurrentEditor()
{
	// get info
	AutoEditorInfo ei(-1, true);
	if (ei.EditorID == -1)
		return nullptr;

	// search for the connected editor
	for (int i = 0; i < _editors.Count; ++i)
	{
		if ((intptr_t)_editors[i]->Id == ei.EditorID)
			return _editors[i];
	}

	// create and connect; rare case, e.g. the editor is opened before the core is loaded
	//_110624_153138 http://forum.farmanager.com/viewtopic.php?f=8&t=6500
	Editor^ editor = gcnew Editor;
	ConnectEditor(editor, ei, false);
	return editor;
}

// INTERNAL
int Editor0::FindEditor(intptr_t id)
{
	for (int i = 0; i < _editors.Count; ++i)
		if (id == (intptr_t)_editors[i]->Id)
			return i;

	return -1;
}

IEditor^ Editor0::GetEditor(intptr_t id)
{
	for (int i = 0; i < _editors.Count; ++i)
		if (id == (intptr_t)_editors[i]->Id)
			return _editors[i];

	return nullptr;
}

void Editor0::ConnectEditor(Editor^ editor, const EditorInfo& ei, bool isEditorWaiting)
{
	// 0) first opening
	if (!_started)
	{
		_started = true;
		if (_anyEditor._FirstOpening)
		{
			_anyEditor._FirstOpening(editor, nullptr);
			_anyEditor._FirstOpening = nullptr;
		}
	}

	//_110624_153138 ignore already connected
	if (FindEditor(ei.EditorID) >= 0)
		return;

	// register
	_editors.Insert(0, editor);

	// 1) start the editor; it calls module editor actions, they may add handlers
	editor->Start(ei, isEditorWaiting);

	// 2) event for any editor handlers, they add this editor handlers
	if (_anyEditor._Opened)
		_anyEditor._Opened(editor, nullptr);

	// 3) event for this editor handlers
	if (editor->_Opened)
		editor->_Opened(editor, nullptr);
}

int Editor0::AsProcessEditorEvent(const ProcessEditorEventInfo* info)
{
	switch (info->Event)
	{
	case EE_READ:
	{
		// pop the waiting or create new
		Editor^ editor;
		bool isEditorWaiting;
		if (_editorWaiting)
		{
			editor = _editorWaiting;
			_editorWaiting = nullptr;
			isEditorWaiting = true;
		}
		else
		{
			editor = gcnew Editor;
			isEditorWaiting = false;
		}

		// get info
		AutoEditorInfo ei(info->EditorID);

		// connect
		ConnectEditor(editor, ei, isEditorWaiting);
	}
	break;
	case EE_CLOSE:
	{
		// get registered, stop, unregister
		int index = FindEditor(info->EditorID);
		Editor^ editor = _editors[index];
		_editors.RemoveAt(index);
		editor->Stop();

		// end async
		editor->EndAsync();

		// events, after the above
		if (_anyEditor._Closed)
			_anyEditor._Closed(editor, nullptr);
		if (editor->_Closed)
			editor->_Closed(editor, nullptr);

		// delete the file after all
		DeleteSourceOptional(editor->FileName, editor->DeleteSource);
	}
	break;
	case EE_SAVE:
	{
		Editor^ editor = _editors[FindEditor(info->EditorID)];
		editor->_TimeOfSave = DateTime::Now;

		if (_anyEditor._Saving || editor->_Saving)
		{
			EditorSaveFile* esf = (EditorSaveFile*)info->Param;
			EditorSavingEventArgs ea(gcnew String(esf->FileName), (int)esf->CodePage);
			if (_anyEditor._Saving)
				_anyEditor._Saving(editor, % ea);
			if (editor->_Saving)
				editor->_Saving(editor, % ea);
		}
	}
	break;
	case EE_CHANGE:
	{
		Editor^ editor = _editors[FindEditor(info->EditorID)];

		++editor->_ChangeCount;

		if (_anyEditor._Changed || editor->_Changed)
		{
			EditorChange* ec = (EditorChange*)info->Param;
			EditorChangedEventArgs ea((EditorChangeKind)ec->Type, (int)ec->StringNumber);
			if (_anyEditor._Changed)
				_anyEditor._Changed(editor, % ea);
			if (editor->_Changed)
				editor->_Changed(editor, % ea);
		}
	}
	break;
	case EE_REDRAW:
	{
		// Far 3.0.4027 EE_REDRAW is called before EE_READ
		int index = FindEditor(info->EditorID);
		if (index < 0)
			break;
		Editor^ editor = _editors[index];

		if (_anyEditor._Redrawing || editor->_Redrawing || editor->_drawers)
		{
			if (_anyEditor._Redrawing)
				_anyEditor._Redrawing(editor, nullptr);

			if (editor->_Redrawing)
				editor->_Redrawing(editor, nullptr);

			if (editor->_drawers)
				editor->InvokeDrawers();
		}
	}
	break;
	case EE_GOTFOCUS:
	{
		int index = FindEditor(info->EditorID);
		Editor^ editor = index < 0 ? nullptr : _editors[index];

		//_110624_153138 rare case
		if (!editor)
		{
			// EE_READ will be called anyway
			if (_editorWaiting)
				break;

			editor = GetCurrentEditor();
			if ((intptr_t)editor->Id != info->EditorID)
				break;
		}

		editor->_TimeOfGotFocus = DateTime::Now;

		if (editor->_asyncText)
			editor->Sync();

		if (_anyEditor._GotFocus)
			_anyEditor._GotFocus(editor, nullptr);

		if (editor->_GotFocus)
			editor->_GotFocus(editor, nullptr);
	}
	break;
	case EE_KILLFOCUS:
	{
		int index = FindEditor(info->EditorID);
		Editor^ editor = index < 0 ? nullptr : _editors[index];

		//_110624_153138 rare case
		if (!editor)
		{
			editor = GetCurrentEditor();
			if ((intptr_t)editor->Id != info->EditorID)
				break;
		}

		if (_anyEditor._LosingFocus)
			_anyEditor._LosingFocus(editor, nullptr);

		if (editor->_LosingFocus)
			editor->_LosingFocus(editor, nullptr);
	}
	break;
	}

	return 0;
}

int Editor0::AsProcessEditorInput(const ProcessEditorInputInfo* info)
{
	const INPUT_RECORD* rec = &info->Rec;
	Editor^ editor = GetCurrentEditor();

	// exiting Far with not active editor
	if (editor == nullptr)
		return 0;

	// async
	if (editor->_asyncText)
	{
		// sync
		editor->Sync();

		// ignore most of events
		if (editor->_asyncMutex)
		{
			if (rec->EventType != KEY_EVENT)
				return true;

			switch (rec->Event.KeyEvent.wVirtualKeyCode)
			{
			case KeyCode::Escape:
			case KeyCode::F10:
				return false;
			case KeyCode::C:
				if ((rec->Event.KeyEvent.dwControlKeyState & int(ControlKeyStates::CtrlAltShift)) == int(ControlKeyStates::LeftCtrlPressed))
				{
					if (editor->_CtrlCPressed)
						editor->_CtrlCPressed(editor, nullptr);
				}
				return true;
			default:
				return true;
			}
		}
	}

	switch (rec->EventType)
	{
	case KEY_EVENT:
	{
		const KEY_EVENT_RECORD& key = rec->Event.KeyEvent;
		// key down
		if (key.bKeyDown) //! it was (bKeyDown & 0xff) != 0
		{
			if (_anyEditor._KeyDown || editor->_KeyDown)
			{
				KeyEventArgs ea(gcnew KeyInfo(key.wVirtualKeyCode, key.uChar.UnicodeChar, (ControlKeyStates)key.dwControlKeyState, true));
				if (_anyEditor._KeyDown)
					_anyEditor._KeyDown(editor, % ea);
				if (editor->_KeyDown)
					editor->_KeyDown(editor, % ea);
				return ea.Ignore;
			}
		}
		// key up
		else
		{
			if (_anyEditor._KeyUp || editor->_KeyUp)
			{
				KeyEventArgs ea(gcnew KeyInfo(key.wVirtualKeyCode, key.uChar.UnicodeChar, (ControlKeyStates)key.dwControlKeyState, false));
				if (_anyEditor._KeyUp)
					_anyEditor._KeyUp(editor, % ea);
				if (editor->_KeyUp)
					editor->_KeyUp(editor, % ea);
				return ea.Ignore;
			}
		}
		break;
	}
	case MOUSE_EVENT:
	{
		const MOUSE_EVENT_RECORD& e = rec->Event.MouseEvent;
		switch (e.dwEventFlags)
		{
		case 0:
			_isLastClickEvent = true;
			if (_anyEditor._MouseClick || editor->_MouseClick)
			{
				MouseEventArgs ea(GetMouseInfo(rec->Event.MouseEvent));
				if (_anyEditor._MouseClick)
					_anyEditor._MouseClick(editor, % ea);
				if (editor->_MouseClick)
					editor->_MouseClick(editor, % ea);
				return ea.Ignore;
			}
			break;
		case DOUBLE_CLICK:
			_isLastClickEvent = false;
			if (_anyEditor._MouseDoubleClick || editor->_MouseDoubleClick)
			{
				MouseEventArgs ea(GetMouseInfo(rec->Event.MouseEvent));
				if (_anyEditor._MouseDoubleClick)
					_anyEditor._MouseDoubleClick(editor, % ea);
				if (editor->_MouseDoubleClick)
					editor->_MouseDoubleClick(editor, % ea);
				return ea.Ignore;
			}
			break;
		case MOUSE_MOVED:
			//! skip `moved` after `click` -- https://github.com/FarGroup/FarManager/issues/1063
			if (_isLastClickEvent)
			{
				_isLastClickEvent = false;
				break;
			}

			if (_anyEditor._MouseMove || editor->_MouseMove)
			{
				MouseEventArgs ea(GetMouseInfo(rec->Event.MouseEvent));
				if (_anyEditor._MouseMove)
					_anyEditor._MouseMove(editor, % ea);
				if (editor->_MouseMove)
					editor->_MouseMove(editor, % ea);
				return ea.Ignore;
			}
			break;
		case MOUSE_WHEELED:
			_isLastClickEvent = false;
			if (_anyEditor._MouseWheel || editor->_MouseWheel)
			{
				MouseEventArgs ea(GetMouseInfo(rec->Event.MouseEvent));
				if (_anyEditor._MouseWheel)
					_anyEditor._MouseWheel(editor, % ea);
				if (editor->_MouseWheel)
					editor->_MouseWheel(editor, % ea);
				return ea.Ignore;
			}
			break;
		}
		break;
	}
	}

	return 0;
}
}
