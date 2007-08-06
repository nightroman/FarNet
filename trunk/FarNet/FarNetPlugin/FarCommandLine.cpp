/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#include "StdAfx.h"
#include "FarCommandLine.h"
#include "CommandLineSelection.h"

namespace FarManagerImpl
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
	return gcnew CommandLineSelection();
}

int FarCommandLine::Length::get()
{
	char sb[1024];
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINE, sb))
		throw gcnew OperationCanceledException();
	return (int)strlen(sb);
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
	char sb[1024];
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINE, sb))
		throw gcnew OperationCanceledException();
	return OemToStr(sb);
}

void FarCommandLine::Text::set(String^ value)
{
	CStr sb(value);
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_SETCMDLINE, sb))
		throw gcnew OperationCanceledException();
}

int FarCommandLine::Pos::get()
{
	int pos;
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINEPOS, &pos))
		throw gcnew OperationCanceledException();
	return pos;
}

void FarCommandLine::Pos::set(int value)
{
	if (value < 0)
	{
		char sb[1024];
		if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINE, sb))
			throw gcnew OperationCanceledException();
		value = (int)strlen(sb);
	}
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_SETCMDLINEPOS, &value))
		throw gcnew OperationCanceledException();
}

void FarCommandLine::Insert(String^ text)
{
	CStr sText(text);
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_INSERTCMDLINE, sText))
		throw gcnew OperationCanceledException();
}

void FarCommandLine::Select(int start, int end)
{
	CmdLineSelect cls;
	cls.SelStart = start;
	cls.SelEnd = end;
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_SETCMDLINESELECTION, &cls))
		throw gcnew OperationCanceledException();
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
