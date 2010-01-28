/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class ModuleCommandInfo;
ref class ModuleEditorInfo;
ref class ModuleFilerInfo;
ref class ModuleToolInfo;

// Plugin manager loading plugins.
ref class Module0
{
public:
	static property IList<BaseModule^>^ Modules { IList<BaseModule^>^ get() { return %_plugins; } }
	static property IList<String^>^ AssemblyNames { IList<String^>^ get() { return _names->Keys; } }
public:
	static bool CanExit();
	static void AddPlugin(BaseModule^ plugin);
	static void LoadPlugins();
	static void UnloadPlugin(BaseModule^ plugin);
	static void UnloadPlugins();
private:
	static int AddPlugin(Type^ type, List<ModuleCommandInfo^>^ commands, List<ModuleEditorInfo^>^ editors, List<ModuleFilerInfo^>^ filers, List<ModuleToolInfo^>^ tools);
	static void LoadFromAssembly(String^ assemblyPath, array<String^>^ classes);
	static void LoadFromConfig(String^ file, String^ dir);
	static void LoadFromDirectory(String^ dir);
	static void ReadCache();
	static void WriteCache(String^ assemblyPath, List<ModuleCommandInfo^>^ commands, List<ModuleEditorInfo^>^ editors, List<ModuleFilerInfo^>^ filers, List<ModuleToolInfo^>^ tools);
private:
	// Static
	Module0() {}
	// Registered plugins
	static List<BaseModule^> _plugins;
	static SortedList<String^, Object^>^ _cache = gcnew SortedList<String^, Object^>(StringComparer::OrdinalIgnoreCase);
	static SortedList<String^, Object^>^ _names = gcnew SortedList<String^, Object^>(StringComparer::OrdinalIgnoreCase);
};
}
