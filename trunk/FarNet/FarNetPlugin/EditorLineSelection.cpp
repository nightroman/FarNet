/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2008 FAR.NET Team
*/

#include "StdAfx.h"
#include "EditorLineSelection.h"
#include "Utils.h"

namespace FarNet
{;
EditorLineSelection::EditorLineSelection(int no)
: _no(no)
{
}

String^ EditorLineSelection::Text::get()
{
	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _no);
	if (egs.SelStart < 0)
		return nullptr;
	if (egs.SelEnd < 0)
		egs.SelEnd = egs.StringLength;

	return FromEditor(egs.StringText + egs.SelStart, egs.SelEnd - egs.SelStart);
}

void EditorLineSelection::Text::set(String^ value)
{
	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _no);
	if (egs.SelStart < 0)
		throw gcnew InvalidOperationException("Can't set text: there is no selection.");

	String^ text1 = FromEditor(egs.StringText, egs.StringLength);
	String^ text2 = text1->Substring(0, egs.SelStart) + value;
	int dd = 0;
	if (egs.SelEnd >= 0)
	{
		text2 = text2 + text1->Substring(egs.SelEnd);
		dd = value->Length - egs.SelEnd;
	}

	// set string
	CBox sb(text2);
	EditorControl_ECTL_OEMTOEDITOR(sb, text2->Length);
	EditorSetString ess;
	ess.StringEOL = (char*)egs.StringEOL;
	ess.StringLength = text2->Length;
	ess.StringNumber = _no;
	ess.StringText = sb;
	EditorControl_ECTL_SETSTRING(ess);
	
	// change selection
	if (dd != 0)
	{
		Place pp = SelectionPlace();
		EditorSelect es;
		es.BlockHeight = pp.Bottom - pp.Top + 1;
		es.BlockStartLine = pp.Top;
		es.BlockStartPos = pp.Left;
		es.BlockType = BTYPE_STREAM;
		es.BlockWidth = pp.Right + 1 - pp.Left + dd;
		EditorControl_ECTL_SELECT(es);
	}
}

int EditorLineSelection::End::get()
{
	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _no);
	return egs.SelStart < 0 ? -2 : egs.SelEnd;
}

int EditorLineSelection::Length::get()
{
	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _no);
	if (egs.SelStart < 0)
		return -1;
	if (egs.SelEnd < 0)
		egs.SelEnd = egs.StringLength;
	return egs.SelEnd - egs.SelStart;
}

int EditorLineSelection::Start::get()
{
	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _no);
	return egs.SelStart;
}

String^ EditorLineSelection::ToString()
{
	return Text;
}
}
