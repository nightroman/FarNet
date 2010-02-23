/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class Macro0 sealed : IMacro
{
public:
	virtual property bool ManualSaveLoad { bool get() override { return _ManualSaveLoad; } void set(bool value) override { _ManualSaveLoad = value; } }
public:
	virtual array<String^>^ GetNames(MacroArea area) override;
	virtual Macro^ GetMacro(MacroArea area, String^ name) override;
	virtual MacroParseError^ Check(String^ sequence, bool silent) override;
	virtual Object^ GetConstant(String^ name) override;
	virtual Object^ GetVariable(String^ name) override;
	virtual void Install(array<Macro^>^ macros) override;
	virtual void InstallConstant(String^ name, Object^ value) override;
	virtual void InstallVariable(String^ name, Object^ value) override;
	virtual void Load() override;
	virtual void Remove(MacroArea area, array<String^>^ names) override;
	virtual void Save() override;
internal:
	static Macro0 Instance;
private:
	Macro0() {}
	void Install(Macro^ macro);
	void InstallScalar(MacroArea area, String^ name, Object^ value);
	void Remove(MacroArea area, String^ name);
private:
	static Object^ RegistryValueToObject(Object^ value);
	static Object^ StringToRegistryValue(String^ text);
	static Object^ GetScalar(MacroArea area, String^ name);
private:
	bool _ManualSaveLoad;
};
}
