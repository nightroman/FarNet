/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class MenuItem : IMenuItem
{
public:
	DEF_EVENT(OnClick, _OnClick);
public:
	virtual property bool Checked;
	virtual property bool Disabled;
	virtual property bool Grayed;
	virtual property bool Hidden;
	virtual property bool IsSeparator;
	virtual property Object^ Data;
	virtual property String^ Text;
	virtual property ToolOptions From;
public:
	virtual String^ ToString() override
	{
		return Text;
	}
};
}
