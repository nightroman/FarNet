/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#include "StdAfx.h"
#include "EditorManager.h"
#include "Editor.h"

namespace FarManagerImpl
{;
EditorManager::EditorManager()
{
	// versions
	DWORD vn; Info.AdvControl(Info.ModuleNumber, ACTL_GETFARVERSION, &vn);
	int v1 = (vn & 0x0000ff00)>>8, v2 = vn & 0x000000ff, v3 = (int)((long)vn&0xffff0000)>>16;
	if (v1 >= 1 && v2 >= 71 && v3 >= 2169)
		_version_1_71_2169 = true;
}

ICollection<IEditor^>^ EditorManager::Editors::get()
{
	return _editors.Values;
}

IAnyEditor^ EditorManager::AnyEditor::get()
{
	return %_anyEditor;
}

Editor^ EditorManager::CreateEditor()
{
	return gcnew Editor(this);
}

Editor^ EditorManager::GetCurrentEditor()
{
	// get current ID
	EditorInfo ei; EditorControl_ECTL_GETINFO(ei, true);
	if (ei.EditorID < 0)
	{
		_editorCurrent = nullptr;
		return nullptr;
	}

	// take cached editor
	if (_editorCurrent && _editorCurrent->Id == ei.EditorID)
		return _editorCurrent;

	// process waiting editor or get editor by ID
	if (_editorWaiting)
	{
		_editorWaiting->Id = ei.EditorID;
		_editors[ei.EditorID] = _editorWaiting;
		_editorCurrent = _editorWaiting;
		_editorWaiting = nullptr;
	}
	else
	{
		_editorCurrent = GetOrCreateEditorById(ei.EditorID);
	}

	// cached current editor
	return _editorCurrent;
}

Editor^ EditorManager::CreateEditorById(int id)
{
	Editor^ r = CreateEditor();
	r->Id = id;
	r->GetParams();

	// !! ?New File? is not removed (Close is not fired for it)
	if (!_version_1_71_2169 && r->FileName->EndsWith("?"))
	{
		for each(KeyValuePair<int, IEditor^>^ i in _editors)
		{
			if (i->Value->FileName->EndsWith("?"))
			{
				_editors.Remove(i->Key);
				break;
			}
		}
	}

	_editors[id] = r;
	return r;
}

Editor^ EditorManager::GetOrCreateEditorById(int id)
{
	IEditor^ ed;
	if (_editors.TryGetValue(id, ed))
		return (Editor^)ed;
	else
		return CreateEditorById(id);
}

void EditorManager::SetWaitingEditor(Editor^ editor)
{
	_editorWaiting = editor;
}

int EditorManager::AsProcessEditorInput(const INPUT_RECORD* rec)
{
	Editor^ editor = GetCurrentEditor();
	while (_fastGetString > 0)
		_editorCurrent->End();

	switch(rec->EventType)
	{
	case KEY_EVENT:
		{
			if (_anyEditor._onKey != nullptr || editor->_onKey != nullptr)
			{
				KeyEventArgs ea(KeyInfo(
					rec->Event.KeyEvent.wVirtualKeyCode,
					rec->Event.KeyEvent.uChar.UnicodeChar,
					(ControlKeyStates)rec->Event.KeyEvent.dwControlKeyState,
					(rec->Event.KeyEvent.bKeyDown&0xff) != 0));
				if (_anyEditor._onKey != nullptr)
					_anyEditor._onKey(editor, %ea);
				if (editor->_onKey != nullptr)
					editor->_onKey(editor, %ea);
				return ea.Ignore;
			}
			break;
		}
	case MOUSE_EVENT:
		{
			if (_anyEditor._onMouse != nullptr || editor->_onMouse != nullptr)
			{
				MouseEventArgs ea(GetMouseInfo(rec->Event.MouseEvent));
				if (_anyEditor._onMouse != nullptr)
					_anyEditor._onMouse(editor, %ea);
				if (editor->_onMouse != nullptr)
					editor->_onMouse(editor, %ea);
				return ea.Ignore;
			}
			break;
		}
	}

	return 0;
}

int EditorManager::AsProcessEditorEvent(int type, void* param)
{
	switch(type)
	{
	case EE_READ:
		{
			Editor^ ed = GetCurrentEditor();
			if (_anyEditor._afterOpen)
				_anyEditor._afterOpen(ed, EventArgs::Empty);
			if (ed->_afterOpen)
				ed->_afterOpen(ed, EventArgs::Empty);
			break;
		}
	case EE_CLOSE:
		{
			int id = *((int*)param);
			IEditor^ ie;
			if (!_editors.TryGetValue(id, ie))
				return 0;
			Editor^ ed = (Editor^)ie;
			if (_anyEditor._afterClose)
				_anyEditor._afterClose(ed, EventArgs::Empty);
			if (ed->_afterClose)
				ed->_afterClose(ed, EventArgs::Empty);

			// unregister
			_fastGetString = 0;
			_editorCurrent = nullptr;
			_editors.Remove(id);
			ed->Id = -1;
			break;
		}
	case EE_SAVE:
		{
			Editor^ ed = GetCurrentEditor();
			if (_anyEditor._beforeSave)
				_anyEditor._beforeSave(ed, EventArgs::Empty);
			if (ed->_beforeSave)
				ed->_beforeSave(ed, EventArgs::Empty);
			break;
		}
	case EE_REDRAW:
		{
			int mode = (int)param;
			Editor^ ed = GetCurrentEditor();
			if (_anyEditor._onRedraw)
			{
				RedrawEventArgs ea(mode);
				_anyEditor._onRedraw(ed, %ea);
			}
			if (ed->_onRedraw)
			{
				RedrawEventArgs ea(mode);
				ed->_onRedraw(ed, %ea);
			}
			break;
		}
	}
	return 0;
}
}
