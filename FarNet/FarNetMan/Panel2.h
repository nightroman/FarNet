
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once
#include "Panel1.h"

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

namespace FarNet
{;
ref class ShelveInfoNative;
ref class ShelveInfoModule;

ref class Panel2 : public Panel1, Works::IPanelWorks
{
internal:
	void Free();
	OpenPluginInfo& Make();
public: // IPanel
	virtual property bool RealNames { bool get() override; void set(bool value) override; }
	virtual property bool UseSortGroups { bool get() override; void set(bool value) override; }
	virtual property FarFile^ CurrentFile { FarFile^ get() override; }
	virtual property IList<FarFile^>^ SelectedFiles { IList<FarFile^>^ get() override; }
	virtual property IList<FarFile^>^ ShownFiles { IList<FarFile^>^ get() override; }
	virtual property PanelSortMode SortMode { PanelSortMode get() override; void set(PanelSortMode value) override; }
	virtual property PanelViewMode ViewMode { PanelViewMode get() override; void set(PanelViewMode value) override; }
	virtual property String^ StartDirectory { String^ get(); }
	virtual void Close() override;
	virtual void Push() override;
public: // IPanelWorks
	FPPI_FLAG(CompareFatTime);
	FPPI_FLAG(PreserveCase);
	FPPI_FLAG(RawSelection);
	FPPI_FLAG(RealNamesDeleteFiles);
	FPPI_FLAG(RealNamesExportFiles);
	FPPI_FLAG(RealNamesImportFiles);
	FPPI_FLAG(RealNamesMakeDirectory);
	FPPI_FLAG(RightAligned);
	FPPI_FLAG(ShowNamesOnly);
	FPPI_FLAG(UseFilter);
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
	virtual property IList<FarFile^>^ Files { IList<FarFile^>^ get(); void set(IList<FarFile^>^ value); }
	virtual property int WorksId { int get() { return Index; } }
	virtual property Panel^ TargetPanel { Panel^ get(); }
	virtual property PanelHighlighting Highlighting { PanelHighlighting get(); void set(PanelHighlighting value); }
	virtual PanelPlan^ GetPlan(PanelViewMode mode);
	virtual void Open();
	virtual void OpenReplace(Panel^ current);
	virtual void PostData(Object^ data);
	virtual void PostFile(FarFile^ file);
	virtual void PostName(String^ name);
	virtual void SetKeyBar(array<String^>^ labels);
	virtual void SetKeyBarAlt(array<String^>^ labels);
	virtual void SetKeyBarAltShift(array<String^>^ labels);
	virtual void SetKeyBarCtrl(array<String^>^ labels);
	virtual void SetKeyBarCtrlAlt(array<String^>^ labels);
	virtual void SetKeyBarCtrlShift(array<String^>^ labels);
	virtual void SetKeyBarShift(array<String^>^ labels);
	virtual void SetPlan(PanelViewMode mode, PanelPlan^ plan);
internal:
	Panel2(Panel^ panel, Explorer^ explorer);
	property bool HasDots { bool get(); }
	property PanelSortMode StartSortMode { PanelSortMode get(); void set(PanelSortMode value); }
	void AssertOpen();
	void ReplaceExplorer(Explorer^ explorer);
	void SwitchFullScreen();
	virtual FarFile^ GetFile(int index, FileType type) override;
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
	IList<FarFile^>^ _Files;
private:
	int Flags();
	void CreateInfoLines();
	void CreateModes();
	void DeleteInfoLines();
	void DeleteModes();
	static void Free12Strings(wchar_t* const dst[12]);
	static void Make12Strings(wchar_t** dst, array<String^>^ src);
private:
	Explorer^ _MyExplorer;
	OpenPluginInfo* m;
	bool _FarStartSortOrder;
	bool _RealNames;
	bool _UseSortGroups;
	int _FarStartSortMode;
	PanelHighlighting _Highlighting;
	array<DataItem^>^ _InfoItems;
	array<PanelPlan^>^ _Plans;
	array<String^>^ _keyBar;
	array<String^>^ _keyBarAlt;
	array<String^>^ _keyBarAltShift;
	array<String^>^ _keyBarCtrl;
	array<String^>^ _keyBarCtrlAlt;
	array<String^>^ _keyBarCtrlShift;
	array<String^>^ _keyBarShift;
};
}
