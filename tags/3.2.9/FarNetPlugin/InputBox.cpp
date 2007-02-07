#include "StdAfx.h"
#include "InputBox.h"
#include "Utils.h"

namespace FarManagerImpl
{;
InputBox::InputBox()
{
	_text = String::Empty;
	_title = "Input";
	_prompt = String::Empty;
	_history = String::Empty;
	_useLastHistory = true;
	_maxLength = 100;
}

String^ InputBox::Text::get()
{
	return _text;
}

void InputBox::Text::set(String^ value)
{
	_text = value;
}

String^ InputBox::Title::get()
{
	return _title;
}

void InputBox::Title::set(String^ value)
{
	_title = value;
}

String^ InputBox::Prompt::get()
{
	return _prompt;
}

void InputBox::Prompt::set(String^ value)
{
	_prompt = value;
}

String^ InputBox::History::get()
{
	return _history;
}

void InputBox::History::set(String^ value)
{
	_history = value;
}

int InputBox::MaxLength::get()
{
	return _maxLength;
}

void InputBox::MaxLength::set(int value)
{
	_maxLength = value;
}

bool InputBox::EmptyEnabled::get()
{
	return _emptyEnabled;
}

void InputBox::EmptyEnabled::set(bool value)
{
	_emptyEnabled = value;
}

bool InputBox::IsPassword::get()
{
	return _isPassword;
}

void InputBox::IsPassword::set(bool value)
{
	_isPassword = value;
}

bool InputBox::EnvExpanded::get()
{
	return _envExpanded;
}

void InputBox::EnvExpanded::set(bool value)
{
	_envExpanded = value;
}

bool InputBox::UseLastHistory::get()
{
	return _useLastHistory;
}

void InputBox::UseLastHistory::set(bool value)
{
	_useLastHistory = value;
}

bool InputBox::ButtonsAreVisible::get()
{
	return _buttonsAreVisible;
}

void InputBox::ButtonsAreVisible::set(bool value)
{
	_buttonsAreVisible = value;
}

bool InputBox::Show()
{
	return Show(nullptr, nullptr, nullptr, nullptr);
}

bool InputBox::Show(String^ prompt, String^ text, String^ title, String^ history)
{
	if (prompt != nullptr)
		_prompt = prompt;
	if (text != nullptr)
		_text = text;
	if (title != nullptr)
		_title = title;
	if (history != nullptr)
		_history = history;
	char* dest = new char[_maxLength+1];
	CStr pc_title(_title); CStr pc_prompt(_prompt); CStr pc_history(_history); CStr pc_text(_text);
	bool ok = Info.InputBox(pc_title, pc_prompt, pc_history, pc_text, dest, MaxLength, "", Flags()) != 0;
	if (ok)
		Text = OemToStr(dest);
	delete dest;
	return ok;
}

int InputBox::Flags()
{
	int r = 0;
	if (_emptyEnabled)
		r += FIB_ENABLEEMPTY;
	if (_isPassword)
		r += FIB_PASSWORD;
	if (_envExpanded)
		r += FIB_EXPANDENV;
	if (!_useLastHistory)
		r += FIB_NOUSELASTHISTORY;
	if (_buttonsAreVisible)
		r += FIB_BUTTONS;
	return r;
}
}
