#pragma once

#pragma region String pins and converters
// STOP: choose a right one, relevant for Far.

// Pins as null, empty or solid string, all as it is.
#define PIN_NE(PinName, StringVar) pin_ptr<const wchar_t> PinName = PtrToStringChars(StringVar);

// Pins as empty or solid string, null is treated as empty.
#define PIN_ES(PinName, StringVar) pin_ptr<const wchar_t> PinName = PtrToStringChars(StringVar ? StringVar : String::Empty);

// Pins as null or solid string, empty is treated as null.
#define PIN_NS(PinName, StringVar) pin_ptr<const wchar_t> PinName = (StringVar && StringVar->Length) ? PtrToStringChars(StringVar) : (wchar_t*)0;

// Converters
wchar_t* NewChars(String^ str);
wchar_t* NewChars(Object^ str);
void CopyStringToChars(String^ str, wchar_t* buffer);

#pragma endregion

// Buffer size in items, not bytes.
#define countof(Buffer) (sizeof(Buffer) / sizeof(*Buffer))

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

#define DEF_EVENT2(EventName, Handler)\
virtual event EventHandler^ EventName {\
void add(EventHandler^ handler) override { Handler += handler; }\
void remove(EventHandler^ handler) override { Handler -= handler; }\
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

#define DEF_EVENT_ARGS2(EventName, Handler, Arguments)\
virtual event EventHandler<Arguments^>^ EventName {\
void add(EventHandler<Arguments^>^ handler) override { Handler += handler; }\
void remove(EventHandler<Arguments^>^ handler) override { Handler -= handler; }\
void raise(Object^ sender, Arguments^ e) { if (Handler != nullptr) Handler(sender, e); }\
}\
internal: EventHandler<Arguments^>^ Handler;

#define DEF_EVENT_IMP(EventName, Handler)\
virtual event EventHandler^ EventName {\
void add(EventHandler^ handler) override { Handler += handler; }\
void remove(EventHandler^ handler) override { Handler -= handler; }\
void raise(Object^ sender, EventArgs^ e) { if (Handler != nullptr) Handler(sender, e); }\
}\
internal: EventHandler^ Handler;

#define DEF_EVENT_ARGS_IMP(EventName, Handler, Arguments)\
virtual event EventHandler<Arguments^>^ EventName {\
void add(EventHandler<Arguments^>^ handler) override { Handler += handler; }\
void remove(EventHandler<Arguments^>^ handler) override { Handler -= handler; }\
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

///<summary> Temp data buffer. </summary>
class CBin
{
public:
	CBin()
	{
		m_size = eSize;
		m_data = m_room;
	}
	CBin(size_t size)
	{
		if (size > eSize)
		{
			m_size = size;
			m_data = new char[size];
		}
		else
		{
			m_size = eSize;
			m_data = m_room;
		}
	}
	~CBin()
	{
		if (m_data != m_room)
			delete m_data;
	}
	void* Data()
	{
		return m_data;
	}
	int Size() const
	{
		return (int)m_size;
	}
	bool operator()(size_t size)
	{
		if (size <= m_size)
			return false;
		
		if (m_data != m_room)
			delete m_data;

		m_size = size;
		m_data = new char[size];
		return true;
	}
private:
	enum { eSize = 1024 };
	char m_room[eSize];
	size_t m_size;
	char* m_data;
};

///<summary> Temp string buffer. </summary>
class CBox : CBin
{
public:
	CBox() : CBin()
	{}
	CBox(size_t size) : CBin(size * sizeof(wchar_t))
	{}
	int Size() const
	{
		return CBin::Size() / sizeof(wchar_t);
	}
	bool operator()(size_t size)
	{
		return CBin::operator()(size * sizeof(wchar_t));
	}
	operator wchar_t*()
	{
		return (wchar_t*)Data();
	}
};

struct SEditorSetPosition : EditorSetPosition
{
	SEditorSetPosition()
	{
		StructSize = sizeof(EditorSetPosition);
		CurLine = -1;
		CurPos = -1;
		CurTabPos = -1;
		LeftPos = -1;
		Overtype = -1;
		TopScreenLine = -1;
	}
};

// Far API wrappers
void ThrowEditorLocked(intptr_t editorId);
void EditorControl_ECTL_DELETEBLOCK(intptr_t editorId);
void EditorControl_ECTL_DELETECHAR(intptr_t editorId);
void EditorControl_ECTL_DELETESTRING(intptr_t editorId);
void EditorControl_ECTL_GETSTRING(EditorGetString& egs, intptr_t editorId, int index);
void EditorControl_ECTL_INSERTSTRING(intptr_t editorId, bool indent);
void EditorControl_ECTL_INSERTTEXT(intptr_t editorId, Char text, int overtype);
void EditorControl_ECTL_INSERTTEXT(intptr_t editorId, String^ text, int overtype);
void EditorControl_ECTL_SELECT(intptr_t editorId, EditorSelect& es);
int EditorControl_ECTL_SETPARAM(intptr_t editorId, const EditorSetParameter& esp);
void EditorControl_ECTL_SETPOSITION(intptr_t editorId, const EditorSetPosition& esp);
void EditorControl_ECTL_SETSTRING(intptr_t editorId, EditorSetString& ess);
void ViewerControl_VCTL_GETINFO(ViewerInfo& vi, bool safe = false);

// Advanced wrappers
Place Edit_SelectionPlace(intptr_t editorId);
void Edit_Clear(intptr_t editorId);
void Edit_GoTo(intptr_t editorId, int pos, int line);
void Edit_RemoveAt(intptr_t editorId, int index);
void Edit_RestoreEditorInfo(const EditorInfo& ei);
void Edit_SetOvertype(intptr_t editorId, bool value);

// DateTime tools
DateTime FileTimeToDateTime(FILETIME time);
FILETIME DateTimeToFileTime(DateTime time);

namespace FarNet
{;
// Constant values
typedef String^ const ConstString;

// Resource strings
ref class Res
{
	Res() {}
internal:
	static ConstString
		Menu = "FarNet",
		ModuleCommands = "Commands",
		ModuleDrawers = "Drawers",
		ModuleEditors = "Editors",
		ModuleTools = "Tools",
		InvalidColumnKind = "Invalid column kind: ",
		Column0IsUsedTwice = "Column '{0}' is used twice.",
		CannotSetSelectedText = "Cannot set selected text because there is no selection.",
		EditorNoSelection = "There is no selection.",
		EditorBadSelection = "This kind of selection is not supported.";
};
}

// Helpers
int ParseInt(String^ value, int fallback);
KeyInfo^ KeyInfoFromInputRecord(const INPUT_RECORD& ir);
MouseInfo^ GetMouseInfo(const MOUSE_EVENT_RECORD& m);
void AssertCurrentViewer();
void DeleteSourceOptional(String^ path, DeleteSource option);
void SetPanelDirectory(HANDLE handle, String^ path);
int Call_ACTL_GETWINDOWCOUNT();
void Call_ACTL_GETWINDOWINFO(WindowInfo& wi, int index);
void Call_ACTL_GETWINDOWINFO(WindowInfo& wi);
void Call_DM_GETDLGITEM(CBin& bin, FarGetDialogItem& gdi, HANDLE hDlg, int item);

Guid FromGUID(const GUID& guid);
GUID ToGUID(Guid guid);
