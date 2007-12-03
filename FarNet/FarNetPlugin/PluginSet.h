/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#pragma once

namespace FarNet
{;
ref class CommandPluginInfo;
ref class FilerPluginInfo;
ref class ToolPluginInfo;

/// <summary> Plugin manager loading plugins </summary>
ref class PluginSet
{
public:
	static void AddPlugin(BasePlugin^ plugin);
	static void LoadPlugins();
	static void UnloadPlugins();
private:
	static int AddPlugin(Type^ type, List<CommandPluginInfo^>^ commands, List<FilerPluginInfo^>^ filers, List<ToolPluginInfo^>^ tools);
	static void LoadFromAssembly(String^ assemblyPath, array<String^>^ classes);
	static void LoadFromConfig(String^ file, String^ dir);
	static void LoadFromDirectory(String^ dir);
	static void ReadCache();
	static void WriteCache(String^ assemblyPath, List<CommandPluginInfo^>^ commands, List<FilerPluginInfo^>^ filers, List<ToolPluginInfo^>^ tools);
private:
	// Static
	PluginSet() {}
	// Option
	static bool _startupErrorDialog;
	// Registered plugins
	static List<BasePlugin^> _plugins;
	static StringDictionary _cache;
};
}
