/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2009 FAR.NET Team
*/

#pragma once
#include "MenuItem.h"

namespace FarNet
{;
ref class AnyMenu abstract : public IAnyMenu
{
public:
	virtual property bool AutoAssignHotkeys;
	virtual property bool SelectLast;
	virtual property bool ShowAmpersands;
	virtual property bool WrapCursor;
	virtual property IList<int>^ BreakKeys { IList<int>^ get(); }
	virtual property IList<IMenuItem^>^ Items { IList<IMenuItem^>^ get(); }
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
	virtual IMenuItem^ Add(String^ text);
	virtual IMenuItem^ Add(String^ text, EventHandler^ handler);
protected:
	AnyMenu();
internal:
	List<IMenuItem^>^ _items;
	int _x;
	int _y;
	int _selected;
	List<int> _keys;
	int _breakKey;
};
}
