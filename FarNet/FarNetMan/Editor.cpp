
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

#include "StdAfx.h"
#include "Editor.h"
#include "Editor0.h"
#include "EditorBookmark.h"
#include "EditorLine.h"
#include "Far0.h"
#include "Wrappers.h"

namespace FarNet
{;
String^ AnyEditor::EditText(String^ text, String^ title)
{
	return Works::EditorTools::EditText(text, title);
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

	if (ValueUserScreen::Get()) //????
	{
		ValueUserScreen::Set(false);
		Far::Net->UI->SaveUserScreen();
	}

	// strings
	PIN_ES(pinFileName, _FileName);
	PIN_ES(pinTitle, _Title);

	// frame
	int nLine = _frameStart.CaretLine >= 0 ? _frameStart.CaretLine + 1 : -1;
	int nPos = _frameStart.CaretColumn >= 0 ? _frameStart.CaretColumn + 1 : -1;

	// from dialog? set modal
	WindowKind wt = Far::Net->Window->Kind;
	if (wt == WindowKind::Dialog)
		mode = OpenMode::Modal;

	// flags
	int flags = 0;

	if (ES(_FileName)) // Far 3.0.2400
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
			_KeyDown == nullptr &&
			_KeyUp == nullptr &&
			_MouseClick == nullptr &&
			_MouseDoubleClick == nullptr &&
			_MouseMove == nullptr &&
			_MouseWheel == nullptr &&
			_Redrawing == nullptr)
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
	Editor0::_editorWaiting = this;
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
	if (wt == WindowKind::Dialog)
		Far::Net->UI->Redraw();

	//! Check errors: ID must not be -1 (even if it is already closed then ID = -2).
	//! Using Far diagnostics fires false errors, e.g.:
	//! Test-CallStack-.ps1 \ s \ type: exit \ enter
	//! SVN tag 4.2.26
	if (_id == -1)
	{
		// - error or a file was already opened in the editor and its window is activated
		Editor^ editor = Editor0::GetCurrentEditor();
		if (editor)
		{
			String^ fileName1 = Path::GetFullPath(_FileName);
			String^ fileName2 = Path::GetFullPath(editor->_FileName);
			if (Compare(fileName1, fileName2) == 0)
			{
				// goto?
				if (nLine >= 0 || nPos >= 0)
					editor->GoTo(_frameStart.CaretColumn, _frameStart.CaretLine);
				return;
			}
		}
		throw gcnew InvalidOperationException("Cannot open the file '" + (FileName ? FileName : "<null>") + "'");
	}
}

void Editor::Close()
{
	if (!Info.EditorControl(-1, ECTL_QUIT, 0, 0))
		throw gcnew InvalidOperationException(__FUNCTION__);
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

bool Editor::IsModified::get()
{
	if (!IsOpened)
		return false;

	AutoEditorInfo ei;

	return (ei.CurState & ECSTATE_MODIFIED) != 0;
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
	if (IsOpened)
	{
		EditorSetParameter esp;
		esp.Type = ESPT_CODEPAGE;
		esp.iParam = value;
		EditorControl_ECTL_SETPARAM(esp);
	}

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
	esp.iParam = (int)value;
	EditorControl_ECTL_SETPARAM(esp);
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
	esp.iParam = value;
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
		Info.EditorControl(-1, ECTL_SETTITLE, 0, (wchar_t*)pin);
	}
	_Title = value;
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

Point Editor::Caret::get()
{
	TextFrame f = Frame;
	return Point(f.CaretColumn, f.CaretLine);
}

void Editor::Caret::set(Point value)
{
	GoTo(value.X, value.Y);
}

void Editor::InsertText(String^ text)
{
	if (!_hMutex)
	{
		EditorControl_ECTL_INSERTTEXT(text, -1);
		return;
	}

	WaitForSingleObject(_hMutex, INFINITE);
	try
	{
		_output->Append(text);
	}
	finally
	{
		ReleaseMutex(_hMutex);
	}
}

void Editor::InsertChar(Char text)
{
	if (!_hMutex)
	{
		EditorControl_ECTL_INSERTTEXT(text, -1);
		return;
	}

	WaitForSingleObject(_hMutex, INFINITE);
	try
	{
		_output->Append(text);
	}
	finally
	{
		ReleaseMutex(_hMutex);
	}
}

void Editor::InsertLine(bool indent)
{
	if (!_hMutex)
	{
		EditorControl_ECTL_INSERTSTRING(indent);
		return;
	}

	WaitForSingleObject(_hMutex, INFINITE);
	try
	{
		_output->Append("\r");
	}
	finally
	{
		ReleaseMutex(_hMutex);
	}
}

void Editor::InsertLine()
{
	InsertLine(false);
}

void Editor::Redraw()
{
	Info.EditorControl(-1, ECTL_REDRAW, 0, 0);
}

void Editor::DeleteChar()
{
	EditorControl_ECTL_DELETECHAR();
}

void Editor::DeleteLine()
{
	EditorControl_ECTL_DELETESTRING();
}

//! 090926 Mantis 921. Fixed in 1142
void Editor::Save()
{
	if (IsSaved)
		return;

	if (!Info.EditorControl(-1, ECTL_SAVEFILE, 0, 0))
		throw gcnew InvalidOperationException("Cannot save the editor file.");
}

void Editor::Save(bool force)
{
	if (!force && IsSaved)
		return;

	if (!Info.EditorControl(-1, ECTL_SAVEFILE, 0, 0))
		throw gcnew InvalidOperationException("Cannot save the editor file.");
}

void Editor::Save(String^ fileName)
{
	if (fileName == nullptr)
		return Save();

	EditorSaveFile esf;
	memset(&esf, 0, sizeof(EditorSaveFile));

	PIN_NE(pin, fileName);
	esf.FileName = pin;

	AutoEditorInfo ei;
	esf.CodePage = ei.CodePage;

	if (!Info.EditorControl(-1, ECTL_SAVEFILE, 0, &esf))
		throw gcnew InvalidOperationException("Cannot save the editor file as: " + fileName);
}

void Editor::AssertClosed()
{
	if (IsOpened)
		throw gcnew InvalidOperationException("This editor must not be open.");
}

TextFrame Editor::Frame::get()
{
	if (!IsOpened)
		return _frameStart;

	AutoEditorInfo ei;

	TextFrame r;
	r.CaretLine = ei.CurLine;
	r.CaretColumn = ei.CurPos;
	r.CaretScreenColumn = ei.CurTabPos;
	r.VisibleLine = ei.TopScreenLine;
	r.VisibleChar = ei.LeftPos;
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
	if (value.CaretLine >= 0)
		esp.CurLine = value.CaretLine;
	if (value.CaretColumn >= 0)
		esp.CurPos = value.CaretColumn;
	if (value.CaretScreenColumn >= 0)
		esp.CurTabPos = value.CaretScreenColumn;
	if (value.VisibleLine >= 0)
		esp.TopScreenLine = value.VisibleLine;
	if (value.VisibleChar >= 0)
		esp.LeftPos = value.VisibleChar;
    EditorControl_ECTL_SETPOSITION(esp);
}

int Editor::ConvertColumnEditorToScreen(int line, int column)
{
	EditorConvertPos ecp;
	ecp.StringNumber = line;
	ecp.SrcPos = column;
	Info.EditorControl(-1, ECTL_REALTOTAB, 0, &ecp);
	return ecp.DestPos;
}

int Editor::ConvertColumnScreenToEditor(int line, int column)
{
	EditorConvertPos ecp;
	ecp.StringNumber = line;
	ecp.SrcPos = column;
	Info.EditorControl(-1, ECTL_TABTOREAL, 0, &ecp);
	return ecp.DestPos;
}

Point Editor::ConvertPointEditorToScreen(Point point)
{
	TextFrame frame = Frame;
	point.X = ConvertColumnEditorToScreen(point.Y, point.X) - frame.VisibleChar;
	point.Y -= frame.VisibleLine - 1;
	return point;
}

Point Editor::ConvertPointScreenToEditor(Point point)
{
	TextFrame frame = Frame;
	point.Y += frame.VisibleLine - 1;
	point.X = ConvertColumnScreenToEditor(point.Y, point.X) + frame.VisibleChar;
	return point;
}

void Editor::GoTo(int column, int line)
{
	TextFrame f(-1);
	f.CaretColumn = column;
	f.CaretLine = line;
	Frame = f;
}

void Editor::GoToLine(int line)
{
	TextFrame f(-1);
	f.CaretLine = line;
	Frame = f;
}

void Editor::GoToColumn(int column)
{
	TextFrame f(-1);
	f.CaretColumn = column;
	Frame = f;
}

void Editor::GoToEnd(bool addLine)
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

   	EditorGetString egs;
	for(egs.StringNumber = 0; egs.StringNumber < ei.TotalLines; ++egs.StringNumber)
    {
        Info.EditorControl(-1, ECTL_GETSTRING, 0, &egs);
		if (egs.StringNumber > 0)
			sb.Append(separator);
		if (egs.StringLength > 0)
			sb.Append(gcnew String(egs.StringText, 0, egs.StringLength));
    }

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

	//_101210_142325 drop selection
	if (ei.BlockType != BTYPE_NONE)
		UnselectText();

	// split: the fact: this way is much faster than clear\insert all text
	array<String^>^ newLines = FarNet::Works::Kit::SplitLines(text);

	const bool overtype = ei.Overtype != 0;
	try
	{
		BeginUndo();

		if (overtype)
			Edit_SetOvertype(false);

		// replace existing lines
		int i;
		for(i = 0; i < newLines->Length; ++i)
		{
			if (i < ei.TotalLines)
			{
				this[i]->Text = newLines[i];
				continue;
			}

			GoToEnd(false);
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
		ILine^ last = this[Count - 1];
		if (i < last->Index)
		{
			SelectText(newLines[i]->Length, i, last->Length, last->Index, PlaceKind::Stream);
			DeleteText();
		}

		ei.Update();

		// empty last line is not deleted
		if (ei.TotalLines > newLines->Length)
			Edit_RemoveAt(ei.TotalLines - 1);
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
	Info.EditorControl(-1, ECTL_UNDOREDO, 0, &eur);
}

void Editor::EndUndo()
{
	EditorUndoRedo eur = { EUR_END };
	Info.EditorControl(-1, ECTL_UNDOREDO, 0, &eur);
}

void Editor::Undo()
{
	EditorUndoRedo eur = { EUR_UNDO };
	Info.EditorControl(-1, ECTL_UNDOREDO, 0, &eur);
}

void Editor::Redo()
{
	EditorUndoRedo eur = { EUR_REDO };
	Info.EditorControl(-1, ECTL_UNDOREDO, 0, &eur);
}

TextWriter^ Editor::OpenWriter()
{
	return gcnew Works::EditorTextWriter(this);
}

void Editor::BeginAsync()
{
	// do throw now, this is a bug to call it twice
	if (_hMutex)
		throw gcnew InvalidOperationException("Asynchronous mode is already started.");

	if (!IsOpened)
		throw gcnew InvalidOperationException("Editor must be opened.");

	_hMutex = CreateMutex(nullptr, FALSE, nullptr);

	BeginUndo();
	_output = gcnew StringBuilder();
}

void Editor::EndAsync()
{
	// do not throw, this is OK to call it twice
	if (!_hMutex)
		return;

	CloseHandle(_hMutex);
	_hMutex = 0;
}

void Editor::Sync()
{
	if (_hMutex)
		WaitForSingleObject(_hMutex, INFINITE);

	try
	{
		if (_output->Length)
		{
			GoToEnd(false);

			EditorControl_ECTL_INSERTTEXT(_output->ToString(), -1);

			Redraw();
		}

		if (_hMutex)
		{
			_output->Length = 0;
		}
		else
		{
			_output = nullptr;
			EndUndo();
		}
	}
	finally
	{
		if (_hMutex)
			ReleaseMutex(_hMutex);
	}
}

String^ Editor::WordDiv::get()
{
	if (!IsOpened)
		return _WordDiv ? _WordDiv : String::Empty;

	EditorSetParameter esp;
	esp.Type = ESPT_GETWORDDIV;
	esp.wszParam = 0;
	esp.Size = EditorControl_ECTL_SETPARAM(esp);

	CBox box(esp.Size);
	esp.wszParam = box;
	EditorControl_ECTL_SETPARAM(esp);

	return gcnew String(box);
}

void Editor::WordDiv::set(String^ value)
{
	if (value == nullptr) throw gcnew ArgumentNullException("value");

	_WordDiv = value;
	if (!IsOpened)
		return;
	
	PIN_NE(pin, value);
	EditorSetParameter esp;
	esp.Type = ESPT_SETWORDDIV;
	esp.wszParam = (wchar_t*)pin;
	EditorControl_ECTL_SETPARAM(esp);
}

bool Editor::IsVirtualSpace::get() { return GetBoolOption(EOPT_CURSORBEYONDEOL, _IsVirtualSpace); }
bool Editor::ShowWhiteSpace::get() { return GetBoolOption(EOPT_SHOWWHITESPACE, _ShowWhiteSpace); }
bool Editor::WriteByteOrderMark::get() { return GetBoolOption(EOPT_BOM, _WriteByteOrderMark); }
bool Editor::GetBoolOption(int option, Nullable<bool> value)
{
	if (!IsOpened)
		return value.HasValue ? value.Value : false;

	AutoEditorInfo ei;

	return (ei.Options & option) != 0;
}

void Editor::IsVirtualSpace::set(bool value)
{
	_IsVirtualSpace = value;
	if (IsOpened)
		SetBoolOption(ESPT_CURSORBEYONDEOL, value);
}
void Editor::ShowWhiteSpace::set(bool value)
{
	_ShowWhiteSpace = value;
	if (IsOpened)
		SetBoolOption(ESPT_SHOWWHITESPACE, value);
}
void Editor::WriteByteOrderMark::set(bool value)
{
	_WriteByteOrderMark = value;
	if (IsOpened)
		SetBoolOption(ESPT_SETBOM, value);
}
void Editor::SetBoolOption(EDITOR_SETPARAMETER_TYPES option, bool value)
{
	EditorSetParameter esp;
	esp.Type = option;
	esp.iParam = (int)value;
	EditorControl_ECTL_SETPARAM(esp);
}

void Editor::Start(const EditorInfo& ei, bool waiting)
{
	CBox fileName(Info.EditorControl(-1, ECTL_GETFILENAME, 0, 0));
	Info.EditorControl(-1, ECTL_GETFILENAME, 0, fileName);

	// set info
	_id = ei.EditorID;
	_TimeOfOpen = DateTime::Now;
	_FileName = gcnew String(fileName);

	// preset waiting runtime properties
	if (waiting)
	{
		//! Property = Field
		if (_WordDiv)
			WordDiv = _WordDiv;
		if (_IsLocked.HasValue)
			IsLocked = _IsLocked.Value;
		if (_IsVirtualSpace.HasValue)
			IsVirtualSpace = _IsVirtualSpace.Value;
		if (_ShowWhiteSpace.HasValue)
			ShowWhiteSpace = _ShowWhiteSpace.Value;
		if (_WriteByteOrderMark.HasValue)
			WriteByteOrderMark = _WriteByteOrderMark.Value;
	}

	// now call the modules
	Far0::InvokeModuleEditors(this, fileName);
}

void Editor::Stop()
{
	_id = -2;
}

String^ Editor::GetSelectedText(String^ separator)
{
	AutoEditorInfo ei;

	if (ei.BlockType == BTYPE_NONE)
		return String::Empty;

	StringBuilder sb;

	if (separator == nullptr)
		separator = CV::CRLF;

	EditorGetString egs;
	for(egs.StringNumber = ei.BlockStartLine; egs.StringNumber < ei.TotalLines; ++egs.StringNumber)
    {
        Info.EditorControl(-1, ECTL_GETSTRING, 0, &egs);
		if (egs.SelStart < 0)
			break;
		if (egs.StringNumber > ei.BlockStartLine)
			sb.Append(separator);
		int len = (egs.SelEnd < 0 ? egs.StringLength : egs.SelEnd) - egs.SelStart;
		if (len > 0)
		{
			// _101210_192119
			if (ei.BlockType != BTYPE_COLUMN || egs.SelStart + len <= egs.StringLength)
			{
				sb.Append(gcnew String(egs.StringText + egs.SelStart, 0, len));
			}
			else
			{
				sb.Append((gcnew String(egs.StringText + egs.SelStart))->PadRight(len));
			}
		}
    }

	return sb.ToString();
}

void Editor::SetSelectedText(String^ text)
{
	AutoEditorInfo ei;

	if (ei.BlockType == BTYPE_NONE)
		throw gcnew InvalidOperationException(Res::EditorNoSelection);

	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, ei.BlockStartLine);
	if (ei.BlockType == BTYPE_COLUMN && egs.SelEnd < 0)
		throw gcnew InvalidOperationException(Res::EditorBadSelection);

	// delete selection
	int top = ei.BlockStartLine;
	int left = egs.SelStart;
	DeleteText();

	// move cursor to the selection start
	GoTo(left, top);

	// insert
	EditorControl_ECTL_INSERTTEXT(text, ei.Overtype);

	// select inserted
	ei.Update();
	SelectText(left, top, ei.CurPos - 1, ei.CurLine, PlaceKind::Stream);
}

void Editor::SelectText(int column1, int line1, int column2, int line2, PlaceKind kind)
{
	// type
	EditorSelect es;
	switch(kind)
	{
	case PlaceKind::None:
		es.BlockType = BTYPE_NONE;
		EditorControl_ECTL_SELECT(es);
		return;
	case PlaceKind::Stream:
		es.BlockType = BTYPE_STREAM;
		break;
	case PlaceKind::Column:
		es.BlockType = BTYPE_COLUMN;
		break;
	default:
		throw gcnew ArgumentException("Unknown selection type");
	}

	// swap
	if (line1 > line2 || line1 == line2 && column1 > column2)
	{
		int t;
		t = column1; column1 = column2; column2 = t;
		t = line1; line1 = line2; line2 = t;
	}

	// go
	es.BlockStartLine = line1;
	es.BlockStartPos = column1;
	es.BlockHeight = line2 - line1 + 1;
	es.BlockWidth = column2 - column1 + 1;
	EditorControl_ECTL_SELECT(es);
}

void Editor::SelectAllText()
{
	AutoEditorInfo ei;
	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, ei.TotalLines - 1);
	SelectText(0, 0, egs.StringLength - 1, ei.TotalLines - 1, PlaceKind::Stream);
}

void Editor::UnselectText()
{
	EditorSelect es;
	es.BlockType = BTYPE_NONE;
	EditorControl_ECTL_SELECT(es);
}

bool Editor::SelectionExists::get()
{
	AutoEditorInfo ei;

	return ei.BlockType != BTYPE_NONE;
}

Place Editor::SelectionPlace::get()
{
	return Edit_SelectionPlace();
}

PlaceKind Editor::SelectionKind::get()
{
	AutoEditorInfo ei;

	return (PlaceKind)ei.BlockType;
}

void Editor::DeleteText()
{
	EditorControl_ECTL_DELETEBLOCK();
}

int Editor::Count::get()
{
	AutoEditorInfo ei;

	return ei.TotalLines;
}

ILine^ Editor::default::get(int index)
{
    return gcnew EditorLine(index);
}

ILine^ Editor::Line::get()
{
    return gcnew EditorLine(-1);
}

void Editor::RemoveAt(int index)
{
	Edit_RemoveAt(index);
}

void Editor::Clear()
{
	Edit_Clear();
}

Point Editor::SelectionPoint::get()
{
	AutoEditorInfo ei;
	if (ei.BlockType == BTYPE_NONE)
		return Point(-1);

	EditorGetString egs;
	EditorControl_ECTL_GETSTRING(egs, ei.BlockStartLine);
	return Point(egs.SelStart, ei.BlockStartLine);
}

void Editor::Add(String^ text)
{
	Insert(-1, text);
}

void Editor::Insert(int line, String^ text)
{
    if (text == nullptr)
		throw gcnew ArgumentNullException("text");

	AutoEditorInfo ei;

	// setup
	const int Count = ei.TotalLines;
	if (line < 0)
		line = Count;

	// prepare text
	text = text->Replace(CV::CRLF, CV::CR)->Replace('\n', '\r');

	// add?
	int len = 0;
	bool newline = true;
	if (line == Count)
	{
		--line;
		ILine^ last = this[line];
		len = last->Text->Length;
		if (len == 0)
		{
			newline = false;
			text += CV::CR;
		}
	}

	// save pos
	if (line <= ei.CurLine)
	{
		++ei.CurLine;
		for each(Char c in text)
			if (c == '\r')
				++ei.CurLine;
	}

	// go to line, insert new line
	Edit_GoTo(len, line);
	if (newline)
	{
		EditorControl_ECTL_INSERTSTRING(false);
		if (len == 0)
			Edit_GoTo(0, line);
	}

	// insert text
	EditorControl_ECTL_INSERTTEXT(text, ei.Overtype);

	// restore
	Edit_RestoreEditorInfo(ei);
}

IList<ILine^>^ Editor::Lines::get()
{
	return IsOpened ? gcnew Works::LineCollection(this, 0, Count) : nullptr;
}

IList<String^>^ Editor::Strings::get()
{
	return IsOpened ? gcnew Works::StringCollection(this) : nullptr;
}

IList<ILine^>^ Editor::SelectedLines::get()
{
	if (!IsOpened)
		return nullptr;

	Place pp = Edit_SelectionPlace();
	if (pp.Top < 0)
		return gcnew Works::LineCollection(this, 0, 0);

	return gcnew Works::LineCollection(this, pp.Top, (pp.Right < 0 ? pp.Height - 1 : pp.Height));
}

IEditorBookmark^ Editor::Bookmark::get()
{
	return %EditorBookmark::Instance;
}

DateTime Editor::TimeOfOpen::get()
{
	return _TimeOfOpen;
}

DateTime Editor::TimeOfSave::get()
{
	return _TimeOfSave;
}

int Editor::KeyCount::get()
{
	return _KeyCount;
}

void Editor::Activate()
{
	int nWindow = Far::Net->Window->Count;
	for(int i = 0; i < nWindow; ++i)
	{
		WindowKind kind = Far::Net->Window->GetKindAt(i);
		if (kind != WindowKind::Editor)
			continue;

		String^ name = Far::Net->Window->GetNameAt(i);
		if (name == _FileName)
		{
			Far::Net->Window->SetCurrentAt(i);
			return;
		}
	}
	throw gcnew InvalidOperationException("Cannot find the window by name.");
}

// STOP: EF_LOCKED is not used, it is not flexible as our flag.
// Case: read only files are opened locked if there is such an option set in settings.
// Compare: Far: no EF_LOCKED still opens locked -- FarNet: IsLocked = false opens not locked.
bool Editor::IsLocked::get()
{
	if (!IsOpened)
		return _IsLocked.HasValue ? _IsLocked.Value : false;

	AutoEditorInfo ei;

	return (ei.CurState & ECSTATE_LOCKED) != 0;
}
void Editor::IsLocked::set(bool value)
{
	_IsLocked = value;
	if (IsOpened)
		SetBoolOption(ESPT_LOCKMODE, value);
}

IList<EditorColorInfo^>^ Editor::GetColors(int line)
{
	::EditorColor ec; ec.StructSize = sizeof(ec);
	ec.StringNumber = line;

	List<EditorColorInfo^>^ spans = gcnew List<EditorColorInfo^>;

	for(ec.ColorItem = 0; Info.EditorControl(-1, ECTL_GETCOLOR, 0, &ec); ++ec.ColorItem)
	{
		spans->Add(gcnew EditorColorInfo(
			line,
			ec.StartPos,
			ec.EndPos + 1,
			(ConsoleColor)ec.Color.ForegroundColor,
			(ConsoleColor)ec.Color.BackgroundColor,
			FromGUID(ec.Owner),
			ec.Priority));
	}
	
	return spans;
}

void Editor::AddColors(Guid owner, int priority, IEnumerable<EditorColor^>^ colors)
{
	::EditorColor ec; ec.StructSize = sizeof(ec);
	ec.ColorItem = 0;
	ec.Priority = priority;
	ec.Flags = 0;
	ec.Color.Flags = FCF_4BITMASK;
	ec.Owner = ToGUID(owner);

	for each(EditorColor^ color in colors)
	{
		ec.StringNumber = color->Line;
		ec.StartPos = color->Start;
		ec.EndPos = color->End - 1;
		ec.Color.BackgroundColor = (COLORREF)color->Background;
		ec.Color.ForegroundColor = (COLORREF)color->Foreground;
		
		Info.EditorControl(-1, ECTL_ADDCOLOR, 0, &ec);
	}
}

void Editor::RemoveColors(Guid owner, int startLine, int endLine)
{
	EditorDeleteColor edc; edc.StructSize = sizeof(edc);
	edc.Owner = ToGUID(owner);
	edc.StartPos = -1;

	for(int line = startLine; line < endLine; ++line)
	{
		edc.StringNumber = line;
		Info.EditorControl(-1, ECTL_DELCOLOR, 0, &edc);
	}
}

void Editor::AddDrawer(IModuleDrawer^ drawer)
{
	if (!_drawers)
		_drawers = gcnew Dictionary<Guid, DrawerInfo^>;
	
	DrawerInfo^ info = gcnew DrawerInfo;
	info->Id = drawer->Id;
	info->Priority = drawer->Priority;
	info->Handler = drawer->CreateHandler();

	_drawers[drawer->Id] = info;
}

void Editor::RemoveDrawer(Guid id)
{
	if (_drawers && _drawers->Remove(id))
		RemoveColors(id, 0, Count);
}

void Editor::InvokeDrawers()
{
	Point size = WindowSize;
	TextFrame frame = Frame;
	int lineCount = Count;

	int startLine = frame.VisibleLine;
	int endLine = Math::Min(startLine + size.Y, lineCount);

	List<EditorColor^> colors;
	Works::LineCollection lines(this, startLine, endLine - startLine);
	ModuleDrawerEventArgs args(%colors, %lines, frame.VisibleChar, frame.VisibleChar + size.X);

	for each(DrawerInfo^ it in _drawers->Values)
	{
		RemoveColors(it->Id, startLine, endLine);

		colors.Clear();
		it->Handler(this, %args);

		AddColors(it->Id, it->Priority, %colors);
	}
}

}
