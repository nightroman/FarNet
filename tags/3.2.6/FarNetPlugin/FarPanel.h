#pragma once

namespace FarManagerImpl
{;
public ref class FarPanel : public IPanel
{
public:
	virtual property bool Highlight { bool get(); }
	virtual property bool IsActive { bool get(); }
	virtual property bool IsPlugin { bool get(); }
	virtual property bool IsVisible { bool get(); }
	virtual property bool NumericSort { bool get(); }
	virtual property bool RealNames { bool get(); }
	virtual property bool ReverseSortOrder { bool get(); }
	virtual property bool SelectedFirst { bool get(); }
	virtual property bool ShowHidden { bool get(); }
	virtual property bool UseSortGroups { bool get(); }
	virtual property IFile^ Current { IFile^ get(); }
	virtual property IFile^ Top { IFile^ get(); }
	virtual property IFolder^ Contents { IFolder^ get(); }
	virtual property IList<IFile^>^ Selected { IList<IFile^>^ get(); }
	virtual property PanelSortMode SortMode { PanelSortMode get(); }
	virtual property PanelType Type { PanelType get(); }
	virtual property PanelViewMode ViewMode { PanelViewMode get(); }
	virtual property String^ Path { String^ get(); void set(String^ value); }
public:
	virtual void Redraw();
	virtual void Update(bool keepSelection);
public:
	virtual String^ ToString() override;
internal:
	FarPanel(bool current);
private:
	StoredItem^ ItemToFile(PluginPanelItem* i);
	void ClearContents();
	void GetBrief(PanelInfo& pi);
	void GetInfo(PanelInfo& pi);
	void RefreshContents();
private:
	bool _isCurrentPanel;
	StoredFolder^ _contents;
	List<IFile^>^ _selected;
	StoredItem^ _current;
	StoredItem^ _top;
};
}
