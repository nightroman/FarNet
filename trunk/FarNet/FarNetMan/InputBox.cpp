
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

#include "StdAfx.h"
#include "InputBox.h"

namespace FarNet
{;
InputBox::InputBox()
: _maxLength(511)
{}

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
	if (ValueUserScreen::Get()) //_100514_000000
	{
		ValueUserScreen::Set(false);
		Far::Api->UI->SaveUserScreen();
	}

	PIN_ES(pinTitle, Title);
	PIN_ES(pinPrompt, Prompt);
	PIN_ES(pinText, Text);
	PIN_NS(pinHelp, HelpTopic);
	PIN_NS(pinHistory, History);
	CBox box(MaxLength + 1);

	if (!Info.InputBox(&MainGuid, &MainGuid, pinTitle, pinPrompt, pinHistory, pinText, box, box.Size(), pinHelp, Flags()))
		return false;

	Text = gcnew String(box);
	return true;
}

int InputBox::Flags()
{
	int r = 0;
	if (ButtonsAreVisible) r += FIB_BUTTONS;
	if (EmptyEnabled) r += FIB_ENABLEEMPTY;
	if (ExpandEnvironmentVariables) r += FIB_EXPANDENV;
	if (IsPath) r += FIB_EDITPATH;
	if (IsPassword) r += FIB_PASSWORD;
	if (!UseLastHistory) r += FIB_NOUSELASTHISTORY;
	return r;
}

}
