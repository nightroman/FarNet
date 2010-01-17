/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class CommandPluginInfo;
ref class EditorPluginInfo;
ref class FilerPluginInfo;
ref class ToolPluginInfo;

/// <summary> Plugin manager loading plugins </summary>
ref class Plugin0
{
public:
	static void AddPlugin(BasePlugin^ plugin);
	static void UnloadPlugin(BasePlugin^ plugin);
	static void LoadPlugins();
	static void UnloadPlugins();
	static property IList<String^>^ AssemblyNames { IList<String^>^ get() { return _names->Keys; } }
	static property IList<BasePlugin^>^ Plugins { IList<BasePlugin^>^ get() { return %_plugins; } }
private:
	static int AddPlugin(Type^ type, List<CommandPluginInfo^>^ commands, List<EditorPluginInfo^>^ editors, List<FilerPluginInfo^>^ filers, List<ToolPluginInfo^>^ tools);
	static void LoadFromAssembly(String^ assemblyPath, array<String^>^ classes);
	static void LoadFromConfig(String^ file, String^ dir);
	static void LoadFromDirectory(String^ dir);
	static void ReadCache();
	static void WriteCache(String^ assemblyPath, List<CommandPluginInfo^>^ commands, List<EditorPluginInfo^>^ editors, List<FilerPluginInfo^>^ filers, List<ToolPluginInfo^>^ tools);
private:
	// Static
	Plugin0() {}
	// Registered plugins
	static List<BasePlugin^> _plugins;
	static SortedList<String^, Object^>^ _cache = gcnew SortedList<String^, Object^>(StringComparer::OrdinalIgnoreCase);
	static SortedList<String^, Object^>^ _names = gcnew SortedList<String^, Object^>(StringComparer::OrdinalIgnoreCase);
};
}
