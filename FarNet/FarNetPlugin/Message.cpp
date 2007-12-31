/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2007 FAR.NET Team
*/

#include "StdAfx.h"
#include "Message.h"

namespace FarNet
{;
Message::Message()
{
	_body = gcnew StringCollection();
	_buttons = gcnew StringCollection();
}

DEF_PROP_FLAG(Message, IsWarning, FMSG_WARNING);
DEF_PROP_FLAG(Message, IsError, FMSG_ERRORTYPE);
DEF_PROP_FLAG(Message, KeepBackground, FMSG_KEEPBACKGROUND);
DEF_PROP_FLAG(Message, LeftAligned, FMSG_LEFTALIGN);

MessageOptions Message::Options::get()
{
	return (MessageOptions)_flags;
}

void Message::Options::set(MessageOptions value)
{
	_flags = (int)value;
}

StringCollection^ Message::Body::get()
{
	return _body;
}

StringCollection^ Message::Buttons::get()
{
	return _buttons;
}

int Message::Selected::get()
{
	return _selected;
}

void Message::Selected::set(int value)
{
	_selected = value;
}

bool Message::Show()
{
	// flags: add OK if no buttons
	int flags = _flags;
	if (_buttons->Count == 0 && (flags & (FMSG_MB_OK|FMSG_MB_OKCANCEL|FMSG_MB_ABORTRETRYIGNORE|FMSG_MB_YESNO|FMSG_MB_YESNOCANCEL|FMSG_MB_RETRYCANCEL)) == 0)
		flags |= FMSG_MB_OK;

	CStr* items = CreateBlock();
	CBox sHelp; sHelp.Reset(HelpTopic);
	_selected = Info.Message(0, flags, sHelp, (char**)items, Amount(), _buttons->Count);
	delete[] items;
	return Selected != -1;
}

void Message::Reset()
{
	Header = nullptr;
	Selected = -1;
	_flags = 0;
	_buttons->Clear();
}

int Message::Amount()
{
	int a = 2;
	if (Body->Count != 0)
		a = 1 + Body->Count;
	return a + _buttons->Count;
}

CStr* Message::CreateBlock()
{
	CStr* r = new CStr[Amount()];
	int index = 0;
	r[index].Set(Header);
	++index;
	if (Body->Count == 0)
	{
		r[index].Set(String::Empty);
		++index;
	}
	else
	{
		Add(Body, r, index);
	}
	Add(_buttons, r, index);
	return r;
}

void Message::Add(StringCollection^ strings, CStr* result, int& index)
{
	for each(String^ s in strings)
	{
		result[index].Set(s);
		++index;
	}
}

int Message::Show(String^ body, String^ header, MessageOptions options, array<String^>^ buttons)
{
	// object
	Message m;
	m.Options = options;

	// text width
	int width = Console::WindowWidth - 16;

	// header
	if (!String::IsNullOrEmpty(header))
	{
		m.Header = Regex::Replace(header, "[\t\r\n]+", " ");
		if (m.Header->Length > width)
			m.Header = m.Header->Substring(0, width);
	}

	// body
	Regex^ format = nullptr;
	int height = Console::WindowHeight - 10;
	for each(String^ s1 in Regex::Split(body->Replace('\t', ' '), "\r\n|\r|\n"))
	{
		if (s1->Length <= width)
		{
			m.Body->Add(s1);
		}
		else
		{
			if (format == nullptr)
				format = gcnew Regex("(.{0," + width + "}(?:\\s|$))");
			for each (String^ s2 in format->Split(s1))
				if (s2->Length > 0)
					m.Body->Add(s2);
		}
		if (m.Body->Count >= height)
			break;
	}

	// buttons
	if (buttons != nullptr)
	{
		for each(String^ s in buttons)
			m.Buttons->Add(s);
	}

	// go
	m.Show();
	return m.Selected;
}
}
