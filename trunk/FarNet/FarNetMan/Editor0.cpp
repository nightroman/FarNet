
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
//! CopyTo() is not used because of different return type.
//! But ToArray() seems to work.
array<IEditor^>^ Editor0::Editors()
{
	return _editors.ToArray();
}

//! For exturnal use.
Editor^ Editor0::GetCurrentEditor()
{
	AutoEditorInfo ei(true);

	for(int i = 0; i < _editors.Count; ++i)
	{
		if (_editors[i]->Id == ei.EditorID)
			return _editors[i];
	}

	return nullptr;
}

int Editor0::AsProcessEditorEvent(int type, void* param)
{
	switch(type)
	{
	case EE_READ:
		{
			Log::Source->TraceInformation("EE_READ");

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

			// register
			_editors.Insert(0, editor);

			// 1) start the editor; it calls module editor actions, they may add handlers
			editor->Start(ei, isEditorWaiting);

			// 2) event for any editor handlers, they add this editor handlers
			if (_anyEditor._Opened)
			{
				Log::Source->TraceInformation("Opened");
				_anyEditor._Opened(editor, nullptr);
			}

			// 3) event for this editor handlers
			if (editor->_Opened)
			{
				Log::Source->TraceInformation("Opened");
				editor->_Opened(editor, nullptr);
			}
		}
		break;
	case EE_CLOSE:
		{
			Log::Source->TraceInformation("EE_CLOSE");

			// get registered, stop, unregister
			int id = *((int*)param);
			Editor^ editor = nullptr;
			for(int i = 0; i < _editors.Count; ++i)
			{
				if (_editors[i]->Id == id)
				{
					editor = _editors[i];
					_editors.RemoveAt(i);
					editor->Stop();
					break;
				}
			}

			// end async
			editor->EndAsync();

			// event, after the above
			if (_anyEditor._Closed)
			{
				Log::Source->TraceInformation("Closed");
				_anyEditor._Closed(editor, nullptr);
			}
			if (editor->_Closed)
			{
				Log::Source->TraceInformation("Closed");
				editor->_Closed(editor, nullptr);
			}

			// delete the file after all
			DeleteSourceOptional(editor->FileName, editor->DeleteSource);
		}
		break;
	case EE_SAVE:
		{
			Log::Source->TraceInformation("EE_SAVE");

			Editor^ ed = GetCurrentEditor();
			ed->_TimeOfSave = DateTime::Now;

			if (_anyEditor._Saving)
			{
				Log::Source->TraceInformation("Saving");
				_anyEditor._Saving(ed, nullptr);
			}
			if (ed->_Saving)
			{
				Log::Source->TraceInformation("Saving");
				ed->_Saving(ed, nullptr);
			}
		}
		break;
	case EE_REDRAW:
		{
			Log::Source->TraceEvent(TraceEventType::Verbose, 0, "EE_REDRAW");

			Editor^ editor = GetCurrentEditor();

			if (param == EEREDRAW_CHANGE)
				++editor->_KeyCount;

			if (_anyEditor._Redrawing || editor->_Redrawing)
			{
				Log::Source->TraceEvent(TraceEventType::Verbose, 0, "Redrawing");
				EditorRedrawingEventArgs ea((int)(INT_PTR)param);
				if (_anyEditor._Redrawing)
					_anyEditor._Redrawing(editor, %ea);
				if (editor->_Redrawing)
					editor->_Redrawing(editor, %ea);
			}
		}
		break;
	case EE_GOTFOCUS:
		{
			Log::Source->TraceEvent(TraceEventType::Verbose, 0, "EE_GOTFOCUS");

			int id = *((int*)param);
			Editor^ editor = nullptr;
			for(int i = 0; i < _editors.Count; ++i)
			{
				if (_editors[i]->Id == id)
				{
					editor = _editors[i];
					if (i > 0)
					{
						_editors.RemoveAt(i);
						_editors.Insert(0, editor);
					}
					break;
				}
			}

			// sync
			if (editor->_output)
				editor->Sync();

			// event
			if (_anyEditor._GotFocus)
			{
				Log::Source->TraceEvent(TraceEventType::Verbose, 0, "GotFocus");
				_anyEditor._GotFocus(editor, nullptr);
			}
			if (editor->_GotFocus)
			{
				Log::Source->TraceEvent(TraceEventType::Verbose, 0, "GotFocus");
				editor->_GotFocus(editor, nullptr);
			}
		}
		break;
	case EE_KILLFOCUS:
		{
			Log::Source->TraceEvent(TraceEventType::Verbose, 0, "EE_KILLFOCUS");

			int id = *((int*)param);
			Editor^ editor = nullptr;
			for(int i = 0; i < _editors.Count; ++i)
			{
				if (_editors[i]->Id == id)
				{
					editor = _editors[i];
					break;
				}
			}

			if (_anyEditor._LosingFocus)
			{
				Log::Source->TraceEvent(TraceEventType::Verbose, 0, "LosingFocus");
				_anyEditor._LosingFocus(editor, nullptr);
			}
			if (editor->_LosingFocus)
			{
				Log::Source->TraceEvent(TraceEventType::Verbose, 0, "LosingFocus");
				editor->_LosingFocus(editor, nullptr);
			}
		}
		break;
	}
	return 0;
}

int Editor0::AsProcessEditorInput(const INPUT_RECORD* rec)
{
	Editor^ editor = GetCurrentEditor();

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
