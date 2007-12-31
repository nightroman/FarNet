/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2007 FAR.NET Team
*/

#include "StdAfx.h"
#include "CommandLineSelection.h"

namespace FarNet
{;
CommandLineSelection::CommandLineSelection()
{
}

String^ CommandLineSelection::Text::get()
{
	char sCmd[1024];
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINESELECTEDTEXT, sCmd))
		throw gcnew OperationCanceledException();
	return OemToStr(sCmd);
}

void CommandLineSelection::Text::set(String^ value)
{
	CmdLineSelect cls;
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINESELECTION, &cls))
		throw gcnew OperationCanceledException();
	if (cls.SelStart < 0)
		throw gcnew InvalidOperationException("Can't set text: there is no selection.");
	String^ text;
	{
		char sb[1024];
		if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINE, sb))
			throw gcnew OperationCanceledException();
		text = OemToStr(sb);
	}
	String^ text1 = text->Substring(0, cls.SelStart);
	String^ text2 = text->Substring(cls.SelEnd);
	text = text1 + value + text2;
	CBox sb(text);
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_SETCMDLINE, sb))
		throw gcnew OperationCanceledException();
	cls.SelEnd = cls.SelStart + value->Length;
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_SETCMDLINESELECTION, &cls))
		throw gcnew OperationCanceledException();
}

int CommandLineSelection::End::get()
{
	CmdLineSelect cls;
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINESELECTION, &cls))
		throw gcnew OperationCanceledException();
	return cls.SelEnd;
}

int CommandLineSelection::Length::get()
{
	CmdLineSelect cls;
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINESELECTION, &cls))
		throw gcnew OperationCanceledException();
	return cls.SelStart >= 0 ? cls.SelEnd - cls.SelStart : 0;
}

int CommandLineSelection::Start::get()
{
	CmdLineSelect cls;
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINESELECTION, &cls))
		throw gcnew OperationCanceledException();
	return cls.SelStart;
}

String^ CommandLineSelection::ToString()
{
	return Text;
}
}
