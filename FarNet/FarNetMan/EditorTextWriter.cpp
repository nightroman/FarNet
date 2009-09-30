/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#include "StdAfx.h"
#include "EditorTextWriter.h"

namespace FarNet
{;
EditorTextWriter::EditorTextWriter(IEditor^ editor) : _editor(editor)
{
	NewLine = "\r";
}

void EditorTextWriter::Write(Char value)
{
	_editor->InsertChar(value);
}

void EditorTextWriter::Write(String^ value)
{
	_editor->Insert(value);
}

}
