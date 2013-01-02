
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
*/

#pragma once
#include "Utils.h"

class Settings
{
	HANDLE _handle;
public:
	Settings(const GUID& guid)
	{
		_handle = INVALID_HANDLE_VALUE;

		FarSettingsCreate settings = {sizeof(FarSettingsCreate), guid, _handle};
		if (!Info.SettingsControl(INVALID_HANDLE_VALUE, SCTL_CREATE, 0, &settings))
			throw gcnew InvalidOperationException(__FUNCTION__);
		
		_handle = settings.Handle;
	}
	~Settings()
	{
		Info.SettingsControl(_handle, SCTL_FREE, 0, 0);
	}
	HANDLE Handle() const
	{
		return _handle;
	}
	int OpenSubKey(int root, const wchar_t* name)
	{
		FarSettingsValue value = {sizeof(value), root, name};
		return (int)Info.SettingsControl(_handle, SCTL_OPENSUBKEY, 0, &value);
	}
	void Enum(int root, FarSettingsEnum& arg)
	{
		arg.Root = root;
		if (!Info.SettingsControl(_handle, SCTL_ENUM, 0, &arg))
			throw gcnew InvalidOperationException(__FUNCTION__);
	}
	bool Get(int root, String^ name, FarSettingsItem& arg)
	{
		PIN_NE(pin, name);
		arg.Root = root;
		arg.Name = pin;
		return Info.SettingsControl(_handle, SCTL_GET, 0, &arg) != 0;
	}
};
