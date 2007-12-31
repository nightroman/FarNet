/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2007 FAR.NET Team
*/

#include "StdAfx.h"
#include "EditorManager.h"
#include "Editor.h"
#include "Far.h"

namespace FarNet
{;
EditorManager::EditorManager()
{
	// versions
	DWORD vn; Info.AdvControl(Info.ModuleNumber, ACTL_GETFARVERSION, &vn);
	int v1 = (vn & 0x0000ff00)>>8, v2 = vn & 0x000000ff, v3 = (int)((long)vn&0xffff0000)>>16;
	if (v1 >= 1 && v2 >= 71 && v3 >= 2169)
		_version_1_71_2169 = true;
}

array<IEditor^>^ EditorManager::Editors()
{
	array<IEditor^>^ r = gcnew array<IEditor^>(_editors.Count);
	int i = 0;
	for each(Editor^ it in _editors.Values)
		r[i++] = it;
	return r;
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
	Editor^ r = gcnew Editor(this);
	r->Id = id;
	r->GetParams();

	// !! ?New File? is not removed (Close is not fired for it)
	if (!_version_1_71_2169 && r->FileName->EndsWith("?"))
	{
		for each(KeyValuePair<int, Editor^>^ i in _editors)
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
	Editor^ ed;
	if (_editors.TryGetValue(id, ed))
		return ed;
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
			if (_anyEditor._OnKey || editor->_OnKey)
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

int EditorManager::AsProcessEditorEvent(int type, void* param)
{
	switch(type)
	{
	case EE_REDRAW:
		{
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
	case EE_READ:
		LogLine(__FUNCTION__ " READ");
		{
			Editor^ ed = GetCurrentEditor();
			Far::Instance->OnEditorOpened(ed);
			if (_anyEditor._AfterOpen)
				_anyEditor._AfterOpen(ed, EventArgs::Empty);
			if (ed->_AfterOpen)
				ed->_AfterOpen(ed, EventArgs::Empty);
		}
		break;
	case EE_CLOSE:
		LogLine(__FUNCTION__ " CLOSE");
		{
			int id = *((int*)param);
			Editor^ ed;
			if (!_editors.TryGetValue(id, ed))
				return 0;
			if (_anyEditor._AfterClose)
				_anyEditor._AfterClose(ed, EventArgs::Empty);
			if (ed->_AfterClose)
				ed->_AfterClose(ed, EventArgs::Empty);

			// unregister
			_fastGetString = 0;
			_editorCurrent = nullptr;
			_editors.Remove(id);
			ed->Id = -1;
		}
		break;
	case EE_SAVE:
		LogLine(__FUNCTION__ " SAVE");
		{
			Editor^ ed = GetCurrentEditor();
			if (_anyEditor._BeforeSave)
				_anyEditor._BeforeSave(ed, EventArgs::Empty);
			if (ed->_BeforeSave)
				ed->_BeforeSave(ed, EventArgs::Empty);
		}
		break;
	case EE_GOTFOCUS:
		LogLine(__FUNCTION__ " GOTFOCUS");
		{
			int id = *((int*)param);
			Editor^ ed;
			if (!_editors.TryGetValue(id, ed))
				return 0;
			
			LogLine(ed->FileName);
			LogLine(GetCurrentEditor()->FileName);

			if (_anyEditor._GotFocus)
				_anyEditor._GotFocus(ed, EventArgs::Empty);
			if (ed->_GotFocus)
				ed->_GotFocus(ed, EventArgs::Empty);
		}
		break;
	case EE_KILLFOCUS:
		LogLine(__FUNCTION__ " KILLFOCUS");
		{
			int id = *((int*)param);
			Editor^ ed;
			if (!_editors.TryGetValue(id, ed))
				return 0;
			
			LogLine(ed->FileName);
			LogLine(GetCurrentEditor()->FileName);

			if (_anyEditor._LosingFocus)
				_anyEditor._LosingFocus(ed, EventArgs::Empty);
			if (ed->_LosingFocus)
				ed->_LosingFocus(ed, EventArgs::Empty);
		}
		break;
	}
	return 0;
}
}
