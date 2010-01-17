/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class Macro : public IMacro
{
public:
	virtual array<String^>^ GetNames(String^ area);
	virtual MacroData^ GetData(String^ area, String^ name);
	virtual void Load();
	virtual void Remove(String^ area, String^ name);
	virtual void Save();
	virtual void Install(String^ area, String^ name, MacroData^ data);
	virtual void Install(array<System::Collections::IDictionary^>^ dataSet);
};
}
