
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
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
	int size = (int)Info.PanelControl(INVALID_HANDLE_VALUE, FCTL_GETCMDLINE, 0, 0);
	return size - 1;
}

String^ CommandLine::Text::get()
{
	CBox box;
	while(box(Info.PanelControl(INVALID_HANDLE_VALUE, FCTL_GETCMDLINE, box.Size(), box))) {}
	
	return gcnew String(box);
}

void CommandLine::Text::set(String^ value)
{
	if (!value)
		throw gcnew ArgumentNullException("value");

	PIN_NE(pin, value);
	if (!Info.PanelControl(INVALID_HANDLE_VALUE, FCTL_SETCMDLINE, 0, (wchar_t*)pin))
		throw gcnew InvalidOperationException(__FUNCTION__);
}

int CommandLine::Caret::get()
{
	int pos;
	if (!Info.PanelControl(INVALID_HANDLE_VALUE, FCTL_GETCMDLINEPOS, 0, &pos))
		throw gcnew InvalidOperationException(__FUNCTION__);
	return pos;
}

void CommandLine::Caret::set(int value)
{
	if (value < 0)
		value = Length;

	if (!Info.PanelControl(INVALID_HANDLE_VALUE, FCTL_SETCMDLINEPOS, value, 0))
		throw gcnew InvalidOperationException(__FUNCTION__);
}

void CommandLine::InsertText(String^ text)
{
	if (!text)
		throw gcnew ArgumentNullException("text");

	PIN_NE(pin, text);
	if (!Info.PanelControl(INVALID_HANDLE_VALUE, FCTL_INSERTCMDLINE, 0, (wchar_t*)pin))
		throw gcnew InvalidOperationException(__FUNCTION__);
}

void CommandLine::SelectText(int start, int end)
{
	CmdLineSelect cls = {sizeof(cls)};
	cls.SelStart = start;
	cls.SelEnd = end;
	if (!Info.PanelControl(INVALID_HANDLE_VALUE, FCTL_SETCMDLINESELECTION, 0, &cls))
		throw gcnew InvalidOperationException(__FUNCTION__);
}

void CommandLine::UnselectText()
{
	SelectText(-1, -1);
}

Span CommandLine::SelectionSpan::get()
{
	CmdLineSelect cls = {sizeof(cls)};
	if (!Info.PanelControl(INVALID_HANDLE_VALUE, FCTL_GETCMDLINESELECTION, 0, &cls))
		throw gcnew InvalidOperationException(__FUNCTION__);

	Span result;
	if (cls.SelStart < 0)
	{
		result.Start = -1;
		result.End = -2;
	}
	else
	{
		result.Start = (int)cls.SelStart;
		result.End = (int)cls.SelEnd;
	}
	return result;
}

String^ CommandLine::SelectedText::get()
{
	CmdLineSelect cls = {sizeof(cls)};
	if (!Info.PanelControl(INVALID_HANDLE_VALUE, FCTL_GETCMDLINESELECTION, 0, &cls))
		throw gcnew InvalidOperationException(__FUNCTION__);

	if (cls.SelStart < 0)
		return nullptr;

	CBox box;
	while(box(Info.PanelControl(INVALID_HANDLE_VALUE, FCTL_GETCMDLINE, box.Size(), box))) {}
	
	return gcnew String(box, (int)cls.SelStart, (int)(cls.SelEnd - cls.SelStart));
}

void CommandLine::SelectedText::set(String^ value)
{
	if (!value)
		throw gcnew ArgumentNullException("value");

	CmdLineSelect cls = {sizeof(cls)};
	if (!Info.PanelControl(INVALID_HANDLE_VALUE, FCTL_GETCMDLINESELECTION, 0, &cls))
		throw gcnew InvalidOperationException(__FUNCTION__);
	if (cls.SelStart < 0)
		throw gcnew InvalidOperationException(Res::CannotSetSelectedText);

	// make new text
	String^ text = Far::Net->CommandLine->Text;
	String^ text1 = text->Substring(0, (int)cls.SelStart);
	String^ text2 = text->Substring((int)cls.SelEnd);
	text = text1 + value + text2;

	// store cursor
	int pos = Caret;

	// set new text
	PIN_NE(pin, text);
	if (!Info.PanelControl(INVALID_HANDLE_VALUE, FCTL_SETCMDLINE, 0, (wchar_t*)pin))
		throw gcnew InvalidOperationException(__FUNCTION__);

	// set new selection
	cls.SelEnd = cls.SelStart + value->Length;
	if (!Info.PanelControl(INVALID_HANDLE_VALUE, FCTL_SETCMDLINESELECTION, 0, &cls))
		throw gcnew InvalidOperationException(__FUNCTION__);

	// restore cursor
	Caret = pos <= text->Length ? pos : text->Length;
}

}
