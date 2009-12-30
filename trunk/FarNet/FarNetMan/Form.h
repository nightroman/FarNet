/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once
#include "Dialog.h"

namespace FarNet
{;
ref class FarForm : IForm
{
public:
	virtual property String^ Title { String^ get(); void set(String^ value); }
public:
	virtual bool Show();
protected:
	FarForm();
protected:
	FarDialog^ _Dialog;
	IBox^ _Box;
};

}
