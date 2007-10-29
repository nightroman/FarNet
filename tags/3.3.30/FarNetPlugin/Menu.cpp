/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#include "StdAfx.h"
#include "Menu.h"
#include "Dialog.h"
#include "FarImpl.h"
#include "InputBox.h"
#include "Message.h"

namespace FarManagerImpl
{;

//
//::MenuItemCollection
//

IMenuItem^ MenuItemCollection::Add(String^ text)
{
	return Add(text, false, false);
}

IMenuItem^ MenuItemCollection::Add(String^ text, EventHandler^ onClick)
{
	MenuItem^ r = gcnew MenuItem();
	r->Text = text;
	r->OnClick += onClick;
	Add(r);
	return r;
}

IMenuItem^ MenuItemCollection::Add(String^ text, bool isChecked, bool isSeparator)
{
	MenuItem^ r = gcnew MenuItem();
	r->Text = text;
	r->Checked = isChecked;
	r->IsSeparator = isSeparator;
	Add(r);
	return r;
}

IMenuItem^ MenuItemCollection::Add(String^ text, bool isChecked)
{
	return Add(text, isChecked, false);
}

//
//::AnyMenu
//

AnyMenu::AnyMenu()
: _x(-1)
, _y(-1)
, _selected(-1)
{
	_items = gcnew MenuItemCollection();
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

IMenuItems^ AnyMenu::Items::get()
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

Regex^ AnyMenu::CreateFilter(String^ filter, bool* ok)
{
	if (ok)
		*ok = true;

	if (String::IsNullOrEmpty(filter))
		return nullptr;

	try
	{
		// case: substring
		if (filter->StartsWith("*"))
			return filter->Length == 0 ? nullptr :
				gcnew Regex(Regex::Escape(filter->Substring(1)), RegexOptions::IgnoreCase);

		// case: prefix
		if (filter->StartsWith("?"))
			return filter->Length == 0 ? nullptr :
				gcnew Regex("^" + Regex::Escape(filter->Substring(1)), RegexOptions::IgnoreCase);

		// standard
		return gcnew Regex(filter, RegexOptions::IgnoreCase);
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

String^ AnyMenu::InputFilter(String^ filter, String^ history)
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

Regex^ AnyMenu::CreateFilter()
{
	// get filter
	if (!Filter && FilterRestore && !String::IsNullOrEmpty(FilterHistory))
	{
		RegistryKey^ key;
		try
		{
			key = Registry::CurrentUser->OpenSubKey(GetFar()->RootFar + "\\SavedDialogHistory\\" + FilterHistory, false);
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
	Regex^ r = CreateFilter(Filter, NULL);
	if (r)
	{
		// init filter
		_ii = gcnew List<int>;
	}
	else
	{
		// reset filter!
		Filter = nullptr;
		_ii = nullptr;
	}
	return r;
}

String^ AnyMenu::InfoLine()
{
	String^ r = FilterKey ? String::Concat("<", Filter) + ">" : String::Empty;
	r += "(";
	if (_ii)
		r += _ii->Count + "/";
	r += _items->Count + ")";
	return JoinText(r, Bottom);
}

IList<int>^ AnyMenu::BreakKeys::get()
{
	return %_breakKeys;
}

int AnyMenu::BreakCode::get()
{
	return _breakCode;
}

//
//::Menu
//

Menu::Menu()
: _createdItems(NULL)
, _createdBreaks(NULL)
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
			r[i++] = (int)FilterKey;
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

FarMenuItem* Menu::CreateItems()
{
	// filter
	Regex^ re = CreateFilter();

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
		StrToOem(item->Text, r[n].Text, sizeof(r->Text));
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
	String^ info = InfoLine();
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
		{
			if (Sender)
				item->_OnClick(Sender, nullptr);
			else
				item->_OnClick(item, nullptr);
		}
	}

	return r;
}

//
//::ListMenu
//

void ListMenu::GetInfo(String^& head, String^& foot)
{
	head = Title ? Title : String::Empty;
	foot = InfoLine();
	if (Bottom)
		foot += " ";
	if (Incremental != FilterOptions::None && IncrementalFilter)
	{
		if (SelectLast)
			foot = "[" + IncrementalFilter + "]" + foot;
		else
			head = JoinText("[" + IncrementalFilter + "]", head);
	}
}

void ListMenu::MakeFilter()
{
	// filter 1
	if (_toFilter1)
	{
		_toFilter1 = false;
		Regex^ re1 = CreateFilter();
		if (re1)
		{
			int i = -1;
			for each(IMenuItem^ mi in Items)
			{
				++i;
				if (re1->IsMatch(mi->Text))
					_ii->Add(i);
			}
		}
	}

	// filter 2
	if (!_toFilter2)
		return;
	_toFilter2 = false;
	Regex^ re2 = CreateIncrementalFilter();
	if (!re2)
		return;

	// case: filter already filtered
	if (_ii)
	{
		List<int>^ ii = gcnew List<int>;
		for each(int k in _ii)
		{
			if (re2->IsMatch(_items[k]->Text))
				ii->Add(k);
		}
		_ii = ii;
		return;
	}

	// case: not yet filtered
	_ii = gcnew List<int>;
	int i = -1;
	for each(IMenuItem^ mi in Items)
	{
		++i;
		if (re2->IsMatch(mi->Text))
			_ii->Add(i);
	}
}

Regex^ ListMenu::CreateIncrementalFilter()
{
	if (!IncrementalFilter)
		return nullptr;

	String^ re = IncrementalFilter;
	if (int(Incremental & FilterOptions::Literal))
		re = Regex::Escape(IncrementalFilter);
	else
		re = Wildcard(IncrementalFilter);

	if (int(Incremental & FilterOptions::Prefix))
		re = "^" + re;

	return gcnew Regex(re, RegexOptions::IgnoreCase);
}

void ListMenu::OnKeyPressed(Object^ sender, KeyPressedEventArgs^ e)
{
	// Tab - go to next
	if (e->Code == KeyCode::Tab)
	{
		FarListBox^ box = (FarListBox^)e->Control;
		++box->Selected;
		e->Ignore = true;
		return;
	}

	FarDialog^ d = (FarDialog^)sender;

	//! break keys first
	if (_breakKeys.IndexOf(e->Code) >= 0)
	{
		_breakCode = e->Code;
		d->Close();
		return;
	}

	// filter
	if (e->Code == FilterKey)
	{
		// input filter
		String^ filter = InputFilter(Filter, FilterHistory);
		if (!filter)
			return;

		// reset filters
		Filter = filter;
		_restart = true;
		_toFilter1 = true;
		IncrementalFilter = nullptr;
		_incrementalLength1 = 0;
		d->Close();
		return;
	}

	// incremental
	if (Incremental != FilterOptions::None)
	{
		if (e->Code == KeyCode::Backspace)
		{
			if (IncrementalFilter)
			{
				if (IncrementalFilter->Length > _incrementalLength1)
				{
					Char c = IncrementalFilter[IncrementalFilter->Length - 1];
					IncrementalFilter = IncrementalFilter->Substring(0, IncrementalFilter->Length - 1);
					// * and ?
					if (!int(Incremental & FilterOptions::Literal) && (c == '*' || c == '?'))
					{
						String^ t; String^ b; GetInfo(t, b);
						d->_items[0]->Text = t;
						d->_items[2]->Text = b;
					}
					else
					{
						_restart = -1;
						_toFilter1 = _toFilter2 = true;
					}
				}
				if (IncrementalFilter->Length == 0)
					IncrementalFilter = nullptr;
			}
		}
		else
		{
			Char c = GetFar()->CodeToChar(e->Code);
			if (c >= ' ')
			{
				// keep and change filter
				String^ filterBak = IncrementalFilter;
				if (IncrementalFilter)
					IncrementalFilter += c;
				else
					IncrementalFilter = gcnew String(c, 1);

				// * and ?
				if (!int(Incremental & FilterOptions::Literal) && (c == '*' || c == '?'))
				{
					String^ t; String^ b; GetInfo(t, b);
					d->_items[0]->Text = t;
					d->_items[2]->Text = b;
					return;
				}
				
				// try the filter, rollback on empty
				List<int>^ iiBak = _ii;
				_toFilter2 = true;
				MakeFilter();
				if (_ii && _ii->Count == 0)
				{
					IncrementalFilter = filterBak;
					_ii = iiBak;
					return;
				}

				_toFilter2 = true;
				_restart = +1;
			}

		}
		if (_restart)
			d->Close();
		return;
	}
}

bool ListMenu::Show()
{
	// keep length
	if (IncrementalFilter)
		_incrementalLength1 = IncrementalFilter->Length;

	// main loop
	_toFilter1 = _toFilter2 = true;
	for(int pass = 0;; ++pass)
	{
		// filtered item number
		const int nItem1 = _ii ? _ii->Count : _items->Count;

		// filter
		MakeFilter();

		// filtered item number
		const int nItem2 = _ii ? _ii->Count : _items->Count;
		if (nItem2 < 2 && AutoSelect)
		{
			if (nItem2 == 1)
			{
				_selected = _ii ? _ii[0] : 0;
				return true;
			}
			else if (pass == 0)
			{
				_selected = -1;
				return false;
			}
		}
		if (_restart && nItem1 != nItem2)
			_selected = 0;

		// title, bottom
		String^ title; String^ info;
		GetInfo(title, info);

		// width
		int w = 0;
		if (_ii)
		{
			for each(int k in _ii)
				if (_items[k]->Text->Length > w)
					w = _items[k]->Text->Length;
		}
		else
		{
			for each(IMenuItem^ mi in _items)
				if (mi->Text->Length > w)
					w = mi->Text->Length;
		}
		w += 2; // if less last chars are lost

		// height
		int n = _ii ? _ii->Count : _items->Count;
		if (MaxHeight > 0 && n > MaxHeight)
			n = MaxHeight;

		// fix width
		if (w > 127)
			w = 127;
		if (title && w < title->Length)
			w = title->Length + 4;
		if (w < info->Length)
			w = info->Length + 4;
		if (w < 20)
			w = 20;

		// place
		const int min = ScreenMargin > 1 ? ScreenMargin : 1;
		int dw = w + 4, dx = _x;
		ValidateRect(dx, dw, min, Console::WindowWidth - (2*min));
		int dh = n + 2, dy = _y;
		ValidateRect(dy, dh, min, Console::WindowHeight - (2*min));

		// dialog
		FarDialog dialog(dx, dy, dx + dw - 1, dy + dh - 1);
		dialog.HelpTopic = HelpTopic;
		dialog.NoShadow = NoShadow;

		// title
		dialog.AddBox(0, 0, dw - 1, dh - 1, title);

		// list
		FarListBox^ box = (FarListBox^)dialog.AddListBox(1, 1, dw - 2, dh - 2, String::Empty);
		box->Selected = _selected;
		box->SelectLast = SelectLast;
		box->NoBox = true;
		ListBox = box;
		if (Incremental == FilterOptions::None)
		{
			box->AutoAssignHotkeys = AutoAssignHotkeys;
			box->NoAmpersands = !ShowAmpersands;
			box->WrapCursor = WrapCursor;
		}

		// "bottom"
		dialog.AddText(1, dh - 1, 0, info);

		// items and filter
		box->_items = _items;
		box->_ii = _ii;

		// filter
		box->_KeyPressed += gcnew EventHandler<KeyPressedEventArgs^>(this, &ListMenu::OnKeyPressed);

		_restart = 0;
		if (_Showing)
			_Showing(this, nullptr);
		bool ok = dialog.Show();
		ListBox = nullptr;
		if (!ok)
			return false;

		if (_restart)
			continue;

		// correct by filter
		_selected = box->Selected;
		if (_ii && _selected >= 0)
			_selected = _ii[_selected];

		//! [Enter] on empty gives -1
		return _selected >= 0;
	}
}

}
