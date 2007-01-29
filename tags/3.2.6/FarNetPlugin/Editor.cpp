#include "StdAfx.h"
#include "Editor.h"
#include "EditorManager.h"
#include "SelectionCollection.h"
#include "Utils.h"
#include "VisibleEditorCursor.h"
#include "VisibleEditorLineCollection.h"

namespace FarManagerImpl
{;
Editor::Editor(EditorManager^ manager)
{
	_manager = manager;
	_id = -1;
	_title = String::Empty;
	_window = gcnew Rect();
	_lines = gcnew VisibleEditorLineCollection();
	_selection = gcnew SelectionCollection(this);
	_openedCursor = gcnew VisibleEditorCursor();
	_storedCursor = gcnew StoredEditorCursor();
	_cursor = gcnew ProxyEditorCursor();
	_cursor->Impl = _storedCursor;
}

void Editor::Open()
{
	EnsureClosed();

	CStr sFileName(FileName);
	CStr sTitle(Title);
	int nLine = _cursor->Line >= 0 ? _cursor->Line + 1 : -1;
	int nPos = _cursor->Pos >= 0 ? _cursor->Pos + 1 : -1;

	// it is used by the manager on READ event
	_manager->Wait(this);

	// it fires READ event and the manager sets the Id
	int res = Info.Editor(
		sFileName, sTitle,
		_window->Left, _window->Top, _window->Right, _window->Bottom,
		Flags(), nLine, nPos);

	// check errors
	if (res != EEC_MODIFIED && res != EEC_NOT_MODIFIED)
		throw gcnew OperationCanceledException("Can't open file: " + FileName);
}

void Editor::Close()
{
	EnsureCurrent();
	if (!Info.EditorControl(ECTL_QUIT, 0))
		throw gcnew OperationCanceledException();
}

bool Editor::Async::get()
{
	return _async;
}

void Editor::Async::set(bool value)
{
	EnsureClosed();
	_async = value;
}

bool Editor::DeleteOnClose::get()
{
	return _deleteOnClose;
}

void Editor::DeleteOnClose::set(bool value)
{
	EnsureClosed();
	_deleteOnClose = value;
}

bool Editor::DeleteOnlyFileOnClose::get()
{
	return _deleteOnlyFileOnClose;
}

void Editor::DeleteOnlyFileOnClose::set(bool value)
{
	EnsureClosed();
	_deleteOnlyFileOnClose = value;
}

bool Editor::EnableSwitch::get()
{
	return _enableSwitch;
}

void Editor::EnableSwitch::set(bool value)
{
	EnsureClosed();
	_enableSwitch = value;
}

bool Editor::DisableHistory::get()
{
	return _disableHistory;
}

void Editor::DisableHistory::set(bool value)
{
	EnsureClosed();
	_disableHistory = value;
}

bool Editor::IsLocked::get()
{
	EditorInfo ei; EnsureCurrent(ei);
	return (ei.CurState & ECSTATE_LOCKED) != 0;
}

bool Editor::IsModal::get()
{
	return _isModal;
}

void Editor::IsModal::set(bool value)
{
	EnsureClosed();
	_isModal = value;
}

bool Editor::IsModified::get()
{
	EditorInfo ei; EnsureCurrent(ei);
	return (ei.CurState & ECSTATE_MODIFIED) != 0;
}

bool Editor::IsNew::get()
{
	return _isNew;
}

void Editor::IsNew::set(bool value)
{
	EnsureClosed();
	_isNew = value;
}

bool Editor::IsOpened::get()
{
	return Id != -1;
}

bool Editor::IsSaved::get()
{
	EditorInfo ei; EnsureCurrent(ei);
	return (ei.CurState & ECSTATE_SAVED) != 0;
}

bool Editor::Overtype::get()
{
	EditorInfo ei; EnsureCurrent(ei);
	return ei.Overtype == 1;
}

void Editor::Overtype::set(bool value)
{
	SEditorSetPosition esp;
	esp.Overtype = value;
	Info.EditorControl(ECTL_SETPOSITION, &esp);
}

ExpandTabsMode Editor::ExpandTabs::get()
{
	if (!IsOpened)
		return ExpandTabsMode::None;

	EditorInfo ei; EnsureCurrent(ei);
	if (ei.Options & EOPT_EXPANDALLTABS)
		return ExpandTabsMode::All;
	if (ei.Options & EOPT_EXPANDONLYNEWTABS)
		return ExpandTabsMode::New;
	return ExpandTabsMode::None;
}

void Editor::ExpandTabs::set(ExpandTabsMode value)
{
	EnsureCurrent();
	EditorSetParameter esp;
	esp.Type = ESPT_EXPANDTABS;
	esp.Param.iParam = (int)value;
	Info.EditorControl(ECTL_SETPARAM, &esp);
}

ICursor^ Editor::Cursor::get()
{
	return _cursor;
}

ILines^ Editor::Lines::get()
{
	return _lines;
}

int Editor::Id::get()
{
	return _id;
}

void Editor::Id::set(int value)
{
	if (IsOpened && (value == -1))
		BecomeClosed();
	if (!IsOpened && (value != -1))
		BecomeOpened();
	_id = value;
}

int Editor::TabSize::get()
{
	if (!IsOpened)
		return 0;
	EditorInfo ei; EnsureCurrent(ei);
	return ei.TabSize;
}

void Editor::TabSize::set(int value)
{
	if (value <= 0)
		throw gcnew ArgumentException("value");
	EnsureCurrent();
	EditorSetParameter esp;
	esp.Type = ESPT_TABSIZE;
	esp.Param.iParam = value;
	Info.EditorControl(ECTL_SETPARAM, &esp);
}

String^ Editor::FileName::get()
{
	return _fileName;
}

void Editor::FileName::set(String^ value)
{
	EnsureClosed();
	_fileName = value;
}

String^ Editor::Title::get()
{
	return _title;
}

void Editor::Title::set(String^ value)
{
	if (IsOpened)
	{
		EnsureCurrent();
		CStr sValue(value);
		Info.EditorControl(ECTL_SETTITLE, sValue);
	}
	else
	{
		_title = value;
	}
}

IRect^ Editor::Window::get()
{
	if (IsOpened)
		GetParams();
	return _window;
}

void Editor::Window::set(IRect^ value)
{
	_window = value;
}

ISelection^ Editor::Selection::get()
{
	return _selection;
}

void Editor::Insert(String^ text)
{
	EnsureCurrent();
	CStr sb(text->Replace("\r\n", "\r")->Replace('\n', '\r'));
	Info.EditorControl(ECTL_INSERTTEXT, sb);
}

void Editor::Redraw()
{
	EnsureCurrent();
	Info.EditorControl(ECTL_REDRAW, 0);
}

void Editor::DeleteChar()
{
	EnsureCurrent();
	Info.EditorControl(ECTL_DELETECHAR, 0);
}

void Editor::DeleteLine()
{
	EnsureCurrent();
	Info.EditorControl(ECTL_DELETESTRING, 0);
}

void Editor::Save()
{
	EnsureCurrent();
	if (!Info.EditorControl(ECTL_SAVEFILE, 0))
		throw gcnew OperationCanceledException("Can't save the editor file.");
}

void Editor::Save(String^ fileName)
{
	if (fileName == nullptr)
		return Save();
	EnsureCurrent();
	CStr sFileName(fileName);
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
	EnsureCurrent();
	int v = indent;
	Info.EditorControl(ECTL_INSERTSTRING, &v);
}

int Editor::Flags()
{
	int r = 0;
	if (!IsModal)
		r |= EF_NONMODAL;
	if (Async)
		r |= EF_IMMEDIATERETURN;
	if (DeleteOnClose)
		r |= EF_DELETEONCLOSE;
	if (DeleteOnlyFileOnClose)
		r |= EF_DELETEONLYFILEONCLOSE;
	if (IsNew)
		r |= EF_CREATENEW;
	if (EnableSwitch)
		r |= EF_ENABLE_F6;
	if (DisableHistory)
		r |= EF_DISABLEHISTORY;
	return r;
}

void Editor::EnsureClosed()
{
	if (IsOpened)
		throw gcnew InvalidOperationException("Editor must not be open for this operation.");
}

void Editor::EnsureCurrent()
{
	EditorInfo ei;
	EnsureCurrent(ei);
}

void Editor::EnsureCurrent(EditorInfo& ei)
{
	if (!IsOpened)
		throw gcnew InvalidOperationException("Editor must be open for this operation.");
	EditorControl_ECTL_GETINFO(ei, true);
	if (ei.EditorID < 0)
		throw gcnew InvalidOperationException("This operation is only for current editor.");
	if (ei.EditorID != _id)
		throw gcnew InvalidOperationException("This editor must be current for this operation.");
}

void Editor::GetParams()
{
	_window = gcnew Rect();
	EditorInfo ei; EditorControl_ECTL_GETINFO(ei);
	_window->Width = ei.WindowSizeX;
	_window->Height = ei.WindowSizeY;
	_fileName = OemToStr(ei.FileName);
}

void Editor::BecomeClosed()
{
	_cursor->Impl = _storedCursor;
}

void Editor::BecomeOpened()
{
	_cursor->Impl = _openedCursor;
}

String^ Editor::WordDiv::get()
{
	if (!IsOpened)
		return String::Empty;
	EnsureCurrent();
	EditorSetParameter esp;
	esp.Type = ESPT_GETWORDDIV;
	char s[257];
	esp.Param.cParam = s;
	Info.EditorControl(ECTL_SETPARAM, &esp);
	return OemToStr(s);
}

void Editor::WordDiv::set(String^ value)
{
	if (value == nullptr)
		throw gcnew ArgumentNullException("value");
	EnsureCurrent();
	EditorSetParameter esp;
	CStr sValue(value);
	esp.Type = ESPT_SETWORDDIV;
	esp.Param.cParam = sValue;
	Info.EditorControl(ECTL_SETPARAM, &esp);
}

ICollection<ICursor^>^ Editor::Bookmarks()
{
	EditorInfo ei; EnsureCurrent(ei);

	List<ICursor^>^ r = gcnew List<ICursor^>();
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
			StoredEditorCursor^ c = gcnew StoredEditorCursor();
			c->LeftPos = ebm.LeftPos[i];
			c->Line = ebm.Line[i];
			c->Pos = ebm.Cursor[i];
			c->TopLine = c->Line - ebm.ScreenLine[i];
			r->Add(c);
		}

		delete ebm.Cursor;
		delete ebm.LeftPos;
		delete ebm.Line;
		delete ebm.ScreenLine;
	}

	return r;
}
}
