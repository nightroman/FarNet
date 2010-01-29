/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
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

ref class FarPanelInfo : IPanelInfo
{
internal:
	FarPanelInfo();
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
	FPPI_FLAG(UseAttributeHighlighting);
	FPPI_FLAG(UseFilter);
	FPPI_FLAG(UseHighlighting);
	FPPI_FLAG(UseSortGroups);
	FPPI_PROP(bool, StartReverseSortOrder, m->StartSortOrder = _StartReverseSortOrder);
	FPPI_PROP(PanelSortMode, StartSortMode, m->StartSortMode = int(_StartSortMode));
	FPPI_PROP(PanelViewMode, StartViewMode, m->StartPanelMode = int(_StartViewMode) + 0x30);
	FPPI_TEXT(CurrentDirectory, CurDir);
	FPPI_TEXT(FormatName, Format);
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
}
