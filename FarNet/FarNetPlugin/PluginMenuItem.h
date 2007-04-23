/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#pragma once

namespace FarManagerImpl
{;
public ref class PluginMenuItem : public IPluginMenuItem
{
public:
	DEF_EVENT_ARGS(OnOpen, _OnOpen, OpenPluginMenuItemEventArgs);
public:
	virtual property String^ Name { String^ get(); void set(String^ value); }
private:
	String^ _name;
};
}
