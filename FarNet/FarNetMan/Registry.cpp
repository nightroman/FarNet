/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "Registry.h"
using namespace Microsoft::Win32;

namespace FarNet
{;
String^ FarRegistryKey::RegistryPath::get()
{
	if (!_RegistryPath_)
	{
		String^ path = gcnew String(Info.RootKey);
		_RegistryPath_ = path->Substring(0, path->LastIndexOf('\\'));
	}

	return _RegistryPath_;
}

FarRegistryKey::FarRegistryKey(RegistryKey^ key)
: Key(key)
{}

FarRegistryKey::~FarRegistryKey()
{
	Key->Close();
}

FarRegistryKey::!FarRegistryKey()
{
	Key->Close();
}

String^ FarRegistryKey::Name::get()
{
	return Key->Name;
}

int FarRegistryKey::SubKeyCount::get()
{
	return Key->SubKeyCount;
}

int FarRegistryKey::ValueCount::get()
{
	return Key->ValueCount;
}

array<String^>^ FarRegistryKey::GetSubKeyNames()
{
	return Key->GetSubKeyNames();
}

array<String^>^ FarRegistryKey::GetValueNames()
{
	return Key->GetValueNames();
}

Object^ FarRegistryKey::GetValue(String^ name, Object^ defaultValue)
{
	return Key->GetValue(name, defaultValue);
}

void FarRegistryKey::SetValue(String^ name, Object^ value)
{
	if (!value)
		Key->DeleteValue(name, false);
	else if (value->GetType() == Int64::typeid)
		Key->SetValue(name, value, RegistryValueKind::QWord);
	else
		Key->SetValue(name, value);
}

Object^ FarRegistryKey::GetFarValue(String^ path, String^ valueName, Object^ defaultValue)
{
	RegistryKey^ key = nullptr;
	try
	{
		key = Registry::CurrentUser->OpenSubKey(RegistryPath + "\\" + path);
		return key ? key->GetValue(valueName, defaultValue) : defaultValue;
	}
	finally
	{
		delete key;
	}
}

void FarRegistryKey::SetFarValue(String^ path, String^ valueName, Object^ newValue)
{
	RegistryKey^ key = nullptr;
	try
	{
		key = Registry::CurrentUser->CreateSubKey(RegistryPath + "\\" + path);
		key->SetValue(valueName, newValue);
	}
	finally
	{
		if (key)
			key->Close();
	}
}

void FarRegistryKey::DeleteSubKey(String^ subkey)
{
	Key->DeleteSubKey(subkey, false);
}

IRegistryKey^ FarRegistryKey::OpenRegistryKey(String^ name, bool writable)
{
	// try to get existing
	String^ path = ES(name) ? RegistryPath : RegistryPath + "\\" + name;
	RegistryKey^ r = Registry::CurrentUser->OpenSubKey(path, writable);
	
	// create, throw
	if (!r && writable)
	{
		r = Registry::CurrentUser->CreateSubKey(path);
		if (!r)
			throw gcnew ModuleException("Cannot open the registry key.");
	}

	// ok
	return r ? gcnew FarRegistryKey(r) : nullptr;
}

}
