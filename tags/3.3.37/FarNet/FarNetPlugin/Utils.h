/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2007 FAR.NET Team
*/

#pragma once

// Log
#define LOG_IDLE 2
#define LOG_KEYS 4
#if defined(_DEBUG)
#define LOG 1
#define Log(Text) Trace::Write(Text); Trace::Write(" ");
#define LogLine(Text) Trace::WriteLine(Text)
#else
#define LOG 0
#define Log(Text)
#define LogLine(Text)
#endif

// Code analysis
#define CA_USED SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")

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

// String converters
Char OemToChar(char oem);
char* NewOem(String^ str);
void StrToOem(String^ str, char* oem);
void StrToOem(String^ str, char* oem, int size);
String^ FromEditor(const char* text, int len);
String^ OemToStr(const char* oem);
String^ OemToStr(const char* oem, int length);

/// <summary> Holder of OEM char* converted from String. </summary>
class CStr
{
public:
	CStr() : m_str(0) {}
	CStr(int len);
	CStr(String^ str);
	~CStr();
	void Set(String^ str);
	operator char*()
	{
		return m_str;
	}
protected:
	char* m_str;
	static char s_empty[1];
};

///<summary> Temp string buffer. </summary>
template<class T>
class TStr
{
public:
	TStr() : m_str(0)
	{}
	TStr(int len)
	{
		if (len > eLen)
			m_str = new T[len + 1];
		else
			m_str = m_buf;
	}
	TStr(const T* str, int len)
	{
		if (len > eLen)
			m_str = new T[len + 1];
		else
			m_str = m_buf;
		strncpy_s(m_str, len + 1, str, len);
	}
	TStr(String^ str)
	{
		if (!str || str->Length == 0)
		{
			m_str = m_buf;
			m_buf[0] = 0;
			return;
		}
		if (str->Length > eLen)
			m_str = new char[str->Length + 1];
		else
			m_str = m_buf;
		StrToOem(str, m_str);
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
	void Set(String^ str)
	{
		if (m_str != m_buf)
			delete m_str;
		if (!str || str->Length == 0)
		{
			m_str = m_buf;
			m_buf[0] = 0;
			return;
		}
		if (str->Length > eLen)
			m_str = new char[str->Length + 1];
		else
			m_str = m_buf;
		StrToOem(str, m_str);
	}
	void Reset(String^ str)
	{
		if (str && str->Length)
		{
			Set(str);
			return;
		}
		if (m_str != m_buf)
			delete m_str;
		m_str = 0;
	}
private:
	enum { eLen = 255 };
	T m_buf[eLen + 1];
	T* m_str;
};

// String box
typedef TStr<char> CBox;

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

// DateTime tools
DateTime FileTimeToDateTime(FILETIME time);
FILETIME DateTimeToFileTime(DateTime time);

// Value host and switch
#define VALUE_HOST(Type, Name)\
class Name\
{\
public:\
	Name(Type value = 0) { _value = value; }\
	~Name() { _value = 0; }\
	static Type Get() { return _value; }\
	static void Set(Type value) { _value = value; }\
private:\
	static Type _value;\
}

// Hosted values
VALUE_HOST(bool, ValueCanOpenPanel);
VALUE_HOST(bool, ValueUserScreen);

namespace FarNet
{;
// Constant values
typedef String^ const ConstString;
ref class CV
{
internal:
	static ConstString
		CR = "\r",
		LF = "\n",
		CRLF = "\r\n";
private:
	CV() {}
};

// Resource strings
ref class Res
{
	Res() {}
internal:
	static ConstString
		CommandPlugins = "Command plugins",
		EditorPlugins = "Editor plugins",
		PanelsTools = "Panels tools",
		DialogTools = "Dialog tools",
		EditorTools = "Editor tools",
		ViewerTools = "Viewer tools",
		DiskTools = "Disk tools",
		ConfigTools = "Config tools",
		FilerPlugins = "Filer plugins",
		ErrorNoHotKey = "Ensure a FAR.NET hotkey (F4) in the FAR plugins menu (F11) and restart FAR.",
		MenuPanels = "Push/show panels",
		MenuPrefix = ".NET ";
};
}
