/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#include "StdAfx.h"
#include "FarPanel.h"

namespace FarManagerImpl
{;
FarPanel::FarPanel(bool current)
: _isCurrentPanel(current)
{
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

	StoredFile^ r = ItemToFile(pi.PanelItems[pi.CurrentItem]);
	return r;
}

int FarPanel::CurrentIndex::get()
{
	PanelInfo pi; GetBrief(pi);
	return pi.ItemsNumber ? pi.CurrentItem : -1;
}

IFile^ FarPanel::Top::get()
{
	PanelInfo pi; GetInfo(pi);
	if (pi.ItemsNumber == 0)
		return nullptr;

	StoredFile^ r = ItemToFile(pi.PanelItems[pi.TopPanelItem]);
	return r;
}

int FarPanel::TopIndex::get()
{
	PanelInfo pi; GetBrief(pi);
	return pi.ItemsNumber ? pi.TopPanelItem : -1;
}

PanelSortMode FarPanel::SortMode::get()
{
	PanelInfo pi; GetBrief(pi);
	return (PanelSortMode)pi.SortMode;
}

PanelViewMode FarPanel::ViewMode::get()
{
	PanelInfo pi; GetBrief(pi);
	return (PanelViewMode)pi.ViewMode;
}

String^ FarPanel::Path::get()
{
	PanelInfo pi; GetBrief(pi);
	return OemToStr(pi.CurDir);
}

void FarPanel::Path::set(String^ value)
{
	int command = _isCurrentPanel ? FCTL_SETPANELDIR : FCTL_SETANOTHERPANELDIR;
	CStr sb(value);
	if (!Info.Control(INVALID_HANDLE_VALUE, command, sb))
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

PanelType FarPanel::Type::get()
{
	PanelInfo pi; GetBrief(pi);
	return (PanelType)pi.PanelType;
}

void FarPanel::GetBrief(PanelInfo& pi)
{
	int command = _isCurrentPanel ? FCTL_GETPANELSHORTINFO : FCTL_GETANOTHERPANELSHORTINFO;
	if (!Info.Control(INVALID_HANDLE_VALUE, command, &pi))
		throw gcnew OperationCanceledException("Can't get panel information.");
}

void FarPanel::GetInfo(PanelInfo& pi)
{
	int command = _isCurrentPanel ? FCTL_GETPANELINFO : FCTL_GETANOTHERPANELINFO;
	if (!Info.Control(INVALID_HANDLE_VALUE, command, &pi))
		throw gcnew OperationCanceledException("Can't get panel information.");
}

StoredFile^ FarPanel::ItemToFile(PluginPanelItem& i)
{
	StoredFile^ f = gcnew StoredFile();

	f->Name = OemToStr(i.FindData.cFileName);
	f->Description = i.Description ? OemToStr(i.Description) : String::Empty; 
	f->AlternateName = gcnew String(i.FindData.cAlternateFileName);

	f->_flags = i.FindData.dwFileAttributes;
	f->CreationTime = ft2dt(i.FindData.ftCreationTime);
	f->LastAccessTime = ft2dt(i.FindData.ftLastAccessTime);
	f->LastWriteTime = ft2dt(i.FindData.ftLastWriteTime);
	f->Size = i.FindData.nFileSizeLow;
	f->IsSelected = (i.Flags & PPIF_SELECTED) != 0;
	f->Tag = i.UserData;

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

bool FarPanel::RealNames::get()
{
	PanelInfo pi; GetBrief(pi);
	return (pi.Flags & PFLAGS_REALNAMES) != 0;
}

void FarPanel::Redraw()
{
	int command = _isCurrentPanel ? FCTL_REDRAWPANEL : FCTL_REDRAWANOTHERPANEL;
	Info.Control(INVALID_HANDLE_VALUE, command, 0);
}

void FarPanel::Redraw(int current, int top)
{
	PanelRedrawInfo pri;
	pri.CurrentItem = current;
	pri.TopPanelItem = top;
	int command = _isCurrentPanel ? FCTL_REDRAWPANEL : FCTL_REDRAWANOTHERPANEL;
	Info.Control(INVALID_HANDLE_VALUE, command, &pri);
}

void FarPanel::Update(bool keepSelection)
{
	int command = _isCurrentPanel ? FCTL_UPDATEPANEL : FCTL_UPDATEANOTHERPANEL;
	Info.Control(INVALID_HANDLE_VALUE, command, (void*)keepSelection);
}

}
