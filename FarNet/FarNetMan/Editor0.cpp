/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "Editor0.h"
#include "Editor.h"
#include "Wrappers.h"

namespace FarNet
{;
//! Values.CopyTo() is not used because of different return type.
array<IEditor^>^ Editor0::Editors()
{
	array<IEditor^>^ r = gcnew array<IEditor^>(_editors.Count);
	int i = 0;
	for each(Editor^ it in _editors.Values)
		r[i++] = it;
	return r;
}

//! For exturnal use.
Editor^ Editor0::GetCurrentEditor()
{
	AutoEditorInfo ei(true);

	// get current ID
	if (ei.EditorID < 0)
	{
		_editorCurrent = nullptr;
		return nullptr;
	}

	// take the cached editor
	if (_editorCurrent && _editorCurrent->Id == ei.EditorID)
		return _editorCurrent;

	// get registered, cache it, return
	_editorCurrent = _editors[ei.EditorID];
	return _editorCurrent;
}

int Editor0::AsProcessEditorEvent(int type, void* param)
{
	switch(type)
	{
	case EE_READ:
		{
			LOG_3("EE_READ");

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
			AutoEditorInfo ei;

			// register and cache the current
			_editors.Add(ei.EditorID, editor);
			_editorCurrent = editor;

			// 1) start the editor; it calls module editor actions, they may add handlers
			editor->Start(ei, isEditorWaiting);

			// 2) event for any editor handlers, they add this editor handlers
			if (_anyEditor._Opened)
			{
				LOG_AUTO(Info, "Opened")
				{
					_anyEditor._Opened(editor, nullptr);
				}
				LOG_END;
			}

			// 3) event for this editor handlers
			if (editor->_Opened)
			{
				LOG_AUTO(Info, "Opened")
				{
					editor->_Opened(editor, nullptr);
				}
				LOG_END;
			}
		}
		break;
	case EE_CLOSE:
		{
			LOG_3("EE_CLOSE");

			// get registered, close and unregister
			int id = *((int*)param);
			Editor^ editor = _editors[id];
			editor->Stop();
			_editors.Remove(id);
			_fastGetString = 0;
			_editorCurrent = nullptr;

			// end async
			editor->EndAsync();

			// event, after the above
			if (_anyEditor._Closed)
			{
				LOG_AUTO(Info, "Closed")
				{
					_anyEditor._Closed(editor, nullptr);
				}
				LOG_END;
			}
			if (editor->_Closed)
			{
				LOG_AUTO(Info, "Closed")
				{
					editor->_Closed(editor, nullptr);
				}
				LOG_END;
			}

			// delete the file after all
			DeleteSourceOptional(editor->FileName, editor->DeleteSource);
		}
		break;
	case EE_SAVE:
		{
			LOG_3("EE_SAVE");

			Editor^ ed = GetCurrentEditor();
			if (_anyEditor._Saving)
			{
				LOG_AUTO(Info, "Saving")
				{
					_anyEditor._Saving(ed, nullptr);
				}
				LOG_END;
			}
			if (ed->_Saving)
			{
				LOG_AUTO(Info, "Saving")
				{
					ed->_Saving(ed, nullptr);
				}
				LOG_END;
			}
		}
		break;
	case EE_REDRAW:
		{
			LOG_4("EE_REDRAW");

			int mode = (int)(INT_PTR)param;
			Editor^ editor = GetCurrentEditor();
			if (_anyEditor._Redrawing || editor->_Redrawing)
			{
				LOG_AUTO(Verbose, "Redrawing")
				{
					EditorRedrawingEventArgs ea(mode);
					if (_anyEditor._Redrawing)
						_anyEditor._Redrawing(editor, %ea);
					if (editor->_Redrawing)
						editor->_Redrawing(editor, %ea);
				}
				LOG_END;
			}

			editor->ApplyPendingChanges();
		}
		break;
	case EE_GOTFOCUS:
		{
			LOG_4("EE_GOTFOCUS");

			int id = *((int*)param);
			Editor^ editor;
			if (!_editors.TryGetValue(id, editor))
				return 0;

			// sync
			if (editor->_output)
				editor->Sync();

			// event
			if (_anyEditor._GotFocus)
			{
				LOG_AUTO(Verbose, "GotFocus")
				{
					_anyEditor._GotFocus(editor, nullptr);
				}
				LOG_END;
			}
			if (editor->_GotFocus)
			{
				LOG_AUTO(Verbose, "GotFocus")
				{
					editor->_GotFocus(editor, nullptr);
				}
				LOG_END;
			}
		}
		break;
	case EE_KILLFOCUS:
		{
			LOG_4("EE_KILLFOCUS");

			int id = *((int*)param);
			Editor^ ed;
			if (!_editors.TryGetValue(id, ed))
				return 0;

			if (_anyEditor._LosingFocus)
			{
				LOG_AUTO(Verbose, "LosingFocus")
				{
					_anyEditor._LosingFocus(ed, nullptr);
				}
				LOG_END;
			}
			if (ed->_LosingFocus)
			{
				LOG_AUTO(Verbose, "LosingFocus")
				{
					ed->_LosingFocus(ed, nullptr);
				}
				LOG_END;
			}
		}
		break;
	}
	return 0;
}

int Editor0::AsProcessEditorInput(const INPUT_RECORD* rec)
{
	Editor^ editor = GetCurrentEditor();
	while (_fastGetString > 0)
		_editorCurrent->EndAccess();

	// async
	if (editor->_output)
	{
		// sync
		editor->Sync();

		// ignore most of events
		if (editor->_hMutex)
		{
			if (rec->EventType != KEY_EVENT)
				return true;

			switch(rec->Event.KeyEvent.wVirtualKeyCode)
			{
			case VKeyCode::Escape:
			case VKeyCode::F10:
				return false;
			case VKeyCode::C:
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

	switch(rec->EventType)
	{
	case KEY_EVENT:
		{
			const KEY_EVENT_RECORD& key = rec->Event.KeyEvent;
			// idled
			if (key.wVirtualKeyCode == 0)
			{
				if (_anyEditor._Idled)
					_anyEditor._Idled(editor, nullptr);
				if (editor->_Idled)
					editor->_Idled(editor, nullptr);
			}
			// key down
			else if (key.bKeyDown) //! it was (bKeyDown & 0xff) != 0
			{
				if (_anyEditor._KeyDown || editor->_KeyDown)
				{
					KeyEventArgs ea(KeyInfo(key.wVirtualKeyCode, key.uChar.UnicodeChar, (ControlKeyStates)key.dwControlKeyState, true));
					if (_anyEditor._KeyDown)
						_anyEditor._KeyDown(editor, %ea);
					if (editor->_KeyDown)
						editor->_KeyDown(editor, %ea);
					return ea.Ignore;
				}
			}
			// key up
			else
			{
				if (_anyEditor._KeyUp || editor->_KeyUp)
				{
					KeyEventArgs ea(KeyInfo(key.wVirtualKeyCode, key.uChar.UnicodeChar, (ControlKeyStates)key.dwControlKeyState, false));
					if (_anyEditor._KeyUp)
						_anyEditor._KeyUp(editor, %ea);
					if (editor->_KeyUp)
						editor->_KeyUp(editor, %ea);
					return ea.Ignore;
				}
			}
			break;
		}
	case MOUSE_EVENT:
		{
			const MOUSE_EVENT_RECORD& e = rec->Event.MouseEvent;
			switch(e.dwEventFlags)
			{
			case 0:
				if (_anyEditor._MouseClick || editor->_MouseClick)
				{
					MouseEventArgs ea(GetMouseInfo(rec->Event.MouseEvent));
					if (_anyEditor._MouseClick)
						_anyEditor._MouseClick(editor, %ea);
					if (editor->_MouseClick)
						editor->_MouseClick(editor, %ea);
					return ea.Ignore;
				}
				break;
			case DOUBLE_CLICK:
				if (_anyEditor._MouseDoubleClick || editor->_MouseDoubleClick)
				{
					MouseEventArgs ea(GetMouseInfo(rec->Event.MouseEvent));
					if (_anyEditor._MouseDoubleClick)
						_anyEditor._MouseDoubleClick(editor, %ea);
					if (editor->_MouseDoubleClick)
						editor->_MouseDoubleClick(editor, %ea);
					return ea.Ignore;
				}
				break;
			case MOUSE_MOVED:
				if (_anyEditor._MouseMove || editor->_MouseMove)
				{
					MouseEventArgs ea(GetMouseInfo(rec->Event.MouseEvent));
					if (_anyEditor._MouseMove)
						_anyEditor._MouseMove(editor, %ea);
					if (editor->_MouseMove)
						editor->_MouseMove(editor, %ea);
					return ea.Ignore;
				}
				break;
			case MOUSE_WHEELED:
				if (_anyEditor._MouseWheel || editor->_MouseWheel)
				{
					MouseEventArgs ea(GetMouseInfo(rec->Event.MouseEvent));
					if (_anyEditor._MouseWheel)
						_anyEditor._MouseWheel(editor, %ea);
					if (editor->_MouseWheel)
						editor->_MouseWheel(editor, %ea);
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
