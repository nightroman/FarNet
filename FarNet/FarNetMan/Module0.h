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

// Module manager
ref class Module0
{
public:
	static property IList<BaseModule^>^ Modules { IList<BaseModule^>^ get() { return %_Modules; } }
	static property IList<String^>^ AssemblyNames { IList<String^>^ get() { return _Names->Keys; } }
public:
	static bool CanExit();
	static void AddModule(BaseModule^ module);
	static void LoadModules();
	static void UnloadModule(BaseModule^ module);
	static void UnloadModules();
private:
	static int AddModule(Type^ type, List<ModuleCommandInfo^>^ commands, List<ModuleEditorInfo^>^ editors, List<ModuleFilerInfo^>^ filers, List<ModuleToolInfo^>^ tools);
	static void LoadFromAssembly(String^ assemblyPath, array<String^>^ classes);
	static void LoadFromConfig(String^ file, String^ dir);
	static void LoadFromDirectory(String^ dir);
	static void ReadCache();
	static void WriteCache(String^ assemblyPath, List<ModuleCommandInfo^>^ commands, List<ModuleEditorInfo^>^ editors, List<ModuleFilerInfo^>^ filers, List<ModuleToolInfo^>^ tools);
private:
	// Static
	Module0() {}
	// Registered modules
	static List<BaseModule^> _Modules;
	static SortedList<String^, Object^>^ _Cache = gcnew SortedList<String^, Object^>(StringComparer::OrdinalIgnoreCase);
	static SortedList<String^, Object^>^ _Names = gcnew SortedList<String^, Object^>(StringComparer::OrdinalIgnoreCase);
};
}
