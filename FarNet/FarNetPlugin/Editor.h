/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2007 FAR.NET Team
*/

#pragma once

namespace FarNet
{;
ref class EditorManager;
ref class SelectionCollection;
ref class VisibleEditorCursor;
ref class EditorLineCollection;

ref class BaseEditor : IAnyEditor
{
public: DEF_EVENT(AfterClose, _AfterClose);
public: DEF_EVENT(AfterOpen, _AfterOpen);
public: DEF_EVENT(BeforeSave, _BeforeSave);
public: DEF_EVENT(GotFocus, _GotFocus);
public: DEF_EVENT(LosingFocus, _LosingFocus);
public: DEF_EVENT_ARGS(OnKey, _OnKey, KeyEventArgs);
public: DEF_EVENT_ARGS(OnMouse, _OnMouse, MouseEventArgs);
public: DEF_EVENT_ARGS(OnRedraw, _OnRedraw, RedrawEventArgs);
public:
	virtual property String^ WordDiv { String^ get(); void set(String^ value); }
	virtual String^ EditText(String^ text, String^ title);
};

ref class Editor : public BaseEditor, public IEditor
{
public:
	virtual property bool Async { bool get(); void set(bool value); }
	virtual property bool DeleteOnClose { bool get(); void set(bool value); }
	virtual property bool DeleteOnlyFileOnClose { bool get(); void set(bool value); }
	virtual property bool DisableHistory { bool get(); void set(bool value); }
	virtual property bool EnableSwitch { bool get(); void set(bool value); }
	virtual property bool IsEnd { bool get(); }
	virtual property bool IsLocked { bool get(); }
	virtual property bool IsModal { bool get(); void set(bool value); }
	virtual property bool IsModified { bool get(); }
	virtual property bool IsNew { bool get(); void set(bool value); }
	virtual property bool IsOpened { bool get(); }
	virtual property bool IsSaved { bool get(); }
	virtual property bool Overtype { bool get(); void set(bool value); }
	virtual property ExpandTabsMode ExpandTabs { ExpandTabsMode get(); void set(ExpandTabsMode value); }
	virtual property ILine^ CurrentLine { ILine^ get(); }
	virtual property ILines^ Lines { ILines^ get(); }
	virtual property ILines^ TrueLines { ILines^ get(); }
	virtual property int Id { int get(); void set(int value); }
	virtual property int TabSize { int get(); void set(int value); }
	virtual property ISelection^ Selection { ISelection^ get(); }
	virtual property ISelection^ TrueSelection { ISelection^ get(); }
	virtual property Object^ Data;
	virtual property Place Window { Place get(); }
	virtual property Point Cursor { Point get(); }
	virtual property String^ FileName { String^ get(); void set(String^ value); }
	virtual property String^ Title { String^ get(); void set(String^ value); }
	virtual property String^ WordDiv { String^ get() override; void set(String^ value) override; }
	virtual property TextFrame Frame { TextFrame get(); void set(TextFrame value); }
public:
	virtual ICollection<TextFrame>^ Bookmarks();
	virtual int ConvertPosToTab(int line, int pos);
	virtual int ConvertTabToPos(int line, int tab);
	virtual Point ConvertScreenToCursor(Point screen);
	virtual String^ GetText() { return GetText(CV::CRLF); }
	virtual String^ GetText(String^ separator);
	virtual void Begin();
	virtual void Close();
	virtual void DeleteChar();
	virtual void DeleteLine();
	virtual void End();
	virtual void GoEnd(bool addLine);
	virtual void GoTo(int pos, int line);
	virtual void GoToLine(int line);
	virtual void GoToPos(int pos);
	virtual void Insert(String^ text);
	virtual void InsertLine();
	virtual void InsertLine(bool indent);
	virtual void Open();
	virtual void Open(OpenMode mode);
	virtual void Redraw();
	virtual void Save();
	virtual void Save(String^ fileName);
	virtual void SetText(String^ text);
internal:
	Editor(EditorManager^ manager);
	void GetParams();
private:
	[CA_USED]
	void AssertClosed();
	[CA_USED]
	void AssertCurrent();
	void CurrentInfo(EditorInfo& ei);
private:
	EditorManager^ _manager;
	int _id;
	bool _async;
	bool _deleteOnClose;
	bool _deleteOnlyFileOnClose;
	bool _disableHistory;
	bool _enableSwitch;
	bool _isModal;
	bool _isNew;
	Place _window;
	String^ _fileName;
	String^ _title;
	TextFrame _frameStart;
	TextFrame _frameSaved;
};
}
