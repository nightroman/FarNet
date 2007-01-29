#include "StdAfx.h"
#include "VisibleEditorLineSelection.h"
#include "Utils.h"

namespace FarManagerImpl
{;
VisibleEditorLineSelection::VisibleEditorLineSelection(int no)
{
	_no = no;
}

String^ VisibleEditorLineSelection::Text::get()
{
	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _no);
	if (egs.SelStart < 0)
		return nullptr;
	if (egs.SelEnd < 0)
		egs.SelEnd = egs.StringLength;
	return fromEditor(egs.StringText + egs.SelStart, egs.SelEnd - egs.SelStart);
}

void VisibleEditorLineSelection::Text::set(String^ value)
{
	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _no);
	if (egs.SelStart < 0)
		throw gcnew InvalidOperationException("Can't set text: there is no selection.");

	String^ text1 = fromEditor(egs.StringText, egs.StringLength);
	String^ text2 = text1->Substring(0, egs.SelStart) + value;
	int dd = 0;
	if (egs.SelEnd >= 0)
	{
		text2 = text2 + text1->Substring(egs.SelEnd);
		dd = value->Length - egs.SelEnd;
	}

	// set string
	CStr sb(text2);
	convert(ECTL_OEMTOEDITOR, sb, text2->Length);
	EditorSetString ess;
	ess.StringEOL = (char*)egs.StringEOL;
	ess.StringLength = text2->Length;
	ess.StringNumber = _no;
	ess.StringText = sb;
	EditorControl_ECTL_SETSTRING(ess);
	
	// change selection
	if (dd != 0)
	{
		EditorInfo ei; EditorControl_ECTL_GETINFO(ei);
		if (ei.BlockType != BTYPE_STREAM)
			throw gcnew InvalidOperationException("Can't process this selection shape");

		EditorGetString egs;
		int top = ei.BlockStartLine;
		int left = -1;
		int right = -1;
		for(egs.StringNumber = top; Info.EditorControl(ECTL_GETSTRING, &egs); ++egs.StringNumber) // TODO dupe
		{
			if (left < 0)
				left = egs.SelStart;
			if (egs.SelStart < 0)
				break;
			right = egs.SelEnd;
		}
		int bottom = egs.StringNumber - 1;

		EditorSelect es;
		es.BlockHeight = bottom - top + 1;
		es.BlockStartLine = top;
		es.BlockStartPos = left;
		es.BlockType = BTYPE_STREAM;
		es.BlockWidth = right - left + dd;
		EditorControl_ECTL_SELECT(es);
	}
}

int VisibleEditorLineSelection::End::get()
{
	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _no);
	return egs.SelStart < 0 ? -2 : egs.SelEnd;
}

int VisibleEditorLineSelection::Length::get()
{
	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _no);
	if (egs.SelStart < 0)
		return -1;
	if (egs.SelEnd < 0)
		egs.SelEnd = egs.StringLength;
	return egs.SelEnd - egs.SelStart;
}

int VisibleEditorLineSelection::Start::get()
{
	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _no);
	return egs.SelStart;
}

String^ VisibleEditorLineSelection::ToString()
{
	return Text;
}
}
