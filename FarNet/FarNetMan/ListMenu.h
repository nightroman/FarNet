/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#pragma once
#include "AnyMenu.h"

namespace FarNet
{;
ref class FarDialog;
ref class FarListBox;

ref class ListMenu : public AnyMenu, public IListMenu
{
public:
	virtual property bool AutoSelect;
	virtual property bool FilterRestore;
	virtual property bool NoInfo;
	virtual property bool NoShadow;
	virtual property bool UsualMargins;
	virtual property int FilterKey { int get() { return _FilterKey; } void set(int value) { _FilterKey = value; } }
	virtual property int ScreenMargin;
	virtual property PatternOptions FilterOptions;
	virtual property PatternOptions IncrementalOptions { PatternOptions get(); void set(PatternOptions value); }
	virtual property String^ Filter { String^ get(); void set(String^ value); }
	virtual property String^ FilterHistory;
	virtual property String^ Incremental { String^ get(); void set(String^ value); }
public:
	virtual bool Show() override;
	virtual void AddKey(int key);
	virtual void AddKey(int key, EventHandler<MenuEventArgs^>^ handler);
internal:
	ListMenu();
private:
	String^ InfoLine();
	void GetInfo(String^& head, String^& foot);
	void MakeFilter1();
	void MakeFilters();
	void MakeSizes(FarDialog^ dialog, Point size);
	void OnConsoleSizeChanged(Object^ sender, SizeEventArgs^ e);
	void OnKeyPressed(Object^ sender, KeyPressedEventArgs^ e);
private:
	FarListBox^ _box;
	List<EventHandler<MenuEventArgs^>^> _handlers;
	String^ _filter1_;
	int _FilterKey;
	// Original user defined filter
	String^ _Incremental_;
	PatternOptions _IncrementalOptions;
	// Currently used filter
	String^ _filter2;
	// To update permanent filter
	bool _toFilter1;
	// To update incremental filter
	bool _toFilter2;
	// Key handler was invoked
	bool _isKeyHandled;
	// Filtered
	List<int>^ _ii;
	Regex^ _re1;
	Regex^ _re2;
};
}
