/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#include "StdAfx.h"
#include "InputBox.h"

namespace FarManagerImpl
{;
InputBox::InputBox()
{
	_maxLength = 511;
}

int InputBox::MaxLength::get()
{
	return _maxLength;
}

void InputBox::MaxLength::set(int value)
{
	if (value < 1)
		throw gcnew ArgumentOutOfRangeException("value");
	_maxLength = value;
}

bool InputBox::Show()
{
	CStr sTitle(Title);
	CStr sPrompt(Prompt);
	CStr sText(Text);
	CStr sDest(_maxLength + 1);
	CStr sHistory;
	if (!String::IsNullOrEmpty(History))
		sHistory.Set(History);

	// help
	const char* help; CStr sHelp;
	if (!String::IsNullOrEmpty(HelpTopic))
		sHelp.Set(HelpTopic), help = sHelp;
	if (_oemHelpTopic)
		help = _oemHelpTopic;
	else
		help = NULL;

	bool ok = Info.InputBox(sTitle, sPrompt, sHistory, sText, sDest, MaxLength, help, Flags()) != 0;
	if (ok)
		Text = OemToStr(sDest);
	return ok;
}

int InputBox::Flags()
{
	int r = 0;
	if (ButtonsAreVisible) r += FIB_BUTTONS;
	if (EmptyEnabled) r += FIB_ENABLEEMPTY;
	if (EnvExpanded) r += FIB_EXPANDENV;
	if (IsPassword) r += FIB_PASSWORD;
	if (NoLastHistory) r += FIB_NOUSELASTHISTORY;
	return r;
}
}
