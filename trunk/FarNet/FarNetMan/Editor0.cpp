/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "Editor0.h"
#include "Editor.h"
#include "Far0.h"
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
				LOG_AUTO(3, "Opened");
				
				_anyEditor._Opened(editor, nullptr);
			}

			// 3) event for this editor handlers
			if (editor->_Opened)
			{
				LOG_AUTO(3, "Opened");
				
				editor->_Opened(editor, nullptr);
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
				LOG_AUTO(3, "Closed");
				
				_anyEditor._Closed(editor, nullptr);
			}
			if (editor->_Closed)
			{
				LOG_AUTO(3, "Closed");
				
				editor->_Closed(editor, nullptr);
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
				LOG_AUTO(3, "Saving");
				
				_anyEditor._Saving(ed, nullptr);
			}
			if (ed->_Saving)
			{
				LOG_AUTO(3, "Saving");
				
				ed->_Saving(ed, nullptr);
			}
		}
		break;
	case EE_REDRAW:
		{
			LOG_4("EE_REDRAW");

			int mode = (int)(INT_PTR)param;
			Editor^ ed = GetCurrentEditor();
			if (_anyEditor._OnRedraw)
			{
				LOG_AUTO(4, "OnRedraw");
				
				RedrawEventArgs ea(mode);
				_anyEditor._OnRedraw(ed, %ea);
			}
			if (ed->_OnRedraw)
			{
				LOG_AUTO(4, "OnRedraw");
				
				RedrawEventArgs ea(mode);
				ed->_OnRedraw(ed, %ea);
			}
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
				LOG_AUTO(4, "GotFocus");
				
				_anyEditor._GotFocus(editor, nullptr);
			}
			if (editor->_GotFocus)
			{
				LOG_AUTO(4, "GotFocus");

				editor->_GotFocus(editor, nullptr);
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
				LOG_AUTO(4, "LosingFocus");
				
				_anyEditor._LosingFocus(ed, nullptr);
			}
			if (ed->_LosingFocus)
			{
				LOG_AUTO(4, "LosingFocus");

				ed->_LosingFocus(ed, nullptr);
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
