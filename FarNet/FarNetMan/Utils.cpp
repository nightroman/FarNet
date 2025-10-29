#include "StdAfx.h"
#include "Wrappers.h"

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
		delete[] m_str;
}

///<summary>Converts a string and holds the result.</summary>
void CStr::Set(String^ str)
{
	if (m_str != s_empty)
		delete[] m_str;

	if (String::IsNullOrEmpty(str))
		m_str = s_empty;
	else
		m_str = NewChars(str);
}

#pragma warning (push)
#pragma warning (disable : 4996)

// Returns nullptr for null or empty. Callers delete the result.
wchar_t* NewChars(String^ str)
{
	if (ES(str))
		return nullptr;

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

void ThrowEditorLocked(intptr_t editorId)
{
	AutoEditorInfo ei(editorId);

	if ((ei.CurState & ECSTATE_LOCKED) != 0)
		throw gcnew InvalidOperationException("Editor is locked for changes. Unlock by [CtrlL].");
}

void EditorControl_ECTL_DELETEBLOCK(intptr_t editorId)
{
	if (Info.EditorControl(editorId, ECTL_DELETEBLOCK, 0, 0))
		return;

	ThrowEditorLocked(editorId);
	throw gcnew InvalidOperationException(__FUNCTION__ " failed.");
}

void EditorControl_ECTL_DELETECHAR(intptr_t editorId)
{
	if (Info.EditorControl(editorId, ECTL_DELETECHAR, 0, 0))
		return;

	ThrowEditorLocked(editorId);
	throw gcnew InvalidOperationException(__FUNCTION__ " failed.");
}

void EditorControl_ECTL_DELETESTRING(intptr_t editorId)
{
	if (Info.EditorControl(editorId, ECTL_DELETESTRING, 0, 0))
		return;

	ThrowEditorLocked(editorId);
	throw gcnew InvalidOperationException(__FUNCTION__ " failed.");
}

void EditorControl_ECTL_GETSTRING(EditorGetString& egs, intptr_t editorId, int index)
{
	egs.StringNumber = index;
	if (!Info.EditorControl(editorId, ECTL_GETSTRING, 0, &egs))
		throw gcnew InvalidOperationException(__FUNCTION__ " failed with line index: " + index + ". Ensure current editor and valid line number.");
}

void EditorControl_ECTL_INSERTSTRING(intptr_t editorId, bool indent)
{
	int value = indent;
	if (Info.EditorControl(editorId, ECTL_INSERTSTRING, 0, &value))
		return;

	ThrowEditorLocked(editorId);
	throw gcnew InvalidOperationException(__FUNCTION__ " failed.");
}

void EditorControl_ECTL_INSERTTEXT(intptr_t editorId, Char text, int overtype)
{
	if (overtype > 0)
		Edit_SetOvertype(editorId, false);

	wchar_t buf[2] = { text, 0 };

	try
	{
		if (Info.EditorControl(editorId, ECTL_INSERTTEXT, 0, (wchar_t*)buf))
			return;

		ThrowEditorLocked(editorId);
		throw gcnew InvalidOperationException(__FUNCTION__ " failed.");
	}
	finally
	{
		if (overtype > 0)
			Edit_SetOvertype(editorId, true);
	}
}

// "\r" -- new line sharp // "\n" -- new line + indent // so "\r\n" -- 2 new lines
void EditorControl_ECTL_INSERTTEXT(intptr_t editorId, String^ text, int overtype)
{
	if (overtype > 0)
		Edit_SetOvertype(editorId, false);

	String^ text2 = text->ReplaceLineEndings("\r");
	PIN_NE(pin, text2);

	try
	{
		if (Info.EditorControl(editorId, ECTL_INSERTTEXT, 0, (wchar_t*)pin))
			return;

		ThrowEditorLocked(editorId);
		throw gcnew InvalidOperationException(__FUNCTION__ " failed.");
	}
	finally
	{
		if (overtype > 0)
			Edit_SetOvertype(editorId, true);
	}
}

//! Don't check result here.
void EditorControl_ECTL_SELECT(intptr_t editorId, EditorSelect& es)
{
	Info.EditorControl(editorId, ECTL_SELECT, 0, &es);
}

int EditorControl_ECTL_SETPARAM(intptr_t editorId, const EditorSetParameter& esp)
{
	return (int)Info.EditorControl(editorId, ECTL_SETPARAM, 0, (void*)&esp);
}

//! *) Looks like it does not fail if input is 'out of range'.
//! *) It is called from 'finally' and FxCop is against exceptions in 'finally'.
void EditorControl_ECTL_SETPOSITION(intptr_t editorId, const EditorSetPosition& esp)
{
	if (!Info.EditorControl(editorId, ECTL_SETPOSITION, 0, (void*)&esp))
		Log::Source->TraceEvent(TraceEventType::Warning, 0, "ECTL_SETPOSITION failed");
}

void EditorControl_ECTL_SETSTRING(intptr_t editorId, EditorSetString& ess)
{
	if (Info.EditorControl(editorId, ECTL_SETSTRING, 0, &ess))
		return;

	ThrowEditorLocked(editorId);
	throw gcnew InvalidOperationException(__FUNCTION__ " failed with line index: " + ess.StringNumber + ". Ensure current editor and valid line number.");
}

bool IsCurrentViewer()
{
	WindowKind windowKind = Wrap::WindowGetKind();

	// viewer
	if (windowKind == WindowKind::Viewer)
		return true;

	// not panels
	if (windowKind != WindowKind::Panels)
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
	Info.ViewerControl(-1, VCTL_GETINFO, 0, &vi);
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
void Edit_Clear(intptr_t editorId)
{
	AutoEditorInfo ei(editorId);
	EditorGetString egs = {sizeof(egs)};
	EditorControl_ECTL_GETSTRING(egs, editorId, (int)ei.TotalLines - 1);

	EditorSelect es = {sizeof(es)};
	es.BlockHeight = ei.TotalLines;
	es.BlockWidth = egs.StringLength;
	if (es.BlockHeight > 1 || es.BlockWidth > 0)
	{
		es.BlockType = BTYPE_STREAM;
		es.BlockStartLine = 0;
		es.BlockStartPos = 0;
		EditorControl_ECTL_SELECT(editorId, es);
		EditorControl_ECTL_DELETEBLOCK(editorId);
	}
}

void Edit_GoTo(intptr_t editorId, int pos, int line)
{
	SEditorSetPosition esp;
	esp.CurLine = line;
	esp.CurPos = pos;
	EditorControl_ECTL_SETPOSITION(editorId, esp);
}

void Edit_RestoreEditorInfo(const EditorInfo& ei)
{
	SEditorSetPosition esp;
	esp.CurLine = ei.CurLine;
	esp.CurPos = ei.CurPos;
	esp.LeftPos = ei.LeftPos;
	esp.Overtype = ei.Overtype;
	esp.TopScreenLine = ei.TopScreenLine;
	EditorControl_ECTL_SETPOSITION(ei.EditorID, esp);
}

void Edit_SetOvertype(intptr_t editorId, bool value)
{
	SEditorSetPosition esp;
	esp.Overtype = value;
	EditorControl_ECTL_SETPOSITION(editorId, esp);
}

void Edit_RemoveAt(intptr_t editorId, int index)
{
	if (index < 0)
		throw gcnew ArgumentException("'index' must not be negative.");

	// get info
	AutoEditorInfo ei(editorId);

	if (index >= ei.TotalLines)
		throw gcnew ArgumentOutOfRangeException("index");

	// last?
	if (index == ei.TotalLines - 1)
	{
		// last
		EditorGetString egsLast = {sizeof(egsLast)};
		EditorControl_ECTL_GETSTRING(egsLast, editorId, (int)ei.TotalLines - 1);

		// remove if not empty
		if (egsLast.StringLength > 0)
		{
			Edit_GoTo(editorId, 0, index);
			EditorControl_ECTL_DELETESTRING(editorId);
		}

		// go to the end of previous
		if (--index < 0)
			return;

		EditorControl_ECTL_GETSTRING(egsLast, editorId, index);
		Edit_GoTo(editorId, (int)egsLast.StringLength, index);

		// and delete EOL
		EditorControl_ECTL_DELETECHAR(editorId);
	}
	else
	{
		Edit_GoTo(editorId, 0, index);
		EditorControl_ECTL_DELETESTRING(editorId);
	}

	// restore
	Edit_RestoreEditorInfo(ei);
}

MouseInfo^ GetMouseInfo(const MOUSE_EVENT_RECORD& m)
{
	return gcnew MouseInfo(
		Point(m.dwMousePosition.X, m.dwMousePosition.Y),
		(MouseAction)m.dwEventFlags & MouseAction::All,
		(MouseButtons)m.dwButtonState & MouseButtons::All,
		(ControlKeyStates)m.dwControlKeyState & ControlKeyStates::All,
		(int)(short)HIWORD(m.dwButtonState));
}

Place Edit_SelectionPlace(intptr_t editorId)
{
	AutoEditorInfo ei(editorId);
	if (ei.BlockType == BTYPE_NONE)
		return Place(-1);

	Place r;
	EditorGetString egs = {sizeof(egs)};
	r.Top = (int)ei.BlockStartLine;
	r.Left = -1;
	for(egs.StringNumber = r.Top; egs.StringNumber < ei.TotalLines; ++egs.StringNumber)
	{
		EditorControl_ECTL_GETSTRING(egs, editorId, (int)egs.StringNumber);
		if (r.Left < 0)
			r.Left = (int)egs.SelStart;
		if (egs.SelStart < 0)
			break;
		r.Right = (int)egs.SelEnd;
	}
	--r.Right;
	r.Bottom = (int)egs.StringNumber - 1;

	return r;
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

int ParseInt(String^ value, int fallback)
{
	int result;
	return int::TryParse(value, result) ? result : fallback;
}

Guid FromGUID(const GUID& guid)
{
	return Guid(
		guid.Data1, guid.Data2, guid.Data3, 
		guid.Data4[0], guid.Data4[1], 
		guid.Data4[2], guid.Data4[3], 
		guid.Data4[4], guid.Data4[5], 
		guid.Data4[6], guid.Data4[7]);
}

GUID ToGUID(Guid guid)
{
   array<Byte>^ guidData = guid.ToByteArray();
   pin_ptr<Byte> data = &(guidData[0]);
   return *(GUID*)data;
}

KeyInfo^ KeyInfoFromInputRecord(const INPUT_RECORD& ir)
{
	return gcnew KeyInfo(
		ir.Event.KeyEvent.wVirtualKeyCode,
		ir.Event.KeyEvent.uChar.UnicodeChar,
		(ControlKeyStates)ir.Event.KeyEvent.dwControlKeyState,
		ir.Event.KeyEvent.bKeyDown != 0);
}

#undef FCTL_SETPANELDIRECTORY
void SetPanelDirectory(HANDLE handle, String^ path)
{
	PIN_NE(pin, path);

	FarPanelDirectory arg; memset(&arg, 0, sizeof(arg)); arg.StructSize = sizeof(arg);
	arg.Name = pin;

	if (!Info.PanelControl(handle, FCTL_SETPANELDIRECTORY, 0, &arg))
		throw gcnew InvalidOperationException("Cannot set panel directory: " + path);
}

#undef ACTL_GETWINDOWCOUNT
int Call_ACTL_GETWINDOWCOUNT()
{
	return (int)Info.AdvControl(&MainGuid, ACTL_GETWINDOWCOUNT, 0, 0);
}

#undef ACTL_GETWINDOWINFO
void Call_ACTL_GETWINDOWINFO(WindowInfo& wi, int index)
{
	wi.StructSize = sizeof(wi);
	wi.TypeName = nullptr;
	wi.Name = nullptr;
	wi.TypeNameSize = 0;
	wi.NameSize = 0;
	wi.Pos = index;
	
	if (!Info.AdvControl(&MainGuid, ACTL_GETWINDOWINFO, 0, &wi))
		throw gcnew InvalidOperationException(__FUNCTION__ " failed, index = " + index);
}
void Call_ACTL_GETWINDOWINFO(WindowInfo& wi)
{
	if (!Info.AdvControl(&MainGuid, ACTL_GETWINDOWINFO, 0, &wi))
		throw gcnew InvalidOperationException(__FUNCTION__ " failed, index = " + wi.Pos);
}

#undef DM_GETDLGITEM
void Call_DM_GETDLGITEM(CBin& bin, FarGetDialogItem& gdi, HANDLE hDlg, int item)
{
	gdi.StructSize = sizeof(gdi);
	gdi.Size = bin.Size();
	gdi.Item = (FarDialogItem*)bin.Data();
	
	gdi.Size = Info.SendDlgMessage(hDlg, DM_GETDLGITEM, item, &gdi);
	if ((int)gdi.Size <= bin.Size())
		return;

	bin(gdi.Size);
	gdi.Item = (FarDialogItem*)bin.Data();
	Info.SendDlgMessage(hDlg, DM_GETDLGITEM, item, &gdi);
}

#ifdef TRACE_MEMORY
#define TRACE_MEMORY_BREAK
#include <Test1.cpp>
#endif
