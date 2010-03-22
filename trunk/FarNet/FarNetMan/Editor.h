/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class SelectionCollection;
ref class VisibleEditorCursor;
ref class EditorLineCollection;

ref class AnyEditor : IAnyEditor
{
public: DEF_EVENT(Closed, _Closed);
public: DEF_EVENT(CtrlCPressed, _CtrlCPressed);
public: DEF_EVENT(GotFocus, _GotFocus);
public: DEF_EVENT(Idled, _Idled);
public: DEF_EVENT(LosingFocus, _LosingFocus);
public: DEF_EVENT(Opened, _Opened);
public: DEF_EVENT(Saving, _Saving);
public: DEF_EVENT_ARGS(OnKey, _OnKey, KeyEventArgs);
public: DEF_EVENT_ARGS(OnMouse, _OnMouse, MouseEventArgs);
public: DEF_EVENT_ARGS(OnRedraw, _OnRedraw, RedrawEventArgs);
public:
	virtual property String^ WordDiv { String^ get(); void set(String^ value); }
	virtual String^ EditText(String^ text, String^ title);
};

ref class Editor : public AnyEditor, public IEditor
{
public:
	virtual property bool DisableHistory { bool get(); void set(bool value); }
	virtual property bool IsLastLine { bool get(); }
	virtual property bool IsLocked { bool get(); }
	virtual property bool IsModified { bool get(); }
	virtual property bool IsNew { bool get(); void set(bool value); }
	virtual property bool IsOpened { bool get(); }
	virtual property bool IsSaved { bool get(); }
	virtual property bool Overtype { bool get(); void set(bool value); }
	virtual property bool SelectionExists { bool get(); }
	virtual property bool ShowWhiteSpace { bool get(); void set(bool value); }
	virtual property bool WriteByteOrderMark { bool get(); void set(bool value); }
	virtual property DeleteSource DeleteSource { FarNet::DeleteSource get(); void set(FarNet::DeleteSource value); }
	virtual property ExpandTabsMode ExpandTabs { ExpandTabsMode get(); void set(ExpandTabsMode value); }
	virtual property ILine^ default[int] { ILine^ get(int index); }
	virtual property int CodePage { int get(); void set(int value); }
	virtual property int Count { int get(); }
	virtual property int Id { int get(); }
	virtual property int TabSize { int get(); void set(int value); }
	virtual property Object^ Data;
	virtual property Object^ Host { Object^ get(); void set(Object^ value); }
	virtual property Place SelectionPlace { Place get(); }
	virtual property Place Window { Place get(); void set(Place value); }
	virtual property Point Caret { Point get(); void set(Point value); }
	virtual property Point WindowSize { Point get(); }
	virtual property RegionKind SelectionKind { RegionKind get(); }
	virtual property String^ FileName { String^ get(); void set(String^ value); }
	virtual property String^ Title { String^ get(); void set(String^ value); }
	virtual property String^ WordDiv { String^ get() override; void set(String^ value) override; }
	virtual property Switching Switching { FarNet::Switching get(); void set(FarNet::Switching value); }
	virtual property TextFrame Frame { TextFrame get(); void set(TextFrame value); }
public:
	virtual ICollection<TextFrame>^ Bookmarks();
	virtual ILineCollection^ Lines(bool ignoreEmptyLast);
	virtual ILineCollection^ SelectedLines(bool ignoreEmptyLast);
	virtual int ConvertColumnEditorToScreen(int line, int column);
	virtual int ConvertColumnScreenToEditor(int line, int column);
	virtual Point ConvertPointScreenToEditor(Point point);
	virtual String^ GetSelectedText();
	virtual String^ GetSelectedText(String^ separator);
	virtual String^ GetText();
	virtual String^ GetText(String^ separator);
	virtual TextWriter^ CreateWriter();
	virtual void Begin();
	virtual void BeginAsync();
	virtual void BeginUndo();
	virtual void Close();
	virtual void DeleteChar();
	virtual void DeleteLine();
	virtual void DeleteText();
	virtual void End();
	virtual void EndAsync();
	virtual void EndUndo();
	virtual void GoTo(int column, int line);
	virtual void GoToColumn(int pos);
	virtual void GoToEnd(bool addLine);
	virtual void GoToLine(int line);
	virtual void InsertChar(Char text);
	virtual void InsertLine();
	virtual void InsertLine(bool indent);
	virtual void InsertText(String^ text);
	virtual void Open();
	virtual void Open(OpenMode mode);
	virtual void Redo();
	virtual void Redraw();
	virtual void RemoveAt(int index);
	virtual void Save();
	virtual void Save(String^ fileName);
	virtual void SelectAllText();
	virtual void SelectText(RegionKind kind, int column1, int line1, int column2, int line2);
	virtual void SetSelectedText(String^ text);
	virtual void SetText(String^ text);
	virtual void Undo();
	virtual void UnselectText();
internal:
	Editor();
	void Sync();
	void Start(const EditorInfo& ei, bool waiting);
	void Stop();
private:
	void AssertClosed();
	bool GetBoolOption(int option, bool value);
	void SetBoolOption(int option, bool value);
private:
	int _id;
	String^ _FileName;
	bool _ShowWhiteSpace;
	bool _ShowWhiteSpaceSet;
	bool _WriteByteOrderMark;
	bool _WriteByteOrderMarkSet;
	String^ _WordDiv;
	bool _WordDivSet;
private:
	FarNet::DeleteSource _DeleteSource;
	FarNet::Switching _Switching;
	bool _DisableHistory;
	bool _IsNew;
	Place _Window;
	String^ _Title;
	int _CodePage;
	TextFrame _frameStart;
	TextFrame _frameSaved;
	Object^ _Host;
internal:
	// async stuff
	HANDLE _hMutex;
	StringBuilder^ _output;
};
}
