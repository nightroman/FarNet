#pragma once

namespace FarManagerImpl
{;
ref class EditorManager;
ref class SelectionCollection;
ref class VisibleEditorCursor;
ref class VisibleEditorLineCollection;

public ref class Editor : public BaseEditor, public IEditor
{
public:
	virtual property bool Async { bool get(); void set(bool value); }
	virtual property bool DeleteOnClose { bool get(); void set(bool value); }
	virtual property bool DeleteOnlyFileOnClose { bool get(); void set(bool value); }
	virtual property bool DisableHistory { bool get(); void set(bool value); }
	virtual property bool EnableSwitch { bool get(); void set(bool value); }
	virtual property bool IsLocked { bool get(); }
	virtual property bool IsModal { bool get(); void set(bool value); }
	virtual property bool IsModified { bool get(); }
	virtual property bool IsNew { bool get(); void set(bool value); }
	virtual property bool IsOpened { bool get(); }
	virtual property bool IsSaved { bool get(); }
	virtual property bool Overtype { bool get(); void set(bool value); }
	virtual property ExpandTabsMode ExpandTabs { ExpandTabsMode get(); void set(ExpandTabsMode value); }
	virtual property ICursor^ Cursor { ICursor^ get(); }
	virtual property ILines^ Lines { ILines^ get(); }
	virtual property int Id { int get(); void set(int value); }
	virtual property int TabSize { int get(); void set(int value); }
	virtual property IRect^ Window { IRect^ get(); void set(IRect^ value); }
	virtual property ISelection^ Selection { ISelection^ get(); }
	virtual property String^ FileName { String^ get(); void set(String^ value); }
	virtual property String^ Title { String^ get(); void set(String^ value); }
	virtual property String^ WordDiv { String^ get(); void set(String^ value); }
public:
	virtual void Close();
	virtual void DeleteChar();
	virtual void DeleteLine();
	virtual void Insert(String^ text);
	virtual void InsertLine();
	virtual void InsertLine(bool indent);
	virtual void Open();
	virtual void Redraw();
	virtual void Save();
	virtual void Save(String^ fileName);
	virtual ICollection<ICursor^>^ Bookmarks();
internal:
	Editor(EditorManager^ manager);
	void GetParams();
private:
	int Flags();
	void BecomeClosed();
	void BecomeOpened();
	void EnsureClosed();
	void EnsureCurrent();
	void EnsureCurrent(EditorInfo& ei);
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
	IRect^ _window;
	SelectionCollection^ _selection;
	String^ _fileName;
	String^ _title;
	VisibleEditorLineCollection^ _lines;
	// cursor
	ProxyEditorCursor^ _cursor;
	StoredEditorCursor^ _storedCursor;
	VisibleEditorCursor^ _openedCursor;
};
}
