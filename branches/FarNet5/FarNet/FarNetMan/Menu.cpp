
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "Menu.h"

namespace FarNet
{;
Menu::Menu()
{}

// Dispose of managed resources.
// Call C++ finalizer to clean up unmanaged resources.
// Mark the class as disposed (manually) to throw an exception if a disposed object is accessed.
Menu::~Menu()
{
	this->!Menu();
}

// The C++ finalizer destructor ensures that unmanaged resources get
// released if the user releases the object without explicitly disposing of it.
Menu::!Menu()
{
	Unlock();
}

int* Menu::CreateBreakKeys()
{
	int* r = NULL;
	int nKey = _keys.Count;
	if (nKey > 0)
	{
		r = new int[nKey + 1];
		int i = 0;
		for each(int k in _keys)
			r[i++] = k;
		r[i] = 0;
	}
	return r;
}

int Menu::Flags()
{
	int r = FMENU_USEEXT;
	if (ShowAmpersands)
		r |= FMENU_SHOWAMPERSAND;
	if (WrapCursor)
		r |= FMENU_WRAPMODE;
	if (AutoAssignHotkeys)
		r |= FMENU_AUTOHIGHLIGHT;
	if (ReverseAutoAssign)
		r |= FMENU_REVERSEAUTOHIGHLIGHT;
	if (ChangeConsoleTitle)
		r |= FMENU_CHANGECONSOLETITLE;
	return r;
}

void Menu::Lock()
{
	// locked?
	if (_createdItems)
		return;

	_createdItems = CreateItems();
	_createdBreaks = CreateBreakKeys();

	_help = NewChars(HelpTopic);
	_title = NewChars(Title);
	_bottom = NewChars(Bottom);
}

void Menu::Unlock()
{
	// not locked?
	if (!_createdItems)
		return;

	DeleteItems(_createdItems);
	delete _createdBreaks;
	delete _help;
	delete _title;
	delete _bottom;
	_createdItems = 0;
	_createdBreaks = 0;
	_help = 0;
	_title = 0;
	_bottom = 0;
}

FarMenuItemEx* Menu::CreateItems()
{
	int n = 0;
	FarMenuItemEx* r = new struct FarMenuItemEx[_items->Count];
	for each(FarItem^ item1 in _items)
	{
		FarMenuItemEx& item2 = r[n];
		item2.Text = NewChars(item1->Text);
		item2.AccelKey = 0;
		item2.Reserved = 0;
		++n;
	}
	return r;
}

void Menu::DeleteItems(FarMenuItemEx* items)
{
	if (items)
	{
		for(int i = Items->Count; --i >= 0;)
			delete items[i].Text;
		delete items;
	}
}

void Menu::ShowMenu(FarMenuItemEx* items, const int* breaks, const wchar_t* title, const wchar_t* bottom, const wchar_t* help)
{
	// validate X, Y to avoid crashes and out of screen
	int x = _x < 0 ? -1 : _x < 2 ? 2 : _x;
	int y = _y;
	if (y != -1)
	{
		int yMax = Far::Net->UI->WindowSize.Y - Math::Max(_items->Count, MaxHeight) - 4;
		if (y > yMax)
			y = yMax;
		if (y < 0)
			y = -1;
		else if (y < 2)
			y = 2;
	}

	// update flags
	for(int i = _items->Count; --i >= 0;)
	{
		// source and destination
		FarItem^ item1 = _items[i];
		FarMenuItemEx& item2 = items[i];

		// common flags
		item2.Flags = 0;
		if (item1->Checked)
			item2.Flags |= MIF_CHECKED;
		if (item1->Disabled)
			item2.Flags |= MIF_DISABLE;
		if (item1->Grayed)
			item2.Flags |= MIF_GRAYED;
		if (item1->Hidden)
			item2.Flags |= MIF_HIDDEN;
		if (item1->IsSeparator)
			item2.Flags |= MIF_SEPARATOR;
	}

	// select an item (same as listbox!)
	if (_selected >= _items->Count || SelectLast && _selected < 0)
		_selected = _items->Count - 1;
	if (_selected >= 0)
		items[_selected].Flags |= MIF_SELECTED;

	// show
	int bc;
	_selected = Info.Menu(
		Info.ModuleNumber,
		x,
		y,
		MaxHeight,
		Flags(),
		title,
		bottom,
		help,
		breaks,
		&bc,
		(const FarMenuItem*)items,
		_items->Count);
	_breakKey = bc < 0 ? 0 : _keys[bc];
}

bool Menu::Show()
{
	if (_createdItems)
	{
		ShowMenu(_createdItems, _createdBreaks, _title, _bottom, _help);
	}
	else
	{
		FarMenuItemEx* items = CreateItems();
		int* breaks = CreateBreakKeys();
		PIN_NS(pinTitle, Title);
		PIN_NS(pinBottom, Bottom);
		PIN_NS(pinHelpTopic, HelpTopic);
		try
		{
			ShowMenu(items, breaks, pinTitle, pinBottom, pinHelpTopic);
		}
		finally
		{
			DeleteItems(items);
			delete breaks;
		}
	}

	//! When nothing is selected (e.g. empty menu) break key still may work

	// check selected
	if (_selected < 0)
		return false;

	// check break key
	if (_breakKey > 0)
		return true;

	// call click (if not a break key!)
	FarItem^ item = _items[_selected];
	if (item->Click)
	{
		if (Sender)
			item->Click(Sender, nullptr);
		else
			item->Click(item, nullptr);
	}

	return true;
}
}
