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
	static BaseModule^ CreatePlugin(Type^ type);
	virtual String^ ToString() override;
	property BaseModule^ Module { BaseModule^ get() { return _Plugin; } }
	property String^ AssemblyPath { String^ get(); }
	property String^ ClassName { String^ get(); }
	property String^ Key { String^ get(); }
	property String^ Name { String^ get() { return _Name; } }
protected:
	BaseModuleInfo(BaseModule^ plugin, String^ name);
	BaseModuleInfo(String^ assemblyPath, String^ className, String^ name);
	void Connect();
private:
	BaseModule^ _Plugin;
	String^ _AssemblyPath;
	String^ _ClassName;
	String^ _Name;
};

ref class ModuleToolInfo : BaseModuleInfo
{
public:
	ModuleToolInfo(BaseModule^ plugin, String^ name, EventHandler<ToolEventArgs^>^ handler, ToolOptions options);
	ModuleToolInfo(String^ assemblyPath, String^ className, String^ name, ToolOptions options) : BaseModuleInfo(assemblyPath, className, name), _Options(options) { _Handler = gcnew EventHandler<ToolEventArgs^>(this, &ModuleToolInfo::Invoke); }
	virtual String^ ToString() override;
	String^ Alias(ToolOptions option);
	void Alias(ToolOptions option, String^ value);
public:
	property EventHandler<ToolEventArgs^>^ Handler { EventHandler<ToolEventArgs^>^ get() { return _Handler; } }
	property ToolOptions Options { ToolOptions get() { return _Options; } }
private:
	void Invoke(Object^ sender, ToolEventArgs^ e);
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
	ModuleCommandInfo(BaseModule^ plugin, String^ name, String^ prefix, EventHandler<CommandEventArgs^>^ handler) : BaseModuleInfo(plugin, name), _DefaultPrefix(prefix), _Handler(handler) {}
	ModuleCommandInfo(String^ assemblyPath, String^ className, String^ name, String^ prefix) : BaseModuleInfo(assemblyPath, className, name), _DefaultPrefix(prefix) { _Handler = gcnew EventHandler<CommandEventArgs^>(this, &ModuleCommandInfo::Invoke); }
	virtual String^ ToString() override;
public:
	property EventHandler<CommandEventArgs^>^ Handler { EventHandler<CommandEventArgs^>^ get() { return _Handler; } }
	property String^ DefaultPrefix { String^ get() { return _DefaultPrefix; } }
	property String^ Prefix { String^ get(); void set(String^ value); }
private:
	void Invoke(Object^ sender, CommandEventArgs^ e);
private:
	EventHandler<CommandEventArgs^>^ _Handler;
	String^ _DefaultPrefix;
	String^ _Prefix;
};

ref class ModuleFilerInfo : BaseModuleInfo
{
public:
	ModuleFilerInfo(BaseModule^ plugin, String^ name, EventHandler<FilerEventArgs^>^ handler, String^ mask, bool creates) : BaseModuleInfo(plugin, name), _Handler(handler), _DefaultMask(mask), _Creates(creates) {}
	ModuleFilerInfo(String^ assemblyPath, String^ className, String^ name, String^ mask, bool creates) : BaseModuleInfo(assemblyPath, className, name), _DefaultMask(mask), _Creates(creates) { _Handler = gcnew EventHandler<FilerEventArgs^>(this, &ModuleFilerInfo::Invoke); }
	virtual String^ ToString() override;
public:
	property bool Creates { bool get() { return _Creates; } }
	property EventHandler<FilerEventArgs^>^ Handler { EventHandler<FilerEventArgs^>^ get() { return _Handler; } }
	property String^ DefaultMask { String^ get() { return _DefaultMask; } }
	property String^ Mask { String^ get(); void set(String^ value); }
private:
	void Invoke(Object^ sender, FilerEventArgs^ e);
private:
	bool _Creates;
	EventHandler<FilerEventArgs^>^ _Handler;
	String^ _DefaultMask;
	String^ _Mask;
};

ref class ModuleEditorInfo : BaseModuleInfo
{
public:
	ModuleEditorInfo(BaseModule^ plugin, String^ name, EventHandler^ handler, String^ mask) : BaseModuleInfo(plugin, name), _Handler(handler), _DefaultMask(mask) {}
	ModuleEditorInfo(String^ assemblyPath, String^ className, String^ name, String^ mask) : BaseModuleInfo(assemblyPath, className, name), _DefaultMask(mask) { _Handler = gcnew EventHandler(this, &ModuleEditorInfo::Invoke); }
	virtual String^ ToString() override;
public:
	property EventHandler^ Handler { EventHandler^ get() { return _Handler; } }
	property String^ DefaultMask { String^ get() { return _DefaultMask; } }
	property String^ Mask { String^ get(); void set(String^ value); }
private:
	void Invoke(Object^ sender, EventArgs^ e);
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
