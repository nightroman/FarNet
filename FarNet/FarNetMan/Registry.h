/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class FarRegistryKey sealed : IRegistryKey
{
public:
	virtual property String^ Name { String^ get(); }
	virtual property int SubKeyCount { int get(); }
	virtual property int ValueCount { int get(); }
public:
	~FarRegistryKey();
	!FarRegistryKey();
	virtual array<String^>^ GetSubKeyNames();
	virtual array<String^>^ GetValueNames();
	virtual Object^ GetValue(String^ name, Object^ defaultValue);
	virtual void DeleteSubKey(String^ subkey);
	virtual void SetValue(String^ name, Object^ value);
internal:
	static Object^ GetFarValue(String^ path, String^ valueName, Object^ defaultValue);
	static void SetFarValue(String^ path, String^ valueName, Object^ newValue);
	static IRegistryKey^ OpenRegistryKey(String^ name, bool writable);
private:
	FarRegistryKey(Microsoft::Win32::RegistryKey^ key);
	static property String^ RegistryPath { String^ get(); }
private:
	Microsoft::Win32::RegistryKey^ Key;
	static String^ _RegistryPath_;
};
}
