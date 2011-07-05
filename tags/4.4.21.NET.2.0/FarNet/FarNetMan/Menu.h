
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once
#include "AnyMenu.h"

namespace FarNet
{;
ref class Menu : public AnyMenu, public IMenu
{
public:
	virtual property bool ReverseAutoAssign;
	virtual property bool ChangeConsoleTitle;
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
	void DeleteItems(FarMenuItemEx* items);
	int Flags();
	int* CreateBreakKeys();
	void ShowMenu(FarMenuItemEx* items, const int* breaks, const wchar_t* title, const wchar_t* bottom, const wchar_t* help);
private:
	FarMenuItemEx* _createdItems;
	int* _createdBreaks;
	wchar_t* _help;
	wchar_t* _title;
	wchar_t* _bottom;
};
}
