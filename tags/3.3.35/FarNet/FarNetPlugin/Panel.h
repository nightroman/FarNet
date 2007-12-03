/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#pragma once

namespace FarNet
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
	virtual property String^ Name
	{
		String^ get() { return _Name; }
		void set(String^ value) { if (!value) throw gcnew ArgumentNullException("value"); _Name = value; }
	}
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
	FarFile() : _Name(String::Empty) {}
internal:
	DWORD _flags;
	String^ _Name;
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
	static FarFile^ ItemToFile(PluginPanelItem& item);
protected:
	bool TryBrief(PanelInfo& pi);
	void GetBrief(PanelInfo& pi);
	void GetInfo(PanelInfo& pi);
private:
internal:
	property int Id { int get(); void set(int value); }
	bool _active;
private:
	HANDLE _id;
};

#define FPPI_FLAG(Name)\
public: virtual property bool Name {\
	bool get() { return _##Name; }\
	void set(bool value) {\
	_##Name = value;\
	if (m) m->Flags = Flags();\
}}\
private: bool _##Name

#define FPPI_PROP(Type, Name, Set)\
public: virtual property Type Name {\
	Type get() { return _##Name; }\
	void set(Type value) {\
	_##Name = value;\
	if (m) { Set; }\
}}\
private: Type _##Name

#define FPPI_TEXT(Name, Data)\
public: virtual property String^ Name {\
	String^ get() { return _##Name; }\
	void set(String^ value) {\
	_##Name = value;\
	if (m) {\
	delete[] m->Data;\
	m->Data = NewOem(value);\
	}\
}}\
private: String^ _##Name

ref class FarPanelPluginInfo : IPanelPluginInfo
{
internal:
	FarPanelPluginInfo();
	void Free();
	OpenPluginInfo& Make();
public:
	FPPI_FLAG(CompareFatTime);
	FPPI_FLAG(ExternalDelete);
	FPPI_FLAG(ExternalGet);
	FPPI_FLAG(ExternalMakeDirectory);
	FPPI_FLAG(ExternalPut);
	FPPI_FLAG(PreserveCase);
	FPPI_FLAG(RawSelection);
	FPPI_FLAG(RealNames);
	FPPI_FLAG(RightAligned);
	FPPI_FLAG(ShowNamesOnly);
	FPPI_FLAG(UseAttrHighlighting);
	FPPI_FLAG(UseFilter);
	FPPI_FLAG(UseHighlighting);
	FPPI_FLAG(UseSortGroups);
	FPPI_PROP(bool, StartSortDesc, m->StartSortOrder = _StartSortDesc);
	FPPI_PROP(PanelSortMode, StartSortMode, m->StartSortMode = int(_StartSortMode));
	FPPI_PROP(PanelViewMode, StartViewMode, m->StartPanelMode = int(_StartViewMode) + 0x30);
	FPPI_TEXT(CurrentDirectory, CurDir);
	FPPI_TEXT(Format, Format);
	FPPI_TEXT(HostFile, HostFile);
	FPPI_TEXT(Title, PanelTitle);
public:
	virtual property array<DataItem^>^ InfoItems { array<DataItem^>^ get() { return _InfoItems; } void set(array<DataItem^>^ value); }
	virtual void SetKeyBarAlt(array<String^>^ labels);
	virtual void SetKeyBarAltShift(array<String^>^ labels);
	virtual void SetKeyBarCtrl(array<String^>^ labels);
	virtual void SetKeyBarCtrlAlt(array<String^>^ labels);
	virtual void SetKeyBarCtrlShift(array<String^>^ labels);
	virtual void SetKeyBarMain(array<String^>^ labels);
	virtual void SetKeyBarShift(array<String^>^ labels);
private:
	int Flags();
	void MakeInfoItems();
	static void Free12Strings(char** dst);
	static void Make12Strings(char** dst, array<String^>^ src);
private:
	OpenPluginInfo* m;
	array<DataItem^>^ _InfoItems;
	array<String^>^ _keyBarAlt;
	array<String^>^ _keyBarAltShift;
	array<String^>^ _keyBarCtrl;
	array<String^>^ _keyBarCtrlAlt;
	array<String^>^ _keyBarCtrlShift;
	array<String^>^ _keyBarMain;
	array<String^>^ _keyBarShift;
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
	virtual property bool AddDots;
	virtual property bool IsOpened { bool get(); }
	virtual property bool IsPushed { bool get() { return _IsPushed; } }
	virtual property IPanelPluginInfo^ Info { IPanelPluginInfo^ get() { return %_info; } }
	virtual property IList<IFile^>^ Files { IList<IFile^>^ get(); }
	virtual property IPanelPlugin^ Another { IPanelPlugin^ get(); }
	virtual property Object^ Data;
	virtual property Object^ Host;
	virtual property String^ DotsDescription;
	virtual void Open();
	virtual void Open(IPanelPlugin^ oldPanel);
	virtual void PostData(Object^ data) { _postData = data; }
	virtual void PostFile(IFile^ file) { _postFile = file; }
	virtual void PostName(String^ name) { _postName = name; }
	virtual void Push();
public: DEF_EVENT(Closed, _Closed);
public: DEF_EVENT(CtrlBreakPressed, _CtrlBreakPressed);
public: DEF_EVENT(GettingInfo, _GettingInfo);
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
internal:
	bool _IsPushed;
	FarPanelPluginInfo _info;
	Object^ _postData;
	IFile^ _postFile;
	String^ _postName;
private:
	List<IFile^>^ _files;
	String^ _StartDirectory;
};

const int cPanels = 4;
ref class PanelSet
{
internal:
	static property FarPanelPlugin^ PostedPanel { FarPanelPlugin^ get() { return _panels[0]; } void set(FarPanelPlugin^ value) { _panels[0] = value; } }
	static HANDLE AddPanelPlugin(FarPanelPlugin^ plugin);
	static int AsDeleteFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int opMode);
	static int AsGetFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, char* destPath, int opMode);
	static int AsGetFindData(HANDLE hPlugin, PluginPanelItem** pPanelItem, int* pItemsNumber, int opMode);
	static int AsMakeDirectory(HANDLE hPlugin, char* name, int opMode);
	static int AsProcessEvent(HANDLE hPlugin, int id, void* param);
	static int AsProcessKey(HANDLE hPlugin, int key, unsigned int controlState);
	static int AsPutFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, int opMode);
	static int AsSetDirectory(HANDLE hPlugin, const char* dir, int opMode);
	static FarPanel^ GetPanel(bool active);
	static FarPanelPlugin^ GetPanelPlugin(Type^ hostType);
	static FarPanelPlugin^ GetPanelPlugin2(FarPanelPlugin^ plugin);
	static void AsClosePlugin(HANDLE hPlugin);
	static void AsFreeFindData(PluginPanelItem* panelItem);
	static void AsGetOpenPluginInfo(HANDLE hPlugin, OpenPluginInfo* info);
	static void OpenPanelPlugin(FarPanelPlugin^ plugin);
	static void PushPanelPlugin(FarPanelPlugin^ plugin);
	static void ReplacePanelPlugin(FarPanelPlugin^ oldPanel, FarPanelPlugin^ newPanel);
internal:
	static List<FarPanelPlugin^> _stack;
private:
	PanelSet() {}
private:
	// Posted [0] and opened [1..3] panels; i.e. size is 4, see AddPanelPlugin().
	static array<FarPanelPlugin^>^ _panels = gcnew array<FarPanelPlugin^>(cPanels);
	static bool _inAsSetDirectory;
};

}
