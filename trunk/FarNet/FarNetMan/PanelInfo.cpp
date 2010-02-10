/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "PanelInfo.h"

namespace FarNet
{;
FarPanelInfo::FarPanelInfo()
: _StartViewMode(PanelViewMode::Undefined)
{}

void FarPanelInfo::Make12Strings(wchar_t** dst, array<String^>^ src)
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

void FarPanelInfo::Free12Strings(wchar_t* const dst[12])
{
	for(int i = 11; i >= 0; --i)
		delete[] dst[i];
}

#define FLAG(Prop, Flag) if (Prop) r |= Flag
int FarPanelInfo::Flags()
{
	int r = 0;
	FLAG(CompareFatTime, OPIF_COMPAREFATTIME);
	FLAG(ExternalDelete, OPIF_EXTERNALDELETE);
	FLAG(ExternalGet, OPIF_EXTERNALGET);
	FLAG(ExternalMakeDirectory, OPIF_EXTERNALMKDIR);
	FLAG(ExternalPut, OPIF_EXTERNALPUT);
	FLAG(PreserveCase, OPIF_SHOWPRESERVECASE);
	FLAG(RawSelection, OPIF_RAWSELECTION);
	FLAG(RealNames, OPIF_REALNAMES);
	FLAG(RightAligned, OPIF_SHOWRIGHTALIGNNAMES);
	FLAG(ShowNamesOnly, OPIF_SHOWNAMESONLY);
	FLAG(UseAttributeHighlighting, OPIF_USEATTRHIGHLIGHTING);
	FLAG(UseFilter, OPIF_USEFILTER);
	FLAG(UseHighlighting, OPIF_USEHIGHLIGHTING);
	FLAG(UseSortGroups, OPIF_USESORTGROUPS);
	return r;
}
#undef FLAG

void FarPanelInfo::CreateInfoLines()
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

void FarPanelInfo::DeleteInfoLines()
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

PanelModeInfo^ FarPanelInfo::GetMode(PanelViewMode viewMode)
{
	int i = int(viewMode);
	if (i < 0 || i > 9)
		throw gcnew ArgumentException("viewMode");

	if (!_Modes)
		return nullptr;

	return _Modes[i];
}

String^ GetColumnTypes(IEnumerable<FarColumn^>^ columns)
{
	// available types
	List<String^> availableColumnTypes(FarColumn::DefaultColumnTypes);

	// pass 1: pre-process specified default types, remove them from available
	int iCustom = 0;
	for each(FarColumn^ column in columns)
	{
		// skip not specified
		if (ES(column->Type))
			continue;

		// pre-process only default types: N, O, Z, C
		switch(column->Type[0])
		{
		case 'N':
			{
				if (!availableColumnTypes.Remove("N"))
					throw gcnew InvalidOperationException("Column 'N' is used twice.");
			}
			break;
		case 'O':
			{
				if (!availableColumnTypes.Remove("O"))
					throw gcnew InvalidOperationException("Column 'O' is used twice.");
			}
			break;
		case 'Z':
			{
				if (!availableColumnTypes.Remove("Z"))
					throw gcnew InvalidOperationException("Column 'Z' is used twice.");
			}
			break;
		case 'C':
			{
				if (column->Type->Length < 2)
					throw gcnew InvalidOperationException("Invalid column type: C");

				if (iCustom != (int)(column->Type[1] - '0'))
					throw gcnew InvalidOperationException("Invalid column type: " + column->Type + ". Expected: C" + iCustom);

				availableColumnTypes.Remove(column->Type->Substring(0, 2));
				++iCustom;
			}
			break;
		}
	}

	// pass 2: get missed types from yet available
	int iAvailable = 0;
	StringBuilder sb(80);
	for each(FarColumn^ column in columns)
	{
		if (sb.Length)
			sb.Append(",");
		
		if (SS(column->Type))
			sb.Append(column->Type);
		else
			sb.Append(availableColumnTypes[iAvailable++]);
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

void InitPanelMode(::PanelMode& d, PanelModeInfo^ s)
{
	assert(s != nullptr);

	// get type strings first, it can throw
	String^ types1 = s->Columns ? GetColumnTypes(s->Columns) : nullptr;
	String^ types2 = s->StatusColumns ? GetColumnTypes(s->StatusColumns) : nullptr;

	// set others
	d.DetailedStatus = s->IsDetailedStatus;
	d.FullScreen = s->IsFullScreen;

	if (types1)
	{
		d.ColumnTypes = NewChars(types1);
		d.ColumnWidths = NewColumnWidths(s->Columns);
		d.ColumnTitles = NewColumnTitles(s->Columns);
	}
	else
	{
		d.ColumnTypes = NULL;
		d.ColumnWidths = NULL;
		d.ColumnTitles = NULL;
	}

	if (types2)
	{
		d.StatusColumnTypes = NewChars(types2);
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

void FarPanelInfo::SetMode(PanelViewMode viewMode, PanelModeInfo^ modeInfo)
{
	// index
	int i = int(viewMode);
	if (i < 0 || i > 9)
		throw gcnew ArgumentOutOfRangeException("viewMode");

	// ensure managed array
	if (!_Modes)
		_Modes = gcnew array<PanelModeInfo^>(10);

	// no native info yet, just keep data
	if (!m)
	{
		_Modes[i] = modeInfo;
		return;
	}

	// native modes?
	if (m->PanelModesArray)
	{
		// free
		if (_Modes[i])
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
	if (modeInfo)
		InitPanelMode((::PanelMode&)m->PanelModesArray[i], modeInfo);

	// keep data
	_Modes[i] = modeInfo;
}

void FarPanelInfo::CreateModes()
{
	assert(m != nullptr);
	assert(_Modes != nullptr);
	assert(!m->PanelModesArray);

	::PanelMode* modes = new PanelMode[10];
	memset(modes, 0, 10 * sizeof(::PanelMode));

	m->PanelModesArray = modes;
	m->PanelModesNumber = 10;

	for(int i = 10; --i >= 0;)
	{
		PanelModeInfo^ s = _Modes[i];
		if (s)
			InitPanelMode(modes[i], s);
	}
}

void FarPanelInfo::DeleteModes()
{
	assert(m != nullptr);

	if (!m->PanelModesArray)
		return;

	assert(_Modes && _Modes->Length == 10);

	for(int i = 10; --i >= 0;)
	{
		if (_Modes[i])
			FreePanelMode(m->PanelModesArray[i]);
	}

	delete[] m->PanelModesArray;
	m->PanelModesNumber = 0;
	m->PanelModesArray = 0;
}

void FarPanelInfo::InfoItems::set(array<DataItem^>^ value)
{
	_InfoItems = value;
	if (m)
	{
		DeleteInfoLines();
		CreateInfoLines();
	}
}

#define SETKEYBAR(Name, Data)\
	void FarPanelInfo::SetKeyBar##Name(array<String^>^ labels)\
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

SETKEYBAR(Alt, AltTitles)
SETKEYBAR(AltShift, AltShiftTitles)
SETKEYBAR(Ctrl, CtrlTitles)
SETKEYBAR(CtrlAlt, CtrlAltTitles)
SETKEYBAR(CtrlShift, CtrlShiftTitles)
SETKEYBAR(Main, Titles)
SETKEYBAR(Shift, ShiftTitles)

OpenPluginInfo& FarPanelInfo::Make()
{
	if (m)
		return *m;

	m = new OpenPluginInfo;
	memset(m, 0, sizeof(*m));
	m->StructSize = sizeof(*m);

	m->Flags = Flags();

	m->StartSortOrder = _StartReverseSortOrder;
	m->StartSortMode = int(_StartSortMode);
	m->StartPanelMode = int(_StartViewMode) + 0x30;

	m->CurDir = NewChars(_CurrentDirectory);
	m->Format = NewChars(_FormatName);
	m->HostFile = NewChars(_HostFile);
	m->PanelTitle = NewChars(_Title);

	SetKeyBarAlt(_keyBarAlt);
	SetKeyBarAltShift(_keyBarAltShift);
	SetKeyBarCtrl(_keyBarCtrl);
	SetKeyBarCtrlAlt(_keyBarCtrlAlt);
	SetKeyBarCtrlShift(_keyBarCtrlShift);
	SetKeyBarMain(_keyBarMain);
	SetKeyBarShift(_keyBarShift);

	if (_InfoItems)
		CreateInfoLines();

	if (_Modes)
		CreateModes();

	return *m;
}

void FarPanelInfo::Free()
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
}
