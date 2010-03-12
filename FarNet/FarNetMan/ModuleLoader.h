/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class ModuleManager;
ref class ProxyAction;
ref class ProxyCommand;
ref class ProxyEditor;
ref class ProxyFiler;
ref class ProxyTool;

ref class ModuleLoader
{
public:
	static property IList<String^>^ AssemblyNames { IList<String^>^ get() { return _Managers->Keys; } }
	static property Dictionary<Guid, ProxyAction^>^ Actions { Dictionary<Guid, ProxyAction^>^ get() { return %_Actions; } }
	static ModuleManager^ GetModuleManager(String^ moduleName) { return _Managers[moduleName]; }
	static array<ProxyTool^>^ GetTools(ModuleToolOptions option);
	static List<IModuleManager^>^ GetModuleManagers();
	static List<IModuleTool^>^ GetTools();
public:
	static bool CanExit();
	static void LoadModules();
	static void RemoveModuleManager(ModuleManager^ manager);
	static void UnloadModules();
private:
	static int LoadClass(ModuleManager^ manager, Type^ type);
	static void LoadFromAssembly(String^ assemblyPath, List<String^>^ classes);
	static void LoadFromManifest(String^ file, String^ dir);
	static void LoadFromDirectory(String^ dir);
	static void ReadModuleCache();
	static void WriteModuleCache(ModuleManager^ manager);
private:
	// Static
	ModuleLoader() {}
	// Registered modules
	static SortedList<String^, ModuleManager^>^ _Managers = gcnew SortedList<String^, ModuleManager^>(StringComparer::OrdinalIgnoreCase);
	// Registered actions
	static Dictionary<Guid, ProxyAction^> _Actions;
};
}
