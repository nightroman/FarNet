/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#include "StdAfx.h"
#include "Collections.h"
#include "FarImpl.h"

namespace FarManagerImpl
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
		throw gcnew ArgumentNullException("array");
	if (arrayIndex < 0)
		throw gcnew ArgumentOutOfRangeException("arrayIndex");
	if (arrayObject->Length - arrayIndex > Count)
		throw gcnew ArgumentException("array, arrayIndex");
	for each(String^ s in this)
		arrayObject[++arrayIndex] = s;
}
}
