/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class ModuleManager : public IModuleManager
{
public: // IModuleManager
	virtual property CultureInfo^ CurrentUICulture { CultureInfo^ get(); void set(CultureInfo^ value); }
	virtual RegistryKey^ OpenSubKey(String^ name, bool writable);
	virtual String^ GetString(String^ name);
	virtual void Unregister();
	virtual IModuleCommand^ RegisterModuleCommand(Guid id, ModuleCommandAttribute^ attribute, EventHandler<ModuleCommandEventArgs^>^ handler);
	virtual IModuleFiler^ RegisterModuleFiler(Guid id, ModuleFilerAttribute^ attribute, EventHandler<ModuleFilerEventArgs^>^ handler);
	virtual IModuleTool^ RegisterModuleTool(Guid id, ModuleToolAttribute^ attribute, EventHandler<ModuleToolEventArgs^>^ handler);
internal:
	ModuleManager(String^ assemblyPath);
	BaseModuleItem^ CreateEntry(Type^ type);
	bool HasHost() { return _ModuleHostInstance || _ModuleHostClassName || _ModuleHostClassType; }
	ModuleHost^ GetLoadedModuleHost() { return _ModuleHostInstance; }
	property Assembly^ AssemblyInstance { Assembly^ get(); }
	property bool CachedResources;
	property String^ AssemblyPath { String^ get(); }
	property String^ ModuleName { String^ get(); }
	String^ GetModuleHostClassName();
	void Invoking();
	void SetModuleHost(String^ moduleHostClassName);
	void SetModuleHost(Type^ moduleHostClassType);
	bool LoadLoadableModuleHost();
internal:
	static Object^ LoadPluginValue(String^ pluginName, String^ valueName, Object^ defaultValue);
	static Object^ LoadFarNetValue(String^ keyPath, String^ valueName, Object^ defaultValue);
	static void SaveFarNetValue(String^ keyPath, String^ valueName, Object^ value);
	static void SavePluginValue(String^ pluginName, String^ valueName, Object^ newValue);
private:
	void Connect();
private: // Assembly
	String^ _AssemblyPath;
	Assembly^ _AssemblyInstance;
	CultureInfo^ _CurrentUICulture;
	ResourceManager^ _ResourceManager;
private: // Module host
	ModuleHost^ _ModuleHostInstance;
	String^ _ModuleHostClassName;
	Type^ _ModuleHostClassType;
};

}
