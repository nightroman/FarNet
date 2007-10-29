/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#include "StdAfx.h"

///<summary>Constructor: converts a string and holds the result.</summary>
CStr::CStr(String^ str)
{
	if (str == nullptr)
		str = String::Empty;
	m_str = new char[str->Length + 1];
	StrToOem(str, m_str);
}

///<summary>Constructor: makes and holds new char[length+1].</summary>
CStr::CStr(int length)
{
	m_str = new char[length + 1];
}

///<summary>Destructor: deletes the data.</summary>
CStr::~CStr()
{
	delete m_str;
}

///<summary>Converts a string and holds the result.</summary>
void CStr::Set(String^ str)
{
	delete m_str;
	if (str == nullptr)
		str = String::Empty;
	m_str = new char[str->Length + 1];
	StrToOem(str, m_str);
}

///<summary>Temp string buffer.</summary>
template<class T>
class TStr
{
public:
	TStr(int len)
	{
		if (len > eMaxLen)
			m_str = new T[len + 1];
		else
			m_str = m_buf;
	}
	TStr(const T* str, int len)
	{
		if (len > eMaxLen)
			m_str = new T[len + 1];
		else
			m_str = m_buf;
		strncpy_s(m_str, len + 1, str, len);
	}
	~TStr()
	{
		if (m_str != m_buf)
			delete m_str;
	}
	operator T*()
	{
		return m_str;
	}
private:
	enum {eMaxLen = 255};
	T m_buf[eMaxLen + 1];
	T* m_str;
};

// now new after this point
#define new dont_use_new

void WC2OEM(LPCWSTR src, char* dst, int len)
{
	if (src == 0)
	{
		*dst = 0;
	}
	else
	{
		int size = len + 1;
		WideCharToMultiByte(CP_OEMCP, 0, src, size, dst, size, NULL, NULL);
	}
}

void StrToOem(String^ str, char* oem)
{
	pin_ptr<const wchar_t> p = PtrToStringChars(str);
	WC2OEM(p, oem, str->Length);
}

void StrToOem(String^ str, char* oem, int size)
{
	if (str)
		StrToOem((str->Length < size ? str : str->Substring(0, size - 1)), oem);
	else
		oem[0] = 0;
}

void OEM2WC(const char* src, LPWSTR dst, size_t size)
{
	MultiByteToWideChar(CP_OEMCP, 0, src, (int)size, dst, (int)size);
}

void OEM2WC(const char* src, LPWSTR dst)
{
	OEM2WC(src, dst, strlen(src) + 1);
}

Char OemToChar(char oem)
{
	Char dst;
	OEM2WC(&oem, &dst, 1);
	return dst;
}

String^ OemToStr(const char* oem, int len)
{
	if (!oem)
		return String::Empty;

	TStr<wchar_t> dst(len);
	OEM2WC(oem, dst, len);
	dst[len] = 0;
	String^ r = gcnew String(dst);
	return r;
}

String^ OemToStr(const char* oem)
{
	if (!oem)
		return String::Empty;

	int len = (int)strlen(oem);
	TStr<wchar_t> dst(len);
	OEM2WC(oem, dst);
	String^ r = gcnew String(dst);
	return r;
}

String^ FromEditor(const char* text, int len)
{
	TStr<char> textCopy(text, len);
	EditorControl_ECTL_EDITORTOOEM(textCopy, len);
	String^ r = OemToStr(textCopy, len);
	return r;
}

//
// Generic FAR wrappers
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

void EditorControl_ECTL_EDITORTOOEM(char* text, int len)
{
	EditorConvertText ect;
	ect.Text = text;
	ect.TextLength = len;
	Info.EditorControl(ECTL_EDITORTOOEM, &ect);
}

void EditorControl_ECTL_GETBOOKMARKS(EditorBookMarks& ebm)
{
	if (!Info.EditorControl(ECTL_GETBOOKMARKS, &ebm))
		throw gcnew InvalidOperationException(__FUNCTION__ " failed. Ensure current editor.");
}

void EditorControl_ECTL_GETINFO(EditorInfo& ei, bool safe)
{
	if (!Info.EditorControl(ECTL_GETINFO, &ei))
	{
		if (safe)
			ei.EditorID = -1;
		else
			throw gcnew InvalidOperationException(__FUNCTION__ " failed. Ensure current editor.");
	}
	else if (ei.BlockStartLine < 0)
	{
		//! it is possible after selecting with current line (-1)
		//! it is fixed in 1.71 m268 on build 2205
		ei.BlockStartLine = ei.CurLine;
	}
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
	CStr sb(text->Replace("\r\n", "\r")->Replace('\n', '\r'));
	Info.EditorControl(ECTL_INSERTTEXT, sb);
	if (overtype > 0)
		Edit_SetOvertype(true);
}

void EditorControl_ECTL_OEMTOEDITOR(char* text, int len)
{
	EditorConvertText ect;
	ect.Text = text;
	ect.TextLength = len;
	Info.EditorControl(ECTL_OEMTOEDITOR, &ect);
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

//! Looks like it does not fail if input is 'out of range'
void EditorControl_ECTL_SETPOSITION(const EditorSetPosition& esp)
{
	if (!Info.EditorControl(ECTL_SETPOSITION, (EditorSetPosition*)&esp))
		throw gcnew InvalidOperationException(__FUNCTION__);
}

void EditorControl_ECTL_SETSTRING(EditorSetString& ess)
{
	if (!Info.EditorControl(ECTL_SETSTRING, &ess))
		throw gcnew InvalidOperationException(__FUNCTION__ " failed with line index: " + ess.StringNumber + ". Ensure current editor and valid line number.");
}

void ViewerControl_VCTL_GETINFO(ViewerInfo& vi, bool safe)
{
	if (!Info.ViewerControl(VCTL_GETINFO, &vi))
	{
		if (safe)
			vi.ViewerID = -1;
		else
			throw gcnew InvalidOperationException(__FUNCTION__ " failed. Ensure current viewer.");
	}
}

//
// Advanced FAR wrappers
//

// select and delete all text if any
void Edit_Clear()
{
	EditorInfo ei; EditorControl_ECTL_GETINFO(ei);
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
	EditorInfo ei; EditorControl_ECTL_GETINFO(ei);
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
			name, BindingFlags::GetProperty | BindingFlags::Public | BindingFlags::Instance, nullptr, obj, nullptr);
	}
	catch(...)
	{
		return nullptr;
	}
}

String^ ExceptionInfo(Exception^ e, bool full)
{
	String^ info = e->Message + "\n";

	Object^ er = nullptr;
	for(Exception^ ex = e; ex != nullptr; ex = ex->InnerException)
	{
		if (ex->GetType()->FullName->StartsWith("System.Management.Automation."))
		{
			er = Property(ex, "ErrorRecord");
			break;
		}
	}
	if (er != nullptr)
	{
		Object^ ii = Property(er, "InvocationInfo");
		if (ii != nullptr)
		{
			Object^ pm = Property(ii, "PositionMessage");
			if (pm != nullptr)
				info += pm->ToString() + "\n";
		}
	}

	if (full)
		info += "\n" + e->StackTrace + "\n";

	return Regex::Replace(info, "[\r\n]+", "\n");
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
