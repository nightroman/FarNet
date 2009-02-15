/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#pragma once

#pragma region String pins and converters
// STOP: choose a right one, relevant for FAR.

// Pins as null, empty or solid string, all as it is.
#define PIN_NE(PinName, StringVar) pin_ptr<const wchar_t> PinName = PtrToStringChars(StringVar);

// Pins as empty or solid string, null is treated as empty.
#define PIN_ES(PinName, StringVar) pin_ptr<const wchar_t> PinName = PtrToStringChars(StringVar ? StringVar : String::Empty);

// Pins as null or solid string, empty is treated as null.
#define PIN_NS(PinName, StringVar) pin_ptr<const wchar_t> PinName = (StringVar && StringVar->Length) ? PtrToStringChars(StringVar) : (wchar_t*)0;

// Converters
wchar_t* NewChars(String^ str);
void CopyStringToChars(String^ str, wchar_t* buffer);

#pragma endregion

// Buffer size in items, not bytes.
#define SIZEOF(Buffer) (sizeof(Buffer) / sizeof(*Buffer))

// Log
#define LOG_IDLE 2
#define LOG_KEYS 4
#if defined(_DEBUG)
#define LOG 1
#define TraceFail(Text) Trace::Fail(Text)
#define LL(Text) Trace::WriteLine(Text);
#else
#define LOG 0
#define TraceFail(Text) {}
#define LL(Text) {} 
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

/// <summary> Holder of chars converted from String. </summary>
class CStr
{
public:
	CStr() : m_str(0) {}
	CStr(int len);
	CStr(String^ str);
	~CStr();
	void Set(String^ str);
	operator wchar_t*()
	{
		return m_str;
	}
protected:
	wchar_t* m_str;
	static wchar_t s_empty[1];
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
	enum { eLen = 255 };
	T m_buf[eLen + 1];
	T* m_str;
};

// String box
typedef TStr<wchar_t> CBox; //???

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
void EditorControl_ECTL_GETBOOKMARKS(EditorBookMarks& ebm);
void EditorControl_ECTL_GETSTRING(EditorGetString& egs, int no);
void EditorControl_ECTL_INSERTSTRING(bool indent);
void EditorControl_ECTL_INSERTTEXT(String^ text, int overtype);
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
		ErrorNoHotKey = "Ensure a FarNet hotkey (F4) in the FAR plugins menu (F11) and restart FAR.",
		MenuPrefix = ".NET ";
};
}

// Helpers
int Compare(String^ strA, String^ strB);
bool EqualsOrdinal(String^ strA, String^ strB);
MouseInfo GetMouseInfo(const MOUSE_EVENT_RECORD& m);
String^ ExceptionInfo(Exception^ e, bool full);
String^ JoinText(String^ head, String^ tail);
String^ Wildcard(String^ pattern);
void AssertCurrentViewer();
void DeleteSourceOptional(String^ path, DeleteSource option);
void ValidateRect(int& x, int& w, int min, int size);
