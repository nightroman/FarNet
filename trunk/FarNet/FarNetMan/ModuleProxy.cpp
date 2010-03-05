/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "ModuleProxy.h"
#include "Far0.h"
#include "ModuleManager.h"
#include "ModuleLoader.h"

namespace FarNet
{;
#pragma region ProxyAction

// reflection
ProxyAction::ProxyAction(ModuleManager^ manager, Type^ classType, Type^ attributeType)
: _ModuleManager(manager)
, _ClassType(classType)
, _Id(classType->GUID)
{
	array<Object^>^ attrs;

	// ID: we have already got it, now ensure it is explicitely set
	attrs = _ClassType->GetCustomAttributes(System::Runtime::InteropServices::GuidAttribute::typeid, false);
	if (attrs->Length == 0)
		throw gcnew ModuleException(Invariant::Format("The 'GuidAttribute' should be set for the class '{0}'.", _ClassType->Name));

	// Module* attribure
	attrs = _ClassType->GetCustomAttributes(attributeType, false);
	if (attrs->Length == 0)
		throw gcnew ModuleException(Invariant::Format("The '{0}' should be set for the class '{1}'.", attributeType->Name, _ClassType->Name));

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
ProxyAction::ProxyAction(ModuleManager^ manager, Guid id, ModuleActionAttribute^ attribute)
: _ModuleManager(manager)
, _Id(id)
, _Attribute(attribute)
{
	Init();
}

// cache
ProxyAction::ProxyAction(ModuleManager^ manager, EnumerableReader^ reader, ModuleActionAttribute^ attribute)
: _ModuleManager(manager)
, _Attribute(attribute)
{
	_ClassName = reader->Read();
	_Attribute->Name = reader->Read();
	String^ id = reader->Read();

	_Id = Guid(id);
}

void ProxyAction::WriteCache(List<String^>^ data)
{
	data->Add(Kind.ToString());
	data->Add(ClassName);
	data->Add(Name);
	data->Add(_Id.ToString());
}

void ProxyAction::Init()
{
	if (ES(_Attribute->Name))
		throw gcnew ModuleException("Empty module tool name is not allowed.");
}

void ProxyAction::Unregister()
{
	Far0::UnregisterProxyAction(this);
}

ModuleAction^ ProxyAction::GetInstance()
{
	if (!_ClassType)
	{
		_ClassType = _ModuleManager->AssemblyInstance->GetType(_ClassName, true, false);
		_ClassName = nullptr;
	}

	return (ModuleAction^)_ModuleManager->CreateEntry(_ClassType);
}

void ProxyAction::Invoking()
{
	_ModuleManager->Invoking();
}

String^ ProxyAction::ToString()
{
	return String::Format("{0} {1} Name='{2}'", Key, Kind, Name);
}

String^ ProxyAction::ModuleName::get()
{
	return _ModuleManager->ModuleName;
}

String^ ProxyAction::ClassName::get()
{
	return _ClassType ? _ClassType->FullName : _ClassName;
}

String^ ProxyAction::Key::get()
{
	return ModuleName + "\\" + _Id.ToString();
}

#pragma endregion

#pragma region ProxyCommand

ProxyCommand::ProxyCommand(ModuleManager^ manager, Guid id, ModuleCommandAttribute^ attribute, EventHandler<ModuleCommandEventArgs^>^ handler)
: ProxyAction(manager, id, (ModuleCommandAttribute^)attribute->Clone())
, _Handler(handler)
{
	Init();
}

ProxyCommand::ProxyCommand(ModuleManager^ manager, Type^ classType)
: ProxyAction(manager, classType, ModuleCommandAttribute::typeid)
{
	Init();
}

ProxyCommand::ProxyCommand(ModuleManager^ manager, EnumerableReader^ reader)
: ProxyAction(manager, reader, gcnew ModuleCommandAttribute)
{
	Attribute->Prefix = reader->Read();

	Init();
}

void ProxyCommand::WriteCache(List<String^>^ data)
{
	ProxyAction::WriteCache(data);
	data->Add(Attribute->Prefix);
}

void ProxyCommand::Init()
{
	// solid prefix!
	if (ES(Attribute->Prefix))
		throw gcnew ModuleException("Empty command prefix is not allowed.");

	// get the working prefix now, it is needed for the command registration
	_Prefix = ModuleManager::LoadFarNetValue(Key, "Prefix", Attribute->Prefix)->ToString();
}

String^ ProxyCommand::ToString()
{
	return String::Format("{0} Prefix='{1}'", ProxyAction::ToString(), Prefix);
}

void ProxyCommand::SetPrefix(String^ value)
{
	if (ES(value))
		throw gcnew ArgumentException("'value' must not be empty.");

	ModuleManager::SaveFarNetValue(Key, "Prefix", value);
	_Prefix = value;
}

void ProxyCommand::Invoke(Object^ sender, ModuleCommandEventArgs^ e)
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

#pragma region ProxyEditor

ProxyEditor::ProxyEditor(ModuleManager^ manager, Type^ classType)
: ProxyAction(manager, classType, ModuleEditorAttribute::typeid)
{
	Init();
}

ProxyEditor::ProxyEditor(ModuleManager^ manager, EnumerableReader^ reader)
: ProxyAction(manager, reader, gcnew ModuleEditorAttribute)
{
	Attribute->Mask = reader->Read();

	Init();
}

void ProxyEditor::Init()
{
	if (!Attribute->Mask)
		Attribute->Mask = String::Empty;
}

void ProxyEditor::WriteCache(List<String^>^ data)
{
	ProxyAction::WriteCache(data);
	data->Add(Attribute->Mask);
}

String^ ProxyEditor::Mask::get()
{
	if (!_Mask)
		_Mask = ModuleManager::LoadFarNetValue(Key, "Mask", Attribute->Mask)->ToString();
	
	return _Mask;
}

void ProxyEditor::SetMask(String^ value)
{
	if (!value)
		throw gcnew ArgumentNullException("value");

	ModuleManager::SaveFarNetValue(Key, "Mask", value);
	_Mask = value;
}

String^ ProxyEditor::ToString()
{
	return String::Format("{0} Mask='{1}'", ProxyAction::ToString(), Mask);
}

void ProxyEditor::Invoke(Object^ sender, ModuleEditorEventArgs^ e)
{
	LOG_AUTO(3, String::Format("Invoking {0} FileName='{1}'", ClassName, ((IEditor^)sender)->FileName));

	Invoking();

	ModuleEditor^ instance = (ModuleEditor^)GetInstance();
	instance->Invoke(sender, e);
}

#pragma endregion

#pragma region ProxyFiler

ProxyFiler::ProxyFiler(ModuleManager^ manager, Guid id, ModuleFilerAttribute^ attribute, EventHandler<ModuleFilerEventArgs^>^ handler)
: ProxyAction(manager, id, (ModuleFilerAttribute^)attribute->Clone())
, _Handler(handler)
{
	Init();
}

ProxyFiler::ProxyFiler(ModuleManager^ manager, Type^ classType)
: ProxyAction(manager, classType, ModuleFilerAttribute::typeid)
{
	Init();
}

ProxyFiler::ProxyFiler(ModuleManager^ manager, EnumerableReader^ reader)
: ProxyAction(manager, reader, gcnew ModuleFilerAttribute)
{
	Attribute->Mask = reader->Read();
	Attribute->Creates = bool::Parse(reader->Read());

	Init();
}

void ProxyFiler::Init()
{
	if (!Attribute->Mask)
		Attribute->Mask = String::Empty;
}

void ProxyFiler::WriteCache(List<String^>^ data)
{
	ProxyAction::WriteCache(data);
	data->Add(Attribute->Mask);
	data->Add(Attribute->Creates.ToString());
}

String^ ProxyFiler::Mask::get()
{
	if (!_Mask)
		_Mask = ModuleManager::LoadFarNetValue(Key, "Mask", Attribute->Mask)->ToString();
	
	return _Mask;
}

void ProxyFiler::SetMask(String^ value)
{
	if (!value)
		throw gcnew ArgumentNullException("value");

	ModuleManager::SaveFarNetValue(Key, "Mask", value);
	_Mask = value;
}

String^ ProxyFiler::ToString()
{
	return String::Format("{0} Mask='{1}'", ProxyAction::ToString(), Mask);
}

void ProxyFiler::Invoke(Object^ sender, ModuleFilerEventArgs^ e)
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

#pragma endregion

#pragma region ProxyTool

ProxyTool::ProxyTool(ModuleManager^ manager, Guid id, ModuleToolAttribute^ attribute, EventHandler<ModuleToolEventArgs^>^ handler)
: ProxyAction(manager, id, (ModuleToolAttribute^)attribute->Clone())
, _Handler(handler)
{}

ProxyTool::ProxyTool(ModuleManager^ manager, Type^ classType)
: ProxyAction(manager, classType, ModuleToolAttribute::typeid)
{}

ProxyTool::ProxyTool(ModuleManager^ manager, EnumerableReader^ reader)
: ProxyAction(manager, reader, gcnew ModuleToolAttribute)
{
	Attribute->Options = (ModuleToolOptions)int::Parse(reader->Read());
}

void ProxyTool::WriteCache(List<String^>^ data)
{
	ProxyAction::WriteCache(data);
	data->Add(((int)Attribute->Options).ToString());
}

ModuleToolOptions ProxyTool::Options::get()
{
	if (!_OptionsValid)
	{
		// merge with the default options
		_Options = Attribute->Options & (ModuleToolOptions)ModuleManager::LoadFarNetValue(Key, "Options", Attribute->Options);
		_OptionsValid = true;
	}
	
	return _Options;
}

void ProxyTool::SetOptions(ModuleToolOptions value)
{
	ModuleManager::SaveFarNetValue(Key, "Options", ~(int(Attribute->Options) & (~int(value))));
	_Options = value;
	_OptionsValid = true;
}

void ProxyTool::Invoke(Object^ sender, ModuleToolEventArgs^ e)
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

String^ ProxyTool::ToString()
{
	return String::Format("{0} Options='{1}'", ProxyAction::ToString(), Attribute->Options);
}

Char ProxyTool::HotkeyChar::get()
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
String^ ProxyTool::HotkeyText::get()
{
	return gcnew String(gcnew array<Char> { '&', HotkeyChar, ' ' });
}

void ProxyTool::SetHotkey(String^ value)
{
	_Hotkey = ES(value) ? ' ' : value[0];
	ModuleManager::SaveFarNetValue(Key, "Hotkey", _Hotkey == ' ' ? String::Empty : value->Substring(0, 1));
}

String^ ProxyTool::GetMenuText()
{
	return HotkeyText + Attribute->Name;
}

#pragma endregion

}
