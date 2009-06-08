/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class Far;
ref class FarDialog;
ref class ListItemCollection;

ref class FarControl abstract : public IControl
{
public:
	virtual property bool Disabled { bool get(); void set(bool value); }
	virtual property bool Hidden { bool get(); void set(bool value); }
	virtual property int Id { int get() { return _id; } }
	virtual property Place Rect { Place get(); void set(Place value); }
	virtual property String^ Text { String^ get(); void set(String^ value); }
public: DEF_EVENT_ARGS(Drawing, _Drawing, DrawingEventArgs);
public: DEF_EVENT_ARGS(GotFocus, _GotFocus, AnyEventArgs);
public: DEF_EVENT_ARGS(KeyPressed, _KeyPressed, KeyPressedEventArgs);
public: DEF_EVENT_ARGS(LosingFocus, _LosingFocus, LosingFocusEventArgs);
public: DEF_EVENT_ARGS(MouseClicked, _MouseClicked, MouseClickedEventArgs);
public:
	virtual String^ ToString() override;
protected:
	FarControl(FarDialog^ dialog, int index);
internal:
	FarControl(FarDialog^ dialog, int left, int top, int right, int bottom, String^ text);
	bool GetFlag(int flag);
	void SetFlag(int flag, bool value);
	int GetSelected();
	void SetSelected(int value);
	virtual void Init(FarDialogItem& item, int type);
	virtual void Starting(FarDialogItem& item) = 0;
	virtual void Started() {}
	virtual void Stop(bool ok);
	virtual void Free();
internal:
	int _id;
	Place _rect;
	FarDialogItem* _item;
	FarDialog^ _dialog;
	long _flags;
	int _selected;
	String^ _text;
};

ref class FarBox : public FarControl, public IBox
{
public:
	virtual property bool LeftText { bool get(); void set(bool value); }
	virtual property bool ShowAmpersand { bool get(); void set(bool value); }
	virtual property bool Single;
internal:
	FarBox(FarDialog^ dialog, int index);
	FarBox(FarDialog^ dialog, int left, int top, int right, int bottom, String^ text);
	virtual void Starting(FarDialogItem& item) override;
};

ref class FarButton : public FarControl, public IButton
{
public:
	virtual property bool CenterGroup { bool get(); void set(bool value); }
	virtual property bool NoBrackets { bool get(); void set(bool value); }
	virtual property bool NoClose { bool get(); void set(bool value); }
	virtual property bool NoFocus { bool get(); void set(bool value); }
	virtual property bool ShowAmpersand { bool get(); void set(bool value); }
public: DEF_EVENT_ARGS(ButtonClicked, _ButtonClicked, ButtonClickedEventArgs);
internal:
	FarButton(FarDialog^ dialog, int index);
	FarButton(FarDialog^ dialog, int left, int top, String^ text);
	virtual void Starting(FarDialogItem& item) override;
};

ref class FarCheckBox : public FarControl, public ICheckBox
{
public:
	virtual property bool CenterGroup { bool get(); void set(bool value); }
	virtual property bool NoFocus { bool get(); void set(bool value); }
	virtual property bool ShowAmpersand { bool get(); void set(bool value); }
	virtual property bool ThreeState { bool get(); void set(bool value); }
	virtual property int Selected { int get(); void set(int value); }
public: DEF_EVENT_ARGS(ButtonClicked, _ButtonClicked, ButtonClickedEventArgs);
internal:
	FarCheckBox(FarDialog^ dialog, int index);
	FarCheckBox(FarDialog^ dialog, int left, int top, String^ text);
	virtual void Starting(FarDialogItem& item) override;
};

ref class FarEdit : public FarControl, public IEdit
{
public:
	virtual property bool Editor { bool get(); void set(bool value); }
	virtual property bool EnvExpanded { bool get(); void set(bool value); }
	virtual property bool Fixed { bool get(); }
	virtual property bool ManualAddHistory { bool get(); void set(bool value); }
	virtual property bool NoAutoComplete { bool get(); void set(bool value); }
	virtual property bool NoFocus { bool get(); void set(bool value); }
	virtual property bool Password { bool get(); }
	virtual property bool ReadOnly { bool get(); void set(bool value); }
	virtual property bool SelectOnEntry { bool get(); void set(bool value); }
	virtual property bool UseLastHistory { bool get(); void set(bool value); }
	virtual property ILine^ Line { ILine^ get(); }
	virtual property String^ History { String^ get(); void set(String^ value); }
	virtual property String^ Mask { String^ get(); void set(String^ value); }
public: DEF_EVENT_ARGS(TextChanged, _TextChanged, TextChangedEventArgs);
internal:
	FarEdit(FarDialog^ dialog, int index, int type);
	FarEdit(FarDialog^ dialog, int left, int top, int right, String^ text, int type);
	virtual void Starting(FarDialogItem& item) override;
	virtual void Stop(bool ok) override;
	virtual void Free() override;
private:
	int _type;
	String^ _history;
};

ref class FarRadioButton : public FarControl, public IRadioButton
{
public:
	virtual property bool CenterGroup { bool get(); void set(bool value); }
	virtual property bool Group { bool get(); void set(bool value); }
	virtual property bool MoveSelect { bool get(); void set(bool value); }
	virtual property bool NoFocus { bool get(); void set(bool value); }
	virtual property bool Selected { bool get(); void set(bool value); }
	virtual property bool ShowAmpersand { bool get(); void set(bool value); }
public: DEF_EVENT_ARGS(ButtonClicked, _ButtonClicked, ButtonClickedEventArgs);
internal:
	FarRadioButton(FarDialog^ dialog, int index);
	FarRadioButton(FarDialog^ dialog, int left, int top, String^ text);
	virtual void Starting(FarDialogItem& item) override;
};

ref class FarText : public FarControl, public IText
{
public:
	virtual property bool BoxColor { bool get(); void set(bool value); }
	virtual property bool Centered { bool get(); void set(bool value); }
	virtual property bool CenterGroup { bool get(); void set(bool value); }
	virtual property bool ShowAmpersand { bool get(); void set(bool value); }
	virtual property bool Vertical { bool get(); }
	virtual property int Separator { int get(); void set(int value); }
internal:
	FarText(FarDialog^ dialog, int index);
	FarText(FarDialog^ dialog, int left, int top, int right, int bottom, String^ text);
	virtual void Starting(FarDialogItem& item) override;
};

ref class FarUserControl : public FarControl, public IUserControl
{
public:
	virtual property bool NoFocus { bool get(); void set(bool value); }
internal:
	FarUserControl(FarDialog^ dialog, int index);
	FarUserControl(FarDialog^ dialog, int left, int top, int right, int bottom);
	virtual void Starting(FarDialogItem& item) override;
};

ref class FarBaseList abstract : public FarControl, public IBaseList
{
public:
	virtual property bool AutoAssignHotkeys { bool get(); void set(bool value); }
	virtual property bool NoAmpersands { bool get(); void set(bool value); }
	virtual property bool NoClose { bool get(); void set(bool value); }
	virtual property bool NoFocus { bool get(); void set(bool value); }
	virtual property bool SelectLast;
	virtual property bool WrapCursor { bool get(); void set(bool value); }
	virtual property IList<IMenuItem^>^ Items { IList<IMenuItem^>^ get(); }
	virtual property int Selected { int get(); void set(int value); }
public:
	virtual IMenuItem^ Add(String^ text);
	virtual void AttachItems();
	virtual void DetachItems();
	virtual void Clear() { _Items->Clear(); } // Bug [_090208_042536]
protected:
	FarBaseList(FarDialog^ dialog, int index);
	FarBaseList(FarDialog^ dialog, int left, int top, int right, int bottom, String^ text);
internal:
	virtual void Init(FarDialogItem& item, int type) override;
	virtual void Free() override;
	void FreeItems();
	static void InitFarListItem(FarListItem& i2, IMenuItem^ i1);
	static void InitFarListItemShort(FarListItem& i2, IMenuItem^ i1);
internal:
	IList<IMenuItem^>^ _Items;
	List<int>^ _ii;
private:
	FarList* _pFarList;
};

ref class FarComboBox : public FarBaseList, public IComboBox
{
public:
	virtual property bool DropDownList { bool get(); void set(bool value); }
	virtual property bool EnvExpanded { bool get(); void set(bool value); }
	virtual property bool ReadOnly { bool get(); void set(bool value); }
	virtual property bool SelectOnEntry { bool get(); void set(bool value); }
	virtual property ILine^ Line { ILine^ get(); }
public: DEF_EVENT_ARGS(TextChanged, _TextChanged, TextChangedEventArgs);
internal:
	FarComboBox(FarDialog^ dialog, int index);
	FarComboBox(FarDialog^ dialog, int left, int top, int right, String^ text);
	virtual void Starting(FarDialogItem& item) override;
	virtual void Stop(bool ok) override;
};

ref class FarListBox : public FarBaseList, public IListBox
{
public:
	virtual property bool NoBox { bool get(); void set(bool value); }
	virtual property String^ Bottom { String^ get(); void set(String^ value); }
	virtual property String^ Text { String^ get() override; void set(String^ value) override; }
	virtual property String^ Title { String^ get(); void set(String^ value); }
public:
	virtual void SetFrame(int selected, int top);
internal:
	FarListBox(FarDialog^ dialog, int index);
	FarListBox(FarDialog^ dialog, int left, int top, int right, int bottom, String^ text);
	virtual void Started() override;
	virtual void Starting(FarDialogItem& item) override;
private:
	String^ _Title;
	String^ _Bottom;
};

ref class FarDialog : IDialog
{
public: DEF_EVENT(Idled, _Idled);
public: DEF_EVENT_ARGS(Closing, _Closing, ClosingEventArgs);
public: DEF_EVENT_ARGS(Initialized, _Initialized, InitializedEventArgs);
public: DEF_EVENT_ARGS(KeyPressed, _KeyPressed, KeyPressedEventArgs);
public: DEF_EVENT_ARGS(MouseClicked, _MouseClicked, MouseClickedEventArgs);
public:
	virtual property bool IsSmall { bool get(); void set(bool value); }
	virtual property bool IsWarning { bool get(); void set(bool value); }
	virtual property bool NoPanel { bool get(); void set(bool value); }
	virtual property bool NoShadow { bool get(); void set(bool value); }
	virtual property bool NoSmartCoords;
	virtual property IButton^ Cancel;
	virtual property IControl^ Default { IControl^ get(); void set(IControl^ value); }
	virtual property IControl^ Focused { IControl^ get(); void set(IControl^ value); }
	virtual property IControl^ Selected { IControl^ get(); }
	virtual property Place Rect { Place get(); void set(Place value); }
	virtual property String^ HelpTopic;
	virtual property Object^ Data;
public:
	virtual bool Show();
	virtual IBox^ AddBox(int left, int top, int right, int bottom, String^ text);
	virtual IButton^ AddButton(int left, int top, String^ text);
	virtual ICheckBox^ AddCheckBox(int left, int top, String^ text);
	virtual IComboBox^ AddComboBox(int left, int top, int right, String^ text);
	virtual IControl^ GetControl(int id);
	virtual IEdit^ AddEdit(int left, int top, int right, String^ text);
	virtual IEdit^ AddEditFixed(int left, int top, int right, String^ text);
	virtual IEdit^ AddEditPassword(int left, int top, int right, String^ text);
	virtual IListBox^ AddListBox(int left, int top, int right, int bottom, String^ title);
	virtual IRadioButton^ AddRadioButton(int left, int top, String^ text);
	virtual IText^ AddText(int left, int top, int right, String^ text);
	virtual IText^ AddVerticalText(int left, int top, int bottom, String^ text);
	virtual IUserControl^ AddUserControl(int left, int top, int right, int bottom);
	virtual void Close();
	virtual void Move(Point point, bool absolute);
	virtual void Resize(Point size);
	virtual void SetFocus(int id);
internal:
	FarDialog(HANDLE hDlg);
	FarDialog(int left, int top, int right, int bottom);
	static int AsProcessDialogEvent(int id, void* param);
	LONG_PTR DialogProc(int msg, int param1, LONG_PTR param2);
	static FarDialog^ GetDialog();
internal:
	static List<FarDialog^> _dialogs;
	static HANDLE _hDlgTop = INVALID_HANDLE_VALUE;
	HANDLE _hDlg;
	List<FarControl^>^ _items;
private:
	void AddItem(FarControl^ item);
private:
	int _flags;
	Place _rect;
	FarControl^ _default;
	FarControl^ _focused;
	FarControl^ _selected;
};
}
