#include "StdAfx.h"
#include "VisibleEditorLineCollection.h"
#include "LineListEnumerator.h"
#include "Utils.h"
#include "VisibleEditorLine.h"

namespace FarManagerImpl
{;
VisibleEditorLineCollection::VisibleEditorLineCollection()
{
}

ILine^ VisibleEditorLineCollection::Item::get(int index)
{
	return gcnew VisibleEditorLine(index, false);
}

void VisibleEditorLineCollection::Item::set(int, ILine^)
{
	throw gcnew NotSupportedException();
}

IEnumerator<ILine^>^ VisibleEditorLineCollection::EnumeratorImpl::get()
{
	return gcnew LineListEnumerator(this, 0, Count);
}

int VisibleEditorLineCollection::Count::get()
{
	EditorInfo ei; EditorControl_ECTL_GETINFO(ei);
	return ei.TotalLines;
}

void VisibleEditorLineCollection::Add(String^ item)
{
	// -1 avoids Count here
	Insert(-1, item);
}

// select and delete all text if any
void VisibleEditorLineCollection::Clear()
{
	ILine^ last = Last;
	EditorSelect es;
	es.BlockHeight = last->No + 1;
	es.BlockWidth = last->Text->Length;
	if (es.BlockHeight > 1 || es.BlockWidth > 0)
	{
		es.BlockType = BTYPE_STREAM;
		es.BlockStartLine = 0;
		es.BlockStartPos = 0;
		EditorControl_ECTL_SELECT(es);
		Info.EditorControl(ECTL_DELETEBLOCK, 0);
	}
}

void VisibleEditorLineCollection::Insert(int index, String^ item)
{
	// setup
	const int Count = this->Count;
	if (index < 0)
		index = Count;

	// prepare string
	item = item->Replace("\r\n", "\r")->Replace('\n', '\r');

	// add?
	int y = 0;
	bool newline = true;
	if (index == Count)
	{
		--index;
		ILine^ last = Last;
		y = last->Text->Length;
		if (y == 0)
		{
			newline = false;
			item += "\r";
		}
	}

	// save pos
	EditorInfo ei; EditorControl_ECTL_GETINFO(ei);
	if (index <= ei.CurLine)
	{
		++ei.CurLine;
		for(int i = item->Length; --i >= 0;)
			if (item[i] == '\r')
				++ei.CurLine;
	}

	// go to line, insert new line
	Go(index, y);
	if (newline)
	{
		Info.EditorControl(ECTL_INSERTSTRING, 0);
		if (y == 0)
			Go(index, y);
	}

	// change overtype
	if (ei.Overtype)
	{
		SEditorSetPosition esp;
		esp.Overtype = false;
		Info.EditorControl(ECTL_SETPOSITION, &esp);
	}

	// insert text
	CStr sb(item);
	Info.EditorControl(ECTL_INSERTTEXT, sb);

	// restore overtype
	if (ei.Overtype)
	{
		SEditorSetPosition esp;
		esp.Overtype = true;
		Info.EditorControl(ECTL_SETPOSITION, &esp);
	}

	// restore pos
	SetPos(ei);
}

void VisibleEditorLineCollection::RemoveAt(int index)
{
	if (index < 0)
		throw gcnew ArgumentException("index");
	int count = Count;
	if (index >= count)
		throw gcnew ArgumentOutOfRangeException("index");

	// keep position
	EditorInfo ei; EditorControl_ECTL_GETINFO(ei);

	// last?
	if (index == count - 1)
	{
		// last
		ILine^ last = this[index];

		// remove if not empty
		if (last->Text->Length > 0)
		{
			Go(index);
			RemoveCurrent();
		}

		// go to the end of previous
		if (--index < 0)
			return;
		last = this[index];
		Go(index, last->Text->Length);

		// and delete EOL
		Info.EditorControl(ECTL_DELETECHAR, 0);
	}
	else
	{
		Go(index);
		RemoveCurrent();
	}

	// restore position
	SetPos(ei);
}

void VisibleEditorLineCollection::SetPos(const EditorInfo& ei)
{
	SEditorSetPosition esp;
	esp.CurLine = ei.CurLine;
	esp.CurPos = ei.CurPos;
	esp.TopScreenLine = ei.TopScreenLine;
	esp.LeftPos = ei.LeftPos;
	Info.EditorControl(ECTL_SETPOSITION, &esp);
}

void VisibleEditorLineCollection::Go(int no, int pos)
{
	SEditorSetPosition esp;
	esp.CurLine = no;
	esp.CurPos = pos;
	Info.EditorControl(ECTL_SETPOSITION, &esp);
}

void VisibleEditorLineCollection::Go(int no)
{
	Go(no, 0);
}

void VisibleEditorLineCollection::RemoveCurrent()
{
	Info.EditorControl(ECTL_DELETESTRING, 0);
}
}
