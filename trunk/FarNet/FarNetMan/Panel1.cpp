
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
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

	return 0 != (pi.Flags & PFLAGS_FOCUS);
}

bool Panel1::IsLeft::get()
{
	PanelInfo pi;
	if (!TryPanelInfo(_handle, pi))
		return false;

	return (pi.Flags & PFLAGS_PANELLEFT) != 0;
}

bool Panel1::IsNavigation::get()
{
	PanelInfo pi;
	if (!TryPanelInfo(_handle, pi))
		return false;

	return (pi.Flags & PFLAGS_SHORTCUT) != 0;
}

bool Panel1::IsPlugin::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return 0 != (pi.Flags & PFLAGS_PLUGIN);
}

bool Panel1::IsVisible::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);
	return 0 != (pi.Flags & PFLAGS_VISIBLE);
}

void Panel1::IsVisible::set(bool value)
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	bool old = 0 != (pi.Flags & PFLAGS_VISIBLE);
	if (old == value)
		return;

	String^ macro = pi.PanelRect.left == 0 ? "Keys('CtrlF1')" : "Keys('CtrlF2')";
	Far::Api->PostMacro(macro);
}

// STOP: It is possible to ask the current file directly, but implementation is not safe.
FarFile^ Panel1::CurrentFile::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	if (pi.ItemsNumber == 0)
		return nullptr;

	AutoPluginPanelItem item(_handle, (int)pi.CurrentItem, ShownFile);

	return ItemToFile(item.Get());
}

int Panel1::CurrentIndex::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return pi.ItemsNumber ? (int)pi.CurrentItem : -1;
}

int Panel1::TopIndex::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return pi.ItemsNumber ? (int)pi.TopPanelItem : -1;
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

	return pi.ItemsNumber ? Point((int)pi.CurrentItem, (int)pi.TopPanelItem) : Point(-1, -1);
}

PanelViewMode Panel1::ViewMode::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (PanelViewMode)pi.ViewMode;
}

void Panel1::ViewMode::set(PanelViewMode value)
{
	Info.PanelControl(_handle, FCTL_SETVIEWMODE, (int)value, nullptr);
}

String^ Panel1::CurrentDirectory::get()
{
	CBin bin;
	FarPanelDirectory* arg = (FarPanelDirectory*)bin.Data();
	arg->StructSize = sizeof(FarPanelDirectory);
	for(;;)
	{
		//_120325_180317 build 2556 - 0 if OPIF_SHORTCUT is not set
		size_t size = Info.PanelControl(_handle, FCTL_GETPANELDIRECTORY, bin.Size(), arg);
		if (0 == size)
			return String::Empty;

		if (!bin(size))
			break;

		arg = (FarPanelDirectory*)bin.Data();
		arg->StructSize = sizeof(FarPanelDirectory);
	}

	return gcnew String(arg->Name);
}

/* _090929_061740
Directory::Exists gets false for paths >= 260. But we have to check at least short, because FCTL_SETPANELDIR
shows the unwanted message on failures. So, let it works with no breaks at least for normal paths.
See also Mantis #1087: before Far 2.0.1187 it used to get true always.

Far 3.0.4284 does not show the unwanted message.
*/
void Panel1::CurrentDirectory::set(String^ value)
{
	if (value == nullptr)
		throw gcnew ArgumentNullException("value");

	::SetPanelDirectory(_handle, value);
}

String^ Panel1::ToString()
{
	return CurrentDirectory;
}

IList<FarFile^>^ Panel1::ShownFiles::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	List<FarFile^>^ r = gcnew List<FarFile^>((int)pi.ItemsNumber);
	for(int i = 0; i < (int)pi.ItemsNumber; ++i)
	{
		AutoPluginPanelItem item(_handle, i, ShownFile);
		if (i == 0 && item.Get().FileName[0] == '.' && item.Get().FileName[1] == '.' && item.Get().FileName[2] == '\0')
			continue;
		r->Add(ItemToFile(item.Get()));
	}

	return r;
}

IList<FarFile^>^ Panel1::SelectedFiles::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	List<FarFile^>^ r = gcnew List<FarFile^>((int)pi.SelectedItemsNumber);
	for(int i = 0; i < (int)pi.SelectedItemsNumber; ++i)
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

	file->Name = gcnew String(item.FileName);
	file->Description = gcnew String(item.Description);
	file->Owner = gcnew String(item.Owner);

	file->Attributes = (FileAttributes)item.FileAttributes;
	file->CreationTime = FileTimeToDateTime(item.CreationTime);
	file->LastAccessTime = FileTimeToDateTime(item.LastAccessTime);
	file->LastWriteTime = FileTimeToDateTime(item.LastWriteTime);
	file->Length = item.FileSize;

	if (item.CustomColumnNumber)
	{
		array<String^>^ columns = gcnew array<String^>((int)item.CustomColumnNumber);
		file->Columns = columns;
		for(int i = (int)item.CustomColumnNumber; --i >= 0;)
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

bool Panel1::UseSortGroups::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (pi.Flags & PFLAGS_USESORTGROUPS) != 0;
}
void Panel1::UseSortGroups::set(bool)
{
	throw gcnew NotSupportedException();
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
	Info.PanelControl(_handle, FCTL_SETNUMERICSORT, (int)value, nullptr);
}

bool Panel1::CaseSensitiveSort::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (pi.Flags & PFLAGS_CASESENSITIVESORT) != 0;
}

void Panel1::CaseSensitiveSort::set(bool value)
{
	Info.PanelControl(_handle, FCTL_SETCASESENSITIVESORT, (int)value, nullptr);
}

bool Panel1::DirectoriesFirst::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (pi.Flags & PFLAGS_DIRECTORIESFIRST) != 0;
}

void Panel1::DirectoriesFirst::set(bool value)
{
	Info.PanelControl(_handle, FCTL_SETDIRECTORIESFIRST, (int)value, nullptr);
}

bool Panel1::RealNames::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (pi.Flags & PFLAGS_REALNAMES) != 0;
}
void Panel1::RealNames::set(bool)
{
	throw gcnew NotSupportedException();
}

int Panel1::GetShownFileCount()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (int)pi.ItemsNumber;
}

int Panel1::GetSelectedFileCount()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (int)pi.SelectedItemsNumber;
}

void Panel1::Close()
{
	Info.PanelControl(_handle, FCTL_CLOSEPANEL, 0, nullptr);
}

void Panel1::Close(String^ path)
{
	PIN_NE(pin, path);
	Info.PanelControl(_handle, FCTL_CLOSEPANEL, 0, (wchar_t*)pin);
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
	for(int i = 0; i < (int)pi.ItemsNumber; ++i)
	{
		AutoPluginPanelItem item(_handle, i, ShownFile);
		if (Info.FSF->LStricmp(pin, item.Get().FileName) == 0)
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
	String^ dir = Path::GetDirectoryName(path);
	if (!dir && (path->StartsWith("\\") || path->StartsWith("/")))
		dir = "\\";
	if (dir && dir->Length)
	{
		CurrentDirectory = dir;
		Redraw();
	}

	String^ name = Path::GetFileName(path);
	if (name->Length > 0)
		GoToName(name);
}

void Panel1::Redraw()
{
	Info.PanelControl(_handle, FCTL_REDRAWPANEL, 0, nullptr);
}

void Panel1::Redraw(int current, int top)
{
	//! do it, else result is different
	if (current < 0 && top < 0)
	{
		Redraw();
		return;
	}

	PanelRedrawInfo pri = {sizeof(pri)};
	pri.CurrentItem = current;
	pri.TopPanelItem = top;
	Info.PanelControl(_handle, FCTL_REDRAWPANEL, 0, &pri);
}

void Panel1::Select(array<int>^ indexes, bool select)
{
	//! ignore null, e.g. empty PS pipeline output
	if (!indexes || indexes->Length == 0)
		return;

	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	Info.PanelControl(_handle, FCTL_BEGINSELECTION, 0, nullptr);
	try
	{
		for(int i = 0; i < indexes->Length; ++i)
		{
			int index = indexes[i];
			if (index < 0 || index >= (int)pi.ItemsNumber)
				throw gcnew IndexOutOfRangeException("Invalid panel item index.");
			Info.PanelControl(_handle, FCTL_SETSELECTION, index, (void*)select);
		}
	}
	finally
	{
		Info.PanelControl(_handle, FCTL_ENDSELECTION, 0, nullptr);
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

	Info.PanelControl(_handle, FCTL_BEGINSELECTION, 0, nullptr);
	{
		for(int i = 0; i < (int)pi.ItemsNumber; ++i)
			Info.PanelControl(_handle, FCTL_SETSELECTION, i, (void*)select);
	}
	Info.PanelControl(_handle, FCTL_ENDSELECTION, 0, nullptr);
}

void Panel1::SelectAll()
{
	SelectAll(true);
}

void Panel1::UnselectAll()
{
	SelectAll(false);
}

void Panel1::SelectNames(System::Collections::IEnumerable^ names, bool select)
{
	if (!names)
		return;

	PanelInfo pi;
	GetPanelInfo(_handle, pi);
	List<String^> namesNow((int)pi.ItemsNumber);
	for(int i = 0; i < (int)pi.ItemsNumber; ++i)
	{
		AutoPluginPanelItem item(_handle, i, ShownFile);
		namesNow.Add(gcnew String(item.Get().FileName));
	}

	Info.PanelControl(_handle, FCTL_BEGINSELECTION, 0, nullptr);
	try
	{
		for each(Object^ it in names)
		{
			if (it)
			{
				int index = namesNow.IndexOf(it->ToString());
				if (index >= 0)
					Info.PanelControl(_handle, FCTL_SETSELECTION, index, (void*)select);
			}
		}
	}
	finally
	{
		Info.PanelControl(_handle, FCTL_ENDSELECTION, 0, nullptr);
	}
}

void Panel1::SelectNames(System::Collections::IEnumerable^ names)
{
	SelectNames(names, true);
}

void Panel1::UnselectNames(System::Collections::IEnumerable^ names)
{
	SelectNames(names, false);
}

void Panel1::Update(bool keepSelection)
{
	Info.PanelControl(_handle, FCTL_UPDATEPANEL, keepSelection, nullptr);
}

void Panel1::Push()
{
	Panel0::ShelvePanel(this, true);
}

array<int>^ Panel1::SelectedIndexes()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	List<int> list((int)pi.SelectedItemsNumber);
	for(int i = 0; i < (int)pi.ItemsNumber; ++i)
	{
		AutoPluginPanelItem item(_handle, i, ShownFile);
		if (0 != (item.Get().Flags & PPIF_SELECTED))
			list.Add(i);
	}

	return list.ToArray();
}

bool Panel1::SelectionExists::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	if (pi.SelectedItemsNumber != 1)
		return pi.SelectedItemsNumber > 0;

	AutoPluginPanelItem item(_handle, 0, SelectedFile);
	return (0 != (item.Get().Flags & PPIF_SELECTED));
}

PanelSortMode Panel1::SortMode::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	bool reversed = (pi.Flags & PFLAGS_REVERSESORTORDER) != 0;
	return (PanelSortMode)(reversed ? -pi.SortMode : pi.SortMode);
}

void Panel1::SortMode::set(PanelSortMode value)
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	int mode = (int)value;
	bool reversed = mode < 0;
	if (reversed)
		mode = -mode;

	//! first
	if (mode != pi.SortMode)
		Info.PanelControl(_handle, FCTL_SETSORTMODE, (int)mode, nullptr);

	//! second
	if (reversed != ((pi.Flags & PFLAGS_REVERSESORTORDER) != 0))
		Info.PanelControl(_handle, FCTL_SETSORTORDER, (int)reversed, nullptr);
}

PanelPlan^ Panel1::ViewPlan::get()
{
	CBox box;

	String^ sColumnTypes;
	{
		while(box(Info.PanelControl(Handle, FCTL_GETCOLUMNTYPES, box.Size(), box))) {}
		sColumnTypes = gcnew String(box);
	}
	String^ sColumnWidths;
	{
		while(box(Info.PanelControl(Handle, FCTL_GETCOLUMNWIDTHS, box.Size(), box))) {}
		sColumnWidths = gcnew String(box);
	}

	array<String^>^ types = sColumnTypes->Split(',');
	array<String^>^ widths = sColumnWidths->Split(',');
	if (types->Length != widths->Length)
		throw gcnew InvalidOperationException("Different numbers of column types and widths.");

	PanelPlan^ plan = gcnew PanelPlan;
	plan->Columns = gcnew array<FarColumn^>(types->Length);
	for(int iType = 0; iType < types->Length; ++iType)
	{
		SetColumn^ column = gcnew SetColumn();
		plan->Columns[iType] = column;
		column->Kind = types[iType];

		if (widths[iType]->EndsWith("%"))
			column->Width = - ParseInt(widths[iType]->Substring(0, widths[iType]->Length - 1), 0);
		else
			column->Width = ParseInt(widths[iType], 0);
	}

	return plan;
}

void Panel1::SetActive()
{
	if (!Info.PanelControl(_handle, FCTL_SETACTIVEPANEL, 0, nullptr))
		throw gcnew InvalidOperationException("Panel cannot be active.");
}

}
