/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "ModuleItems.h"
#include "ModuleManager.h"
#include "ModuleLoader.h"

namespace FarNet
{;
#pragma region BaseModuleToolInfo

// reflection
BaseModuleToolInfo::BaseModuleToolInfo(ModuleManager^ manager, Type^ classType, Type^ attributeType)
: _ModuleManager(manager)
, _ClassType(classType)
, _Id(classType->GUID)
{
	array<Object^>^ attrs = _ClassType->GetCustomAttributes(attributeType, false);
	if (attrs->Length == 0)
		throw gcnew ModuleException("Module class has no required Module* attribute.");

	_Attribute = (BaseModuleToolAttribute^)attrs[0];

	Init();

	if (_Attribute->Resources)
	{
		_ModuleManager->CachedResources = true;
		String^ name = _ModuleManager->GetString(_Attribute->Name);
		if (SS(name))
			_Attribute->Name = name;
	}
}

// dynamic
BaseModuleToolInfo::BaseModuleToolInfo(ModuleManager^ manager, Guid id, BaseModuleToolAttribute^ attribute)
: _ModuleManager(manager)
, _Id(id)
, _Attribute(attribute)
{
	Init();
}

// cache
BaseModuleToolInfo::BaseModuleToolInfo(ModuleManager^ manager, System::Collections::IEnumerator^ data, BaseModuleToolAttribute^ attribute)
: _ModuleManager(manager)
, _Attribute(attribute)
{
	_ClassName = NextString(data);
	_Attribute->Name = NextString(data);
	String^ id = NextString(data);

	_Id = Guid(id);
}

void BaseModuleToolInfo::WriteCache(List<String^>^ data)
{
	data->Add(ClassName);
	data->Add(ToolName);
	data->Add(_Id.ToString());
}

void BaseModuleToolInfo::Init()
{
	if (ES(_Attribute->Name))
		throw gcnew ModuleException("Empty module tool name is not allowed.");
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
	return String::Format("{0} Name='{1}' Class='{2}'", GetType()->FullName, ToolName, ClassName);
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
		return Path::GetFileName(path) + "\\" + _Id.ToString();
	else
		return "<items>\\" + _Id.ToString();
}

#pragma endregion

#pragma region ModuleToolInfo

ModuleToolInfo::ModuleToolInfo(ModuleManager^ manager, Guid id, EventHandler<ModuleToolEventArgs^>^ handler, ModuleToolAttribute^ attribute)
: BaseModuleToolInfo(manager, id, attribute)
, _Handler(handler)
{}

ModuleToolInfo::ModuleToolInfo(ModuleManager^ manager, Type^ classType)
: BaseModuleToolInfo(manager, classType, ModuleToolAttribute::typeid)
{}

ModuleToolInfo::ModuleToolInfo(ModuleManager^ manager, System::Collections::IEnumerator^ data)
: BaseModuleToolInfo(manager, data, gcnew ModuleToolAttribute)
{
	String^ options = NextString(data);

	Attribute->Options = (ModuleToolOptions)int::Parse(options);
}

void ModuleToolInfo::WriteCache(List<String^>^ data)
{
	BaseModuleToolInfo::WriteCache(data);
	data->Add(((int)Attribute->Options).ToString());
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
	return String::Format("{0} Options='{1}'", BaseModuleToolInfo::ToString(), Attribute->Options);
}

Char ModuleToolInfo::HotkeyChar::get()
{
	// get once
	if (_Hotkey == 0)
	{
		String^ value = ModuleManager::LoadFarNetValue(Key, "Hotkey", String::Empty)->ToString();
		if (value->Length)
			_Hotkey = value[0];
		else
			_Hotkey = ' ';
	}

	return _Hotkey;
}

// 3 chars: "&x " or "&  "
String^ ModuleToolInfo::HotkeyText::get()
{
	return gcnew String(gcnew array<Char> { '&', HotkeyChar, ' ' });
}

void ModuleToolInfo::SetHotkey(String^ value)
{
	_Hotkey = ES(value) ? ' ' : value[0];
	ModuleManager::SaveFarNetValue(Key, "Hotkey", _Hotkey == ' ' ? String::Empty : value->Substring(0, 1));
}

String^ ModuleToolInfo::GetMenuText()
{
	return HotkeyText + Attribute->Name;
}

#pragma endregion

#pragma region ModuleCommandInfo

ModuleCommandInfo::ModuleCommandInfo(ModuleManager^ manager, Guid id, EventHandler<ModuleCommandEventArgs^>^ handler, ModuleCommandAttribute^ attribute)
: BaseModuleToolInfo(manager, id, attribute)
, _Handler(handler)
{
	Init();
}

ModuleCommandInfo::ModuleCommandInfo(ModuleManager^ manager, Type^ classType)
: BaseModuleToolInfo(manager, classType, ModuleCommandAttribute::typeid)
{
	Init();
}

ModuleCommandInfo::ModuleCommandInfo(ModuleManager^ manager, System::Collections::IEnumerator^ data)
: BaseModuleToolInfo(manager, data, gcnew ModuleCommandAttribute)
{
	String^ prefix = NextString(data);
	
	Attribute->Prefix = prefix;

	Init();
}

void ModuleCommandInfo::WriteCache(List<String^>^ data)
{
	BaseModuleToolInfo::WriteCache(data);
	data->Add(_DefaultPrefix);
}

void ModuleCommandInfo::Init()
{
	if (ES(Attribute->Prefix))
		throw gcnew ModuleException("Empty command prefix is not allowed.");
	
	_DefaultPrefix = Attribute->Prefix;
	Attribute->Prefix = ModuleManager::LoadFarNetValue(Key, "Prefix", DefaultPrefix)->ToString();
}

String^ ModuleCommandInfo::ToString()
{
	return String::Format("{0} Prefix='{1}'", BaseModuleToolInfo::ToString(), Attribute->Prefix);
}

void ModuleCommandInfo::SetPrefix(String^ value)
{
	if (ES(value))
		throw gcnew ArgumentException("'value' must not be empty.");

	ModuleManager::SaveFarNetValue(Key, "Prefix", value);
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

#pragma region ModuleEditorInfo

ModuleEditorInfo::ModuleEditorInfo(ModuleManager^ manager, Guid id, EventHandler^ handler, ModuleEditorAttribute^ attribute)
: BaseModuleToolInfo(manager, id, attribute)
, _Handler(handler)
{
	Init();
}

ModuleEditorInfo::ModuleEditorInfo(ModuleManager^ manager, Type^ classType)
: BaseModuleToolInfo(manager, classType, ModuleEditorAttribute::typeid)
{
	Init();
}

ModuleEditorInfo::ModuleEditorInfo(ModuleManager^ manager, System::Collections::IEnumerator^ data)
: BaseModuleToolInfo(manager, data, gcnew ModuleEditorAttribute)
{
	String^ mask = NextString(data);

	Attribute->Mask = mask;

	Init();
}

void ModuleEditorInfo::WriteCache(List<String^>^ data)
{
	BaseModuleToolInfo::WriteCache(data);
	data->Add(_DefaultMask);
}

void ModuleEditorInfo::Init()
{
	_DefaultMask = Attribute->Mask ? Attribute->Mask : String::Empty;
	Attribute->Mask = ModuleManager::LoadFarNetValue(Key, "Mask", DefaultMask)->ToString();
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

	ModuleManager::SaveFarNetValue(Key, "Mask", value);
	Attribute->Mask = value;
}

#pragma endregion

#pragma region ModuleFilerInfo

ModuleFilerInfo::ModuleFilerInfo(ModuleManager^ manager, Guid id, EventHandler<ModuleFilerEventArgs^>^ handler, ModuleFilerAttribute^ attribute)
: BaseModuleToolInfo(manager, id, attribute)
, _Handler(handler)
{
	Init();
}

ModuleFilerInfo::ModuleFilerInfo(ModuleManager^ manager, Type^ classType)
: BaseModuleToolInfo(manager, classType, ModuleFilerAttribute::typeid)
{
	Init();
}

ModuleFilerInfo::ModuleFilerInfo(ModuleManager^ manager, System::Collections::IEnumerator^ data)
: BaseModuleToolInfo(manager, data, gcnew ModuleFilerAttribute)
{
	String^ mask = NextString(data);
	String^ creates = NextString(data);

	Attribute->Mask = mask;
	Attribute->Creates = bool::Parse(creates);

	Init();
}

void ModuleFilerInfo::WriteCache(List<String^>^ data)
{
	BaseModuleToolInfo::WriteCache(data);
	data->Add(_DefaultMask);
	data->Add(Attribute->Creates.ToString());
}

void ModuleFilerInfo::Init()
{
	_DefaultMask = Attribute->Mask ? Attribute->Mask : String::Empty;
	Attribute->Mask = ModuleManager::LoadFarNetValue(Key, "Mask", DefaultMask)->ToString();
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

	ModuleManager::SaveFarNetValue(Key, "Mask", value);
	Attribute->Mask = value;
}

#pragma endregion

}
