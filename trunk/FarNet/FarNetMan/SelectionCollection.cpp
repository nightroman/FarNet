/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "SelectionCollection.h"
#include "EditorLine.h"
#include "Wrappers.h"

namespace FarNet
{;
SelectionCollection::SelectionCollection(IEditor^ editor, bool ignoreEmptyLast)
: _editor(editor)
, IgnoreEmptyLast(ignoreEmptyLast)
{}

IEnumerator<ILine^>^ SelectionCollection::GetEnumerator()
{
	Place ss = SelectionPlace();
	return gcnew Works::LineEnumerator(this, 0, ss.Top < 0 ? 0 : ss.Height);
}

Collections::IEnumerator^ SelectionCollection::GetEnumeratorObject()
{
	return GetEnumerator();
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

ILine^ SelectionCollection::Item::get(int index)
{
	AutoEditorInfo ei;

	if (ei.BlockType == BTYPE_NONE)
		throw gcnew InvalidOperationException;

	return gcnew EditorLine(index + ei.BlockStartLine, true);
}

Object^ SelectionCollection::SyncRoot::get()
{
	return this;
}

void SelectionCollection::AddText(String^ item)
{
	// -1 avoids Count here
	InsertText(-1, item);
}

void SelectionCollection::Clear()
{
	EditorControl_ECTL_DELETEBLOCK();
}

void SelectionCollection::InsertText(int index, String^ item)
{
	AutoEditorInfo ei;

	if (ei.BlockType == BTYPE_NONE)
		throw gcnew InvalidOperationException(Res::EditorNoSelection);

	EditorGetString egss; EditorControl_ECTL_GETSTRING(egss, ei.BlockStartLine);
	if (ei.BlockType == BTYPE_COLUMN && egss.SelEnd < 0)
		throw gcnew InvalidOperationException(Res::EditorBadSelection);

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
		_editor->Lines(false)->InsertText(ei.BlockStartLine + index, item);
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

	AutoEditorInfo ei2;

	// select inserted
	_editor->SelectText(RegionKind::Stream, egss.SelStart, ei.BlockStartLine, ei2.CurPos - 1, ei2.CurLine);
}

void SelectionCollection::RemoveAt(int index)
{
	AutoEditorInfo ei;

	if (ei.BlockType == BTYPE_NONE)
		throw gcnew InvalidOperationException(Res::EditorNoSelection);

	EditorGetString egss; EditorControl_ECTL_GETSTRING(egss, ei.BlockStartLine);
	if (ei.BlockType == BTYPE_COLUMN && egss.SelEnd < 0)
		throw gcnew InvalidOperationException(Res::EditorBadSelection);

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
		Place ss = ::SelectionPlace();
		++ss.Top;
		ss.Left = 0;
		_editor->SelectText((RegionKind)ei.BlockType, ss.Left, ss.Top, ss.Right, ss.Bottom);

		// remove selected part of line
		ILine^ line = _editor[top];
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
			ILine^ line = _editor[bottom];
			line->Text = line->Text->Substring(egsi.SelEnd);
		}

		// select to the end of previous line
		--bottom;
		ILine^ line = _editor[bottom];
		String^ text = line->Text;
		if (text->Length == 0 && ell)
		{
			// prior line is empty, remove it
			Edit_RemoveAt(bottom);
			return;
		}
		_editor->SelectText(RegionKind::Stream, egss.SelStart, ei.BlockStartLine, text->Length - 1, bottom);
		return;
	}

	// remove inside
	Edit_RemoveAt(bottom);
}

int SelectionCollection::Count::get()
{
	AutoEditorInfo ei;

	if (ei.BlockType == BTYPE_NONE)
		return 0;

	int r = 0;
	EditorGetString egs;
	for(egs.StringNumber = ei.BlockStartLine; egs.StringNumber < ei.TotalLines; ++egs.StringNumber)
	{
		EditorControl_ECTL_GETSTRING(egs, egs.StringNumber);
		if (egs.SelStart < 0)
			break;

		//! empty last line
		if (egs.SelEnd == 0)
		{
			if (!IgnoreEmptyLast)
				++r;
			break;
		}

		// count
		++r;
	}
	return r;
}

}
