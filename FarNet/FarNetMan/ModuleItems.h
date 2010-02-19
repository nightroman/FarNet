/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class ModuleManager;

ref class BaseModuleToolInfo abstract
{
public:
	virtual void WriteCache(List<String^>^ data);
	void Invoking();
	BaseModuleTool^ GetInstance();
	virtual String^ ToString() override;
	property String^ AssemblyPath { String^ get(); }
	property String^ ClassName { String^ get(); }
	property String^ Key { String^ get(); }
	property String^ ToolName { String^ get() { return _Attribute->Name; } }
protected:
	// reflection
	BaseModuleToolInfo(ModuleManager^ manager, Type^ classType, Type^ attributeType);
	// dynamic
	BaseModuleToolInfo(ModuleManager^ manager, Guid id, BaseModuleToolAttribute^ attribute);
	// cache
	BaseModuleToolInfo(ModuleManager^ manager, System::Collections::IEnumerator^ data, BaseModuleToolAttribute^ attribute);
	BaseModuleToolAttribute^ GetAttribute() { return _Attribute; }
private:
	void Init();
private:
	// Any tool has the module manager. Handlers may or may not have it.
	ModuleManager^ const _ModuleManager;
	// Tool ID;
	Guid _Id;
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
	ModuleToolInfo(ModuleManager^ manager, Type^ classType);
	ModuleToolInfo(ModuleManager^ manager, Guid id, EventHandler<ModuleToolEventArgs^>^ handler, ModuleToolAttribute^ attribute);
	ModuleToolInfo(ModuleManager^ manager, System::Collections::IEnumerator^ data);
	virtual void WriteCache(List<String^>^ data) override;
	virtual String^ ToString() override;
	void Invoke(Object^ sender, ModuleToolEventArgs^ e);
	bool HasHandler(EventHandler<ModuleToolEventArgs^>^ handler) { return _Handler == handler; }
	property Char HotkeyChar { Char get(); }
	property String^ HotkeyText { String^ get(); }
	void SetHotkey(String^ value);
	String^ GetMenuText();
public:
	property ModuleToolAttribute^ Attribute { ModuleToolAttribute^ get() { return (ModuleToolAttribute^)GetAttribute(); } }
private:
	EventHandler<ModuleToolEventArgs^>^ _Handler;
	Char _Hotkey;
};

ref class ModuleCommandInfo sealed : BaseModuleToolInfo
{
public:
	ModuleCommandInfo(ModuleManager^ manager, Guid id, EventHandler<ModuleCommandEventArgs^>^ handler, ModuleCommandAttribute^ attribute);
	ModuleCommandInfo(ModuleManager^ manager, System::Collections::IEnumerator^ data);
	ModuleCommandInfo(ModuleManager^ manager, Type^ classType);
	virtual void WriteCache(List<String^>^ data) override;
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

ref class ModuleEditorInfo sealed : BaseModuleToolInfo
{
public:
	ModuleEditorInfo(ModuleManager^ manager, Guid id, EventHandler^ handler, ModuleEditorAttribute^ attribute);
	ModuleEditorInfo(ModuleManager^ manager, System::Collections::IEnumerator^ data);
	ModuleEditorInfo(ModuleManager^ manager, Type^ classType);
	virtual void WriteCache(List<String^>^ data) override;
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

ref class ModuleFilerInfo sealed : BaseModuleToolInfo
{
public:
	ModuleFilerInfo(ModuleManager^ manager, Guid id, EventHandler<ModuleFilerEventArgs^>^ handler, ModuleFilerAttribute^ attribute);
	ModuleFilerInfo(ModuleManager^ manager, System::Collections::IEnumerator^ data);
	ModuleFilerInfo(ModuleManager^ manager, Type^ classType);
	virtual void WriteCache(List<String^>^ data) override;
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

}
