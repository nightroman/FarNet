/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#pragma once

namespace FarManagerImpl
{;
public ref class StoredFile : IFile
{
public:
	INL_PROP_FLAG(IsAlias, FILE_ATTRIBUTE_REPARSE_POINT);
	INL_PROP_FLAG(IsArchive, FILE_ATTRIBUTE_ARCHIVE);
	INL_PROP_FLAG(IsCompressed, FILE_ATTRIBUTE_COMPRESSED);
	INL_PROP_FLAG(IsDirectory, FILE_ATTRIBUTE_DIRECTORY);
	INL_PROP_FLAG(IsEncrypted, FILE_ATTRIBUTE_ENCRYPTED);
	INL_PROP_FLAG(IsHidden, FILE_ATTRIBUTE_HIDDEN);
	INL_PROP_FLAG(IsReadOnly, FILE_ATTRIBUTE_READONLY);
	INL_PROP_FLAG(IsSystem, FILE_ATTRIBUTE_SYSTEM);
	virtual property bool IsSelected;
	virtual property DateTime CreationTime;
	virtual property DateTime LastAccessTime;
	virtual property DateTime LastWriteTime;
	virtual property int Tag;
	virtual property Int64 Size;
	virtual property String^ AlternateName;
	virtual property String^ Description;
	virtual property String^ Owner;
	virtual property String^ Name;
public:
	virtual void SetAttributes(FileAttributes attributes)
	{
		_flags = (DWORD)attributes;
	}
public:
	virtual String^ ToString() override
	{
		return Name;
	}
internal:
	StoredFile()
	{
	}
	StoredFile(const PluginPanelItem& item)
	{
		Tag = item.UserData;
		_flags = item.FindData.dwFileAttributes;
		Name = OemToStr(item.FindData.cFileName);
		AlternateName = OemToStr(item.FindData.cAlternateFileName);
		//not used now
		//IsSelected = (item.Flags & PPIF_SELECTED) != 0;
	}
internal:
	DWORD _flags;
};

public ref class FarPanel : public IPanel
{
public:
	virtual property bool Highlight { bool get(); }
	virtual property bool IsActive { bool get(); }
	virtual property bool IsPlugin { bool get(); }
	virtual property bool IsVisible { bool get(); void set(bool value); }
	virtual property bool NumericSort { bool get(); }
	virtual property bool RealNames { bool get(); }
	virtual property bool ReverseSortOrder { bool get(); }
	virtual property bool SelectedFirst { bool get(); }
	virtual property bool ShowHidden { bool get(); }
	virtual property bool UseSortGroups { bool get(); }
	virtual property IFile^ Current { IFile^ get(); }
	virtual property IFile^ Top { IFile^ get(); }
	virtual property IList<IFile^>^ Contents { IList<IFile^>^ get(); }
	virtual property IList<IFile^>^ Selected { IList<IFile^>^ get(); }
	virtual property int CurrentIndex { int get(); }
	virtual property int TopIndex { int get(); }
	virtual property PanelSortMode SortMode { PanelSortMode get(); }
	virtual property PanelType Type { PanelType get(); }
	virtual property PanelViewMode ViewMode { PanelViewMode get(); }
	virtual property String^ Path { String^ get(); void set(String^ value); }
public:
	virtual void Redraw();
	virtual void Redraw(int current, int top);
	virtual void Update(bool keepSelection);
public:
	virtual String^ ToString() override;
internal:
	FarPanel(bool current);
private:
	static StoredFile^ ItemToFile(PluginPanelItem& i);
	void GetBrief(PanelInfo& pi);
	void GetInfo(PanelInfo& pi);
private:
	bool _isCurrentPanel;
};

}
