
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

#pragma once

namespace FarNet
{;
ref class Far;
ref class FarDialog;

ref class FarControl abstract : public IControl
{
public:
	virtual property bool Disabled { bool get(); void set(bool value); }
	virtual property bool Hidden { bool get(); void set(bool value); }
	virtual property int Id { int get() { return _id; } }
	virtual property Place Rect { Place get(); void set(Place value); }
	virtual property String^ Name;
	virtual property String^ Text { String^ get(); void set(String^ value); }
	virtual property Object^ Data { Object^ get(); void set(Object^ value); }
public: DEF_EVENT_ARGS(Coloring, _Coloring, ColoringEventArgs);
public: DEF_EVENT_ARGS(Drawing, _Drawing, DrawingEventArgs);
public: DEF_EVENT_ARGS(Drawn, _Drawn, DrawnEventArgs);
public: DEF_EVENT_ARGS(DropDownClosed, _DropDownClosed, DropDownClosedEventArgs);
public: DEF_EVENT_ARGS(DropDownOpening, _DropDownOpening, DropDownOpeningEventArgs);
public: DEF_EVENT_ARGS(GotFocus, _GotFocus, AnyEventArgs);
public: DEF_EVENT_ARGS(KeyPressed, _KeyPressed, KeyPressedEventArgs);
public: DEF_EVENT_ARGS(LosingFocus, _LosingFocus, LosingFocusEventArgs);
public: DEF_EVENT_ARGS(MouseClicked, _MouseClicked, MouseClickedEventArgs);
public:
	virtual String^ ToString() override;
protected:
	FarControl(FarDialog^ dialog, int index);
	void AssertOpened();
internal:
	FarControl(FarDialog^ dialog, int left, int top, int right, int bottom, String^ text);
	bool GetFlag(int flag);
	void SetFlag(int flag, bool value);
	int GetSelected();
	void SetSelected(int value);
	bool GetChanged();
	void SetChanged(bool value);
	virtual void Init(FarDialogItem& item, FARDIALOGITEMTYPES type);
	virtual void Starting(FarDialogItem& item) = 0;
	virtual void Started() {}
	virtual void Stop(bool ok);
	virtual void Free();
internal:
	int _id;
	Place _rect;
	FarDialogItem* _item;
	FarDialog^ const _dialog;
	FARDIALOGITEMFLAGS _flags;
	int _selected;
	String^ _text;
	Object^ _data;
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
	virtual property String^ Text { String^ get() override; void set(String^ value) override; }
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
	virtual property bool ExpandEnvironmentVariables { bool get(); void set(bool value); }
	virtual property bool Fixed { bool get(); }
	virtual property bool IsPath { bool get(); void set(bool value); }
	virtual property bool IsPassword { bool get(); }
	virtual property bool IsTouched { bool get(); void set(bool value); }
	virtual property bool ManualAddHistory { bool get(); void set(bool value); }
	virtual property bool NoAutoComplete { bool get(); void set(bool value); }
	virtual property bool NoFocus { bool get(); void set(bool value); }
	virtual property bool ReadOnly { bool get(); void set(bool value); }
	virtual property bool SelectOnEntry { bool get(); void set(bool value); }
	virtual property bool UseLastHistory { bool get(); void set(bool value); }
	virtual property ILine^ Line { ILine^ get(); }
	virtual property String^ History { String^ get(); void set(String^ value); }
	virtual property String^ Mask { String^ get(); void set(String^ value); }
public: DEF_EVENT_ARGS(TextChanged, _TextChanged, TextChangedEventArgs);
internal:
	FarEdit(FarDialog^ dialog, int index, FARDIALOGITEMTYPES type);
	FarEdit(FarDialog^ dialog, int left, int top, int right, String^ text, FARDIALOGITEMTYPES type);
	virtual void Starting(FarDialogItem& item) override;
	virtual void Stop(bool ok) override;
	virtual void Free() override;
private:
	FARDIALOGITEMTYPES _type;
	String^ _history;
	String^ _mask;
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
	virtual property IList<FarItem^>^ Items { IList<FarItem^>^ get(); }
	virtual property int Selected { int get(); void set(int value); }
	virtual void Stop(bool ok) override;
public:
	virtual FarItem^ Add(String^ text);
	virtual void AttachItems();
	virtual void DetachItems();
protected:
	FarBaseList(FarDialog^ dialog, int index);
	FarBaseList(FarDialog^ dialog, int left, int top, int right, int bottom, String^ text);
internal:
	virtual void Init(FarDialogItem& item, FARDIALOGITEMTYPES type) override;
	virtual void Free() override;
	void FreeItems();
	static void InitFarListItem(FarListItem& i2, FarItem^ i1);
	static void InitFarListItemShort(FarListItem& i2, FarItem^ i1);
internal:
	IList<FarItem^>^ _Items;
	List<int>^ _ii;
private:
	FarList* _pFarList;
};

ref class FarComboBox : public FarBaseList, public IComboBox
{
public:
	virtual property bool DropDownList { bool get(); void set(bool value); }
	virtual property bool ExpandEnvironmentVariables { bool get(); void set(bool value); }
	virtual property bool IsTouched { bool get(); void set(bool value); }
	virtual property bool ReadOnly { bool get(); void set(bool value); }
	virtual property bool SelectOnEntry { bool get(); void set(bool value); }
	virtual property ILine^ Line { ILine^ get(); }
	virtual void Stop(bool ok) override;
public: DEF_EVENT_ARGS(TextChanged, _TextChanged, TextChangedEventArgs);
internal:
	FarComboBox(FarDialog^ dialog, int index);
	FarComboBox(FarDialog^ dialog, int left, int top, int right, String^ text);
	virtual void Starting(FarDialogItem& item) override;
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

}
