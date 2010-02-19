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
#pragma region ModuleActionInfo

// reflection
ModuleActionInfo::ModuleActionInfo(ModuleManager^ manager, Type^ classType, Type^ attributeType)
: _ModuleManager(manager)
, _ClassType(classType)
, _Id(classType->GUID)
{
	array<Object^>^ attrs = _ClassType->GetCustomAttributes(attributeType, false);
	if (attrs->Length == 0)
		throw gcnew ModuleException("Module class has no required Module* attribute.");

	_Attribute = (ModuleActionAttribute^)attrs[0];

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
ModuleActionInfo::ModuleActionInfo(ModuleManager^ manager, Guid id, ModuleActionAttribute^ attribute)
: _ModuleManager(manager)
, _Id(id)
, _Attribute(attribute)
{
	Init();
}

// cache
ModuleActionInfo::ModuleActionInfo(ModuleManager^ manager, ListReader^ reader, ModuleActionAttribute^ attribute)
: _ModuleManager(manager)
, _Attribute(attribute)
{
	_ClassName = reader->Read();
	_Attribute->Name = reader->Read();
	String^ id = reader->Read();

	_Id = Guid(id);
}

void ModuleActionInfo::WriteCache(List<String^>^ data)
{
	data->Add(ClassName);
	data->Add(ToolName);
	data->Add(_Id.ToString());
}

void ModuleActionInfo::Init()
{
	if (ES(_Attribute->Name))
		throw gcnew ModuleException("Empty module tool name is not allowed.");
}

ModuleAction^ ModuleActionInfo::GetInstance()
{
	if (!_ClassType)
	{
		_ClassType = _ModuleManager->AssemblyInstance->GetType(_ClassName, true, false);
		_ClassName = nullptr;
	}

	return (ModuleAction^)_ModuleManager->CreateEntry(_ClassType);
}

void ModuleActionInfo::Invoking()
{
	//! may be null for handlers
	if (_ModuleManager)
		_ModuleManager->Invoking();
}

String^ ModuleActionInfo::ToString()
{
	return String::Format("{0} Name='{1}' Class='{2}'", GetType()->FullName, ToolName, ClassName);
}

String^ ModuleActionInfo::AssemblyPath::get()
{
	return _ModuleManager ? _ModuleManager->AssemblyPath : nullptr;
}

String^ ModuleActionInfo::ClassName::get()
{
	return _ClassType ? _ClassType->FullName : _ClassName;
}

String^ ModuleActionInfo::Key::get()
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
: ModuleActionInfo(manager, id, attribute)
, _Handler(handler)
{}

ModuleToolInfo::ModuleToolInfo(ModuleManager^ manager, Type^ classType)
: ModuleActionInfo(manager, classType, ModuleToolAttribute::typeid)
{}

ModuleToolInfo::ModuleToolInfo(ModuleManager^ manager, ListReader^ reader)
: ModuleActionInfo(manager, reader, gcnew ModuleToolAttribute)
{
	Attribute->Options = (ModuleToolOptions)int::Parse(reader->Read());
}

void ModuleToolInfo::WriteCache(List<String^>^ data)
{
	ModuleActionInfo::WriteCache(data);
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
	return String::Format("{0} Options='{1}'", ModuleActionInfo::ToString(), Attribute->Options);
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
: ModuleActionInfo(manager, id, attribute)
, _Handler(handler)
{
	Init();
}

ModuleCommandInfo::ModuleCommandInfo(ModuleManager^ manager, Type^ classType)
: ModuleActionInfo(manager, classType, ModuleCommandAttribute::typeid)
{
	Init();
}

ModuleCommandInfo::ModuleCommandInfo(ModuleManager^ manager, ListReader^ reader)
: ModuleActionInfo(manager, reader, gcnew ModuleCommandAttribute)
{
	Attribute->Prefix = reader->Read();

	Init();
}

void ModuleCommandInfo::WriteCache(List<String^>^ data)
{
	ModuleActionInfo::WriteCache(data);
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
	return String::Format("{0} Prefix='{1}'", ModuleActionInfo::ToString(), Attribute->Prefix);
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
: ModuleActionInfo(manager, id, attribute)
, _Handler(handler)
{
	Init();
}

ModuleEditorInfo::ModuleEditorInfo(ModuleManager^ manager, Type^ classType)
: ModuleActionInfo(manager, classType, ModuleEditorAttribute::typeid)
{
	Init();
}

ModuleEditorInfo::ModuleEditorInfo(ModuleManager^ manager, ListReader^ reader)
: ModuleActionInfo(manager, reader, gcnew ModuleEditorAttribute)
{
	Attribute->Mask = reader->Read();

	Init();
}

void ModuleEditorInfo::WriteCache(List<String^>^ data)
{
	ModuleActionInfo::WriteCache(data);
	data->Add(_DefaultMask);
}

void ModuleEditorInfo::Init()
{
	_DefaultMask = Attribute->Mask ? Attribute->Mask : String::Empty;
	Attribute->Mask = ModuleManager::LoadFarNetValue(Key, "Mask", DefaultMask)->ToString();
}

String^ ModuleEditorInfo::ToString()
{
	return String::Format("{0} Mask='{1}'", ModuleActionInfo::ToString(), Attribute->Mask);
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
: ModuleActionInfo(manager, id, attribute)
, _Handler(handler)
{
	Init();
}

ModuleFilerInfo::ModuleFilerInfo(ModuleManager^ manager, Type^ classType)
: ModuleActionInfo(manager, classType, ModuleFilerAttribute::typeid)
{
	Init();
}

ModuleFilerInfo::ModuleFilerInfo(ModuleManager^ manager, ListReader^ reader)
: ModuleActionInfo(manager, reader, gcnew ModuleFilerAttribute)
{
	Attribute->Mask = reader->Read();
	Attribute->Creates = bool::Parse(reader->Read());

	Init();
}

void ModuleFilerInfo::WriteCache(List<String^>^ data)
{
	ModuleActionInfo::WriteCache(data);
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
	return String::Format("{0} Mask='{1}'", ModuleActionInfo::ToString(), Attribute->Mask);
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
