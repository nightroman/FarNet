/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "CommandLine.h"
#include "Far.h"
#include "CommandLineSelection.h"

namespace FarNet
{;
ILine^ Far::CommandLine::get()
{
	return gcnew FarNet::CommandLine;
}

FarNet::WindowType CommandLine::WindowType::get()
{
	return FarNet::WindowType::Panels;
}

ILine^ CommandLine::FullLine::get()
{
	return this;
}

ILineSelection^ CommandLine::Selection::get()
{
	return gcnew CommandLineSelection;
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

int CommandLine::Pos::get()
{
	int pos;
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINEPOS, 0, (LONG_PTR)&pos))
		throw gcnew OperationCanceledException;
	return pos;
}

void CommandLine::Pos::set(int value)
{
	if (value < 0)
		value = Length;

	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_SETCMDLINEPOS, value, 0))
		throw gcnew OperationCanceledException;
}

void CommandLine::Insert(String^ text)
{
	if (!text)
		throw gcnew ArgumentNullException("text");

	PIN_NE(pin, text);
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_INSERTCMDLINE, 0, (LONG_PTR)pin))
		throw gcnew OperationCanceledException;
}

void CommandLine::Select(int start, int end)
{
	CmdLineSelect cls;
	cls.SelStart = start;
	cls.SelEnd = end;
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_SETCMDLINESELECTION, 0, (LONG_PTR)&cls))
		throw gcnew OperationCanceledException;
}

void CommandLine::Unselect()
{
	Select(-1, -1);
}

String^ CommandLine::ToString()
{
	return Text;
}
}
