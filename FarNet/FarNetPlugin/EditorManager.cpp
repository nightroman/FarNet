#include "StdAfx.h"
#include "EditorManager.h"
#include "Editor.h"
#include "Utils.h"

namespace FarManagerImpl
{;
EditorManager::EditorManager()
{
	_anyEditor = gcnew BaseEditor();
	_editors = gcnew Dictionary<int, IEditor^>();
	_key = gcnew Key();
	_mouse = gcnew Mouse();

	// workarounds
	DWORD vn; Info.AdvControl(Info.ModuleNumber, ACTL_GETFARVERSION, &vn);
	int v1 = (vn & 0x0000ff00)>>8, v2 = vn & 0x000000ff, v3 = (int)((long)vn&0xffff0000)>>16;
	if (v1 < 1 || v2 < 71 || v3 < 2169)
		_before_1_71_2169 = true;
}

ICollection<IEditor^>^ EditorManager::Editors::get()
{
	return _editors->Values;
}

IAnyEditor^ EditorManager::AnyEditor::get()
{
	return _anyEditor;
}

Editor^ EditorManager::CreateEditor()
{
	return gcnew Editor(this);
}

int EditorManager::ProcessEditorInput(const INPUT_RECORD* rec)
{
	Editor^ e = GetCurrentEditor();
	int r = 0;
	if (rec->EventType == KEY_EVENT)
	{
		ToKey(rec);
		_anyEditor->FireOnKey(e, _key);
		e->FireOnKey(e, _key);
		r = _key->Ignore?1:0;
	}
	if (rec->EventType == MOUSE_EVENT)
	{
		ToMouse(rec);
		_anyEditor->FireOnMouse(e, _mouse);
		_anyEditor->FireOnMouse(e, _mouse);
		r = _mouse->Ignore ? 1 : 0;
	}
	return r;
}

int EditorManager::ProcessEditorEvent(int type, void* param)
{
	switch(type)
	{
	case EE_READ:
		{
			IEditor^ e = GetCurrentEditor();
			_anyEditor->FireAfterOpen(e);
			e->FireAfterOpen(e);
			return 0;
		}
	case EE_CLOSE:
		{
			int id = *((int*)param);
			Editor^ e = GetEditorById(id);
			_anyEditor->FireAfterClose(e);
			e->FireAfterClose(e);
			_editors->Remove(id);
			e->Id = -1;
			return 0;
		}
	case EE_SAVE:
		/*
		The file being edited is about to save. The plugin can use
		EditorControl commands to modify data before saving.
		Param equals NULL. Return value must be 0.
		*/
		{
			IEditor^ e = GetCurrentEditor();
			_anyEditor->FireBeforeSave(e);
			e->FireBeforeSave(e);
			return 0;
		}
	case EE_REDRAW:
		/*
		(Changed!) The editor screen is about to redraw. Plugin can
		use EditorControl ECTL_ADDCOLOR command to set line colors. When the
		edited text has not changed Param is equal to EEREDRAW_ALL, if the
		whole screen will be redrawn, or to EEREDRAW_LINE, if only the current
		line will be redrawn. After the edited text is changed, this event will
		be sent with Param is equal to EEREDRAW_CHANGE. Return value must be 0.
		Note: If plugin is setting line colors, then after receiving this
		event with Param equal to EEREDRAW_CHANGE it is recommended to use
		EditorControl ECTL_REDRAW command to avoid colors blinking.
		*/
		{
			int mode = (int)param;
			IEditor^ e = GetCurrentEditor();
			_anyEditor->FireOnRedraw(e, mode);
			e->FireOnRedraw(e, mode);
			return 0;
		}
	}
	return 0;
}

int EditorManager::CurrentEditorId()
{
	EditorInfo ei; EditorControl_ECTL_GETINFO(ei, true);
	return ei.EditorID;
}

Editor^ EditorManager::GetCurrentEditor()
{
	int id = CurrentEditorId();
	if (id < 0)
		return nullptr;

	if (_waiting != nullptr)
	{
		_waiting->Id = id;
		_editors[id] = _waiting;
		Editor^ r = _waiting;
		_waiting = nullptr;
		return r;
	}

	return GetEditorById(id);
}

Editor^ EditorManager::GetEditorById(int id)
{
	IEditor^ ed;
	if (_editors->TryGetValue(id, ed))
		return (Editor^)ed;
	else
		return CreateEditorById(id);
}

Editor^ EditorManager::CreateEditorById(int id)
{
	Editor^ r = CreateEditor();
	r->Id = id;
	r->GetParams();

	// !! ?New File? is not removed (Close is not fired for it)
	if (_before_1_71_2169 && r->FileName->EndsWith("?"))
	{
		IEnumerator<int>^ i = _editors->Keys->GetEnumerator();
		while(i->MoveNext())
		{
			IEditor^ e = _editors[i->Current];
			if (e->FileName->EndsWith("?"))
			{
				_editors->Remove(i->Current);
				break;
			}
		}
	}

	_editors[id] = r;
	return r;
}

void EditorManager::Wait(Editor^ editor)
{
	_waiting = editor;
}

bool has(int k, int c)
{
	return (k&c) != 0;
}

bool khas(KEY_EVENT_RECORD k,int c)
{
	return has(k.dwControlKeyState, c);
}

void SetCState(Controls^ k1, int k)
{
	k1->LAlt = has(k, LEFT_ALT_PRESSED);
	k1->RAlt = has(k, RIGHT_ALT_PRESSED);
	k1->LCtrl = has(k, LEFT_CTRL_PRESSED);
	k1->RCtrl = has(k, RIGHT_CTRL_PRESSED);
	k1->Shift = has(k, SHIFT_PRESSED);
	k1->NumLock = has(k, NUMLOCK_ON);
	k1->ScrollLock = has(k, SCROLLLOCK_ON);
	k1->Enhanced = has(k, ENHANCED_KEY);
	k1->CapsLock = has(k, CAPSLOCK_ON);
}

void EditorManager::ToKey(const INPUT_RECORD* rec)
{
	KEY_EVENT_RECORD k = rec->Event.KeyEvent;
	_key->Code = k.wVirtualKeyCode;
	_key->Scan = k.wVirtualScanCode;
	_key->Char = k.uChar.UnicodeChar;
	_key->Down = (k.bKeyDown&0xff) != 0; //!!!
	_key->Ignore = false;
	_key->RepeatCount = k.wRepeatCount; //!!!
	SetCState(_key, k.dwControlKeyState);
}

#define mhas(m,c) has(m.dwButtonState,c)
void EditorManager::ToMouse(const INPUT_RECORD* rec)
{
	MOUSE_EVENT_RECORD k = rec->Event.MouseEvent;
	_mouse->Line = k.dwMousePosition.X;
	_mouse->Pos = k.dwMousePosition.Y;

	_mouse->Left = mhas(k, FROM_LEFT_1ST_BUTTON_PRESSED);
	_mouse->Right = mhas(k, RIGHTMOST_BUTTON_PRESSED);
	_mouse->Click = k.dwEventFlags== 0;
	_mouse->DoubleClick = has(k.dwEventFlags, DOUBLE_CLICK);
	_mouse->Moved = has(k.dwEventFlags, MOUSE_MOVED);
	_mouse->Wheeled = has(k.dwEventFlags, MOUSE_WHEELED);
	SetCState(_mouse, k.dwControlKeyState);
	_mouse->Ignore = false;
}
}
