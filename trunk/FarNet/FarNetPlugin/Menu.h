#pragma once

namespace FarManagerImpl
{;
public ref class Menu : public IMenu
{
public:
	virtual event EventHandler^ OnClick;
	virtual property bool AutoAssignHotkeys { bool get(); void set(bool value); }
	virtual property bool ReverseAutoAssign { bool get(); void set(bool value); }
	virtual property bool ShowAmpersands { bool get(); void set(bool value); }
	virtual property bool WrapCursor { bool get(); void set(bool value); }
	virtual property IList<int>^ BreakKeys { IList<int>^ get(); }
	virtual property IMenuItems^ Items { IMenuItems^ get(); }
	virtual property int BreakCode { int get(); void set(int value); }
	virtual property int MaxHeight { int get(); void set(int value); }
	virtual property int Selected { int get(); void set(int value); }
	virtual property int X { int get(); void set(int value); }
	virtual property int Y { int get(); void set(int value); }
	virtual property Object^ SelectedData { Object^ get(); }
	virtual property String^ Bottom { String^ get(); void set(String^ value); }
	virtual property String^ Title { String^ get(); void set(String^ value); }
public:
	~Menu();
	!Menu();
	virtual bool Show();
	virtual bool Show(int index);
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
	FarMenuItem* _createdItems;
	int* _createdBreaks;
	MenuItemCollection^ _items;
	IList<int>^ _breakKeys;
	int _x;
	int _y;
	int _maxHeight;
	String^ _title;
	String^ _bottom;
	int _selected;
	int _breakCode;
	bool _showAmpersands;
	bool _wrapCursor;
	bool _autoAssignHotkeys;
	bool _reverseAutoAssign;
};
}
