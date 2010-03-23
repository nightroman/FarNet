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
SelectionCollection::SelectionCollection(IEditor^ editor)
: _Editor(editor)
{}

IEnumerator<ILine^>^ SelectionCollection::GetEnumerator()
{
	Place pp = Edit_SelectionPlace();
	return Works::EditorTools::Enumerate(_Editor, pp.Top, pp.Top < 0 ? pp.Top : pp.Bottom + 1)->GetEnumerator();
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
	Point p = _Editor->SelectionPoint;
	if (p.Y < 0)
		throw gcnew InvalidOperationException;

	return _Editor->GetLine(p.Y + index, true);
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

int SelectionCollection::Count::get()
{
	Place pp = _Editor->SelectionPlace;
	if (pp.Top < 0)
		return 0;

	return pp.Height;
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
		_Editor->GoTo(egss.SelStart, ei.BlockStartLine);
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
		_Editor->InsertText(ei.BlockStartLine + index, item);
		return;
	}

	// case: add (prior is actually the last)
	_Editor->GoTo(egsp.SelEnd, ip);

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
	_Editor->SelectText(RegionKind::Stream, egss.SelStart, ei.BlockStartLine, ei2.CurPos - 1, ei2.CurLine);
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
		Place pp = Edit_SelectionPlace();
		++pp.Top;
		pp.Left = 0;
		_Editor->SelectText((RegionKind)ei.BlockType, pp.Left, pp.Top, pp.Right, pp.Bottom);

		// remove selected part of line
		ILine^ line = _Editor[top];
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
			ILine^ line = _Editor[bottom];
			line->Text = line->Text->Substring(egsi.SelEnd);
		}

		// select to the end of previous line
		--bottom;
		ILine^ line = _Editor[bottom];
		String^ text = line->Text;
		if (text->Length == 0 && ell)
		{
			// prior line is empty, remove it
			Edit_RemoveAt(bottom);
			return;
		}
		_Editor->SelectText(RegionKind::Stream, egss.SelStart, ei.BlockStartLine, text->Length - 1, bottom);
		return;
	}

	// remove inside
	_Editor->RemoveAt(bottom);
}

}
