/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "ModuleManager.h"
#include "ModuleLoader.h"

namespace FarNet
{;

ModuleManager::ModuleManager(String^ assemblyPath)
: _AssemblyPath(assemblyPath)
{}

//! Don't use Far UI
void ModuleManager::Unload()
{
	if (!_ModuleHostInstance)
		return;

	try
	{
		LOG_AUTO(3, String::Format("{0}.Disconnect", _ModuleHostInstance));
		_ModuleHostInstance->Disconnect();
	}
	catch(Exception^ e)
	{
		String^ msg = "ERROR: module " + _ModuleHostInstance->ToString() + ":\n" + Log::FormatException(e) + "\n" + e->StackTrace;
		Console::ForegroundColor = ConsoleColor::Red;
		Console::WriteLine(msg);
		Log::TraceError(msg);

		System::Threading::Thread::Sleep(1000);
	}
	finally
	{
		_ModuleHostInstance = nullptr;
	}
}

void ModuleManager::SetModuleHost(String^ moduleHostClassName)
{
	if (HasHost())
		throw gcnew InvalidOperationException("The module host is already set.");

	_ModuleHostClassName = moduleHostClassName;
}

void ModuleManager::SetModuleHost(Type^ moduleHostClassType)
{
	if (HasHost())
		throw gcnew InvalidOperationException("The module host is already set.");

	_ModuleHostClassType = moduleHostClassType;
	
	array<Object^>^ attrs = _ModuleHostClassType->GetCustomAttributes(ModuleHostAttribute::typeid, false);
	if (attrs->Length > 0 && ((ModuleHostAttribute^)attrs[0])->Load)
		Connect();
}

void ModuleManager::Invoking()
{
	if (_ModuleHostClassName)
	{
		_ModuleHostClassType = AssemblyInstance->GetType(_ModuleHostClassName, true, false);
		_ModuleHostClassName = nullptr;
	}
	
	if (_ModuleHostClassType)
		Connect();

	if (_ModuleHostInstance)
		_ModuleHostInstance->Invoking();
}

String^ ModuleManager::GetModuleHostClassName()
{
	if (_ModuleHostClassName)
		return _ModuleHostClassName;
	
	if (_ModuleHostClassType)
		return _ModuleHostClassType->FullName;

	if (_ModuleHostInstance)
		_ModuleHostInstance->GetType()->FullName;

	return nullptr;
}

void ModuleManager::Connect()
{
	_ModuleHostInstance = (ModuleHost^)CreateEntry(_ModuleHostClassType);
	_ModuleHostClassType = nullptr;

	LOG_AUTO(3, String::Format("{0}.Connect", _ModuleHostInstance));
	_ModuleHostInstance->Connect();
}

String^ ModuleManager::AssemblyPath::get()
{
	return _AssemblyPath;
}

Assembly^ ModuleManager::AssemblyInstance::get()
{
	if (!_AssemblyInstance)
		_AssemblyInstance = Assembly::LoadFrom(_AssemblyPath);

	return _AssemblyInstance;
}

CultureInfo^ ModuleManager::CurrentUICulture::get()
{
	if (!_CurrentUICulture)
	{
		// the custom culture, if any
		String^ assemblyName = Path::GetFileName(_AssemblyPath);
		String^ cultureName = GetFarNetValue(assemblyName , "UICulture", String::Empty)->ToString();
		if (cultureName->Length)
		{
			try
			{
				_CurrentUICulture = CultureInfo::GetCultureInfo(cultureName);
			}
			catch(ArgumentException^ ex)
			{
				ModuleException ex2("Invalid culture name.\rCorrect it in the configuration dialog.", ex);
				Far::Net->ShowError(assemblyName, %ex2);
			}
		}
		// the current culture
		else
		{
			Far::Net->GetCurrentUICulture(true);
		}
	}

	return _CurrentUICulture;
}

void  ModuleManager::CurrentUICulture::set(CultureInfo^ value)
{
	_CurrentUICulture = value;
}

String^ ModuleManager::GetString(String^ name)
{
	if (!_ResourceManager)
	{
		String^ baseName = Path::GetFileNameWithoutExtension(_AssemblyPath);
		String^ resourceDir = Path::GetDirectoryName(_AssemblyPath);
		_ResourceManager = ResourceManager::CreateFileBasedResourceManager(baseName, resourceDir, nullptr);
	}

	return _ResourceManager->GetString(name, CurrentUICulture);
}

BaseModuleEntry^ ModuleManager::CreateEntry(Type^ type)
{
	// create the instance
	BaseModuleEntry^ instance = (BaseModuleEntry^)Activator::CreateInstance(type);
	
	// connect the instance
	instance->ModuleManager = this;
	
	return instance;
}

Object^ ModuleManager::GetFarNetValue(String^ keyPath, String^ valueName, Object^ defaultValue)
{
	return Far::Net->GetPluginValue("FarNet\\" + keyPath, valueName, defaultValue);
}

void ModuleManager::SetFarNetValue(String^ keyPath, String^ valueName, Object^ value)
{
	Far::Net->SetPluginValue("FarNet\\" + keyPath, valueName, value);
}

#pragma region BaseModuleToolInfo

BaseModuleToolInfo::BaseModuleToolInfo(ModuleManager^ manager, String^ name)
: _ModuleManager(manager)
, _Name(name)
{}

BaseModuleToolInfo::BaseModuleToolInfo(ModuleManager^ manager, Type^ classType)
: _ModuleManager(manager), _ClassType(classType)
{}

BaseModuleToolInfo::BaseModuleToolInfo(ModuleManager^ manager, String^ className, String^ name)
: _ModuleManager(manager), _ClassName(className), _Name(name)
{}

BaseModuleTool^ BaseModuleToolInfo::GetInstance()
{
	if (!_ClassType)
	{
		_ClassType = _ModuleManager->AssemblyInstance->GetType(_ClassName, true, false);
		_ClassName = nullptr;
	}

	return (BaseModuleTool^)_ModuleManager->CreateEntry(_ClassType);
}

void BaseModuleToolInfo::Invoking()
{
	if (_ModuleManager)
		_ModuleManager->Invoking();
}

String^ BaseModuleToolInfo::ToString()
{
	return String::Format("{0} Name='{1}' Class='{2}'", GetType()->FullName, _Name, ClassName);
}

String^ BaseModuleToolInfo::AssemblyPath::get()
{
	return _ModuleManager ? _ModuleManager->AssemblyPath : nullptr;
}

String^ BaseModuleToolInfo::ClassName::get()
{
	return _ClassType ? _ClassType->FullName : _ClassName;
}

String^ BaseModuleToolInfo::Key::get()
{
	String^ path = AssemblyPath;
	if (path)
		return Path::GetFileName(path) + "\\" + _Name->Replace("\\", "/");
	else
		return ">" + _Name->Replace("\\", "/");
}

BaseModuleToolAttribute^ BaseModuleToolInfo::InitFromAttribute(Type^ attrType)
{
	BaseModuleToolAttribute^ r;
	array<Object^>^ attrs = _ClassType->GetCustomAttributes(attrType, false);
	
	if (attrs->Length == 0)
		r = (BaseModuleToolAttribute^)Activator::CreateInstance(attrType);
	else
		r = (BaseModuleToolAttribute^)attrs[0];
	
	if (ES(r->Name))
		r->Name = _ClassType->FullName;

	_Name = r->Name;
	return r;
}

#pragma endregion

#pragma region ModuleToolInfo

ModuleToolInfo::ModuleToolInfo(ModuleManager^ manager, String^ name, EventHandler<ModuleToolEventArgs^>^ handler, ModuleToolOptions options)
: BaseModuleToolInfo(manager, name)
, _Handler(handler)
, _Options(options)
{}

ModuleToolInfo::ModuleToolInfo(ModuleManager^ manager, String^ className, String^ name, ModuleToolOptions options)
: BaseModuleToolInfo(manager, className, name)
, _Options(options)
{}

ModuleToolInfo::ModuleToolInfo(ModuleManager^ manager, Type^ classType)
: BaseModuleToolInfo(manager, classType)
{
	ModuleToolAttribute^ attr = (ModuleToolAttribute^)InitFromAttribute(ModuleToolAttribute::typeid);
	_Options = attr->Options;
}

void ModuleToolInfo::Invoke(Object^ sender, ModuleToolEventArgs^ e)
{
	LOG_AUTO(3, String::Format("Invoking {0} From='{1}'", (_Handler ? Log::Format(_Handler->Method) : ClassName), e->From));

	Invoking();

	if (_Handler)
	{
		_Handler(sender, e);
	}
	else
	{
		ModuleTool^ instance = (ModuleTool^)GetInstance();
		instance->Invoke(sender, e);
	}
}

String^ ModuleToolInfo::ToString()
{
	return String::Format("{0} Options='{1}'", BaseModuleToolInfo::ToString(), Options);
}

String^ ModuleToolInfo::Alias(ModuleToolOptions option)
{
	if (ES(Name))
		return String::Empty;
	switch(option)
	{
	case ModuleToolOptions::Config:
		if (ES(_AliasConfig))
			_AliasConfig = ModuleManager::GetFarNetValue(Key, "Config", Name)->ToString();
		return _AliasConfig;
	case ModuleToolOptions::Disk:
		if (ES(_AliasDisk))
			_AliasDisk = ModuleManager::GetFarNetValue(Key, "Disk", Name)->ToString();
		return _AliasDisk;
	case ModuleToolOptions::Editor:
		if (ES(_AliasEditor))
			_AliasEditor = ModuleManager::GetFarNetValue(Key, "Editor", Name)->ToString();
		return _AliasEditor;
	case ModuleToolOptions::Panels:
		if (ES(_AliasPanels))
			_AliasPanels = ModuleManager::GetFarNetValue(Key, "Panels", Name)->ToString();
		return _AliasPanels;
	case ModuleToolOptions::Viewer:
		if (ES(_AliasViewer))
			_AliasViewer = ModuleManager::GetFarNetValue(Key, "Viewer", Name)->ToString();
		return _AliasViewer;
	case ModuleToolOptions::Dialog:
		if (ES(_AliasDialog))
			_AliasDialog = ModuleManager::GetFarNetValue(Key, "Dialog", Name)->ToString();
		return _AliasDialog;
	default:
		throw gcnew InvalidOperationException("Unknown tool option.");
	}
}

void ModuleToolInfo::Alias(ModuleToolOptions option, String^ value)
{
	if (ES(value))
		throw gcnew ArgumentException("'value' must not be empty.");

	switch(option)
	{
	case ModuleToolOptions::Config:
		ModuleManager::SetFarNetValue(Key, "Config", value);
		_AliasConfig = value;
		break;
	case ModuleToolOptions::Disk:
		ModuleManager::SetFarNetValue(Key, "Disk", value);
		_AliasDisk = value;
		break;
	case ModuleToolOptions::Editor:
		ModuleManager::SetFarNetValue(Key, "Editor", value);
		_AliasEditor = value;
		break;
	case ModuleToolOptions::Panels:
		ModuleManager::SetFarNetValue(Key, "Panels", value);
		_AliasPanels = value;
		break;
	case ModuleToolOptions::Viewer:
		ModuleManager::SetFarNetValue(Key, "Viewer", value);
		_AliasViewer = value;
		break;
	case ModuleToolOptions::Dialog:
		ModuleManager::SetFarNetValue(Key, "Dialog", value);
		_AliasDialog = value;
		break;
	default:
		throw gcnew InvalidOperationException("Unknown tool option.");
	}
}

#pragma endregion

#pragma region ModuleCommandInfo

ModuleCommandInfo::ModuleCommandInfo(ModuleManager^ manager, String^ name, String^ prefix, EventHandler<ModuleCommandEventArgs^>^ handler)
: BaseModuleToolInfo(manager, name)
, _DefaultPrefix(prefix)
, _Handler(handler)
{}

ModuleCommandInfo::ModuleCommandInfo(ModuleManager^ manager, String^ className, String^ name, String^ prefix)
: BaseModuleToolInfo(manager, className, name)
, _DefaultPrefix(prefix)
{}

ModuleCommandInfo::ModuleCommandInfo(ModuleManager^ manager, Type^ classType)
: BaseModuleToolInfo(manager, classType)
{
	ModuleCommandAttribute^ attr = (ModuleCommandAttribute^)InitFromAttribute(ModuleCommandAttribute::typeid);
	_DefaultPrefix = attr->Prefix ? attr->Prefix : classType->Name;
}

String^ ModuleCommandInfo::ToString()
{
	return String::Format("{0} Prefix='{1}'", BaseModuleToolInfo::ToString(), Prefix);
}

String^ ModuleCommandInfo::Prefix::get()
{
	if (ES(_Prefix))
		_Prefix = ModuleManager::GetFarNetValue(Key, "Prefix", DefaultPrefix)->ToString();
	return _Prefix;
}

void ModuleCommandInfo::Prefix::set(String^ value)
{
	if (ES(value))
		throw gcnew ArgumentException("'value' must not be empty.");

	ModuleManager::SetFarNetValue(Key, "Prefix", value);
	_Prefix = value;
}

void ModuleCommandInfo::Invoke(Object^ sender, ModuleCommandEventArgs^ e)
{
	LOG_AUTO(3, String::Format("Invoking {0} Command='{1}'", (_Handler ? Log::Format(_Handler->Method) : ClassName), e->Command));

	Invoking();

	if (_Handler)
	{
		_Handler(sender, e);
	}
	else
	{
		ModuleCommand^ instance = (ModuleCommand^)GetInstance();
		instance->Invoke(sender, e);
	}
}

#pragma endregion

#pragma region ModuleFilerInfo

ModuleFilerInfo::ModuleFilerInfo(ModuleManager^ manager, String^ name, EventHandler<ModuleFilerEventArgs^>^ handler, String^ mask, bool creates)
: BaseModuleToolInfo(manager, name)
, _Handler(handler)
, _DefaultMask(mask)
, _Creates(creates)
{}

ModuleFilerInfo::ModuleFilerInfo(ModuleManager^ manager, String^ className, String^ name, String^ mask, bool creates)
: BaseModuleToolInfo(manager, className, name)
, _DefaultMask(mask)
, _Creates(creates)
{}

ModuleFilerInfo::ModuleFilerInfo(ModuleManager^ manager, Type^ classType)
: BaseModuleToolInfo(manager, classType)
{
	ModuleFilerAttribute^ attr = (ModuleFilerAttribute^)InitFromAttribute(ModuleFilerAttribute::typeid);
	_DefaultMask = attr->Mask ? attr->Mask : String::Empty;
	_Creates = attr->Creates;
}

String^ ModuleFilerInfo::ToString()
{
	return String::Format("{0} Mask='{1}'", BaseModuleToolInfo::ToString(), Mask);
}

void ModuleFilerInfo::Invoke(Object^ sender, ModuleFilerEventArgs^ e)
{
	LOG_AUTO(3, String::Format("Invoking {0} Name='{1}' Mode='{2}'", (_Handler ? Log::Format(_Handler->Method) : ClassName), e->Name, e->Mode));

	Invoking();

	if (_Handler)
	{
		_Handler(sender, e);
	}
	else
	{
		ModuleFiler^ instance = (ModuleFiler^)GetInstance();
		instance->Invoke(sender, e);
	}
}

String^ ModuleFilerInfo::Mask::get()
{
	if (ES(_Mask))
		_Mask = ModuleManager::GetFarNetValue(Key, "Mask", DefaultMask)->ToString();
	return _Mask;
}

void ModuleFilerInfo::Mask::set(String^ value)
{
	if (!value) throw gcnew ArgumentNullException("value");

	ModuleManager::SetFarNetValue(Key, "Mask", value);
	_Mask = value;
}

#pragma endregion

#pragma region ModuleEditorInfo

ModuleEditorInfo::ModuleEditorInfo(ModuleManager^ manager, String^ name, EventHandler^ handler, String^ mask)
: BaseModuleToolInfo(manager, name)
, _Handler(handler)
, _DefaultMask(mask)
{}

ModuleEditorInfo::ModuleEditorInfo(ModuleManager^ manager, String^ className, String^ name, String^ mask)
: BaseModuleToolInfo(manager, className, name)
, _DefaultMask(mask)
{}

ModuleEditorInfo::ModuleEditorInfo(ModuleManager^ manager, Type^ classType)
: BaseModuleToolInfo(manager, classType)
{
	ModuleEditorAttribute^ attr = (ModuleEditorAttribute^)InitFromAttribute(ModuleEditorAttribute::typeid);
	_DefaultMask = attr->Mask ? attr->Mask : String::Empty;
}

String^ ModuleEditorInfo::ToString()
{
	return String::Format("{0} Mask='{1}'", BaseModuleToolInfo::ToString(), Mask);
}

void ModuleEditorInfo::Invoke(Object^ sender, ModuleEditorEventArgs^ e)
{
	LOG_AUTO(3, String::Format("Invoking {0} FileName='{1}'", (_Handler ? Log::Format(_Handler->Method) : ClassName), ((IEditor^)sender)->FileName));

	Invoking();

	if (_Handler)
	{
		_Handler(sender, e);
	}
	else
	{
		ModuleEditor^ instance = (ModuleEditor^)GetInstance();
		instance->Invoke(sender, e);
	}
}

String^ ModuleEditorInfo::Mask::get()
{
	if (ES(_Mask))
		_Mask = ModuleManager::GetFarNetValue(Key, "Mask", DefaultMask)->ToString();
	return _Mask;
}

void ModuleEditorInfo::Mask::set(String^ value)
{
	if (!value) throw gcnew ArgumentNullException("value");

	ModuleManager::SetFarNetValue(Key, "Mask", value);
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
