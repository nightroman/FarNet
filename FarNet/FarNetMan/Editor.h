
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

#pragma once

namespace FarNet
{;
ref class AnyEditor : IAnyEditor
{
public: DEF_EVENT_ARGS_IMP(Changed, _Changed, EditorChangedEventArgs);
public: DEF_EVENT_ARGS_IMP(KeyDown, _KeyDown, KeyEventArgs);
public: DEF_EVENT_ARGS_IMP(KeyUp, _KeyUp, KeyEventArgs);
public: DEF_EVENT_ARGS_IMP(MouseClick, _MouseClick, MouseEventArgs);
public: DEF_EVENT_ARGS_IMP(MouseDoubleClick, _MouseDoubleClick, MouseEventArgs);
public: DEF_EVENT_ARGS_IMP(MouseMove, _MouseMove, MouseEventArgs);
public: DEF_EVENT_ARGS_IMP(MouseWheel, _MouseWheel, MouseEventArgs);
public: DEF_EVENT_ARGS_IMP(Saving, _Saving, EditorSavingEventArgs);
public: DEF_EVENT_IMP(Closed, _Closed);
public: DEF_EVENT_IMP(CtrlCPressed, _CtrlCPressed);
public: DEF_EVENT_IMP(GotFocus, _GotFocus);
public: DEF_EVENT_IMP(Idled, _Idled);
public: DEF_EVENT_IMP(LosingFocus, _LosingFocus);
public: DEF_EVENT_IMP(Opened, _Opened);
public: DEF_EVENT_IMP(Redrawing, _Redrawing);
public:
	virtual String^ EditText(String^ text, String^ title) override;
};

ref class DrawerInfo sealed
{
public:
	Guid Id;
	int Priority;
	EventHandler<ModuleDrawerEventArgs^>^ Handler;
};

ref class Editor sealed : IEditor
{
public: DEF_EVENT_ARGS_IMP(Changed, _Changed, EditorChangedEventArgs);
public: DEF_EVENT_ARGS_IMP(KeyDown, _KeyDown, KeyEventArgs);
public: DEF_EVENT_ARGS_IMP(KeyUp, _KeyUp, KeyEventArgs);
public: DEF_EVENT_ARGS_IMP(MouseClick, _MouseClick, MouseEventArgs);
public: DEF_EVENT_ARGS_IMP(MouseDoubleClick, _MouseDoubleClick, MouseEventArgs);
public: DEF_EVENT_ARGS_IMP(MouseMove, _MouseMove, MouseEventArgs);
public: DEF_EVENT_ARGS_IMP(MouseWheel, _MouseWheel, MouseEventArgs);
public: DEF_EVENT_ARGS_IMP(Saving, _Saving, EditorSavingEventArgs);
public: DEF_EVENT_IMP(Closed, _Closed);
public: DEF_EVENT_IMP(CtrlCPressed, _CtrlCPressed);
public: DEF_EVENT_IMP(GotFocus, _GotFocus);
public: DEF_EVENT_IMP(Idled, _Idled);
public: DEF_EVENT_IMP(LosingFocus, _LosingFocus);
public: DEF_EVENT_IMP(Opened, _Opened);
public: DEF_EVENT_IMP(Redrawing, _Redrawing);
public:
	virtual property bool DisableHistory { bool get() override; void set(bool value) override; }
	virtual property bool IsLocked { bool get() override; void set(bool value) override; }
	virtual property bool IsModified { bool get() override; }
	virtual property bool IsOpened { bool get() override; }
	virtual property bool IsSaved { bool get() override; }
	virtual property bool IsVirtualSpace { bool get() override; void set(bool value) override; }
	virtual property bool Overtype { bool get() override; void set(bool value) override; }
	virtual property bool SelectionExists { bool get() override; }
	virtual property bool ShowWhiteSpace { bool get() override; void set(bool value) override; }
	virtual property bool WriteByteOrderMark { bool get() override; void set(bool value) override; }
	virtual property DateTime TimeOfOpen { DateTime get() override; }
	virtual property DateTime TimeOfSave { DateTime get() override; }
	virtual property FarNet::DeleteSource DeleteSource { FarNet::DeleteSource get() override; void set(FarNet::DeleteSource value) override; }
	virtual property ExpandTabsMode ExpandTabs { ExpandTabsMode get() override; void set(ExpandTabsMode value) override; }
	virtual property IEditorBookmark^ Bookmark { IEditorBookmark^ get() override; }
	virtual property ILine^ default[int] { ILine^ get(int index) override; }
	virtual property ILine^ Line { ILine^ get() override; }
	virtual property IList<ILine^>^ Lines { IList<ILine^>^ get() override; }
	virtual property IList<ILine^>^ SelectedLines { IList<ILine^>^ get() override; }
	virtual property IList<String^>^ Strings { IList<String^>^ get() override; }
	virtual property int CodePage { int get() override; void set(int value) override; }
	virtual property int Count { int get() override; }
	virtual property int KeyCount { int get() override; }
	virtual property int TabSize { int get() override; void set(int value) override; }
	virtual property IntPtr Id { IntPtr get() override; }
	virtual property Place SelectionPlace { Place get() override; }
	virtual property Place Window { Place get() override; void set(Place value) override; }
	virtual property Point Caret { Point get() override; void set(Point value) override; }
	virtual property Point SelectionPoint { Point get() override; }
	virtual property Point WindowSize { Point get() override; }
	virtual property PlaceKind SelectionKind { PlaceKind get() override; }
	virtual property String^ FileName { String^ get() override; void set(String^ value) override; }
	virtual property String^ Title { String^ get() override; void set(String^ value) override; }
	virtual property String^ WordDiv { String^ get() override; void set(String^ value) override; }
	virtual property FarNet::Switching Switching { FarNet::Switching get() override; void set(FarNet::Switching value) override; }
	virtual property TextFrame Frame { TextFrame get() override; void set(TextFrame value) override; }
public:
	virtual IList<EditorColorInfo^>^ GetColors(int line) override;
	virtual int ConvertColumnEditorToScreen(int line, int column) override;
	virtual int ConvertColumnScreenToEditor(int line, int column) override;
	virtual Point ConvertPointEditorToScreen(Point point) override;
	virtual Point ConvertPointScreenToEditor(Point point) override;
	virtual String^ GetSelectedText(String^ separator) override;
	virtual String^ GetText(String^ separator) override;
	virtual TextWriter^ OpenWriter() override;
	virtual void Activate() override;
	virtual void Add(String^ text) override;
	virtual void AddDrawer(IModuleDrawer^ drawer) override;
	virtual void BeginAsync() override;
	virtual void BeginUndo() override;
	virtual void Clear() override;
	virtual void Close() override;
	virtual void DeleteChar() override;
	virtual void DeleteLine() override;
	virtual void DeleteText() override;
	virtual void EndAsync() override;
	virtual void EndUndo() override;
	virtual void GoTo(int column, int line) override;
	virtual void GoToColumn(int pos) override;
	virtual void GoToEnd(bool addLine) override;
	virtual void GoToLine(int line) override;
	virtual void Insert(int line, String^ text) override;
	virtual void InsertChar(Char text) override;
	virtual void InsertLine() override;
	virtual void InsertLine(bool indent) override;
	virtual void InsertText(String^ text) override;
	virtual void Open() override;
	virtual void Open(OpenMode mode) override;
	virtual void Redo() override;
	virtual void Redraw() override;
	virtual void RemoveAt(int index) override;
	virtual void RemoveDrawer(Guid id) override;
	virtual void Save() override;
	virtual void Save(bool force) override;
	virtual void Save(String^ fileName) override;
	virtual void SelectAllText() override;
	virtual void SelectText(int column1, int line1, int column2, int line2, PlaceKind kind) override;
	virtual void SetSelectedText(String^ text) override;
	virtual void SetText(String^ text) override;
	virtual void Undo() override;
	virtual void UnselectText() override;
internal:
	Editor();
	void InvokeDrawers();
	void Sync();
	void Start(const EditorInfo& ei, bool waiting);
	void Stop();
private:
	static void AddColors(Guid owner, int priority, IEnumerable<EditorColor^>^ colors);
	static void RemoveColors(Guid owner, int startLine, int endLine);
	void AssertClosed();
	bool GetBoolOption(int option, Nullable<bool> value);
	void SetBoolOption(EDITOR_SETPARAMETER_TYPES option, bool value);
private:
	intptr_t _id;
	String^ _FileName;
	DateTime _TimeOfOpen;
	String^ _WordDiv; // null = none
	Nullable<bool> _IsLocked;
	Nullable<bool> _IsVirtualSpace;
	Nullable<bool> _ShowWhiteSpace;
	Nullable<bool> _WriteByteOrderMark;
private:
	FarNet::DeleteSource _DeleteSource;
	FarNet::Switching _Switching;
	bool _DisableHistory;
	Place _Window;
	String^ _Title;
	intptr_t _CodePage;
	TextFrame _frameStart;
internal:
	DateTime _TimeOfSave;
	int _KeyCount;
	// async stuff
	HANDLE _hMutex;
	StringBuilder^ _output;
	Dictionary<Guid, DrawerInfo^>^ _drawers;
};
}
