
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

#pragma once
#include "AnyMenu.h"

namespace FarNet
{
	ref class Menu : public AnyMenu, public IMenu
	{
	public:
		virtual property bool ReverseAutoAssign;
		virtual property bool ChangeConsoleTitle;
		virtual property bool NoBox;
		virtual property bool NoMargin;
		virtual property bool SingleBox;
	public:
		~Menu();
		!Menu();
		virtual void Lock();
		virtual bool Show() override;
		virtual void Unlock();
	internal:
		Menu();
	private:
		FarMenuItem* CreateItems();
		void DeleteItems(FarMenuItem* items);
		int Flags();
		FarKey* CreateBreakKeys();
		void ShowMenu(FarMenuItem* items, const FarKey* breaks, const wchar_t* title, const wchar_t* bottom, const wchar_t* help);
	private:
		FarMenuItem* _createdItems;
		FarKey* _createdBreaks;
		wchar_t* _help;
		wchar_t* _title;
		wchar_t* _bottom;
	};
}
