/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#include "StdAfx.h"
#include "ListMenu.h"
#include "Dialog.h"
#include "Far.h"
#include "InputBox.h"
#include "Message.h"

namespace FarNet
{;
//! UI: error message
static Regex^ CreateRegex(String^ pattern, PatternOptions options, bool* ok)
{
	if (ok)
		*ok = true;

	if (ES(pattern) || !int(options))
		return nullptr;

	// regex?
	if (int(options & PatternOptions::Regex))
	{
		try
		{
			if (pattern->StartsWith("?"))
			{
				// prefix
				if (pattern->Length <= 1)
					return nullptr;

				pattern = pattern->Substring(1);
				options = (options | PatternOptions::Prefix);
			}
			else if (pattern->StartsWith("*"))
			{
				// substring
				if (pattern->Length <= 1)
					return nullptr;

				pattern = pattern->Substring(1);
				options = (options | PatternOptions::Substring);
			}
			else
			{
				//! standard regex; errors may come here
				return gcnew Regex(pattern, RegexOptions::IgnoreCase);
			}
		}
		catch(ArgumentException^ e)
		{
			if (ok)
			{
				*ok = false;
				Message::Show(e->Message, "Filter expression", MsgOptions::Ok, nullptr, nullptr);
			}
			return nullptr;
		}
	}

	// literal else wildcard
	String^ re;
	if (int(options & PatternOptions::Literal))
		re = Regex::Escape(pattern);
	else
		re = Wildcard(pattern);

	// prefix?
	if (int(options & PatternOptions::Prefix))
		re = "^" + re;

	//! normally errors must not come here
	return gcnew Regex(re, RegexOptions::IgnoreCase);
}

static String^ InputFilter(String^ pattern, PatternOptions options, String^ history, Regex^& regex)
{
	static CStr help;
	InputBox ib;
	ib.Title = "Filter";
	ib.Prompt = "Pattern (" + options.ToString() + ")";
	ib.EmptyEnabled = true;
	ib.History = history;
	if (SS(pattern))
		ib.Text = pattern;

	//! help; note that it fails without 'Far::_helpTopic' part
	if (help == NULL)
		help.Set(Far::_helpTopic + "InputFilter");
	ib._oemHelpTopic = help;

	// show filter input box
	for(;;)
	{
		// cancelled
		if (!ib.Show())
			return nullptr;

		// get regex (with UI errors)
		bool ok;
		regex = CreateRegex(ib.Text, options, &ok);
		if (ok)
			return ib.Text;
	}
}

ListMenu::ListMenu()
: _filter1_(String::Empty)
, _Incremental_(String::Empty)
, _filter2(String::Empty)
, _FilterKey(KeyMode::Ctrl | KeyCode::Down)
{
}

String^ ListMenu::Incremental::get() { return _Incremental_; }
void ListMenu::Incremental::set(String^ value)
{
	if (!value) throw gcnew ArgumentNullException("value");
	_Incremental_ = value;
	_filter2 = value;
	_re2 = nullptr;
} 

String^ ListMenu::Filter::get() { return _filter1_; }
void ListMenu::Filter::set(String^ value)
{
	_filter1_ = value;
	_re1 = nullptr;
}

PatternOptions ListMenu::IncrementalOptions::get() { return _IncrementalOptions; }
void ListMenu::IncrementalOptions::set(PatternOptions value)
{
	if (int(value & PatternOptions::Regex))
		throw gcnew ArgumentException("Incremental filter can not be 'Regex'.");
	_IncrementalOptions = value;
}

String^ ListMenu::InfoLine()
{
	String^ r = "(";
	if (_ii)
		r += _ii->Count + "/";
	r += _items->Count + ")";
	if (int(FilterOptions))
		r += "<" + Filter + ">";
	return JoinText(r, Bottom);
}

void ListMenu::GetInfo(String^& head, String^& foot)
{
	head = Title ? Title : String::Empty;
	foot = InfoLine();
	if (SS(Bottom))
		foot += " ";
	if (int(IncrementalOptions) && _filter2->Length)
	{
		if (SelectLast)
			foot = "[" + _filter2 + "]" + foot;
		else
			head = JoinText("[" + _filter2 + "]", head);
	}
}

void ListMenu::MakeFilter1()
{
	if (_re1)
		return;

	// get pattern
	if (!Filter->Length && FilterRestore && SS(FilterHistory))
	{
		RegistryKey^ key = nullptr;
		try
		{
			key = Registry::CurrentUser->OpenSubKey(Far::Instance->RootFar + "\\SavedDialogHistory\\" + FilterHistory, false);
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

	// make filter
	_re1 = CreateRegex(Filter, FilterOptions, NULL);
}

void ListMenu::MakeFilters()
{
	// filter 1
	if (_toFilter1)
	{
		_toFilter1 = false;
		MakeFilter1();
		if (_re1)
		{
			// init filter
			_ii = gcnew List<int>;

			// fill
			int i = -1;
			for each(IMenuItem^ mi in Items)
			{
				++i;
				if (_re1->IsMatch(mi->Text))
					_ii->Add(i);
			}
		}
		else
		{
			// reset filter!
			Filter = String::Empty;
			_ii = nullptr;
		}
	}

	// filter 2
	if (!_toFilter2)
		return;
	_toFilter2 = false;

	// Don't filter by predefined, assume that predefined filter is usually used without permanent.
	// E.g. TabEx: 'sc[Tab]' gets 'Set-Contents' which doesn't match prefix 'sc', but it should be shown.
	if (!Filter->Length && _filter2 == Incremental)
		return;

	// create
	if (!_re2)
		_re2 = CreateRegex(_filter2, IncrementalOptions, NULL);
	if (!_re2)
		return;

	// case: filter already filtered
	if (_ii)
	{
		List<int>^ ii = gcnew List<int>;
		for each(int k in _ii)
		{
			if (_re2->IsMatch(_items[k]->Text))
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
		if (_re2->IsMatch(mi->Text))
			_ii->Add(i);
	}
}

void ListMenu::AddKey(int key)
{
	AddKey(key, nullptr);
}

void ListMenu::AddKey(int key, EventHandler<MenuEventArgs^>^ handler)
{
	_keys.Add(key);
	_handlers.Add(handler);
}

void ListMenu::OnKeyPressed(Object^ sender, KeyPressedEventArgs^ e)
{
	// Tab: go to next
	if (e->Code == KeyCode::Tab)
	{
		FarListBox^ box = (FarListBox^)e->Control;
		++box->Selected;
		e->Ignore = true;
		return;
	}

	FarDialog^ d = (FarDialog^)sender;

	//! break keys first
	int k = _keys.IndexOf(e->Code);
	if (k >= 0)
	{
		if (_handlers[k])
		{
			_selected = _box->Selected;
			if (_ii && _selected >= 0)
				_selected = _ii[_selected];
			MenuEventArgs a((_selected >= 0 ? _items[_selected] : nullptr));
			_handlers[k]((Sender ? Sender : this), %a);
			if (a.Ignore)
			{
				e->Ignore = true;
				return;
			}
			d->Close();
			if (a.Restart)
			{
				_breakKey = -1;
				_toFilter1 = true;
				_toFilter2 = true;
			}
		}
		else
		{
			_breakKey = e->Code;
			d->Close();
		}
		return;
	}

	// filter
	if (e->Code == _FilterKey && FilterOptions != PatternOptions::None)
	{
		// input filter
		Regex^ re;
		String^ filter = InputFilter(Filter, FilterOptions, FilterHistory, re);
		if (!filter)
			return;

		// reset
		Filter = filter;
		_re1 = re;
		_toFilter1 = true;
		_toFilter2 = true;
		d->Close();
		return;
	}

	// CtrlC: copy to clipboard
	if (e->Code == (KeyMode::Ctrl | 'C') || e->Code == (KeyMode::Ctrl | KeyCode::Ins))
	{
		FarListBox^ box = (FarListBox^)e->Control;
		Far::Instance->CopyToClipboard(box->Text);
		e->Ignore = true;
		return;
	}

	// incremental
	if (int(IncrementalOptions))
	{
		if (e->Code == KeyCode::BS || e->Code == (KeyCode::BS | KeyMode::Shift))
		{
			if (_filter2->Length)
			{
				// case: Shift, drop incremental
				if (e->Code != KeyCode::BS)
				{
					Incremental = String::Empty;
					_toFilter1 = true;
					_toFilter2 = false;
					d->Close();
					return;
				}

				if (_filter2->Length > Incremental->Length || _filter2->Length == Incremental->Length && Incremental->EndsWith("*"))
				{
					Char c = _filter2[_filter2->Length - 1];
					_filter2 = _filter2->Substring(0, _filter2->Length - 1);
					_re2 = nullptr;
					// * and ?
					if (!int(IncrementalOptions & PatternOptions::Literal) && (c == '*' || c == '?'))
					{
						// update title/bottom
						String^ t; String^ b; GetInfo(t, b);
						d->_items[0]->Text = t;
						d->_items[2]->Text = b;
					}
					else
					{
						_toFilter1 = true;
						_toFilter2 = true;
					}
				}
			}
		}
		else
		{
			Char c = Far::Instance->CodeToChar(e->Code);
			if (c >= ' ')
			{
				// keep and change filter
				String^ filter2Bak = _filter2;
				Regex^ re2Bak = _re2;
				_filter2 += c;
				_re2 = nullptr;

				// * and ?
				if (!int(IncrementalOptions & PatternOptions::Literal) && (c == '*' || c == '?'))
				{
					// update title/bottom
					String^ t; String^ b; GetInfo(t, b);
					d->_items[0]->Text = t;
					d->_items[2]->Text = b;
					return;
				}

				// try the filter, rollback on empty
				List<int>^ iiBak = _ii;
				_toFilter2 = true;
				MakeFilters();
				if (_ii && _ii->Count == 0)
				{
					_filter2 = filter2Bak;
					_re2 = re2Bak;
					_ii = iiBak;
					return;
				}

				_toFilter2 = true;
			}

		}
		if (_toFilter1 || _toFilter2)
			d->Close();
		return;
	}
}

bool ListMenu::Show()
{
	// main loop
	_toFilter1 = _toFilter2 = true;
	for(int pass = 0;; ++pass)
	{
		// filter
		MakeFilters();

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

		// title, bottom
		String^ title; String^ info;
		GetInfo(title, info);

		// margins
		const int ms = ScreenMargin > 1 ? ScreenMargin : 1;
		const int mx = UsualMargins ? 2 : 0;
		const int my = UsualMargins ? 1 : 0;

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
		w += 2 + 2*mx; // if less last chars are lost

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
		int dw = w + 4, dx = _x;
		ValidateRect(dx, dw, ms, Console::WindowWidth - 2*ms);
		int dh = n + 2 + 2*my, dy = _y;
		ValidateRect(dy, dh, ms, Console::WindowHeight - 2*ms);

		// dialog
		FarDialog dialog(dx, dy, dx + dw - 1, dy + dh - 1);
		dialog.HelpTopic = SS(HelpTopic) ? HelpTopic : "ListMenu";
		dialog.NoShadow = NoShadow;

		// title
		dialog.AddBox(mx, my, dw - 1 - mx, dh - 1 - my, title);

		// list
		_box = (FarListBox^)dialog.AddListBox(1 + mx, 1 + my, dw - 2 - mx, dh - 2 - my, String::Empty);
		_box->Selected = _selected;
		_box->SelectLast = SelectLast;
		_box->NoBox = true;
		if (IncrementalOptions == PatternOptions::None)
		{
			_box->AutoAssignHotkeys = AutoAssignHotkeys;
			_box->NoAmpersands = !ShowAmpersands;
			_box->WrapCursor = WrapCursor;
		}

		// "bottom"
		dialog.AddText(1 + mx, dh - 1 - my, 0, info);

		// items and filter
		_box->_Items = _items;
		_box->_ii = _ii;

		// filter
		_box->_KeyPressed += gcnew EventHandler<KeyPressedEventArgs^>(this, &ListMenu::OnKeyPressed);

		// go!
		_toFilter1 = _toFilter2 = false;
		_breakKey = 0;
		bool ok = dialog.Show();
		if (!ok)
			return false;
		if (_breakKey == -1 || _toFilter1 || _toFilter2)
			continue;

		// correct by filter
		_selected = _box->Selected;
		if (_ii && _selected >= 0)
			_selected = _ii[_selected];

		// OnClick
		if (_selected >= 0)
		{
			MenuItem^ item = (MenuItem^)_items[_selected];
			MenuEventArgs e(item);
			if (item->_OnClick)
				item->_OnClick((Sender ? Sender : this), %e);
			if (e.Ignore || e.Restart)
				continue;
		}

		//! [Enter] on empty gives -1
		return _selected >= 0;
	}
}
}
