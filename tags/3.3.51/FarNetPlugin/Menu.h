/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2008 FAR.NET Team
*/

#pragma once
#include "AnyMenu.h"

namespace FarNet
{;
ref class Menu : public AnyMenu, public IMenu
{
public:
	virtual property bool ReverseAutoAssign;
public:
	~Menu();
	!Menu();
	virtual void Lock();
	virtual bool Show() override;
	virtual void Unlock();
internal:
	Menu();
private:
	FarMenuItemEx* CreateItems();
	int Flags();
	int* CreateBreakKeys();
	void ShowMenu(FarMenuItemEx* items, const int* breaks, const char* title, const char* bottom, const char* help);
	static ToolOptions From();
private:
	FarMenuItemEx* _createdItems;
	int* _createdBreaks;
	char* _help;
	char* _title;
	char* _bottom;
};
}
