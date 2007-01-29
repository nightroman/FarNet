#include "StdAfx.h"
#include "VisibleEditorLine.h"
#include "Utils.h"
#include "VisibleEditorLineSelection.h"

namespace FarManagerImpl
{;
VisibleEditorLine::VisibleEditorLine(int no, bool selected)
{
	_no = no;
	_selected = selected;
}

ILineSelection^ VisibleEditorLine::Selection::get()
{
	if (_selection == nullptr)
		_selection = gcnew VisibleEditorLineSelection(_no);
	return _selection;
}
// DON'T use:
#define _selection _selection__use_property

int VisibleEditorLine::No::get()
{
	return _no;
}

String^ VisibleEditorLine::Eol::get()
{
	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _no);
	return fromEditor(egs.StringEOL, strlen(egs.StringEOL));
}

void VisibleEditorLine::Eol::set(String^ value)
{
	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, _no);
	EditorSetString ess = GetEss();
	CStr sb(value);
	convert(ECTL_OEMTOEDITOR, sb, value->Length);

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
	return fromEditor(egs.StringText,  egs.StringLength);
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
	convert(ECTL_OEMTOEDITOR, sb, value->Length);
	ess.StringText = sb;
	ess.StringEOL = (char*)egs.StringEOL;
	ess.StringLength = value->Length;
	EditorControl_ECTL_SETSTRING(ess);
}

EditorSetString VisibleEditorLine::GetEss()
{
	EditorSetString ess;
	ess.StringNumber = _no;
	return ess;
}

String^ VisibleEditorLine::ToString()
{
	return Text;
}
}
