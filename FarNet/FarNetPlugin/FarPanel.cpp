#include "StdAfx.h"
#include "FarPanel.h"
#include "Utils.h"

DateTime ft2dt(FILETIME time)
{
	__int64* p = (__int64* )&time;
	return DateTime::FromFileTime(*p);
}

bool at(unsigned int a, int f)
{
	return(a & f) != 0;
}

namespace FarManagerImpl
{;
FarPanel::FarPanel(bool current)
{
	_isCurrentPanel = current;
	_contents = gcnew StoredFolder();
	_selected = gcnew List<IFile^>();
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

IFile^ FarPanel::Current::get()
{
	RefreshContents();
	return _current;
}

IFile^ FarPanel::Top::get()
{
	RefreshContents();
	return _top;
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
	CStr sValue(value);
	if (!Info.Control(INVALID_HANDLE_VALUE, command, sValue))
		throw gcnew OperationCanceledException();
}

String^ FarPanel::ToString()
{
	return Path;
}

IFolder^ FarPanel::Contents::get()
{
	RefreshContents();
	return _contents;
}

IList<IFile^>^ FarPanel::Selected::get()
{
	RefreshContents();
	return _selected;
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

void FarPanel::RefreshContents()
{
	PanelInfo pi; GetInfo(pi);
	_contents->Path = OemToStr(pi.CurDir);
	ClearContents();
	_selected->Clear();
	PluginPanelItem* itm = pi.PanelItems;
	for(int i = 0; i < pi.ItemsNumber; ++i)
	{
		StoredItem^ f = ItemToFile(itm);
		f->Parent = _contents;
		_contents->Files->Add(f);
		itm++;
	}
	if (_contents->Files->Count != 0)
	{
		_current = safe_cast<StoredItem^>(_contents->Files[pi.CurrentItem]);
		_top = safe_cast<StoredItem^>(_contents->Files[pi.TopPanelItem]);
	}
}

void FarPanel::ClearContents()
{
	for(int i = 0; i < _contents->Files->Count; ++i)
	{
		StoredItem^ f = safe_cast<StoredItem^>(_contents->Files[i]);
		f->Parent = nullptr;
	}
	_contents->Files->Clear();
}

StoredItem^ FarPanel::ItemToFile(PluginPanelItem* i)
{
	String^ name = OemToStr(i->FindData.cFileName);
	unsigned int a = i->FindData.dwFileAttributes;
	bool isFolder = at(a, FILE_ATTRIBUTE_DIRECTORY);
	StoredItem^ f;
	if (isFolder)
		f = gcnew StoredFolder();
	else
		f = gcnew StoredFile();
	f->Name = name;
	f->AlternateName = gcnew String(i->FindData.cAlternateFileName);
	f->CreationTime = ft2dt(i->FindData.ftCreationTime);
	f->LastAccessTime = ft2dt(i->FindData.ftLastAccessTime);
	f->Size = i->FindData.nFileSizeLow;
	f->IsReadOnly = at(a, FILE_ATTRIBUTE_READONLY);
	f->IsHidden = at(a, FILE_ATTRIBUTE_READONLY);
	f->IsVolume = false;
	f->IsSystem = at(a, FILE_ATTRIBUTE_SYSTEM);
	f->IsDirectory = at(a, FILE_ATTRIBUTE_DIRECTORY);
	f->IsArchive = at(a, FILE_ATTRIBUTE_ARCHIVE);
	f->IsAlias = at(a, FILE_ATTRIBUTE_REPARSE_POINT);
	f->IsCompressed = at(a, FILE_ATTRIBUTE_COMPRESSED);
	f->IsEncrypted = at(a, FILE_ATTRIBUTE_ENCRYPTED);
	f->Description = i->Description ? OemToStr(i->Description) : String::Empty; 
	f->Path = System::IO::Path::Combine(_contents->Path, f->Name);
	if (at(i->Flags, PPIF_SELECTED))
		_selected->Add(f);
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

void FarPanel::Update(bool keepSelection)
{
	int command = _isCurrentPanel ? FCTL_UPDATEPANEL : FCTL_UPDATEANOTHERPANEL;
	Info.Control(INVALID_HANDLE_VALUE, command, (void*)keepSelection);
}
}
