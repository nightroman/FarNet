/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#include "StdAfx.h"
#include "Wrappers.h"

// hosted values
bool ValueUserScreen::_value;

// empty string
wchar_t CStr::s_empty[1] = {0};

///<summary>Converts a string and holds the result.</summary>
CStr::CStr(String^ str)
{
	if (String::IsNullOrEmpty(str))
		m_str = s_empty;
	else
		m_str = NewChars(str);
}

///<summary>Makes and holds new char[len + 1].</summary>
CStr::CStr(int len)
{
	m_str = new wchar_t[len + 1];
}

///<summary>Destructor: deletes the data.</summary>
CStr::~CStr()
{
	if (m_str != s_empty)
		delete m_str;
}

///<summary>Converts a string and holds the result.</summary>
void CStr::Set(String^ str)
{
	if (m_str != s_empty)
		delete m_str;

	if (String::IsNullOrEmpty(str))
		m_str = s_empty;
	else
		m_str = NewChars(str);
}

#pragma warning (push)
#pragma warning (disable : 4996)

// Returns NULL for null or empty. Callers delete the result.
wchar_t* NewChars(String^ str)
{
	if (ES(str))
		return 0;

	wchar_t* r = new wchar_t[str->Length + 1];
	PIN_NE(pin, str);
	wcscpy(r, pin);
	return r;
}
wchar_t* NewChars(Object^ str)
{
	return str ? NewChars(str->ToString()) : 0;
}
// No new after this point.
#undef new
#define new dont_use_new

void CopyStringToChars(String^ str, wchar_t* buffer)
{
	PIN_NE(pin, str);
	wcscpy(buffer, pin);
}
#pragma warning (pop)

//
// Generic Far wrappers
//

void EditorControl_ECTL_DELETEBLOCK()
{
	Info.EditorControl(ECTL_DELETEBLOCK, 0);
}

void EditorControl_ECTL_DELETECHAR()
{
	Info.EditorControl(ECTL_DELETECHAR, 0);
}

void EditorControl_ECTL_DELETESTRING()
{
	Info.EditorControl(ECTL_DELETESTRING, 0);
}

void EditorControl_ECTL_GETBOOKMARKS(EditorBookMarks& ebm)
{
	if (!Info.EditorControl(ECTL_GETBOOKMARKS, &ebm))
		throw gcnew InvalidOperationException(__FUNCTION__ " failed. Ensure current editor.");
}

// dirty global
int _fastGetString;

void EditorControl_ECTL_GETSTRING(EditorGetString& egs, int no)
{
	if (no >= 0 && _fastGetString)
	{
		static EditorSetPosition esp = {-1, -1, -1, -1, -1, -1};
		esp.CurLine = no;
		if (!Info.EditorControl(ECTL_SETPOSITION, &esp))
			throw gcnew InvalidOperationException(__FUNCTION__ " failed with line index: " + no + ". Ensure current editor and valid line number.");
		egs.StringNumber = -1;
		Info.EditorControl(ECTL_GETSTRING, &egs);
		egs.StringNumber = no;
	}
	else
	{
		egs.StringNumber = no;
		if (!Info.EditorControl(ECTL_GETSTRING, &egs))
			throw gcnew InvalidOperationException(__FUNCTION__ " failed with line index: " + no + ". Ensure current editor and valid line number.");
	}
}

void EditorControl_ECTL_INSERTSTRING(bool indent)
{
	int value = indent;
	Info.EditorControl(ECTL_INSERTSTRING, &value);
}

void EditorControl_ECTL_INSERTTEXT(String^ text, int overtype)
{
	if (overtype > 0)
		Edit_SetOvertype(false);

	String^ text2 = text->Replace("\r\n", "\r")->Replace('\n', '\r');
	PIN_NE(pin, text2);
	Info.EditorControl(ECTL_INSERTTEXT, (wchar_t*)pin);

	if (overtype > 0)
		Edit_SetOvertype(true);
}

//! Don't check result here.
void EditorControl_ECTL_SELECT(EditorSelect& es)
{
	Info.EditorControl(ECTL_SELECT, &es);
}

void EditorControl_ECTL_SETPARAM(const EditorSetParameter esp)
{
	Info.EditorControl(ECTL_SETPARAM, (void*)&esp);
}

//! *) Looks like it does not fail if input is 'out of range'.
//! *) It is called from 'finally' and FxCop is against exceptions in 'finally'.
void EditorControl_ECTL_SETPOSITION(const EditorSetPosition& esp)
{
	if (!Info.EditorControl(ECTL_SETPOSITION, (EditorSetPosition*)&esp))
		Log::TraceWarning("ECTL_SETPOSITION failed");
}

void EditorControl_ECTL_SETSTRING(EditorSetString& ess)
{
	if (!Info.EditorControl(ECTL_SETSTRING, &ess))
		throw gcnew InvalidOperationException(__FUNCTION__ " failed with line index: " + ess.StringNumber + ". Ensure current editor and valid line number.");
}

bool IsCurrentViewer()
{
	// get type
	WindowInfo wi; wi.Pos = -1;
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_GETSHORTWINDOWINFO, &wi))
		throw gcnew InvalidOperationException("ACTL_GETSHORTWINDOWINFO failed.");

	// viewer
	if (wi.Type == WTYPE_VIEWER)
		return true;

	// not panels
	if (wi.Type != WTYPE_PANELS)
		return false;
	
	// qview
	//! Verified for Far 2.0.843:
	//! QView panel creates a viewer, it gets read and close events, but not focus.
	PanelInfo pi;
	GetPanelInfo(INVALID_HANDLE_VALUE, pi);
	return pi.PanelType == PTYPE_QVIEWPANEL;
}

//! If a viewer does not exist or even it is not current then Far may crash.
//! Thus, we check the current window type.
void ViewerControl_VCTL_GETINFO(ViewerInfo& vi, bool safe)
{
	// check window type
	if (!IsCurrentViewer())
	{
		if (safe)
		{
			vi.ViewerID = -1;
			return;
		}
		throw gcnew InvalidOperationException("A viewer window must be current.");
	}

	// get viewer info
	vi.StructSize = sizeof(vi);
	Info.ViewerControl(VCTL_GETINFO, &vi);
}

void AssertCurrentViewer()
{
	if (!IsCurrentViewer())
		throw gcnew InvalidOperationException("A viewer window must be current.");
}

//
// Advanced Far wrappers
//

// select and delete all text if any
void Edit_Clear()
{
	AutoEditorInfo ei;
	EditorGetString egs; EditorControl_ECTL_GETSTRING(egs, ei.TotalLines - 1);

	EditorSelect es;
	es.BlockHeight = ei.TotalLines;
	es.BlockWidth = egs.StringLength;
	if (es.BlockHeight > 1 || es.BlockWidth > 0)
	{
		es.BlockType = BTYPE_STREAM;
		es.BlockStartLine = 0;
		es.BlockStartPos = 0;
		EditorControl_ECTL_SELECT(es);
		EditorControl_ECTL_DELETEBLOCK();
	}
}

void Edit_GoTo(int pos, int line)
{
	SEditorSetPosition esp;
	esp.CurLine = line;
	esp.CurPos = pos;
	EditorControl_ECTL_SETPOSITION(esp);
}

void Edit_RestoreEditorInfo(const EditorInfo& ei)
{
	SEditorSetPosition esp;
	esp.CurLine = ei.CurLine;
	esp.CurPos = ei.CurPos;
	esp.LeftPos = ei.LeftPos;
	esp.Overtype = ei.Overtype;
	esp.TopScreenLine = ei.TopScreenLine;
	EditorControl_ECTL_SETPOSITION(esp);
}

void Edit_SetOvertype(bool value)
{
	SEditorSetPosition esp;
	esp.Overtype = value;
	EditorControl_ECTL_SETPOSITION(esp);
}

MouseInfo GetMouseInfo(const MOUSE_EVENT_RECORD& m)
{
	return MouseInfo(
		Point(m.dwMousePosition.X, m.dwMousePosition.Y),
		(MouseAction)m.dwEventFlags & MouseAction::All,
		(MouseButtons)m.dwButtonState & MouseButtons::All,
		(ControlKeyStates)m.dwControlKeyState & ControlKeyStates::All);
}

Place SelectionPlace()
{
	AutoEditorInfo ei;
	if (ei.BlockType == BTYPE_NONE)
		return Place(-1);

	Place r;
	EditorGetString egs;
	r.Top = ei.BlockStartLine;
	r.Left = -1;
	for(egs.StringNumber = r.Top; egs.StringNumber < ei.TotalLines; ++egs.StringNumber)
	{
		EditorControl_ECTL_GETSTRING(egs, egs.StringNumber);
		if (r.Left < 0)
			r.Left = egs.SelStart;
		if (egs.SelStart < 0)
			break;
		r.Right = egs.SelEnd;
	}
	--r.Right;
	r.Bottom = egs.StringNumber - 1;

	return r;
}

// Gets a property value by name or null
Object^ Property(Object^ obj, String^ name)
{
	try
	{
		return obj->GetType()->InvokeMember(
			name,
			BindingFlags::GetProperty | BindingFlags::Public | BindingFlags::Instance,
			nullptr,
			obj,
			nullptr);
	}
	catch(Exception^ e)
	{
		Log::TraceException(e);
		return nullptr;
	}
}

DateTime FileTimeToDateTime(FILETIME time)
{
	return DateTime::FromFileTime(*(Int64*)&time);
}

FILETIME DateTimeToFileTime(DateTime time)
{
	Int64 r;
	if (time.Ticks == 0)
		r = 0;
	else
		r = time.ToFileTime();
	return *(FILETIME*)&r;
}

// Simple wildcard (* and ?)
String^ Wildcard(String^ pattern)
{
	pattern = Regex::Escape(pattern);
	for(int i = 0; i < pattern->Length - 1; ++i)
	{
		if (pattern[i] != '\\')
			continue;
		
		if (pattern[i + 1] == '*')
			pattern = pattern->Substring(0, i) + ".*" + pattern->Substring(i + 2);
		if (pattern[i + 1] == '?')
			pattern = pattern->Substring(0, i) + ".?" + pattern->Substring(i + 2);
		else
			++i;
	}
	return pattern;
}

// Joins strings with spaces
String^ JoinText(String^ head, String^ tail)
{
	if (String::IsNullOrEmpty(head))
		return tail ? tail : String::Empty;
	if (String::IsNullOrEmpty(tail))
		return head ? head : String::Empty;
	return head + " " + tail;
}

// Validates rect position and width by screen size so that rect is visible.
void ValidateRect(int& x, int& w, int min, int size)
{
	if (x < 0)
		x = min + (size - w)/2;
	int r = x + w - 1;
	if (r > min + size - 1)
	{
		x -= (r - min - size + 1);
		if (x < min)
			x = min;
		r = x + w - 1;
		if (r > min + size - 1)
			w -= (r - min - size + 1);
	}
}

void DeleteSourceOptional(String^ path, DeleteSource option)
{
	if (option != DeleteSource::File && option != DeleteSource::Folder)
		return;

	if (File::Exists(path))
		File::Delete(path);

	if (option != DeleteSource::Folder)
		return;

	try
	{
		Directory::Delete(Path::GetDirectoryName(path));
	}
	catch(IOException^ e)
	{
		Log::TraceException(e);
	}
}

int Compare(String^ strA, String^ strB)
{
	return String::Compare(strA, strB, true, CultureInfo::InvariantCulture);
}

bool EqualsOrdinal(String^ strA, String^ strB)
{
	return StringComparer::OrdinalIgnoreCase->Equals(strA, strB);
}

#ifdef TRACE_MEMORY
#define TRACE_MEMORY_BREAK
#include <Test1.cpp>
#endif
