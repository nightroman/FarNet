/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "Message.h"

namespace FarNet
{;
bool Message::Show()
{
	//! flags: add OK if no buttons; otherwise wierd: [Esc] has no effect, [Enter] passed through, other keys too and dialog is still shown
	int flags = _flags;
	if ((!_buttons || _buttons->Length == 0) && (flags & (FMSG_MB_OK|FMSG_MB_OKCANCEL|FMSG_MB_ABORTRETRYIGNORE|FMSG_MB_YESNO|FMSG_MB_YESNOCANCEL|FMSG_MB_RETRYCANCEL)) == 0)
		flags |= FMSG_MB_OK;

	int nbItems;
	CStr* items = CreateBlock(nbItems);
	PIN_NS(pinHelpTopic, _helpTopic);
	_selected = Info.Message(0, flags, pinHelpTopic, (wchar_t**)items, nbItems, _buttons ? _buttons->Length : 0);
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

int Message::Show(String^ body, String^ header, MsgOptions options, array<String^>^ buttons, String^ helpTopic)
{
	// GUI on macro
	if (int(options & MsgOptions::GuiOnMacro) != 0)
	{
		// check macro
		if (Far::Net->MacroState != FarMacroState::None)
			options = options | MsgOptions::Gui;
	}

	// GUI
	if (int(options & MsgOptions::Gui) != 0)
	{
		if (buttons)
			throw gcnew ArgumentException("Custom buttons are not supported in GUI message boxes.");

		if (!Configuration::GetBool(Configuration::DisableGui))
			return ShowGui(body, header, options);
	}

	// object
	Message m;
	m._helpTopic = helpTopic;
	m._flags = (int)options;

	// text width
	int width = Console::WindowWidth - 16;

	// header
	if (!String::IsNullOrEmpty(header))
	{
		m._header = Regex::Replace(header, "[\t\r\n]+", " ");
		if (m._header->Length > width)
			m._header = m._header->Substring(0, width);
	}

	// body
	int height = Console::WindowHeight - 9;
	FormatMessageLines(%m._body, body, width, height);

	// buttons? dialog?
	if (buttons)
	{
		m._buttons = buttons;
		int len = 0;
		for each(String^ s in buttons)
		{
			len += s->Length + 2;
			if (len > width)
				return m.ShowDialog(width);
		}
	}

	// go
	m.Show();
	return m._selected;
}

int Message::ShowDialog(int width)
{
	int w = _header->Length;
	for each(String^ s in _body)
		if (s->Length > w)
			w = s->Length;
	for each(String^ s in _buttons)
	{
		if (s->Length > w)
		{
			w = s->Length;
			if (w > width)
			{
				w = width;
				break;
			}
		}
	}
	w += 10;
	int nBody = Math::Min(_body.Count, Console::WindowHeight / 3);
	int h = 5 + nBody + _buttons->Length;
	if (h > Console::WindowHeight - 4)
		h = Console::WindowHeight - 4;

	IDialog^ dialog = Far::Net->CreateDialog(-1, -1, w, h);
	dialog->HelpTopic = _helpTopic;
	dialog->IsWarning = (_flags & FMSG_WARNING);
	dialog->AddBox(3, 1, w - 4, h - 2, _header);
	for(int i = 0; i < nBody; ++i)
		dialog->AddText(5, -1, 0, _body[i]);
	dialog->AddText(5, -1, 0, nullptr)->Separator = 1;

	IListBox^ list = dialog->AddListBox(4, -1, w - 5, h - 6 - nBody, nullptr);
	list->NoAmpersands = true;
	list->NoBox = true;
	for each(String^ s in _buttons)
		list->Add(s);

	if (!dialog->Show())
		return -1;

	return list->Selected;
}

void Message::FormatMessageLines(List<String^>^ lines, String^ message, int width, int height)
{
	Regex^ format = nullptr;
	for each(String^ line in Regex::Split(message->Replace('\t', ' '), "\r\n|\r|\n"))
	{
		if (line->Length <= width)
		{
			lines->Add(line);
		}
		else
		{
			if (format == nullptr)
				format = gcnew Regex("(.{0," + width + "}(?:\\s|$))");
			for each (String^ split in format->Split(line))
			{
				if (split->Length > 0)
				{
					lines->Add(split);
					if (lines->Count >= height)
						return;
				}
			}
		}
		if (lines->Count >= height)
			return;
	}
}

int Message::ShowGui(String^ body, String^ header, MsgOptions options)
{
	PIN_ES(pinText, body);
	PIN_ES(pinCaption, header);

	UINT type = MB_SYSTEMMODAL;

	// buttons
	switch(UINT(options) & 0xFFFF0000)
	{
	case UINT(MsgOptions::OkCancel):
		type |= MB_OKCANCEL;
		break;
	case UINT(MsgOptions::AbortRetryIgnore):
		type |= MB_ABORTRETRYIGNORE;
		break;
	case UINT(MsgOptions::YesNo):
		type |= MB_YESNO;
		break;
	case UINT(MsgOptions::YesNoCancel):
		type |= MB_YESNOCANCEL;
		break;
	case UINT(MsgOptions::RetryCancel):
		type |= MB_RETRYCANCEL;
		break;
	default:
		type |= MB_OK;
		break;
	}

	// icon
	if (int(options & MsgOptions::Warning))
		type |= MB_ICONSTOP;
	else
		type |= MB_ICONEXCLAMATION;

	// show
	int res = ::MessageBox(0, pinText, pinCaption, type);

	// result
	switch(UINT(options) & 0xFFFF0000)
	{
	case UINT(MsgOptions::Ok):
		return res == IDOK ? 0 : -1;
	case UINT(MsgOptions::OkCancel):
		return res == IDOK ? 0 : res == IDCANCEL ? 1 : -1;
	case UINT(MsgOptions::AbortRetryIgnore):
		return res == IDABORT ? 0 : res == IDRETRY ? 1 : res == IDIGNORE ? 2 : -1;
	case UINT(MsgOptions::YesNo):
		return res == IDYES ? 0 : res == IDNO ? 1 : -1;
	case UINT(MsgOptions::YesNoCancel):
		return res == IDYES ? 0 : res == IDNO ? 1 : res == IDCANCEL ? 2 : -1;
	case UINT(MsgOptions::RetryCancel):
		return res == IDRETRY ? 0 : res == IDCANCEL ? 1 : -1;
	}
	return -1;
}

}
