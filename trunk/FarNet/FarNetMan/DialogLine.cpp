
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

#include "StdAfx.h"
#include "DialogLine.h"
#include "Dialog.h"
#include "Wrappers.h"

namespace FarNet
{;
DialogLine::DialogLine(HANDLE hDlg, int id)
: _hDlg(hDlg)
, _id(id)
{}

int DialogLine::Length::get()
{
	return (int)Info.SendDlgMessage(_hDlg, DM_GETTEXT, _id, 0);
}

int DialogLine::Caret::get()
{
	COORD c;
	c.Y = 0;
	Info.SendDlgMessage(_hDlg, DM_GETCURSORPOS, _id, &c);
	return c.X;
}

void DialogLine::Caret::set(int value)
{
	if (value < 0)
		value = Length;

	COORD c;
	c.Y = 0;
	c.X = (SHORT)value;
	Info.SendDlgMessage(_hDlg, DM_SETCURSORPOS, _id, &c);

	//_100819_142053 Mantis 1464. ?? For now drop 'unchanged' manually.
	//???? Wait! It affects $TestLine.SelectedText = '12345' @ "Test-Line+.ps1"
	//Info.SendDlgMessage(_hDlg, DM_EDITUNCHANGEDFLAG, _id, (LONG_PTR)0);
}

String^ DialogLine::Text::get()
{
	return ::GetDialogControlText(_hDlg, _id, -1, 0);
}

void DialogLine::Text::set(String^ value)
{
	PIN_NE(pin, value);
	Info.SendDlgMessage(_hDlg, DM_SETTEXTPTR, _id, (wchar_t*)pin);
}

Span DialogLine::SelectionSpan::get()
{
	EditorSelect es = {sizeof(es)};
	Info.SendDlgMessage(_hDlg, DM_GETSELECTION, _id, &es);

	Span result;
	if (es.BlockType == BTYPE_NONE)
	{
		result.Start = -1;
		result.End = -2;
	}
	else
	{
		result.Start = (int)es.BlockStartPos;
		result.End = (int)(es.BlockStartPos + es.BlockWidth);
	}

	return result;
}

String^ DialogLine::SelectedText::get()
{
	EditorSelect es = {sizeof(es)};
	Info.SendDlgMessage(_hDlg, DM_GETSELECTION, _id, &es);
	if (es.BlockType == BTYPE_NONE)
		return nullptr;

	return ::GetDialogControlText(_hDlg, _id, (int)es.BlockStartPos, (int)es.BlockWidth);
}

void DialogLine::SelectedText::set(String^ value)
{
	EditorSelect es = {sizeof(es)};
	Info.SendDlgMessage(_hDlg, DM_GETSELECTION, _id, &es);
	if (es.BlockType == BTYPE_NONE)
		throw gcnew InvalidOperationException(Res::CannotSetSelectedText);

	// store cursor
	int pos = Caret;

	// make and set new text
	String^ text = ::GetDialogControlText(_hDlg, _id, -1, 0);
	text = text->Substring(0, (int)es.BlockStartPos) + value + text->Substring((int)(es.BlockStartPos + es.BlockWidth));
	PIN_NE(pin, text);
	Info.SendDlgMessage(_hDlg, DM_SETTEXTPTR, _id, (wchar_t*)pin);

	// set selection
	es.BlockWidth = value->Length;
	Info.SendDlgMessage(_hDlg, DM_SETSELECTION, _id, &es);

	// restore cursor
	Caret = pos <= text->Length ? pos : text->Length;
}

FarNet::WindowKind DialogLine::WindowKind::get()
{
	return FarNet::WindowKind::Dialog;
}

void DialogLine::InsertText(String^ text)
{
	if (!text) throw gcnew ArgumentNullException("text");

	// insert string before cursor
	int pos = Caret;
	String^ str = Text;

	// set new text and move cursor to the end of inserted part
	Text = str->Substring(0, pos) + text + str->Substring(pos);
	Caret = pos + text->Length;
}

void DialogLine::SelectText(int start, int end)
{
	EditorSelect es = {sizeof(es)};
	es.BlockType = BTYPE_STREAM;
	es.BlockStartLine = 0;
	es.BlockStartPos = start;
	es.BlockWidth = end - start;
	es.BlockHeight = 1;
	Info.SendDlgMessage(_hDlg, DM_SETSELECTION, _id, &es);
}

void DialogLine::UnselectText()
{
	EditorSelect es = {sizeof(es)};
	es.BlockType = BTYPE_NONE;
	Info.SendDlgMessage(_hDlg, DM_SETSELECTION, _id, &es);
}

bool DialogLine::IsReadOnly::get()
{
	FarDialogItem di;
	Info.SendDlgMessage(_hDlg, DM_GETDLGITEMSHORT, _id, &di);
	return (di.Flags & DIF_READONLY) != 0;
}

}
