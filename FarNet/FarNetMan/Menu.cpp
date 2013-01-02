
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
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

FarKey* Menu::CreateBreakKeys()
{
	FarKey* r = nullptr;
	int nKey = _keys.Count;
	if (nKey > 0)
	{
		r = new FarKey[nKey + 1];
		int i = 0;
		for each(KeyData^ k in _keys)
		{
			r[i].VirtualKeyCode = (WORD)k->VirtualKeyCode;
			r[i].ControlKeyState = (DWORD)k->ControlKeyState;
			++i;
		}
		r[i].VirtualKeyCode = 0;
		r[i].ControlKeyState = 0;
	}
	return r;
}

int Menu::Flags()
{
	int r = 0;
	if (AutoAssignHotkeys) r |= FMENU_AUTOHIGHLIGHT;
	if (ChangeConsoleTitle) r |= FMENU_CHANGECONSOLETITLE;
	if (ReverseAutoAssign) r |= FMENU_REVERSEAUTOHIGHLIGHT;
	if (ShowAmpersands) r |= FMENU_SHOWAMPERSAND;
	if (WrapCursor) r |= FMENU_WRAPMODE;
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

FarMenuItem* Menu::CreateItems()
{
	int n = 0;
	FarMenuItem* r = new struct FarMenuItem[_items->Count];
	memset(r, 0, sizeof(FarMenuItem) * _items->Count);
	for each(FarItem^ item1 in _items)
	{
		FarMenuItem& item2 = r[n];
		item2.Text = NewChars(item1->Text);
		++n;
	}
	return r;
}

void Menu::DeleteItems(FarMenuItem* items)
{
	if (items)
	{
		for(int i = Items->Count; --i >= 0;)
			delete items[i].Text;
		delete items;
	}
}

void Menu::ShowMenu(FarMenuItem* items, const FarKey* breaks, const wchar_t* title, const wchar_t* bottom, const wchar_t* help)
{
	// validate X, Y to avoid crashes and out of screen
	int x = _x < 0 ? -1 : _x < 2 ? 2 : _x;
	int y = _y;
	if (y != -1)
	{
		int yMax = Far::Api->UI->WindowSize.Y - Math::Max(_items->Count, MaxHeight) - 4;
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
		FarMenuItem& item2 = items[i];

		// common flags
		item2.Flags = 0;
		if (item1->Checked) item2.Flags |= MIF_CHECKED;
		if (item1->Disabled) item2.Flags |= MIF_DISABLE;
		if (item1->Grayed) item2.Flags |= MIF_GRAYED;
		if (item1->Hidden) item2.Flags |= MIF_HIDDEN;
		if (item1->IsSeparator) item2.Flags |= MIF_SEPARATOR;
	}

	// select an item (same as listbox!)
	if (_selected >= _items->Count || SelectLast && _selected < 0)
		_selected = _items->Count - 1;
	if (_selected >= 0)
		items[_selected].Flags |= MIF_SELECTED;

	// show
	intptr_t bc = -1;
	_selected = (int)Info.Menu(
		&MainGuid,
		&MainGuid,
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

	_keyIndex = (int)bc;
}

bool Menu::Show()
{
	bool lock = _createdItems == nullptr;
	if (lock)
		Lock();
	
	try
	{
		for(;;)
		{
			ShowMenu(_createdItems, _createdBreaks, _title, _bottom, _help);

			// check the key before the selected, it may work in empty menus with nothing selected
			if (_keyIndex >= 0)
			{
				if (_handlers[_keyIndex])
				{
					MenuEventArgs e((_selected >= 0 ? _items[_selected] : nullptr));
					_handlers[_keyIndex]((Sender ? Sender : this), %e);
					if (e.Ignore || e.Restart)
						continue;
				}
				return true;
			}

			// check selected
			if (_selected < 0)
				return false;
			
			// call click (if not a break key!)
			FarItem^ item = _items[_selected];
			if (item->Click)
			{
				MenuEventArgs e(item);
				item->Click((Sender ? Sender : this), %e);
				if (e.Ignore || e.Restart)
					continue;
			}

			return true;
		}
	}
	finally
	{
		if (lock)
			Unlock();
	}
}
}
