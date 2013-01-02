
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
*/

#include "StdAfx.h"
#include "AnyMenu.h"

namespace FarNet
{;
AnyMenu::AnyMenu()
: _x(-1)
, _y(-1)
, _selected(-1)
, _keyIndex(-1)
{
	_items = gcnew List<FarItem^>;
	WrapCursor = true; //! default is true, as recommended by Far API
}

int AnyMenu::X::get()
{
	return _x;
}

void AnyMenu::X::set(int value)
{
	_x = value;
}

int AnyMenu::Y::get()
{
	return _y;
}

void AnyMenu::Y::set(int value)
{
	_y = value;
}

Object^ AnyMenu::SelectedData::get()
{
	if (_selected < 0 || _selected >= _items->Count)
		return nullptr;
	return _items[_selected]->Data;
}

IList<FarItem^>^ AnyMenu::Items::get()
{
	return _items;
}

int AnyMenu::Selected::get()
{
	return _selected;
}

void AnyMenu::Selected::set(int value)
{
	_selected = value;
}

KeyData^ AnyMenu::Key::get()
{
	return _keyIndex < 0 ? KeyData::Empty : _keys[_keyIndex];
}

FarItem^ AnyMenu::Add(String^ text)
{
	return Add(text, nullptr);
}

FarItem^ AnyMenu::Add(String^ text, EventHandler<MenuEventArgs^>^ click)
{
	FarItem^ r = gcnew SetItem;
	r->Text = text;
	r->Click = click;
	Items->Add(r);
	return r;
}

void AnyMenu::AddKey(int virtualKeyCode)
{
	AddKey(virtualKeyCode, ControlKeyStates::None, nullptr);
}
void AnyMenu::AddKey(int virtualKeyCode, ControlKeyStates controlKeyState)
{
	AddKey(virtualKeyCode, controlKeyState, nullptr);
}
void AnyMenu::AddKey(int virtualKeyCode, ControlKeyStates controlKeyState, EventHandler<MenuEventArgs^>^ handler)
{
	_keys.Add(gcnew KeyData(virtualKeyCode, controlKeyState));
	_handlers.Add(handler);
}

}
