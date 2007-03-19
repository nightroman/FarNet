#include "StdAfx.h"
#include "Collections.h"
#include "FarImpl.h"

namespace FarManagerImpl
{;
String^ EditorStringCollection::Text::get()
{
	StringBuilder sb;
	String^ eol = String::Empty;
	for each(ILine^ line in _lines)
	{
		sb.Append(eol + (_selected ? line->Selection->Text : line->Text));
		eol = line->Eol;
		if (eol->Length == 0)
			eol = CV::CRLF;
	}
	return sb.ToString();
}

void EditorStringCollection::Text::set(String^ value)
{
	if (value == nullptr)
		throw gcnew ArgumentNullException("value");

	if (_selected)
	{
		_lines->Text = value;
		return;
	}

	// case: empty
	if (value->Length == 0)
	{
		Clear();
		return;
	}

	// editor
	//TODO using editor here is dirty
	IEditor^ editor = GetFar()->Editor;

	// info
	EditorInfo ei;
	EditorControl_ECTL_GETINFO(ei);

	// workaround: Watch-Output-.ps1, missed the first empty line of the first output
	if (ei.TotalLines == 1 && ei.CurPos == 0 && editor->IsNew)
	{
		EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, 0);
		if (egs.StringLength == 0)
			editor->InsertLine();
		EditorControl_ECTL_GETINFO(ei);
	}

	// split: the fact: this way is much faster than clear\insert all text
	array<String^>^ newLines = Regex::Split(value, "\\r\\n|[\\r|\\n]");

	const bool overtype = ei.Overtype != 0;
	try
	{
		// off
		if (overtype)
		{
			SEditorSetPosition esp;
			esp.Overtype = 0;
			EditorControl_ECTL_SETPOSITION(esp);
		}

		// replace existing lines
		int i;
		ILines^ lines = editor->Lines;
		for(i = 0; i < newLines->Length; ++i)
		{
			if (i < ei.TotalLines)
			{
				lines[i]->Text = newLines[i];
				continue;
			}

			editor->GoEnd(false);
			while(i < newLines->Length)
			{
				editor->InsertLine();
				editor->Insert(newLines[i]);
				++i;
			}
			return;
		}

		// kill the rest of text (only if any, don't touch selection!)
		--i;
		ILine^ last = lines->Last;
		if (i < last->No)
		{
			editor->Selection->Select(SelectionType::Stream, newLines[i]->Length, i, last->Length, last->No);
			editor->Selection->Clear();
		}

		// empty last line is not deleted
		EditorControl_ECTL_GETINFO(ei);
		if (ei.TotalLines > newLines->Length)
			lines->RemoveAt(ei.TotalLines - 1);
	}
	finally
	{
		if (overtype)
		{
			SEditorSetPosition esp;
			esp.Overtype = 1;
			EditorControl_ECTL_SETPOSITION(esp);
		}
	}
}
}
