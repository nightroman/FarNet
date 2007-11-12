/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#pragma once

// Empty String and Solid String
#define ES(s) (String::IsNullOrEmpty(s))
#define SS(s) (!String::IsNullOrEmpty(s))

#define DEF_EVENT(EventName, Handler)\
virtual event EventHandler^ EventName {\
void add(EventHandler^ handler) { Handler += handler; }\
void remove(EventHandler^ handler) { Handler -= handler; }\
void raise(Object^ sender, EventArgs^ e) { if (Handler != nullptr) Handler(sender, e); }\
}\
internal: EventHandler^ Handler;

#define DEF_EVENT_ARGS(EventName, Handler, Arguments)\
virtual event EventHandler<Arguments^>^ EventName {\
void add(EventHandler<Arguments^>^ handler) { Handler += handler; }\
void remove(EventHandler<Arguments^>^ handler) { Handler -= handler; }\
void raise(Object^ sender, Arguments^ e) { if (Handler != nullptr) Handler(sender, e); }\
}\
internal: EventHandler<Arguments^>^ Handler;

#define DEF_PROP_SET(Class, Type, Prop, Var)\
Type Class::Prop::get() { return Var; }\
void Class::Prop::set(Type value) { Var = value; }

#define DEF_PROP_FLAG(Class, Prop, Flag)\
bool Class::Prop::get() { return (_flags & Flag) != 0; }\
void Class::Prop::set(bool value) { if (value) _flags |= Flag; else _flags &= ~Flag; }

#define INL_PROP_FLAG(Prop, Flag) virtual property bool Prop\
{\
	bool get() { return (_flags & Flag) != 0; }\
	void set(bool value) { if (value) _flags |= Flag; else _flags &= ~Flag; }\
}

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
	static char s_empty[1];
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

// String converters
Char OemToChar(char oem);
void StrToOem(String^ str, char* oem);
void StrToOem(String^ str, char* oem, int size);
String^ FromEditor(const char* text, int len);
String^ OemToStr(const char* oem);
String^ OemToStr(const char* oem, int length);

// FAR API wrappers
void EditorControl_ECTL_DELETEBLOCK();
void EditorControl_ECTL_DELETECHAR();
void EditorControl_ECTL_DELETESTRING();
void EditorControl_ECTL_EDITORTOOEM(char* text, int len);
void EditorControl_ECTL_GETBOOKMARKS(EditorBookMarks& ebm);
void EditorControl_ECTL_GETINFO(EditorInfo& ei, bool safe = false);
void EditorControl_ECTL_GETSTRING(EditorGetString& egs, int no);
void EditorControl_ECTL_INSERTSTRING(bool indent);
void EditorControl_ECTL_INSERTTEXT(String^ text, int overtype);
void EditorControl_ECTL_OEMTOEDITOR(char* text, int len);
void EditorControl_ECTL_SELECT(EditorSelect& es);
void EditorControl_ECTL_SETPARAM(const EditorSetParameter esp);
void EditorControl_ECTL_SETPOSITION(const EditorSetPosition& esp);
void EditorControl_ECTL_SETSTRING(EditorSetString& ess);
void ViewerControl_VCTL_GETINFO(ViewerInfo& vi, bool safe = false);

// Advanced wrappers
void Edit_Clear();
void Edit_GoTo(int pos, int line);
void Edit_RestoreEditorInfo(const EditorInfo& ei);
void Edit_SetOvertype(bool value);

// Helpers
MouseInfo GetMouseInfo(const MOUSE_EVENT_RECORD& m);
String^ ExceptionInfo(Exception^ e, bool full);
void ValidateRect(int& x, int& w, int min, int size);
String^ Wildcard(String^ pattern);
String^ JoinText(String^ head, String^ tail);

extern int _fastGetString;

Place SelectionPlace();

namespace FarManagerImpl
{;
// Constant Values
public ref class CV
{
internal:
	static String^ const CR = "\r";
	static String^ const LF = "\n";
	static String^ const CRLF = "\r\n";
};
}

// DateTime tools
DateTime FileTimeToDateTime(FILETIME time);
FILETIME DateTimeToFileTime(DateTime time);
