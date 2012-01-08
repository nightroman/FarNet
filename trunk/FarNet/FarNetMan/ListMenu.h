
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
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
	virtual property bool NoInfo;
	virtual property bool NoShadow;
	virtual property bool UsualMargins;
	virtual property int ScreenMargin;
	virtual property PatternOptions IncrementalOptions { PatternOptions get(); void set(PatternOptions value); }
	virtual property String^ Incremental { String^ get(); void set(String^ value); }
public:
	virtual bool Show() override;
internal:
	ListMenu();
private:
	String^ InfoLine();
	void GetInfo(String^& head, String^& foot);
	void MakeFilter();
	void MakeSizes(FarDialog^ dialog, Point size);
	void OnConsoleSizeChanged(Object^ sender, SizeEventArgs^ e);
	void OnKeyPressed(Object^ sender, KeyPressedEventArgs^ e);
private:
	FarListBox^ _box;
	// Original user defined filter
	String^ _Incremental_;
	PatternOptions _IncrementalOptions;
	// Currently used filter
	String^ _filter;
	// To update the filter
	bool _toFilter;
	// Key handler was invoked
	bool _isKeyHandled;
	// Filtered
	List<int>^ _ii;
	Regex^ _re;
};
}
