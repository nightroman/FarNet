#include "StdAfx.h"
#include "Message.h"
#include "Utils.h"

namespace FarManagerImpl
{;
Message::Message()
{
	_body = gcnew StringCollection();
	_buttons = gcnew StringCollection();
	_header = String::Empty;
}

StringCollection^ Message::Body::get()
{
	return _body;
}

StringCollection^ Message::Buttons::get()
{
	return _buttons;
}

String^ Message::Header::get()
{
	return _header;
}

void Message::Header::set(String^ value)
{
	_header = value;
}

int Message::Selected::get()
{
	return _selected;
}

void Message::Selected::set(int value)
{
	_selected = value;
}

bool Message::IsWarning::get()
{
	return _isWarning;
}

void Message::IsWarning::set(bool value)
{
	_isWarning = value;
}

bool Message::IsError::get()
{
	return _isError;
}

void Message::IsError::set(bool value)
{
	_isError = value;
}

bool Message::KeepBackground::get()
{
	return _keepBackground;
}

void Message::KeepBackground::set(bool value)
{
	_keepBackground = value;
}

bool Message::LeftAligned::get()
{
	return _leftAligned;
}

void Message::LeftAligned::set(bool value)
{
	_leftAligned = value;
}

bool Message::Show()
{
	CStr* items = CreateBlock();
	_selected = Info.Message(0, Flags(), "", (char**)items, Amount(), Buttons->Count);
	delete[] items;
	return Selected != -1;
}

void Message::Reset()
{
	Header = "";
	Selected = 0;
	IsWarning = false;
	IsError = false;
	KeepBackground = false;
	LeftAligned = false;
	Buttons->Clear();
}

int Message::Amount()
{
	int a = 2;
	if (Body->Count != 0)
		a = 1+Body->Count;
	return a+Buttons->Count;
}

int Message::Flags()
{
	int Result = 0;
	if (IsWarning) Result += FMSG_WARNING;
	if (IsError) Result += FMSG_ERRORTYPE;
	if (KeepBackground) Result += FMSG_KEEPBACKGROUND;
	if (LeftAligned) Result += FMSG_LEFTALIGN;
	return Result;
}

CStr* Message::CreateBlock()
{
	CStr* r = new CStr[Amount()];
	int index = 0;
	r[index].Set(Header);
	index++;
	if (Body->Count == 0)
	{
		r[index].Set(String::Empty);
		index++;
	}
	else
	{
		Add(Body, r, index);
	}
	Add(Buttons, r, index);
	return r;
}

void Message::Add(StringCollection^ Coll, CStr* result, int& index)
{
	for(int i = 0; i<Coll->Count; i++)
	{
		result[index].Set(Coll[i]);
		index++;
	}
}
}
