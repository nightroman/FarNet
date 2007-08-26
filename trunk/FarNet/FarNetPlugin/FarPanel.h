/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#pragma once

namespace FarManagerImpl
{;
public ref class FarFile : IFile
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
	virtual property Int64 Length;
	virtual property Object^ Data;
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
	FarFile()
	{}
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
	virtual property bool NumericSort { bool get(); void set(bool value); }
	virtual property bool RealNames { bool get(); }
	virtual property bool ReverseSortOrder { bool get(); void set(bool value); }
	virtual property bool SelectedFirst { bool get(); }
	virtual property bool ShowHidden { bool get(); }
	virtual property bool UseSortGroups { bool get(); }
	virtual property IFile^ Current { IFile^ get(); }
	virtual property IList<IFile^>^ Contents { IList<IFile^>^ get(); }
	virtual property IList<IFile^>^ Selected { IList<IFile^>^ get(); }
	virtual property IList<IFile^>^ Targeted { IList<IFile^>^ get(); }
	virtual property int CurrentIndex { int get(); }
	virtual property int TopIndex { int get(); }
	virtual property PanelSortMode SortMode { PanelSortMode get(); void set(PanelSortMode value); }
	virtual property PanelType Type { PanelType get(); }
	virtual property PanelViewMode ViewMode { PanelViewMode get(); void set(PanelViewMode value); }
	virtual property Place Window { Place get(); }
	virtual property Point Frame { Point get(); }
	virtual property String^ Path { String^ get(); void set(String^ value); }
public:
	virtual void Close();
	virtual void Close(String^ path);
	virtual void Redraw();
	virtual void Redraw(int current, int top);
	virtual void Update(bool keepSelection);
public:
	virtual String^ ToString() override;
internal:
	FarPanel(bool current);
protected:
	void GetBrief(PanelInfo& pi);
	void GetInfo(PanelInfo& pi);
private:
	static FarFile^ ItemToFile(PluginPanelItem& item);
internal:
	property int Id { int get(); void set(int value); }
	bool _active;
private:
	HANDLE _id;
};

ref class FarPanelPluginInfo : IPanelPluginInfo
{
internal:
	FarPanelPluginInfo()
		: m(new OpenPluginInfo)
	{
		memset(m, 0, sizeof(*m));
		m->StructSize = sizeof(*m);
	}
	~FarPanelPluginInfo()
	{
		if (m)
		{
			delete m->CurDir;
			delete m->Format;
			delete m->HostFile;
			delete m->PanelTitle;
			delete m;
		}
	}
	OpenPluginInfo& Get()
	{
		return *m;
	}
public:
	virtual property bool AddDots { bool get(); void set(bool value); }
	virtual property bool CompareFatTime { bool get(); void set(bool value); }
	virtual property bool ExternalDelete { bool get(); void set(bool value); }
	virtual property bool ExternalGet { bool get(); void set(bool value); }
	virtual property bool ExternalMakeDirectory { bool get(); void set(bool value); }
	virtual property bool ExternalPut { bool get(); void set(bool value); }
	virtual property bool PreserveCase { bool get(); void set(bool value); }
	virtual property bool RawSelection { bool get(); void set(bool value); }
	virtual property bool RealNames { bool get(); void set(bool value); }
	virtual property bool ShowNamesOnly { bool get(); void set(bool value); }
	virtual property bool RightAligned { bool get(); void set(bool value); }
	virtual property bool UseAttrHighlighting { bool get(); void set(bool value); }
	virtual property bool UseFilter { bool get(); void set(bool value); }
	virtual property bool UseHighlighting { bool get(); void set(bool value); }
	virtual property bool UseSortGroups { bool get(); void set(bool value); }
	virtual property String^ CurrentDirectory { String^ get(); void set(String^ value); }
	virtual property String^ Format { String^ get(); void set(String^ value); }
	virtual property String^ HostFile { String^ get(); void set(String^ value); }
	virtual property String^ Title { String^ get(); void set(String^ value); }
	virtual property bool StartSortDesc
	{
		bool get() { return m->StartSortOrder != 0; }
		void set(bool value) { m->StartSortOrder = value; }
	}
	virtual property PanelSortMode StartSortMode
	{
		PanelSortMode get() { return (PanelSortMode)(m->StartSortMode); }
		void set(PanelSortMode value) { m->StartSortMode = (int)value; }
	}
	virtual property PanelViewMode StartViewMode
	{
		PanelViewMode get() { return (PanelViewMode)(m->StartPanelMode - 0x30); }
		void set(PanelViewMode value) { m->StartPanelMode = (int)value + 0x30; }
	}
private:
	OpenPluginInfo* m;
};

ref class FarPanelPlugin : public FarPanel, IPanelPlugin
{
public: // FarPanel
	virtual property bool IsPlugin { bool get() override; }
	virtual property IFile^ Current { IFile^ get() override; }
	virtual property IList<IFile^>^ Contents { IList<IFile^>^ get() override; }
	virtual property IList<IFile^>^ Selected { IList<IFile^>^ get() override; }
	virtual property IList<IFile^>^ Targeted { IList<IFile^>^ get() override; }
	virtual property String^ Path { String^ get() override; void set(String^ value) override; }
public: // IPanelPlugin
	virtual property bool IsOpened { bool get(); }
	virtual property IPanelPluginInfo^ Info { IPanelPluginInfo^ get() { return %_info; } }
	virtual property IList<IFile^>^ Files { IList<IFile^>^ get(); }
	virtual property IPanelPlugin^ Another { IPanelPlugin^ get(); }
	virtual property Object^ Data;
	virtual property Object^ Host;
	virtual void Open();
	virtual void Open(IPanelPlugin^ oldPanel);
public: DEF_EVENT(GettingInfo, _GettingInfo);
public: DEF_EVENT(Closed, _Closed);
public: DEF_EVENT(CtrlBreakPressed, _CtrlBreakPressed);
public: DEF_EVENT(Idled, _Idled);
public: DEF_EVENT_ARGS(Closing, _Closing, PanelEventArgs);
public: DEF_EVENT_ARGS(DeletingFiles, _DeletingFiles, FilesEventArgs);
public: DEF_EVENT_ARGS(Executing, _Executing, ExecutingEventArgs);
public: DEF_EVENT_ARGS(GettingData, _GettingData, PanelEventArgs);
public: DEF_EVENT_ARGS(GettingFiles, _GettingFiles, GettingFilesEventArgs);
public: DEF_EVENT_ARGS(KeyPressed, _KeyPressed, PanelKeyEventArgs);
public: DEF_EVENT_ARGS(MakingDirectory, _MakingDirectory, MakingDirectoryEventArgs);
public: DEF_EVENT_ARGS(PuttingFiles, _PuttingFiles, FilesEventArgs);
public: DEF_EVENT_ARGS(Redrawing, _Redrawing, PanelEventArgs);
public: DEF_EVENT_ARGS(SettingDirectory, _SettingDirectory, SettingDirectoryEventArgs);
public: DEF_EVENT_ARGS(ViewModeChanged, _ViewModeChanged, ViewModeChangedEventArgs);
internal:
	FarPanelPlugin() : FarPanel(true) {}
	void AssertOpen();
private:
	FarPanelPluginInfo _info;
	List<IFile^> _files;
};

}
