
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

#include "StdAfx.h"
#include "ListMenu.h"
#include "Dialog.h"
#include "DialogControls.h"
#include "Far0.h"
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
				Message::Show(e->Message, "Filter expression", MessageOptions::Ok, nullptr, nullptr);
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

ListMenu::ListMenu()
: _Incremental_(String::Empty)
, _filter(String::Empty)
{
}

String^ ListMenu::Incremental::get() { return _Incremental_; }
void ListMenu::Incremental::set(String^ value)
{
	if (!value) throw gcnew ArgumentNullException("value");
	_Incremental_ = value;
	_filter = value;
	_re = nullptr;
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
	return JoinText(r, Bottom);
}

void ListMenu::GetInfo(String^& head, String^& foot)
{
	head = Title ? Title : String::Empty;
	foot = NoInfo ? String::Empty : InfoLine();
	if (SS(Bottom))
		foot += " ";
	if (int(IncrementalOptions) && _filter->Length)
	{
		if (SelectLast)
			foot = "[" + _filter + "]" + foot;
		else
			head = JoinText("[" + _filter + "]", head);
	}
}

void ListMenu::MakeFilter()
{
	// filter
	if (!_toFilter)
		return;
	_toFilter = false;

	// Don't filter by predefined, assume that predefined filter is usually used without permanent.
	// E.g. TabEx: 'sc[Tab]' gets 'Set-Contents' which doesn't match prefix 'sc', but it should be shown.
	if (_filter == Incremental)
		return;

	// create
	if (!_re)
		_re = CreateRegex(_filter, IncrementalOptions, nullptr);
	if (!_re)
		return;

	// case: filter already filtered
	if (_ii)
	{
		List<int>^ ii = gcnew List<int>;
		for each(int k in _ii)
		{
			if (_re->IsMatch(_items[k]->Text))
				ii->Add(k);
		}
		_ii = ii;
		return;
	}

	// case: not yet filtered
	_ii = gcnew List<int>;
	int i = -1;
	for each(FarItem^ mi in Items)
	{
		++i;
		if (_re->IsMatch(mi->Text))
			_ii->Add(i);
	}
}

void ListMenu::MakeSizes(FarDialog^ dialog, Point size)
{
	// controls with text
	IControl^ border = dialog[0];
	IControl^ bottom = dialog[2];

	// text lengths
	String^ borderText = border->Text;
	int borderTextLength = borderText ? borderText->Length : 0;
	String^ bottomText = bottom ? bottom->Text : nullptr;
	int bottomTextLength = bottomText ? bottomText->Length : 0;

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
		for each(FarItem^ mi in _items)
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
	if (w < borderTextLength)
		w = borderTextLength + 4;
	if (w < bottomTextLength)
		w = bottomTextLength + 4;
	if (w < 20)
		w = 20;

	// X
	int dw = w + 4, dx = _x;
	ValidateRect(dx, dw, ms, size.X - 2*ms);

	// Y
	int dh = n + 2 + 2*my, dy = _y;
	ValidateRect(dy, dh, ms, size.Y - 2*ms);

	// dialog
	dialog->Rect = Place(dx, dy, dx + dw - 1, dy + dh - 1);

	// border
	border->Rect = Place(mx, my, dw - 1 - mx, dh - 1 - my);

	// list
	dialog[1]->Rect = Place(1 + mx, 1 + my, dw - 2 - mx, dh - 2 - my);

	// bottom
	if (bottom)
	{
		Point xy(1 + mx, dh - 1 - my);
		bottom->Rect = Place(xy.X, xy.Y, xy.X + bottomTextLength - 1, xy.Y);
	}
}

void ListMenu::OnConsoleSizeChanged(Object^ sender, SizeEventArgs^ e)
{
	MakeSizes((FarDialog^)sender, e->Size);
}

void ListMenu::OnKeyPressed(Object^ sender, KeyPressedEventArgs^ e)
{
	// Tab: go to next
	if (e->Key->VirtualKeyCode == KeyCode::Tab)
	{
		FarListBox^ box = (FarListBox^)e->Control;
		++box->Selected;
		e->Ignore = true;
		return;
	}

	FarDialog^ d = (FarDialog^)sender;

	//! break keys first
	KeyData key(e->Key->VirtualKeyCode, e->Key->CtrlAltShift());
	_keyIndex = _keys.IndexOf(%key);
	if (_keyIndex >= 0)
	{
		if (_handlers[_keyIndex])
		{
			_isKeyHandled = true;
			_selected = _box->Selected;
			if (_ii && _selected >= 0)
				_selected = _ii[_selected];
			MenuEventArgs a((_selected >= 0 ? _items[_selected] : nullptr));
			_handlers[_keyIndex]((Sender ? Sender : this), %a);
			if (a.Ignore)
			{
				e->Ignore = true;
				return;
			}
			d->Close();
			if (a.Restart)
			{
				_keyIndex = -2;
				_toFilter = true;
			}
		}
		else
		{
			d->Close();
		}
		return;
	}

	// CtrlC: copy to clipboard
	if (e->Key->IsCtrl(KeyCode::C) || e->Key->IsCtrl(KeyCode::Insert))
	{
		FarListBox^ box = (FarListBox^)e->Control;
		Far::Net->CopyToClipboard(box->Text);
		e->Ignore = true;
		return;
	}

	// incremental
	if (!int(IncrementalOptions))
		return;
	
	if (key.Is(KeyCode::Backspace) || key.IsShift(KeyCode::Backspace))
	{
		if (_filter->Length == 0)
			return;
		
		// case: Shift, drop incremental
		if (key.IsShift())
		{
			Incremental = String::Empty;
			_ii = nullptr;
			_keyIndex = -2;
			_toFilter = false;
			d->Close();
			return;
		}

		if (_filter->Length > Incremental->Length || _filter->Length == Incremental->Length && Incremental->EndsWith("*"))
		{
			Char c = _filter[_filter->Length - 1];
			_filter = _filter->Substring(0, _filter->Length - 1);
			_re = nullptr;
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
				_toFilter = true;
			}
		}
	}
	else
	{
		Char c = e->Key->Character;
		if (c >= ' ')
		{
			// keep and change filter
			String^ filterBak = _filter;
			Regex^ reBak = _re;
			_filter += c;
			_re = nullptr;

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
			_toFilter = true;
			MakeFilter();
			if (_ii && _ii->Count == 0)
			{
				_filter = filterBak;
				_re = reBak;
				_ii = iiBak;
				return;
			}

			_toFilter = true;
		}
	}

	if (_toFilter)
		d->Close();
}

bool ListMenu::Show()
{
	// main loop
	_toFilter = true;
	for(int pass = 0;; ++pass)
	{
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

		// title, bottom
		String^ title; String^ info;
		GetInfo(title, info);

		// dialog
		FarDialog dialog(1, 1, 1, 1);
		dialog.HelpTopic = SS(HelpTopic) ? HelpTopic : "ListMenu";
		dialog.NoShadow = NoShadow;

		// title
		dialog.AddBox(1, 1, 1, 1, title);

		// list
		_box = (FarListBox^)dialog.AddListBox(1, 1, 1, 1, String::Empty);
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
		if (info->Length)
			dialog.AddText(1, 1, 1, info);

		// items and filter
		_box->_Items = _items;
		_box->_ii = _ii;

		// now we are ready to make sizes
		MakeSizes(%dialog, Far::Net->UI->WindowSize);

		// handlers
		dialog._ConsoleSizeChanged += gcnew EventHandler<SizeEventArgs^>(this, &ListMenu::OnConsoleSizeChanged);
		_box->_KeyPressed += gcnew EventHandler<KeyPressedEventArgs^>(this, &ListMenu::OnKeyPressed);

		// go!
		_toFilter = _isKeyHandled = false;
		_keyIndex = -1;
		bool ok = dialog.Show();
		if (!ok)
			return false;
		if (_keyIndex == -2 || _toFilter)
			continue;

		// correct by filter
		_selected = _box->Selected;
		if (_ii && _selected >= 0)
			_selected = _ii[_selected];

		// call click if a key was not handled yet
		if (_selected >= 0 && !_isKeyHandled)
		{
			FarItem^ item = _items[_selected];
			if (item->Click)
			{
				MenuEventArgs e(item);
				item->Click((Sender ? Sender : this), %e);
				if (e.Ignore || e.Restart)
					continue;
			}
		}

		//! [Enter] on empty gives -1
		return _selected >= 0;
	}
}

}
