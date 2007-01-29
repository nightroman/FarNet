#include "StdAfx.h"
#include "Utils.h"

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
	pin_ptr<wchar_t> dst = new wchar_t[len+1];
	OEM2WC(oem, dst, len);
	dst[len] = 0;
	String^ r = gcnew String(dst);
	delete dst;
	return r;
}

String^ OemToStr(const char* oem)
{
	size_t len = strlen(oem);
	pin_ptr<wchar_t> dst = new wchar_t[len+1];
	OEM2WC(oem, dst);
	String^ r = gcnew String(dst);
	delete dst;
	return r;
}

void convert(int cmd, char* text, int len)
{
	EditorConvertText ect;
	ect.Text = text;
	ect.TextLength = len;
	Info.EditorControl(cmd, &ect);
}

String^ fromEditor(const char* text, int len)
{
	char* textCopy = new char[len+1];
	strncpy_s(textCopy, len+1, text, len);
	convert(ECTL_EDITORTOOEM, textCopy, len);
	String^ r = OemToStr(textCopy, len);
	delete textCopy;
	return r;
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
}

void EditorControl_ECTL_GETSTRING(EditorGetString& egs, int no)
{
	egs.StringNumber = no;
	if (!Info.EditorControl(ECTL_GETSTRING, &egs))
		throw gcnew InvalidOperationException(__FUNCTION__ " failed with line index: " + no + ". Ensure current editor and valid line number.");
}

// Don't check result here!
void EditorControl_ECTL_SELECT(EditorSelect& es)
{
	Info.EditorControl(ECTL_SELECT, &es);
}

void EditorControl_ECTL_SETSTRING(EditorSetString& ess)
{
	if (!Info.EditorControl(ECTL_SETSTRING, &ess))
		throw gcnew InvalidOperationException(__FUNCTION__ " failed with line index: " + ess.StringNumber + ". Ensure current editor and valid line number.");
}
