/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#include "StdAfx.h"
#include "InputBox.h"

namespace FarNet
{;
InputBox::InputBox()
: _maxLength(511)
{
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

String^ InputBox::HelpTopic::get()
{
	return _HelpTopic;
}

void InputBox::HelpTopic::set(String^ value)
{
	if (SS(value) && !value->StartsWith("<"))
		throw gcnew ArgumentException("'value' format must be '<FullPath\\>Topic'");

	_HelpTopic = value;
}

bool InputBox::Show()
{
	PIN_ES(pinTitle, Title);
	PIN_ES(pinPrompt, Prompt);
	PIN_ES(pinText, Text);
	PIN_NS(pinHelp, HelpTopic);
	PIN_NS(pinHistory, History);
	CBox sDest(_maxLength);

	if (!Info.InputBox(pinTitle, pinPrompt, pinHistory, pinText, sDest, MaxLength, pinHelp, Flags()))
		return false;

	Text = gcnew String(sDest);
	return true;
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
