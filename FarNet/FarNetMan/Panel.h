/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#pragma once
#include "Wrappers.h"

namespace FarNet
{;
ref class FarPanel : public IPanel
{
public:
	virtual property bool Highlight { bool get(); }
	virtual property bool IsActive { bool get(); }
	virtual property bool IsLeft { bool get(); }
	virtual property bool IsPlugin { bool get(); }
	virtual property bool IsVisible { bool get(); void set(bool value); }
	virtual property bool NumericSort { bool get(); void set(bool value); }
	virtual property bool RealNames { bool get(); }
	virtual property bool ReverseSortOrder { bool get(); void set(bool value); }
	virtual property bool SelectedFirst { bool get(); }
	virtual property bool ShowHidden { bool get(); }
	virtual property bool UseSortGroups { bool get(); }
	virtual property FarFile^ CurrentFile { FarFile^ get(); }
	virtual property IList<FarFile^>^ ShownFiles { IList<FarFile^>^ get(); }
	virtual property IList<FarFile^>^ SelectedFiles { IList<FarFile^>^ get(); }
	virtual property IList<FarFile^>^ ShownList { IList<FarFile^>^ get(); }
	virtual property IList<FarFile^>^ SelectedList { IList<FarFile^>^ get(); }
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
	virtual void GoToName(String^ name);
	virtual void GoToPath(String^ path);
	virtual void Redraw();
	virtual void Redraw(int current, int top);
	virtual void SelectAt(array<int>^ indexes);
	virtual void SelectAll();
	virtual void UnselectAt(array<int>^ indexes);
	virtual void UnselectAll();
	virtual void Update(bool keepSelection);
public:
	virtual String^ ToString() override;
internal:
	FarPanel(bool current);
	static SetFile^ ItemToFile(const PluginPanelItem& item);
	int GetShownFileCount();
	int GetSelectedFileCount();
	virtual FarFile^ GetFile(int index, FileType type);
private:
	void Select(array<int>^ indexes, bool select);
	void SelectAll(bool select);
internal:
	property HANDLE Handle { HANDLE get(); void set(HANDLE value); }
	property int Index { int get() { return (int)(INT_PTR)_handle; } void set(int value) { _handle = (HANDLE)(INT_PTR)value; } }
private:
	// PANEL_ACTIVE, PANEL_PASSIVE, or a plugin handle
	HANDLE _handle;
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
	m->Data = NewChars(value);\
	}\
}}\
private: String^ _##Name

ref class FarPluginPanelInfo : IPluginPanelInfo
{
internal:
	FarPluginPanelInfo();
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
	virtual property bool AutoAlternateNames;
	virtual PanelModeInfo^ GetMode(PanelViewMode viewMode);
	virtual void SetKeyBarAlt(array<String^>^ labels);
	virtual void SetKeyBarAltShift(array<String^>^ labels);
	virtual void SetKeyBarCtrl(array<String^>^ labels);
	virtual void SetKeyBarCtrlAlt(array<String^>^ labels);
	virtual void SetKeyBarCtrlShift(array<String^>^ labels);
	virtual void SetKeyBarMain(array<String^>^ labels);
	virtual void SetKeyBarShift(array<String^>^ labels);
	virtual void SetMode(PanelViewMode viewMode, PanelModeInfo^ modeInfo);
private:
	int Flags();
	void CreateInfoLines();
	void CreateModes();
	void DeleteInfoLines();
	void DeleteModes();
	static void Free12Strings(wchar_t* const dst[12]);
	static void Make12Strings(wchar_t** dst, array<String^>^ src);
private:
	OpenPluginInfo* m;
	array<DataItem^>^ _InfoItems;
	array<PanelModeInfo^>^ _Modes;
	array<String^>^ _keyBarAlt;
	array<String^>^ _keyBarAltShift;
	array<String^>^ _keyBarCtrl;
	array<String^>^ _keyBarCtrlAlt;
	array<String^>^ _keyBarCtrlShift;
	array<String^>^ _keyBarMain;
	array<String^>^ _keyBarShift;
};

ref class FarPluginPanel : public FarPanel, IPluginPanel
{
public: // FarPanel
	virtual property bool IsPlugin { bool get() override; }
	virtual property Guid TypeId { Guid get(); void set(Guid value); }
	virtual property FarFile^ CurrentFile { FarFile^ get() override; }
	virtual property IList<FarFile^>^ ShownFiles { IList<FarFile^>^ get() override; }
	virtual property IList<FarFile^>^ SelectedFiles { IList<FarFile^>^ get() override; }
	virtual property String^ Path { String^ get() override; void set(String^ value) override; }
	virtual property String^ StartDirectory { String^ get(); void set(String^ value); }
public: // IPluginPanel
	virtual property bool AddDots;
	virtual property bool IdleUpdate;
	virtual property bool IsOpened { bool get(); }
	virtual property bool IsPushed { bool get() { return _IsPushed; } }
	virtual property IList<FarFile^>^ Files { IList<FarFile^>^ get(); void set(IList<FarFile^>^ value); }
	virtual property Comparison<Object^>^ DataComparison;
	virtual property IPluginPanel^ AnotherPanel { IPluginPanel^ get(); }
	virtual property IPluginPanelInfo^ Info { IPluginPanelInfo^ get() { return %_info; } }
	virtual property Object^ Data;
	virtual property Object^ Host;
	virtual property String^ DotsDescription;
	virtual void Open();
	virtual void Open(IPluginPanel^ oldPanel);
	virtual void PostData(Object^ data) { _postData = data; }
	virtual void PostFile(FarFile^ file) { _postFile = file; }
	virtual void PostName(String^ name) { _postName = name; }
	virtual void Push();
public: DEF_EVENT(Closed, _Closed);
public: DEF_EVENT(CtrlBreakPressed, _CtrlBreakPressed);
public: DEF_EVENT(GettingInfo, _GettingInfo);
public: DEF_EVENT(GotFocus, _GotFocus);
public: DEF_EVENT(Idled, _Idled);
public: DEF_EVENT(LosingFocus, _LosingFocus);
public: DEF_EVENT_ARGS(Closing, _Closing, PanelEventArgs);
public: DEF_EVENT_ARGS(DeletingFiles, _DeletingFiles, FilesEventArgs);
public: DEF_EVENT_ARGS(Escaping, _Escaping, PanelEventArgs);
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
	FarPluginPanel();
	void AssertOpen();
	void SwitchFullScreen();
	virtual FarFile^ GetFile(int index, FileType type) override;
internal:
	bool _IsPushed;
	bool _skipGettingData;
	bool _voidGettingData;
	FarPluginPanelInfo _info;
	Object^ _postData;
	FarFile^ _postFile;
	String^ _postName;
private:
	Guid _TypeId;
	IList<FarFile^>^ _files;
	String^ _StartDirectory;
};

const int cPanels = 4;
ref class PanelSet
{
internal:
	static property FarPluginPanel^ PostedPanel { FarPluginPanel^ get() { return _panels[0]; } }
	static void BeginOpenMode();
	static void EndOpenMode();
	static HANDLE AddPluginPanel(FarPluginPanel^ plugin);
	static int AsDeleteFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int opMode);
	static int AsGetFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, const wchar_t** destPath, int opMode);
	static int AsGetFindData(HANDLE hPlugin, PluginPanelItem** pPanelItem, int* pItemsNumber, int opMode);
	static int AsMakeDirectory(HANDLE hPlugin, const wchar_t** name, int opMode);
	static int AsProcessEvent(HANDLE hPlugin, int id, void* param);
	static int AsProcessKey(HANDLE hPlugin, int key, unsigned int controlState);
	static int AsPutFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, int opMode);
	static int AsSetDirectory(HANDLE hPlugin, const wchar_t* dir, int opMode);
	static FarPanel^ GetPanel(bool active);
	static FarPluginPanel^ GetPluginPanel(Guid id);
	static FarPluginPanel^ GetPluginPanel(Type^ hostType);
	static FarPluginPanel^ GetPluginPanel2(FarPluginPanel^ plugin);
	static void AsClosePlugin(HANDLE hPlugin);
	static void AsFreeFindData(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber);
	static void AsGetOpenPluginInfo(HANDLE hPlugin, OpenPluginInfo* info);
	static void OpenPluginPanel(FarPluginPanel^ plugin);
	static void PushPluginPanel(FarPluginPanel^ plugin);
	static void ReplacePluginPanel(FarPluginPanel^ oldPanel, FarPluginPanel^ newPanel);
internal:
	static List<FarPluginPanel^> _stack;
private:
	PanelSet() {}
private:
	// Posted [0] and opened [1..3] panels; i.e. size is 4, see AddPluginPanel().
	static array<FarPluginPanel^>^ _panels = gcnew array<FarPluginPanel^>(cPanels);
	static bool _inAsSetDirectory;
	static int _openMode;
};

}
