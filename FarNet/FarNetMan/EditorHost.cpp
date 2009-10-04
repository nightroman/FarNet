/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#include "StdAfx.h"
#include "EditorHost.h"
#include "Editor.h"
#include "Far.h"
#include "Wrappers.h"

namespace FarNet
{;
//! Values.CopyTo() is not used because of different return type.
array<IEditor^>^ EditorHost::Editors()
{
	array<IEditor^>^ r = gcnew array<IEditor^>(_editors.Count);
	int i = 0;
	for each(Editor^ it in _editors.Values)
		r[i++] = it;
	return r;
}

//! For exturnal use.
Editor^ EditorHost::GetCurrentEditor()
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

int EditorHost::AsProcessEditorEvent(int type, void* param)
{
	switch(type)
	{
	case EE_READ:
		{
			LOG_AUTO(3, "EE_READ");

			// take waiting or create new
			Editor^ editor;
			if (_editorWaiting)
			{
				editor = _editorWaiting;
				_editorWaiting = nullptr;
			}
			else
			{
				editor = gcnew Editor;
			}
			
			// get info
			AutoEditorInfo ei;

			// register and cache
			_editors.Add(ei.EditorID, editor);
			_editorCurrent = editor;

			// set info
			editor->_id = ei.EditorID;
			CBox fileName(Info.EditorControl(ECTL_GETFILENAME, 0));
			Info.EditorControl(ECTL_GETFILENAME, fileName);
			editor->_FileName = gcnew String(fileName);

			// event
			Far::Instance->OnEditorOpened(editor);
			if (_anyEditor._Opened)
				_anyEditor._Opened(editor, EventArgs::Empty);
			if (editor->_Opened)
				editor->_Opened(editor, EventArgs::Empty);
		}
		break;
	case EE_CLOSE:
		{
			LOG_AUTO(3, "EE_CLOSE");

			// get registered, close and unregister
			int id = *((int*)param);
			Editor^ editor = _editors[id];
			editor->_id = -2;
			_editors.Remove(id);
			_fastGetString = 0;
			_editorCurrent = nullptr;

			// end async
			editor->EndAsync();

			// event, after the above
			if (_anyEditor._Closed)
				_anyEditor._Closed(editor, EventArgs::Empty);
			if (editor->_Closed)
				editor->_Closed(editor, EventArgs::Empty);

			// delete the file after all
			DeleteSourceOptional(editor->_FileName, editor->DeleteSource);
		}
		break;
	case EE_REDRAW:
		{
			LOG_AUTO(4, "EE_REDRAW");

			int mode = (int)(INT_PTR)param;
			Editor^ ed = GetCurrentEditor();
			if (_anyEditor._OnRedraw)
			{
				RedrawEventArgs ea(mode);
				_anyEditor._OnRedraw(ed, %ea);
			}
			if (ed->_OnRedraw)
			{
				RedrawEventArgs ea(mode);
				ed->_OnRedraw(ed, %ea);
			}
		}
		break;
	case EE_SAVE:
		{
			LOG_AUTO(3, "EE_SAVE");

			Editor^ ed = GetCurrentEditor();
			if (_anyEditor._Saving)
				_anyEditor._Saving(ed, EventArgs::Empty);
			if (ed->_Saving)
				ed->_Saving(ed, EventArgs::Empty);
		}
		break;
	case EE_GOTFOCUS:
		{
			LOG_AUTO(3, "EE_GOTFOCUS");

			int id = *((int*)param);
			Editor^ editor;
			if (!_editors.TryGetValue(id, editor))
				return 0;

			// sync
			if (editor->_output)
				editor->Sync();
			
			// event
			if (_anyEditor._GotFocus)
				_anyEditor._GotFocus(editor, EventArgs::Empty);
			if (editor->_GotFocus)
				editor->_GotFocus(editor, EventArgs::Empty);
		}
		break;
	case EE_KILLFOCUS:
		{
			LOG_AUTO(3, "EE_KILLFOCUS");

			int id = *((int*)param);
			Editor^ ed;
			if (!_editors.TryGetValue(id, ed))
				return 0;
			
			if (_anyEditor._LosingFocus)
				_anyEditor._LosingFocus(ed, EventArgs::Empty);
			if (ed->_LosingFocus)
				ed->_LosingFocus(ed, EventArgs::Empty);
		}
		break;
	}
	return 0;
}

int EditorHost::AsProcessEditorInput(const INPUT_RECORD* rec)
{
	Editor^ editor = GetCurrentEditor();
	while (_fastGetString > 0)
		_editorCurrent->End();

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
			// idled
			if (rec->Event.KeyEvent.wVirtualKeyCode == 0)
			{
				if (_anyEditor._Idled)
					_anyEditor._Idled(editor, nullptr);
				if (editor->_Idled)
					editor->_Idled(editor, nullptr);
			}
			// a key
			else if (_anyEditor._OnKey || editor->_OnKey)
			{
				KeyEventArgs ea(KeyInfo(
					rec->Event.KeyEvent.wVirtualKeyCode,
					rec->Event.KeyEvent.uChar.UnicodeChar,
					(ControlKeyStates)rec->Event.KeyEvent.dwControlKeyState,
					(rec->Event.KeyEvent.bKeyDown&0xff) != 0));
				if (_anyEditor._OnKey)
					_anyEditor._OnKey(editor, %ea);
				if (editor->_OnKey)
					editor->_OnKey(editor, %ea);
				return ea.Ignore;
			}
			break;
		}
	case MOUSE_EVENT:
		{
			if (_anyEditor._OnMouse || editor->_OnMouse)
			{
				MouseEventArgs ea(GetMouseInfo(rec->Event.MouseEvent));
				if (_anyEditor._OnMouse)
					_anyEditor._OnMouse(editor, %ea);
				if (editor->_OnMouse)
					editor->_OnMouse(editor, %ea);
				return ea.Ignore;
			}
			break;
		}
	}

	return 0;
}

}
