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

#define FPPI_PROP(Type, Name)\
public: virtual property Type Name { Type get() { return _##Name; } void set(Type value) { Free(); _##Name = value; } }\
private: Type _##Name

ref class FarPanelPluginInfo : IPanelPluginInfo
{
internal:
	FarPanelPluginInfo();
	void Free();
	OpenPluginInfo& Make();
public:
	FPPI_PROP(bool, AddDots);
	FPPI_PROP(bool, CompareFatTime);
	FPPI_PROP(bool, ExternalDelete);
	FPPI_PROP(bool, ExternalGet);
	FPPI_PROP(bool, ExternalMakeDirectory);
	FPPI_PROP(bool, ExternalPut);
	FPPI_PROP(bool, PreserveCase);
	FPPI_PROP(bool, RawSelection);
	FPPI_PROP(bool, RealNames);
	FPPI_PROP(bool, RightAligned);
	FPPI_PROP(bool, ShowNamesOnly);
	FPPI_PROP(bool, StartSortDesc);
	FPPI_PROP(bool, UseAttrHighlighting);
	FPPI_PROP(bool, UseFilter);
	FPPI_PROP(bool, UseHighlighting);
	FPPI_PROP(bool, UseSortGroups);
	FPPI_PROP(PanelSortMode, StartSortMode);
	FPPI_PROP(PanelViewMode, StartViewMode);
	FPPI_PROP(String^, CurrentDirectory);
	FPPI_PROP(String^, Format);
	FPPI_PROP(String^, HostFile);
	FPPI_PROP(String^, Title);
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
	virtual property String^ StartDirectory { String^ get(); void set(String^ value); }
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
	FarPanelPlugin();
	void AssertOpen();
	List<IFile^>^ ReplaceFiles(List<IFile^>^ files);
private:
	List<IFile^>^ _files;
	FarPanelPluginInfo _info;
	String^ _StartDirectory;
};

}
