/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class BaseModuleInfo abstract
{
public:
	static BaseModule^ CreateModule(Type^ type);
	virtual String^ ToString() override;
	property BaseModule^ Module { BaseModule^ get() { return _Module; } }
	property String^ AssemblyPath { String^ get(); }
	property String^ ClassName { String^ get(); }
	property String^ Key { String^ get(); }
	property String^ Name { String^ get() { return _Name; } }
protected:
	BaseModuleInfo(BaseModule^ module, String^ name);
	BaseModuleInfo(String^ assemblyPath, String^ className, String^ name);
	void Connect();
private:
	BaseModule^ _Module;
	String^ _AssemblyPath;
	String^ _ClassName;
	String^ _Name;
};

ref class ModuleToolInfo : BaseModuleInfo
{
public:
	ModuleToolInfo(BaseModule^ module, String^ name, EventHandler<ToolEventArgs^>^ handler, ToolOptions options);
	ModuleToolInfo(String^ assemblyPath, String^ className, String^ name, ToolOptions options);
	virtual String^ ToString() override;
	String^ Alias(ToolOptions option);
	void Alias(ToolOptions option, String^ value);
	void Invoke(Object^ sender, ToolEventArgs^ e);
	bool HasHandler(EventHandler<ToolEventArgs^>^ handler) { return _Handler == handler; }
public:
	property ToolOptions Options { ToolOptions get() { return _Options; } }
private:
	EventHandler<ToolEventArgs^>^ _Handler;
	ToolOptions _Options;
	String^ _AliasConfig;
	String^ _AliasDisk;
	String^ _AliasDialog;
	String^ _AliasEditor;
	String^ _AliasPanels;
	String^ _AliasViewer;
};

ref class ModuleCommandInfo : BaseModuleInfo
{
public:
	ModuleCommandInfo(BaseModule^ module, String^ name, String^ prefix, EventHandler<CommandEventArgs^>^ handler);
	ModuleCommandInfo(String^ assemblyPath, String^ className, String^ name, String^ prefix);
	virtual String^ ToString() override;
	void Invoke(Object^ sender, CommandEventArgs^ e);
	bool HasHandler(EventHandler<CommandEventArgs^>^ handler) { return _Handler == handler; }
public:
	property String^ DefaultPrefix { String^ get() { return _DefaultPrefix; } }
	property String^ Prefix { String^ get(); void set(String^ value); }
private:
	EventHandler<CommandEventArgs^>^ _Handler;
	String^ _DefaultPrefix;
	String^ _Prefix;
};

ref class ModuleFilerInfo : BaseModuleInfo
{
public:
	ModuleFilerInfo(BaseModule^ module, String^ name, EventHandler<FilerEventArgs^>^ handler, String^ mask, bool creates);
	ModuleFilerInfo(String^ assemblyPath, String^ className, String^ name, String^ mask, bool creates);
	virtual String^ ToString() override;
	void Invoke(Object^ sender, FilerEventArgs^ e);
	bool HasHandler(EventHandler<FilerEventArgs^>^ handler) { return _Handler == handler; }
public:
	property bool Creates { bool get() { return _Creates; } }
	property String^ DefaultMask { String^ get() { return _DefaultMask; } }
	property String^ Mask { String^ get(); void set(String^ value); }
private:
private:
	bool _Creates;
	EventHandler<FilerEventArgs^>^ _Handler;
	String^ _DefaultMask;
	String^ _Mask;
};

ref class ModuleEditorInfo : BaseModuleInfo
{
public:
	ModuleEditorInfo(BaseModule^ module, String^ name, EventHandler^ handler, String^ mask);
	ModuleEditorInfo(String^ assemblyPath, String^ className, String^ name, String^ mask);
	virtual String^ ToString() override;
	void Invoke(Object^ sender, EventArgs^ e);
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
	ModuleToolAliasComparer(ToolOptions option) : _Option(option) {}
	virtual int Compare(ModuleToolInfo^ x, ModuleToolInfo^ y);
private:
	ToolOptions _Option;
};

}
