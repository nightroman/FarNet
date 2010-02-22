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
	virtual property ModuleItemKind Kind { ModuleItemKind get() = 0; }
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
	// new reflected
	ProxyAction(ModuleManager^ manager, Type^ classType, Type^ attributeType);
	// new dynamic
	ProxyAction(ModuleManager^ manager, Guid id, ModuleActionAttribute^ attribute);
	// new cached
	ProxyAction(ModuleManager^ manager, EnumerableReader^ reader, ModuleActionAttribute^ attribute);
	// attribute
	ModuleActionAttribute^ GetAttribute() { return _Attribute; }
private:
	void Init();
private:
	// The only module manager shared between module items. Not null.
	ModuleManager^ const _ModuleManager;
	// Action attribute with default parameters. Not null.
	ModuleActionAttribute^ _Attribute;
	// Tool ID;
	Guid _Id;
	// Class name from the cache. Null for handlers and after loading.
	String^ _ClassName;
	// Type coming from the assembly reflection. Null for handlers.
	Type^ _ClassType;
};

ref class ProxyCommand sealed : ProxyAction, IModuleCommand
{
public:
	virtual String^ ToString() override;
	virtual void Invoke(Object^ sender, ModuleCommandEventArgs^ e);
	virtual property String^ Prefix { String^ get() { return _Prefix; } }
	virtual property ModuleItemKind Kind { ModuleItemKind get() override { return ModuleItemKind::Command; } }
internal:
	ProxyCommand(ModuleManager^ manager, Guid id, ModuleCommandAttribute^ attribute, EventHandler<ModuleCommandEventArgs^>^ handler);
	ProxyCommand(ModuleManager^ manager, EnumerableReader^ reader);
	ProxyCommand(ModuleManager^ manager, Type^ classType);
	virtual void WriteCache(List<String^>^ data) override;
internal:
	property String^ DefaultPrefix { String^ get() { return Attribute->Prefix; } }
	void SetPrefix(String^ value);
private:
	void Init();
	property ModuleCommandAttribute^ Attribute { ModuleCommandAttribute^ get() { return (ModuleCommandAttribute^)GetAttribute(); } }
private:
	// Working prefix.
	String^ _Prefix;
	// Dynamic proxy handler.
	EventHandler<ModuleCommandEventArgs^>^ _Handler;
};

ref class ProxyEditor sealed : ProxyAction
{
public:
	virtual String^ ToString() override;
	virtual property ModuleItemKind Kind { ModuleItemKind get() override { return ModuleItemKind::Editor; } }
internal:
	ProxyEditor(ModuleManager^ manager, EnumerableReader^ reader);
	ProxyEditor(ModuleManager^ manager, Type^ classType);
	virtual void WriteCache(List<String^>^ data) override;
	void Invoke(Object^ sender, ModuleEditorEventArgs^ e);
	virtual property String^ Mask { String^ get(); }
internal:
	property String^ DefaultMask { String^ get() { return Attribute->Mask; } }
	void SetMask(String^ value);
private:
	void Init();
	property ModuleEditorAttribute^ Attribute { ModuleEditorAttribute^ get() { return (ModuleEditorAttribute^)GetAttribute(); } }
private:
	// Working mask.
	String^ _Mask;
};

ref class ProxyFiler sealed : ProxyAction, IModuleFiler
{
public:
	virtual String^ ToString() override;
	virtual void Invoke(Object^ sender, ModuleFilerEventArgs^ e);
	virtual property String^ Mask { String^ get(); }
	virtual property bool Creates { bool get() { return Attribute->Creates; } }
	virtual property ModuleItemKind Kind { ModuleItemKind get() override { return ModuleItemKind::Filer; } }
internal:
	ProxyFiler(ModuleManager^ manager, Guid id, ModuleFilerAttribute^ attribute, EventHandler<ModuleFilerEventArgs^>^ handler);
	ProxyFiler(ModuleManager^ manager, EnumerableReader^ reader);
	ProxyFiler(ModuleManager^ manager, Type^ classType);
	virtual void WriteCache(List<String^>^ data) override;
internal:
	property String^ DefaultMask { String^ get() { return Attribute->Mask; } }
	void SetMask(String^ value);
private:
	void Init();
	property ModuleFilerAttribute^ Attribute { ModuleFilerAttribute^ get() { return (ModuleFilerAttribute^)GetAttribute(); } }
private:
	// Working mask.
	String^ _Mask;
	// Dynamic proxy handler.
	EventHandler<ModuleFilerEventArgs^>^ _Handler;
};

ref class ProxyTool sealed : ProxyAction, IModuleTool
{
public:
	virtual String^ ToString() override;
	virtual void Invoke(Object^ sender, ModuleToolEventArgs^ e);
	virtual property ModuleToolOptions Options { ModuleToolOptions get() { return Attribute->Options; } }
	virtual property ModuleItemKind Kind { ModuleItemKind get() override { return ModuleItemKind::Tool; } }
internal:
	ProxyTool(ModuleManager^ manager, Type^ classType);
	ProxyTool(ModuleManager^ manager, Guid id, ModuleToolAttribute^ attribute, EventHandler<ModuleToolEventArgs^>^ handler);
	ProxyTool(ModuleManager^ manager, EnumerableReader^ reader);
	virtual void WriteCache(List<String^>^ data) override;
	property Char HotkeyChar { Char get(); }
	property String^ HotkeyText { String^ get(); }
	void SetHotkey(String^ value);
	String^ GetMenuText();
private:
	property ModuleToolAttribute^ Attribute { ModuleToolAttribute^ get() { return (ModuleToolAttribute^)GetAttribute(); } }
private:
	// Dynamic proxy handler.
	EventHandler<ModuleToolEventArgs^>^ _Handler;
	// Menu hotkey.
	Char _Hotkey;
};

}
