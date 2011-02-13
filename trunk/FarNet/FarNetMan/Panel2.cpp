
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "Panel2.h"
#include "Panel0.h"
#include "Shelve.h"

namespace FarNet
{;
Panel2::Panel2(Panel^ panel)
: Panel1(true)
, Host(panel)
, _Files(gcnew List<FarFile^>())
, _StartViewMode(PanelViewMode::Undefined)
, _ActiveInfo(ShelveInfoNative::CreateActiveInfo(false))
{}

IList<FarFile^>^ Panel2::Files::get() { return _Files; }
void Panel2::Files::set(IList<FarFile^>^ value)
{
	if (!value) gcnew ArgumentNullException("value");
	_Files = value;
}

bool Panel2::HasDots()
{
	switch(Host->DotsMode)
	{
	case PanelDotsMode::Dots: return true;
	case PanelDotsMode::Off: return false;
	default: return Host->Parent != nullptr;
	}
}

void Panel2::AssertOpen()
{
	if (Index <= 0)
		throw gcnew InvalidOperationException("Expected opened module panel.");
}

/*
?? It works only for panels that have the current mode defined,
because Far does not provide this info and we do not want to hack
Far:\Panel\ViewModes\ModeX, though it should work, more likely.
For now we just do nothing for not defined modes.
To submit a wish?
*/
void Panel2::SwitchFullScreen()
{
	// get
	PanelViewMode iViewMode = ViewMode;
	PanelPlan^ plan = GetPlan(iViewMode);
	if (!plan)
	{
		String^ sColumnTypes;
		{
			int size = ::Info.Control(Handle, FCTL_GETCOLUMNTYPES, 0, NULL);
			CBox buf(size);
			::Info.Control(Handle, FCTL_GETCOLUMNTYPES, size, (LONG_PTR)(wchar_t*)buf);
			sColumnTypes = gcnew String(buf);
		}
		String^ sColumnWidths;
		{
			int size = ::Info.Control(Handle, FCTL_GETCOLUMNWIDTHS, 0, NULL);
			CBox buf(size);
			::Info.Control(Handle, FCTL_GETCOLUMNWIDTHS, size, (LONG_PTR)(wchar_t*)buf);
			sColumnWidths = gcnew String(buf);
		}

		array<String^>^ types = sColumnTypes->Split(',');
		array<String^>^ widths = sColumnWidths->Split(',');
		if (types->Length != widths->Length)
			throw gcnew InvalidOperationException("Different numbers of column types and widths.");

		plan = gcnew PanelPlan;
		plan->Columns = gcnew array<FarColumn^>(types->Length);
		for(int iType = 0; iType < types->Length; ++iType)
		{
			SetColumn^ column = gcnew SetColumn();
			plan->Columns[iType] = column;
			column->Kind = types[iType];
			
			if (widths[iType]->EndsWith("%"))
				column->Width = - ParseInt(widths[iType]->Substring(0, widths[iType]->Length - 1), 0);
			else if (types[iType] == "N" || types[iType] == "Z" || types[iType] == "O")
				column->Width = 0;
			else
				column->Width = ParseInt(widths[iType], 0);
		}
	}

	// switch
	plan->IsFullScreen = !plan->IsFullScreen;

	// set
	SetPlan(iViewMode, plan);
	Redraw();
}

//! see remark for Panel1::CurrentFile::get()
FarFile^ Panel2::CurrentFile::get()
{
	AssertOpen();

	PanelInfo pi;
	GetPanelInfo(Handle, pi);

	if (pi.ItemsNumber == 0)
		return nullptr;

	AutoPluginPanelItem item(Handle, pi.CurrentItem, ShownFile);
	int fi = (int)(INT_PTR)item.Get().UserData;
	if (fi < 0)
		return nullptr;

	// 090411 Extra sanity test and watch.
	// See State::GetPanelInfo - this approach fixes the problem, but let's watch for a while.
	if (fi >= _Files->Count)
	{
		assert(0);
		return nullptr;
	}

	return _Files[fi];
}

IList<FarFile^>^ Panel2::ShownFiles::get()
{
	AssertOpen();

	PanelInfo pi;
	GetPanelInfo(Handle, pi);

	List<FarFile^>^ r = gcnew List<FarFile^>(pi.ItemsNumber);
	for(int i = 0; i < pi.ItemsNumber; ++i)
	{
		AutoPluginPanelItem item(Handle, i, ShownFile);
		int fi = (int)(INT_PTR)item.Get().UserData;
		if (fi >= 0)
			r->Add(_Files[fi]);
	}

	return r;
}

IList<FarFile^>^ Panel2::SelectedFiles::get()
{
	AssertOpen();

	PanelInfo pi;
	GetPanelInfo(Handle, pi);

	List<FarFile^>^ r = gcnew List<FarFile^>(pi.SelectedItemsNumber);
	for(int i = 0; i < pi.SelectedItemsNumber; ++i)
	{
		AutoPluginPanelItem item(Handle, i, SelectedFile);
		int fi = (int)(INT_PTR)item.Get().UserData;
		if (fi >= 0)
			r->Add(_Files[fi]);
	}

	return r;
}

FarFile^ Panel2::GetFile(int index, FileType type)
{
	AutoPluginPanelItem item(Handle, index, type);
	int fi = (int)(INT_PTR)item.Get().UserData;
	if (fi >= 0)
		// plugin file
		return _Files[fi];
	else
		// 090823 dots, not null
		return ItemToFile(item.Get());
}

String^ Panel2::StartDirectory::get()
{
	return _ActiveInfo ? _ActiveInfo->Path : String::Empty;
}

Panel^ Panel2::AnotherPanel::get()
{
	Panel2^ p = Panel0::GetPanel2(this);
	return p ? p->Host : nullptr;
}

void Panel2::OpenReplace(Panel^ current)
{
	if (!current)
		throw gcnew ArgumentNullException("current");

	Panel0::ReplacePanel(Panel0::GetPanel(current->WorksId), this);
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
		_ActiveInfo->Pop();
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

bool Panel2::RealNames::get()
{
	if (IsOpened)
		return Panel1::RealNames;
	else
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

	if (!_Plans)
		return nullptr;

	return _Plans[i];
}

#define SETKEYBAR(Name, Data) void Panel2::SetKeyBar##Name(array<String^>^ labels)\
{\
	_keyBar##Name = labels;\
	if (!m) return;\
	if (m->KeyBar)\
{\
	if (labels)\
	Make12Strings((wchar_t**)m->KeyBar->Data, labels);\
else\
	Free12Strings((wchar_t**)m->KeyBar->Data);\
	return;\
}\
	if (labels)\
{\
	m->KeyBar = new KeyBarTitles;\
	memset((void*)m->KeyBar, 0, sizeof(KeyBarTitles));\
	Make12Strings((wchar_t**)m->KeyBar->Data, labels);\
}\
}

SETKEYBAR(, Titles)
SETKEYBAR(Alt, AltTitles)
SETKEYBAR(AltShift, AltShiftTitles)
SETKEYBAR(Ctrl, CtrlTitles)
SETKEYBAR(CtrlAlt, CtrlAltTitles)
SETKEYBAR(CtrlShift, CtrlShiftTitles)
SETKEYBAR(Shift, ShiftTitles)

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
		m->StartSortMode = _FarStartSortMode;
		m->StartSortOrder = _FarStartSortOrder;
	}
}

void Panel2::Make12Strings(wchar_t** dst, array<String^>^ src)
{
	for(int i = 11; i >= 0; --i)
	{
		delete dst[i];
		if (i >= src->Length)
			dst[i] = 0;
		else
			dst[i] = NewChars(src[i]);
	}
}

void Panel2::Free12Strings(wchar_t* const dst[12])
{
	for(int i = 11; i >= 0; --i)
		delete[] dst[i];
}

#define FLAG(Prop, Flag) if (Prop) r |= Flag
int Panel2::Flags()
{
	int r = 0;
	
	FLAG(CompareFatTime, OPIF_COMPAREFATTIME);
	FLAG(PreserveCase, OPIF_SHOWPRESERVECASE);
	FLAG(RawSelection, OPIF_RAWSELECTION);
	FLAG(RealNames, OPIF_REALNAMES);
	FLAG(RealNamesDeleteFiles, OPIF_EXTERNALDELETE);
	FLAG(RealNamesExportFiles, OPIF_EXTERNALGET);
	FLAG(RealNamesImportFiles, OPIF_EXTERNALPUT);
	FLAG(RealNamesMakeDirectory, OPIF_EXTERNALMKDIR);
	FLAG(RightAligned, OPIF_SHOWRIGHTALIGNNAMES);
	FLAG(ShowNamesOnly, OPIF_SHOWNAMESONLY);
	FLAG(UseFilter, OPIF_USEFILTER);
	FLAG(UseSortGroups, OPIF_USESORTGROUPS);

	switch(_Highlighting)
	{
	case PanelHighlighting::Default: r |= OPIF_USEATTRHIGHLIGHTING; break;
	case PanelHighlighting::Full: r |= OPIF_USEHIGHLIGHTING; break;
	}

	return r;
}
#undef FLAG

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

		if (m->InfoLinesNumber < _InfoItems->Length)
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
			d.Separator = false;
		}
		else
		{
			d.Data = 0;
			d.Separator = true;
		}
	}
}

void Panel2::DeleteInfoLines()
{
	if (m->InfoLines)
	{
		for(int i = m->InfoLinesNumber; --i >= 0;)
		{
			delete m->InfoLines[i].Text;
			delete m->InfoLines[i].Data;
		}

		delete[] m->InfoLines;
		m->InfoLines = 0;
	}
}

String^ GetColumnKinds(IEnumerable<FarColumn^>^ columns)
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
	wchar_t** r = NULL;
	for each(FarColumn^ column in columns)
	{
		++i;
		if (ES(column->Name))
			continue;

		if (r == NULL)
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
	d.AlignExtensions = s->IsAlignedExtensions;
	d.CaseConversion = s->IsCaseConversion;
	d.DetailedStatus = s->IsDetailedStatus;
	d.FullScreen = s->IsFullScreen;

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
		d.ColumnTypes = NULL;
		d.ColumnWidths = NULL;
		d.ColumnTitles = NULL;
	}

	if (kinds2)
	{
		d.StatusColumnTypes = NewChars(kinds2);
		d.StatusColumnWidths = NewColumnWidths(s->StatusColumns);
	}
	else
	{
		d.StatusColumnTypes = NULL;
		d.StatusColumnWidths = NULL;
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

OpenPluginInfo& Panel2::Make()
{
	if (m)
		return *m;

	m = new OpenPluginInfo;
	memset(m, 0, sizeof(*m));
	m->StructSize = sizeof(*m);

	m->Flags = Flags();

	m->StartSortMode = _FarStartSortMode;
	m->StartSortOrder = _FarStartSortOrder;
	m->StartPanelMode = int(_StartViewMode) + 0x30;

	m->CurDir = NewChars(_PanelDirectory);
	m->Format = NewChars(_FormatName);
	m->HostFile = NewChars(_HostFile);
	m->PanelTitle = NewChars(_Title);

	SetKeyBar(_keyBar);
	SetKeyBarAlt(_keyBarAlt);
	SetKeyBarAltShift(_keyBarAltShift);
	SetKeyBarCtrl(_keyBarCtrl);
	SetKeyBarCtrlAlt(_keyBarCtrlAlt);
	SetKeyBarCtrlShift(_keyBarCtrlShift);
	SetKeyBarShift(_keyBarShift);

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
			Free12Strings(m->KeyBar->AltShiftTitles);
			Free12Strings(m->KeyBar->AltTitles);
			Free12Strings(m->KeyBar->CtrlAltTitles);
			Free12Strings(m->KeyBar->CtrlShiftTitles);
			Free12Strings(m->KeyBar->CtrlTitles);
			Free12Strings(m->KeyBar->ShiftTitles);
			Free12Strings(m->KeyBar->Titles);
			delete m->KeyBar;
		}

		delete m;
		m = 0;
	}
}

String^ Panel2::CurrentDirectory::get()
{
	return _PanelDirectory;
}

void Panel2::CurrentDirectory::set(String^ value)
{
	if (value == nullptr)
		throw gcnew ArgumentNullException("value");

	if (!Host->WorksSetPanelDirectory(nullptr))
	{
		// _090929_061740 Directory::Exists gets false for long paths
		if (value->Length < 260 && !Directory::Exists(value))
			throw gcnew ArgumentException("Directory '" + value + "' does not exist.");
		
		Close(value);
		return;
	}

	SetDirectoryEventArgs e;
	e.Name = value;
	
	Host->WorksSetPanelDirectory(%e);
	if (!e.Ignore)
	{
		Update(false);
		Redraw();
	}
}

}
