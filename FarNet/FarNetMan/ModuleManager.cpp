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

Object^ ModuleManager::LoadPluginValue(String^ pluginName, String^ valueName, Object^ defaultValue)
{
	RegistryKey^ key = nullptr;
	try
	{
		key = Registry::CurrentUser->OpenSubKey(Far::Net->RegistryPluginsPath + "\\" + pluginName);
		return key ? key->GetValue(valueName, defaultValue) : defaultValue;
	}
	finally
	{
		if (key)
			key->Close();
	}
}

void ModuleManager::SavePluginValue(String^ pluginName, String^ valueName, Object^ newValue)
{
	RegistryKey^ key = nullptr;
	try
	{
		key = Registry::CurrentUser->CreateSubKey(Far::Net->RegistryPluginsPath + "\\" + pluginName);
		key->SetValue(valueName, newValue);
	}
	finally
	{
		if (key)
			key->Close();
	}
}

Object^ ModuleManager::LoadFarNetValue(String^ keyPath, String^ valueName, Object^ defaultValue) //?????
{
	return LoadPluginValue("FarNet\\" + keyPath, valueName, defaultValue);
}

void ModuleManager::SaveFarNetValue(String^ keyPath, String^ valueName, Object^ value)
{
	SavePluginValue("FarNet\\" + keyPath, valueName, value);
}

RegistryKey^ ModuleManager::OpenSubKey(String^ name, bool writable)
{
	String^ path = Far::Net->RegistryPluginsPath + "\\FarNet.Modules\\" + ModuleName;
	if (SS(name))
		path += "\\" + name;
	
	RegistryKey^ r = Registry::CurrentUser->OpenSubKey(path, writable);
	if (!r)
		r = Registry::CurrentUser->CreateSubKey(path);

	if (!r)
		throw gcnew ModuleException("Cannot open the registry key.");

	return r;
}

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
		throw gcnew ModuleException("The module host is already set.");

	_ModuleHostClassName = moduleHostClassName;
}

void ModuleManager::SetModuleHost(Type^ moduleHostClassType)
{
	if (HasHost())
		throw gcnew ModuleException("The module host is already set.");

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

String^ ModuleManager::ModuleName::get()
{
	return Path::GetFileName(_AssemblyPath);
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
		String^ cultureName = LoadFarNetValue(ModuleName, "UICulture", String::Empty)->ToString();
		if (cultureName->Length)
		{
			try
			{
				_CurrentUICulture = CultureInfo::GetCultureInfo(cultureName);
			}
			catch(ArgumentException^ ex)
			{
				ModuleException ex2("Invalid culture name.\rCorrect it in the configuration dialog.", ex);
				Far::Net->ShowError(ModuleName, %ex2);
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

BaseModuleItem^ ModuleManager::CreateEntry(Type^ type)
{
	// create the instance
	BaseModuleItem^ instance = (BaseModuleItem^)Activator::CreateInstance(type);
	
	// connect the instance
	instance->Manager = this;
	
	return instance;
}

}
