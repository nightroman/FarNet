/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#pragma once

namespace FarManagerImpl
{;
/// <summary>
/// Plugin manager which loads plugins
/// </summary>
ref class PluginSet
{
	static List<IPlugin^> _plugins;
public:
	static void LoadPlugins();
	static void UnloadPlugins();
private:
	static void AddPlugin(Type^ type);
	static void LoadAllFrom(String^ dir);
	static void LoadConfig(StreamReader^ text, String^ dir);
	static void LoadPlugin(String^ dir);
private:
	PluginSet() {}
	static bool _startupErrorDialog;
};
}
