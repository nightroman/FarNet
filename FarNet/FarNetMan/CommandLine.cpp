/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "CommandLine.h"

namespace FarNet
{;
FarNet::WindowKind CommandLine::WindowKind::get()
{
	return FarNet::WindowKind::Panels;
}

int CommandLine::Length::get()
{
	int size = Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINE, 0, 0);
	return size - 1;
}

String^ CommandLine::Text::get()
{
	int size = Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINE, 0, 0);
	CBox buf(size);
	Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINE, size, (LONG_PTR)(wchar_t*)buf);
	return gcnew String(buf);
}

void CommandLine::Text::set(String^ value)
{
	if (!value)
		throw gcnew ArgumentNullException("value");

	PIN_NE(pin, value);
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_SETCMDLINE, 0, (LONG_PTR)pin))
		throw gcnew OperationCanceledException;
}

int CommandLine::Caret::get()
{
	int pos;
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINEPOS, 0, (LONG_PTR)&pos))
		throw gcnew OperationCanceledException;
	return pos;
}

void CommandLine::Caret::set(int value)
{
	if (value < 0)
		value = Length;

	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_SETCMDLINEPOS, value, 0))
		throw gcnew OperationCanceledException;
}

void CommandLine::InsertText(String^ text)
{
	if (!text)
		throw gcnew ArgumentNullException("text");

	PIN_NE(pin, text);
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_INSERTCMDLINE, 0, (LONG_PTR)pin))
		throw gcnew OperationCanceledException;
}

void CommandLine::SelectText(int start, int end)
{
	CmdLineSelect cls;
	cls.SelStart = start;
	cls.SelEnd = end;
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_SETCMDLINESELECTION, 0, (LONG_PTR)&cls))
		throw gcnew OperationCanceledException;
}

void CommandLine::UnselectText()
{
	SelectText(-1, -1);
}

Span CommandLine::Selection::get()
{
	CmdLineSelect cls;
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINESELECTION, 0, (LONG_PTR)&cls))
		throw gcnew OperationCanceledException;

	Span result;
	if (cls.SelStart < 0)
	{
		result.Start = -1;
		result.End = -2;
	}
	else
	{
		result.Start = cls.SelStart;
		result.End = cls.SelEnd;
	}
	return result;
}

String^ CommandLine::SelectedText::get()
{
	CmdLineSelect cls;
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINESELECTION, 0, (LONG_PTR)&cls))
		throw gcnew OperationCanceledException;

	if (cls.SelStart < 0)
		return nullptr;

	int size = Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINE, 0, 0);
	CBox buf(size);
	Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINE, size, (LONG_PTR)(wchar_t*)buf);
	return gcnew String(buf, cls.SelStart, cls.SelEnd - cls.SelStart);
}

void CommandLine::SelectedText::set(String^ value)
{
	if (!value)
		throw gcnew ArgumentNullException("value");

	CmdLineSelect cls;
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINESELECTION, 0, (LONG_PTR)&cls))
		throw gcnew OperationCanceledException;
	if (cls.SelStart < 0)
		throw gcnew InvalidOperationException(Res::CannotSetSelectedText);

	// make new text
	String^ text = Far::Net->CommandLine->Text;
	String^ text1 = text->Substring(0, cls.SelStart);
	String^ text2 = text->Substring(cls.SelEnd);
	text = text1 + value + text2;

	// store cursor
	int pos = Caret;

	// set new text
	PIN_NE(pin, text);
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_SETCMDLINE, 0, (LONG_PTR)pin))
		throw gcnew OperationCanceledException;

	// set new selection
	cls.SelEnd = cls.SelStart + value->Length;
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_SETCMDLINESELECTION, 0, (LONG_PTR)&cls))
		throw gcnew OperationCanceledException;

	// restore cursor
	Caret = pos <= text->Length ? pos : text->Length;
}

}
