
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

#include "StdAfx.h"
#include "Message.h"

namespace FarNet
{;
const int ALL_BUTTONS = FMSG_MB_OK|FMSG_MB_OKCANCEL|FMSG_MB_ABORTRETRYIGNORE|FMSG_MB_YESNO|FMSG_MB_YESNOCANCEL|FMSG_MB_RETRYCANCEL;

bool Message::Show()
{
	if (ValueUserScreen::Get()) //_100514_000000
	{
		ValueUserScreen::Set(false);
		Far::Api->UI->SaveUserScreen();
	}

	// process the draw flag
	int flags = _flags;
	if ((flags & (int)MessageOptions::Draw) == 0)
	{
		// add at least one button
		if ((flags & ALL_BUTTONS) == 0 && (!_buttons || _buttons->Length == 0))
			flags |= FMSG_MB_OK;
	}

	int nbItems;
	CStr* items = CreateBlock(nbItems);
	PIN_NS(pinHelpTopic, _helpTopic);
	_selected = (int)Info.Message(
		&MainGuid,
		nullptr,
		flags,
		pinHelpTopic,
		(wchar_t**)items,
		nbItems,
		_buttons ? _buttons->Length : 0);
	delete[] items;

	return _selected != -1;
}

CStr* Message::CreateBlock(int& outNbItems)
{
	outNbItems = (_buttons ? _buttons->Length : 0) + (_body.Count == 0 ? 2 : 1 + _body.Count);
	CStr* r = new CStr[outNbItems];

	r[0].Set(_header);
	int index = 1;
	if (_body.Count == 0)
	{
		r[index].Set(String::Empty);
		++index;
	}
	else
	{
		for each(String^ s in _body)
		{
			r[index].Set(s);
			++index;
		}
	}

	if (_buttons)
	{
		for each(String^ s in _buttons)
		{
			r[index].Set(s);
			++index;
		}
	}

	return r;
}

int Message::Show(MessageArgs^ args)
{
	if (!args) throw gcnew ArgumentNullException("args");
	
	// to change
	MessageOptions options = args->Options;
	
	// Draw?
	if (int(options & MessageOptions::Draw))
	{
		if (int(options & (MessageOptions::Gui | MessageOptions::GuiOnMacro)))
			throw gcnew ArgumentException("Draw and GUI options cannot be used together.");
		if ((int(options) & ALL_BUTTONS) || (args->Buttons && args->Buttons->Length))
			throw gcnew ArgumentException("Buttons cannot be used in drawn messages.");
	}

	// GUI on macro?
	if (int(options & MessageOptions::GuiOnMacro))
	{
		if (Far::Api->MacroState != MacroState::None)
			options = options | MessageOptions::Gui;
	}

	// case: GUI
	if (int(options & MessageOptions::Gui))
	{
		if (args->Buttons)
			throw gcnew ArgumentException("Custom buttons cannot be used in GUI messages.");

		if (!Configuration::GetBool(Configuration::DisableGui))
			return ShowGui(args->Text, args->Caption, options);
	}

	// standard message box
	Message m;
	m._flags = (int)options;
	m._helpTopic = args->HelpTopic;
	m._position = args->Position;

	// text width
	int maxTextWidth = Far::Api->UI->WindowSize.X - 16;

	// header
	if (!String::IsNullOrEmpty(args->Caption))
	{
		m._header = Regex::Replace(args->Caption, "[\t\r\n]+", " ");
		if (m._header->Length > maxTextWidth)
			m._header = m._header->Substring(0, maxTextWidth);
	}

	// body
	int height = Far::Api->UI->WindowSize.Y - 9;
	FarNet::Works::Kit::FormatMessage(%m._body, args->Text, maxTextWidth, height, FarNet::Works::FormatMessageMode::Word);

	// buttons? dialog?
	if (args->Buttons)
	{
		m._buttons = args->Buttons;
		bool needButtonList = NeedButtonList(args->Buttons, maxTextWidth);
		
		if (m._position.HasValue || needButtonList)
			return m.ShowDialog(maxTextWidth, needButtonList);
	}

	// go
	m.Show();
	return m._selected;
}

bool Message::NeedButtonList(array<String^>^ buttons, int width)
{
	int len = 0;
	for each(String^ s in buttons)
	{
		len += s->Length + 2;
		if (len > width)
			return true;
	}
	return false;
}

int Message::ShowDialog(int maxTextWidth, bool needButtonList)
{
	// dialog width
	int w = _header->Length;
	for each(String^ s in _body)
		if (s->Length > w)
			w = s->Length;
	for each(String^ s in _buttons)
	{
		if (s->Length > w)
		{
			w = s->Length;
			if (w > maxTextWidth)
			{
				w = maxTextWidth;
				break;
			}
		}
	}
	w += 10;

	// dialog height
	Point size = Far::Api->UI->WindowSize;
	int nBody = Math::Min(_body.Count, size.Y / 3);
	int h;
	if (needButtonList)
	{
		h = 5 + nBody + _buttons->Length;
		if (h > size.Y - 4)
			h = size.Y - 4;
	}
	else
	{
		h = 6 + nBody;
	}

	// dialog place
	Point position = _position.HasValue ? _position.Value : Point(-1, -1);
	int x1 = position.X;
	if (x1 >= size.X)
		x1 = size.X - w - 1;
	int y1 = position.Y;
	if (y1 >= size.Y)
		y1 = size.Y - h - 1;
	int x2 = x1 < 0 ? w : x1 + w - 1;
	int y2 = y1 < 0 ? h : y1 + h - 1;

	// dialog
	IDialog^ dialog = Far::Api->CreateDialog(x1, y1, x2, y2);
	dialog->HelpTopic = _helpTopic;
	dialog->IsWarning = (_flags & FMSG_WARNING);
	dialog->AddBox(3, 1, w - 4, h - 2, _header);
	
	// text
	for(int i = 0; i < nBody; ++i)
		dialog->AddText(5, -1, 0, _body[i]);

	// separator
	dialog->AddText(5, -1, 0, nullptr)->Separator = 1;

	// case: button list
	if (needButtonList)
	{
		IListBox^ list = dialog->AddListBox(4, -1, w - 5, h - 6 - nBody, nullptr);
		list->NoAmpersands = true;
		list->NoBox = true;
		for each(String^ s in _buttons)
			list->Add(s);

		if (!dialog->Show())
			return -1;

		return list->Selected;
	}

	// else: normal buttons

	List<IControl^> buttons(_buttons->Length);
	for each(String^ s in _buttons)
	{
		IButton^ button = dialog->AddButton(0, (buttons.Count ? 0 : -1), s);
		button->CenterGroup = true;
		buttons.Add(button);
	}

	if (!dialog->Show())
		return -1;

	return buttons.IndexOf(dialog->Selected);
}

int Message::ShowGui(String^ body, String^ header, MessageOptions options)
{
	PIN_ES(pinText, body);
	PIN_ES(pinCaption, header);

	UINT type = MB_SYSTEMMODAL;

	// buttons
	switch(UINT(options) & 0xFFFF0000)
	{
	case UINT(MessageOptions::OkCancel):
		type |= MB_OKCANCEL;
		break;
	case UINT(MessageOptions::AbortRetryIgnore):
		type |= MB_ABORTRETRYIGNORE;
		break;
	case UINT(MessageOptions::YesNo):
		type |= MB_YESNO;
		break;
	case UINT(MessageOptions::YesNoCancel):
		type |= MB_YESNOCANCEL;
		break;
	case UINT(MessageOptions::RetryCancel):
		type |= MB_RETRYCANCEL;
		break;
	default:
		type |= MB_OK;
		break;
	}

	// icon
	if (int(options & MessageOptions::Warning))
		type |= MB_ICONSTOP;
	else
		type |= MB_ICONEXCLAMATION;

	// show
	int res = ::MessageBox(0, pinText, pinCaption, type);

	// result
	switch(UINT(options) & 0xFFFF0000)
	{
	case UINT(MessageOptions::Ok):
		return res == IDOK ? 0 : -1;
	case UINT(MessageOptions::OkCancel):
		return res == IDOK ? 0 : res == IDCANCEL ? 1 : -1;
	case UINT(MessageOptions::AbortRetryIgnore):
		return res == IDABORT ? 0 : res == IDRETRY ? 1 : res == IDIGNORE ? 2 : -1;
	case UINT(MessageOptions::YesNo):
		return res == IDYES ? 0 : res == IDNO ? 1 : -1;
	case UINT(MessageOptions::YesNoCancel):
		return res == IDYES ? 0 : res == IDNO ? 1 : res == IDCANCEL ? 2 : -1;
	case UINT(MessageOptions::RetryCancel):
		return res == IDRETRY ? 0 : res == IDCANCEL ? 1 : -1;
	}
	return -1;
}

}
