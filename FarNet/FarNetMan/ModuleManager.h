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
	virtual String^ ToString() override;
	property String^ AssemblyPath { String^ get(); }
	property String^ ClassName { String^ get(); }
	property String^ Key { String^ get(); }
	property String^ Name { String^ get() { return _Attribute->Name; } }
protected:
	BaseModuleToolInfo(ModuleManager^ manager, BaseModuleToolAttribute^ attribute);
	BaseModuleToolInfo(ModuleManager^ manager, Type^ classType, Type^ attributeType);
	BaseModuleToolInfo(ModuleManager^ manager, String^ className, BaseModuleToolAttribute^ attribute);
	BaseModuleToolAttribute^ GetAttribute() { return _Attribute; }
private:
	void Init();
private:
	// Any tool has the module manager. Handlers may or may not have it.
	ModuleManager^ const _ModuleManager;
	// Class name from the cache. Null for handlers and after getting the type.
	String^ _ClassName;
	// Type coming from the assembly reflection. Null for handlers.
	Type^ _ClassType;
	// Attribute. Not null.
	BaseModuleToolAttribute^ _Attribute;
};

ref class ModuleToolInfo sealed : BaseModuleToolInfo
{
public:
	ModuleToolInfo(ModuleManager^ manager, EventHandler<ModuleToolEventArgs^>^ handler, ModuleToolAttribute^ attribute);
	ModuleToolInfo(ModuleManager^ manager, String^ className, ModuleToolAttribute^ attribute);
	ModuleToolInfo(ModuleManager^ manager, Type^ classType);
	virtual String^ ToString() override;
	String^ Alias(ModuleToolOptions option);
	void Alias(ModuleToolOptions option, String^ value);
	void Invoke(Object^ sender, ModuleToolEventArgs^ e);
	bool HasHandler(EventHandler<ModuleToolEventArgs^>^ handler) { return _Handler == handler; }
public:
	property ModuleToolAttribute^ Attribute { ModuleToolAttribute^ get() { return (ModuleToolAttribute^)GetAttribute(); } }
private:
	EventHandler<ModuleToolEventArgs^>^ _Handler;
	String^ _AliasConfig;
	String^ _AliasDisk;
	String^ _AliasDialog;
	String^ _AliasEditor;
	String^ _AliasPanels;
	String^ _AliasViewer;
};

ref class ModuleCommandInfo sealed : BaseModuleToolInfo
{
public:
	ModuleCommandInfo(ModuleManager^ manager, EventHandler<ModuleCommandEventArgs^>^ handler, ModuleCommandAttribute^ attribute);
	ModuleCommandInfo(ModuleManager^ manager, String^ className, ModuleCommandAttribute^ attribute);
	ModuleCommandInfo(ModuleManager^ manager, Type^ classType);
	virtual String^ ToString() override;
	void Invoke(Object^ sender, ModuleCommandEventArgs^ e);
	bool HasHandler(EventHandler<ModuleCommandEventArgs^>^ handler) { return _Handler == handler; }
public:
	property ModuleCommandAttribute^ Attribute { ModuleCommandAttribute^ get() { return (ModuleCommandAttribute^)GetAttribute(); } }
	property String^ DefaultPrefix { String^ get() { return _DefaultPrefix; } }
	void SetPrefix(String^ value);
private:
	void Init();
private:
	EventHandler<ModuleCommandEventArgs^>^ _Handler;
	String^ _DefaultPrefix;
};

ref class ModuleFilerInfo sealed : BaseModuleToolInfo
{
public:
	ModuleFilerInfo(ModuleManager^ manager, EventHandler<ModuleFilerEventArgs^>^ handler, ModuleFilerAttribute^ attribute);
	ModuleFilerInfo(ModuleManager^ manager, String^ className, ModuleFilerAttribute^ attribute);
	ModuleFilerInfo(ModuleManager^ manager, Type^ classType);
	virtual String^ ToString() override;
	void Invoke(Object^ sender, ModuleFilerEventArgs^ e);
	bool HasHandler(EventHandler<ModuleFilerEventArgs^>^ handler) { return _Handler == handler; }
public:
	property ModuleFilerAttribute^ Attribute { ModuleFilerAttribute^ get() { return (ModuleFilerAttribute^)GetAttribute(); } }
	property String^ DefaultMask { String^ get() { return _DefaultMask; } }
	void SetMask(String^ value);
private:
	void Init();
private:
	EventHandler<ModuleFilerEventArgs^>^ _Handler;
	String^ _DefaultMask;
};

ref class ModuleEditorInfo sealed : BaseModuleToolInfo
{
public:
	ModuleEditorInfo(ModuleManager^ manager, EventHandler^ handler, ModuleEditorAttribute^ attribute);
	ModuleEditorInfo(ModuleManager^ manager, String^ className, ModuleEditorAttribute^ attribute);
	ModuleEditorInfo(ModuleManager^ manager, Type^ classType);
	virtual String^ ToString() override;
	void Invoke(Object^ sender, ModuleEditorEventArgs^ e);
	bool HasHandler(EventHandler^ handler) { return _Handler == handler; }
public:
	property ModuleEditorAttribute^ Attribute { ModuleEditorAttribute^ get() { return (ModuleEditorAttribute^)GetAttribute(); } }
	property String^ DefaultMask { String^ get() { return _DefaultMask; } }
	void SetMask(String^ value);
private:
	void Init();
private:
	EventHandler^ _Handler;
	String^ _DefaultMask;
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
