/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "PluginInfo.h"
#include "Far.h"
#include "Plugin0.h"

namespace FarNet
{;
#pragma region BasePluginInfo

BasePluginInfo::BasePluginInfo(BasePlugin^ plugin, String^ name)
: _Plugin(plugin)
, _Name(name)
{}

BasePluginInfo::BasePluginInfo(String^ assemblyPath, String^ className, String^ name)
: _AssemblyPath(assemblyPath), _ClassName(className), _Name(name)
{}

String^ BasePluginInfo::ToString()
{
	//?? Do not call property 'ClassName' to avoid premature loading
	return String::Format("{0} Name='{1}' Class='{2}'", GetType()->FullName, _Name, _ClassName);
}

String^ BasePluginInfo::AssemblyPath::get()
{
	return _Plugin ? Assembly::GetAssembly(_Plugin->GetType())->Location : _AssemblyPath;
}

String^ BasePluginInfo::ClassName::get()
{
	return _Plugin ? _Plugin->GetType()->FullName : _ClassName;
}

String^ BasePluginInfo::Key::get()
{
	String^ path = AssemblyPath;
	if (path)
		return Path::GetFileName(path) + "\\" + _Name->Replace("\\", "/");
	else
		return ">" + _Name->Replace("\\", "/");
}

void BasePluginInfo::Connect()
{
	LOG_AUTO(3, String::Format("Load plugin Class='{0}' Path='{1}'", _ClassName, _AssemblyPath));

	if (_Plugin)
		throw gcnew InvalidOperationException("Plugin is already connected.");

	// create from info
	Assembly^ assembly = Assembly::LoadFrom(_AssemblyPath);
	Type^ type = assembly->GetType(_ClassName, true);
	_Plugin = (BasePlugin^)Activator::CreateInstance(type);

	// drop info
	_AssemblyPath = nullptr;
	_ClassName = nullptr;

	// register, attach and connect
	Plugin0::AddPlugin(_Plugin);
	_Plugin->Far = Far::Instance;
	{
		LOG_AUTO(3, String::Format("{0}.Connect", _Plugin));

		_Plugin->Connect();
	}
}

#pragma endregion

#pragma region ToolPluginInfo

ToolPluginInfo::ToolPluginInfo(BasePlugin^ plugin, String^ name, EventHandler<ToolEventArgs^>^ handler, ToolOptions options)
: BasePluginInfo(plugin, name)
, _Handler(handler)
, _Options(options)
{}

String^ ToolPluginInfo::ToString()
{
	return String::Format("{0} Options='{1}'", BasePluginInfo::ToString(), Options);
}

String^ ToolPluginInfo::Alias(ToolOptions option)
{
	if (ES(Name))
		return String::Empty;
	switch(option)
	{
	case ToolOptions::Config:
		if (ES(_AliasConfig))
			_AliasConfig = Far::Instance->GetFarNetValue(Key, "Config", Name)->ToString();
		return _AliasConfig;
	case ToolOptions::Disk:
		if (ES(_AliasDisk))
			_AliasDisk = Far::Instance->GetFarNetValue(Key, "Disk", Name)->ToString();
		return _AliasDisk;
	case ToolOptions::Editor:
		if (ES(_AliasEditor))
			_AliasEditor = Far::Instance->GetFarNetValue(Key, "Editor", Name)->ToString();
		return _AliasEditor;
	case ToolOptions::Panels:
		if (ES(_AliasPanels))
			_AliasPanels = Far::Instance->GetFarNetValue(Key, "Panels", Name)->ToString();
		return _AliasPanels;
	case ToolOptions::Viewer:
		if (ES(_AliasViewer))
			_AliasViewer = Far::Instance->GetFarNetValue(Key, "Viewer", Name)->ToString();
		return _AliasViewer;
	case ToolOptions::Dialog:
		if (ES(_AliasDialog))
			_AliasDialog = Far::Instance->GetFarNetValue(Key, "Dialog", Name)->ToString();
		return _AliasDialog;
	default:
		throw gcnew InvalidOperationException("Unknown tool option.");
	}
}

void ToolPluginInfo::Alias(ToolOptions option, String^ value)
{
	if (ES(value))
		throw gcnew ArgumentException("'value' must not be empty.");

	switch(option)
	{
	case ToolOptions::Config:
		Far::Instance->SetFarNetValue(Key, "Config", value);
		_AliasConfig = value;
		break;
	case ToolOptions::Disk:
		Far::Instance->SetFarNetValue(Key, "Disk", value);
		_AliasDisk = value;
		break;
	case ToolOptions::Editor:
		Far::Instance->SetFarNetValue(Key, "Editor", value);
		_AliasEditor = value;
		break;
	case ToolOptions::Panels:
		Far::Instance->SetFarNetValue(Key, "Panels", value);
		_AliasPanels = value;
		break;
	case ToolOptions::Viewer:
		Far::Instance->SetFarNetValue(Key, "Viewer", value);
		_AliasViewer = value;
		break;
	case ToolOptions::Dialog:
		Far::Instance->SetFarNetValue(Key, "Dialog", value);
		_AliasDialog = value;
		break;
	default:
		throw gcnew InvalidOperationException("Unknown tool option.");
	}
}

void ToolPluginInfo::Invoke(Object^ sender, ToolEventArgs^ e)
{
	Connect();
	ToolPlugin^ instance = (ToolPlugin^)Plugin;
	_Handler = gcnew EventHandler<ToolEventArgs^>(instance, &ToolPlugin::Invoke);
	instance->Invoke(sender, e);
}

#pragma endregion

#pragma region CommandPluginInfo

String^ CommandPluginInfo::ToString()
{
	return String::Format("{0} Prefix='{1}'", BasePluginInfo::ToString(), Prefix);
}

String^ CommandPluginInfo::Prefix::get()
{
	if (ES(_Prefix))
		_Prefix = Far::Instance->GetFarNetValue(Key, "Prefix", DefaultPrefix)->ToString();
	return _Prefix;
}

void CommandPluginInfo::Prefix::set(String^ value)
{
	if (ES(value))
		throw gcnew ArgumentException("'value' must not be empty.");

	Far::Instance->SetFarNetValue(Key, "Prefix", value);
	_Prefix = value;
}

void CommandPluginInfo::Invoke(Object^ sender, CommandEventArgs^ e)
{
	// connect
	Connect();
	CommandPlugin^ instance = (CommandPlugin^)Plugin;

	// notify
	instance->Invoking();

	// invoke
	instance->Prefix = Prefix;
	_Handler = gcnew EventHandler<CommandEventArgs^>(instance, &CommandPlugin::Invoke);
	instance->Invoke(sender, e);
}

#pragma endregion

#pragma region FilerPluginInfo

String^ FilerPluginInfo::ToString()
{
	return String::Format("{0} Mask='{1}'", BasePluginInfo::ToString(), Mask);
}

void FilerPluginInfo::Invoke(Object^ sender, FilerEventArgs^ e)
{
	Connect();
	FilerPlugin^ instance = (FilerPlugin^)Plugin;
	_Handler = gcnew EventHandler<FilerEventArgs^>(instance, &FilerPlugin::Invoke);
	instance->Invoke(sender, e);
}

String^ FilerPluginInfo::Mask::get()
{
	if (ES(_Mask))
		_Mask = Far::Instance->GetFarNetValue(Key, "Mask", DefaultMask)->ToString();
	return _Mask;
}

void FilerPluginInfo::Mask::set(String^ value)
{
	if (!value) throw gcnew ArgumentNullException("value");

	Far::Instance->SetFarNetValue(Key, "Mask", value);
	_Mask = value;
}

#pragma endregion

#pragma region EditorPluginInfo

String^ EditorPluginInfo::ToString()
{
	return String::Format("{0} Mask='{1}'", BasePluginInfo::ToString(), Mask);
}

void EditorPluginInfo::Invoke(Object^ sender, EventArgs^ e)
{
	Connect();
	EditorPlugin^ instance = (EditorPlugin^)Plugin;
	_Handler = gcnew EventHandler(instance, &EditorPlugin::Invoke);
	instance->Invoke(sender, e);
}

String^ EditorPluginInfo::Mask::get()
{
	if (ES(_Mask))
		_Mask = Far::Instance->GetFarNetValue(Key, "Mask", DefaultMask)->ToString();
	return _Mask;
}

void EditorPluginInfo::Mask::set(String^ value)
{
	if (!value) throw gcnew ArgumentNullException("value");

	Far::Instance->SetFarNetValue(Key, "Mask", value);
	_Mask = value;
}

#pragma endregion

#pragma region ToolPluginAliasComparer

int ToolPluginAliasComparer::Compare(ToolPluginInfo^ x, ToolPluginInfo^ y)
{
	return String::Compare(x->Alias(_Option), y->Alias(_Option), true, CultureInfo::InvariantCulture);
}

#pragma endregion
}
