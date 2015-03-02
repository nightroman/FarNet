
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

#include "StdAfx.h"
#include "Panel2.h"
#include "Panel0.h"
#include "Shelve.h"

namespace FarNet
{;
static void WINAPI FarPanelItemFreeCallback(void* userData, const struct FarPanelItemFreeInfo* /*info*/);

ref struct ExplorerFilePair
{
public:
	ExplorerFilePair(Explorer^ explorer, FarFile^ file) : Explorer(explorer), File(file) {}
	Explorer^ Explorer;
	FarFile^ File;
};

ref class FileStore
{
internal:
	static int _lastFileKey = -1;
	static Dictionary<int, ExplorerFilePair^> _files;
	static FarFile^ GetFile(int key)
	{
		return _files[key]->File;
	}
	static void AddFile(PluginPanelItem& panelItem, Explorer^ explorer, FarFile^ file)
	{
		--_lastFileKey;
		_files.Add(_lastFileKey, gcnew ExplorerFilePair(explorer, file));
		panelItem.UserData.Data = (void*)_lastFileKey;
		panelItem.UserData.FreeData = FarPanelItemFreeCallback;
	}
};

static void WINAPI FarPanelItemFreeCallback(void* userData, const struct FarPanelItemFreeInfo* /*info*/)
{
	if (!FileStore::_files.Remove((int)userData))
		Log::Source->TraceEvent(TraceEventType::Warning, 0, __FUNCTION__);
}

Panel2::Panel2(Panel^ panel, Explorer^ explorer)
: Panel1(true)
, Host(panel)
, _MyExplorer(explorer)
, _Files_(gcnew List<FarFile^>())
, _StartViewMode(PanelViewMode::Undefined)
, _ActiveInfo(ShelveInfoNative::CreateActiveInfo(false))
{}

void Panel2::AssertOpen()
{
	if (Index <= 0)
		throw gcnew InvalidOperationException("Expected opened module panel.");
}

bool Panel2::HasDots::get()
{
	switch(Host->DotsMode)
	{
	case PanelDotsMode::Dots: return true;
	case PanelDotsMode::Off: return false;
	default: return Host->Parent != nullptr;
	}
}

//! see remark for Panel1::CurrentFile::get()
FarFile^ Panel2::CurrentFile::get()
{
	AssertOpen();

	if (Host->Explorer->CanExploreLocation)
		return Panel1::CurrentFile;

	PanelInfo pi;
	GetPanelInfo(Handle, pi);

	if (pi.ItemsNumber == 0)
		return nullptr;

	AutoPluginPanelItem item(Handle, (int)pi.CurrentItem, ShownFile);
	return GetItemFile(item.Get());
}

IList<FarFile^>^ Panel2::ShownFiles::get()
{
	AssertOpen();

	if (Host->Explorer->CanExploreLocation)
		return Panel1::ShownFiles;

	PanelInfo pi;
	GetPanelInfo(Handle, pi);

	List<FarFile^>^ r = gcnew List<FarFile^>((int)pi.ItemsNumber);
	for(int i = 0; i < (int)pi.ItemsNumber; ++i)
	{
		AutoPluginPanelItem item(Handle, i, ShownFile);
		FarFile^ file = GetItemFile(item.Get());
		if (file)
			r->Add(file);
	}

	return r;
}

IList<FarFile^>^ Panel2::SelectedFiles::get()
{
	AssertOpen();

	if (Host->Explorer->CanExploreLocation)
		return Panel1::SelectedFiles;

	PanelInfo pi;
	GetPanelInfo(Handle, pi);

	List<FarFile^>^ r = gcnew List<FarFile^>((int)pi.SelectedItemsNumber);
	for(int i = 0; i < (int)pi.SelectedItemsNumber; ++i)
	{
		AutoPluginPanelItem item(Handle, i, SelectedFile);
		FarFile^ file = GetItemFile(item.Get());
		if (file)
			r->Add(file);
	}

	return r;
}

//! It works for both: module and and no-data files
FarFile^ Panel2::GetFile(int index, FileType type)
{
	AutoPluginPanelItem item(Handle, index, type);
	FarFile^ file = GetItemFile(item.Get());
	if (file)
		// module file
		return file;
	else
		// dots or no-data module file
		return ItemToFile(item.Get());
}

String^ Panel2::StartDirectory::get()
{
	return _ActiveInfo ? _ActiveInfo->Path : String::Empty;
}

Panel^ Panel2::TargetPanel::get()
{
	Panel2^ p = Panel0::GetPanel2(this);
	return p ? p->Host : nullptr;
}

void Panel2::OpenReplace(Panel^ current)
{
	if (!current)
		throw gcnew ArgumentNullException("current");

	Panel0::ReplacePanel((Panel2^)current->WorksPanel, this);
}

void Panel2::Open()
{
	if (Index > 0)
		throw gcnew InvalidOperationException("Cannot open the panel because it is already opened.");

	Panel0::OpenPanel(this);
	if (_Pushed)
	{
		Works::ShelveInfo::Stack->Remove(_Pushed);
		_skipUpdateFiles = true;
		_Pushed = nullptr;
	}
}

void Panel2::Push()
{
	Panel0::PushPanel(this);
}

// close and restore the shelved
void Panel2::Close()
{
	Log::Source->TraceInformation(__FUNCTION__);
	if (_ActiveInfo)
		_ActiveInfo->Pop(IsActive);
	else
		Panel1::Close();
}

PanelSortMode Panel2::SortMode::get()
{
	if (IsOpened)
		return Panel1::SortMode;
	else
		return StartSortMode;
}
void Panel2::SortMode::set(PanelSortMode value)
{
	if (IsOpened)
		Panel1::SortMode = value;
	else
		StartSortMode = value;
}

PanelViewMode Panel2::ViewMode::get()
{
	if (IsOpened)
		return Panel1::ViewMode;
	else
		return StartViewMode;
}
void Panel2::ViewMode::set(PanelViewMode value)
{
	if (IsOpened)
		Panel1::ViewMode = value;
	else
		StartViewMode = value;
}

//! Do not call Panel1::RealNames even when IsOpened, it is not working
bool Panel2::RealNames::get()
{
	return _RealNames;
}
void Panel2::RealNames::set(bool value)
{
	_RealNames = value;
	if (m) m->Flags = Flags();
}

bool Panel2::UseSortGroups::get()
{
	if (IsOpened)
		return Panel1::UseSortGroups;
	else
		return _UseSortGroups;
}
void Panel2::UseSortGroups::set(bool value)
{
	_UseSortGroups = value;
	if (m)
		m->Flags = Flags();
}

void Panel2::InfoItems::set(array<DataItem^>^ value)
{
	_InfoItems = value;
	if (m)
	{
		DeleteInfoLines();
		CreateInfoLines();
	}
}

PanelHighlighting Panel2::Highlighting::get()
{
	return _Highlighting;
}
void Panel2::Highlighting::set(PanelHighlighting value)
{
	_Highlighting = value;
	if (m)
		m->Flags = Flags();
}

PanelPlan^ Panel2::GetPlan(PanelViewMode mode)
{
	int i = int(mode);
	if (i < 0 || i > 9)
		throw gcnew ArgumentException("mode");

	if (_Plans)
		return _Plans[i];
	else
		return nullptr;
}

PanelSortMode Panel2::StartSortMode::get()
{
	return (PanelSortMode)(_FarStartSortOrder ? -_FarStartSortMode : _FarStartSortMode);
}
void Panel2::StartSortMode::set(PanelSortMode value)
{
	int mode = (int)value;
	if (mode < 0)
	{
		_FarStartSortMode = -mode;
		_FarStartSortOrder = true;
	}
	else
	{
		_FarStartSortMode = mode;
		_FarStartSortOrder = false;
	}

	if (m)
	{
		m->StartSortMode = (OPENPANELINFO_SORTMODES)_FarStartSortMode;
		m->StartSortOrder = _FarStartSortOrder;
	}
}

int Panel2::Flags()
{
	//_120325_180317
	int r = 0; //??????OPIF_SHORTCUT;

	// highlighting
	switch(_Highlighting)
	{
	case PanelHighlighting::Default: r |= OPIF_USEATTRHIGHLIGHTING; break;
	case PanelHighlighting::Off: r |= OPIF_DISABLEHIGHLIGHTING; break;
	}

	// other flags
	if (CompareFatTime) r |= OPIF_COMPAREFATTIME;
	if (NoFilter) r |= OPIF_DISABLEFILTER;
	if (PreserveCase) r |= OPIF_SHOWPRESERVECASE;
	if (RawSelection) r |= OPIF_RAWSELECTION;
	if (RealNames) r |= OPIF_REALNAMES;
	if (RealNamesDeleteFiles) r |= OPIF_EXTERNALDELETE;
	if (RealNamesExportFiles) r |= OPIF_EXTERNALGET;
	if (RealNamesImportFiles) r |= OPIF_EXTERNALPUT;
	if (RealNamesMakeDirectory) r |= OPIF_EXTERNALMKDIR;
	if (RightAligned) r |= OPIF_SHOWRIGHTALIGNNAMES;
	if (ShowNamesOnly) r |= OPIF_SHOWNAMESONLY;
	if (!UseSortGroups) r |= OPIF_DISABLESORTGROUPS;

	return r;
}

void Panel2::CreateInfoLines()
{
	if (m->InfoLines)
	{
		if (!_InfoItems)
		{
			DeleteInfoLines();
			m->InfoLinesNumber = 0;
			m->InfoLines = 0;
			return;
		}

		if ((int)m->InfoLinesNumber < _InfoItems->Length)
			DeleteInfoLines();
	}

	if (!_InfoItems)
		return;

	m->InfoLinesNumber = _InfoItems->Length;
	if (!m->InfoLines)
		m->InfoLines = new InfoPanelLine[_InfoItems->Length];

	for(int i = _InfoItems->Length; --i >= 0;)
	{
		DataItem^ s = _InfoItems[i];
		InfoPanelLine& d = (InfoPanelLine&)m->InfoLines[i];
		d.Text = NewChars(s->Name);
		if (s->Data)
		{
			d.Data = NewChars(s->Data->ToString());
			d.Flags = 0;
		}
		else
		{
			d.Data = 0;
			d.Flags = IPLFLAGS_SEPARATOR;
		}
	}
}

void Panel2::DeleteInfoLines()
{
	if (m->InfoLines)
	{
		for(int i = (int)m->InfoLinesNumber; --i >= 0;)
		{
			delete m->InfoLines[i].Text;
			delete m->InfoLines[i].Data;
		}

		delete[] m->InfoLines;
		m->InfoLines = 0;
	}
}

String^ GetColumnKinds(array<FarColumn^>^ columns)
{
	// available kinds
	List<String^> availableColumnKinds(FarColumn::DefaultColumnKinds);

	// pass 1: pre-process specified default kinds, remove them from available
	int iCustom = 0;
	for each(FarColumn^ column in columns)
	{
		// skip not specified
		if (ES(column->Kind))
			continue;

		// pre-process only default kinds: N, O, Z, C
		switch(column->Kind[0])
		{
		case 'N':
			{
				if (!availableColumnKinds.Remove("N"))
					throw gcnew InvalidOperationException(String::Format(Res::Column0IsUsedTwice, "N"));
			}
			break;
		case 'O':
			{
				if (!availableColumnKinds.Remove("O"))
					throw gcnew InvalidOperationException(String::Format(Res::Column0IsUsedTwice, "O"));
			}
			break;
		case 'Z':
			{
				if (!availableColumnKinds.Remove("Z"))
					throw gcnew InvalidOperationException(String::Format(Res::Column0IsUsedTwice, "Z"));
			}
			break;
		case 'C':
			{
				if (column->Kind->Length < 2)
					throw gcnew InvalidOperationException(Res::InvalidColumnKind + "C");

				if (iCustom != (int)(column->Kind[1] - '0'))
					throw gcnew InvalidOperationException(Res::InvalidColumnKind + column->Kind + ". Expected: C" + iCustom);

				availableColumnKinds.Remove(column->Kind->Substring(0, 2));
				++iCustom;
			}
			break;
		}
	}

	// pass 2: get missed kinds from yet available
	int iAvailable = 0;
	StringBuilder sb(80);
	for each(FarColumn^ column in columns)
	{
		if (sb.Length)
			sb.Append(",");

		if (SS(column->Kind))
			sb.Append(column->Kind);
		else
			sb.Append(availableColumnKinds[iAvailable++]);
	}

	return sb.ToString();
}

wchar_t* NewColumnWidths(IEnumerable<FarColumn^>^ columns)
{
	StringBuilder sb(80);
	for each(FarColumn^ column in columns)
	{
		if (sb.Length)
			sb.Append(",");

		if (column->Width == 0)
			sb.Append("0");
		else if (column->Width > 0)
			sb.Append(column->Width.ToString());
		else
			sb.AppendFormat("{0}%", - column->Width);
	}
	return NewChars(sb.ToString());
}

wchar_t** NewColumnTitles(array<FarColumn^>^ columns)
{
	int i = -1;
	wchar_t** r = nullptr;
	for each(FarColumn^ column in columns)
	{
		++i;
		if (ES(column->Name))
			continue;

		if (r == nullptr)
		{
			int n = columns->Length + 1;
			r = new wchar_t*[n];
			memset(r, 0, n * sizeof(wchar_t*));
		}

		r[i] = NewChars(column->Name);
	}
	return r;
}

void InitPanelMode(::PanelMode& d, PanelPlan^ s)
{
	assert(s != nullptr);

	// options
	if (s->IsAlignedExtensions)
		d.Flags |= PMFLAGS_ALIGNEXTENSIONS;
	if (s->IsCaseConversion)
		d.Flags |= PMFLAGS_CASECONVERSION;
	if (s->IsDetailedStatus)
		d.Flags |= PMFLAGS_DETAILEDSTATUS;
	if (s->IsFullScreen)
		d.Flags |= PMFLAGS_FULLSCREEN;

	// kind strings, it can throw
	String^ kinds1 = s->Columns ? GetColumnKinds(s->Columns) : nullptr;
	String^ kinds2 = s->StatusColumns ? GetColumnKinds(s->StatusColumns) : nullptr;

	if (kinds1)
	{
		d.ColumnTypes = NewChars(kinds1);
		d.ColumnWidths = NewColumnWidths(s->Columns);
		d.ColumnTitles = NewColumnTitles(s->Columns);
	}
	else
	{
		d.ColumnTypes = nullptr;
		d.ColumnWidths = nullptr;
		d.ColumnTitles = nullptr;
	}

	if (kinds2)
	{
		d.StatusColumnTypes = NewChars(kinds2);
		d.StatusColumnWidths = NewColumnWidths(s->StatusColumns);
	}
	else
	{
		d.StatusColumnTypes = nullptr;
		d.StatusColumnWidths = nullptr;
	}
}

void FreePanelMode(const ::PanelMode& d)
{
	delete d.ColumnTypes;
	delete d.ColumnWidths;
	delete d.StatusColumnTypes;
	delete d.StatusColumnWidths;

	if (d.ColumnTitles)
	{
		for(int i = 0; d.ColumnTitles[i]; ++i)
			delete d.ColumnTitles[i];
		delete d.ColumnTitles;
	}
}

void Panel2::SetPlan(PanelViewMode mode, PanelPlan^ plan)
{
	// index
	int i = int(mode);
	if (i < 0 || i > 9)
		throw gcnew ArgumentOutOfRangeException("mode");

	// empty plan ~ just name
	if (!plan->Columns)
	{
		SetColumn^ column = gcnew SetColumn;
		column->Kind = "N";
		plan->Columns = gcnew array<FarColumn^>{column};
	}

	// ensure managed array
	if (!_Plans)
		_Plans = gcnew array<PanelPlan^>(10);

	// no native info yet, just keep data
	if (!m)
	{
		_Plans[i] = plan;
		return;
	}

	// native modes?
	if (m->PanelModesArray)
	{
		// free
		if (_Plans[i])
		{
			FreePanelMode(m->PanelModesArray[i]);
			memset((void*)&m->PanelModesArray[i], 0, sizeof(::PanelMode));
		}
	}
	// no native modes, create empty
	else
	{
		::PanelMode* modes = new PanelMode[10];
		memset(modes, 0, 10 * sizeof(::PanelMode));

		m->PanelModesArray = modes;
		m->PanelModesNumber = 10;
	}

	// init
	if (plan)
		InitPanelMode((::PanelMode&)m->PanelModesArray[i], plan);

	// keep data
	_Plans[i] = plan;
}

void Panel2::CreateModes()
{
	assert(m != nullptr);
	assert(_Plans != nullptr);
	assert(!m->PanelModesArray);

	::PanelMode* modes = new PanelMode[10];
	memset(modes, 0, 10 * sizeof(::PanelMode));

	m->PanelModesArray = modes;
	m->PanelModesNumber = 10;

	for(int i = 10; --i >= 0;)
	{
		PanelPlan^ s = _Plans[i];
		if (s)
			InitPanelMode(modes[i], s);
	}
}

void Panel2::DeleteModes()
{
	assert(m != nullptr);

	if (!m->PanelModesArray)
		return;

	assert(_Plans && _Plans->Length == 10);

	for(int i = 10; --i >= 0;)
	{
		if (_Plans[i])
			FreePanelMode(m->PanelModesArray[i]);
	}

	delete[] m->PanelModesArray;
	m->PanelModesNumber = 0;
	m->PanelModesArray = 0;
}

OpenPanelInfo& Panel2::Make()
{
	if (m)
		return *m;

	m = new OpenPanelInfo;
	memset(m, 0, sizeof(*m));
	m->StructSize = sizeof(*m);

	m->Flags = Flags();

	m->StartSortMode = (OPENPANELINFO_SORTMODES)_FarStartSortMode;
	m->StartSortOrder = _FarStartSortOrder;
	m->StartPanelMode = int(_StartViewMode) + 0x30;

	m->CurDir = NewChars(_CurrentLocation);
	m->Format = NewChars(_FormatName);
	m->HostFile = NewChars(_HostFile);
	m->PanelTitle = NewChars(_Title);

	SetKeyBars(_keyBars);

	if (_InfoItems)
		CreateInfoLines();

	if (_Plans)
		CreateModes();

	return *m;
}

void Panel2::Free()
{
	if (m)
	{
		delete[] m->CurDir;
		delete[] m->Format;
		delete[] m->HostFile;
		delete[] m->PanelTitle;

		DeleteInfoLines();
		DeleteModes();

		if (m->KeyBar)
		{
			DeleteKeyBars(*m->KeyBar);
			delete m->KeyBar;
		}

		delete m;
		m = 0;
	}
}

void Panel2::PostData(Object^ data)
{
	_postData = data;
}

void Panel2::PostFile(FarFile^ file)
{
	_postFile = file;
}

void Panel2::PostName(String^ name)
{
	_postName = name;
}

void Panel2::ReplaceExplorer(Explorer^ explorer)
{
	ExplorerEnteredEventArgs args(_MyExplorer);
	_MyExplorer = explorer;
	explorer->EnterPanel(Host);
	Host->UIExplorerEntered(%args);
}

void Panel2::Navigate(Explorer^ explorer)
{
	ReplaceExplorer(explorer);
	Update(false);
	Redraw();
}

void Panel2::CreateKeyBars(KeyBarTitles& b)
{
	b.CountLabels = _keyBars->Length;
	b.Labels = new KeyBarLabel[_keyBars->Length];
	
	for(int i = _keyBars->Length; --i >= 0;)
	{
		KeyBarLabel& it = b.Labels[i];
		KeyBar^ bar = _keyBars[i];
		it.Key.VirtualKeyCode = (WORD)bar->Key->VirtualKeyCode;
		it.Key.ControlKeyState = (DWORD)bar->Key->ControlKeyState;
		it.Text = SS(bar->Text) ? NewChars(bar->Text) : nullptr;
		it.LongText = SS(bar->LongText) ? NewChars(bar->LongText) : nullptr;
	}
}

void Panel2::DeleteKeyBars(const KeyBarTitles& b)
{
	for(int i = (int)b.CountLabels; --i >= 0;)
	{
		KeyBarLabel& it = b.Labels[i];
		delete it.Text;
		delete it.LongText;
	}
	delete b.Labels;
}

void Panel2::SetKeyBars(array<KeyBar^>^ bars)
{
	_keyBars = bars;
	if (!m)
		return;

	if (m->KeyBar)
	{
		DeleteKeyBars(*m->KeyBar);
		if (bars)
		{
			CreateKeyBars(*(KeyBarTitles*)m->KeyBar);
		}
		else
		{
			delete m->KeyBar;
			m->KeyBar = nullptr;
		}
	}
	else if (bars)
	{
		m->KeyBar = new KeyBarTitles;
		CreateKeyBars(*(KeyBarTitles*)m->KeyBar);
	}
}

List<FarFile^>^ Panel2::ItemsToFiles(IList<String^>^ names, PluginPanelItem* panelItem, int itemsNumber)
{
	List<FarFile^>^ r = gcnew List<FarFile^>(itemsNumber);

	//? Far bug: alone dots has UserData = 0 no matter what was written there; so check the dots name
	if (itemsNumber == 1 && panelItem[0].UserData.Data == 0 && wcscmp(panelItem[0].FileName, L"..") == 0)
		return r;

	// pure case
	if (Host->Explorer->CanExploreLocation)
	{
		for(int i = 0; i < itemsNumber; ++i)
		{
			r->Add(Panel1::ItemToFile(panelItem[i]));
			if (names)
				names->Add(gcnew String(panelItem[i].AlternateFileName));
		}
		return r;
	}

	// data case
	for(int i = 0; i < itemsNumber; ++i)
	{
		FarFile^ file = GetItemFile(panelItem[i]);
		if (file)
		{
			r->Add(file);
			if (names)
				names->Add(gcnew String(panelItem[i].AlternateFileName));
		}
	}

	return r;
}

// Explorer enters to the panel
void Panel2::OpenExplorer(Explorer^ explorer, ExploreEventArgs^ args)
{
	Panel^ oldPanel = Host;

	// explorers must get new explorers
	if ((Object^)explorer == (Object^)oldPanel->Explorer)
		throw gcnew InvalidOperationException("The same explorer object is not expected.");

	// make the panel
	Panel^ newPanel = nullptr;

	// make or reuse
	if (args->NewPanel || explorer->TypeId != oldPanel->Explorer->TypeId)
	{
		// make a new panel
		newPanel = explorer->CreatePanel();
	}
	else
	{
		// reuse, update is called there
		ReplaceExplorer(explorer);
		newPanel = oldPanel;
	}

	// post
	if (args)
	{
		newPanel->PostData(args->PostData);
		newPanel->PostFile(args->PostFile);
		newPanel->PostName(args->PostName);
	}

	// location
	String^ location = explorer->Location;
	if (location->Length)
		newPanel->CurrentLocation = location;
	else
		newPanel->CurrentLocation = "*";

	// same panel? update, reuse
	if (newPanel == oldPanel)
		return;

	// open new as child
	newPanel->OpenChild(oldPanel);
}

//! 090712. Allocation by chunks was originally used. But it turns out it does not improve
//! performance much (tested for 200000+ files). On the other hand allocation of large chunks
//! may fail due to memory fragmentation more frequently.
int Panel2::AsGetFindData(GetFindDataInfo* info)
{
	info->StructSize = sizeof(*info);

	Explorer^ explorer = Host->Explorer;
	ExplorerModes mode = (ExplorerModes)info->OpMode;
	const bool canExploreLocation = explorer->CanExploreLocation;

	Log::Source->TraceInformation("GetFindDataW Mode='{0}' Location='{1}'", mode, CurrentLocation);

	try
	{
		// fake empty panel needed on switching modes, for example
		if (_voidUpdateFiles)
		{
			Log::Source->TraceInformation("GetFindDataW fake empty panel");
			info->ItemsNumber = 0;
			info->PanelItem = 0;
			return 1;
		}

		// the Find mode //???????
		const bool isFind = 0 != (info->OpMode & OPM_FIND);
		const bool isSpecialFind = isFind && !canExploreLocation;

		// get the files
		if (!_skipUpdateFiles)
		{
			GetFilesEventArgs args(mode, Host->PageOffset, Host->PageLimit, Host->NeedsNewFiles);
			_Files_ = Host->UIGetFiles(%args);
			if (args.Result != JobResult::Done)
				return 0;

			Host->NeedsNewFiles = false;
		}

		// all item number
		int nItem = _Files_->Count;
		if (HasDots)
			++nItem;
		info->ItemsNumber = nItem;
		if (nItem == 0)
		{
			info->PanelItem = 0;
			return true;
		}

		// alloc all
		info->PanelItem = new PluginPanelItem[nItem];
		memset(info->PanelItem, 0, nItem * sizeof(PluginPanelItem));
		Log::Source->TraceInformation("GetFindDataW Address='{0:x}'", (long)info->PanelItem);

		// add dots
		int itemIndex = -1, fileIndex = -1;
		if (HasDots)
		{
			++itemIndex;
			wchar_t* dots = new wchar_t[3];
			dots[0] = dots[1] = '.'; dots[2] = '\0';
			PluginPanelItem& p = info->PanelItem[0];
			p.UserData.Data = (void*)(-1); //???????
			p.FileName = dots;
			p.Description = NewChars(Host->DotsDescription);
		}

		// add files
		for each(FarFile^ file in _Files_)
		{
			++itemIndex;
			++fileIndex;

			PluginPanelItem& p = info->PanelItem[itemIndex];

			// names
			p.FileName = NewChars(file->Name);
			p.Description = NewChars(file->Description);
			p.Owner = NewChars(file->Owner);

			// alternate names are for QView to work with any names,
			// even ExploreLocation explorers may have problem names
			if (info->OpMode == 0)
			{
				wchar_t buf[12]; // 12: 10=len(0xffffffff=4294967295) + 1=sign + 1=\0
				Info.FSF->itoa(fileIndex, buf, 10);
				int size = (int)wcslen(buf) + 1;
				wchar_t* alternate = new wchar_t[size];
				memcpy(alternate, buf, size * sizeof(wchar_t));
				p.AlternateFileName = alternate;
			}
			else
			{
				p.AlternateFileName = 0;
			}

			// other
			if (isSpecialFind) //???????
				FileStore::AddFile(p, explorer, file);
			else
				p.UserData.Data = (void*)(canExploreLocation ? -1 : fileIndex + 1);
			p.FileAttributes = (DWORD)file->Attributes;
			p.FileSize = file->Length;
			p.CreationTime = DateTimeToFileTime(file->CreationTime);
			p.LastWriteTime = DateTimeToFileTime(file->LastWriteTime);
			p.LastAccessTime = DateTimeToFileTime(file->LastAccessTime);

			// columns
			System::Collections::ICollection^ columns = file->Columns;
			if (columns)
			{
				int nb = columns->Count;
				if (nb)
				{
					wchar_t** custom = new wchar_t*[nb];
					p.CustomColumnNumber = nb;
					p.CustomColumnData = custom;
					int iColumn = 0;
					for each(Object^ it in columns)
					{
						if (it)
							custom[iColumn] = NewChars(it->ToString());
						else
							custom[iColumn] = 0;
						++iColumn;
					}
				}
			}
		}

		// drop pure files
		if (canExploreLocation)
			_Files_ = nullptr;

		return 1;
	}
	catch(Exception^ e)
	{
		if ((info->OpMode & (OPM_FIND | OPM_SILENT)) == 0)
			Far::Api->ShowError("Getting panel files", e);
		else
			Log::TraceException(e);

		return 0;
	}
}

int Panel2::AsSetDirectory(const SetDirectoryInfo* info)
{
	ExplorerModes mode = (ExplorerModes)info->OpMode;
	String^ directory = gcnew String(info->Dir);

	Log::Source->TraceInformation("SetDirectoryW Mode='{0}' Name='{1}'", mode, directory);

	const bool canExploreLocation = Host->Explorer->CanExploreLocation;

	//! Silent but not Find is possible on CtrlQ scan
	if (!canExploreLocation && 0 != (info->OpMode & (OPM_FIND | OPM_SILENT))) //???????
		return 0;

	Explorer^ explorer2;
	ExploreEventArgs^ args2;
	if (directory == "\\")
	{
		ExploreRootEventArgs^ args = gcnew ExploreRootEventArgs(mode);
		explorer2 = Host->UIExploreRoot(args);
		if (!explorer2)
		{
			Panel^ mp = Host;
			if (!mp->Parent)
				return 0;

			while(mp->Parent)
			{
				Panel^ parent = mp->Parent;
				mp->CloseChild();
				mp = parent;
			}

			return 1;
		}
		args2 = args;
	}
	else if (directory == "..")
	{
		ExploreParentEventArgs^ args = gcnew ExploreParentEventArgs(mode);
		explorer2 = Host->UIExploreParent(args);
		if (!explorer2)
		{
			if (!Host->Parent)
				return 0;

			Host->CloseChild();
			return 1;
		}
		args2 = args;
	}
	else if (canExploreLocation)
	{
		ExploreLocationEventArgs^ args = gcnew ExploreLocationEventArgs(mode, directory);
		explorer2 = Host->UIExploreLocation(args);
		args2 = args;
	}
	else
	{
		FarFile^ file = GetFileByUserData(info->UserData.Data);
		ExploreDirectoryEventArgs^ args = gcnew ExploreDirectoryEventArgs(mode, file);
		explorer2 = Host->UIExploreDirectory(args);
		args2 = args;
	}

	if (!explorer2)
		return 0;

	// open
	OpenExplorer(explorer2, args2);
	return 1;
}

FarFile^ Panel2::GetItemFile(const PluginPanelItem& panelItem)
{
	int key = (int)panelItem.UserData.Data;
	if (key > 0)
		return _Files_[key - 1];
	if (key < -1)
		return FileStore::GetFile(key);
	return nullptr;
}

FarFile^ Panel2::GetFileByUserData(void* data)
{
	int key = (int)data;
	if (key > 0)
		return _Files_[key - 1];
	if (key < -1)
		return FileStore::GetFile(key);
	return nullptr;
}

}
