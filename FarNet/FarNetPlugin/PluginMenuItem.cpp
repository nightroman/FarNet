/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#include "StdAfx.h"
#include "PluginMenuItem.h"

namespace FarManagerImpl
{;
String^ PluginMenuItem::Name::get()
{
	return _name;
}

void PluginMenuItem::Name::set(String^ value)
{
	_name = value;
}

}
