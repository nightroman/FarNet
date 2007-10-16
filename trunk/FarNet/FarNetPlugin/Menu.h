/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#pragma once

namespace FarManagerImpl
{;
ref class MenuItemCollection;

public ref class Menu : public IMenu
{
public:
	virtual event EventHandler^ OnClick;
	virtual property bool AutoAssignHotkeys;
	virtual property bool FilterRestore;
	virtual property bool ReverseAutoAssign;
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
	~Menu();
	!Menu();
	virtual bool Show();
	virtual void Lock();
	virtual void Unlock();
internal:
	Menu();
private:
	FarMenuItem* CreateItems();
	int Flags();
	int* CreateBreakKeys();
	void ShowMenu(const FarMenuItem* items, const int* breaks);
	static Regex^ CreateFilter(String^ filter, bool* ok);
	static String^ InputFilter(String^ filter, String^ history);
private:
	MenuItemCollection^ _items;
	List<int> _breakKeys;
	int _x;
	int _y;
	int _selected;
	int _breakCode;
	// locked items
	FarMenuItem* _createdItems;
	// locked breaks
	int* _createdBreaks;
	// filtered indexes
	List<int>^ _ii;
};
}
