#include "stdafx.h"
#include "Message.h"

namespace FarNet
{
const int ALL_BUTTONS = FMSG_MB_OK | FMSG_MB_OKCANCEL | FMSG_MB_ABORTRETRYIGNORE | FMSG_MB_YESNO | FMSG_MB_YESNOCANCEL | FMSG_MB_RETRYCANCEL;

bool Message::Show()
{
	//! invert FMSG_LEFTALIGN
	int flags = _flags ^ FMSG_LEFTALIGN;

	// process Draw flag
	if ((flags & (int)MessageOptions::Draw) == 0)
	{
		// add at least one button
		if ((flags & ALL_BUTTONS) == 0 && (!_buttons || _buttons->Length == 0))
			flags |= FMSG_MB_OK;
	}

	auto items = CreateBlock();
	GUID typeId = ToGUID(_args->TypeId);

	PIN_NS(pinHelpTopic, _args->HelpTopic);
	_selected = (int)Info.Message(
		&MainGuid,
		&typeId,
		flags,
		pinHelpTopic,
		(wchar_t**)items.data(),
		items.size(),
		_buttons ? _buttons->Length : 0);

	return _selected != -1;
}

std::vector<CStr> Message::CreateBlock()
{
	std::vector<CStr> items((_buttons ? _buttons->Length : 0) + (_body.Count == 0 ? 2 : 1 + _body.Count));

	items[0].Set(_header);
	int index = 1;
	if (_body.Count == 0)
	{
		items[index].Set(String::Empty);
		++index;
	}
	else
	{
		for each (String ^ s in _body)
		{
			items[index].Set(s);
			++index;
		}
	}

	if (_buttons)
	{
		for each (String ^ s in _buttons)
		{
			items[index].Set(s);
			++index;
		}
	}

	return items;
}

int Message::Show(MessageArgs^ args)
{
	if (!args || !args->Text)
		throw gcnew ArgumentException("Null args or Text.");

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

		auto disableGui = Environment::GetEnvironmentVariable("FarNet:DisableGui");
		if (!disableGui)
			return ShowGui(args->Text, args->Caption, options);
	}

	// standard message box
	Message m;
	m._args = args;
	m._flags = (int)options;

	// text width
	int maxTextWidth = Far::Api->UI->WindowSize.X - 16;

	// header
	if (!String::IsNullOrEmpty(args->Caption))
	{
		m._header = Works::Regexes::TabsAndNewLines()->Replace(args->Caption, " ");
		if (m._header->Length > maxTextWidth)
			m._header = m._header->Substring(0, maxTextWidth);
	}

	// body
	auto text = args->Text->TrimEnd();
	int height = Far::Api->UI->WindowSize.Y - 9;
	FarNet::Works::Kit::FormatMessage(% m._body, text, maxTextWidth, height, FarNet::Works::FormatMessageMode::Space);

	// buttons?
	bool needButtonList = false;
	if (args->Buttons)
	{
		m._buttons = args->Buttons;
		m._buttonLineLength = GetButtonLineLength(args->Buttons);
		needButtonList = m._buttonLineLength > maxTextWidth;
	}

	// dialog box?
	if (m._args->Position.HasValue || needButtonList)
		return m.ShowDialog(maxTextWidth, needButtonList);

	// message box
	m.Show();
	return m._selected;
}

// Button list ~ button line: "B1", "B2", ... ~ "[ B1 ] [ B2 ] ..."
int Message::GetButtonLineLength(array<String^>^ buttons)
{
	int len = 0;
	for each (String ^ s in buttons)
		len += s->Length + 4;
	return len + buttons->Length - 1;
}

int Message::ShowDialog(int maxTextWidth, bool needButtonList)
{
	if (!_header)
		_header = String::Empty;

	// dialog width
	int w = _header->Length;

	// text lines
	for each (String ^ s in _body)
		if (s->Length > w)
			w = s->Length;

	// each button line
	if (needButtonList)
	{
		// extra for possible vertical scroll, to avoid >>
		const int extra = 1;
		for each (String ^ s in _buttons)
		{
			if (s->Length + extra > w)
			{
				w = s->Length + extra;
				if (w > maxTextWidth)
				{
					w = maxTextWidth;
					break;
				}
			}
		}
	}
	// joined button line
	else if (w < _buttonLineLength)
	{
		w = _buttonLineLength;
	}

	// 10 = 2 * (3 dialog<->box + 1 box + 1 box<->button)
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
		h = nBody + (_buttons ? 6 : 4);
	}

	// dialog place
	Point position = _args->Position.HasValue ? _args->Position.Value : Point(-1, -1);
	int x1 = position.X;
	if (x1 >= size.X)
		x1 = size.X - w - 1;
	int y1 = position.Y;
	if (y1 >= size.Y)
		y1 = size.Y - h - 1;
	int x2 = x1 < 0 ? w : x1 + w - 1;
	int y2 = y1 < 0 ? h : y1 + h - 1;

	// dialog
	auto dialog = Far::Api->CreateDialog(x1, y1, x2, y2);
	dialog->TypeId = _args->TypeId;
	dialog->HelpTopic = _args->HelpTopic;
	dialog->IsWarning = (_flags & FMSG_WARNING);
	dialog->AddBox(3, 1, w - 4, h - 2, _header);

	// text
	for (int i = 0; i < nBody; ++i)
		dialog->AddText(5, -1, 0, _body[i])->ShowAmpersand = true;

	// case: no buttons
	if (!_buttons)
	{
		dialog->Show();
		return -1;
	}

	// separator
	dialog->AddText(5, -1, 0, nullptr)->Separator = 1;

	// case: button list
	if (needButtonList)
	{
		IListBox^ list = dialog->AddListBox(4, -1, w - 5, h - 6 - nBody, nullptr);
		list->NoAmpersands = true;
		list->NoBox = true;
		for each (String ^ s in _buttons)
			list->Add(s);

		if (!dialog->Show())
			return -1;

		return list->Selected;
	}

	// else: normal buttons

	List<IControl^> buttons(_buttons->Length);
	for each (String ^ s in _buttons)
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
	switch (UINT(options) & 0xFFFF0000)
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
	switch (UINT(options) & 0xFFFF0000)
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
