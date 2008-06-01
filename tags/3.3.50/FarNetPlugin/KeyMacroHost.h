/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2008 FAR.NET Team
*/

#pragma once

namespace FarNet
{;
ref class KeyMacroHost : public IKeyMacroHost
{
public:
	virtual array<String^>^ GetNames(String^ area);
	virtual KeyMacroData^ GetData(String^ area, String^ name);
	virtual void Load();
	virtual void Post(String^ macro);
	virtual void Post(String^ macro, bool enableOutput, bool disablePlugins);
	virtual void Remove(String^ area, String^ name);
	virtual void Save();
	virtual void Install(String^ area, String^ name, KeyMacroData^ data);
	virtual void Install(array<System::Collections::IDictionary^>^ dataSet);
internal:
	static KeyMacroHost _instance;
};
}
