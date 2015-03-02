
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

#include "StdAfx.h"
#include "EditorLine.h"
#include "Wrappers.h"

namespace FarNet
{;
EditorLine::EditorLine(intptr_t editorId, int index)
: _EditorId(editorId)
, _Index(index)
{}

FarNet::WindowKind EditorLine::WindowKind::get()
{
	return FarNet::WindowKind::Editor;
}

int EditorLine::Length::get()
{
	EditorGetString egs = {sizeof(egs)};
	EditorControl_ECTL_GETSTRING(egs, _EditorId, _Index);
	return (int)egs.StringLength;
}

int EditorLine::Index::get()
{
	return _Index;
}

int EditorLine::Caret::get()
{
	AutoEditorInfo ei(_EditorId);

	if (_Index < 0 || _Index == ei.CurLine)
		return (int)ei.CurPos;

	return -1;
}

void EditorLine::Caret::set(int value)
{
	if (value < 0)
	{
		EditorGetString egs = {sizeof(egs)};
		EditorControl_ECTL_GETSTRING(egs, _EditorId, _Index);
		value = (int)egs.StringLength;
	}
	SEditorSetPosition esp;
	esp.CurPos = value;
	esp.CurLine = _Index;
	EditorControl_ECTL_SETPOSITION(_EditorId, esp);
}

String^ EditorLine::Text::get()
{
	EditorGetString egs = {sizeof(egs)};
	EditorControl_ECTL_GETSTRING(egs, _EditorId, _Index);
	return gcnew String(egs.StringText, 0, (int)egs.StringLength);
}

void EditorLine::Text::set(String^ value)
{
	if (!value)
		throw gcnew ArgumentNullException("value");

	EditorGetString egs = {sizeof(egs)};
	EditorControl_ECTL_GETSTRING(egs, _EditorId, _Index);
	EditorSetString ess = GetEss();
	PIN_NE(pin, value);
	ess.StringText = pin;
	ess.StringEOL = egs.StringEOL;
	ess.StringLength = value->Length;
	EditorControl_ECTL_SETSTRING(_EditorId, ess);
}

Span EditorLine::SelectionSpan::get()
{
	Span result;
	EditorGetString egs = {sizeof(egs)};
	EditorControl_ECTL_GETSTRING(egs, _EditorId, _Index);

	if (egs.SelStart < 0)
	{
		result.Start = -1;
		result.End = -2;
	}
	else if (egs.SelEnd < 0)
	{
		result.Start = (int)egs.SelStart;
		result.End = (int)egs.StringLength;
	}
	else
	{
		result.Start = (int)egs.SelStart;
		result.End = (int)egs.SelEnd;
	}

	return result;
}

String^ EditorLine::SelectedText::get()
{
	EditorGetString egs = {sizeof(egs)};
	EditorControl_ECTL_GETSTRING(egs, _EditorId, _Index);
	if (egs.SelStart < 0)
		return nullptr;
	if (egs.SelEnd < 0)
		egs.SelEnd = egs.StringLength;

	return gcnew String(egs.StringText + egs.SelStart, 0, (int)(egs.SelEnd - egs.SelStart));
}

void EditorLine::SelectedText::set(String^ value)
{
	if (!value)
		throw gcnew ArgumentNullException("value");

	EditorGetString egs = {sizeof(egs)};
	EditorControl_ECTL_GETSTRING(egs, _EditorId, _Index);
	if (egs.SelStart < 0)
		throw gcnew InvalidOperationException(Res::CannotSetSelectedText);

	String^ text1 = gcnew String(egs.StringText, 0, (int)egs.StringLength);
	String^ text2 = text1->Substring(0, (int)egs.SelStart) + value;
	int dd = 0;
	if (egs.SelEnd >= 0)
	{
		text2 = text2 + text1->Substring((int)egs.SelEnd);
		dd = (int)(egs.SelStart + value->Length - egs.SelEnd);
	}

	// set string
	PIN_NE(pin, text2);
	EditorSetString ess = {sizeof(ess)};
	ess.StringEOL = egs.StringEOL;
	ess.StringLength = text2->Length;
	ess.StringNumber = _Index;
	ess.StringText = pin;
	EditorControl_ECTL_SETSTRING(_EditorId, ess);

	// change selection
	if (dd != 0)
	{
		Place pp = Edit_SelectionPlace(_EditorId);
		EditorSelect es = {sizeof(es)};
		es.BlockHeight = pp.Bottom - pp.Top + 1;
		es.BlockStartLine = pp.Top;
		es.BlockStartPos = pp.Left;
		es.BlockType = BTYPE_STREAM;
		es.BlockWidth = pp.Right + 1 - pp.Left + dd;
		EditorControl_ECTL_SELECT(_EditorId, es);
	}
}

void EditorLine::InsertText(String^ text)
{
	int pos = Caret;
	if (pos < 0)
		throw gcnew InvalidOperationException("The line is not current.");

	EditorControl_ECTL_INSERTTEXT(_EditorId, text, -1);
}

void EditorLine::SelectText(int start, int end)
{
	EditorSelect es = {sizeof(es)};
	es.BlockType = BTYPE_STREAM;
	es.BlockStartLine = _Index;
	es.BlockStartPos = start;
	es.BlockHeight = 1;
	es.BlockWidth = end - start;
	EditorControl_ECTL_SELECT(_EditorId, es);
}

void EditorLine::UnselectText()
{
	EditorSelect es = {sizeof(es)};
	es.BlockType = BTYPE_NONE;
	EditorControl_ECTL_SELECT(_EditorId, es);
}

EditorSetString EditorLine::GetEss()
{
	EditorSetString ess = {sizeof(ess)};
	ess.StringNumber = _Index;
	return ess;
}

bool EditorLine::IsReadOnly::get()
{
	AutoEditorInfo ei(_EditorId);

	return (ei.CurState & ECSTATE_LOCKED) != 0;
}

}
