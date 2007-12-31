/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2007 FAR.NET Team
*/

#pragma once

namespace FarNet
{;
ref class FarListBox;

ref class MenuItem : IMenuItem
{
public:
	DEF_EVENT(OnClick, _OnClick);
public:
	virtual property bool Checked;
	virtual property bool Disabled;
	virtual property bool IsSeparator;
	virtual property Object^ Data;
	virtual property String^ Text;
public:
	virtual String^ ToString() override
	{
		return Text;
	}
};

ref class MenuItemCollection : public List<IMenuItem^>, IMenuItems
{
public:
	virtual IMenuItem^ Add(String^ text);
	virtual IMenuItem^ Add(String^ text, EventHandler^ onClick);
	virtual IMenuItem^ Add(String^ text, bool isChecked, bool isSeparator);
private:
	// private: it was private originally, perhaps it used to make problems, too
	virtual IMenuItem^ Add(String^ text, bool isChecked) sealed = IMenuItems::Add;
};

ref class AnyMenu abstract : public IAnyMenu
{
public:
	virtual property bool AutoAssignHotkeys;
	virtual property bool SelectLast;
	virtual property bool ShowAmpersands;
	virtual property bool WrapCursor;
	virtual property IList<int>^ BreakKeys { IList<int>^ get(); }
	virtual property IMenuItems^ Items { IMenuItems^ get(); }
	virtual property int BreakCode { int get(); }
	virtual property int BreakKey { int get(); }
	virtual property int MaxHeight;
	virtual property int Selected { int get(); void set(int value); }
	virtual property int X { int get(); void set(int value); }
	virtual property int Y { int get(); void set(int value); }
	virtual property Object^ SelectedData { Object^ get(); }
	virtual property Object^ Sender;
	virtual property String^ Bottom;
	virtual property String^ HelpTopic;
	virtual property String^ Title;
public:
	virtual bool Show() = 0;
protected:
	AnyMenu();
internal:
	MenuItemCollection^ _items;
	int _x;
	int _y;
	int _selected;
	List<int> _keys;
	int _breakKey;
};

ref class Menu : public AnyMenu, public IMenu
{
public:
	virtual property bool ReverseAutoAssign;
public:
	~Menu();
	!Menu();
	virtual bool Show() override;
	virtual void Lock();
	virtual void Unlock();
internal:
	Menu();
private:
	FarMenuItem* CreateItems();
	int Flags();
	int* CreateBreakKeys();
	void ShowMenu(const FarMenuItem* items, const int* breaks, const char* title, const char* bottom, const char* help);
private:
	FarMenuItem* _createdItems;
	int* _createdBreaks;
	char* _help;
	char* _title;
	char* _bottom;
};

ref class ListMenu : public AnyMenu, public IListMenu
{
public:
	virtual property bool AutoSelect;
	virtual property bool FilterRestore;
	virtual property bool NoShadow;
	virtual property bool UsualMargins;
	virtual property int FilterKey { int get() { return _FilterKey; } void set(int value) { _FilterKey = value; } }
	virtual property int ScreenMargin;
	virtual property PatternOptions FilterOptions;
	virtual property PatternOptions IncrementalOptions { PatternOptions get(); void set(PatternOptions value); }
	virtual property String^ Filter { String^ get(); void set(String^ value); }
	virtual property String^ FilterHistory;
	virtual property String^ Incremental { String^ get(); void set(String^ value); }
public:
	virtual bool Show() override;
	virtual void AddKey(int key);
	virtual void AddKey(int key, EventHandler<MenuEventArgs^>^ handler);
internal:
	ListMenu();
private:
	String^ InfoLine();
	void GetInfo(String^& head, String^& foot);
	void MakeFilter1();
	void MakeFilters();
	void OnKeyPressed(Object^ sender, KeyPressedEventArgs^ e);
private:
	FarListBox^ _box;
	List<EventHandler<MenuEventArgs^>^> _handlers;
	String^ _filter1_;
	int _FilterKey;
	// Original user defined filter
	String^ _Incremental_;
	PatternOptions _IncrementalOptions;
	// Currently used filter
	String^ _filter2;
	// To update permanent filter
	bool _toFilter1;
	// To update incremental filter
	bool _toFilter2;
	// Filtered
	List<int>^ _ii;
	Regex^ _re1;
	Regex^ _re2;
};

}
