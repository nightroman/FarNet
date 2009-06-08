/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class CommandLineSelection : public ILineSelection
{
public:
	virtual property String^ Text { String^ get(); void set(String^ value); }
	virtual property int End { int get(); }
	virtual property int Length { int get(); }
	virtual property int Start { int get(); }
	virtual String^ ToString() override;
internal:
	CommandLineSelection();
};
}
