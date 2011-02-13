
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "EditorLine.h"
#include "Wrappers.h"

namespace FarNet
{;
EditorLine::EditorLine(int index)
: _Index(index)
{}

FarNet::WindowKind EditorLine::WindowKind::get()
{
	return FarNet::WindowKind::Editor;
}

int EditorLine::Length::get()
{
	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _Index);
	return egs.StringLength;
}

int EditorLine::Index::get()
{
	return _Index;
}

int EditorLine::Caret::get()
{
	AutoEditorInfo ei;

	if (_Index < 0 || _Index == ei.CurLine)
		return ei.CurPos;

	return -1;
}

void EditorLine::Caret::set(int value)
{
	if (value < 0)
	{
		EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _Index);
		value = egs.StringLength;
	}
	SEditorSetPosition esp;
	esp.CurPos = value;
	esp.CurLine = _Index;
	EditorControl_ECTL_SETPOSITION(esp);
}

String^ EditorLine::Text::get()
{
	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _Index);
	return gcnew String(egs.StringText, 0, egs.StringLength);
}

void EditorLine::Text::set(String^ value)
{
	if (!value)
		throw gcnew ArgumentNullException("value");

	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _Index);
	EditorSetString ess = GetEss();
	PIN_NE(pin, value);
	ess.StringText = pin;
	ess.StringEOL = egs.StringEOL;
	ess.StringLength = value->Length;
	EditorControl_ECTL_SETSTRING(ess);
}

Span EditorLine::SelectionSpan::get()
{
	Span result;
	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _Index);

	if (egs.SelStart < 0)
	{
		result.Start = -1;
		result.End = -2;
	}
	else if (egs.SelEnd < 0)
	{
		result.Start = egs.SelStart;
		result.End = egs.StringLength;
	}
	else
	{
		result.Start = egs.SelStart;
		result.End = egs.SelEnd;
	}

	return result;
}

String^ EditorLine::SelectedText::get()
{
	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _Index);
	if (egs.SelStart < 0)
		return nullptr;
	if (egs.SelEnd < 0)
		egs.SelEnd = egs.StringLength;

	return gcnew String(egs.StringText + egs.SelStart, 0, egs.SelEnd - egs.SelStart);
}

void EditorLine::SelectedText::set(String^ value)
{
	if (!value)
		throw gcnew ArgumentNullException("value");

	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _Index);
	if (egs.SelStart < 0)
		throw gcnew InvalidOperationException(Res::CannotSetSelectedText);

	String^ text1 = gcnew String(egs.StringText, 0, egs.StringLength);
	String^ text2 = text1->Substring(0, egs.SelStart) + value;
	int dd = 0;
	if (egs.SelEnd >= 0)
	{
		text2 = text2 + text1->Substring(egs.SelEnd);
		dd = egs.SelStart + value->Length - egs.SelEnd;
	}

	// set string
	PIN_NE(pin, text2);
	EditorSetString ess;
	ess.StringEOL = egs.StringEOL;
	ess.StringLength = text2->Length;
	ess.StringNumber = _Index;
	ess.StringText = pin;
	EditorControl_ECTL_SETSTRING(ess);

	// change selection
	if (dd != 0)
	{
		Place pp = Edit_SelectionPlace();
		EditorSelect es;
		es.BlockHeight = pp.Bottom - pp.Top + 1;
		es.BlockStartLine = pp.Top;
		es.BlockStartPos = pp.Left;
		es.BlockType = BTYPE_STREAM;
		es.BlockWidth = pp.Right + 1 - pp.Left + dd;
		EditorControl_ECTL_SELECT(es);
	}
}

void EditorLine::InsertText(String^ text)
{
	int pos = Caret;
	if (pos < 0)
		throw gcnew InvalidOperationException("The line is not current.");

	EditorControl_ECTL_INSERTTEXT(text, -1);
}

void EditorLine::SelectText(int start, int end)
{
	EditorSelect es;
	es.BlockType = BTYPE_STREAM;
	es.BlockStartLine = _Index;
	es.BlockStartPos = start;
	es.BlockHeight = 1;
	es.BlockWidth = end - start;
	EditorControl_ECTL_SELECT(es);
}

void EditorLine::UnselectText()
{
	EditorSelect es;
	es.BlockType = BTYPE_NONE;
	EditorControl_ECTL_SELECT(es);
}

EditorSetString EditorLine::GetEss()
{
	EditorSetString ess;
	ess.StringNumber = _Index;
	return ess;
}
}
