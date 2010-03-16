/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "EditorLineCollection.h"
#include "EditorLine.h"
#include "Wrappers.h"

namespace FarNet
{;
EditorLineCollection::EditorLineCollection(bool trueLines)
: _trueLines(trueLines)
{
	_strings = gcnew EditorStringCollection(this, false);
}

bool EditorLineCollection::IsFixedSize::get()
{
	return false;
}

bool EditorLineCollection::IsReadOnly::get()
{
    return false;
}

bool EditorLineCollection::IsSynchronized::get()
{
    return false;
}

ILine^ EditorLineCollection::default::get(int index)
{
    return gcnew EditorLine(index, false);
}

ILine^ EditorLineCollection::First::get()
{
    return this[0];
}

ILine^ EditorLineCollection::Last::get()
{
    return this[Count - 1];
}

int EditorLineCollection::Count::get()
{
	AutoEditorInfo ei;

	if (_trueLines)
	{
		//! mind recursion, e.g. `Last uses `Count
		EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, ei.TotalLines - 1);
		if (egs.StringLength == 0)
			--ei.TotalLines;
	}

	return ei.TotalLines;
}

IStrings^ EditorLineCollection::Strings::get()
{
    return _strings;
}

bool EditorLineCollection::Remove(ILine^ item)
{
    if (item == nullptr) throw gcnew ArgumentNullException("item");
    RemoveAt(item->No);
    return true;
}

IEnumerator<ILine^>^ EditorLineCollection::GetEnumerator()
{
    return gcnew LineListEnumerator(this, 0, Count);
}

void EditorLineCollection::Add(ILine^ item)
{
    if (item == nullptr) throw gcnew ArgumentNullException("item");
    Add(item->Text);
}

void EditorLineCollection::Add(String^ item)
{
	// -1 avoids Count here
	Insert(-1, item);
}

void EditorLineCollection::Clear()
{
	Edit_Clear();
}

void EditorLineCollection::Insert(int index, ILine^ item)
{
    if (item == nullptr) throw gcnew ArgumentNullException("item");
    Insert(index, item->Text);
}

void EditorLineCollection::Insert(int index, String^ item)
{
    if (item == nullptr) throw gcnew ArgumentNullException("item");

	// setup
	const int Count = this->Count;
	if (index < 0)
		index = Count;

	// prepare string
	item = item->Replace(CV::CRLF, CV::CR)->Replace('\n', '\r');

	// add?
	int len = 0;
	bool newline = true;
	if (index == Count)
	{
		--index;
		ILine^ last = Last;
		len = last->Text->Length;
		if (len == 0)
		{
			newline = false;
			item += CV::CR;
		}
	}

	AutoEditorInfo ei;

	// save pos
	if (index <= ei.CurLine)
	{
		++ei.CurLine;
		for each(Char c in item)
			if (c == '\r')
				++ei.CurLine;
	}

	// go to line, insert new line
	Edit_GoTo(len, index);
	if (newline)
	{
		EditorControl_ECTL_INSERTSTRING(false);
		if (len == 0)
			Edit_GoTo(0, index);
	}

	// insert text
	EditorControl_ECTL_INSERTTEXT(item, ei.Overtype);

	// restore
	Edit_RestoreEditorInfo(ei);
}

System::Collections::IEnumerator^ EditorLineCollection::GetEnumerator2()
{
    return gcnew LineListEnumerator(this, 0, Count);
}

void EditorLineCollection::RemoveAt(int index)
{
	if (index < 0) throw gcnew ArgumentException("'index' must not be negative.");
	int count = Count;
	if (index >= count) throw gcnew ArgumentOutOfRangeException("index");

	// keep position
	AutoEditorInfo ei;

	// last?
	if (index == count - 1)
	{
		// last
		ILine^ last = this[index];

		// remove if not empty
		if (last->Text->Length > 0)
		{
			Edit_GoTo(0, index);
			EditorControl_ECTL_DELETESTRING();
		}

		// go to the end of previous
		if (--index < 0)
			return;
		last = this[index];
		Edit_GoTo(last->Text->Length, index);

		// and delete EOL
		EditorControl_ECTL_DELETECHAR();
	}
	else
	{
		Edit_GoTo(0, index);
		EditorControl_ECTL_DELETESTRING();
	}

	// restore
	Edit_RestoreEditorInfo(ei);
}

}
