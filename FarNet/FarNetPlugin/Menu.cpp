#include "StdAfx.h"
#include "Menu.h"

namespace FarManagerImpl
{;
public ref class MenuItem : IMenuItem
{
public:
	DEF_EVENT(OnClick, OnClickHandler);
public:
	virtual property String^ Text;
	virtual property bool Selected;
	virtual property bool Checked;
	virtual property bool IsSeparator;
	virtual property Object^ Data;
	virtual void FireOnClick()
	{
		if (OnClickHandler != nullptr)
			OnClickHandler(this, gcnew EventArgs());
	}
	void Add(EventHandler^ handler)
	{
		OnClick += handler;
	}
	virtual String^ ToString() override
	{
		return Text;
	}
};

public ref class MenuItemCollection : public List<IMenuItem^>, IMenuItems
{
public:
	virtual IMenuItem^ Add(String^ text)
	{
		return Add(text, false, false, false);
	}
	virtual IMenuItem^ Add(String^ text, EventHandler^ onClick)
	{
		MenuItem^ r = gcnew MenuItem();
		r->Text = text;
		r->OnClick += onClick;
		Add(r);
		return r;
	}
	virtual IMenuItem^ Add(String^ text, bool isSelected, bool isChecked, bool isSeparator)
	{
		MenuItem^ r = gcnew MenuItem();
		r->Text = text;
		r->Selected = isSelected;
		r->Checked = isChecked;
		r->IsSeparator = isSeparator;
		Add(r);
		return r;
	}
private:
	// private: makes problems in PowerShell: it is called instead of Add(string, EventHandler)
	virtual IMenuItem^ Add(String^ text, bool isSelected) sealed = IMenuItems::Add
	{
		return Add(text, isSelected, false, false);
	}
	// private: it was private originally, perhaps it used to make problems, too
	virtual IMenuItem^ Add(String^ text, bool isSelected, bool isChecked) sealed = IMenuItems::Add
	{
		return Add(text, isSelected, isChecked, false);
	}
};

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

Object^ Menu::SelectedData::get()
{
	if (_selected < 0 || _selected >= _items->Count)
		return nullptr;
	return _items[_selected]->Data;
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

void Menu::ShowMenu(const FarMenuItem* items, const int* breaks)
{
	// validate X, Y to avoid crashes and out of screen
	int x = _x < 0 ? -1 : _x < 2 ? 2 : _x;
	int y = _y;
	if (y != -1)
	{
		int yMax = Console::WindowHeight - Math::Max(_items->Count, _maxHeight) - 4;
		if (y > yMax)
			y = yMax;
		if (y < 0)
			y = -1;
		else if (y < 2)
			y = 2;
	}

	// show
	int bc;
	CStr sTitle(_title);
	CStr sBottom(_bottom);
	CStr sHelp;
	if (!String::IsNullOrEmpty(HelpTopic))
		sHelp.Set(HelpTopic);
	_selected = Info.Menu(
		Info.ModuleNumber, x, y, _maxHeight, Flags(), sTitle, sBottom, sHelp, breaks, &bc, items, _items->Count);
	_breakCode = bc;
}
}
