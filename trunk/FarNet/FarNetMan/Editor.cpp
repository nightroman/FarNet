/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#include "StdAfx.h"
#include "Editor.h"
#include "EditorHost.h"
#include "Far.h"
#include "SelectionCollection.h"
#include "EditorLine.h"
#include "EditorLineCollection.h"
#include "Wrappers.h"

namespace FarNet
{;
//$RVK move
ref class EditorTextWriter : TextWriter
{
	IEditor^ _editor;
internal:
	EditorTextWriter(IEditor^ editor);
public:
	virtual void Write(Char value) override;
	virtual void Write(String^ value) override;
	virtual property System::Text::Encoding^ Encoding { System::Text::Encoding ^ get() override { return System::Text::Encoding::Unicode; } }
};

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

Editor::Editor()
: _id(-1)
, _Title(String::Empty)
, _CodePage(CP_AUTODETECT)
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
	PIN_ES(pinFileName, _FileName);
	PIN_ES(pinTitle, _Title);

	// frame
	int nLine = _frameStart.Line >= 0 ? _frameStart.Line + 1 : -1;
	int nPos = _frameStart.Pos >= 0 ? _frameStart.Pos + 1 : -1;

	// from dialog? set modal
	WindowType wt = Far::Instance->WindowType;
	if (wt == WindowType::Dialog)
		mode = OpenMode::Modal;

	// flags
	int flags = 0;

	if (_IsNew)
		flags |= EF_CREATENEW;

	if (_DisableHistory)
		flags |= EF_DISABLEHISTORY;

	switch(_Switching)
	{
	case FarNet::Switching::Enabled:
		flags |= VF_ENABLE_F6;
		break;
	case FarNet::Switching::Auto:
		if (_DeleteSource == FarNet::DeleteSource::None &&
			_Closed == nullptr &&
			_GotFocus == nullptr &&
			_LosingFocus == nullptr &&
			_Opened == nullptr &&
			_Saving == nullptr &&
			_OnKey == nullptr &&
			_OnMouse == nullptr &&
			_OnRedraw == nullptr)
			flags |= VF_ENABLE_F6;
		break;
	}

	switch(_DeleteSource)
	{
	case FarNet::DeleteSource::UnusedFile:
		flags |= EF_DELETEONLYFILEONCLOSE; break;
	case FarNet::DeleteSource::UnusedFolder:
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
	Info.Editor(
		pinFileName,
		pinTitle,
		_Window.Left,
		_Window.Top,
		_Window.Right,
		_Window.Bottom,
		flags,
		nLine,
		nPos,
		_CodePage); //?? test window values, make window settable

	// redraw Far
	if (wt == WindowType::Dialog)
		Far::Instance->Redraw();

	//! Check errors: ID must not be -1 (even if it is already closed then ID = -2).
	//! Using Far diagnostics fires false errors, e.g.:
	//! Test-CallStack-.ps1 \ s \ type: exit \ enter
	if (_id == -1)
	{
		// - error or a file was already opened in the editor and its window is activated
		Editor^ editor = EditorHost::GetCurrentEditor();
		if (editor)
		{
			String^ fileName1 = Path::GetFullPath(_FileName);
			String^ fileName2 = Path::GetFullPath(editor->_FileName);
			if (Compare(fileName1, fileName2) == 0)
			{
				// goto?
				if (nLine >= 0 || nPos >= 0)
					editor->GoTo(_frameStart.Pos, _frameStart.Line);
				return;
			}
		}
		throw gcnew OperationCanceledException("Cannot open the file '" + (FileName ? FileName : "<null>") + "'");
	}
}

void Editor::Close()
{
	if (!Info.EditorControl(ECTL_QUIT, 0))
		throw gcnew OperationCanceledException;
}

DeleteSource Editor::DeleteSource::get()
{
	return _DeleteSource;
}

void Editor::DeleteSource::set(FarNet::DeleteSource value)
{
	_DeleteSource = value;
}

Switching Editor::Switching::get()
{
	return _Switching;
}

void Editor::Switching::set(FarNet::Switching value)
{
	AssertClosed();
	_Switching = value;
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

	AutoEditorInfo ei;

	return ei.CurLine == ei.TotalLines - 1;
}

bool Editor::IsLocked::get()
{
	if (!IsOpened)
		return false;

	AutoEditorInfo ei;

	return (ei.CurState & ECSTATE_LOCKED) != 0;
}

bool Editor::IsModified::get()
{
	if (!IsOpened)
		return false;

	AutoEditorInfo ei;

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

	AutoEditorInfo ei;

	return (ei.CurState & ECSTATE_SAVED) != 0;
}

bool Editor::Overtype::get()
{
	if (!IsOpened)
		return false;

	AutoEditorInfo ei;

	return ei.Overtype == 1;
}

void Editor::Overtype::set(bool value)
{
	Edit_SetOvertype(value);
}

int Editor::CodePage::get()
{
	if (!IsOpened)
		return _CodePage;

	AutoEditorInfo ei;

	return ei.CodePage;
}

void Editor::CodePage::set(int value)
{
	AssertClosed();

	_CodePage = value;
}

ExpandTabsMode Editor::ExpandTabs::get()
{
	if (!IsOpened)
		return ExpandTabsMode::None;

	AutoEditorInfo ei;

	if (ei.Options & EOPT_EXPANDALLTABS)
		return ExpandTabsMode::All;
	if (ei.Options & EOPT_EXPANDONLYNEWTABS)
		return ExpandTabsMode::New;
	return ExpandTabsMode::None;
}

void Editor::ExpandTabs::set(ExpandTabsMode value)
{
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

	AutoEditorInfo ei;

	return ei.TabSize;
}

void Editor::TabSize::set(int value)
{
	if (value <= 0)
		throw gcnew ArgumentException("'value' must be positive.");

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
	AutoEditorInfo ei(true);

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
		PIN_NE(pin, value);
		Info.EditorControl(ECTL_SETTITLE, (wchar_t*)pin);
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

void Editor::Cursor::set(Point value)
{
	GoTo(value.X, value.Y);
}

void Editor::Insert(String^ text)
{
	EditorControl_ECTL_INSERTTEXT(text, -1);
}

void Editor::InsertChar(Char text)
{
	EditorControl_ECTL_INSERTTEXT(text, -1);
}

void Editor::Redraw()
{
	Info.EditorControl(ECTL_REDRAW, 0);
}

void Editor::DeleteChar()
{
	EditorControl_ECTL_DELETECHAR();
}

void Editor::DeleteLine()
{
	EditorControl_ECTL_DELETESTRING();
}

//! 090607 see my post "√рабли с ECTL_SAVEFILE" at forum. ???
void Editor::Save()
{
#if 1
	if (!Info.EditorControl(ECTL_SAVEFILE, 0))
		throw gcnew OperationCanceledException("Cannot save the editor file.");
#else
	AutoEditorInfo ei;

	EditorSaveFile esf;
	memset(&esf, 0, sizeof(EditorSaveFile));

	esf.FileName = ei.FileName;
	esf.CodePage = ei.CodePage;

	if (!Info.EditorControl(ECTL_SAVEFILE, &esf))
		throw gcnew OperationCanceledException("Cannot save the editor file.");
#endif
}

void Editor::Save(String^ fileName)
{
	if (fileName == nullptr)
		return Save();

	PIN_NE(pin, fileName);

	AutoEditorInfo ei;

	EditorSaveFile esf;
	memset(&esf, 0, sizeof(EditorSaveFile));

	esf.FileName = pin;
	esf.CodePage = ei.CodePage;

	if (!Info.EditorControl(ECTL_SAVEFILE, &esf))
		throw gcnew OperationCanceledException("Cannot save the editor file as: " + fileName);
}

void Editor::InsertLine()
{
	InsertLine(false);
}

void Editor::InsertLine(bool indent)
{
	EditorControl_ECTL_INSERTSTRING(indent);
}

void Editor::AssertClosed()
{
	if (IsOpened)
		throw gcnew InvalidOperationException("This editor must not be open.");
}

String^ Editor::WordDiv::get()
{
	if (!IsOpened)
		return String::Empty;

	EditorSetParameter esp;
	esp.Type = ESPT_GETWORDDIV;
	wchar_t buf[257];
	esp.Param.cParam = buf;
	EditorControl_ECTL_SETPARAM(esp);
	return gcnew String(buf);
}

void Editor::WordDiv::set(String^ value)
{
	if (value == nullptr)
		throw gcnew ArgumentNullException("value");

	PIN_NE(pin, value);
	EditorSetParameter esp;
	esp.Type = ESPT_SETWORDDIV;
	esp.Param.cParam = (wchar_t*)pin;
	EditorControl_ECTL_SETPARAM(esp);
}

TextFrame Editor::Frame::get()
{
	if (!IsOpened)
		return _frameStart;

	if (_fastGetString > 0)
		return _frameSaved;

	AutoEditorInfo ei;

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
	AutoEditorInfo ei;

	List<TextFrame>^ r = gcnew List<TextFrame>();
	if (ei.BookMarkCount > 0)
	{
		EditorBookMarks ebm;
		ebm.Cursor = new long[ei.BookMarkCount];
		ebm.LeftPos = new long[ei.BookMarkCount];
		ebm.Line = new long[ei.BookMarkCount];
		ebm.ScreenLine = new long[ei.BookMarkCount];
		try
		{
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
		}
		finally
		{
			delete ebm.Cursor;
			delete ebm.LeftPos;
			delete ebm.Line;
			delete ebm.ScreenLine;
		}
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
	AutoEditorInfo ei;

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

    AutoEditorInfo ei;

   	EditorGetString egs; egs.StringNumber = -1;
	SEditorSetPosition esp;
	for(esp.CurLine = 0; esp.CurLine < ei.TotalLines; ++esp.CurLine)
    {
        EditorControl_ECTL_SETPOSITION(esp);
        Info.EditorControl(ECTL_GETSTRING, &egs);
		if (esp.CurLine > 0)
			sb.Append(separator);
		if (egs.StringLength > 0)
			sb.Append(gcnew String(egs.StringText, 0, egs.StringLength));
    }
    Edit_RestoreEditorInfo(ei);

	return sb.ToString();
}

void Editor::SetText(String^ text)
{
	// case: empty
	if (String::IsNullOrEmpty(text))
	{
		Edit_Clear();
		return;
	}

	AutoEditorInfo ei;

	// workaround: Watch-Output-.ps1, missed the first empty line of the first output;
	// 090617 disabled this workaround because I cannot see any problem (I do not remember what it was)
#if 0
	if (ei.TotalLines == 1 && ei.CurPos == 0 && _IsNew)
	{
		EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, 0);
		if (egs.StringLength == 0)
			EditorControl_ECTL_INSERTSTRING(false);
		
		ei.Update();
	}
#endif

	// split: the fact: this way is much faster than clear\insert all text
	array<String^>^ newLines = Regex::Split(text, "\\r\\n|[\\r\\n]");

	const bool overtype = ei.Overtype != 0;
	try
	{
		BeginUndo();

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

		ei.Update();

		// empty last line is not deleted
		if (ei.TotalLines > newLines->Length)
			lines->RemoveAt(ei.TotalLines - 1);
	}
	finally
	{
		if (overtype)
			Edit_SetOvertype(true);

		EndUndo();
	}
}

void Editor::BeginUndo()
{
	EditorUndoRedo eur = { EUR_BEGIN };
	Info.EditorControl(ECTL_UNDOREDO, &eur);
}

void Editor::EndUndo()
{
	EditorUndoRedo eur = { EUR_END };
	Info.EditorControl(ECTL_UNDOREDO, &eur);
}

void Editor::Undo()
{
	EditorUndoRedo eur = { EUR_UNDO };
	Info.EditorControl(ECTL_UNDOREDO, &eur);
}

void Editor::Redo()
{
	EditorUndoRedo eur = { EUR_REDO };
	Info.EditorControl(ECTL_UNDOREDO, &eur);
}

TextWriter^ Editor::CreateWriter()
{
	return gcnew EditorTextWriter(this);
}

}
