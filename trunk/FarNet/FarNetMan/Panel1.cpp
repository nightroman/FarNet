/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "Panel1.h"
#include "Panel0.h"
#include "PanelFileCollection.h"

namespace FarNet
{;
Panel1::Panel1(bool current)
: _handle(current ? PANEL_ACTIVE : PANEL_PASSIVE)
{}

HANDLE Panel1::Handle::get()
{
	return _handle;
}

void Panel1::Handle::set(HANDLE value)
{
	_handle = value;
}

bool Panel1::IsActive::get()
{
	PanelInfo pi;
	if (!TryPanelInfo(_handle, pi))
		return false;

	return pi.Focus != 0;
}

bool Panel1::IsLeft::get()
{
	PanelInfo pi;
	if (!TryPanelInfo(_handle, pi))
		return false;

	return (pi.Flags & PFLAGS_PANELLEFT) != 0;
}

bool Panel1::IsPlugin::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return pi.Plugin != 0;
}

bool Panel1::IsVisible::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);
	return pi.Visible != 0;
}

void Panel1::IsVisible::set(bool value)
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	bool old = pi.Visible != 0;
	if (old == value)
		return;

	DWORD key = pi.PanelRect.left == 0 ? (KeyMode::Ctrl | KeyCode::F1) : (KeyMode::Ctrl | KeyCode::F2);
	KeySequence ks;
	ks.Count = 1;
	ks.Flags = 0;
	ks.Sequence = &key;
	Info.AdvControl(Info.ModuleNumber, ACTL_POSTKEYSEQUENCE, &ks);
}

//! It is possible to ask the current file directly, but implementation is not safe
FarFile^ Panel1::CurrentFile::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	if (pi.ItemsNumber == 0)
		return nullptr;

	AutoPluginPanelItem item(_handle, pi.CurrentItem, ShownFile);

	return ItemToFile(item.Get());
}

int Panel1::CurrentIndex::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return pi.ItemsNumber ? pi.CurrentItem : -1;
}

int Panel1::TopIndex::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return pi.ItemsNumber ? pi.TopPanelItem : -1;
}

Place Panel1::Window::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	Place r;
	r.Left = pi.PanelRect.left; r.Top = pi.PanelRect.top;
	r.Right = pi.PanelRect.right; r.Bottom = pi.PanelRect.bottom;
	return r;
}

Point Panel1::Frame::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return pi.ItemsNumber ? Point(pi.CurrentItem, pi.TopPanelItem) : Point(-1, -1);
}

PanelSortMode Panel1::SortMode::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (PanelSortMode)pi.SortMode;
}

void Panel1::SortMode::set(PanelSortMode value)
{
	Info.Control(_handle, FCTL_SETSORTMODE, (int)value, NULL);
}

PanelViewMode Panel1::ViewMode::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (PanelViewMode)pi.ViewMode;
}

void Panel1::ViewMode::set(PanelViewMode value)
{
	Info.Control(_handle, FCTL_SETVIEWMODE, (int)value, NULL);
}

String^ Panel1::Path::get()
{
	int size = Info.Control(_handle, FCTL_GETPANELDIR, 0, NULL);
	CBox buf(size);
	Info.Control(_handle, FCTL_GETPANELDIR, size, (LONG_PTR)(wchar_t*)buf);
	return gcnew String(buf);
}

// _090929_061740
// Directory::Exists gets false for paths >= 260. But we have to check at least short, because FCTL_SETPANELDIR
// shows unwanted dialog on failure. So, let it works with no breaks at least for normal paths.
// See also Mantis #1087: before Far 2.0.1187 it used to get true always.
void Panel1::Path::set(String^ value)
{
	if (value == nullptr)
		throw gcnew ArgumentNullException("value");

	if (value->Length < 260 && !Directory::Exists(value))
		throw gcnew ArgumentException("Directory '" + value + "' does not exist.");

	PIN_NE(pin, value);
	if (!Info.Control(_handle, FCTL_SETPANELDIR, 0, (LONG_PTR)pin))
		throw gcnew OperationCanceledException("Cannot set panel directory: " + value);
}

String^ Panel1::ToString()
{
	return Path;
}

IList<FarFile^>^ Panel1::ShownFiles::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	List<FarFile^>^ r = gcnew List<FarFile^>(pi.ItemsNumber);
	for(int i = 0; i < pi.ItemsNumber; ++i)
	{
		AutoPluginPanelItem item(_handle, i, ShownFile);
		if (i == 0 && item.Get().FindData.lpwszFileName[0] == '.' && item.Get().FindData.lpwszFileName[1] == '.' && item.Get().FindData.lpwszFileName[2] == '\0')
			continue;
		r->Add(ItemToFile(item.Get()));
	}

	return r;
}

IList<FarFile^>^ Panel1::SelectedFiles::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	List<FarFile^>^ r = gcnew List<FarFile^>(pi.SelectedItemsNumber);
	for(int i = 0; i < pi.SelectedItemsNumber; ++i)
	{
		AutoPluginPanelItem item(_handle, i, SelectedFile);
		r->Add(ItemToFile(item.Get()));
	}

	return r;
}

IList<FarFile^>^ Panel1::ShownList::get()
{
	return gcnew PanelFileCollection(this, ShownFile);
}

IList<FarFile^>^ Panel1::SelectedList::get()
{
	return gcnew PanelFileCollection(this, SelectedFile);
}

FarFile^ Panel1::GetFile(int index, FileType type)
{
	AutoPluginPanelItem item(_handle, index, type);
	return ItemToFile(item.Get());
}

PanelKind Panel1::Kind::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (PanelKind)pi.PanelType;
}

SetFile^ Panel1::ItemToFile(const PluginPanelItem& item)
{
	SetFile^ file = gcnew SetFile;

	file->Name = gcnew String(item.FindData.lpwszFileName);
	file->AlternateName = gcnew String(item.FindData.lpwszAlternateFileName);
	file->Description = gcnew String(item.Description);
	file->Owner = gcnew String(item.Owner);

	file->Attributes = (FileAttributes)item.FindData.dwFileAttributes;
	file->CreationTime = FileTimeToDateTime(item.FindData.ftCreationTime);
	file->LastAccessTime = FileTimeToDateTime(item.FindData.ftLastAccessTime);
	file->LastWriteTime = FileTimeToDateTime(item.FindData.ftLastWriteTime);
	file->Length = item.FindData.nFileSize;

	if (item.CustomColumnNumber)
	{
		array<String^>^ columns = gcnew array<String^>(item.CustomColumnNumber);
		file->Columns = columns;
		for(int i = item.CustomColumnNumber; --i >= 0;)
			columns[i] = gcnew String(item.CustomColumnData[i]);
	}

	return file;
}

bool Panel1::ShowHidden::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (pi.Flags & PFLAGS_SHOWHIDDEN) != 0;
}

bool Panel1::Highlight::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (pi.Flags & PFLAGS_HIGHLIGHT) != 0;
}

bool Panel1::ReverseSortOrder::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (pi.Flags & PFLAGS_REVERSESORTORDER) != 0;
}

void Panel1::ReverseSortOrder::set(bool value)
{
	Info.Control(_handle, FCTL_SETSORTORDER, (int)value, NULL);
}

bool Panel1::UseSortGroups::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (pi.Flags & PFLAGS_USESORTGROUPS) != 0;
}

bool Panel1::SelectedFirst::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (pi.Flags & PFLAGS_SELECTEDFIRST) != 0;
}

bool Panel1::NumericSort::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (pi.Flags & PFLAGS_NUMERICSORT) != 0;
}

void Panel1::NumericSort::set(bool value)
{
	Info.Control(_handle, FCTL_SETNUMERICSORT, (int)value, NULL);
}

bool Panel1::RealNames::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (pi.Flags & PFLAGS_REALNAMES) != 0;
}

int Panel1::GetShownFileCount()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return pi.ItemsNumber;
}

int Panel1::GetSelectedFileCount()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return pi.SelectedItemsNumber;
}

void Panel1::Close()
{
	Info.Control(_handle, FCTL_CLOSEPLUGIN, 0, NULL);
}

void Panel1::Close(String^ path)
{
	PIN_NE(pin, path);
	Info.Control(_handle, FCTL_CLOSEPLUGIN, 0, (LONG_PTR)(const wchar_t*)pin);
}

void Panel1::GoToName(String^ name)
{
	GoToName(name, false);
}
bool Panel1::GoToName(String^ name, bool fail)
{
	if (!name)
		throw gcnew ArgumentNullException("name");

	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	PIN_NE(pin, name);
	for(int i = 0; i < pi.ItemsNumber; ++i)
	{
		AutoPluginPanelItem item(_handle, i, ShownFile);
		if (Info.FSF->LStricmp(pin, item.Get().FindData.lpwszFileName) == 0)
		{
			Redraw(i, 0);
			return true;
		}
	}

	if (fail)
		throw gcnew FileNotFoundException("File is not found: " + name);

	return false;
}

void Panel1::GoToPath(String^ path)
{
	if (!path)
		throw gcnew ArgumentNullException("path");

	//! can be nullptr, e.g. for '\'
	String^ dir = IO::Path::GetDirectoryName(path);
	if (!dir && (path->StartsWith("\\") || path->StartsWith("/")))
		dir = "\\";
	if (dir && dir->Length)
	{
		Path = dir;
		Redraw();
	}

	String^ name = IO::Path::GetFileName(path);
	if (name->Length > 0)
		GoToName(name);
}

void Panel1::Redraw()
{
	Info.Control(_handle, FCTL_REDRAWPANEL, 0, NULL);
}

void Panel1::Redraw(int current, int top)
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
	Info.Control(_handle, FCTL_REDRAWPANEL, 0, (LONG_PTR)&pri);
}

void Panel1::Select(array<int>^ indexes, bool select)
{
	//! ignore null, e.g. empty PS pipeline output
	if (!indexes || indexes->Length == 0)
		return;
	
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	Info.Control(_handle, FCTL_BEGINSELECTION, 0, 0);
	try
	{
		for(int i = 0; i < indexes->Length; ++i)
		{
			int index = indexes[i];
			if (index < 0 || index >= pi.ItemsNumber)
				throw gcnew IndexOutOfRangeException("Invalid panel item index.");
			Info.Control(_handle, FCTL_SETSELECTION, index, select);
		}
	}
	finally
	{
		Info.Control(_handle, FCTL_ENDSELECTION, 0, 0);
	}
}

void Panel1::SelectAt(array<int>^ indexes)
{
	Select(indexes, true);
}

void Panel1::UnselectAt(array<int>^ indexes)
{
	Select(indexes, false);
}

void Panel1::SelectAll(bool select)
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	Info.Control(_handle, FCTL_BEGINSELECTION, 0, 0);
	{
		for(int i = 0; i < pi.ItemsNumber; ++i)
			Info.Control(_handle, FCTL_SETSELECTION, i, select);
	}
	Info.Control(_handle, FCTL_ENDSELECTION, 0, 0);
}

void Panel1::SelectAll()
{
	SelectAll(true);
}

void Panel1::UnselectAll()
{
	SelectAll(false);
}

void Panel1::SelectNames(array<String^>^ names, bool select)
{
	if (!names || names->Length == 0)
		return;

	PanelInfo pi;
	GetPanelInfo(_handle, pi);
	List<String^> namesNow(pi.ItemsNumber);
	for(int i = 0; i < pi.ItemsNumber; ++i)
	{
		AutoPluginPanelItem item(_handle, i, ShownFile);
		namesNow.Add(gcnew String(item.Get().FindData.lpwszFileName));
	}

	Info.Control(_handle, FCTL_BEGINSELECTION, 0, 0);
	try
	{
		for each(String^ name in names)
		{
			int index = namesNow.IndexOf(name);
			if (index >= 0)
				Info.Control(_handle, FCTL_SETSELECTION, index, select);
		}
	}
	finally
	{
		Info.Control(_handle, FCTL_ENDSELECTION, 0, 0);
	}
}

void Panel1::SelectNames(array<String^>^ names)
{
	SelectNames(names, true);
}

void Panel1::UnselectNames(array<String^>^ names)
{
	SelectNames(names, false);
}

void Panel1::Update(bool keepSelection)
{
	Info.Control(_handle, FCTL_UPDATEPANEL, keepSelection, NULL);
}

void Panel1::Push()
{
	Panel0::ShelvePanel(this, true);
}
}
