/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2009 FAR.NET Team
*/

#include "StdAfx.h"
#include "Message.h"
#include "Dialog.h"

namespace FarNet
{;
bool Message::Show()
{
	// flags: add OK if no buttons
	int flags = _flags;
	if (_buttons.Count == 0 && (flags & (FMSG_MB_OK|FMSG_MB_OKCANCEL|FMSG_MB_ABORTRETRYIGNORE|FMSG_MB_YESNO|FMSG_MB_YESNOCANCEL|FMSG_MB_RETRYCANCEL)) == 0)
		flags |= FMSG_MB_OK;

	CStr* items = CreateBlock();
	CBox sHelp; sHelp.Reset(_helpTopic);
	_selected = Info.Message(0, flags, sHelp, (char**)items, Amount(), _buttons.Count);
	delete[] items;

	return _selected != -1;
}

int Message::Amount()
{
	int a = 2;
	if (_body.Count != 0)
		a = 1 + _body.Count;
	return a + _buttons.Count;
}

CStr* Message::CreateBlock()
{
	CStr* r = new CStr[Amount()];
	int index = 0;
	r[index].Set(_header);
	++index;
	if (_body.Count == 0)
	{
		r[index].Set(String::Empty);
		++index;
	}
	else
	{
		Add(%_body, r, index);
	}
	Add(%_buttons, r, index);
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

int Message::Show(String^ body, String^ header, MessageOptions options, array<String^>^ buttons, String^ helpTopic)
{
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
	if (buttons != nullptr)
	{
		int len = 0;
		for each(String^ s in buttons)
		{
			len += s->Length + 2;
			if (len > width)
				return ShowDialog(%m, buttons, width);
		}
		m._buttons.AddRange(buttons);
	}

	// go
	m.Show();
	return m._selected;
}

int Message::ShowDialog(Message^ m, array<String^>^ buttons, int width)
{
	int w = m->_header->Length;
	for each(String^ s in m->_body)
		if (s->Length > w)
			w = s->Length;
	for each(String^ s in buttons)
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
	int nBody = min(m->_body.Count, Console::WindowHeight / 3);
	int h = 5 + nBody + buttons->Length;
	if (h > Console::WindowHeight - 4)
		h = Console::WindowHeight - 4;

	FarDialog dialog(-1, -1, w, h);
	dialog.HelpTopic = m->_helpTopic;
	dialog.IsWarning = (m->_flags & FMSG_WARNING);
	dialog.AddBox(3, 1, w - 4, h - 2, m->_header);
	for(int i = 0; i < nBody; ++i)
		dialog.AddText(5, -1, 0, m->_body[i]);
	dialog.AddText(5, -1, 0, nullptr)->Separator = 1;

	IListBox^ list = dialog.AddListBox(4, -1, w - 5, h - 6 - nBody, nullptr);
	list->NoAmpersands = true;
	list->NoBox = true;
	for each(String^ s in buttons)
		list->Add(s);

	if (!dialog.Show())
		return -1;

	return list->Selected;
}

void Message::FormatMessageLines(StringCollection^ lines, String^ message, int width, int height)
{
	Regex^ format = nullptr;
	for each(String^ s1 in Regex::Split(message->Replace('\t', ' '), "\r\n|\r|\n"))
	{
		if (s1->Length <= width)
		{
			lines->Add(s1);
		}
		else
		{
			if (format == nullptr)
				format = gcnew Regex("(.{0," + width + "}(?:\\s|$))");
			for each (String^ s2 in format->Split(s1))
			{
				if (s2->Length > 0)
				{
					lines->Add(s2);
					if (lines->Count >= height)
						return;
				}
			}
		}
		if (lines->Count >= height)
			return;
	}
}

}
