/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2007 FAR.NET Team
*/

#pragma once

namespace FarNet
{;
ref class Far;
ref class FarDialog;
ref class MenuItemCollection;

ref class FarControl abstract : public IControl
{
public:
	virtual property bool Disabled { bool get(); void set(bool value); }
	virtual property bool Hidden { bool get(); void set(bool value); }
	virtual property int Id;
	virtual property Place Rect { Place get() { return _rect; } }
	virtual property String^ Text { String^ get(); void set(String^ value); }
public: DEF_EVENT_ARGS(GotFocus, _GotFocus, AnyEventArgs);
public: DEF_EVENT_ARGS(KeyPressed, _KeyPressed, KeyPressedEventArgs);
public: DEF_EVENT_ARGS(LosingFocus, _LosingFocus, LosingFocusEventArgs);
public: DEF_EVENT_ARGS(MouseClicked, _MouseClicked, MouseClickedEventArgs);
public:
	virtual String^ ToString() override;
internal:
	FarControl(FarDialog^ dialog, int left, int top, int right, int bottom, String^ text);
	bool GetFlag(int flag);
	void SetFlag(int flag, bool value);
	int GetSelected();
	void SetSelected(int value);
	virtual void Setup(FarDialogItem& item, int type);
	virtual void Setup(FarDialogItem& item) = 0;
	virtual void Update(bool ok);
internal:
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
	FarBox(FarDialog^ dialog, int left, int top, int right, int bottom, String^ text);
	virtual void Setup(FarDialogItem& item) override;
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
	FarButton(FarDialog^ dialog, int left, int top, String^ text);
	virtual void Setup(FarDialogItem& item) override;
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
	FarCheckBox(FarDialog^ dialog, int left, int top, String^ text);
	virtual void Setup(FarDialogItem& item) override;
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
	FarEdit(FarDialog^ dialog, int left, int top, int right, String^ text, int type);
	virtual void Setup(FarDialogItem& item) override;
	virtual void Update(bool ok) override;
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
	FarRadioButton(FarDialog^ dialog, int left, int top, String^ text);
	virtual void Setup(FarDialogItem& item) override;
};

ref class FarText : public FarControl, public IText
{
public:
	virtual property bool BoxColor { bool get(); void set(bool value); }
	virtual property bool Centered { bool get(); void set(bool value); }
	virtual property bool CenterGroup { bool get(); void set(bool value); }
	virtual property bool Separator { bool get(); void set(bool value); }
	virtual property bool Separator2 { bool get(); void set(bool value); }
	virtual property bool ShowAmpersand { bool get(); void set(bool value); }
internal:
	FarText(FarDialog^ dialog, int left, int top, int right, int bottom, String^ text);
	virtual void Setup(FarDialogItem& item) override;
};

ref class FarUserControl : public FarControl, public IUserControl
{
public:
	virtual property bool NoFocus { bool get(); void set(bool value); }
internal:
	FarUserControl(FarDialog^ dialog, int left, int top, int right, int bottom);
	virtual void Setup(FarDialogItem& item) override;
};

ref class FarBaseBox abstract : public FarControl, public IBaseList
{
public:
	virtual property bool AutoAssignHotkeys { bool get(); void set(bool value); }
	virtual property bool NoAmpersands { bool get(); void set(bool value); }
	virtual property bool NoClose { bool get(); void set(bool value); }
	virtual property bool NoFocus { bool get(); void set(bool value); }
	virtual property bool SelectLast;
	virtual property bool WrapCursor { bool get(); void set(bool value); }
	virtual property IMenuItems^ Items { IMenuItems^ get(); }
	virtual property int Selected { int get(); void set(int value); }
public:
	virtual IMenuItem^ Add(String^ text);
protected:
	[CA_USED]
	FarBaseBox(FarDialog^ dialog, int left, int top, int right, int bottom, String^ text);
internal:
	virtual void Setup(FarDialogItem& item, int type) override;
	virtual void Update(bool ok) override;
internal:
	MenuItemCollection^ _items;
	List<int>^ _ii;
private:
	FarList* _pFarList;
};

ref class FarComboBox : public FarBaseBox, public IComboBox
{
public:
	virtual property bool DropDownList { bool get(); void set(bool value); }
	virtual property bool EnvExpanded { bool get(); void set(bool value); }
	virtual property bool ReadOnly { bool get(); void set(bool value); }
	virtual property bool SelectOnEntry { bool get(); void set(bool value); }
	virtual property ILine^ Line { ILine^ get(); }
public: DEF_EVENT_ARGS(TextChanged, _TextChanged, TextChangedEventArgs);
internal:
	FarComboBox(FarDialog^ dialog, int left, int top, int right, String^ text);
	virtual void Setup(FarDialogItem& item) override;
};

ref class FarListBox : public FarBaseBox, public IListBox
{
public:
	virtual property bool NoBox { bool get(); void set(bool value); }
internal:
	FarListBox(FarDialog^ dialog, int left, int top, int right, int bottom, String^ text);
	virtual void Setup(FarDialogItem& item) override;
};

ref class FarDialog : public IDialog
{
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
	virtual property Place Rect { Place get() { return _rect; } }
	virtual property String^ HelpTopic;
	virtual property Object^ Data;
public:
	virtual bool Show();
	virtual IBox^ AddBox(int left, int top, int right, int bottom, String^ text);
	virtual IButton^ AddButton(int left, int top, String^ text);
	virtual ICheckBox^ AddCheckBox(int left, int top, String^ text);
	virtual IComboBox^ AddComboBox(int left, int top, int right, String^ text);
	virtual IEdit^ AddEdit(int left, int top, int right, String^ text);
	virtual IEdit^ AddEditFixed(int left, int top, int right, String^ text);
	virtual IEdit^ AddEditPassword(int left, int top, int right, String^ text);
	virtual IListBox^ AddListBox(int left, int top, int right, int bottom, String^ text);
	virtual IRadioButton^ AddRadioButton(int left, int top, String^ text);
	virtual IText^ AddText(int left, int top, int right, String^ text);
	virtual IText^ AddVerticalText(int left, int top, int bottom, String^ text);
	virtual IUserControl^ AddUserControl(int left, int top, int right, int bottom);
	virtual void Close();
public: DEF_EVENT_ARGS(Closing, _Closing, ClosingEventArgs);
public: DEF_EVENT_ARGS(Idled, _Idled, AnyEventArgs);
public: DEF_EVENT_ARGS(Initialized, _Initialized, InitializedEventArgs);
public: DEF_EVENT_ARGS(KeyPressed, _KeyPressed, KeyPressedEventArgs);
public: DEF_EVENT_ARGS(MouseClicked, _MouseClicked, MouseClickedEventArgs);
internal:
	FarDialog(int left, int top, int right, int bottom);
	static int AsProcessDialogEvent(int id, void* param);
	LONG_PTR DialogProc(int msg, int param1, LONG_PTR param2);
internal:
	static List<FarDialog^> _dialogs;
	HANDLE _hDlg;
	IList<FarControl^>^ _items;
private:
	void AddItem(FarControl^ item);
private:
	Place _rect;
	int _flags;
	FarControl^ _default;
	FarControl^ _focused;
	FarControl^ _selected;
};
}
