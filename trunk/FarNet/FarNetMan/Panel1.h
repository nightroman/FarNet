
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
*/

#pragma once
#include "Wrappers.h"

namespace FarNet
{;
ref class Panel1 : public IPanel
{
public:
	virtual property bool CaseSensitiveSort { bool get(); void set(bool value); }
	virtual property bool DirectoriesFirst { bool get(); void set(bool value); }
	virtual property bool Highlight { bool get(); }
	virtual property bool IsActive { bool get(); }
	virtual property bool IsLeft { bool get(); }
	virtual property bool IsNavigation { bool get(); }
	virtual property bool IsPlugin { bool get(); }
	virtual property bool IsVisible { bool get(); void set(bool value); }
	virtual property bool NumericSort { bool get(); void set(bool value); }
	virtual property bool RealNames { bool get(); void set(bool value); }
	virtual property bool SelectedFirst { bool get(); }
	virtual property bool SelectionExists { bool get(); }
	virtual property bool ShowHidden { bool get(); }
	virtual property bool UseSortGroups { bool get(); void set(bool value); }
	virtual property FarFile^ CurrentFile { FarFile^ get(); }
	virtual property IList<FarFile^>^ ShownFiles { IList<FarFile^>^ get(); }
	virtual property IList<FarFile^>^ SelectedFiles { IList<FarFile^>^ get(); }
	virtual property IList<FarFile^>^ ShownList { IList<FarFile^>^ get(); }
	virtual property IList<FarFile^>^ SelectedList { IList<FarFile^>^ get(); }
	virtual property int CurrentIndex { int get(); }
	virtual property int TopIndex { int get(); }
	virtual property PanelSortMode SortMode { PanelSortMode get(); void set(PanelSortMode value); }
	virtual property PanelKind Kind { PanelKind get(); }
	virtual property PanelPlan^ ViewPlan { PanelPlan^ get(); }
	virtual property PanelViewMode ViewMode { PanelViewMode get(); void set(PanelViewMode value); }
	virtual property Place Window { Place get(); }
	virtual property Point Frame { Point get(); }
	virtual property String^ CurrentDirectory { String^ get(); void set(String^ value); }
public:
	virtual bool GoToName(String^ name, bool fail);
	virtual array<int>^ SelectedIndexes();
	virtual void Close();
	virtual void Close(String^ path);
	virtual void GoToName(String^ name);
	virtual void GoToPath(String^ path);
	virtual void Push();
	virtual void Redraw();
	virtual void Redraw(int current, int top);
	virtual void SelectAt(array<int>^ indexes);
	virtual void SelectAll();
	virtual void SelectNames(System::Collections::IEnumerable^ names);
	virtual void UnselectAt(array<int>^ indexes);
	virtual void UnselectAll();
	virtual void UnselectNames(System::Collections::IEnumerable^ names);
	virtual void Update(bool keepSelection);
public:
	virtual String^ ToString() override;
internal:
	Panel1(bool current);
	static SetFile^ ItemToFile(const PluginPanelItem& item);
	int GetShownFileCount();
	int GetSelectedFileCount();
	virtual FarFile^ GetFile(int index, FileType type);
private:
	void Select(array<int>^ indexes, bool select);
	void SelectAll(bool select);
	void SelectNames(System::Collections::IEnumerable^ names, bool select);
internal:
	property HANDLE Handle { HANDLE get(); void set(HANDLE value); }
	property int Index { int get() { return (int)(INT_PTR)_handle; } void set(int value) { _handle = (HANDLE)(INT_PTR)value; } }
private:
	// PANEL_ACTIVE, PANEL_PASSIVE, or a plugin handle
	HANDLE _handle;
};
}
