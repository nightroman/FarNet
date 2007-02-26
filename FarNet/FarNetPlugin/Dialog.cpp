#include "StdAfx.h"
#include "Dialog.h"
#include "FarImpl.h"

#define SET_FLAG(Var, Flag, Value) { if (Value) Var |= Flag; else Var &= ~Flag; }

#define DEF_CONTROL_FLAG(Class, Prop, Flag)\
bool Class::Prop::get() { return GetFlag(Flag); }\
void Class::Prop::set(bool value) { SetFlag(Flag, value); }

namespace FarManagerImpl
{;
// Dialog callback
long WINAPI FarDialogProc(HANDLE hDlg, int msg, int param1, long param2)
{
	for each(FarDialog^ dialog in FarDialog::_dialogs)
	{
		if(dialog->_hDlg == hDlg)
			return dialog->DialogProc(msg, param1, param2);

		if (msg == DN_INITDIALOG && dialog->_hDlg == 0)
		{
			// set ID
			dialog->_hDlg = hDlg;

			// event
			if (dialog->_initializedHandler)
			{
				InitializedEventArgs ea(param1 < 0 ? nullptr : dialog->_items[param1]);
				dialog->_initializedHandler(dialog, %ea);
				return !ea.Ignore;
			}
			break;
		}
	}
	return Info.DefDlgProc(hDlg, msg, param1, param2);
}

//
// FarEditLineSelection
//

public ref class FarEditLineSelection : public ILineSelection
{
public:
	virtual property String^ Text
	{
		String^ get()
		{
			EditorSelect es;
			Info.SendDlgMessage(_hDlg, DM_GETSELECTION, _id, (long)&es);
			if (es.BlockType == BTYPE_NONE)
				return String::Empty;

			char buf[512];
			Info.SendDlgMessage(_hDlg, DM_GETTEXTPTR, _id, (long)buf);
			return OemToStr(buf + es.BlockStartPos, es.BlockWidth);
		}
		void set(String^ value)
		{
			EditorSelect es;
			Info.SendDlgMessage(_hDlg, DM_GETSELECTION, _id, (long)&es);
			if (es.BlockType == BTYPE_NONE)
				return; //TODO ?

			char buf[512];
			Info.SendDlgMessage(_hDlg, DM_GETTEXTPTR, _id, (long)buf);

			String^ text = OemToStr(buf, es.BlockStartPos) + value + OemToStr(buf + es.BlockStartPos + es.BlockWidth);
			CStr sText(text);
			Info.SendDlgMessage(_hDlg, DM_SETTEXTPTR, _id, (long)(char*)sText);

			es.BlockWidth = value->Length;
			Info.SendDlgMessage(_hDlg, DM_SETSELECTION, _id, (long)&es);
		}
	}
	virtual property int End
	{
		int get()
		{
			EditorSelect es;
			Info.SendDlgMessage(_hDlg, DM_GETSELECTION, _id, (long)&es);
			return es.BlockType == BTYPE_NONE ? -1 : es.BlockStartPos + es.BlockWidth;
		}
	}
	virtual property int Length
	{
		int get()
		{
			EditorSelect es;
			Info.SendDlgMessage(_hDlg, DM_GETSELECTION, _id, (long)&es);
			return es.BlockType == BTYPE_NONE ? 0 : es.BlockWidth;
		}
	}
	virtual property int Start
	{
		int get()
		{
			EditorSelect es;
			Info.SendDlgMessage(_hDlg, DM_GETSELECTION, _id, (long)&es);
			return es.BlockType == BTYPE_NONE ? -1 : es.BlockStartPos;
		}
	}
	virtual String^ ToString() override
	{
		return Text;
	}
internal:
	FarEditLineSelection(HANDLE hDlg, int id) : _hDlg(hDlg), _id(id)
	{
	}
private:
	HANDLE _hDlg;
	int _id;
};

//
// FarEditLine
//

public ref class FarEditLine : public ILine
{
public:
	virtual property ILine^ FullLine
	{
		ILine^ get()
		{
			return this;
		}
	}
	virtual property ILineSelection^ Selection
	{
		ILineSelection^ get()
		{
			return gcnew FarEditLineSelection(_hDlg, _id);
		}
	}
	virtual property int No
	{
		int get()
		{
			return -1;
		}
	}
	virtual property int Pos
	{
		int get()
		{
			COORD c;
			c.Y = 0;
			Info.SendDlgMessage(_hDlg, DM_GETCURSORPOS, _id, (long)&c);
			return c.X;
		}
		void set(int value)
		{
			COORD c;
			c.Y = 0;
			c.X = (SHORT)value;
			Info.SendDlgMessage(_hDlg, DM_SETCURSORPOS, _id, (long)&c);
		}
	}
	virtual property String^ Eol
	{
		String^ get()
		{
			return String::Empty;
		}
		void set(String^)
		{
		}
	}
	virtual property String^ Text
	{
		String^ get()
		{
			char buf[512];
			Info.SendDlgMessage(_hDlg, DM_GETTEXTPTR, _id, (long)buf);
			return OemToStr(buf);
		}
		void set(String^ value)
		{
			CStr sText(value);
			Info.SendDlgMessage(_hDlg, DM_SETTEXTPTR, _id, (long)(char*)sText);
		}
	}
	virtual void Insert(String^ text)
	{
		if (!text)
			throw gcnew ArgumentNullException("text");

		// insert string before cursor
		int pos = Pos;
		String^ str = Text;
		str = str->Substring(0, pos) + text + str->Substring(pos);

		// move cursor to the end of inserted part
		Pos = pos + text->Length;
	}
	virtual void Select(int start, int end)
	{
		EditorSelect es;
		es.BlockType = BTYPE_STREAM;
		es.BlockStartLine = 0;
		es.BlockStartPos = start;
		es.BlockWidth = end - start;
		es.BlockHeight = 1;
		Info.SendDlgMessage(_hDlg, DM_SETSELECTION, _id, (long)&es);
	}
	virtual void Unselect()
	{
		EditorSelect es;
		es.BlockType = BTYPE_NONE;
		Info.SendDlgMessage(_hDlg, DM_SETSELECTION, _id, (long)&es);
	}
internal:
	FarEditLine(HANDLE hDlg, int id) : _hDlg(hDlg), _id(id)
	{
	}
private:
	HANDLE _hDlg;
	int _id;
};

//
// FarControl
//

FarControl::FarControl(FarDialog^ dialog, int left, int top, int right, int bottom, String^ text)
: _rect(left, top, right, bottom)
, _dialog(dialog)
, _text(text)
{
}

String^ FarControl::ToString()
{
	String^ r = _rect.ToString();
	if (!String::IsNullOrEmpty(_text))
		r += " " + _text;
	return r;
}

void FarControl::Setup(FarDialogItem& item, int type)
{
	const int MaxEditLen = sizeof(item.Data) - 1;
	_item = &item;
	item.Type = type;
	item.X1 = _rect.Left;
	item.Y1 = _rect.Top;
	item.X2 = _rect.Right;
	item.Y2 = _rect.Bottom;
	item.Focus = 0;
	item.Selected = _selected;
	item.Flags = _flags;
	item.DefaultButton = 0;
	StrToOem((Text->Length > MaxEditLen ? Text->Substring(0, MaxEditLen) : Text), item.Data);
}

void FarControl::Update(bool ok)
{
	if (ok)
	{
		_selected = _item->Selected;
		_flags = _item->Flags;
		_text = OemToStr(_item->Data);
	}
}

bool FarControl::GetFlag(int flag)
{
	if (_dialog->_hDlg)
	{
		FarDialogItem di;
		Info.SendDlgMessage(_dialog->_hDlg, DM_GETDLGITEM, Id, (long)&di);
		return (di.Flags & flag) != 0;
	}
	else
	{
		return (_flags & flag) != 0;
	}
}

void FarControl::SetFlag(int flag, bool value)
{
	if (_dialog->_hDlg)
	{
		FarDialogItem di;
		Info.SendDlgMessage(_dialog->_hDlg, DM_GETDLGITEM, Id, (long)&di);
		if (value == ((di.Flags & flag) != 0))
			return;
		SET_FLAG(di.Flags, flag, value);
		Info.SendDlgMessage(_dialog->_hDlg, DM_SETDLGITEM, Id, (long)&di);
	}
	else
	{
		SET_FLAG(_flags, flag, value);
	}
}

int FarControl::GetSelected()
{
	if (_dialog->_hDlg)
	{
		FarDialogItem di;
		Info.SendDlgMessage(_dialog->_hDlg, DM_GETDLGITEM, Id, (long)&di);
		return di.Selected;
	}
	else
	{
		return _selected;
	}
}

void FarControl::SetSelected(int value)
{
	if (_dialog->_hDlg)
	{
		FarDialogItem di;
		Info.SendDlgMessage(_dialog->_hDlg, DM_GETDLGITEM, Id, (long)&di);
		if (di.Selected == value)
			return;
		di.Selected = value;
		Info.SendDlgMessage(_dialog->_hDlg, DM_SETDLGITEM, Id, (long)&di);
	}
	else
	{
		_selected = value;
	}
}

bool FarControl::Disabled::get()
{
	if (_dialog->_hDlg)
	{
		return Info.SendDlgMessage(_dialog->_hDlg, DM_ENABLE, Id, -1) == 0;
	}
	else
	{
		return (_flags & DIF_DISABLE) != 0;
	}
}

void FarControl::Disabled::set(bool value)
{
	if (_dialog->_hDlg)
	{
		Info.SendDlgMessage(_dialog->_hDlg, DM_ENABLE, Id, !value);
	}
	else
	{
		SET_FLAG(_flags, DIF_DISABLE, value);
	}
}

bool FarControl::Hidden::get()
{
	if (_dialog->_hDlg)
	{
		return Info.SendDlgMessage(_dialog->_hDlg, DM_SHOWITEM, Id, -1) == 0;
	}
	else
	{
		return (_flags & DIF_HIDDEN) != 0;
	}
}

void FarControl::Hidden::set(bool value)
{
	if (_dialog->_hDlg)
	{
		Info.SendDlgMessage(_dialog->_hDlg, DM_SHOWITEM, Id, !value);
	}
	else
	{
		SET_FLAG(_flags, DIF_HIDDEN, value);
	}
}

String^ FarControl::Text::get()
{
	if (_dialog->_hDlg)
	{
		char buf[512];
		Info.SendDlgMessage(_dialog->_hDlg, DM_GETTEXTPTR, Id, (long)buf);
		return OemToStr(buf);
	}
	else
	{
		return _text;
	}
}

void FarControl::Text::set(String^ value)
{
	if (_dialog->_hDlg)
	{
		CStr sText(value);
		Info.SendDlgMessage(_dialog->_hDlg, DM_SETTEXTPTR, Id, (long)(char*)sText);
	}
	else
	{
		_text = value;
	}
}

//
// FarBox
//

FarBox::FarBox(FarDialog^ dialog, int left, int top, int right, int bottom, String^ text)
: FarControl(dialog, left, top, right, bottom, text)
{
}

DEF_CONTROL_FLAG(FarBox, LeftText, DIF_LEFTTEXT);
DEF_CONTROL_FLAG(FarBox, ShowAmpersand, DIF_SHOWAMPERSAND);

void FarBox::Setup(FarDialogItem& item)
{
	Setup(item, Single ? DI_SINGLEBOX : DI_DOUBLEBOX);
}

//
// FarButton
//

FarButton::FarButton(FarDialog^ dialog, int left, int top, String^ text)
: FarControl(dialog, left, top, 0, top, text)
{
}

DEF_CONTROL_FLAG(FarButton, CenterGroup, DIF_CENTERGROUP);
DEF_CONTROL_FLAG(FarButton, NoBrackets, DIF_NOBRACKETS);
DEF_CONTROL_FLAG(FarButton, NoClose, DIF_BTNNOCLOSE);
DEF_CONTROL_FLAG(FarButton, NoFocus, DIF_NOFOCUS);
DEF_CONTROL_FLAG(FarButton, ShowAmpersand, DIF_SHOWAMPERSAND);

void FarButton::Setup(FarDialogItem& item)
{
	Setup(item, DI_BUTTON);
}

//
// FarCheckBox
//

FarCheckBox::FarCheckBox(FarDialog^ dialog, int left, int top, String^ text)
: FarControl(dialog, left, top, 0, top, text)
{
}

DEF_CONTROL_FLAG(FarCheckBox, CenterGroup, DIF_CENTERGROUP);
DEF_CONTROL_FLAG(FarCheckBox, NoFocus, DIF_NOFOCUS);
DEF_CONTROL_FLAG(FarCheckBox, ShowAmpersand, DIF_SHOWAMPERSAND);
DEF_CONTROL_FLAG(FarCheckBox, ThreeState, DIF_3STATE);

void FarCheckBox::Setup(FarDialogItem& item)
{
	Setup(item, DI_CHECKBOX);
}

int FarCheckBox::Selected::get()
{
	return GetSelected();
}

void FarCheckBox::Selected::set(int value)
{
	SetSelected(value);
}

//
// FarEdit
//

FarEdit::FarEdit(FarDialog^ dialog, int left, int top, int right, String^ text, int type)
: FarControl(dialog, left, top, right, top, text)
, _type(type)
{
}

DEF_CONTROL_FLAG(FarEdit, Editor, DIF_EDITOR);
DEF_CONTROL_FLAG(FarEdit, EnvExpanded, DIF_EDITEXPAND);
DEF_CONTROL_FLAG(FarEdit, ManualAddHistory, DIF_MANUALADDHISTORY);
DEF_CONTROL_FLAG(FarEdit, NoFocus, DIF_NOFOCUS);
DEF_CONTROL_FLAG(FarEdit, ReadOnly, DIF_READONLY);
DEF_CONTROL_FLAG(FarEdit, SelectOnEntry, DIF_SELECTONENTRY);
DEF_CONTROL_FLAG(FarEdit, UseLastHistory, DIF_USELASTHISTORY);

bool FarEdit::Fixed::get()
{
	return _type == DI_FIXEDIT;
}

bool FarEdit::Password::get()
{
	return _type == DI_PSWEDIT;
}

String^ FarEdit::History::get()
{
	if ((_flags & DIF_HISTORY) == 0)
		return nullptr;
	return _history;
}

void FarEdit::History::set(String^ value)
{
	_history = value;
	_flags &= ~DIF_MASKEDIT;
	if (String::IsNullOrEmpty(value))
		_flags &= ~DIF_HISTORY;
	else
		_flags |= DIF_HISTORY;
}

String^ FarEdit::Mask::get()
{
	if ((_flags & DIF_MASKEDIT) == 0)
		return nullptr;
	return _history;
}

void FarEdit::Mask::set(String^ value)
{
	if (_type != DI_FIXEDIT)
		throw gcnew InvalidOperationException("You can set this only for fixed size edit control.");

	_history = value;
	_flags &= ~DIF_HISTORY;
	if (String::IsNullOrEmpty(value))
		_flags &= ~DIF_MASKEDIT;
	else
		_flags |= DIF_MASKEDIT;
}

void FarEdit::Setup(FarDialogItem& item)
{
	Setup(item, _type);
	if ((_flags & (DIF_HISTORY|DIF_MASKEDIT)) != 0)
	{
		item.History = new char[_history->Length + 1];
		StrToOem(_history, (char*)item.History);
	}
}

void FarEdit::Update(bool ok)
{
	FarControl::Update(ok);
	if (_item->History)
		delete _item->History;
}

ILine^ FarEdit::Line::get()
{
	if (!_dialog->_hDlg)
		return nullptr;
	return gcnew FarEditLine(_dialog->_hDlg, Id);
}

//
// FarRadioButton
//

FarRadioButton::FarRadioButton(FarDialog^ dialog, int left, int top, String^ text)
: FarControl(dialog, left, top, 0, top, text)
{
}

DEF_CONTROL_FLAG(FarRadioButton, CenterGroup, DIF_CENTERGROUP);
DEF_CONTROL_FLAG(FarRadioButton, Group, DIF_GROUP);
DEF_CONTROL_FLAG(FarRadioButton, MoveSelect, DIF_MOVESELECT);
DEF_CONTROL_FLAG(FarRadioButton, NoFocus, DIF_NOFOCUS);
DEF_CONTROL_FLAG(FarRadioButton, ShowAmpersand, DIF_SHOWAMPERSAND);

void FarRadioButton::Setup(FarDialogItem& item)
{
	Setup(item, DI_RADIOBUTTON);
}

bool FarRadioButton::Selected::get()
{
	return GetSelected() != 0;
}

void FarRadioButton::Selected::set(bool value)
{
	SetSelected(value);
}

//
// FarText
//

FarText::FarText(FarDialog^ dialog, int left, int top, int right, int bottom, String^ text)
: FarControl(dialog, left, top, right, bottom, text)
{
}

DEF_CONTROL_FLAG(FarText, BoxColor, DIF_BOXCOLOR);
DEF_CONTROL_FLAG(FarText, Centered, DIF_CENTERTEXT);
DEF_CONTROL_FLAG(FarText, CenterGroup, DIF_CENTERGROUP);
DEF_CONTROL_FLAG(FarText, Separator, DIF_SEPARATOR);
DEF_CONTROL_FLAG(FarText, Separator2, DIF_SEPARATOR2);
DEF_CONTROL_FLAG(FarText, ShowAmpersand, DIF_SHOWAMPERSAND);

void FarText::Setup(FarDialogItem& item)
{
	Setup(item, _rect.Top == _rect.Bottom ? DI_TEXT : DI_VTEXT);
}

//
// ListItem
//

ListItem::ListItem()
{
}

DEF_PROP_FLAG(ListItem, Checked, LIF_CHECKED);
DEF_PROP_FLAG(ListItem, Disabled, LIF_DISABLE);
DEF_PROP_FLAG(ListItem, IsSeparator, LIF_SEPARATOR);

String^ ListItem::ToString()
{
	return Text;
}

//
// FarBaseBox
//

FarBaseBox::FarBaseBox(FarDialog^ dialog, int left, int top, int right, int bottom, String^ text)
: FarControl(dialog, left, top, right, bottom, text)
, _items(gcnew List<IListItem^>())
{
	_selected = -1;
}

DEF_CONTROL_FLAG(FarBaseBox, AutoAssignHotkeys, DIF_LISTAUTOHIGHLIGHT);
DEF_CONTROL_FLAG(FarBaseBox, NoAmpersands, DIF_LISTNOAMPERSAND);
DEF_CONTROL_FLAG(FarBaseBox, NoClose, DIF_LISTNOCLOSE);
DEF_CONTROL_FLAG(FarBaseBox, NoFocus, DIF_NOFOCUS);
DEF_CONTROL_FLAG(FarBaseBox, WrapCursor, DIF_LISTWRAPMODE);

int FarBaseBox::Selected::get()
{
	if (_dialog->_hDlg)
	{
		return Info.SendDlgMessage(_dialog->_hDlg, DM_LISTGETCURPOS, Id, 0);
	}
	else
	{
		return _selected;
	}
}

void FarBaseBox::Selected::set(int value)
{
	if (_dialog->_hDlg)
	{
		FarListPos flp;
		flp.SelectPos = value;
		flp.TopPos = -1;
		Info.SendDlgMessage(_dialog->_hDlg, DM_LISTSETCURPOS, Id, (long)&flp);
	}
	else
	{
		_selected = value;
	}
}

IList<IListItem^>^ FarBaseBox::Items::get()
{
	return _items;
}

IListItem^ FarBaseBox::Add(String^ text)
{
	ListItem^ r = gcnew ListItem();
	r->Text = text;
	_items->Add(r);
	return r;
}

void FarBaseBox::Setup(FarDialogItem& item, int type)
{
	FarControl::Setup(item, type);

	_pFarList = item.ListItems = new FarList;
	_pFarList->ItemsNumber = _items->Count;
	_pFarList->Items = new FarListItem[_items->Count];

	for(int i = _items->Count; --i >= 0;)
	{
		ListItem^ i1 = (ListItem^)_items[i];
		FarListItem& i2 = _pFarList->Items[i];
		i2.Reserved[0] = i2.Reserved[1] = i2.Reserved[2] = 0;
		i2.Flags = i1->_flags;
		StrToOem((i1->Text->Length > 127 ? i1->Text->Substring(0, 127) : i1->Text), i2.Text);
	}

	if (_selected >= 0 && _selected < _items->Count)
		_pFarList->Items[_selected].Flags |= LIF_SELECTED;
}

void FarBaseBox::Update(bool ok)
{
	FarControl::Update(ok);
	if (ok)
		_selected = _item->ListPos;

	delete _pFarList->Items;
	delete _pFarList;
	_pFarList = NULL;
}

//
// FarComboBox
//

FarComboBox::FarComboBox(FarDialog^ dialog, int left, int top, int right, String^ text)
: FarBaseBox(dialog, left, top, right, top, text)
{
}

DEF_CONTROL_FLAG(FarComboBox, DropDownList, DIF_DROPDOWNLIST);
DEF_CONTROL_FLAG(FarComboBox, EnvExpanded, DIF_EDITEXPAND);
DEF_CONTROL_FLAG(FarComboBox, ReadOnly, DIF_READONLY);
DEF_CONTROL_FLAG(FarComboBox, SelectOnEntry, DIF_SELECTONENTRY);

void FarComboBox::Setup(FarDialogItem& item)
{
	Setup(item, DI_COMBOBOX);
}

ILine^ FarComboBox::Line::get()
{
	if (!_dialog->_hDlg)
		return nullptr;
	return gcnew FarEditLine(_dialog->_hDlg, Id);
}

//
// FarListBox
//

FarListBox::FarListBox(FarDialog^ dialog, int left, int top, int right, int bottom, String^ text)
: FarBaseBox(dialog, left, top, right, bottom, text)
{
}

DEF_CONTROL_FLAG(FarListBox, NoBox, DIF_LISTNOBOX);

void FarListBox::Setup(FarDialogItem& item)
{
	Setup(item, DI_LISTBOX);
}

//
// FarDialog
//

FarDialog::FarDialog(Far^ manager, int left, int top, int right, int bottom)
: _rect(left, top, right, bottom)
, _far(manager)
, _items(gcnew List<FarControl^>())
{
	if (left < 0)
	{
		_rect.Left = (Console::WindowWidth - right)/2;
		_rect.Right = _rect.Left + right - 1;
	}
	if (top < 0)
	{
		_rect.Top = (Console::WindowHeight - bottom)/2;
		_rect.Bottom = _rect.Top + bottom - 1;
	}
}

DEF_PROP_FLAG(FarDialog, IsSmall, FDLG_SMALLDIALOG);
DEF_PROP_FLAG(FarDialog, IsWarning, FDLG_WARNING);
DEF_PROP_FLAG(FarDialog, NoPanel, FDLG_NODRAWPANEL);
DEF_PROP_FLAG(FarDialog, NoShadow, FDLG_NODRAWSHADOW);

IControl^ FarDialog::Default::get()
{
	return _default;
}

void FarDialog::Default::set(IControl^ value)
{
	_default = (FarControl^)value;
}

IControl^ FarDialog::Focused::get()
{
	return _focused;
}

void FarDialog::Focused::set(IControl^ value)
{
	_focused = (FarControl^)value;
}

IControl^ FarDialog::Selected::get()
{
	return _selected;
}

void FarDialog::AddItem(FarControl^ item)
{
	item->Id = _items->Count;
	_items->Add(item);
}

IBox^ FarDialog::AddBox(int left, int top, int right, int bottom, String^ text)
{
	FarBox^ r = gcnew FarBox(this, left, top, right, bottom, text);
	AddItem(r);
	return r;
}

IButton^ FarDialog::AddButton(int left, int top, String^ text)
{
	FarButton^ r = gcnew FarButton(this, left, top, text);
	AddItem(r);
	return r;
}

ICheckBox^ FarDialog::AddCheckBox(int left, int top, String^ text)
{
	FarCheckBox^ r = gcnew FarCheckBox(this, left, top, text);
	AddItem(r);
	return r;
}

IComboBox^ FarDialog::AddComboBox(int left, int top, int right, String^ text)
{
	FarComboBox^ r = gcnew FarComboBox(this, left, top, right, text);
	AddItem(r);
	return r;
}

IEdit^ FarDialog::AddEdit(int left, int top, int right, String^ text)
{
	FarEdit^ r = gcnew FarEdit(this, left, top, right, text, DI_EDIT);
	AddItem(r);
	return r;
}

IEdit^ FarDialog::AddEditFixed(int left, int top, int right, String^ text)
{
	FarEdit^ r = gcnew FarEdit(this, left, top, right, text, DI_FIXEDIT);
	AddItem(r);
	return r;
}

IEdit^ FarDialog::AddEditPassword(int left, int top, int right, String^ text)
{
	FarEdit^ r = gcnew FarEdit(this, left, top, right, text, DI_PSWEDIT);
	AddItem(r);
	return r;
}

IListBox^ FarDialog::AddListBox(int left, int top, int right, int bottom, String^ text)
{
	FarListBox^ r = gcnew FarListBox(this, left, top, right, bottom, text);
	AddItem(r);
	return r;
}

IRadioButton^ FarDialog::AddRadioButton(int left, int top, String^ text)
{
	FarRadioButton^ r = gcnew FarRadioButton(this, left, top, text);
	AddItem(r);
	return r;
}

IText^ FarDialog::AddText(int left, int top, int right, String^ text)
{
	FarText^ r = gcnew FarText(this, left, top, right, top, text);
	AddItem(r);
	return r;
}

IText^ FarDialog::AddVerticalText(int left, int top, int bottom, String^ text)
{
	FarText^ r = gcnew FarText(this, left, top, left, bottom, text);
	AddItem(r);
	return r;
}

bool FarDialog::Show()
{
	FarDialogItem* items = new FarDialogItem[_items->Count];
	try
	{
		// setup items
		for(int i = _items->Count; --i >= 0;)
			_items[i]->Setup(items[i]);
		
		// set default
		if (_default)
		{
			int i = _items->IndexOf(_default);
			if (i >= 0)
				items[i].DefaultButton = true;
		}

		// set focused
		if (_focused)
		{
			int i = _items->IndexOf(_focused);
			if (i >= 0)
				items[i].Focus = true;
		}

		// help
		CStr sHelp;
		if (!String::IsNullOrEmpty(HelpTopic))
			sHelp.Set(HelpTopic);

		// show
		_dialogs.Add(this);
		int selected = Info.DialogEx(Info.ModuleNumber,
			_rect.Left, _rect.Top, _rect.Right, _rect.Bottom, sHelp,
			items, _items->Count,
			0, _flags, FarDialogProc, NULL);

		// update
		for(int i = _items->Count; --i >= 0;)
			_items[i]->Update(selected >= 0);

		// result
		if (selected >= 0)
		{
			_selected = _items[selected];
			return true;
		}
		else
		{
			_selected = nullptr;
			return false;
		}
	}
	finally
	{
		_hDlg = 0;
		_dialogs.Remove(this);
		delete items;
	}
}

long FarDialog::DialogProc(int msg, int param1, long param2)
{
	try
	{
		// message:
		switch(msg)
		{
		case DN_CLOSE:
			{
				FarControl^ fc = param1 >= 0 ? _items[param1] : nullptr;
				if (_closingHandler)
				{
					ClosingEventArgs ea(fc);
					_closingHandler(this, %ea);
					if (ea.Ignore)
						return false;
				}
				break;
			}
		case DN_GOTFOCUS:
			{
				FarControl^ fc = _items[param1];
				if (fc->_gotFocusHandler)
				{
					AnyEventArgs ea(fc);
					fc->_gotFocusHandler(this, %ea);
				}
				return 0;
			}
		case DN_KILLFOCUS:
			{
				FarControl^ fc = _items[param1];
				if (fc->_losingFocusHandler)
				{
					LosingFocusEventArgs ea(fc);
					fc->_losingFocusHandler(this, %ea);
					if (ea.Focused)
						return ea.Focused->Id;
				}
				return -1;
			}
		case DN_BTNCLICK:
			{
				FarControl^ fc = _items[param1];
				FarButton^ fb = dynamic_cast<FarButton^>(fc);
				if (fb)
				{
					if (fb->_buttonClickedHandler)
					{
						ButtonClickedEventArgs ea(fb, 0);
						fb->_buttonClickedHandler(this, %ea);
						return ea.Ignore;
					}
					break;
				}
				FarCheckBox^ cb = dynamic_cast<FarCheckBox^>(fc);
				if (cb)
				{
					if (cb->_buttonClickedHandler)
					{
						ButtonClickedEventArgs ea(cb, param2);
						cb->_buttonClickedHandler(this, %ea);
						return !ea.Ignore;
					}
					break;
				}
				FarRadioButton^ rb = dynamic_cast<FarRadioButton^>(fc);
				if (rb)
				{
					if (rb->_buttonClickedHandler)
					{
						ButtonClickedEventArgs ea(rb, param2);
						rb->_buttonClickedHandler(this, %ea);
						return !ea.Ignore;
					}
					break;
				}
				break;
			}
		case DN_EDITCHANGE:
			{
				FarControl^ fc = _items[param1];
				FarEdit^ fe = dynamic_cast<FarEdit^>(fc);
				if (fe)
				{
					if (fe->_textChangedHandler)
					{
						FarDialogItem& item = *(FarDialogItem*)param2;
						TextChangedEventArgs ea(fe, OemToStr(item.Data));
						fe->_textChangedHandler(this, %ea);
						return !ea.Ignore;
					}
					break;
				}
				FarComboBox^ cb = dynamic_cast<FarComboBox^>(fc);
				if (cb)
				{
					if (cb->_textChangedHandler)
					{
						FarDialogItem& item = *(FarDialogItem*)param2;
						TextChangedEventArgs ea(cb, OemToStr(item.Data));
						cb->_textChangedHandler(this, %ea);
						return !ea.Ignore;
					}
					break;
				}
				break;
			}
		case DN_ENTERIDLE:
			{
				if (_idledHandler)
				{
					AnyEventArgs ea(nullptr);
					_idledHandler(this, %ea);
				}
				break;
			}
		case DN_MOUSECLICK:
			{
				FarControl^ fc = param1 >= 0 ? _items[param1] : nullptr;
				if (fc)
				{
					if (fc->_mouseClickedHandler)
					{
						MouseClickedEventArgs ea(fc, GetMouseInfo(*(MOUSE_EVENT_RECORD*)param2));
						fc->_mouseClickedHandler(this, %ea);
						if (ea.Ignore)
							return true;
					}
				}
				else if (_mouseClickedHandler)
				{
					MouseClickedEventArgs ea(nullptr, GetMouseInfo(*(MOUSE_EVENT_RECORD*)param2));
					_mouseClickedHandler(this, %ea);
					if (ea.Ignore)
						return true;
				}
				break;
			}
		case DN_KEY:
			{
				FarControl^ fc = param1 >= 0 ? _items[param1] : nullptr;
				if (fc && fc->_keyPressedHandler)
				{
					KeyPressedEventArgs ea(fc, param2);
					fc->_keyPressedHandler(this, %ea);
					if (ea.Ignore)
						return true;
				}
				if (_keyPressedHandler)
				{
					KeyPressedEventArgs ea(fc, param2);
					_keyPressedHandler(this, %ea);
					if (ea.Ignore)
						return true;
				}
				break;
			}
		}
	}
	catch(Exception^ e)
	{
		_far->ShowError("Error in DlgProc", e);
	}

	// default
	return Info.DefDlgProc(_hDlg, msg, param1, param2);
}
}
