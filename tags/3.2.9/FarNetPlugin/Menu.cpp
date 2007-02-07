#include "StdAfx.h"
#include "Menu.h"
#include "Utils.h"

namespace FarManagerImpl
{;
Menu::Menu()
{
	_createdItems = NULL;
	_createdBreaks = NULL;
	_items = gcnew MenuItemCollection();
	_breakKeys = gcnew List<int>();
	_x = -1;
	_y = -1;
	_title = String::Empty;
	_bottom = String::Empty;
	_selected = -1;
}

int Menu::X::get()
{
	return _x;
}

void Menu::X::set(int value)
{
	_x = value;
}

int Menu::Y::get()
{
	return _y;
}

void Menu::Y::set(int value)
{
	_y = value;
}

int Menu::MaxHeight::get()
{
	return _maxHeight;
}

void Menu::MaxHeight::set(int value)
{
	_maxHeight = value;
}

String^ Menu::Title::get()
{
	return _title;
}

void Menu::Title::set(String^ value)
{
	_title = value;
}

Object^ Menu::SelectedData::get()
{
	if (_selected < 0 || _selected >= _items->Count)
		return nullptr;
	return _items[_selected]->Data;
}

String^ Menu::Bottom::get()
{
	return _bottom;
}

void Menu::Bottom::set(String^ value)
{
	_bottom = value;
}

IList<int>^ Menu::BreakKeys::get()
{
	return _breakKeys;
}

IMenuItems^ Menu::Items::get()
{
	return _items;
}

int Menu::Selected::get()
{
	return _selected;
}

void Menu::Selected::set(int value)
{
	_selected = value;
}

int Menu::BreakCode::get()
{
	return _breakCode;
}

void Menu::BreakCode::set(int value)
{
	_breakCode = value;
}

bool Menu::ShowAmpersands::get()
{
	return _showAmpersands;
}

void Menu::ShowAmpersands::set(bool value)
{
	_showAmpersands = value;
}

bool Menu::WrapCursor::get()
{
	return _wrapCursor;
}

void Menu::WrapCursor::set(bool value)
{
	_wrapCursor = value;
}

bool Menu::AutoAssignHotkeys::get()
{
	return _autoAssignHotkeys;
}

void Menu::AutoAssignHotkeys::set(bool value)
{
	_autoAssignHotkeys = value;
}

bool Menu::ReverseAutoAssign::get()
{
	return _reverseAutoAssign;
}

void Menu::ReverseAutoAssign::set(bool value)
{
	_reverseAutoAssign = value;
}

FarMenuItem* Menu::CreateItems()
{
	FarMenuItem* r = new struct FarMenuItem[_items->Count];
	FarMenuItem* p = r;
	for each(IMenuItem^ item in _items)
	{
		StrToOem((item->Text->Length > 127 ? item->Text->Substring(0, 127) : item->Text), p->Text);
		p->Selected = item->Selected;
		p->Checked = item->Checked;
		p->Separator = item->IsSeparator;
		++p;
	}
	return r;
}

int* Menu::CreateBreakKeys()
{
	int* r = NULL;
	if (_breakKeys->Count > 0)
	{
		r = new int[_breakKeys->Count + 1];
		int* cur = r;
		for each(int i in _breakKeys)
		{
			*cur = i;
			++cur;
		}
		*cur = 0;
	}
	return r;
}

int Menu::Flags()
{
	int r = 0;
	if (_showAmpersands)
		r |= FMENU_SHOWAMPERSAND;
	if (_wrapCursor)
		r |= FMENU_WRAPMODE;
	if (_autoAssignHotkeys)
		r |= FMENU_AUTOHIGHLIGHT;
	if (_reverseAutoAssign)
		r |= FMENU_REVERSEAUTOHIGHLIGHT;
	return r;
}

void Menu::ShowMenu(const FarMenuItem* items, const int* breaks)
{
	// validate X,Y to avoid crashes
	int x = _x < 0 ? -1 : _x < 2 ? 2 : _x;
	int y = _y < 0 ? -1 : _y < 2 ? 2 : _y;

	// show
	int bc;
	CStr sTitle(_title);
	CStr sBottom(_bottom);
	_selected = Info.Menu(
		Info.ModuleNumber, x, y, _maxHeight, Flags(), sTitle, sBottom, "", breaks, &bc, items, _items->Count);
	_breakCode = bc;
}

bool Menu::Show()
{
	FarMenuItem* items;
	int* bkeys;

	if (_createdItems)
	{
		items = _createdItems;
		bkeys = _createdBreaks;
	}
	else
	{
		items = CreateItems();
		bkeys = CreateBreakKeys();
	}

	try
	{
		ShowMenu(items, bkeys);
	}
	finally
	{
		if (!_createdItems)
		{
			delete items;
			delete bkeys;
		}
	}

	bool r = _selected != -1;
	if (r)
		_items[_selected]->FireOnClick();

	return r;
}

bool Menu::Show(int index)
{
	if (index >= 0)
	{
		if (_createdItems)
		{
			for (int i = _items->Count; --i >= 0;)
				_createdItems[i].Selected = (i == index);
		}
		else
		{
			for (int i = _items->Count; --i >= 0;)
				_items[i]->Selected = (i == index);
		}
	}
	return Show();
}

void Menu::Lock()
{
	if (_createdItems)
	{
		delete _createdItems;
		delete _createdBreaks;
	}
	_createdItems = CreateItems();
	_createdBreaks = CreateBreakKeys();
}

void Menu::Unlock()
{
	if (_createdItems)
	{
		delete _createdItems;
		delete _createdBreaks;
		_createdItems = NULL;
		_createdBreaks = NULL;
	}
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
}
