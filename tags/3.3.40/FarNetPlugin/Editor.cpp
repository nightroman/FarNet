/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2008 FAR.NET Team
*/

#include "StdAfx.h"
#include "Editor.h"
#include "EditorHost.h"
#include "Far.h"
#include "SelectionCollection.h"
#include "EditorLine.h"
#include "EditorLineCollection.h"

namespace FarNet
{;
String^ BaseEditor::WordDiv::get()
{
	int length = (int)Info.AdvControl(Info.ModuleNumber, ACTL_GETSYSWORDDIV, 0);
	CBox wd(length);
	Info.AdvControl(Info.ModuleNumber, ACTL_GETSYSWORDDIV, wd);
	return OemToStr(wd);
}

void BaseEditor::WordDiv::set(String^)
{
	throw gcnew NotSupportedException("You may set it only for an editor instance, not globally.");
}

String^ BaseEditor::EditText(String^ text, String^ title)
{
	String^ file = Far::Instance->TempName();
	try
	{
		if (SS(text))
			File::WriteAllText(file, text, Encoding::Default);
		
		IEditor^ edit = Far::Instance->CreateEditor();
		edit->FileName = file;
		edit->DisableHistory = true;
		if (SS(title))
			edit->Title = title;
		edit->Open(OpenMode::Modal);
		
		return File::ReadAllText(file, Encoding::Default);
	}
	finally
	{
		File::Delete(file);
	}
}

Editor::Editor()
: _id(-1)
, _Title(String::Empty)
, _frameStart(-1)
{}

void Editor::Open()
{
	Open(OpenMode::None);
}

void Editor::Open(OpenMode mode)
{
	AssertClosed();

	// strings
	CBox sFileName(_FileName);
	CBox sTitle(_Title);

	// frame
	int nLine = _frameStart.Line >= 0 ? _frameStart.Line + 1 : -1;
	int nPos = _frameStart.Pos >= 0 ? _frameStart.Pos + 1 : -1;

	// from dialog? set modal
	WindowType wt = Far::Instance->GetWindowType(-1);
	if (wt == WindowType::Dialog)
		mode = OpenMode::Modal;

	// flags
	int flags = 0;
	if (_IsNew)
		flags |= EF_CREATENEW;
	if (_EnableSwitch)
		flags |= EF_ENABLE_F6;
	if (_DisableHistory)
		flags |= EF_DISABLEHISTORY;
	switch(_DeleteSource)
	{
	case FarManager::DeleteSource::UnusedFile:
		flags |= EF_DELETEONLYFILEONCLOSE; break;
	case FarManager::DeleteSource::UnusedFolder:
		flags |= EF_DELETEONCLOSE; break;
	}
	switch(mode)
	{
	case OpenMode::None:
		flags |= (EF_NONMODAL | EF_IMMEDIATERETURN); break;
	case OpenMode::Wait:
		flags |= EF_NONMODAL; break;
	}

	// open:
	// - set ID to -1 just in case if it is reopened;
	// - it fires READ event and the host sets the Id;
	// - in any case after this ID = -1 means an error
	_id = -1;
	EditorHost::_editorWaiting = this;
	Info.Editor(sFileName, sTitle, _Window.Left, _Window.Top, _Window.Right, _Window.Bottom, flags, nLine, nPos); //?? test window values, make window settable

	// redraw FAR
	if (wt == WindowType::Dialog)
		Far::Instance->Redraw();

	// Check an error: ID must not be -1, even if it is already closed then ID = -2.
	// Using FAR diagnostics fires false errors, e.g.:
	// Test-CallStack-.ps1 \ s \ type: exit \ enter
	if (_id == -1)
	{
		// - error or a file was already opened in the editor and its window is activated
		Editor^ editor = EditorHost::GetCurrentEditor();
		if (editor)
		{
			String^ fileName1 = Path::GetFullPath(_FileName);
			String^ fileName2 = Path::GetFullPath(editor->_FileName);
			if (Compare(fileName1, fileName2) == 0)
				return;
		}
		throw gcnew OperationCanceledException("Cannot open the file '" + FileName + "'");
	}
}

void Editor::Close()
{
	AssertCurrent();
	if (!Info.EditorControl(ECTL_QUIT, 0))
		throw gcnew OperationCanceledException();
}

DeleteSource Editor::DeleteSource::get()
{
	return _DeleteSource;
}

void Editor::DeleteSource::set(FarManager::DeleteSource value)
{
	_DeleteSource = value;
}

bool Editor::EnableSwitch::get()
{
	return _EnableSwitch;
}

void Editor::EnableSwitch::set(bool value)
{
	AssertClosed();
	_EnableSwitch = value;
}

bool Editor::DisableHistory::get()
{
	return _DisableHistory;
}

void Editor::DisableHistory::set(bool value)
{
	AssertClosed();
	_DisableHistory = value;
}

bool Editor::IsEnd::get()
{
	if (!IsOpened)
		return false;
	EditorInfo ei; CurrentInfo(ei);
	return ei.CurLine == ei.TotalLines - 1;
}

bool Editor::IsLocked::get()
{
	if (!IsOpened)
		return false;
	EditorInfo ei; CurrentInfo(ei);
	return (ei.CurState & ECSTATE_LOCKED) != 0;
}

bool Editor::IsModified::get()
{
	if (!IsOpened)
		return false;
	EditorInfo ei; CurrentInfo(ei);
	return (ei.CurState & ECSTATE_MODIFIED) != 0;
}

bool Editor::IsNew::get()
{
	return _IsNew;
}

void Editor::IsNew::set(bool value)
{
	AssertClosed();
	_IsNew = value;
}

bool Editor::IsOpened::get()
{
	return Id >= 0;
}

bool Editor::IsSaved::get()
{
	if (!IsOpened)
		return false;
	EditorInfo ei; CurrentInfo(ei);
	return (ei.CurState & ECSTATE_SAVED) != 0;
}

bool Editor::Overtype::get()
{
	if (!IsOpened)
		return false;
	EditorInfo ei; CurrentInfo(ei);
	return ei.Overtype == 1;
}

void Editor::Overtype::set(bool value)
{
	Edit_SetOvertype(value);
}

ExpandTabsMode Editor::ExpandTabs::get()
{
	if (!IsOpened)
		return ExpandTabsMode::None;

	EditorInfo ei; CurrentInfo(ei);
	if (ei.Options & EOPT_EXPANDALLTABS)
		return ExpandTabsMode::All;
	if (ei.Options & EOPT_EXPANDONLYNEWTABS)
		return ExpandTabsMode::New;
	return ExpandTabsMode::None;
}

void Editor::ExpandTabs::set(ExpandTabsMode value)
{
	AssertCurrent();
	EditorSetParameter esp;
	esp.Type = ESPT_EXPANDTABS;
	esp.Param.iParam = (int)value;
	EditorControl_ECTL_SETPARAM(esp);
}

ILine^ Editor::CurrentLine::get()
{
	if (!IsOpened)
		return nullptr;
	return gcnew EditorLine(-1, false);
}

ILines^ Editor::Lines::get()
{
	if (!IsOpened)
		return nullptr;
	return gcnew EditorLineCollection(false);
}

ILines^ Editor::TrueLines::get()
{
	if (!IsOpened)
		return nullptr;
	return gcnew EditorLineCollection(true);
}

int Editor::Id::get()
{
	return _id;
}

int Editor::TabSize::get()
{
	if (!IsOpened)
		return 0;
	EditorInfo ei; CurrentInfo(ei);
	return ei.TabSize;
}

void Editor::TabSize::set(int value)
{
	if (value <= 0) throw gcnew ArgumentException("'value' must be positive.");
	AssertCurrent();
	EditorSetParameter esp;
	esp.Type = ESPT_TABSIZE;
	esp.Param.iParam = value;
	EditorControl_ECTL_SETPARAM(esp);
}

String^ Editor::FileName::get()
{
	return _FileName;
}

void Editor::FileName::set(String^ value)
{
	AssertClosed();
	_FileName = value;
}

Point Editor::WindowSize::get()
{
	EditorInfo ei; EditorControl_ECTL_GETINFO(ei, true);
	Point r;
	if (ei.EditorID >= 0 && ei.EditorID == _id)
	{
		r.X = ei.WindowSizeX;
		r.Y = ei.WindowSizeY;
	}
	return r;
}

String^ Editor::Title::get()
{
	return _Title;
}

void Editor::Title::set(String^ value)
{
	if (IsOpened)
	{
		AssertCurrent();
		CBox sValue(value);
		Info.EditorControl(ECTL_SETTITLE, sValue);
	}
	else
	{
		_Title = value;
	}
}

Place Editor::Window::get()
{
	return _Window;
}

void Editor::Window::set(Place value)
{
	AssertClosed();
	_Window = value;
}

ISelection^ Editor::Selection::get()
{
	if (!IsOpened)
		return nullptr;
	return gcnew SelectionCollection(this, false);
}

ISelection^ Editor::TrueSelection::get()
{
	if (!IsOpened)
		return nullptr;
	return gcnew SelectionCollection(this, true);
}

Point Editor::Cursor::get()
{
	TextFrame f = Frame;
	return Point(f.Pos, f.Line);
}

void Editor::Insert(String^ text)
{
	AssertCurrent();
	EditorControl_ECTL_INSERTTEXT(text, -1);
}

void Editor::Redraw()
{
	AssertCurrent();
	Info.EditorControl(ECTL_REDRAW, 0);
}

void Editor::DeleteChar()
{
	AssertCurrent();
	EditorControl_ECTL_DELETECHAR();
}

void Editor::DeleteLine()
{
	AssertCurrent();
	EditorControl_ECTL_DELETESTRING();
}

void Editor::Save()
{
	AssertCurrent();
	if (!Info.EditorControl(ECTL_SAVEFILE, 0))
		throw gcnew OperationCanceledException("Can't save the editor file.");
}

void Editor::Save(String^ fileName)
{
	if (fileName == nullptr)
		return Save();
	AssertCurrent();
	CBox sFileName(fileName);
	EditorSaveFile esf;
	memset(&esf, 0, NM);
	strncpy_s(esf.FileName, NM, sFileName, NM);
	esf.FileEOL = 0;
	if (!Info.EditorControl(ECTL_SAVEFILE, &esf))
		throw gcnew OperationCanceledException("Can't save the editor file as: " + fileName);
}

void Editor::InsertLine()
{
	InsertLine(false);
}

void Editor::InsertLine(bool indent)
{
	AssertCurrent();
	EditorControl_ECTL_INSERTSTRING(indent);
}

void Editor::AssertClosed()
{
	if (IsOpened) throw gcnew InvalidOperationException("This editor must not be open.");
}

void Editor::AssertCurrent()
{
	EditorInfo ei;
	CurrentInfo(ei);
}

void Editor::CurrentInfo(EditorInfo& ei)
{
	if (!IsOpened) throw gcnew InvalidOperationException("This editor is not opened.");
	EditorControl_ECTL_GETINFO(ei, true);
	if (ei.EditorID < 0) throw gcnew InvalidOperationException("There is no current editor.");
	if (ei.EditorID != _id) throw gcnew InvalidOperationException("This editor is not the current.");
}

String^ Editor::WordDiv::get()
{
	if (!IsOpened)
		return String::Empty;
	AssertCurrent();
	EditorSetParameter esp;
	esp.Type = ESPT_GETWORDDIV;
	char s[257];
	esp.Param.cParam = s;
	EditorControl_ECTL_SETPARAM(esp);
	return OemToStr(s);
}

void Editor::WordDiv::set(String^ value)
{
	if (value == nullptr)
		throw gcnew ArgumentNullException("value");
	AssertCurrent();
	EditorSetParameter esp;
	CBox sValue(value);
	esp.Type = ESPT_SETWORDDIV;
	esp.Param.cParam = sValue;
	EditorControl_ECTL_SETPARAM(esp);
}

TextFrame Editor::Frame::get()
{
	if (!IsOpened)
		return _frameStart;

	if (_fastGetString > 0)
		return _frameSaved;

	EditorInfo ei;
	EditorControl_ECTL_GETINFO(ei);
	TextFrame r;
	r.Line = ei.CurLine;
	r.Pos = ei.CurPos;
	r.TabPos = ei.CurTabPos;
	r.TopLine = ei.TopScreenLine;
	r.LeftPos = ei.LeftPos;
	return r;
}

void Editor::Frame::set(TextFrame value)
{
	if (!IsOpened)
	{
		_frameStart = value;
		return;
	}

    SEditorSetPosition esp;
	if (value.Line >= 0)
		esp.CurLine = value.Line;
	if (value.Pos >= 0)
		esp.CurPos = value.Pos;
	if (value.TabPos >= 0)
		esp.CurTabPos = value.TabPos;
	if (value.TopLine >= 0)
		esp.TopScreenLine = value.TopLine;
	if (value.LeftPos >= 0)
		esp.LeftPos = value.LeftPos;
    EditorControl_ECTL_SETPOSITION(esp);

	if (_fastGetString > 0)
		_frameSaved = Frame;
}

ICollection<TextFrame>^ Editor::Bookmarks()
{
	EditorInfo ei; CurrentInfo(ei);

	List<TextFrame>^ r = gcnew List<TextFrame>();
	if (ei.BookMarkCount > 0)
	{
		EditorBookMarks ebm;
		ebm.Cursor = new long[ei.BookMarkCount];
		ebm.LeftPos = new long[ei.BookMarkCount];
		ebm.Line = new long[ei.BookMarkCount];
		ebm.ScreenLine = new long[ei.BookMarkCount];
		EditorControl_ECTL_GETBOOKMARKS(ebm);

		r->Capacity = ei.BookMarkCount;
		for(int i = 0; i < ei.BookMarkCount; ++i)
		{
			TextFrame f;
			f.Line = ebm.Line[i];
			f.Pos = ebm.Cursor[i];
			f.TabPos = -1;
			f.TopLine = f.Line - ebm.ScreenLine[i];
			f.LeftPos = ebm.LeftPos[i];
			r->Add(f);
		}

		delete ebm.Cursor;
		delete ebm.LeftPos;
		delete ebm.Line;
		delete ebm.ScreenLine;
	}

	return r;
}

int Editor::ConvertPosToTab(int line, int pos)
{
	EditorConvertPos ecp;
	ecp.StringNumber = line;
	ecp.SrcPos = pos;
	Info.EditorControl(ECTL_REALTOTAB, &ecp);
	return ecp.DestPos;
}

int Editor::ConvertTabToPos(int line, int tab)
{
	EditorConvertPos ecp;
	ecp.StringNumber = line;
	ecp.SrcPos = tab;
	Info.EditorControl(ECTL_TABTOREAL, &ecp);
	return ecp.DestPos;
}

Point Editor::ConvertScreenToCursor(Point screen)
{
	TextFrame f = Frame;
	screen.Y += f.TopLine - 1;
	screen.X = ConvertTabToPos(screen.Y, screen.X) + f.LeftPos;
	return screen;
}

void Editor::Begin()
{
	if (_fastGetString > 0)
	{
		++_fastGetString;
		return;
	}

	_frameSaved = Frame;
	_fastGetString = 1;
}

void Editor::End()
{
	if (_fastGetString == 1)
		Frame = _frameSaved;
	if (--_fastGetString < 0)
		_fastGetString = 0;
}

void Editor::GoTo(int pos, int line)
{
	TextFrame f(-1);
	f.Pos = pos;
	f.Line = line;
	Frame = f;
}

void Editor::GoToLine(int line)
{
	TextFrame f(-1);
	f.Line = line;
	Frame = f;
}

void Editor::GoToPos(int pos)
{
	TextFrame f(-1);
	f.Pos = pos;
	Frame = f;
}

void Editor::GoEnd(bool addLine)
{
	EditorInfo ei; CurrentInfo(ei);
	if (ei.CurLine != ei.TotalLines - 1)
	{
		SEditorSetPosition esp;
		esp.CurPos = 0;
		esp.CurLine = ei.TotalLines - 1;
		EditorControl_ECTL_SETPOSITION(esp);
	}
	EditorGetString egs;
	EditorControl_ECTL_GETSTRING(egs, -1);
	if (egs.StringLength > 0)
	{
		SEditorSetPosition esp;
		esp.CurPos = egs.StringLength;
		EditorControl_ECTL_SETPOSITION(esp);
		if (addLine)
			InsertLine();
	}
}

String^ Editor::GetText(String^ separator)
{
	StringBuilder sb;
	if (separator == nullptr)
		separator = CV::CRLF;

    EditorInfo ei; EditorControl_ECTL_GETINFO(ei);
   	EditorGetString egs; egs.StringNumber = -1;
	SEditorSetPosition esp;
	for(esp.CurLine = 0; esp.CurLine < ei.TotalLines; ++esp.CurLine)
    {
        EditorControl_ECTL_SETPOSITION(esp);
        Info.EditorControl(ECTL_GETSTRING, &egs);
		if (esp.CurLine > 0)
			sb.Append(separator);
		if (egs.StringLength > 0)
			sb.Append(FromEditor(egs.StringText,  egs.StringLength));
    }
    Edit_RestoreEditorInfo(ei);

	return sb.ToString();
}

void Editor::SetText(String^ text)
{
	// info
	EditorInfo ei; CurrentInfo(ei);

	// case: empty
	if (String::IsNullOrEmpty(text))
	{
		Edit_Clear();
		return;
	}

	// workaround: Watch-Output-.ps1, missed the first empty line of the first output
	if (ei.TotalLines == 1 && ei.CurPos == 0 && _IsNew)
	{
		EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, 0);
		if (egs.StringLength == 0)
			EditorControl_ECTL_INSERTSTRING(false);
		EditorControl_ECTL_GETINFO(ei);
	}

	// split: the fact: this way is much faster than clear\insert all text
	array<String^>^ newLines = Regex::Split(text, "\\r\\n|[\\r\\n]");

	const bool overtype = ei.Overtype != 0;
	try
	{
		if (overtype)
			Edit_SetOvertype(false);

		// replace existing lines
		int i;
		ILines^ lines = Lines;
		for(i = 0; i < newLines->Length; ++i)
		{
			if (i < ei.TotalLines)
			{
				lines[i]->Text = newLines[i];
				continue;
			}

			GoEnd(false);
			while(i < newLines->Length)
			{
				EditorControl_ECTL_INSERTSTRING(false);
				EditorControl_ECTL_INSERTTEXT(newLines[i], -1);
				++i;
			}
			return;
		}

		// kill the rest of text (only if any, don't touch selection!)
		--i;
		ILine^ last = lines->Last;
		if (i < last->No)
		{
			ISelection^ select = Selection;
			select->Select(SelectionType::Stream, newLines[i]->Length, i, last->Length, last->No);
			select->Clear();
		}

		// empty last line is not deleted
		EditorControl_ECTL_GETINFO(ei);
		if (ei.TotalLines > newLines->Length)
			lines->RemoveAt(ei.TotalLines - 1);
	}
	finally
	{
		if (overtype)
			Edit_SetOvertype(true);
	}
}

}
