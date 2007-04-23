/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#include "StdAfx.h"
#include "VisibleEditorLine.h"
#include "Utils.h"
#include "VisibleEditorLineSelection.h"

namespace FarManagerImpl
{;
VisibleEditorLine::VisibleEditorLine(int no, bool selected)
: _no(no), _selected(selected)
{
}

//!! Keep it before "#define _selection"
ILineSelection^ VisibleEditorLine::Selection::get()
{
	if (_selection == nullptr)
		_selection = gcnew VisibleEditorLineSelection(_no);
	return _selection;
}
//!! DON'T use _selection after this point
#define _selection _selection__use_property

ILine^ VisibleEditorLine::FullLine::get()
{
	return _selected ? gcnew VisibleEditorLine(_no, false) : this;
}

int VisibleEditorLine::Length::get()
{
	if (_selected)
		return Selection->Length;

	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _no);
	return egs.StringLength;
}

int VisibleEditorLine::No::get()
{
	return _no;
}

int VisibleEditorLine::Pos::get()
{
	EditorInfo ei;
	EditorControl_ECTL_GETINFO(ei);
	if (_no < 0 || _no == ei.CurLine)
		return ei.CurPos;
	return -1;
}

void VisibleEditorLine::Pos::set(int value)
{
	if (value < 0)
	{
		EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _no);
		value = egs.StringLength;
	}
	SEditorSetPosition esp;
	esp.CurPos = value;
	esp.CurLine = _no;
	EditorControl_ECTL_SETPOSITION(esp);
}

String^ VisibleEditorLine::Eol::get()
{
	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _no);
	return FromEditor(egs.StringEOL, strlen(egs.StringEOL));
}

void VisibleEditorLine::Eol::set(String^ value)
{
	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _no);
	EditorSetString ess = GetEss();
	CStr sb(value);
	EditorControl_ECTL_OEMTOEDITOR(sb, value->Length);

	ess.StringEOL = sb;
	ess.StringLength = egs.StringLength;
	ess.StringText = (char*)egs.StringText;

	EditorControl_ECTL_SETSTRING(ess);
}

String^ VisibleEditorLine::Text::get()
{
	if (_selected)
		return Selection->Text;

	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _no);
	return FromEditor(egs.StringText,  egs.StringLength);
}

void VisibleEditorLine::Text::set(String^ value)
{
	if (_selected)
	{
		Selection->Text = value;
		return;
	}

	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _no);
	EditorSetString ess = GetEss();
	CStr sb(value);
	EditorControl_ECTL_OEMTOEDITOR(sb, value->Length);
	ess.StringText = sb;
	ess.StringEOL = (char*)egs.StringEOL;
	ess.StringLength = value->Length;
	EditorControl_ECTL_SETSTRING(ess);
}

void VisibleEditorLine::Insert(String^ text)
{
	int pos = Pos;
	if (pos < 0)
		throw gcnew InvalidOperationException("The line is not current");
	CStr sb(text->Replace(CV::CRLF, CV::CR)->Replace('\n', '\r'));
	Info.EditorControl(ECTL_INSERTTEXT, sb);
}

void VisibleEditorLine::Select(int start, int end)
{
	EditorSelect es;
	es.BlockType = BTYPE_STREAM;
	es.BlockStartLine = _no;
	es.BlockStartPos = start;
	es.BlockHeight = 1;
	es.BlockWidth = end - start;
	EditorControl_ECTL_SELECT(es);
}

void VisibleEditorLine::Unselect()
{
	EditorSelect es;
	es.BlockType = BTYPE_NONE;
	EditorControl_ECTL_SELECT(es);
}

String^ VisibleEditorLine::ToString()
{
	return Text;
}

EditorSetString VisibleEditorLine::GetEss()
{
	EditorSetString ess;
	ess.StringNumber = _no;
	return ess;
}
}
