/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#pragma once

namespace FarNet
{;
ref class BasePluginInfo abstract
{
public:
	property BasePlugin^ Plugin { BasePlugin^ get() { return _Plugin; } }
	property String^ AssemblyPath { String^ get(); }
	property String^ ClassName { String^ get(); }
	property String^ Key { String^ get(); }
	property String^ Name { String^ get() { return _Name; } }
protected:
	BasePluginInfo(BasePlugin^ plugin, String^ name);
	BasePluginInfo(String^ assemblyPath, String^ className, String^ name);
	void Connect();
private:
	BasePlugin^ _Plugin;
	String^ _AssemblyPath;
	String^ _ClassName;
	String^ _Name;
};

ref class ToolPluginInfo : BasePluginInfo
{
public:
	ToolPluginInfo(BasePlugin^ plugin, String^ name, EventHandler<ToolEventArgs^>^ handler, ToolOptions options);
	ToolPluginInfo(String^ assemblyPath, String^ className, String^ name, ToolOptions options) : BasePluginInfo(assemblyPath, className, name), _Options(options) { _Handler = gcnew EventHandler<ToolEventArgs^>(this, &ToolPluginInfo::Invoke); }
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
	String^ _AliasEditor;
	String^ _AliasPanels;
	String^ _AliasViewer;
};

ref class CommandPluginInfo : BasePluginInfo
{
public:
	CommandPluginInfo(BasePlugin^ plugin, String^ name, String^ prefix, EventHandler<CommandEventArgs^>^ handler) : BasePluginInfo(plugin, name), _DefaultPrefix(prefix), _Handler(handler) {}
	CommandPluginInfo(String^ assemblyPath, String^ className, String^ name, String^ prefix) : BasePluginInfo(assemblyPath, className, name), _DefaultPrefix(prefix) { _Handler = gcnew EventHandler<CommandEventArgs^>(this, &CommandPluginInfo::Invoke); }
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

ref class FilerPluginInfo : BasePluginInfo
{
public:
	FilerPluginInfo(BasePlugin^ plugin, String^ name, EventHandler<FilerEventArgs^>^ handler, String^ mask, bool creates) : BasePluginInfo(plugin, name), _Handler(handler), _DefaultMask(mask), _Creates(creates) {}
	FilerPluginInfo(String^ assemblyPath, String^ className, String^ name, String^ mask, bool creates) : BasePluginInfo(assemblyPath, className, name), _DefaultMask(mask), _Creates(creates) { _Handler = gcnew EventHandler<FilerEventArgs^>(this, &FilerPluginInfo::Invoke); }
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

ref class ToolPluginAliasComparer : IComparer<ToolPluginInfo^>
{
public:
	ToolPluginAliasComparer(ToolOptions option) : _Option(option) {}
	virtual int Compare(ToolPluginInfo^ x, ToolPluginInfo^ y)
	{
		return String::Compare(x->Alias(_Option), y->Alias(_Option), true, CultureInfo::InvariantCulture);
	}
private:
	ToolOptions _Option;
};

}
