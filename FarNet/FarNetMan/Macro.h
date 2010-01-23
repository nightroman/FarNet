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
	virtual array<String^>^ GetNames(String^ area);
	virtual Macro^ GetMacro(String^ area, String^ name);
	virtual void Load();
	virtual void Remove(String^ area, String^ name);
	virtual void Save();
	virtual void Install(array<Macro^>^ macros);
private:
	void Install(Macro^ macro);
};
}
