#pragma once

/// <summary>
/// Holder of OEM char* converted from String.
/// </summary>
class CStr
{
public:
	CStr() : m_str(0) {}
	CStr(String^ str);
	CStr(int length);
	~CStr();
	void Set(String^ str);
	operator char*() const
	{
		return m_str;
	}
private:
	char* m_str;
};

/// <summary>
/// Holder of OEM char* converted from String.
/// </summary>
struct SEditorSetPosition : EditorSetPosition
{
	SEditorSetPosition()
	{
		CurLine = -1;
		CurPos = -1;
		CurTabPos = -1;
		LeftPos = -1;
		Overtype = -1;
		TopScreenLine = -1;
	}
};

void WC2OEM(LPCWSTR src, char* dst, int len);
void StrToOem(String^ str, char* oem);
void OEM2WC(const char* src, LPCWSTR* dst);
void OEM2WC(const char* src, LPCWSTR* dst, size_t size);
String^ OemToStr(const char* oem);
String^ OemToStr(const char* oem, int length);
#define STR_ARG(X) CStr pc##X(X)

// Far string converters
String^ fromEditor(const char* text, int len);
void convert(int cmd, char* text, int len);

// Far API wrappers
void EditorControl_ECTL_GETBOOKMARKS(EditorBookMarks& ebm);
void EditorControl_ECTL_GETINFO(EditorInfo& ei, bool safe = false);
void EditorControl_ECTL_GETSTRING(EditorGetString& egs, int no);
void EditorControl_ECTL_SELECT(EditorSelect& es);
void EditorControl_ECTL_SETSTRING(EditorSetString& ess);
