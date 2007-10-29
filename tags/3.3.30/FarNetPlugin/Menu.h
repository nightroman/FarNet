/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#pragma once

namespace FarManagerImpl
{;
public ref class MenuItem : IMenuItem
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

public ref class MenuItemCollection : public List<IMenuItem^>, IMenuItems
{
public:
	virtual IMenuItem^ Add(String^ text);
	virtual IMenuItem^ Add(String^ text, EventHandler^ onClick);
	virtual IMenuItem^ Add(String^ text, bool isChecked, bool isSeparator);
private:
	// private: it was private originally, perhaps it used to make problems, too
	virtual IMenuItem^ Add(String^ text, bool isChecked) sealed = IMenuItems::Add;
};

public ref class AnyMenu abstract : public IAnyMenu
{
public:
	virtual property bool AutoAssignHotkeys;
	virtual property bool FilterRestore;
	virtual property bool SelectLast;
	virtual property bool ShowAmpersands;
	virtual property bool WrapCursor;
	virtual property IList<int>^ BreakKeys { IList<int>^ get(); }
	virtual property IMenuItems^ Items { IMenuItems^ get(); }
	virtual property int BreakCode { int get(); }
	virtual property int FilterKey;
	virtual property int MaxHeight;
	virtual property int Selected { int get(); void set(int value); }
	virtual property int X { int get(); void set(int value); }
	virtual property int Y { int get(); void set(int value); }
	virtual property Object^ SelectedData { Object^ get(); }
	virtual property Object^ Sender;
	virtual property String^ Bottom;
	virtual property String^ Filter;
	virtual property String^ FilterHistory;
	virtual property String^ HelpTopic;
	virtual property String^ Title;
public:
	virtual bool Show() = 0;
protected:
	AnyMenu();
	Regex^ CreateFilter();
	String^ AnyMenu::InfoLine();
	static Regex^ CreateFilter(String^ filter, bool* ok);
	static String^ InputFilter(String^ filter, String^ history);
internal:
	MenuItemCollection^ _items;
	List<int>^ _ii;
	int _x;
	int _y;
	int _selected;
	List<int> _breakKeys;
	int _breakCode;
};

public ref class Menu : public AnyMenu, public IMenu
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
	void ShowMenu(const FarMenuItem* items, const int* breaks);
private:
	// locked items
	FarMenuItem* _createdItems;
	// locked breaks
	int* _createdBreaks;
};

public ref class ListMenu sealed : public AnyMenu, public IListMenu
{
public:
	DEF_EVENT(Showing, _Showing);
public:
	virtual property bool AutoSelect;
	virtual property bool NoShadow;
	virtual property FilterOptions Alternative;
	virtual property FilterOptions Incremental;
	virtual property IListBox^ ListBox;
	virtual property int ScreenMargin;
	virtual property String^ IncrementalFilter;
public:
	virtual bool Show() override;
private:
	void OnKeyPressed(Object^ sender, KeyPressedEventArgs^ e);
	Regex^ CreateIncrementalFilter();
	void MakeFilter();
	void GetInfo(String^& head, String^& foot);
private:
	int _restart;
	bool _toFilter1;
	bool _toFilter2;
	int _incrementalLength1;
};

}
