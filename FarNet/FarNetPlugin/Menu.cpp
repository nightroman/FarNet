/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#include "StdAfx.h"
#include "Menu.h"
#include "InputBox.h"
#include "Message.h"

namespace FarManagerImpl
{;
public ref class MenuItem : IMenuItem
{
public:
	DEF_EVENT(OnClick, _OnClick);
public:
	virtual property String^ Text;
	virtual property bool Checked;
	virtual property bool IsSeparator;
	virtual property Object^ Data;
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
		return Add(text, false, false);
	}
	virtual IMenuItem^ Add(String^ text, EventHandler^ onClick)
	{
		MenuItem^ r = gcnew MenuItem();
		r->Text = text;
		r->OnClick += onClick;
		Add(r);
		return r;
	}
	virtual IMenuItem^ Add(String^ text, bool isChecked, bool isSeparator)
	{
		MenuItem^ r = gcnew MenuItem();
		r->Text = text;
		r->Checked = isChecked;
		r->IsSeparator = isSeparator;
		Add(r);
		return r;
	}
private:
	// private: it was private originally, perhaps it used to make problems, too
	virtual IMenuItem^ Add(String^ text, bool isChecked) sealed = IMenuItems::Add
	{
		return Add(text, isChecked, false);
	}
};

Menu::Menu()
{
	_createdItems = NULL;
	_createdBreaks = NULL;
	_items = gcnew MenuItemCollection();
	_x = -1;
	_y = -1;
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

Object^ Menu::SelectedData::get()
{
	if (_selected < 0 || _selected >= _items->Count)
		return nullptr;
	return _items[_selected]->Data;
}

IList<int>^ Menu::BreakKeys::get()
{
	return %_breakKeys;
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

int* Menu::CreateBreakKeys()
{
	int* r = NULL;
	int nKey = _breakKeys.Count;
	if (FilterKey)
		++nKey;
	if (nKey > 0)
	{
		r = new int[nKey + 1];
		int i = 0;
		for each(int k in _breakKeys)
			r[i++] = k;
		if (FilterKey)
			r[i++] = FilterKey;
		r[i] = 0;
	}
	return r;
}

int Menu::Flags()
{
	int r = 0;
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
		int yMax = Console::WindowHeight - Math::Max(_items->Count, MaxHeight) - 4;
		if (y > yMax)
			y = yMax;
		if (y < 0)
			y = -1;
		else if (y < 2)
			y = 2;
	}

	// title
	CStr sTitle;
	if (!String::IsNullOrEmpty(Title))
		sTitle.Set(Title);

	// help
	CStr sHelpTopic;
	if (!String::IsNullOrEmpty(HelpTopic))
		sHelpTopic.Set(HelpTopic);

	// bottom
	String^ info = Bottom ? Bottom : String::Empty;
	if (FilterKey)
	{
		info += info->Length ? " [" : "[";
		if (Filter)
			info += Filter;
		info += "] (";
		if (_ii)
			info += _ii->Count + "/";
		info += _items->Count + ")";
	}
	CStr sBottom;
	if (info->Length)
		sBottom.Set(info);

	// show
	int bc;
	_selected = Info.Menu(
		Info.ModuleNumber, x, y, MaxHeight, Flags(), sTitle, sBottom, sHelpTopic, breaks, &bc, items,
		_ii ? _ii->Count : _items->Count);
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
		for(;;)
		{
			ShowMenu(items, bkeys);

			// correct filtered index
			if (_ii && _selected >= 0)
				_selected = _ii[_selected];

			// stop if not a filter key
			if (_breakCode != _breakKeys.Count)
				break;

			// input filter
			String^ filter = InputFilter(Filter, FilterHistory);
			if (!filter)
				continue;

			// reset filter
			Filter = filter;
			delete items;
			items = CreateItems();
		}
	}
	finally
	{
		if (!_createdItems)
		{
			delete items;
			delete bkeys;
			_ii = nullptr;
		}
	}

	bool r = _selected >= 0;
	if (r)
	{
		MenuItem^ item = (MenuItem^)_items[_selected];
		if (item->_OnClick)
			item->_OnClick(item, nullptr);
	}

	return r;
}

FarMenuItem* Menu::CreateItems()
{
	// get filter
	if (!Filter && FilterRestore && !String::IsNullOrEmpty(FilterHistory))
	{
		RegistryKey^ key;
		try
		{
			key = Registry::CurrentUser->OpenSubKey("Software\\Far\\SavedDialogHistory\\" + FilterHistory, false);
			if (key)
			{
				int flags = (int)key->GetValue("Flags");
				if (flags)
					Filter = key->GetValue("Line0")->ToString();
			}
		}
		finally
		{
			if (key)
				key->Close();
		}
	}

	// filter
	Regex^ re = CreateFilter(Filter, NULL);
	if (re)
	{
		_ii = gcnew List<int>;
	}
	else
	{
		// reset Filter!
		Filter = nullptr;
		_ii = nullptr;
	}

	// add items, filter
	int i = -1, n = 0;
	FarMenuItem* r = new struct FarMenuItem[_items->Count];
	for each(IMenuItem^ item in _items)
	{
		++i;
		if (re)
		{
			if (!re->IsMatch(item->Text))
				continue;
			_ii->Add(i);
		}
		StrToOem((item->Text->Length > 127 ? item->Text->Substring(0, 127) : item->Text), r[n].Text);
		r[n].Checked = item->Checked;
		r[n].Separator = item->IsSeparator;
		r[n].Selected = 0;
		++n;
	}

	// select an item
	if (SelectLast)
	{
		if (n > 0)
			r[n - 1].Selected = true;
	}
	else if (_selected >= 0)
	{
		if (_selected < n)
			r[_selected].Selected = true;
		else if (n > 0)
			r[n - 1].Selected = true;
	}

	return r;
}

Regex^ Menu::CreateFilter(String^ filter, bool* ok)
{
	if (ok)
		*ok = true;

	if (String::IsNullOrEmpty(filter))
		return nullptr;

	try
	{
		if (!filter->StartsWith("*"))
			return gcnew Regex(filter, RegexOptions::IgnoreCase);
		else if (filter->Length > 1)
			return gcnew Regex(Regex::Escape(filter->Substring(1)), RegexOptions::IgnoreCase);
		return nullptr;
	}
	catch(ArgumentException^ e)
	{
		if (ok)
		{
			*ok = false;
			Message::Show(e->Message, "Filter expression", MessageOptions::Ok, nullptr);
		}
		return nullptr;
	}
}

String^ Menu::InputFilter(String^ filter, String^ history)
{
	InputBox ib;
	ib.Title = "Filter";
	ib.Prompt = "Filter expression";
	ib.EmptyEnabled = true;
	ib.History = history;
	if (!String::IsNullOrEmpty(filter))
		ib.Text = filter;

	// show filter input box
	for(;;)
	{
		// cancelled
		if (!ib.Show())
			return nullptr;

		// validate
		bool ok;
		CreateFilter(ib.Text, &ok);
		if (ok)
			return ib.Text;
	}
}
}
