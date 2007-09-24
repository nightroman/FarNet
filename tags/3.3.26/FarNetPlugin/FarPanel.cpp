/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#include "StdAfx.h"
#include "FarPanel.h"
#include "FarImpl.h"

namespace FarManagerImpl
{;

//
//::FarPanel::
//

FarPanel::FarPanel(bool current)
: _id(INVALID_HANDLE_VALUE)
, _active(current)
{
}

int FarPanel::Id::get()
{
	return (int)(INT_PTR)_id;
}

void FarPanel::Id::set(int value)
{
	_id = (HANDLE)(INT_PTR)value;
}

bool FarPanel::IsActive::get()
{
	PanelInfo pi; GetBrief(pi);
	return pi.Focus != 0;
}

bool FarPanel::IsPlugin::get()
{
	PanelInfo pi; GetBrief(pi);
	return pi.Plugin != 0;
}

bool FarPanel::IsVisible::get()
{
	PanelInfo pi; GetBrief(pi);
	return pi.Visible != 0;
}

void FarPanel::IsVisible::set(bool value)
{
	PanelInfo pi; GetBrief(pi);
	bool old = pi.Visible != 0;
	if (old == value)
		return;

	DWORD key = Info.FSF->FarNameToKey(pi.PanelRect.left == 0 ? "CtrlF1" : "CtrlF2");
	KeySequence ks;
	ks.Count = 1;
	ks.Flags = 0;
	ks.Sequence = &key;
	Info.AdvControl(Info.ModuleNumber, ACTL_POSTKEYSEQUENCE, &ks);
}

IFile^ FarPanel::Current::get()
{
	PanelInfo pi; GetInfo(pi);
	if (pi.ItemsNumber == 0)
		return nullptr;

	FarFile^ r = ItemToFile(pi.PanelItems[pi.CurrentItem]);
	return r;
}

int FarPanel::CurrentIndex::get()
{
	PanelInfo pi; GetBrief(pi);
	return pi.ItemsNumber ? pi.CurrentItem : -1;
}

int FarPanel::TopIndex::get()
{
	PanelInfo pi; GetBrief(pi);
	return pi.ItemsNumber ? pi.TopPanelItem : -1;
}

Place FarPanel::Window::get()
{
	PanelInfo pi; GetBrief(pi);
	Place r;
	r.Left = pi.PanelRect.left; r.Top = pi.PanelRect.top;
	r.Right = pi.PanelRect.right; r.Bottom = pi.PanelRect.bottom;
	return r;
}

Point FarPanel::Frame::get()
{
	PanelInfo pi; GetBrief(pi);
	return pi.ItemsNumber ? Point(pi.CurrentItem, pi.TopPanelItem) : Point(-1, -1);
}

PanelSortMode FarPanel::SortMode::get()
{
	PanelInfo pi; GetBrief(pi);
	return (PanelSortMode)pi.SortMode;
}

void FarPanel::SortMode::set(PanelSortMode value)
{
	int command = _active ? FCTL_SETSORTMODE : FCTL_SETANOTHERSORTMODE;
	int mode = (int)value;
	Info.Control(_id, command, &mode);
}

PanelViewMode FarPanel::ViewMode::get()
{
	PanelInfo pi; GetBrief(pi);
	return (PanelViewMode)pi.ViewMode;
}

void FarPanel::ViewMode::set(PanelViewMode value)
{
	int command = _active ? FCTL_SETVIEWMODE : FCTL_SETANOTHERVIEWMODE;
	int mode = (int)value;
	Info.Control(_id, command, &mode);
}

String^ FarPanel::Path::get()
{
	PanelInfo pi; GetBrief(pi);
	return OemToStr(pi.CurDir);
}

void FarPanel::Path::set(String^ value)
{
	int command = _active ? FCTL_SETPANELDIR : FCTL_SETANOTHERPANELDIR;
	CStr sb(value);
	if (!Info.Control(_id, command, sb))
		throw gcnew OperationCanceledException();
}

String^ FarPanel::ToString()
{
	return Path;
}

IList<IFile^>^ FarPanel::Contents::get()
{
	List<IFile^>^ r = gcnew List<IFile^>();
	PanelInfo pi; GetInfo(pi);
	for(int i = 0; i < pi.ItemsNumber; ++i)
		r->Add(ItemToFile(pi.PanelItems[i]));
	return r;
}

IList<IFile^>^ FarPanel::Selected::get()
{
	List<IFile^>^ r = gcnew List<IFile^>();
	PanelInfo pi; GetInfo(pi);
	for(int i = 0; i < pi.ItemsNumber; ++i)
	{
		if (pi.PanelItems[i].Flags & PPIF_SELECTED)
			r->Add(ItemToFile(pi.PanelItems[i]));
	}
	return r;
}

IList<IFile^>^ FarPanel::Targeted::get()
{
	List<IFile^>^ r = gcnew List<IFile^>();
	PanelInfo pi; GetInfo(pi);
	for(int i = 0; i < pi.ItemsNumber; ++i)
	{
		if (pi.PanelItems[i].Flags & PPIF_SELECTED)
			r->Add(ItemToFile(pi.PanelItems[i]));
	}
	if (r->Count == 0)
	{
		if (pi.ItemsNumber > 0)
		{
			FarFile^ f = ItemToFile(pi.PanelItems[pi.CurrentItem]);
			if (f->Name != "..")
				r->Add(f);
		}
	}
	return r;
}

PanelType FarPanel::Type::get()
{
	PanelInfo pi; GetBrief(pi);
	return (PanelType)pi.PanelType;
}

void FarPanel::GetBrief(PanelInfo& pi)
{
	int command = _active ? FCTL_GETPANELSHORTINFO : FCTL_GETANOTHERPANELSHORTINFO;
	if (!Info.Control(_id, command, &pi))
		throw gcnew OperationCanceledException("Can't get panel information.");
}

void FarPanel::GetInfo(PanelInfo& pi)
{
	int command = _active ? FCTL_GETPANELINFO : FCTL_GETANOTHERPANELINFO;
	if (!Info.Control(_id, command, &pi))
		throw gcnew OperationCanceledException("Can't get panel information.");
}

FarFile^ FarPanel::ItemToFile(PluginPanelItem& item)
{
	FarFile^ f = gcnew FarFile();

	f->Name = OemToStr(item.FindData.cFileName);
	f->Description = item.Description ? OemToStr(item.Description) : String::Empty; 
	f->AlternateName = gcnew String(item.FindData.cAlternateFileName);

	f->_flags = item.FindData.dwFileAttributes;
	f->CreationTime = ft2dt(item.FindData.ftCreationTime);
	f->LastAccessTime = ft2dt(item.FindData.ftLastAccessTime);
	f->LastWriteTime = ft2dt(item.FindData.ftLastWriteTime);
	f->Length = item.FindData.nFileSizeLow;
	f->IsSelected = (item.Flags & PPIF_SELECTED) != 0;

	return f;
}

bool FarPanel::ShowHidden::get()
{
	PanelInfo pi; GetBrief(pi);
	return (pi.Flags & PFLAGS_SHOWHIDDEN) != 0;
}

bool FarPanel::Highlight::get()
{
	PanelInfo pi; GetBrief(pi);
	return (pi.Flags & PFLAGS_HIGHLIGHT) != 0;
}

bool FarPanel::ReverseSortOrder::get()
{
	PanelInfo pi; GetBrief(pi);
	return (pi.Flags & PFLAGS_REVERSESORTORDER) != 0;
}

void FarPanel::ReverseSortOrder::set(bool value)
{
	int command = _active ? FCTL_SETSORTORDER : FCTL_SETANOTHERSORTORDER;
	int mode = (int)value;
	Info.Control(_id, command, &mode);
}

bool FarPanel::UseSortGroups::get()
{
	PanelInfo pi; GetBrief(pi);
	return (pi.Flags & PFLAGS_USESORTGROUPS) != 0;
}

bool FarPanel::SelectedFirst::get()
{
	PanelInfo pi; GetBrief(pi);
	return (pi.Flags & PFLAGS_SELECTEDFIRST) != 0;
}

bool FarPanel::NumericSort::get()
{
	PanelInfo pi; GetBrief(pi);
	return (pi.Flags & PFLAGS_NUMERICSORT) != 0;
}

void FarPanel::NumericSort::set(bool value)
{
	int command = _active ? FCTL_SETNUMERICSORT : FCTL_SETANOTHERNUMERICSORT;
	int mode = (int)value;
	Info.Control(_id, command, &mode);
}

bool FarPanel::RealNames::get()
{
	PanelInfo pi; GetBrief(pi);
	return (pi.Flags & PFLAGS_REALNAMES) != 0;
}

void FarPanel::Close()
{
	Info.Control(_id, FCTL_CLOSEPLUGIN, 0);
}

void FarPanel::Close(String^ path)
{
	CStr sb;
	if (!String::IsNullOrEmpty(path))
		sb.Set(path);

	Info.Control(_id, FCTL_CLOSEPLUGIN, sb);
}

void FarPanel::Redraw()
{
	int command = _active ? FCTL_REDRAWPANEL : FCTL_REDRAWANOTHERPANEL;
	Info.Control(_id, command, 0);
}

void FarPanel::Redraw(int current, int top)
{
	//! do it, else result is different
	if (current < 0 && top < 0)
	{
		Redraw();
		return;
	}

	PanelRedrawInfo pri;
	pri.CurrentItem = current;
	pri.TopPanelItem = top;
	int command = _active ? FCTL_REDRAWPANEL : FCTL_REDRAWANOTHERPANEL;
	Info.Control(_id, command, &pri);
}

void FarPanel::Update(bool keepSelection)
{
	int command = _active ? FCTL_UPDATEPANEL : FCTL_UPDATEANOTHERPANEL;
	Info.Control(_id, command, (void*)keepSelection);
}

//
//::FarPanelPluginInfo::
//

// Flags
#define DEF_FLAG(Prop, Flag)\
bool FarPanelPluginInfo::Prop::get() { return (m->Flags & (Flag)) != 0; }\
void FarPanelPluginInfo::Prop::set(bool value) { { if (value) m->Flags |= Flag; else m->Flags &= ~Flag; } }
DEF_FLAG(AddDots, OPIF_ADDDOTS);
DEF_FLAG(CompareFatTime, OPIF_COMPAREFATTIME);
DEF_FLAG(ExternalDelete, OPIF_EXTERNALDELETE);
DEF_FLAG(ExternalGet, OPIF_EXTERNALGET);
DEF_FLAG(ExternalMakeDirectory, OPIF_EXTERNALMKDIR);
DEF_FLAG(ExternalPut, OPIF_EXTERNALPUT);
DEF_FLAG(PreserveCase, OPIF_SHOWPRESERVECASE);
DEF_FLAG(RawSelection, OPIF_RAWSELECTION);
DEF_FLAG(RealNames, OPIF_REALNAMES);
DEF_FLAG(RightAligned, OPIF_SHOWRIGHTALIGNNAMES);
DEF_FLAG(ShowNamesOnly, OPIF_SHOWNAMESONLY);
DEF_FLAG(UseAttrHighlighting, OPIF_USEATTRHIGHLIGHTING);
DEF_FLAG(UseFilter, OPIF_USEFILTER);
DEF_FLAG(UseHighlighting, OPIF_USEHIGHLIGHTING);
DEF_FLAG(UseSortGroups, OPIF_USESORTGROUPS);

// Strings
#define DEF_STRING(Prop, Field)\
String^ FarPanelPluginInfo::Prop::get()\
{\
	return OemToStr(m->Field);\
}\
void FarPanelPluginInfo::Prop::set(String^ value)\
{\
	if (m->Field)\
		delete m->Field;\
	if (String::IsNullOrEmpty(value))\
	{\
		m->Field = NULL;\
		return;\
	}\
	m->Field = new char[value->Length + 1];\
	StrToOem(value, (char*)m->Field);\
}
DEF_STRING(CurrentDirectory, CurDir);
DEF_STRING(Format, Format);
DEF_STRING(HostFile, HostFile);
DEF_STRING(Title, PanelTitle);

//
//::FarPanelPlugin::
//

FarPanelPlugin::FarPanelPlugin() : FarPanel(true)
{
	_StartDirectory = Environment::CurrentDirectory;
}

void FarPanelPlugin::AssertOpen()
{
	if (Id <= 0) throw gcnew InvalidOperationException("Panel plugin is not opened.");
}

bool FarPanelPlugin::IsOpened::get()
{
	return Id > 0;
}

IList<IFile^>^ FarPanelPlugin::Files::get()
{
	return %_files;
}

bool FarPanelPlugin::IsPlugin::get()
{
	return true;
}

IFile^ FarPanelPlugin::Current::get()
{
	AssertOpen();
	PanelInfo pi; GetInfo(pi);
	if (pi.ItemsNumber == 0 || _info.AddDots && pi.CurrentItem == 0)
		return nullptr;
	return _files[(int)(INT_PTR)pi.PanelItems[pi.CurrentItem].UserData];
}

IList<IFile^>^ FarPanelPlugin::Contents::get()
{
	AssertOpen();
	List<IFile^>^ r = gcnew List<IFile^>();
	PanelInfo pi; GetInfo(pi);
	for(int i = (_info.AddDots ? 1 : 0); i < pi.ItemsNumber; ++i)
		r->Add(_files[(int)(INT_PTR)pi.PanelItems[i].UserData]);
	return r;
}

IList<IFile^>^ FarPanelPlugin::Selected::get()
{
	AssertOpen();
	List<IFile^>^ r = gcnew List<IFile^>();
	PanelInfo pi; GetInfo(pi);
	for(int i = (_info.AddDots ? 1 : 0); i < pi.ItemsNumber; ++i)
	{
		if (pi.PanelItems[i].Flags & PPIF_SELECTED)
			r->Add(_files[(int)(INT_PTR)pi.PanelItems[i].UserData]);
	}
	return r;
}

IList<IFile^>^ FarPanelPlugin::Targeted::get()
{
	AssertOpen();
	List<IFile^>^ r = gcnew List<IFile^>();
	PanelInfo pi; GetInfo(pi);
	for(int i = 0; i < pi.ItemsNumber; ++i)
	{
		if (pi.PanelItems[i].Flags & PPIF_SELECTED)
			r->Add(_files[(int)(INT_PTR)pi.PanelItems[i].UserData]);
	}
	if (r->Count == 0)
	{
		if (pi.ItemsNumber > 0)
		{
			int j = (int)(INT_PTR)pi.PanelItems[pi.CurrentItem].UserData;
			if (j >= 0 && _files[j]->Name != "..")
				r->Add(_files[j]);
		}
	}
	return r;
}

String^ FarPanelPlugin::Path::get()
{
	return _info.CurrentDirectory;
}

void FarPanelPlugin::Path::set(String^ value)
{
	if (!_SettingDirectory)
		throw gcnew NotSupportedException("Plugin panel does not support setting a new path.");
	SettingDirectoryEventArgs e(value, OperationModes::None);
	_SettingDirectory(this, %e);
	if (!e.Ignore)
	{
		Update(false);
		Redraw();
	}
}

String^ FarPanelPlugin::StartDirectory::get()
{
	return _StartDirectory;
}

void FarPanelPlugin::StartDirectory::set(String^ value)
{
	_StartDirectory = value;
}

IPanelPlugin^ FarPanelPlugin::Another::get()
{
	return GetFar()->GetAnotherPanelPlugin(this);
}

void FarPanelPlugin::Open()
{
	if (Id > 0)
		throw gcnew InvalidOperationException("Panel plugin can not be opened because it is already opened.");
	GetFar()->OpenPanelPlugin(this);
}

void FarPanelPlugin::Open(IPanelPlugin^ oldPanel)
{
	if (!oldPanel)
		throw gcnew ArgumentNullException("oldPanel");
	GetFar()->ReplacePanelPlugin((FarPanelPlugin^)oldPanel, this);
}

}
