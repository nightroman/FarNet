
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class AnyMenu abstract : public IAnyMenu
{
public:
	virtual property bool AutoAssignHotkeys;
	virtual property bool SelectLast;
	virtual property bool ShowAmpersands;
	virtual property bool WrapCursor;
	virtual property IList<FarItem^>^ Items { IList<FarItem^>^ get(); }
	virtual property IList<int>^ BreakKeys { IList<int>^ get(); }
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
	virtual FarItem^ Add(String^ text);
	virtual FarItem^ Add(String^ text, EventHandler^ handler);
protected:
	AnyMenu();
internal:
	List<FarItem^>^ _items;
	int _x;
	int _y;
	int _selected;
	List<int> _keys;
	int _breakKey;
};
}
