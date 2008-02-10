/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2008 FAR.NET Team
*/

#include "StdAfx.h"
#include "SelectionCollection.h"
#include "Utils.h"
#include "EditorLine.h"

namespace FarNet
{;
SelectionCollection::SelectionCollection(IEditor^ editor, bool trueLines)
: _editor(editor)
, _trueLines(trueLines)
{
	_strings = gcnew EditorStringCollection(this, true);
}

IEnumerator<ILine^>^ SelectionCollection::GetEnumerator()
{
	Place ss = SelectionPlace();
	if (ss.Top < 0)
		return gcnew LineListEnumerator(this, 0, 0);
	return gcnew LineListEnumerator(this, 0, ss.Height);
}

Collections::IEnumerator^ SelectionCollection::GetEnumeratorObject()
{
	return GetEnumerator();
}

bool SelectionCollection::Exists::get()
{
	return Type != SelectionType::None;
}

bool SelectionCollection::IsFixedSize::get()
{
	return false;
}

bool SelectionCollection::IsReadOnly::get()
{
	return false;
}

bool SelectionCollection::IsSynchronized::get()
{
	return false;
}

ILine^ SelectionCollection::First::get()
{
	return Item[0];
}

ILine^ SelectionCollection::Last::get()
{
	return Item[Count - 1];
}

Place SelectionCollection::Shape::get()
{
	return SelectionPlace();
}

IStrings^ SelectionCollection::Strings::get()
{
	return _strings;
}

ILine^ SelectionCollection::Item::get(int index)
{
	EditorInfo ei; EditorControl_ECTL_GETINFO(ei);
	if (ei.BlockType == BTYPE_NONE)
		throw gcnew InvalidOperationException();
	return gcnew EditorLine(index + ei.BlockStartLine, true);
}

Object^ SelectionCollection::SyncRoot::get()
{
	return this;
}

SelectionType SelectionCollection::Type::get()
{
	EditorInfo ei; EditorControl_ECTL_GETINFO(ei);
	return (SelectionType)ei.BlockType;
}

void SelectionCollection::Add(String^ item)
{
	// -1 avoids Count here
	Insert(-1, item);
}

void SelectionCollection::Clear()
{
	EditorControl_ECTL_DELETEBLOCK();
}

void SelectionCollection::Insert(int index, String^ item)
{
	EditorInfo ei; EditorControl_ECTL_GETINFO(ei);
	if (ei.BlockType == BTYPE_NONE)
		throw gcnew InvalidOperationException("No selection shape");

	EditorGetString egss; EditorControl_ECTL_GETSTRING(egss, ei.BlockStartLine);
	if (ei.BlockType == BTYPE_COLUMN && egss.SelEnd < 0)
		throw gcnew InvalidOperationException("Can't process this selection shape");

	// case: first
	if (index == 0)
	{
		// NB: both cases: first incomplete\complete
		_editor->GoTo(egss.SelStart, ei.BlockStartLine);
		EditorControl_ECTL_INSERTTEXT(item + CV::CR, ei.Overtype);
		return;
	}

	// correct negative index
	if (index < 0)
		index = Count;

	// prior to insertion line
	int ip = ei.BlockStartLine + index - 1;
	EditorGetString egsp; EditorControl_ECTL_GETSTRING(egsp, ip);

	// case: inside
	if (egsp.SelEnd < 0)
	{
		_editor->Lines->Insert(ei.BlockStartLine + index, item);
		return;
	}

	// case: add (prior is actually the last)
	_editor->GoTo(egsp.SelEnd, ip);

	if (egsp.SelEnd == 0)
	{
		// ELL case
		EditorControl_ECTL_INSERTTEXT(item + CV::CR, ei.Overtype);
	}
	else
	{
		// not ELL case
		EditorControl_ECTL_INSERTSTRING(false);
		EditorControl_ECTL_DELETECHAR();
		EditorControl_ECTL_INSERTTEXT(item, ei.Overtype);
	}

	// select inserted
	EditorInfo ei2; EditorControl_ECTL_GETINFO(ei2);
	Select(SelectionType::Stream, egss.SelStart, ei.BlockStartLine, ei2.CurPos - 1, ei2.CurLine);
}

void SelectionCollection::RemoveAt(int index)
{
	EditorInfo ei; EditorControl_ECTL_GETINFO(ei);
	if (ei.BlockType == BTYPE_NONE)
		throw gcnew InvalidOperationException("No selection shape");

	EditorGetString egss; EditorControl_ECTL_GETSTRING(egss, ei.BlockStartLine);
	if (ei.BlockType == BTYPE_COLUMN && egss.SelEnd < 0)
		throw gcnew InvalidOperationException("Can't process this selection shape");

	if (index < 0 || egss.SelStart < 0)
		throw gcnew ArgumentOutOfRangeException("index");

	// case: just 1 line
	if (egss.SelEnd >= 0)
	{
		Clear();
		return;
	}

	// case: remove first incomplete
	if (index == 0 && egss.SelStart > 0)
	{
		// keep first
		int top = ei.BlockStartLine;
		int left = egss.SelStart;

		// change selection
		Place ss = Shape;
		++ss.Top;
		ss.Left = 0;
		Select((SelectionType)ei.BlockType, ss.Left, ss.Top, ss.Right, ss.Bottom);

		// remove selected part of line
		ILine^ line = _editor->Lines[top];
		line->Text = line->Text->Substring(0, left);
		return;
	}

	// case: remove last
	int bottom = ei.BlockStartLine + index;
	EditorGetString egsi; EditorControl_ECTL_GETSTRING(egsi, bottom);
	if (egsi.SelEnd >= 0)
	{
		bool ell = egsi.SelEnd == 0;

		// remove not empty part of line
		if (!ell)
		{
			ILine^ line = _editor->Lines[bottom];
			line->Text = line->Text->Substring(egsi.SelEnd);
		}

		// select to the end of previous line
		--bottom;
		ILine^ line = _editor->Lines[bottom];
		String^ text = line->Text;
		if (text->Length == 0 && ell)
		{
			// prior line is empty, remove it
			_editor->Lines->RemoveAt(bottom);
			return;
		}
		Select(SelectionType::Stream, egss.SelStart, ei.BlockStartLine, text->Length - 1, bottom);
		return;
	}

	// remove inside
	_editor->Lines->RemoveAt(bottom);
}

void SelectionCollection::Select(SelectionType type, int pos1, int line1, int pos2, int line2)
{
	// type
	EditorSelect es;
	switch(type)
	{
	case SelectionType::None:
		es.BlockType = BTYPE_NONE;
		EditorControl_ECTL_SELECT(es);
		return;
	case SelectionType::Rect:
		es.BlockType = BTYPE_COLUMN;
		break;
	case SelectionType::Stream:
		es.BlockType = BTYPE_STREAM;
		break;
	default:
		throw gcnew ArgumentException("Unknown selection type");
	}

	// swap
	if (line1 > line2 || line1 == line2 && pos1 > pos2)
	{
		int t;
		t = pos1; pos1 = pos2; pos2 = t;
		t = line1; line1 = line2; line2 = t;
	}

	// go
	es.BlockStartLine = line1;
	es.BlockStartPos = pos1;
	es.BlockHeight = line2 - line1 + 1;
	es.BlockWidth = pos2 - pos1 + 1;
	EditorControl_ECTL_SELECT(es);
}

void SelectionCollection::Unselect()
{
	EditorSelect es;
	es.BlockType = BTYPE_NONE;
	EditorControl_ECTL_SELECT(es);
}

int SelectionCollection::Count::get()
{
	EditorInfo ei; EditorControl_ECTL_GETINFO(ei);
	if (ei.BlockType == BTYPE_NONE)
		return 0;

	int r = 0;
	EditorGetString egs;
	for(egs.StringNumber = ei.BlockStartLine; egs.StringNumber < ei.TotalLines; ++egs.StringNumber)
	{
		EditorControl_ECTL_GETSTRING(egs, egs.StringNumber);
		if (egs.SelStart < 0)
			break;
		if (egs.SelEnd == 0)
		{
			if (!_trueLines)
				++r;
			break;
		}
		++r;
	}
	return r;
}

String^ SelectionCollection::GetText(String^ separator)
{
	StringBuilder sb;

	EditorInfo ei; EditorControl_ECTL_GETINFO(ei);
	if (ei.BlockType == BTYPE_NONE)
		return String::Empty;

	if (separator == nullptr)
		separator = CV::CRLF;

	EditorGetString egs; egs.StringNumber = -1;
    SEditorSetPosition esp;
	for(esp.CurLine = ei.BlockStartLine; esp.CurLine < ei.TotalLines; ++esp.CurLine)
    {
        EditorControl_ECTL_SETPOSITION(esp);
        Info.EditorControl(ECTL_GETSTRING, &egs);
		if (egs.SelStart < 0)
			break;
		if (esp.CurLine > ei.BlockStartLine)
			sb.Append(separator);
		int len = (egs.SelEnd < 0 ? egs.StringLength : egs.SelEnd) - egs.SelStart;
		if (len > 0)
			sb.Append(FromEditor(egs.StringText + egs.SelStart, len));
    }
	Edit_RestoreEditorInfo(ei);

	return sb.ToString();
}

void SelectionCollection::SetText(String^ text)
{
	EditorInfo ei; EditorControl_ECTL_GETINFO(ei);
	if (ei.BlockType == BTYPE_NONE)
		throw gcnew InvalidOperationException("No selection shape");

	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, ei.BlockStartLine);
	if (ei.BlockType == BTYPE_COLUMN && egs.SelEnd < 0)
		throw gcnew InvalidOperationException("Can't process this selection shape.");

	// delete selection
	int top = ei.BlockStartLine;
	int left = egs.SelStart;
	Clear();

	// move cursor to the selection start
	_editor->GoTo(left, top);

	// insert
	EditorControl_ECTL_INSERTTEXT(text, ei.Overtype);

	// select inserted
	EditorControl_ECTL_GETINFO(ei);
	Select(SelectionType::Stream, left, top, ei.CurPos - 1, ei.CurLine);
}
}
