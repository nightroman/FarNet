
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

#include "StdAfx.h"
#include "DialogControls.h"
#include "Dialog.h"
#include "DialogLine.h"
#include "ListItemCollection.h"

#define SET_FLAG(Var, Flag, Value) { if (Value) Var |= Flag; else Var &= ~Flag; }

#define DEF_CONTROL_FLAG(Class, Prop, Flag)\
bool Class::Prop::get() { return GetFlag(Flag); }\
void Class::Prop::set(bool value) { SetFlag(Flag, value); }

namespace FarNet
{;
#pragma region FarControl

FarControl::FarControl(FarDialog^ dialog, int index)
: _dialog(dialog)
, _id(index)
{}

FarControl::FarControl(FarDialog^ dialog, int left, int top, int right, int bottom, String^ text)
: _dialog(dialog)
, _rect(left, top, right, bottom)
, _text(text)
{}

//! Info.SendDlgMessage() must be called only for the opened dialog or Far crashes.
//! Bug case: called from a handler of a button that has just closed the dialog.
//! Any Info.SendDlgMessage() caller has to call this or check the handle.
void FarControl::AssertOpened()
{
	if (_dialog->_hDlg == INVALID_HANDLE_VALUE)
		throw gcnew InvalidOperationException("The dialog has to be opened.");
}

String^ FarControl::ToString()
{
	//! do not use fields
	String^ r = Rect.ToString();
	String^ text = Text;
	if (SS(text))
		r += " " + text;
	return r;
}

void FarControl::Init(FarDialogItem& item, FARDIALOGITEMTYPES type)
{
	_item = &item;
	memset(_item, 0, sizeof(FarDialogItem));
	
	item.Type = type;
	item.X1 = _rect.Left;
	item.Y1 = _rect.Top;
	item.X2 = _rect.Right;
	item.Y2 = _rect.Bottom;
	item.Selected = _selected;
	item.Flags = _flags;
	if (Text)
		item.Data = NewChars(Text);
}

// Called internally when a dialog has exited but still exists
void FarControl::Stop(bool ok)
{
	if (ok)
	{
		FarDialogItem di;
		Info.SendDlgMessage(_dialog->_hDlg, DM_GETDLGITEMSHORT, Id, &di);
		_selected = (int)di.Selected;
		_flags = di.Flags;
	}
}

void FarControl::Free()
{
	delete _item->Data;
	_item->Data = nullptr;
}

bool FarControl::GetFlag(int flag)
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
	{
		FarDialogItem di;
		Info.SendDlgMessage(_dialog->_hDlg, DM_GETDLGITEMSHORT, Id, &di);
		return (di.Flags & flag) != 0;
	}
	else
	{
		return (_flags & flag) != 0;
	}
}

void FarControl::SetFlag(int flag, bool value)
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
	{
		FarDialogItem di;
		Info.SendDlgMessage(_dialog->_hDlg, DM_GETDLGITEMSHORT, Id, &di);
		if (value == ((di.Flags & flag) != 0))
			return;
		SET_FLAG(di.Flags, flag, value);
		Info.SendDlgMessage(_dialog->_hDlg, DM_SETDLGITEMSHORT, Id, &di);
	}
	else
	{
		SET_FLAG(_flags, flag, value);
	}
}

int FarControl::GetSelected()
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
		return (int)Info.SendDlgMessage(_dialog->_hDlg, DM_GETCHECK, Id, 0);
	else
		return _selected;
}

void FarControl::SetSelected(int value)
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
		Info.SendDlgMessage(_dialog->_hDlg, DM_SETCHECK, Id, (void*)value);
	else
		_selected = value;
}

bool FarControl::GetChanged()
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
		return !Info.SendDlgMessage(_dialog->_hDlg, DM_EDITUNCHANGEDFLAG, Id, (void*)(-1));
	else
		return false;
}

void FarControl::SetChanged(bool value)
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
		Info.SendDlgMessage(_dialog->_hDlg, DM_EDITUNCHANGEDFLAG, Id, (void*)(!value));
}

bool FarControl::Disabled::get()
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
		return Info.SendDlgMessage(_dialog->_hDlg, DM_ENABLE, Id, (void*)-1) == 0;
	else
		return (_flags & DIF_DISABLE) != 0;
}

void FarControl::Disabled::set(bool value)
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
		Info.SendDlgMessage(_dialog->_hDlg, DM_ENABLE, Id, (void*)!value);
	else
		SET_FLAG(_flags, DIF_DISABLE, value);
}

bool FarControl::Hidden::get()
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
		return Info.SendDlgMessage(_dialog->_hDlg, DM_SHOWITEM, Id, (void*)-1) == 0;
	else
		return (_flags & DIF_HIDDEN) != 0;
}

void FarControl::Hidden::set(bool value)
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
		Info.SendDlgMessage(_dialog->_hDlg, DM_SHOWITEM, Id, (void*)!value);
	else
		SET_FLAG(_flags, DIF_HIDDEN, value);
}

String^ FarControl::Text::get()
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
		return gcnew String((const wchar_t*)Info.SendDlgMessage(_dialog->_hDlg, DM_GETCONSTTEXTPTR, Id, 0));
	else
		return _text;
}

void FarControl::Text::set(String^ value)
{
	if (!value)
		throw gcnew ArgumentNullException("value");

	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
	{
		PIN_NE(pin, value);
		Info.SendDlgMessage(_dialog->_hDlg, DM_SETTEXTPTR, Id, (wchar_t*)pin);
	}
	else
	{
		_text = value;
	}
}

// Gets always, data is null for the native dialog control.
Object^ FarControl::Data::get()
{
	return _data;
}

// Throws for the native dialog control.
void FarControl::Data::set(Object^ value)
{
	if (!_dialog->_items)
		throw gcnew NotSupportedException("Setting data is not supported for native dialogs.");

	_data = value;
}

Place FarControl::Rect::get()
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
	{
		SMALL_RECT arg;
		if (!Info.SendDlgMessage(_dialog->_hDlg, DM_GETITEMPOSITION, Id, &arg))
			return Place();
		return Place(arg.Left, arg.Top, arg.Right, arg.Bottom);
	}
	else
	{
		return _rect;
	}
}

void FarControl::Rect::set(Place value)
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
	{
		SMALL_RECT arg = { (SHORT)value.Left, (SHORT)value.Top, (SHORT)value.Right, (SHORT)value.Bottom };
		if (!Info.SendDlgMessage(_dialog->_hDlg, DM_SETITEMPOSITION, Id, &arg))
			throw gcnew InvalidOperationException(__FUNCTION__);
	}
	else
	{
		_rect = value;
	}
}

#pragma endregion

#pragma region FarBox

FarBox::FarBox(FarDialog^ dialog, int index)
: FarControl(dialog, index)
{
}

FarBox::FarBox(FarDialog^ dialog, int left, int top, int right, int bottom, String^ text)
: FarControl(dialog, left, top, right, bottom, text)
{
}

DEF_CONTROL_FLAG(FarBox, LeftText, DIF_LEFTTEXT);
DEF_CONTROL_FLAG(FarBox, ShowAmpersand, DIF_SHOWAMPERSAND);

void FarBox::Starting(FarDialogItem& item)
{
	Init(item, Single ? DI_SINGLEBOX : DI_DOUBLEBOX);
}

#pragma endregion

#pragma region FarButton

FarButton::FarButton(FarDialog^ dialog, int index)
: FarControl(dialog, index)
{
}

FarButton::FarButton(FarDialog^ dialog, int left, int top, String^ text)
: FarControl(dialog, left, top, 0, top, text)
{
}

DEF_CONTROL_FLAG(FarButton, CenterGroup, DIF_CENTERGROUP);
DEF_CONTROL_FLAG(FarButton, NoBrackets, DIF_NOBRACKETS);
DEF_CONTROL_FLAG(FarButton, NoClose, DIF_BTNNOCLOSE);
DEF_CONTROL_FLAG(FarButton, NoFocus, DIF_NOFOCUS);
DEF_CONTROL_FLAG(FarButton, ShowAmpersand, DIF_SHOWAMPERSAND);

void FarButton::Starting(FarDialogItem& item)
{
	Init(item, DI_BUTTON);
}

//! Default one gets text with [] and {}, so does DM_GETCONSTTEXTPTR
String^ FarButton::Text::get()
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
	{
		FarDialogItemData arg = {sizeof(FarDialogItemData)};
		arg.PtrLength = Info.SendDlgMessage(_dialog->_hDlg, DM_GETTEXT, Id, 0);
		CBox box(arg.PtrLength);
		arg.PtrData = box;
		Info.SendDlgMessage(_dialog->_hDlg, DM_GETTEXT, Id, &arg);
		return gcnew String(box);
	}
	else
	{
		return _text;
	}
}
//! for reflection to see the setter, too
void FarButton::Text::set(String^ value)
{
	FarControl::Text = value;
}

#pragma endregion

#pragma region FarCheckBox

FarCheckBox::FarCheckBox(FarDialog^ dialog, int index)
: FarControl(dialog, index)
{
}

FarCheckBox::FarCheckBox(FarDialog^ dialog, int left, int top, String^ text)
: FarControl(dialog, left, top, 0, top, text)
{
}

DEF_CONTROL_FLAG(FarCheckBox, CenterGroup, DIF_CENTERGROUP);
DEF_CONTROL_FLAG(FarCheckBox, NoFocus, DIF_NOFOCUS);
DEF_CONTROL_FLAG(FarCheckBox, ShowAmpersand, DIF_SHOWAMPERSAND);
DEF_CONTROL_FLAG(FarCheckBox, ThreeState, DIF_3STATE);

void FarCheckBox::Starting(FarDialogItem& item)
{
	Init(item, DI_CHECKBOX);
}

int FarCheckBox::Selected::get()
{
	return GetSelected();
}

void FarCheckBox::Selected::set(int value)
{
	SetSelected(value);
}

#pragma endregion

#pragma region FarEdit

FarEdit::FarEdit(FarDialog^ dialog, int index, FARDIALOGITEMTYPES type)
: FarControl(dialog, index)
, _type(type)
{
}

FarEdit::FarEdit(FarDialog^ dialog, int left, int top, int right, String^ text, FARDIALOGITEMTYPES type)
: FarControl(dialog, left, top, right, top, text)
, _type(type)
{
}

DEF_CONTROL_FLAG(FarEdit, Editor, DIF_EDITOR);
DEF_CONTROL_FLAG(FarEdit, ExpandEnvironmentVariables, DIF_EDITEXPAND);
DEF_CONTROL_FLAG(FarEdit, IsPath, DIF_EDITPATH);
DEF_CONTROL_FLAG(FarEdit, ManualAddHistory, DIF_MANUALADDHISTORY);
DEF_CONTROL_FLAG(FarEdit, NoAutoComplete, DIF_NOAUTOCOMPLETE);
DEF_CONTROL_FLAG(FarEdit, NoFocus, DIF_NOFOCUS);
DEF_CONTROL_FLAG(FarEdit, ReadOnly, DIF_READONLY);
DEF_CONTROL_FLAG(FarEdit, SelectOnEntry, DIF_SELECTONENTRY);
DEF_CONTROL_FLAG(FarEdit, UseLastHistory, DIF_USELASTHISTORY);

bool FarEdit::Fixed::get()
{
	return _type == DI_FIXEDIT;
}

bool FarEdit::IsTouched::get()
{
	return GetChanged();
}

void FarEdit::IsTouched::set(bool value)
{
	SetChanged(value);
}

bool FarEdit::IsPassword::get()
{
	return _type == DI_PSWEDIT;
}

String^ FarEdit::History::get()
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
	{
		CBin bin;
		FarGetDialogItem gdi;
		Call_DM_GETDLGITEM(bin, gdi, _dialog->_hDlg, Id);
		return gcnew String(gdi.Item->History);
	}
	else
	{
		return _history;
	}
}

void FarEdit::History::set(String^ value)
{
	//! We can DM_SETHISTORY but why?
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
		throw gcnew NotImplementedException;

	_history = value;
}

String^ FarEdit::Mask::get()
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
	{
		CBin bin;
		FarGetDialogItem gdi;
		Call_DM_GETDLGITEM(bin, gdi, _dialog->_hDlg, Id);
		return gcnew String(gdi.Item->Mask);
	}
	else
	{
		return _mask;
	}
}

void FarEdit::Mask::set(String^ value)
{
	//! Perhaps we can but why?
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
		throw gcnew NotImplementedException;

	if (_type != DI_FIXEDIT)
		throw gcnew InvalidOperationException("Masks are supported by fixed size edit controls.");

	_mask = value;
}

void FarEdit::Starting(FarDialogItem& item)
{
	Init(item, _type);
	
	if (SS(_history))
	{
		item.Flags |= DIF_HISTORY;
		item.History = NewChars(_history);
	}

	if (SS(_mask))
	{
		item.Flags |= DIF_MASKEDIT;
		item.Mask = NewChars(_mask);
	}
}

void FarEdit::Stop(bool ok)
{
	FarControl::Stop(ok);
	if (ok)
		_text = Text;
}

void FarEdit::Free()
{
	FarControl::Free();
	
	delete _item->History;
	_item->History = nullptr;
	
	delete _item->Mask;
	_item->Mask = nullptr;
}

ILine^ FarEdit::Line::get()
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
		return gcnew DialogLine(_dialog->_hDlg, Id);
	else
		return nullptr;
}

#pragma endregion

#pragma region FarRadioButton

FarRadioButton::FarRadioButton(FarDialog^ dialog, int index)
: FarControl(dialog, index)
{
}

FarRadioButton::FarRadioButton(FarDialog^ dialog, int left, int top, String^ text)
: FarControl(dialog, left, top, 0, top, text)
{
}

DEF_CONTROL_FLAG(FarRadioButton, CenterGroup, DIF_CENTERGROUP);
DEF_CONTROL_FLAG(FarRadioButton, Group, DIF_GROUP);
DEF_CONTROL_FLAG(FarRadioButton, MoveSelect, DIF_MOVESELECT);
DEF_CONTROL_FLAG(FarRadioButton, NoFocus, DIF_NOFOCUS);
DEF_CONTROL_FLAG(FarRadioButton, ShowAmpersand, DIF_SHOWAMPERSAND);

void FarRadioButton::Starting(FarDialogItem& item)
{
	Init(item, DI_RADIOBUTTON);
}

bool FarRadioButton::Selected::get()
{
	return GetSelected() != 0;
}

void FarRadioButton::Selected::set(bool value)
{
	SetSelected(value);
}

#pragma endregion

#pragma region FarText

FarText::FarText(FarDialog^ dialog, int index)
: FarControl(dialog, index)
{
}

FarText::FarText(FarDialog^ dialog, int left, int top, int right, int bottom, String^ text)
: FarControl(dialog, left, top, right, bottom, text)
{
}

DEF_CONTROL_FLAG(FarText, BoxColor, DIF_BOXCOLOR);
DEF_CONTROL_FLAG(FarText, Centered, DIF_CENTERTEXT);
DEF_CONTROL_FLAG(FarText, CenterGroup, DIF_CENTERGROUP);
DEF_CONTROL_FLAG(FarText, ShowAmpersand, DIF_SHOWAMPERSAND);

int FarText::Separator::get()
{
	if (GetFlag(DIF_SEPARATOR))
		return 1;
	if (GetFlag(DIF_SEPARATOR2))
		return 2;
	return 0;
}

void FarText::Separator::set(int value)
{
	switch(value)
	{
	case 0:
		SetFlag(DIF_SEPARATOR, false);
		SetFlag(DIF_SEPARATOR2, false);
		break;
	case 1:
		SetFlag(DIF_SEPARATOR, true);
		SetFlag(DIF_SEPARATOR2, false);
		break;
	case 2:
		SetFlag(DIF_SEPARATOR, false);
		SetFlag(DIF_SEPARATOR2, true);
		break;
	default:
		throw gcnew ArgumentOutOfRangeException("value");
	}
}

bool FarText::Vertical::get()
{
	Place rect = Rect;
	return rect.Top != rect.Bottom;
}

void FarText::Starting(FarDialogItem& item)
{
	Init(item, _rect.Top == _rect.Bottom ? DI_TEXT : DI_VTEXT);
}

#pragma endregion

#pragma region FarUserControl

FarUserControl::FarUserControl(FarDialog^ dialog, int index)
: FarControl(dialog, index)
{
}

FarUserControl::FarUserControl(FarDialog^ dialog, int left, int top, int right, int bottom)
: FarControl(dialog, left, top, right, bottom, String::Empty)
{
}

DEF_CONTROL_FLAG(FarUserControl, NoFocus, DIF_NOFOCUS);

void FarUserControl::Starting(FarDialogItem& item)
{
	Init(item, DI_USERCONTROL);
}

#pragma endregion

#pragma region FarBaseList

FarBaseList::FarBaseList(FarDialog^ dialog, int index)
: FarControl(dialog, index)
{
	_Items = gcnew ListItemCollection(this);
}

FarBaseList::FarBaseList(FarDialog^ dialog, int left, int top, int right, int bottom, String^ text)
: FarControl(dialog, left, top, right, bottom, text)
{
	_selected = -1;
	_Items = gcnew ListItemCollection(this);
}

DEF_CONTROL_FLAG(FarBaseList, AutoAssignHotkeys, DIF_LISTAUTOHIGHLIGHT);
DEF_CONTROL_FLAG(FarBaseList, NoAmpersands, DIF_LISTNOAMPERSAND);
DEF_CONTROL_FLAG(FarBaseList, NoClose, DIF_LISTNOCLOSE);
DEF_CONTROL_FLAG(FarBaseList, NoFocus, DIF_NOFOCUS);
DEF_CONTROL_FLAG(FarBaseList, WrapCursor, DIF_LISTWRAPMODE);

int FarBaseList::Selected::get()
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
		return (int)Info.SendDlgMessage(_dialog->_hDlg, DM_LISTGETCURPOS, Id, 0);
	else
		return _selected;
}

void FarBaseList::Selected::set(int value)
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
	{
		FarListPos arg = {sizeof(arg)};
		arg.SelectPos = value;
		arg.TopPos = -1;
		Info.SendDlgMessage(_dialog->_hDlg, DM_LISTSETCURPOS, Id, &arg);
	}
	else
	{
		_selected = value;
	}
}

IList<FarItem^>^ FarBaseList::Items::get()
{
	return _Items;
}

FarItem^ FarBaseList::Add(String^ text)
{
	FarItem^ r = gcnew SetItem;
	r->Text = text;
	_Items->Add(r);
	return r;
}

void FarBaseList::InitFarListItem(FarListItem& i2, FarItem^ i1)
{
	i2.Text = NewChars(i1->Text);
	i2.Flags = 0;
	if (i1->Checked)
		i2.Flags |= LIF_CHECKED;
	if (i1->Disabled)
		i2.Flags |= LIF_DISABLE;
	if (i1->Grayed)
		i2.Flags |= LIF_GRAYED;
	if (i1->Hidden)
		i2.Flags |= LIF_HIDDEN;
	if (i1->IsSeparator)
		i2.Flags |= LIF_SEPARATOR;
}

void FarBaseList::InitFarListItemShort(FarListItem& i2, FarItem^ i1)
{
	i2.Flags = i2.Reserved[0] = i2.Reserved[1] = i2.Reserved[2] = 0;
	if (i1->Checked)
		i2.Flags |= LIF_CHECKED;
	if (i1->Disabled)
		i2.Flags |= LIF_DISABLE;
	if (i1->Grayed)
		i2.Flags |= LIF_GRAYED;
	if (i1->Hidden)
		i2.Flags |= LIF_HIDDEN;
	if (i1->IsSeparator)
		i2.Flags |= LIF_SEPARATOR;
}

void FarBaseList::Init(FarDialogItem& item, FARDIALOGITEMTYPES type)
{
	FarControl::Init(item, type);

	_pFarList = item.ListItems = new FarList;
	_pFarList->StructSize = sizeof(*_pFarList);

	if (_ii)
	{
		_pFarList->ItemsNumber = _ii->Count;
		_pFarList->Items = new FarListItem[_ii->Count];

		for(int i = _ii->Count; --i >= 0;)
			InitFarListItem(_pFarList->Items[i], _Items[_ii[i]]);
	}
	else
	{
		_pFarList->ItemsNumber = _Items->Count;
		_pFarList->Items = new FarListItem[_Items->Count];

		for(int i = _Items->Count; --i >= 0;)
			InitFarListItem(_pFarList->Items[i], _Items[i]);
	}

	// select an item (same as menu!)
	if (_selected >= (int)_pFarList->ItemsNumber || SelectLast && _selected < 0)
		_selected = (int)_pFarList->ItemsNumber - 1;
	if (_selected >= 0)
		_pFarList->Items[_selected].Flags |= LIF_SELECTED;
}

// Base used to work in Far2. Now the base gets ListItems, not Selected.
void FarBaseList::Stop(bool ok)
{
	if (ok)
		_selected = Selected;
}

void FarBaseList::FreeItems()
{
	if (_pFarList)
	{
		for(int i = (int)_pFarList->ItemsNumber; --i >= 0;)
			delete _pFarList->Items[i].Text;
		delete _pFarList->Items;
		delete _pFarList;
		_pFarList = nullptr;
	}
}

void FarBaseList::Free()
{
	FarControl::Free();
	FreeItems();
}

void FarBaseList::DetachItems()
{
	if (_dialog->_hDlg == INVALID_HANDLE_VALUE)
		throw gcnew InvalidOperationException("Dialog must be started.");

	FreeItems();

	ListItemCollection^ list = dynamic_cast<ListItemCollection^>(_Items);
	if (list)
		list->SetBox(nullptr);
}

void FarBaseList::AttachItems()
{
	AssertOpened();

	FreeItems();

	ListItemCollection^ list = dynamic_cast<ListItemCollection^>(_Items);
	if (list)
		list->SetBox(this);

	FarList arg = {sizeof(arg)};
	arg.Items = new FarListItem[_Items->Count];
	arg.ItemsNumber = _Items->Count;
	for(int i = _Items->Count; --i >= 0;)
		InitFarListItem(arg.Items[i], _Items[i]);

	try
	{
		if (!Info.SendDlgMessage(_dialog->_hDlg, DM_LISTSET, Id, &arg))
			throw gcnew InvalidOperationException();
	}
	finally
	{
		for(int i = _Items->Count; --i >= 0;)
			delete arg.Items[i].Text;
		delete[] arg.Items;
	}
}

#pragma endregion

#pragma region FarComboBox

FarComboBox::FarComboBox(FarDialog^ dialog, int index)
: FarBaseList(dialog, index)
{
}

FarComboBox::FarComboBox(FarDialog^ dialog, int left, int top, int right, String^ text)
: FarBaseList(dialog, left, top, right, top, text)
{
}

DEF_CONTROL_FLAG(FarComboBox, DropDownList, DIF_DROPDOWNLIST);
DEF_CONTROL_FLAG(FarComboBox, ExpandEnvironmentVariables, DIF_EDITEXPAND);
DEF_CONTROL_FLAG(FarComboBox, ReadOnly, DIF_READONLY);
DEF_CONTROL_FLAG(FarComboBox, SelectOnEntry, DIF_SELECTONENTRY);

void FarComboBox::Starting(FarDialogItem& item)
{
	Init(item, DI_COMBOBOX);
}

void FarComboBox::Stop(bool ok)
{
	FarBaseList::Stop(ok);
	if (ok)
		_text = Text;
}

ILine^ FarComboBox::Line::get()
{
	if (_dialog->_hDlg == INVALID_HANDLE_VALUE || DropDownList)
		return nullptr;

	return gcnew DialogLine(_dialog->_hDlg, Id);
}

bool FarComboBox::IsTouched::get()
{
	return GetChanged();
}

void FarComboBox::IsTouched::set(bool value)
{
	SetChanged(value);
}

#pragma endregion

#pragma region FarListBox

FarListBox::FarListBox(FarDialog^ dialog, int index)
: FarBaseList(dialog, index)
{
}

FarListBox::FarListBox(FarDialog^ dialog, int left, int top, int right, int bottom, String^ text)
: FarBaseList(dialog, left, top, right, bottom, nullptr)
, _Title(text)
{
}

DEF_CONTROL_FLAG(FarListBox, NoBox, DIF_LISTNOBOX);

void FarListBox::Starting(FarDialogItem& item)
{
	Init(item, DI_LISTBOX);
	item.Data = NewChars(_Title);
}

void FarListBox::Started()
{
	FarBaseList::Started();
	if (SS(_Bottom))
		Bottom = _Bottom;
}

//! Bottom and Title use the same calls, so that our get/set methods are similar, sync.
String^ FarListBox::Bottom::get()
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
	{
		FarListTitles arg = {sizeof(arg), 0, 0, 0, 0};
		if (!Info.SendDlgMessage(_dialog->_hDlg, DM_LISTGETTITLES, Id, &arg))
			return String::Empty;

		CBox box(arg.BottomSize);
		arg.Bottom = box;

		if (!Info.SendDlgMessage(_dialog->_hDlg, DM_LISTGETTITLES, Id, &arg))
			return String::Empty;

		return gcnew String(box);
	}
	else
	{
		return _Bottom;
	}
}

//! See Bottom::get
String^ FarListBox::Title::get()
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
	{
		FarListTitles arg = {sizeof(arg), 0, 0, 0, 0};
		if (!Info.SendDlgMessage(_dialog->_hDlg, DM_LISTGETTITLES, Id, &arg))
			return String::Empty;

		CBox box(arg.TitleSize);
		arg.Title = box;

		if (!Info.SendDlgMessage(_dialog->_hDlg, DM_LISTGETTITLES, Id, &arg))
			return String::Empty;

		return gcnew String(box);
	}
	else
	{
		return _Title;
	}
}

//! See Bottom::get
void FarListBox::Bottom::set(String^ value)
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
	{
		PIN_NE(pinBottom, value);
		PIN_NE(pinTitle, Title);
		FarListTitles arg = {sizeof(arg)};
		arg.Bottom = pinBottom;
		arg.Title = pinTitle;
		Info.SendDlgMessage(_dialog->_hDlg, DM_LISTSETTITLES, Id, &arg);
	}
	else
	{
		_Bottom = value;
	}
}

//! See Bottom::get
void FarListBox::Title::set(String^ value)
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
	{
		PIN_NE(pinBottom, Bottom);
		PIN_NE(pinTitle, value);
		FarListTitles arg = {sizeof(arg)};
		arg.Bottom = pinBottom;
		arg.Title = pinTitle;
		Info.SendDlgMessage(_dialog->_hDlg, DM_LISTSETTITLES, Id, &arg);
	}
	else
	{
		_Title = value;
	}
}

String^ FarListBox::Text::get()
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
	{
		FarListGetItem list = {sizeof(list)};
		list.ItemIndex = (int)Info.SendDlgMessage(_dialog->_hDlg, DM_LISTGETCURPOS, Id, 0);
		if (!Info.SendDlgMessage(_dialog->_hDlg, DM_LISTGETITEM, Id, &list))
			throw gcnew InvalidOperationException(__FUNCTION__);

		return gcnew String(list.Item.Text);
	}
	else
	{
		return nullptr;
	}
}

void FarListBox::Text::set(String^ value)
{
	if (_dialog->_hDlg != INVALID_HANDLE_VALUE)
		Text = value;
	else
		throw gcnew NotSupportedException;
}

void FarListBox::SetFrame(int selected, int top)
{
	AssertOpened();

	FarListPos arg = {sizeof(arg)};
	arg.SelectPos = selected;
	arg.TopPos = top;
	Info.SendDlgMessage(_dialog->_hDlg, DM_LISTSETCURPOS, Id, &arg);
}

#pragma endregion

}
