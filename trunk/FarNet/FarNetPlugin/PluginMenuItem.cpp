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

void PluginMenuItem::FireOnOpen(IPluginMenuItem^ sender, OpenFrom from)
{
	OnOpen(sender, gcnew OpenPluginMenuItemEventArgs(from));
}
}
