/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class ModuleManager : IModuleManager
{
public: // IModuleManager
	virtual property CultureInfo^ CurrentUICulture { CultureInfo^ get(); void set(CultureInfo^ value); }
	virtual String^ GetString(String^ name);
internal:
	ModuleManager(String^ assemblyPath);
	BaseModuleEntry^ CreateEntry(Type^ type);
	bool HasHost() { return _ModuleHostInstance || _ModuleHostClassName || _ModuleHostClassType; }
	ModuleHost^ GetLoadedModuleHost() { return _ModuleHostInstance; }
	property Assembly^ AssemblyInstance { Assembly^ get(); }
	property String^ AssemblyPath { String^ get(); }
	String^ GetModuleHostClassName();
	void Invoking();
	void SetModuleHost(String^ moduleHostClassName);
	void SetModuleHost(Type^ moduleHostClassType);
	void Unload();
internal:
	static Object^ GetFarNetValue(String^ keyPath, String^ valueName, Object^ defaultValue);
	static void SetFarNetValue(String^ keyPath, String^ valueName, Object^ value);
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

ref class BaseModuleToolInfo abstract
{
public:
	void Invoking();
	BaseModuleTool^ GetInstance();
	BaseModuleToolAttribute^ InitFromAttribute(Type^ attrType);
	virtual String^ ToString() override;
	property String^ AssemblyPath { String^ get(); }
	property String^ ClassName { String^ get(); }
	property String^ Key { String^ get(); }
	property String^ Name { String^ get() { return _Name; } }
protected:
	BaseModuleToolInfo(ModuleManager^ manager, String^ name);
	BaseModuleToolInfo(ModuleManager^ manager, Type^ classType);
	BaseModuleToolInfo(ModuleManager^ manager, String^ className, String^ toolName);
private:
	// Any tool has the module manager. Handlers may or may not have it.
	ModuleManager^ const _ModuleManager;
	// Class name from the cache. Null for handlers and after getting the type.
	String^ _ClassName;
	// Type coming from the assembly reflection. Null for handlers.
	Type^ _ClassType;
	// UI name from cache, attributes (after getting type) or just set for handlers.
	String^ _Name;
};

ref class ModuleToolInfo : BaseModuleToolInfo
{
public:
	ModuleToolInfo(ModuleManager^ manager, String^ name, EventHandler<ModuleToolEventArgs^>^ handler, ModuleToolOptions options);
	ModuleToolInfo(ModuleManager^ manager, String^ className, String^ name, ModuleToolOptions options);
	ModuleToolInfo(ModuleManager^ manager, Type^ classType);
	virtual String^ ToString() override;
	String^ Alias(ModuleToolOptions option);
	void Alias(ModuleToolOptions option, String^ value);
	void Invoke(Object^ sender, ModuleToolEventArgs^ e);
	bool HasHandler(EventHandler<ModuleToolEventArgs^>^ handler) { return _Handler == handler; }
public:
	property ModuleToolOptions Options { ModuleToolOptions get() { return _Options; } }
private:
	EventHandler<ModuleToolEventArgs^>^ _Handler;
	ModuleToolOptions _Options;
	String^ _AliasConfig;
	String^ _AliasDisk;
	String^ _AliasDialog;
	String^ _AliasEditor;
	String^ _AliasPanels;
	String^ _AliasViewer;
};

ref class ModuleCommandInfo : BaseModuleToolInfo
{
public:
	ModuleCommandInfo(ModuleManager^ manager, String^ name, String^ prefix, EventHandler<ModuleCommandEventArgs^>^ handler);
	ModuleCommandInfo(ModuleManager^ manager, String^ className, String^ name, String^ prefix);
	ModuleCommandInfo(ModuleManager^ manager, Type^ classType);
	virtual String^ ToString() override;
	void Invoke(Object^ sender, ModuleCommandEventArgs^ e);
	bool HasHandler(EventHandler<ModuleCommandEventArgs^>^ handler) { return _Handler == handler; }
public:
	property String^ DefaultPrefix { String^ get() { return _DefaultPrefix; } }
	property String^ Prefix { String^ get(); void set(String^ value); }
private:
	EventHandler<ModuleCommandEventArgs^>^ _Handler;
	String^ _DefaultPrefix;
	String^ _Prefix;
};

ref class ModuleFilerInfo : BaseModuleToolInfo
{
public:
	ModuleFilerInfo(ModuleManager^ manager, String^ name, EventHandler<ModuleFilerEventArgs^>^ handler, String^ mask, bool creates);
	ModuleFilerInfo(ModuleManager^ manager, String^ className, String^ name, String^ mask, bool creates);
	ModuleFilerInfo(ModuleManager^ manager, Type^ classType);
	virtual String^ ToString() override;
	void Invoke(Object^ sender, ModuleFilerEventArgs^ e);
	bool HasHandler(EventHandler<ModuleFilerEventArgs^>^ handler) { return _Handler == handler; }
public:
	property bool Creates { bool get() { return _Creates; } }
	property String^ DefaultMask { String^ get() { return _DefaultMask; } }
	property String^ Mask { String^ get(); void set(String^ value); }
private:
private:
	bool _Creates;
	EventHandler<ModuleFilerEventArgs^>^ _Handler;
	String^ _DefaultMask;
	String^ _Mask;
};

ref class ModuleEditorInfo : BaseModuleToolInfo
{
public:
	ModuleEditorInfo(ModuleManager^ manager, String^ name, EventHandler^ handler, String^ mask);
	ModuleEditorInfo(ModuleManager^ manager, String^ className, String^ name, String^ mask);
	ModuleEditorInfo(ModuleManager^ manager, Type^ classType);
	virtual String^ ToString() override;
	void Invoke(Object^ sender, ModuleEditorEventArgs^ e);
	bool HasHandler(EventHandler^ handler) { return _Handler == handler; }
public:
	property String^ DefaultMask { String^ get() { return _DefaultMask; } }
	property String^ Mask { String^ get(); void set(String^ value); }
private:
	EventHandler^ _Handler;
	String^ _DefaultMask;
	String^ _Mask;
};

ref class ModuleToolAliasComparer : IComparer<ModuleToolInfo^>
{
public:
	ModuleToolAliasComparer(ModuleToolOptions option) : _Option(option) {}
	virtual int Compare(ModuleToolInfo^ x, ModuleToolInfo^ y);
private:
	ModuleToolOptions _Option;
};

}
