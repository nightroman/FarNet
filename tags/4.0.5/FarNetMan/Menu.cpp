/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#include "StdAfx.h"
#include "Menu.h"
#include "Far.h"

namespace FarNet
{;
Menu::Menu()
{
}

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
	return r;
}

void Menu::Lock()
{
	if (_createdItems)
	{
		DeleteItems(_createdItems);
		delete _createdBreaks;
		delete _help;
		delete _title;
		delete _bottom;
	}
	_createdItems = CreateItems();
	_createdBreaks = CreateBreakKeys();
	
	_help = NewChars(HelpTopic);
	_title = NewChars(Title);
	_bottom = NewChars(Bottom);
}

void Menu::Unlock()
{
	if (_createdItems)
	{
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
}

FarMenuItemEx* Menu::CreateItems()
{
	int n = 0;
	FarMenuItemEx* r = new struct FarMenuItemEx[_items->Count];
	for each(IMenuItem^ item1 in _items)
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

ToolOptions Menu::From()
{
	switch(Far::Instance->GetWindowType(-1))
	{
	case WindowType::Panels:
		return ToolOptions::Panels;
	case WindowType::Editor:
		return ToolOptions::Editor;
	case WindowType::Viewer:
		return ToolOptions::Viewer;
	case WindowType::Dialog:
		return ToolOptions::Dialog;
	default:
		// not a window value
		return ToolOptions::Config;
	}
}

void Menu::ShowMenu(FarMenuItemEx* items, const int* breaks, const wchar_t* title, const wchar_t* bottom, const wchar_t* help)
{
	// validate X, Y to avoid crashes and out of screen
	int x = _x < 0 ? -1 : _x < 2 ? 2 : _x;
	int y = _y;
	if (y != -1)
	{
		int yMax = Console::WindowHeight - Math::Max(_items->Count, MaxHeight) - 4;
		if (y > yMax)
			y = yMax;
		if (y < 0)
			y = -1;
		else if (y < 2)
			y = 2;
	}

	// update flags
	ToolOptions from = ToolOptions::None;
	for(int i = _items->Count; --i >= 0;)
	{
		MenuItem^ item1 = (MenuItem^)_items[i];
		FarMenuItemEx& item2 = items[i];

		item2.Flags = 0;
		if (item1->Checked)
			item2.Flags |= MIF_CHECKED;
		if (item1->IsSeparator)
			item2.Flags |= MIF_SEPARATOR;

		// enable\disable
		if (item1->Disabled)
		{
			item2.Flags |= MIF_DISABLE;
		}
		else if (item1->From != ToolOptions::None)
		{
			if (from == ToolOptions::None)
				from = From();
			if (!int(item1->From & from))
				items[i].Flags |= MIF_DISABLE;
		}
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

	// exit
	if (_selected < 0)
		return false;

	// more
	MenuItem^ item = (MenuItem^)_items[_selected];
	if (item->_OnClick)
	{
		if (Sender)
			item->_OnClick(Sender, nullptr);
		else
			item->_OnClick(item, nullptr);
	}
	return true;
}
}
