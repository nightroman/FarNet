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
	virtual void ResetPrefix(String^ value);
public:
	virtual property String^ DefaultPrefix { String^ get() { return Attribute->Prefix; } }
	virtual property String^ Prefix { String^ get() { return _Prefix; } }
	virtual property ModuleItemKind Kind { ModuleItemKind get() override { return ModuleItemKind::Command; } }
internal:
	ProxyCommand(ModuleManager^ manager, Guid id, ModuleCommandAttribute^ attribute, EventHandler<ModuleCommandEventArgs^>^ handler);
	ProxyCommand(ModuleManager^ manager, EnumerableReader^ reader);
	ProxyCommand(ModuleManager^ manager, Type^ classType);
	virtual void WriteCache(List<String^>^ data) override;
internal:
private:
	void Init();
	property ModuleCommandAttribute^ Attribute { ModuleCommandAttribute^ get() { return (ModuleCommandAttribute^)GetAttribute(); } }
private:
	// Working prefix.
	String^ _Prefix;
	// Dynamic proxy handler.
	EventHandler<ModuleCommandEventArgs^>^ _Handler;
};

ref class ProxyEditor sealed : ProxyAction, IModuleEditor
{
public:
	virtual String^ ToString() override;
	virtual void Invoke(Object^ sender, ModuleEditorEventArgs^ e);
	virtual void ResetMask(String^ value);
public:
	virtual property ModuleItemKind Kind { ModuleItemKind get() override { return ModuleItemKind::Editor; } }
	virtual property String^ DefaultMask { String^ get() { return Attribute->Mask; } }
	virtual property String^ Mask { String^ get(); }
internal:
	ProxyEditor(ModuleManager^ manager, EnumerableReader^ reader);
	ProxyEditor(ModuleManager^ manager, Type^ classType);
	virtual void WriteCache(List<String^>^ data) override;
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
	virtual void ResetMask(String^ value);
public:
	virtual property String^ Mask { String^ get(); }
	virtual property bool Creates { bool get() { return Attribute->Creates; } }
	virtual property ModuleItemKind Kind { ModuleItemKind get() override { return ModuleItemKind::Filer; } }
	virtual property String^ DefaultMask { String^ get() { return Attribute->Mask; } }
internal:
	ProxyFiler(ModuleManager^ manager, Guid id, ModuleFilerAttribute^ attribute, EventHandler<ModuleFilerEventArgs^>^ handler);
	ProxyFiler(ModuleManager^ manager, EnumerableReader^ reader);
	ProxyFiler(ModuleManager^ manager, Type^ classType);
	virtual void WriteCache(List<String^>^ data) override;
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
	virtual void ResetHotkey(String^ value);
	virtual void ResetOptions(ModuleToolOptions value);
public:
	virtual property ModuleToolOptions Options { ModuleToolOptions get(); }
	virtual property ModuleItemKind Kind { ModuleItemKind get() override { return ModuleItemKind::Tool; } }
	virtual property ModuleToolOptions DefaultOptions { ModuleToolOptions get() { return Attribute->Options; } }
	virtual property String^ Hotkey { String^ get(); }
internal:
	ProxyTool(ModuleManager^ manager, Type^ classType);
	ProxyTool(ModuleManager^ manager, Guid id, ModuleToolAttribute^ attribute, EventHandler<ModuleToolEventArgs^>^ handler);
	ProxyTool(ModuleManager^ manager, EnumerableReader^ reader);
	virtual void WriteCache(List<String^>^ data) override;
	String^ GetMenuText();
private:
	property ModuleToolAttribute^ Attribute { ModuleToolAttribute^ get() { return (ModuleToolAttribute^)GetAttribute(); } }
	void SetValidHotkey(String^ value);
private:
	// Dynamic proxy handler.
	EventHandler<ModuleToolEventArgs^>^ _Handler;
	// User options.
	ModuleToolOptions _Options;
	bool _OptionsValid;
	// Menu hotkey.
	String^ _Hotkey;
};

ref class ModuleToolComparer : IComparer<IModuleTool^>
{
public:
	virtual int Compare(IModuleTool^ x, IModuleTool^ y)
	{
		return String::Compare(((ProxyTool^)x)->GetMenuText(), ((ProxyTool^)y)->GetMenuText(), true, Far::Net->GetCurrentUICulture(false));
	}
};

}
