/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class Macro0 : public IMacro
{
public:
	virtual property bool ManualSaveLoad;
public:
	virtual array<String^>^ GetNames(MacroArea area);
	virtual Macro^ GetMacro(MacroArea area, String^ name);
	virtual MacroParseError^ Check(String^ sequence, bool silent);
	virtual void Load();
	virtual void Remove(MacroArea area, array<String^>^ names);
	virtual void Save();
	virtual void Install(array<Macro^>^ macros);
private:
	void Install(Macro^ macro);
	void Remove(MacroArea area, String^ name);
};
}
