/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "ModuleInfo.h"
#include "Far.h"
#include "Module0.h"

namespace FarNet
{;
#pragma region BaseModuleInfo

BaseModule^ BaseModuleInfo::CreateModule(Type^ type)
{
	// create the instance
	BaseModule^ instance = (BaseModule^)Activator::CreateInstance(type);

	// get and set UI culture, if any
	String^ assemblyName = Path::GetFileName(instance->GetType()->Assembly->Location);
	String^ cultureName = Far::Instance->GetFarNetValue(assemblyName , "UICulture", String::Empty)->ToString();
	if (cultureName->Length)
	{
		try
		{
			instance->CurrentUICulture = CultureInfo::GetCultureInfo(cultureName);
		}
		catch(ArgumentException^ ex)
		{
			ModuleException ex2("Invalid culture name.\rCorrect it in the configuration dialog.", ex);
			Far::Instance->ShowError(assemblyName, %ex2);
		}
	}

	return instance;
}

BaseModuleInfo::BaseModuleInfo(BaseModule^ module, String^ name)
: _Module(module)
, _Name(name)
{}

BaseModuleInfo::BaseModuleInfo(String^ assemblyPath, String^ className, String^ name)
: _AssemblyPath(assemblyPath), _ClassName(className), _Name(name)
{}

String^ BaseModuleInfo::ToString()
{
	//?? Do not call property 'ClassName' to avoid premature loading
	return String::Format("{0} Name='{1}' Class='{2}'", GetType()->FullName, _Name, _ClassName);
}

String^ BaseModuleInfo::AssemblyPath::get()
{
	return _Module ? Assembly::GetAssembly(_Module->GetType())->Location : _AssemblyPath;
}

String^ BaseModuleInfo::ClassName::get()
{
	return _Module ? _Module->GetType()->FullName : _ClassName;
}

String^ BaseModuleInfo::Key::get()
{
	String^ path = AssemblyPath;
	if (path)
		return Path::GetFileName(path) + "\\" + _Name->Replace("\\", "/");
	else
		return ">" + _Name->Replace("\\", "/");
}

void BaseModuleInfo::Connect()
{
	LOG_AUTO(3, String::Format("Load module Class='{0}' Path='{1}'", _ClassName, _AssemblyPath));

	if (_Module)
		throw gcnew InvalidOperationException("Module is already connected.");

	// create from info
	Assembly^ assembly = Assembly::LoadFrom(_AssemblyPath);
	Type^ type = assembly->GetType(_ClassName, true);
	_Module = BaseModuleInfo::CreateModule(type);

	// drop info
	_AssemblyPath = nullptr;
	_ClassName = nullptr;

	// register, attach, connect
	Module0::AddModule(_Module);
	_Module->Far = Far::Instance;
	{
		LOG_AUTO(3, String::Format("{0}.Connect", _Module));

		_Module->Connect();
	}
}

#pragma endregion

#pragma region ModuleToolInfo

ModuleToolInfo::ModuleToolInfo(BaseModule^ module, String^ name, EventHandler<ToolEventArgs^>^ handler, ToolOptions options)
: BaseModuleInfo(module, name)
, _Handler(handler)
, _Options(options)
{}

ModuleToolInfo::ModuleToolInfo(String^ assemblyPath, String^ className, String^ name, ToolOptions options)
: BaseModuleInfo(assemblyPath, className, name)
, _Options(options)
{}

void ModuleToolInfo::Invoke(Object^ sender, ToolEventArgs^ e)
{
	LOG_AUTO(3, String::Format("Invoking {0} From='{1}'", (_Handler ? Log::Format(_Handler->Method) : ClassName), e->From));

	if (!_Handler)
	{
		Connect();
		ModuleTool^ instance = (ModuleTool^)Module;
		_Handler = gcnew EventHandler<ToolEventArgs^>(instance, &ModuleTool::Invoke);
	}

	if (Module)
		Module->Invoking();

	_Handler(sender, e);
}

String^ ModuleToolInfo::ToString()
{
	return String::Format("{0} Options='{1}'", BaseModuleInfo::ToString(), Options);
}

String^ ModuleToolInfo::Alias(ToolOptions option)
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

void ModuleToolInfo::Alias(ToolOptions option, String^ value)
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

#pragma endregion

#pragma region ModuleCommandInfo

ModuleCommandInfo::ModuleCommandInfo(BaseModule^ module, String^ name, String^ prefix, EventHandler<CommandEventArgs^>^ handler)
: BaseModuleInfo(module, name)
, _DefaultPrefix(prefix)
, _Handler(handler)
{}

ModuleCommandInfo::ModuleCommandInfo(String^ assemblyPath, String^ className, String^ name, String^ prefix)
: BaseModuleInfo(assemblyPath, className, name)
, _DefaultPrefix(prefix)
{}

String^ ModuleCommandInfo::ToString()
{
	return String::Format("{0} Prefix='{1}'", BaseModuleInfo::ToString(), Prefix);
}

String^ ModuleCommandInfo::Prefix::get()
{
	if (ES(_Prefix))
		_Prefix = Far::Instance->GetFarNetValue(Key, "Prefix", DefaultPrefix)->ToString();
	return _Prefix;
}

void ModuleCommandInfo::Prefix::set(String^ value)
{
	if (ES(value))
		throw gcnew ArgumentException("'value' must not be empty.");

	Far::Instance->SetFarNetValue(Key, "Prefix", value);
	_Prefix = value;
}

void ModuleCommandInfo::Invoke(Object^ sender, CommandEventArgs^ e)
{
	LOG_AUTO(3, String::Format("Invoking {0} Command='{1}'", (_Handler ? Log::Format(_Handler->Method) : ClassName), e->Command));

	if (!_Handler)
	{
		Connect();
		ModuleCommand^ instance = (ModuleCommand^)Module;
		instance->Prefix = Prefix;
		_Handler = gcnew EventHandler<CommandEventArgs^>(instance, &ModuleCommand::Invoke);
	}

	if (Module)
		Module->Invoking();

	_Handler(sender, e);
}

#pragma endregion

#pragma region ModuleFilerInfo

ModuleFilerInfo::ModuleFilerInfo(BaseModule^ module, String^ name, EventHandler<FilerEventArgs^>^ handler, String^ mask, bool creates)
: BaseModuleInfo(module, name)
, _Handler(handler)
, _DefaultMask(mask)
, _Creates(creates)
{}

ModuleFilerInfo::ModuleFilerInfo(String^ assemblyPath, String^ className, String^ name, String^ mask, bool creates)
: BaseModuleInfo(assemblyPath, className, name)
, _DefaultMask(mask)
, _Creates(creates)
{}

String^ ModuleFilerInfo::ToString()
{
	return String::Format("{0} Mask='{1}'", BaseModuleInfo::ToString(), Mask);
}

void ModuleFilerInfo::Invoke(Object^ sender, FilerEventArgs^ e)
{
	LOG_AUTO(3, String::Format("Invoking {0} Name='{1}' Mode='{2}'", (_Handler ? Log::Format(_Handler->Method) : ClassName), e->Name, e->Mode));

	if (!_Handler)
	{
		Connect();
		ModuleFiler^ instance = (ModuleFiler^)Module;
		_Handler = gcnew EventHandler<FilerEventArgs^>(instance, &ModuleFiler::Invoke);
	}

	if (Module)
		Module->Invoking();

	_Handler(sender, e);
}

String^ ModuleFilerInfo::Mask::get()
{
	if (ES(_Mask))
		_Mask = Far::Instance->GetFarNetValue(Key, "Mask", DefaultMask)->ToString();
	return _Mask;
}

void ModuleFilerInfo::Mask::set(String^ value)
{
	if (!value) throw gcnew ArgumentNullException("value");

	Far::Instance->SetFarNetValue(Key, "Mask", value);
	_Mask = value;
}

#pragma endregion

#pragma region ModuleEditorInfo

ModuleEditorInfo::ModuleEditorInfo(BaseModule^ module, String^ name, EventHandler^ handler, String^ mask)
: BaseModuleInfo(module, name)
, _Handler(handler)
, _DefaultMask(mask)
{}

ModuleEditorInfo::ModuleEditorInfo(String^ assemblyPath, String^ className, String^ name, String^ mask)
: BaseModuleInfo(assemblyPath, className, name)
, _DefaultMask(mask)
{}

String^ ModuleEditorInfo::ToString()
{
	return String::Format("{0} Mask='{1}'", BaseModuleInfo::ToString(), Mask);
}

void ModuleEditorInfo::Invoke(Object^ sender, EventArgs^ e)
{
	LOG_AUTO(3, String::Format("Invoking {0} FileName='{1}'", (_Handler ? Log::Format(_Handler->Method) : ClassName), ((IEditor^)sender)->FileName));

	if (!_Handler)
	{
		Connect();
		ModuleEditor^ instance = (ModuleEditor^)Module;
		_Handler = gcnew EventHandler(instance, &ModuleEditor::Invoke);
	}

	if (Module)
		Module->Invoking();

	_Handler(sender, e);
}

String^ ModuleEditorInfo::Mask::get()
{
	if (ES(_Mask))
		_Mask = Far::Instance->GetFarNetValue(Key, "Mask", DefaultMask)->ToString();
	return _Mask;
}

void ModuleEditorInfo::Mask::set(String^ value)
{
	if (!value) throw gcnew ArgumentNullException("value");

	Far::Instance->SetFarNetValue(Key, "Mask", value);
	_Mask = value;
}

#pragma endregion

#pragma region ModuleToolAliasComparer

int ModuleToolAliasComparer::Compare(ModuleToolInfo^ x, ModuleToolInfo^ y)
{
	return String::Compare(x->Alias(_Option), y->Alias(_Option), true, CultureInfo::InvariantCulture);
}

#pragma endregion
}
