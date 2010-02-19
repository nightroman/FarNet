/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class ModuleManager;
ref class ModuleCommandInfo;
ref class ModuleEditorInfo;
ref class ModuleFilerInfo;
ref class ModuleToolInfo;

ref class ModuleLoader
{
public:
	static property IList<String^>^ AssemblyNames { IList<String^>^ get() { return _ModuleManagers->Keys; } }
	static ModuleManager^ GetModuleManager(String^ moduleName) { return _ModuleManagers[moduleName]; }
public:
	static bool CanExit();
	static void LoadModules();
	static void UnloadModules();
	static void UnloadModuleItem(BaseModuleItem^ item);
private:
	static void AddModuleEntry(ModuleManager^ manager, Type^ type, List<ModuleCommandInfo^>^ commands, List<ModuleEditorInfo^>^ editors, List<ModuleFilerInfo^>^ filers, List<ModuleToolInfo^>^ tools);
	static void LoadFromAssembly(String^ assemblyPath, List<String^>^ classes);
	static void LoadFromManifest(String^ file, String^ dir);
	static void LoadFromDirectory(String^ dir);
	static void ReadModuleCache();
	static void WriteModuleCache(ModuleManager^ manager, List<ModuleCommandInfo^>^ commands, List<ModuleEditorInfo^>^ editors, List<ModuleFilerInfo^>^ filers, List<ModuleToolInfo^>^ tools);
private:
	// Static
	ModuleLoader() {}
	// Registered modules
	static SortedList<String^, ModuleManager^>^ _ModuleManagers = gcnew SortedList<String^, ModuleManager^>(StringComparer::OrdinalIgnoreCase);
};
}
