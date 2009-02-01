/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#include "StdAfx.h"
#include "CommandLine.h"
#include "CommandLineSelection.h"

namespace FarNet
{;
FarCommandLine::FarCommandLine()
{
}

ILine^ FarCommandLine::FullLine::get()
{
	return this;
}

ILineSelection^ FarCommandLine::Selection::get()
{
	return gcnew CommandLineSelection;
}

int FarCommandLine::Length::get()
{
	int size = Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINE, 0, 0);
	return size - 1;
}

int FarCommandLine::No::get()
{
	return -1;
}

String^ FarCommandLine::Eol::get()
{
	return String::Empty;
}

void FarCommandLine::Eol::set(String^)
{
}

String^ FarCommandLine::Text::get()
{
	//??? tweak size and empty
	int size = Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINE, 0, 0);
	CBox buf(size);
	Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINE, size, (LONG_PTR)(wchar_t*)buf);
	return gcnew String(buf);
}

void FarCommandLine::Text::set(String^ value)
{
	CBox sb(value);
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_SETCMDLINE, 0, (LONG_PTR)(wchar_t*)sb))
		throw gcnew OperationCanceledException;
}

int FarCommandLine::Pos::get()
{
	int pos;
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINEPOS, 0, (LONG_PTR)&pos))
		throw gcnew OperationCanceledException;
	return pos;
}

void FarCommandLine::Pos::set(int value)
{
	if (value < 0)
		value = Length;

	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_SETCMDLINEPOS, value, 0))
		throw gcnew OperationCanceledException;
}

void FarCommandLine::Insert(String^ text)
{
	CBox sText(text);
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_INSERTCMDLINE, 0, (LONG_PTR)(wchar_t*)sText))
		throw gcnew OperationCanceledException;
}

void FarCommandLine::Select(int start, int end)
{
	CmdLineSelect cls;
	cls.SelStart = start;
	cls.SelEnd = end;
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_SETCMDLINESELECTION, 0, (LONG_PTR)&cls))
		throw gcnew OperationCanceledException;
}

void FarCommandLine::Unselect()
{
	Select(-1, -1);
}

String^ FarCommandLine::ToString()
{
	return Text;
}
}
