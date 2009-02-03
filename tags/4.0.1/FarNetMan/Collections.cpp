/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#include "StdAfx.h"
#include "Collections.h"
#include "Far.h"

namespace FarNet
{;
EditorStringCollection::EditorStringCollection(ILines^ lines, bool selected)
: _lines(lines)
, _selected(selected)
{}

String^ EditorStringCollection::default::get(int index)
{
	if (_selected)
		return _lines[index]->Selection->Text;
	else
		return _lines[index]->Text;
}

void EditorStringCollection::default::set(int index, String^ value)
{
	if (_selected)
		_lines[index]->Selection->Text = value;
	else
		_lines[index]->Text = value;
}

void EditorStringCollection::CopyTo(array<String^>^ arrayObject, int arrayIndex)
{
	if (arrayObject == nullptr)
		throw gcnew ArgumentNullException("arrayObject");
	if (arrayIndex < 0)
		throw gcnew ArgumentOutOfRangeException("arrayIndex");
	if (arrayObject->Length - arrayIndex > Count)
		throw gcnew ArgumentException("arrayObject, arrayIndex");
	for each(String^ s in this)
		arrayObject[++arrayIndex] = s;
}
}
