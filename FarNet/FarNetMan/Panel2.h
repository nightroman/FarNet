
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

#pragma once
#include "Panel1.h"

#define FPPI_FLAG(Type, Name, Override)\
public: virtual property Type Name {\
	Type get() Override { return _##Name; }\
	void set(Type value) Override { _##Name = value; if (m) { m->Flags = Flags(); } }\
}\
private: Type _##Name

#define FPPI_PROP(Type, Name, Set)\
public: virtual property Type Name {\
	Type get() { return _##Name; }\
	void set(Type value) { _##Name = value; if (m) { Set; } }\
}\
private: Type _##Name

#define FPPI_TEXT(Name, Data)\
public: virtual property String^ Name {\
	String^ get() { return _##Name; }\
	void set(String^ value) { _##Name = value; if (m) { delete[] m->Data; m->Data = NewChars(value); } }\
}\
private: String^ _##Name

namespace FarNet
{;
ref class ShelveInfoNative;
ref class ShelveInfoModule;

ref class Panel2 : public Panel1, Works::IPanelWorks
{
internal:
	void Free();
	OpenPanelInfo& Make();
public: // IPanel
	virtual property FarFile^ CurrentFile { FarFile^ get() override; }
	virtual property PanelSortMode SortMode { PanelSortMode get() override; void set(PanelSortMode value) override; }
	virtual property PanelViewMode ViewMode { PanelViewMode get() override; void set(PanelViewMode value) override; }
	virtual property String^ StartDirectory { String^ get(); }
	virtual array<FarFile^>^ GetFiles() override;
	virtual array<FarFile^>^ GetSelectedFiles() override;
	virtual void Close() override;
	virtual void Push() override;
public: // IPanelWorks
	FPPI_FLAG(bool, CompareFatTime,);
	FPPI_FLAG(bool, NoFilter,);
	FPPI_FLAG(bool, PreserveCase,);
	FPPI_FLAG(bool, RawSelection,);
	FPPI_FLAG(bool, RealNames, override); //_230529_0854
	FPPI_FLAG(bool, RealNamesDeleteFiles,);
	FPPI_FLAG(bool, RealNamesExportFiles,);
	FPPI_FLAG(bool, RealNamesImportFiles,);
	FPPI_FLAG(bool, RealNamesMakeDirectory,);
	FPPI_FLAG(bool, RightAligned,);
	FPPI_FLAG(bool, ShowNamesOnly,);
	FPPI_FLAG(bool, UseSortGroups, override); //_230529_0854
	FPPI_FLAG(PanelHighlighting, Highlighting,);
	FPPI_PROP(PanelViewMode, StartViewMode, m->StartPanelMode = int(_StartViewMode) + 0x30);
	FPPI_TEXT(CurrentLocation, CurDir);
	FPPI_TEXT(FormatName, Format);
	FPPI_TEXT(HostFile, HostFile);
	FPPI_TEXT(Title, PanelTitle);
public:
	virtual property array<DataItem^>^ InfoItems { array<DataItem^>^ get() { return _InfoItems; } void set(array<DataItem^>^ value); }
	virtual property bool IsOpened { bool get() { return Index > 0; } }
	virtual property bool IsPushed { bool get() { return _Pushed != nullptr; } }
	virtual property Explorer^ MyExplorer { Explorer^ get() { return _MyExplorer; } }
	virtual property Panel^ TargetPanel { Panel^ get(); }
	virtual PanelPlan^ GetPlan(PanelViewMode mode);
	virtual void Navigate(Explorer^ explorer);
	virtual void Open();
	virtual void OpenReplace(Panel^ current);
	virtual void PostData(Object^ data);
	virtual void PostFile(FarFile^ file);
	virtual void PostName(String^ name);
	virtual void SetKeyBars(array<KeyBar^>^ bars);
	virtual void SetPlan(PanelViewMode mode, PanelPlan^ plan);
internal:
	Panel2(Panel^ panel, Explorer^ explorer);
	property bool HasDots { bool get(); }
	property PanelSortMode StartSortMode { PanelSortMode get(); void set(PanelSortMode value); }
	void AssertOpen();
	virtual FarFile^ GetFile(int index, FileType type) override;
	List<FarFile^>^ ItemsToFiles(IList<String^>^ names, PluginPanelItem* panelItem, int itemsNumber);
	int AsGetFindData(GetFindDataInfo* info);
	int AsSetDirectory(const SetDirectoryInfo* info);
internal:
	Panel^ const Host;
	ShelveInfoModule^ _Pushed;
	bool _skipUpdateFiles;
	bool _voidUpdateFiles;
	Object^ _postData;
	FarFile^ _postFile;
	String^ _postName;
	array<int>^ _postSelected;
	ShelveInfoNative^ _ActiveInfo;
private:
	int Flags();
	void CreateInfoLines();
	void CreateModes();
	void DeleteInfoLines();
	void DeleteModes();
	void CreateKeyBars(KeyBarTitles& m);
	static void DeleteKeyBars(const KeyBarTitles& m);
	FarFile^ GetItemFile(const PluginPanelItem& panelItem);
	FarFile^ GetFileByUserData(void* data);
	void OpenExplorer(Explorer^ explorer, ExploreEventArgs^ args);
	void ReplaceExplorer(Explorer^ explorer);
	void OnTimerJob();
internal:
	void OnTimer(Object^ state);
private:
	IList<FarFile^>^ _Files_;
	Explorer^ _MyExplorer;
	OpenPanelInfo* m;
	bool _FarStartSortOrder;
	int _FarStartSortMode;
	array<DataItem^>^ _InfoItems;
	array<PanelPlan^>^ _Plans;
	array<KeyBar^>^ _keyBars;
internal:
	Timer^ _timerInstance;
};
}
