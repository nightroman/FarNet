
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

#include "StdAfx.h"
#include "Dialog.h"
#include "DialogControls.h"
#include "Wrappers.h"

namespace FarNet
{;
// Dialog callback dispatches the event to the specified dialog
INT_PTR WINAPI FarDialogProc(HANDLE hDlg, intptr_t msg, intptr_t param1, void* param2)
{
	for each(FarDialog^ dialog in FarDialog::_dialogs)
	{
		if (dialog->_hDlg == hDlg)
			return dialog->DialogProc(msg, param1, param2);
	}
	return Info.DefDlgProc(hDlg, msg, param1, param2);
}

// Flags properties
DEF_PROP_FLAG(FarDialog, IsSmall, FDLG_SMALLDIALOG);
DEF_PROP_FLAG(FarDialog, IsWarning, FDLG_WARNING);
DEF_PROP_FLAG(FarDialog, KeepWindowTitle, FDLG_KEEPCONSOLETITLE);
DEF_PROP_FLAG(FarDialog, NoPanel, FDLG_NODRAWPANEL);
DEF_PROP_FLAG(FarDialog, NoShadow, FDLG_NODRAWSHADOW);

// Native dialog wrapper
FarDialog::FarDialog(HANDLE hDlg)
: _hDlg(hDlg)
{}

// FarNet dialog host
FarDialog::FarDialog(int left, int top, int right, int bottom)
: _hDlg(INVALID_HANDLE_VALUE)
, _rect(left, top, right, bottom)
, _items(gcnew List<FarControl^>)
{
	if (left < 0 || top < 0)
	{
		Point size = Far::Net->UI->WindowSize;
		if (left < 0)
		{
			_rect.Left = (size.X - right) / 2;
			_rect.Right = _rect.Left + right - 1;
		}
		if (top < 0)
		{
			_rect.Top = (size.Y - bottom) / 2;
			_rect.Bottom = _rect.Top + bottom - 1;
		}
	}
}

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
	if (_hDlg == INVALID_HANDLE_VALUE)
		return _focused;

	int index = (int)Info.SendDlgMessage(_hDlg, DM_GETFOCUS, 0, 0);
	return this[index];
}

void FarDialog::Focused::set(IControl^ value)
{
	FarControl^ control = (FarControl^)value;
	if (_hDlg != INVALID_HANDLE_VALUE)
	{
		if (!value)
			throw gcnew ArgumentNullException("value");
		//! Check handle, not 'this'. It won't matter if we force the same dialog instance later. ??
		if (control->_dialog->_hDlg != this->_hDlg)
			throw gcnew ArgumentException("'value': the control does not belong to this dialog.");
		if (!Info.SendDlgMessage(_hDlg, DM_SETFOCUS, control->Id, 0))
			throw gcnew InvalidOperationException("Cannot set focus to this control.");
	}
	else
	{
		_focused = control;
	}
}

Place FarDialog::Rect::get()
{
	if (_hDlg == INVALID_HANDLE_VALUE)
		return _rect;

	SMALL_RECT arg;
    Info.SendDlgMessage(_hDlg, DM_GETDLGRECT, 0, &arg);
	return Place(arg.Left, arg.Top, arg.Right, arg.Bottom);
}

void FarDialog::Rect::set(Place value)
{
	if (_hDlg != INVALID_HANDLE_VALUE)
	{
		AutoStopDialogRedraw autoStopDialogRedraw(_hDlg);

		Move(value.First, true);
		Resize(value.Size);
	}
	else
	{
		_rect = value;
	}
}

// _091126_135929
Guid FarDialog::TypeId::get()
{
	if (_hDlg == INVALID_HANDLE_VALUE)
		return _typeId;

	// shortcut
	if (_typeId != Guid::Empty)
		return _typeId;

	// request
	DialogInfo arg = { sizeof(DialogInfo) };
	if (Info.SendDlgMessage(_hDlg, DM_GETDIALOGINFO, 0, &arg))
	{
		// get and save it to reuse
		_typeId = FromGUID(arg.Id);
		return _typeId;
	}

	return Guid::Empty;
}

void FarDialog::TypeId::set(Guid value)
{
	if (_hDlg != INVALID_HANDLE_VALUE)
		throw gcnew InvalidOperationException("Cannot set after opening.");
	else
		_typeId = value;
}

IControl^ FarDialog::Selected::get()
{
	return _selected;
}

void FarDialog::AddItem(FarControl^ item)
{
	// add
	item->_id = _items->Count;
	_items->Add(item);

	// done?
	if (NoSmartCoordinates || item->Id == 0)
		return;

	// smart coords
	int i = _items->Count - 2;
	if (item->_rect.Top <= 0)
	{
		item->_rect.Top = _items[i]->_rect.Top - item->_rect.Top;
		if (item->_rect.Bottom <= 0)
			item->_rect.Bottom = item->_rect.Top;
		else
			item->_rect.Bottom += item->_rect.Top;
	}
}

IBox^ FarDialog::AddBox(int left, int top, int right, int bottom, String^ text)
{
	if (right == 0)
		right = _rect.Width - left - 1;
	if (bottom == 0)
		bottom = _rect.Height - top - 1;
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

IListBox^ FarDialog::AddListBox(int left, int top, int right, int bottom, String^ title)
{
	FarListBox^ r = gcnew FarListBox(this, left, top, right, bottom, title);
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

IUserControl^ FarDialog::AddUserControl(int left, int top, int right, int bottom)
{
	FarUserControl^ r = gcnew FarUserControl(this, left, top, right, bottom);
	AddItem(r);
	return r;
}

bool FarDialog::Show()
{
	if (ValueUserScreen::Get()) //_100514_000000
	{
		ValueUserScreen::Set(false);
		Far::Net->UI->SaveUserScreen();
	}

	FarDialogItem* items = new FarDialogItem[_items->Count];
	try
	{
		// setup items
		for(int i = _items->Count; --i >= 0;)
			_items[i]->Starting(items[i]);

		// set default
		if (_default)
		{
			int i = _items->IndexOf(_default);
			if (i >= 0)
				items[i].Flags |= DIF_DEFAULTBUTTON;
		}

		// set focused
		if (_focused)
		{
			int i = _items->IndexOf(_focused);
			if (i >= 0)
				items[i].Flags |= DIF_FOCUS;
		}

		// help
		PIN_NE(pinHelpTopic, HelpTopic);

		// init
		_dialogs.Add(this);
		GUID typeId = ToGUID(_typeId);
		_hDlg = Info.DialogInit(
			&MainGuid,
			&typeId,
			_rect.Left,
			_rect.Top,
			_rect.Right,
			_rect.Bottom,
			pinHelpTopic,
			items,
			_items->Count,
			0,
			_flags,
			FarDialogProc,
			nullptr);

		if (_hDlg == INVALID_HANDLE_VALUE)
			return false;

		// show
		int selected = (int)Info.DialogRun(_hDlg);

		// update
		for(int i = _items->Count; --i >= 0;)
			_items[i]->Stop(selected >= 0);

		// result
		if (selected >= 0)
		{
			_selected = _items[selected];
			return (Object^)_selected != (Object^)Cancel;
		}
		else
		{
			_selected = nullptr;
			return false;
		}
	}
	finally
	{
		// free dialog
		if (_hDlg != INVALID_HANDLE_VALUE)
		{
			Info.DialogFree(_hDlg);
			_hDlg = INVALID_HANDLE_VALUE;
		}

		// delete items
		for(int i = _items->Count; --i >= 0;)
			_items[i]->Free();
		delete[] items;

		// unregister
		_dialogs.Remove(this);
	}
}

void FarDialog::Close()
{
	if (_hDlg != INVALID_HANDLE_VALUE)
		Info.SendDlgMessage(_hDlg, DM_CLOSE, -1, 0);
}

void FarDialog::DisableRedraw()
{
#pragma push_macro("DM_ENABLEREDRAW")
#undef DM_ENABLEREDRAW
	Info.SendDlgMessage(_hDlg, DM_ENABLEREDRAW, FALSE, 0);
#pragma pop_macro("DM_ENABLEREDRAW")
}

void FarDialog::EnableRedraw()
{
#pragma push_macro("DM_ENABLEREDRAW")
#undef DM_ENABLEREDRAW
	Info.SendDlgMessage(_hDlg, DM_ENABLEREDRAW, TRUE, 0);
#pragma pop_macro("DM_ENABLEREDRAW")
}

FarDialog^ FarDialog::GetDialog()
{
	WindowInfo wi;
	Call_ACTL_GETWINDOWINFO(wi, -1);

	if (wi.Id == 0 || wi.Type != WTYPE_DIALOG)
		return nullptr;

	HANDLE hDlg = (HANDLE)wi.Id;
	for each(FarDialog^ dialog in FarDialog::_dialogs)
	{
		if (dialog->_hDlg == hDlg)
			return dialog;
	}

	return gcnew FarDialog(hDlg);
}

//! 090719 There is no way to get control count, so we allow an index to be too large - we return null in this case even for our dialog.
IControl^ FarDialog::default::get(int id)
{
	if (id < 0)
		throw gcnew ArgumentOutOfRangeException("id");

	if (_items)
	{
		if (id >= _items->Count)
			return nullptr;

		return _items[id];
	}

	FarDialogItem di;
	if (!Info.SendDlgMessage(_hDlg, DM_GETDLGITEMSHORT, id, &di))
		return nullptr;

	switch(di.Type)
	{
	case DI_BUTTON:
		return gcnew FarButton(this, id);
	case DI_CHECKBOX:
		return gcnew FarCheckBox(this, id);
	case DI_COMBOBOX:
		return gcnew FarComboBox(this, id);
	case DI_DOUBLEBOX:
		return gcnew FarBox(this, id);
	case DI_EDIT:
		return gcnew FarEdit(this, id, di.Type);
	case DI_FIXEDIT:
		return gcnew FarEdit(this, id, di.Type);
	case DI_LISTBOX:
		return gcnew FarListBox(this, id);
	case DI_PSWEDIT:
		return gcnew FarEdit(this, id, di.Type);
	case DI_RADIOBUTTON:
		return gcnew FarRadioButton(this, id);
	case DI_SINGLEBOX:
		return gcnew FarBox(this, id);
	case DI_TEXT:
		return gcnew FarText(this, id);
	case DI_USERCONTROL:
		return gcnew FarUserControl(this, id);
	case DI_VTEXT:
		return gcnew FarText(this, id);
	default:
		return nullptr;
	}
}

IEnumerable<IControl^>^ FarDialog::Controls::get()
{
	return Works::DialogTools::GetControls(this);
}

void FarDialog::SetFocus(int id)
{
	if (_hDlg != INVALID_HANDLE_VALUE)
	{
		if (!Info.SendDlgMessage(_hDlg, DM_SETFOCUS, id, 0))
			throw gcnew InvalidOperationException("Cannot set focus.");
	}
	else
	{
		if (!_items)
			throw gcnew InvalidOperationException("Dialog has no items.");
		if (id < 0 || id >= _items->Count)
			throw gcnew ArgumentOutOfRangeException("id");
		_focused = _items[id];
	}
}

void FarDialog::Move(Point point, bool absolute)
{
	if (_hDlg == INVALID_HANDLE_VALUE)
		throw gcnew InvalidOperationException("Dialog is not started.");

	COORD arg = { (SHORT)point.X, (SHORT)point.Y };
	Info.SendDlgMessage(_hDlg, DM_MOVEDIALOG, absolute, &arg);
}

void FarDialog::Resize(Point size)
{
	if (_hDlg == INVALID_HANDLE_VALUE)
		throw gcnew InvalidOperationException("Dialog is not started.");

	COORD arg = { (SHORT)size.X, (SHORT)size.Y };
	Info.SendDlgMessage(_hDlg, DM_RESIZEDIALOG, 0, &arg);
}

INT_PTR FarDialog::DialogProc(intptr_t msg, intptr_t param1, void* param2)
{
	try
	{
		// message:
		switch(msg)
		{
		case DN_INITDIALOG:
			{
				// setup items
				for each(FarControl^ fc in _items)
					fc->Started();

				if (_Initialized)
				{
					InitializedEventArgs ea(param1 < 0 ? nullptr : _items[(int)param1]);
					_Initialized(this, %ea);
					_Initialized = nullptr;
					return !ea.Ignore;
				}
				break;
			}
		case DN_CLOSE:
			{
				FarControl^ fc = param1 >= 0 ? _items[(int)param1] : nullptr;
				if (_Closing)
				{
					ClosingEventArgs ea(fc);
					_Closing(this, %ea);
					if (ea.Ignore)
						return false;
				}
				break;
			}
		case DN_DRAWDLGITEM:
			{
				FarControl^ fc = _items[(int)param1];
				if (fc->_Drawing)
				{
					DrawingEventArgs ea(fc);
					fc->_Drawing(this, %ea);
					return !ea.Ignore;
				}
				return 1;
			}
		case DN_CTLCOLORDLGITEM:
			{
				FarControl^ fc = _items[(int)param1];
				if (fc->_Coloring)
				{
					ColoringEventArgs ea(fc);
					FarDialogItemColors& arg = *(FarDialogItemColors*)param2;

					ea.Foreground1 = ConsoleColor(arg.Colors[0].ForegroundColor);
					ea.Background1 = ConsoleColor(arg.Colors[0].BackgroundColor);
					ea.Foreground2 = ConsoleColor(arg.Colors[1].ForegroundColor);
					ea.Background2 = ConsoleColor(arg.Colors[1].BackgroundColor);
					ea.Foreground3 = ConsoleColor(arg.Colors[2].ForegroundColor);
					ea.Background3 = ConsoleColor(arg.Colors[2].BackgroundColor);
					ea.Foreground4 = ConsoleColor(arg.Colors[3].ForegroundColor);
					ea.Background4 = ConsoleColor(arg.Colors[3].BackgroundColor);

					fc->_Coloring(this, %ea);

					arg.Colors[0].ForegroundColor = COLORREF(ea.Foreground1);
					arg.Colors[0].BackgroundColor = COLORREF(ea.Background1);
					arg.Colors[1].ForegroundColor = COLORREF(ea.Foreground2);
					arg.Colors[1].BackgroundColor = COLORREF(ea.Background2);
					arg.Colors[2].ForegroundColor = COLORREF(ea.Foreground3);
					arg.Colors[2].BackgroundColor = COLORREF(ea.Background3);
					arg.Colors[3].ForegroundColor = COLORREF(ea.Foreground4);
					arg.Colors[3].BackgroundColor = COLORREF(ea.Background4);

					return 1;
				}
				break;
			}
		case DN_GOTFOCUS:
			{
				FarControl^ fc = _items[(int)param1];
				if (fc->_GotFocus)
				{
					AnyEventArgs ea(fc);
					fc->_GotFocus(this, %ea);
				}
				return 0;
			}
		case DN_KILLFOCUS:
			{
				FarControl^ fc = _items[(int)param1];
				if (fc->_LosingFocus)
				{
					LosingFocusEventArgs ea(fc);
					fc->_LosingFocus(this, %ea);
					if (ea.Focused)
						return ea.Focused->Id;
				}
				return -1;
			}
		case DN_BTNCLICK:
			{
				FarControl^ fc = _items[(int)param1];
				FarButton^ fb = dynamic_cast<FarButton^>(fc);
				if (fb)
				{
					if (fb->_ButtonClicked)
					{
						ButtonClickedEventArgs ea(fb, 0);
						fb->_ButtonClicked(this, %ea);
						return ea.Ignore;
					}
					break;
				}
				FarCheckBox^ cb = dynamic_cast<FarCheckBox^>(fc);
				if (cb)
				{
					if (cb->_ButtonClicked)
					{
						ButtonClickedEventArgs ea(cb, (int)param2);
						cb->_ButtonClicked(this, %ea);
						return !ea.Ignore;
					}
					break;
				}
				FarRadioButton^ rb = dynamic_cast<FarRadioButton^>(fc);
				if (rb)
				{
					if (rb->_ButtonClicked)
					{
						ButtonClickedEventArgs ea(rb, (int)param2);
						rb->_ButtonClicked(this, %ea);
						return !ea.Ignore;
					}
					break;
				}
				break;
			}
		case DN_EDITCHANGE:
			{
				FarControl^ fc = _items[(int)param1];
				FarEdit^ fe = dynamic_cast<FarEdit^>(fc);
				if (fe)
				{
					if (fe->_TextChanged)
					{
						FarDialogItem& item = *(FarDialogItem*)param2;
						TextChangedEventArgs ea(fe, gcnew String(item.Data));
						fe->_TextChanged(this, %ea);
						return !ea.Ignore;
					}
					break;
				}
				FarComboBox^ cb = dynamic_cast<FarComboBox^>(fc);
				if (cb)
				{
					if (cb->_TextChanged)
					{
						FarDialogItem& item = *(FarDialogItem*)param2;
						TextChangedEventArgs ea(cb, gcnew String(item.Data));
						cb->_TextChanged(this, %ea);
						return !ea.Ignore;
					}
					break;
				}
				break;
			}
		case DN_ENTERIDLE:
			{
				if (_Idled)
				{
					_Idled(this, nullptr);
				}
				break;
			}
		case DN_CONTROLINPUT:
			{
				FarControl^ fc = param1 >= 0 ? _items[(int)param1] : nullptr;
				INPUT_RECORD* ir = (INPUT_RECORD*)param2;
				
				if (MOUSE_EVENT == ir->EventType)
				{
					if (fc && fc->_MouseClicked || _MouseClicked)
					{
						//! get args once: if both handler work then for the second this memory may be garbage
						MouseClickedEventArgs ea(fc, GetMouseInfo(ir->Event.MouseEvent));
						if (fc && fc->_MouseClicked)
						{
							fc->_MouseClicked(this, %ea);
							if (ea.Ignore)
								return true;
						}
						if (_MouseClicked)
						{
							//! translate user control coordinates to standard
							if (fc && dynamic_cast<FarUserControl^>(fc) != nullptr)
							{
								Point pt1 = Rect.First;
								Point pt2 = fc->Rect.First;
								Point pt3 = ea.Mouse->Where;
								ea.Mouse = gcnew MouseInfo(
									Point(pt1.X + pt2.X + pt3.X, pt1.Y + pt2.Y + pt3.Y),
									ea.Mouse->Action, ea.Mouse->Buttons, ea.Mouse->ControlKeyState, ea.Mouse->Value);
							}
							_MouseClicked(this, %ea);
							if (ea.Ignore)
								return true;
						}
					}
				}
				else if (KEY_EVENT == ir->EventType)
				{
					if (fc && fc->_KeyPressed)
					{
						KeyPressedEventArgs ea(fc, KeyInfoFromInputRecord(*ir));
						fc->_KeyPressed(this, %ea);
						if (ea.Ignore)
							return true;
					}
					if (_KeyPressed)
					{
						KeyPressedEventArgs ea(fc, KeyInfoFromInputRecord(*ir));
						_KeyPressed(this, %ea);
						if (ea.Ignore)
							return true;
					}
				}
				break;
			}
		case DN_RESIZECONSOLE:
			{
				if (_ConsoleSizeChanged)
				{
					AutoStopDialogRedraw autoStopDialogRedraw(_hDlg);

					SizeEventArgs ea(nullptr, Point(((COORD*)param2)->X, ((COORD*)param2)->Y));
					_ConsoleSizeChanged(this, %ea);

					return true;
				}
				break;
			}
		}
	}
	catch(Exception^ e)
	{
		Far::Net->ShowError("Error in " __FUNCTION__, e);
	}

	// default
	return Info.DefDlgProc(_hDlg, msg, param1, param2);
}

}
