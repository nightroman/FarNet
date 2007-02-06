#include "StdAfx.h"
#include "SelectionCollection.h"
#include "LineListEnumerator.h"
#include "VisibleEditorLine.h"
#include "Utils.h"

namespace FarManagerImpl
{;
SelectionCollection::SelectionCollection(IEditor^ editor)
: _editor(editor)
{
	_strings = gcnew EditorStringCollection(this, true);
}

bool SelectionCollection::Contains(ILine^)
{
	throw gcnew NotSupportedException();
}

IEnumerator<ILine^>^ SelectionCollection::GetEnumerator()
{
	ITwoPoint^ shape = Shape;
	if (shape == nullptr)
		return gcnew LineListEnumerator(this, 0, 0);
	return gcnew LineListEnumerator(this, 0, Count);
}

Collections::IEnumerator^ SelectionCollection::GetEnumeratorObject()
{
	return GetEnumerator();
}

int SelectionCollection::IndexOf(ILine^)
{
	throw gcnew NotSupportedException();
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

int SelectionCollection::Count::get()
{
	ITwoPoint^ shape = Shape;
	if (shape == nullptr)
		return 0;
	return shape->Height;
}

ITwoPoint^ SelectionCollection::Shape::get()
{
	EditorInfo ei; EditorControl_ECTL_GETINFO(ei);
	if (ei.BlockType == BTYPE_NONE)
		return nullptr;

	ITwoPoint^ r;
	if (ei.BlockType == BTYPE_STREAM)
		r = gcnew Impl::Stream();
	else
		r = gcnew Impl::Rect();

	EditorGetString egs;
	r->Top = ei.BlockStartLine;
	r->Left = -1;
	for(egs.StringNumber = r->Top; Info.EditorControl(ECTL_GETSTRING, &egs); ++egs.StringNumber) // use EditorControl() here, not wrapper
	{
		if (r->Left < 0)
			r->Left = egs.SelStart;
		if (egs.SelStart < 0)
			break;
		r->Right = egs.SelEnd;
	}
	--r->Right;
	r->Bottom = egs.StringNumber - 1;

	return r;
}

void SelectionCollection::Shape::set(ITwoPoint^ value)
{
	if (value == nullptr)
		Unselect();
	else
		Select(
		(dynamic_cast<IRect^>(value) != nullptr ? SelectionType::Rect : SelectionType::Stream),
		value->Left, value->Top, value->Right, value->Bottom);
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
	return gcnew VisibleEditorLine(index + ei.BlockStartLine, true);
}

void SelectionCollection::Item::set(int, ILine^)
{
	throw gcnew NotSupportedException();
}

Object^ SelectionCollection::SyncRoot::get()
{
	return this;
}

String^ SelectionCollection::Text::get()
{
	return _strings->Text;
}

SelectionType SelectionCollection::Type::get()
{
	EditorInfo ei; EditorControl_ECTL_GETINFO(ei);
	return (SelectionType)ei.BlockType;
}

void SelectionCollection::Text::set(String^ value)
{
	EditorInfo ei; EditorControl_ECTL_GETINFO(ei);
	if (ei.BlockType == BTYPE_NONE)
		throw gcnew InvalidOperationException("No selection shape");

	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, ei.BlockStartLine);
	if (ei.BlockType == BTYPE_COLUMN && egs.SelEnd < 0)
		throw gcnew InvalidOperationException("Can't process this selection shape");

	// delete selection
	int top = ei.BlockStartLine;
	int left = egs.SelStart;
	Clear();

	// move cursor to the selection start
	_editor->Cursor->Set(left, top);

	// change overtype
	if (ei.Overtype)
		_editor->Overtype = false;

	// insert
	CStr sb(value->Replace("\r\n", "\r")->Replace('\n', '\r'));
	Info.EditorControl(ECTL_INSERTTEXT, sb);

	// restore overtype
	if (ei.Overtype)
		_editor->Overtype = true;

	// select inserted
	EditorControl_ECTL_GETINFO(ei);
	Select(SelectionType::Stream, left, top, ei.CurPos - 1, ei.CurLine);
}

void SelectionCollection::Add(ILine^)
{
	throw gcnew NotSupportedException();
}

void SelectionCollection::Add(String^ item)
{
	// -1 avoids Count here
	Insert(-1, item);
}

void SelectionCollection::Clear()
{
	Info.EditorControl(ECTL_DELETEBLOCK, 0);
}

void SelectionCollection::CopyTo(array<ILine^>^, int)
{
	throw gcnew NotSupportedException();
}

void SelectionCollection::Insert(int, ILine^)
{
	throw gcnew NotSupportedException();
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
		// case: first incomplete
		if (egss.SelStart > 0)
		{
			// TODO tweak
			Text = item + "\r" + Text;
			return;
		}

		// case: first complete
		_editor->Lines->Insert(ei.BlockStartLine, item);
		return;
	}

	// correct negative index
	if (index < 0)
		index = Count; //TODO tweak

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
	item = item->Replace("\r\n", "\r")->Replace('\n', '\r');
	_editor->Cursor->Set(egsp.SelEnd, ip);

	// change overtype
	if (ei.Overtype)
		_editor->Overtype = false;

	if (egsp.SelEnd == 0)
	{
		// ELL case
		CStr sb(item + "\r");
		Info.EditorControl(ECTL_INSERTTEXT, sb);
	}
	else
	{
		// not ELL case
		Info.EditorControl(ECTL_INSERTSTRING, 0);
		Info.EditorControl(ECTL_DELETECHAR, 0);
		CStr sb(item);
		Info.EditorControl(ECTL_INSERTTEXT, sb);
	}

	// restore overtype
	if (ei.Overtype)
		_editor->Overtype = true;

	// select inserted
	EditorInfo ei2; EditorControl_ECTL_GETINFO(ei2);
	Select(SelectionType::Stream, egss.SelStart, ei.BlockStartLine, ei2.CurPos - 1, ei2.CurLine);
}

bool SelectionCollection::Remove(ILine^)
{
	throw gcnew NotSupportedException();
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
		ITwoPoint^ shape = Shape;
		++shape->Top;
		shape->Left = 0;
		Shape = shape;

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

void SelectionCollection::Select(SelectionType type, int left, int top, int right, int bottom)
{
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
	es.BlockStartLine = top;
	es.BlockStartPos = left;
	es.BlockHeight = bottom - top + 1;
	es.BlockWidth = right - left + 1;
	EditorControl_ECTL_SELECT(es);
}

void SelectionCollection::Unselect()
{
	EditorSelect es;
	es.BlockType = BTYPE_NONE;
	EditorControl_ECTL_SELECT(es);
}
}
