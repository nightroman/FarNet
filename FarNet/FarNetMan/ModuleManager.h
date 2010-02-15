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
	void Unload();
	BaseModuleEntry^ CreateEntry(Type^ type);
	property String^ AssemblyPath { String^ get(); }
	property Assembly^ AssemblyInstance { Assembly^ get(); }
internal:
	bool HasHost() { return _ModuleHostInstance || _ModuleHostClassName || _ModuleHostClassType; }
	ModuleHost^ GetLoadedModuleHost() { return _ModuleHostInstance; }
	String^ GetModuleHostClassName();
	void SetModuleHost(String^ moduleHostClassName);
	void SetModuleHost(Type^ moduleHostClassType);
	void Invoking();
internal:
	static Object^ GetFarNetValue(String^ keyPath, String^ valueName, Object^ defaultValue);
	static void SetFarNetValue(String^ keyPath, String^ valueName, Object^ value);
private:
	void Connect();
private:
	// assembly
	String^ _AssemblyPath;
	Assembly^ _AssemblyInstance;
	CultureInfo^ _CurrentUICulture;
	ResourceManager^ _ResourceManager;
	// host
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
	ModuleManager^ _ModuleManager;
	String^ _ClassName;
	Type^ _ClassType;
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
