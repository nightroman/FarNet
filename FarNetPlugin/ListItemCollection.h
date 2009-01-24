/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2009 FAR.NET Team
*/

#pragma once
#include "Menu.h"

namespace FarNet
{;
ref class FarBaseList;

ref class ListItemCollection : public Collection<IMenuItem^>
{
public:
	ListItemCollection(FarBaseList^ box);
protected:
	virtual void ClearItems() override;
	virtual void InsertItem(int index, IMenuItem^ item) override;
	virtual void RemoveItem(int index) override;
	virtual void SetItem(int index, IMenuItem^ item) override;
internal:
	FarBaseList^ _box;
};

}
