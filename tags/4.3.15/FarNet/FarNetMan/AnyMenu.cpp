/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "AnyMenu.h"

namespace FarNet
{;
AnyMenu::AnyMenu()
: _x(-1)
, _y(-1)
, _selected(-1)
{
	_items = gcnew List<FarItem^>;
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

IList<int>^ AnyMenu::BreakKeys::get()
{
	return %_keys;
}

int AnyMenu::BreakKey::get()
{
	return _breakKey;
}

FarItem^ AnyMenu::Add(String^ text)
{
	return Add(text, nullptr);
}

FarItem^ AnyMenu::Add(String^ text, EventHandler^ handler)
{
	FarItem^ r = gcnew SetItem;
	r->Text = text;
	r->Click = handler;
	Items->Add(r);
	return r;
}
}
