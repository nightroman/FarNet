/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class ModuleManager;

ref class ProxyAction abstract : IModuleAction
{
public:
	virtual String^ ToString() override;
	virtual property Guid Id { Guid get() { return _Id; } }
	virtual property String^ Name { String^ get() { return _Attribute->Name; } }
	virtual property String^ TypeName { String^ get() = 0; }
	virtual property String^ ModuleName { String^ get(); }
	virtual void Unregister();
internal:
	virtual void WriteCache(List<String^>^ data);
	void Invoking();
	ModuleAction^ GetInstance();
	property ModuleManager^ Manager { ModuleManager^ get() { return _ModuleManager; } }
	property String^ ClassName { String^ get(); }
	property String^ Key { String^ get(); }
protected:
	// reflection
	ProxyAction(ModuleManager^ manager, Type^ classType, Type^ attributeType);
	// dynamic
	ProxyAction(ModuleManager^ manager, Guid id, ModuleActionAttribute^ attribute);
	// cache
	ProxyAction(ModuleManager^ manager, EnumerableReader^ reader, ModuleActionAttribute^ attribute);
	ModuleActionAttribute^ GetAttribute() { return _Attribute; }
private:
	void Init();
private:
	// Any action has the module manager. Not null.
	ModuleManager^ const _ModuleManager;
	// Tool ID;
	Guid _Id;
	// Class name from the cache. Null for handlers and after getting the type.
	String^ _ClassName;
	// Type coming from the assembly reflection. Null for handlers.
	Type^ _ClassType;
	// Attribute. Not null.
	ModuleActionAttribute^ _Attribute;
};

ref class ProxyCommand sealed : ProxyAction, IModuleCommand
{
public:
	virtual String^ ToString() override;
	virtual void Invoke(Object^ sender, ModuleCommandEventArgs^ e);
	virtual property String^ Prefix { String^ get() { return Attribute->Prefix; } }
	virtual property String^ TypeName { String^ get() override { return gcnew String("Command"); } }
internal:
	ProxyCommand(ModuleManager^ manager, Guid id, ModuleCommandAttribute^ attribute, EventHandler<ModuleCommandEventArgs^>^ handler);
	ProxyCommand(ModuleManager^ manager, EnumerableReader^ reader);
	ProxyCommand(ModuleManager^ manager, Type^ classType);
	virtual void WriteCache(List<String^>^ data) override;
internal:
	property ModuleCommandAttribute^ Attribute { ModuleCommandAttribute^ get() { return (ModuleCommandAttribute^)GetAttribute(); } }
	property String^ DefaultPrefix { String^ get() { return _DefaultPrefix; } }
	void SetPrefix(String^ value);
private:
	void Init();
private:
	EventHandler<ModuleCommandEventArgs^>^ _Handler;
	String^ _DefaultPrefix;
};

ref class ProxyEditor sealed : ProxyAction
{
public:
	virtual String^ ToString() override;
	virtual property String^ TypeName { String^ get() override { return gcnew String("Editor"); } }
internal:
	ProxyEditor(ModuleManager^ manager, EnumerableReader^ reader);
	ProxyEditor(ModuleManager^ manager, Type^ classType);
	virtual void WriteCache(List<String^>^ data) override;
	void Invoke(Object^ sender, ModuleEditorEventArgs^ e);
internal:
	property ModuleEditorAttribute^ Attribute { ModuleEditorAttribute^ get() { return (ModuleEditorAttribute^)GetAttribute(); } }
	property String^ DefaultMask { String^ get() { return _DefaultMask; } }
	void SetMask(String^ value);
private:
	void Init();
private:
	String^ _DefaultMask;
};

ref class ProxyFiler sealed : ProxyAction, IModuleFiler
{
public:
	virtual String^ ToString() override;
	virtual void Invoke(Object^ sender, ModuleFilerEventArgs^ e);
	virtual property String^ Mask { String^ get() { return Attribute->Mask; } }
	virtual property bool Creates { bool get() { return Attribute->Creates; } }
	virtual property String^ TypeName { String^ get() override { return gcnew String("Filer"); } }
internal:
	ProxyFiler(ModuleManager^ manager, Guid id, ModuleFilerAttribute^ attribute, EventHandler<ModuleFilerEventArgs^>^ handler);
	ProxyFiler(ModuleManager^ manager, EnumerableReader^ reader);
	ProxyFiler(ModuleManager^ manager, Type^ classType);
	virtual void WriteCache(List<String^>^ data) override;
internal:
	property ModuleFilerAttribute^ Attribute { ModuleFilerAttribute^ get() { return (ModuleFilerAttribute^)GetAttribute(); } }
	property String^ DefaultMask { String^ get() { return _DefaultMask; } }
	void SetMask(String^ value);
private:
	void Init();
private:
	EventHandler<ModuleFilerEventArgs^>^ _Handler;
	String^ _DefaultMask;
};

ref class ProxyTool sealed : ProxyAction, IModuleTool
{
public:
	virtual String^ ToString() override;
	virtual void Invoke(Object^ sender, ModuleToolEventArgs^ e);
	virtual property ModuleToolOptions Options { ModuleToolOptions get() { return Attribute->Options; } }
	virtual property String^ TypeName { String^ get() override { return gcnew String("Tool"); } }
internal:
	ProxyTool(ModuleManager^ manager, Type^ classType);
	ProxyTool(ModuleManager^ manager, Guid id, ModuleToolAttribute^ attribute, EventHandler<ModuleToolEventArgs^>^ handler);
	ProxyTool(ModuleManager^ manager, EnumerableReader^ reader);
	virtual void WriteCache(List<String^>^ data) override;
	property Char HotkeyChar { Char get(); }
	property String^ HotkeyText { String^ get(); }
	void SetHotkey(String^ value);
	String^ GetMenuText();
internal:
	property ModuleToolAttribute^ Attribute { ModuleToolAttribute^ get() { return (ModuleToolAttribute^)GetAttribute(); } }
private:
	EventHandler<ModuleToolEventArgs^>^ _Handler;
	Char _Hotkey;
};

}
