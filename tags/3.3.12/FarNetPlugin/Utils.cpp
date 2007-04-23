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

void OEM2WC(const char* src, LPWSTR dst, size_t size)
{
	MultiByteToWideChar(CP_OEMCP, 0, src, (int)size, dst, (int)size);
}

void OEM2WC(const char* src, LPWSTR dst)
{
	OEM2WC(src, dst, strlen(src) + 1);
}

String^ OemToStr(const char* oem, int len)
{
	TStr<wchar_t> dst(len);
	OEM2WC(oem, dst, len);
	dst[len] = 0;
	String^ r = gcnew String(dst);
	return r;
}

String^ OemToStr(const char* oem)
{
	size_t len = strlen(oem);
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

void EditorControl_ECTL_OEMTOEDITOR(char* text, int len)
{
	EditorConvertText ect;
	ect.Text = text;
	ect.TextLength = len;
	Info.EditorControl(ECTL_OEMTOEDITOR, &ect);
}

// Don't check result here!
void EditorControl_ECTL_SELECT(EditorSelect& es)
{
	Info.EditorControl(ECTL_SELECT, &es);
}

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

String^ ExceptionInfo(Exception^ e)
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

	return Regex::Replace(info, "[\r\n]+", "\n");
}

DateTime ft2dt(FILETIME time)
{
	return DateTime::FromFileTime(*(Int64*)&time);
}

FILETIME dt2ft(DateTime time)
{
	Int64 r;
	if (time.Ticks == 0)
		r = 0;
	else
		r = time.ToFileTime();
	return *(FILETIME*)&r;
}
