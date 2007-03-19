#include "StdAfx.h"
#include "FarPanel.h"

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
public ref class StoredItem : IFile
{
public:
	virtual property bool IsAlias;
	virtual property bool IsArchive;
	virtual property bool IsCompressed;
	virtual property bool IsDirectory;
	virtual property bool IsEncrypted;
	virtual property bool IsFolder;
	virtual property bool IsHidden;
	virtual property bool IsReadOnly;
	virtual property bool IsSelected;
	virtual property bool IsSystem;
	virtual property bool IsVolume;
	virtual property DateTime CreationTime;
	virtual property DateTime LastAccessTime;
	virtual property IFolder^ Parent;
	virtual property Int64 Size;
	virtual property String^ AlternateName;
	virtual property String^ Description;
	virtual property String^ Name;
	virtual property String^ Owner;
	virtual property String^ Path;
	virtual String^ ToString() override
	{
		return Path;
	}
};

public ref class StoredFile : public StoredItem, IFile
{
public:
	StoredFile()
	{
		IsDirectory = false;
		IsFolder = false;
	}
};

public ref class StoredFolder : public StoredItem, IFolder
{
	List<IFile^>^ _files;
public:
	StoredFolder()
	{
		IsDirectory = true;
		IsFolder = true;
		_files = gcnew List<IFile^>();
	}
	virtual property IList<IFile^>^ Files
	{
		IList<IFile^>^ get() { return _files; }
	}
};

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
	for each(IFile^ i in _contents->Files)
	{
		StoredItem^ f = safe_cast<StoredItem^>(i);
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
	{
		f->IsSelected = true;
		_selected->Add(f);
	}
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
