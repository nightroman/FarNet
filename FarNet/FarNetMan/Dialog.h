
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

#pragma once

namespace FarNet
{
;
ref class Far;
ref class FarControl;

ref class FarDialog : IDialog
{
public: DEF_EVENT2(Timer, _Timer);
public: DEF_EVENT2(GotFocus, _GotFocus);
public: DEF_EVENT2(LosingFocus, _LosingFocus);
public: DEF_EVENT_ARGS2(Closed, _Closed, AnyEventArgs);
public: DEF_EVENT_ARGS2(Closing, _Closing, ClosingEventArgs);
public: DEF_EVENT_ARGS2(ConsoleSizeChanged, _ConsoleSizeChanged, SizeEventArgs);
public: DEF_EVENT_ARGS2(Initialized, _Initialized, InitializedEventArgs);
public: DEF_EVENT_ARGS2(KeyPressed, _KeyPressed, KeyPressedEventArgs);
public: DEF_EVENT_ARGS2(MouseClicked, _MouseClicked, MouseClickedEventArgs);
public:
	virtual property bool IsSmall { bool get() override; void set(bool value) override; }
	virtual property bool IsWarning { bool get() override; void set(bool value) override; }
	virtual property bool KeepWindowTitle { bool get() override; void set(bool value) override; }
	virtual property bool NoPanel { bool get() override; void set(bool value) override; }
	virtual property bool NoShadow{ bool get() override; void set(bool value) override; }
	virtual property bool StayOnTop { bool get() override; void set(bool value) override; }
	virtual property bool NoSmartCoordinates { bool get() override { return _NoSmartCoordinates; } void set(bool value) override { _NoSmartCoordinates = value; } }
	virtual property Guid TypeId { Guid get() override; void set(Guid value) override; }
	virtual property IButton^ Cancel { IButton^ get() override { return _Cancel; } void set(IButton^ value) override { _Cancel = value; } }
	virtual property IControl^ default[int]{ IControl ^ get(int id) override; }
	virtual property IControl^ Default { IControl^ get() override; void set(IControl^ value) override; }
	virtual property IControl^ Focused { IControl^ get() override; void set(IControl^ value) override; }
	virtual property IControl^ Selected { IControl^ get() override; }
	virtual property IEnumerable<IControl^>^ Controls { IEnumerable<IControl^>^ get() override; }
	virtual property Place Rect { Place get() override; void set(Place value) override; }
	virtual property String^ HelpTopic { String^ get() override { return _HelpTopic; } void set(String^ value) override { _HelpTopic = value; } }
	virtual property int TimerInterval { int get() override { return _TimerInterval; } void set(int value) override { _TimerInterval = value; } }
	virtual property IntPtr Id { IntPtr get() override; }
public:
	virtual IBox^ AddBox(int left, int top, int right, int bottom, String^ text) override;
	virtual IButton^ AddButton(int left, int top, String^ text) override;
	virtual ICheckBox^ AddCheckBox(int left, int top, String^ text) override;
	virtual IComboBox^ AddComboBox(int left, int top, int right, String^ text) override;
	virtual IEdit^ AddEdit(int left, int top, int right, String^ text) override;
	virtual IEdit^ AddEditFixed(int left, int top, int right, String^ text) override;
	virtual IEdit^ AddEditPassword(int left, int top, int right, String^ text) override;
	virtual IListBox^ AddListBox(int left, int top, int right, int bottom, String^ title) override;
	virtual IRadioButton^ AddRadioButton(int left, int top, String^ text) override;
	virtual IText^ AddText(int left, int top, int right, String^ text) override;
	virtual IText^ AddVerticalText(int left, int top, int bottom, String^ text) override;
	virtual IUserControl^ AddUserControl(int left, int top, int right, int bottom) override;
	virtual bool Show() override;
	virtual void Close(int id) override;
	virtual void DisableRedraw() override;
	virtual void EnableRedraw() override;
	virtual void Move(Point point, bool absolute) override;
	virtual void Open() override;
	virtual void Redraw() override;
	virtual void Resize(Point size) override;
	virtual void SetFocus(int id) override;
internal:
	FarDialog(HANDLE hDlg);
	FarDialog(int left, int top, int right, int bottom);
	INT_PTR DialogProc(intptr_t msg, intptr_t param1, void* param2);
	static FarDialog^ GetDialog();
internal:
	static List<FarDialog^> _dialogs;
	HANDLE _hDlg;
	List<FarControl^>^ _items;
private:
	void AddItem(FarControl^ item);
	void OnTimer(Object^ state);
	void OnTimerJob();
	bool Stop(int selected);
	void Free();
private:
	int _flags;
	bool _NoModal;
	Place _rect;
	FarControl^ _default;
	FarControl^ _focused;
	FarControl^ _selected;
	Guid _typeId;
	bool _NoSmartCoordinates;
	IButton^ _Cancel;
	String^ _HelpTopic;
	FarDialogItem* _farItems;
	wchar_t* _helpTopic;
	System::Threading::Timer^ _timerInstance;
	int _TimerInterval;
};

}
