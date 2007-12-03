/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#include "StdAfx.h"
#include "EditorLine.h"
#include "Utils.h"
#include "EditorLineSelection.h"

namespace FarNet
{;
EditorLine::EditorLine(int no, bool selected)
: _no(no), _selected(selected)
{
}

//!! Keep it before "#define _selection"
ILineSelection^ EditorLine::Selection::get()
{
	if (_selection == nullptr)
		_selection = gcnew EditorLineSelection(_no);
	return _selection;
}
//!! DON'T use _selection after this point
#define _selection _selection__use_property

ILine^ EditorLine::FullLine::get()
{
	return _selected ? gcnew EditorLine(_no, false) : this;
}

int EditorLine::Length::get()
{
	if (_selected)
		return Selection->Length;

	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _no);
	return egs.StringLength;
}

int EditorLine::No::get()
{
	return _no;
}

int EditorLine::Pos::get()
{
	EditorInfo ei;
	EditorControl_ECTL_GETINFO(ei);
	if (_no < 0 || _no == ei.CurLine)
		return ei.CurPos;
	return -1;
}

void EditorLine::Pos::set(int value)
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

String^ EditorLine::Eol::get()
{
	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _no);
	return FromEditor(egs.StringEOL, (int)strlen(egs.StringEOL));
}

void EditorLine::Eol::set(String^ value)
{
	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _no);
	EditorSetString ess = GetEss();
	CBox sb(value);
	EditorControl_ECTL_OEMTOEDITOR(sb, value->Length);

	ess.StringEOL = sb;
	ess.StringLength = egs.StringLength;
	ess.StringText = (char*)egs.StringText;

	EditorControl_ECTL_SETSTRING(ess);
}

String^ EditorLine::Text::get()
{
	if (_selected)
		return Selection->Text;

	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _no);
	return FromEditor(egs.StringText,  egs.StringLength);
}

void EditorLine::Text::set(String^ value)
{
	if (_selected)
	{
		Selection->Text = value;
		return;
	}

	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _no);
	EditorSetString ess = GetEss();
	CBox sb(value);
	EditorControl_ECTL_OEMTOEDITOR(sb, value->Length);
	ess.StringText = sb;
	ess.StringEOL = (char*)egs.StringEOL;
	ess.StringLength = value->Length;
	EditorControl_ECTL_SETSTRING(ess);
}

void EditorLine::Insert(String^ text)
{
	int pos = Pos;
	if (pos < 0)
		throw gcnew InvalidOperationException("The line is not current");
	EditorControl_ECTL_INSERTTEXT(text, -1);
}

void EditorLine::Select(int start, int end)
{
	EditorSelect es;
	es.BlockType = BTYPE_STREAM;
	es.BlockStartLine = _no;
	es.BlockStartPos = start;
	es.BlockHeight = 1;
	es.BlockWidth = end - start;
	EditorControl_ECTL_SELECT(es);
}

void EditorLine::Unselect()
{
	EditorSelect es;
	es.BlockType = BTYPE_NONE;
	EditorControl_ECTL_SELECT(es);
}

String^ EditorLine::ToString()
{
	return Text;
}

EditorSetString EditorLine::GetEss()
{
	EditorSetString ess;
	ess.StringNumber = _no;
	return ess;
}
}
