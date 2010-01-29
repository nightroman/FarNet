/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class Far;
ref class FarControl;

ref class FarDialog : IDialog
{
public: DEF_EVENT(Idled, _Idled);
public: DEF_EVENT_ARGS(Closing, _Closing, ClosingEventArgs);
public: DEF_EVENT_ARGS(ConsoleSizeChanged, _ConsoleSizeChanged, SizeEventArgs);
public: DEF_EVENT_ARGS(Initialized, _Initialized, InitializedEventArgs);
public: DEF_EVENT_ARGS(KeyPressed, _KeyPressed, KeyPressedEventArgs);
public: DEF_EVENT_ARGS(MouseClicked, _MouseClicked, MouseClickedEventArgs);
public:
	virtual property bool IsSmall { bool get(); void set(bool value); }
	virtual property bool IsWarning { bool get(); void set(bool value); }
	virtual property bool NoPanel { bool get(); void set(bool value); }
	virtual property bool NoShadow { bool get(); void set(bool value); }
	virtual property bool NoSmartCoordinates;
	virtual property Guid TypeId { Guid get(); void set(Guid value); }
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
	virtual void DisableRedraw();
	virtual void EnableRedraw();
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
	Guid _typeId;
};

}
