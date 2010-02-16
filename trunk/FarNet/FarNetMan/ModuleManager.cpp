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
		throw gcnew OperationCanceledException("The module host is already set.");

	_ModuleHostClassName = moduleHostClassName;
}

void ModuleManager::SetModuleHost(Type^ moduleHostClassType)
{
	if (HasHost())
		throw gcnew OperationCanceledException("The module host is already set.");

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
	instance->Manager = this;
	
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

BaseModuleToolInfo::BaseModuleToolInfo(ModuleManager^ manager, BaseModuleToolAttribute^ attribute)
: _ModuleManager(manager)
, _Attribute(attribute)
{
	Init();
}

BaseModuleToolInfo::BaseModuleToolInfo(ModuleManager^ manager, String^ className, BaseModuleToolAttribute^ attribute)
: _ModuleManager(manager)
, _ClassName(className)
, _Attribute(attribute)
{
	Init();
}

BaseModuleToolInfo::BaseModuleToolInfo(ModuleManager^ manager, Type^ classType, Type^ attributeType)
: _ModuleManager(manager)
, _ClassType(classType)
{
	array<Object^>^ attrs = _ClassType->GetCustomAttributes(attributeType, false);
	if (attrs->Length == 0)
		throw gcnew OperationCanceledException("Module class has no required Module* attribute.");

	_Attribute = (BaseModuleToolAttribute^)attrs[0];

	Init();
}

void BaseModuleToolInfo::Init()
{
	if (ES(_Attribute->Name))
		throw gcnew OperationCanceledException("Empty module tool name is not allowed.");
}

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
	//! may be null for handlers
	if (_ModuleManager)
		_ModuleManager->Invoking();
}

String^ BaseModuleToolInfo::ToString()
{
	return String::Format("{0} Name='{1}' Class='{2}'", GetType()->FullName, Name, ClassName);
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
		return Path::GetFileName(path) + "\\" + Name->Replace("\\", "/");
	else
		return ">" + Name->Replace("\\", "/");
}

#pragma endregion

#pragma region ModuleToolInfo

ModuleToolInfo::ModuleToolInfo(ModuleManager^ manager, EventHandler<ModuleToolEventArgs^>^ handler, ModuleToolAttribute^ attribute)
: BaseModuleToolInfo(manager, attribute)
, _Handler(handler)
{}

ModuleToolInfo::ModuleToolInfo(ModuleManager^ manager, String^ className, ModuleToolAttribute^ attribute)
: BaseModuleToolInfo(manager, className, attribute)
{}

ModuleToolInfo::ModuleToolInfo(ModuleManager^ manager, Type^ classType)
: BaseModuleToolInfo(manager, classType, ModuleToolAttribute::typeid)
{}

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
	return String::Format("{0} Options='{1}'", BaseModuleToolInfo::ToString(), Attribute->Options);
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
		throw gcnew OperationCanceledException("Unknown tool option.");
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
		throw gcnew OperationCanceledException("Unknown tool option.");
	}
}

#pragma endregion

#pragma region ModuleCommandInfo

ModuleCommandInfo::ModuleCommandInfo(ModuleManager^ manager, EventHandler<ModuleCommandEventArgs^>^ handler, ModuleCommandAttribute^ attribute)
: BaseModuleToolInfo(manager, attribute)
, _Handler(handler)
{
	Init();
}

ModuleCommandInfo::ModuleCommandInfo(ModuleManager^ manager, String^ className, ModuleCommandAttribute^ attribute)
: BaseModuleToolInfo(manager, className, attribute)
{
	Init();
}

ModuleCommandInfo::ModuleCommandInfo(ModuleManager^ manager, Type^ classType)
: BaseModuleToolInfo(manager, classType, ModuleCommandAttribute::typeid)
{
	Init();
}

void ModuleCommandInfo::Init()
{
	if (ES(Attribute->Prefix))
		throw gcnew OperationCanceledException("Empty command prefix is not allowed.");
	
	_DefaultPrefix = Attribute->Prefix;
	Attribute->Prefix = ModuleManager::GetFarNetValue(Key, "Prefix", DefaultPrefix)->ToString();
}

String^ ModuleCommandInfo::ToString()
{
	return String::Format("{0} Prefix='{1}'", BaseModuleToolInfo::ToString(), Attribute->Prefix);
}

void ModuleCommandInfo::SetPrefix(String^ value)
{
	if (ES(value))
		throw gcnew ArgumentException("'value' must not be empty.");

	ModuleManager::SetFarNetValue(Key, "Prefix", value);
	Attribute->Prefix = value;
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

ModuleFilerInfo::ModuleFilerInfo(ModuleManager^ manager, EventHandler<ModuleFilerEventArgs^>^ handler, ModuleFilerAttribute^ attribute)
: BaseModuleToolInfo(manager, attribute)
, _Handler(handler)
{
	Init();
}

ModuleFilerInfo::ModuleFilerInfo(ModuleManager^ manager, String^ className, ModuleFilerAttribute^ attribute)
: BaseModuleToolInfo(manager, className, attribute)
{
	Init();
}

ModuleFilerInfo::ModuleFilerInfo(ModuleManager^ manager, Type^ classType)
: BaseModuleToolInfo(manager, classType, ModuleFilerAttribute::typeid)
{
	Init();
}

void ModuleFilerInfo::Init()
{
	_DefaultMask = Attribute->Mask ? Attribute->Mask : String::Empty;
	Attribute->Mask = ModuleManager::GetFarNetValue(Key, "Mask", DefaultMask)->ToString();
}

String^ ModuleFilerInfo::ToString()
{
	return String::Format("{0} Mask='{1}'", BaseModuleToolInfo::ToString(), Attribute->Mask);
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

void ModuleFilerInfo::SetMask(String^ value)
{
	if (!value)
		throw gcnew ArgumentNullException("value");

	ModuleManager::SetFarNetValue(Key, "Mask", value);
	Attribute->Mask = value;
}

#pragma endregion

#pragma region ModuleEditorInfo

ModuleEditorInfo::ModuleEditorInfo(ModuleManager^ manager, EventHandler^ handler, ModuleEditorAttribute^ attribute)
: BaseModuleToolInfo(manager, attribute)
, _Handler(handler)
{
	Init();
}

ModuleEditorInfo::ModuleEditorInfo(ModuleManager^ manager, String^ className, ModuleEditorAttribute^ attribute)
: BaseModuleToolInfo(manager, className, attribute)
{
	Init();
}

ModuleEditorInfo::ModuleEditorInfo(ModuleManager^ manager, Type^ classType)
: BaseModuleToolInfo(manager, classType, ModuleEditorAttribute::typeid)
{
	Init();
}

void ModuleEditorInfo::Init()
{
	_DefaultMask = Attribute->Mask ? Attribute->Mask : String::Empty;
	Attribute->Mask = ModuleManager::GetFarNetValue(Key, "Mask", DefaultMask)->ToString();
}

String^ ModuleEditorInfo::ToString()
{
	return String::Format("{0} Mask='{1}'", BaseModuleToolInfo::ToString(), Attribute->Mask);
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

void ModuleEditorInfo::SetMask(String^ value)
{
	if (!value)
		throw gcnew ArgumentNullException("value");

	ModuleManager::SetFarNetValue(Key, "Mask", value);
	Attribute->Mask = value;
}

#pragma endregion

#pragma region ModuleToolAliasComparer

int ModuleToolAliasComparer::Compare(ModuleToolInfo^ x, ModuleToolInfo^ y)
{
	return String::Compare(x->Alias(_Option), y->Alias(_Option), true, CultureInfo::InvariantCulture);
}

#pragma endregion
}
