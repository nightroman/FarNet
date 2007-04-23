/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#pragma once

namespace FarManagerImpl
{;
ref class Far;
/// <summary>
/// Plugin manager which loads plugins
/// </summary>
ref class PluginManager
{
	Far^ _far;
	List<IPlugin^> _plugins;
public:
	PluginManager(Far^ plugin);
	void LoadPlugins();
	void UnloadPlugins();
private:
	void AddPlugin(Type^ type);
	void LoadAllFrom(String^ dir);
	void LoadConfig(StreamReader^ text, String^ dir);
	void LoadPlugin(String^ dir);
};
}