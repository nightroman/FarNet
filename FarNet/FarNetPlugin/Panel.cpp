/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2008 FAR.NET Team
*/

#include "StdAfx.h"
#include "Panel.h"
#include "Far.h"

namespace FarNet
{;
static List<IFile^>^ ItemsToFiles(IList<IFile^>^ files, PluginPanelItem* panelItem, int itemsNumber)
{
	List<IFile^>^ r = gcnew List<IFile^>(itemsNumber);

	//? FAR bug: alone dots has UserData = 0 no matter what was written there; so check the dots name
	if (itemsNumber == 1 && panelItem[0].UserData == 0 && strcmp(panelItem[0].FindData.cFileName, "..") == 0)
		return r;

	for(int i = 0; i < itemsNumber; ++i)
	{
		int fi = (int)(INT_PTR)panelItem[i].UserData;
		if (fi >= 0)
			r->Add(files[fi]);
	}
	return r;
}

//
//::PanelSet::
//

void PanelSet::AsClosePlugin(HANDLE hPlugin)
{
	Log(__FUNCTION__); LogLine((INT_PTR)hPlugin);

	FarPanelPlugin^ pp = _panels[(int)(INT_PTR)hPlugin];
	pp->_info.Free();
	_panels[(int)(INT_PTR)hPlugin] = nullptr;
	if (!pp->_IsPushed && pp->_Closed) //??
		pp->_Closed(pp, nullptr);
}

int PanelSet::AsDeleteFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int opMode)
{
	Log(__FUNCTION__); LogLine(INT_PTR(hPlugin));

	FarPanelPlugin^ pp = _panels[(int)(INT_PTR)hPlugin];
	if (!pp->_DeletingFiles)
		return FALSE;
	IList<IFile^>^ files = ItemsToFiles(pp->Files, panelItem, itemsNumber);
	FilesEventArgs e(files, (OperationModes)opMode, false);
	pp->_DeletingFiles(pp, %e);
	return e.Ignore ? FALSE : TRUE;
}

void PanelSet::AsFreeFindData(PluginPanelItem* panelItem)
{
	LogLine(__FUNCTION__);

	delete[] (char*)panelItem;
}

int PanelSet::AsGetFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, char* destPath, int opMode)
{
	Log(__FUNCTION__); LogLine(INT_PTR(hPlugin));

	FarPanelPlugin^ pp = _panels[(int)(INT_PTR)hPlugin];
	if (!pp->_GettingFiles)
		return 0;
	List<IFile^>^ files = ItemsToFiles(pp->Files, panelItem, itemsNumber);
	GettingFilesEventArgs e(files, (OperationModes)opMode, move != 0, OemToStr(destPath));
	pp->_GettingFiles(pp, %e);
	return e.Ignore ? FALSE : TRUE;
}

int PanelSet::AsGetFindData(HANDLE hPlugin, PluginPanelItem** pPanelItem, int* pItemsNumber, int opMode)
{
	Log(__FUNCTION__); LogLine(INT_PTR(hPlugin));

	try
	{
		FarPanelPlugin^ pp = _panels[(int)(INT_PTR)hPlugin];
		if (pp->_GettingData)
		{
			PanelEventArgs e((OperationModes)opMode);
			pp->_GettingData(pp, %e);
			if (e.Ignore)
				return FALSE;
		}

		// all item number
		int nItem = pp->Files->Count;
		if (pp->AddDots)
			++nItem;
		(*pItemsNumber) = nItem;
		if (nItem == 0)
		{
			(*pPanelItem) = NULL;
			return TRUE;
		}

		// descriptions size
		int sizeDesc = 0;
		if (pp->AddDots && SS(pp->DotsDescription))
			sizeDesc += pp->DotsDescription->Length + 1;
		for each(FarFile^ f in pp->Files)
		{
			if (SS(f->Description))
				sizeDesc += f->Description->Length + 1;
		}

		// alloc all
		int sizeFile = nItem*sizeof(PluginPanelItem);
		char* buff = new char[sizeFile + sizeDesc];
		char* desc = buff + sizeFile;
		(*pPanelItem) = (PluginPanelItem*)buff;
		memset((*pPanelItem), 0, nItem*sizeof(PluginPanelItem));

		// add dots
		int i = -1, fi = -1;
		if (pp->AddDots)
		{
			++i;
			PluginPanelItem& p = (*pPanelItem)[0];
			p.UserData = (DWORD_PTR)-1;
			p.FindData.cFileName[0] = p.FindData.cFileName[1] = '.';
			if (SS(pp->DotsDescription))
			{
				p.Description = desc;
				desc += pp->DotsDescription->Length + 1;
				StrToOem(pp->DotsDescription, p.Description);
			}
		}

		// add files
		for each(FarFile^ f in pp->Files)
		{
			++i;
			++fi;

			PluginPanelItem& p = (*pPanelItem)[i];
			FAR_FIND_DATA& d = p.FindData;

			// names
			StrToOem(f->Name->Length >= MAX_PATH ? f->Name->Substring(0, MAX_PATH - 1) : f->Name, d.cFileName);
			if (!String::IsNullOrEmpty(f->AlternateName))
			{
				if (f->AlternateName->Length > 12)
					throw gcnew InvalidOperationException("Alternate name is longer than 12 chars.");
				StrToOem(f->AlternateName, d.cAlternateFileName);
			}

			// other
			d.dwFileAttributes = f->_flags;
			d.nFileSizeLow = (DWORD)(f->Length & 0xFFFFFFFF);
			d.nFileSizeHigh = (DWORD)(f->Length >> 32);
			d.ftCreationTime = DateTimeToFileTime(f->CreationTime);
			d.ftLastAccessTime = DateTimeToFileTime(f->LastAccessTime);
			d.ftLastWriteTime = DateTimeToFileTime(f->LastWriteTime);
			p.UserData = fi;

			if (SS(f->Description))
			{
				p.Description = desc;
				desc += f->Description->Length + 1;
				StrToOem(f->Description, p.Description);
			}
		}
		return TRUE;
	}
	catch(Exception^ e)
	{
		if ((opMode & (OPM_FIND | OPM_SILENT)) == 0)
			Far::Instance->ShowError(__FUNCTION__, e);
		return FALSE;
	}
}

void PanelSet::AsGetOpenPluginInfo(HANDLE hPlugin, OpenPluginInfo* info)
{
	Log(__FUNCTION__); LogLine(INT_PTR(hPlugin));

	FarPanelPlugin^ pp = _panels[(int)(INT_PTR)hPlugin];
	if (pp->_GettingInfo)
		pp->_GettingInfo(pp, nullptr);
	*info = pp->_info.Make();
}

int PanelSet::AsMakeDirectory(HANDLE hPlugin, char* name, int opMode)
{
	Log(__FUNCTION__); LogLine(INT_PTR(hPlugin));

	FarPanelPlugin^ pp = _panels[(int)(INT_PTR)hPlugin];
	if (!pp->_MakingDirectory)
		return FALSE;
	MakingDirectoryEventArgs e(OemToStr(name), (OperationModes)opMode);
	pp->_MakingDirectory(pp, %e);
	return e.Ignore ? FALSE : TRUE;
}

static bool _reenterOnRedrawing;
int PanelSet::AsProcessEvent(HANDLE hPlugin, int id, void* param)
{
	FarPanelPlugin^ pp = _panels[(int)(INT_PTR)hPlugin];
	switch(id)
	{
	case FE_IDLE:
#if LOG & LOG_IDLE
		Log(__FUNCTION__); Log(INT_PTR(hPlugin)); LogLine("IDLE");
#endif
		{
			if (pp->_Idled)
				pp->_Idled(pp, nullptr);
		}
		break;
	case FE_CHANGEVIEWMODE:
		Log(__FUNCTION__); Log(INT_PTR(hPlugin)); LogLine("CHANGEVIEWMODE");
		{
			if (pp->_ViewModeChanged)
			{
				ViewModeChangedEventArgs e(OemToStr((const char*)param));
				pp->_ViewModeChanged(pp, %e);
			}
		}
		break;
	case FE_CLOSE:
		Log(__FUNCTION__); Log(INT_PTR(hPlugin)); LogLine("CLOSE");
		{
			//? FE_CLOSE issues:
			// *) unwanted extra call on plugin commands entered in command line
			// *) may not be called at all e.g. if tmp panel is opened
			if (!pp->_IsPushed && pp->_Closing)
			{
				PanelEventArgs e(OperationModes::None);
				pp->_Closing(pp, %e);
				return e.Ignore;
			}
		}
		break;
	case FE_COMMAND:
		Log(__FUNCTION__); Log(INT_PTR(hPlugin)); LogLine("COMMAND");
		{
			if (pp->_Executing)
			{
				//! We have to try\catch in here in order to return exactly what plugin returns.
				ExecutingEventArgs e(OemToStr((const char*)param));
				try
				{
					pp->_Executing(pp, %e);
				}
				catch(Exception^ exception)
				{
					Far::Instance->ShowError("Event: Executing", exception);
				}
				return e.Ignore;
			}
		}
		break;
	case FE_REDRAW:
		Log(__FUNCTION__); Log(INT_PTR(hPlugin)); LogLine("REDRAW");
		{
			if (_reenterOnRedrawing)
			{
				_reenterOnRedrawing = false;
				return FALSE;
			}

			if (pp->_Redrawing)
			{
				PanelEventArgs e(OperationModes::None);
				pp->_Redrawing(pp, %e);
				if (e.Ignore)
					return TRUE;
			}

			int r = FALSE;

			// case: check posted data
			if (pp->_postData)
			{
				pp->_postFile = nullptr;
				pp->_postName = nullptr;

				int i = pp->AddDots ? 0 : -1;
				for each (IFile^ f in pp->Contents)
				{
					++i;
					if (pp->_postData == f->Data)
					{
						_reenterOnRedrawing = true;
						pp->Redraw(i, -1);
						r = true;
						break;
					}
				}

				pp->_postData = nullptr;
				return r;
			}

			// case: check posted file
			if (pp->_postFile)
			{
				pp->_postName = nullptr;

				int i = pp->AddDots ? 0 : -1;
				for each (IFile^ f in pp->Contents)
				{
					++i;
					if (pp->_postFile == f)
					{
						_reenterOnRedrawing = true;
						pp->Redraw(i, -1);
						r = true;
						break;
					}
				}

				pp->_postFile = nullptr;
				return r;
			}

			// case: check posted name
			if (pp->_postName)
			{
				int i = pp->AddDots ? 0 : -1;
				for each (IFile^ f in pp->Contents)
				{
					++i;
					if (String::Compare(pp->_postName, f->Name, true, CultureInfo::InvariantCulture) == 0)
					{
						_reenterOnRedrawing = true;
						pp->Redraw(i, -1);
						r = true;
						break;
					}
				}

				pp->_postName = nullptr;
				return r;
			}
		}
		break;
	case FE_GOTFOCUS:
		Log(__FUNCTION__); Log(INT_PTR(hPlugin)); LogLine("GOTFOCUS");
		LogLine((gcnew FarPanel(true))->Path);
		{
			if (pp->_GotFocus)
				pp->_GotFocus(pp, nullptr);
		}
		break;
	case FE_KILLFOCUS:
		Log(__FUNCTION__); Log(INT_PTR(hPlugin)); LogLine("KILLFOCUS");
		LogLine((gcnew FarPanel(true))->Path);
		{
			if (pp->_LosingFocus)
				pp->_LosingFocus(pp, nullptr);
		}
		break;
	case FE_BREAK:
		Log(__FUNCTION__); Log(INT_PTR(hPlugin)); LogLine("BREAK");
		{
			if (pp->_CtrlBreakPressed)
				pp->_CtrlBreakPressed(pp, nullptr);
		}
		break;
	}
	return FALSE;
}

int PanelSet::AsProcessKey(HANDLE hPlugin, int key, unsigned int controlState)
{
#if LOG & LOG_KEYS
	Log(__FUNCTION__); LogLine(INT_PTR(hPlugin));
#endif

	//! mind rare case: plugin in null already (e.g. closed by AltF12\select folder)
	FarPanelPlugin^ pp = _panels[(int)(INT_PTR)hPlugin];
	if (!pp || !pp->_KeyPressed)
		return FALSE;

	PanelKeyEventArgs e((key & ~PKF_PREPROCESS), (KeyStates)controlState, (key & PKF_PREPROCESS) != 0);
	pp->_KeyPressed(pp, %e);
	return e.Ignore;
}

int PanelSet::AsPutFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, int opMode)
{
	Log(__FUNCTION__); LogLine(INT_PTR(hPlugin));

	FarPanelPlugin^ pp = _panels[(int)(INT_PTR)hPlugin];
	if (!pp->_PuttingFiles)
		return 0;
	FarPanelPlugin^ plugin2 = GetPanelPlugin2(pp);
	List<IFile^>^ files;
	if (plugin2)
	{
		files = ItemsToFiles(plugin2->Files, panelItem, itemsNumber);
	}
	else
	{
		files = gcnew List<IFile^>(itemsNumber);
		for(int i = 0; i < itemsNumber; ++i)
			files->Add(FarPanel::ItemToFile(panelItem[i]));
	}
	FilesEventArgs e(files, (OperationModes)opMode, move != 0);
	pp->_PuttingFiles(pp, %e);
	return e.Ignore ? FALSE : TRUE;
}

int PanelSet::AsSetDirectory(HANDLE hPlugin, const char* dir, int opMode)
{
	Log(__FUNCTION__); LogLine(INT_PTR(hPlugin));

	_inAsSetDirectory = true;
	try
	{
		FarPanelPlugin^ pp = _panels[(int)(INT_PTR)hPlugin];
		if (!pp->_SettingDirectory)
			return TRUE;
		SettingDirectoryEventArgs e(OemToStr(dir), (OperationModes)opMode);
		pp->_SettingDirectory(pp, %e);
		return !e.Ignore;
	}
	finally
	{
		_inAsSetDirectory = false;
	}
}

FarPanel^ PanelSet::GetPanel(bool active)
{
	// get info and return null (e.g. FAR started with /e or /v)
	PanelInfo pi;
	if (!Info.Control(INVALID_HANDLE_VALUE, (active ? FCTL_GETPANELSHORTINFO : FCTL_GETANOTHERPANELSHORTINFO), &pi))
		return nullptr;

	if (!pi.Plugin)
		return gcnew FarPanel(active);

	for (int i = 1; i < cPanels; ++i)
	{
		FarPanelPlugin^ p = _panels[i];
		if (p && p->IsActive == active)
			return p;
	}

	return gcnew FarPanel(true);
}

FarPanelPlugin^ PanelSet::GetPanelPlugin(Type^ hostType)
{
	// case: any panel
	if (hostType == nullptr)
	{
		for (int i = 1; i < cPanels; ++i)
		{
			FarPanelPlugin^ p = _panels[i];
			if (p)
				return p;
		}
		return nullptr;
	}

	// panel with defined host type
	for (int i = 1; i < cPanels; ++i)
	{
		FarPanelPlugin^ p = _panels[i];
		if (p && p->Host)
		{
			Type^ type = p->Host->GetType();
			if (type == hostType || type->IsSubclassOf(hostType))
				return p;
		}
	}

	return nullptr;
}

FarPanelPlugin^ PanelSet::GetPanelPlugin2(FarPanelPlugin^ plugin)
{
	for (int i = 1; i < cPanels; ++i)
	{
		FarPanelPlugin^ p = _panels[i];
		if (p && p != plugin)
			return p;
	}
	return nullptr;
}

// [0] is for a waiting panel;
// [1-3] are 2 for already opened and 1 for being added
//? create a test case: open 2 panels and try to open 1 more
HANDLE PanelSet::AddPanelPlugin(FarPanelPlugin^ plugin)
{
	for(int i = 1; i < cPanels; ++i)
	{
		if (_panels[i] == nullptr)
		{
			_panels[i] = plugin;
			plugin->Id = i;
			return (HANDLE)(INT_PTR)i;
		}
	}
	throw gcnew InvalidOperationException("Can't register plugin panel.");
}

//! it call Update/Redraw in some cases
void PanelSet::ReplacePanelPlugin(FarPanelPlugin^ oldPanel, FarPanelPlugin^ newPanel)
{
	// check
	if (!oldPanel)
		throw gcnew ArgumentNullException("oldPanel");
	if (!newPanel)
		throw gcnew ArgumentNullException("newPanel");

	int id1 = oldPanel->Id;
	if (id1 < 1)
		throw gcnew InvalidOperationException("Old panel must be opened.");

	if (newPanel->Id >= 1)
		throw gcnew InvalidOperationException("New panel must be not opened.");

	// save old modes
	oldPanel->Info->StartSortDesc = oldPanel->ReverseSortOrder;
	oldPanel->Info->StartSortMode = oldPanel->SortMode;
	oldPanel->Info->StartViewMode = oldPanel->ViewMode;

	// disconnect old panel
	oldPanel->Id = 0;
	((FarPanelPluginInfo^)oldPanel->Info)->Free();

	// connect new panel
	_panels[id1] = newPanel;
	newPanel->Id = id1;

	// change panel modes
	if (newPanel->Info->StartViewMode != PanelViewMode::Undefined &&
		newPanel->Info->StartViewMode != oldPanel->Info->StartViewMode ||
		newPanel->Info->StartSortMode != PanelSortMode::Default && (
		newPanel->Info->StartSortMode != oldPanel->Info->StartSortMode ||
		newPanel->Info->StartSortDesc != oldPanel->Info->StartSortDesc))
	{
		// detach files to change modes with no files
		List<IFile^> dummy;
		List<IFile^>^ files = newPanel->ReplaceFiles(%dummy);
		newPanel->Update(false);

		// set only new modes
		if (newPanel->Info->StartViewMode != PanelViewMode::Undefined && newPanel->Info->StartViewMode != oldPanel->Info->StartViewMode)
			newPanel->ViewMode = newPanel->Info->StartViewMode;
		if (newPanel->Info->StartSortMode != PanelSortMode::Default)
		{
			if (newPanel->Info->StartSortMode != oldPanel->Info->StartSortMode)
				newPanel->SortMode = newPanel->Info->StartSortMode;
			if (newPanel->Info->StartSortDesc != oldPanel->Info->StartSortDesc)
				newPanel->ReverseSortOrder = newPanel->Info->StartSortDesc;
		}

		// restore original files
		newPanel->ReplaceFiles(files);
	}

	//! switch to new data and redraw, but not always: in some cases it will be done anyway, e.g. by FAR
	if (!_inAsSetDirectory) //??
	{
		newPanel->Update(false);
		newPanel->Redraw(0, 0);
	}
}

void PanelSet::OpenPanelPlugin(FarPanelPlugin^ plugin)
{
	if (!ValueCanOpenPanel::Get()) throw gcnew InvalidOperationException("Can't open a plugin panel at this moment.");
	_panels[0] = plugin;
}

void PanelSet::PushPanelPlugin(FarPanelPlugin^ plugin)
{
	if (plugin->_IsPushed) throw gcnew InvalidOperationException("Can't push the panel because it is already pushed.");
	
	plugin->_IsPushed = true;
	_stack.Add(plugin);

	// save modes
	plugin->_info.StartSortDesc = plugin->ReverseSortOrder;
	plugin->_info.StartSortMode = plugin->SortMode;
	plugin->_info.StartViewMode = plugin->ViewMode;

	// save/reset position and close
	IFile^ f = plugin->Current;
	plugin->Redraw(0, 0);
	plugin->Close();
	if (f)
		plugin->PostName(f->Name);
}

//
//::FarPanelPluginInfo::
//

FarPanelPluginInfo::FarPanelPluginInfo()
: _StartViewMode(PanelViewMode::Undefined)
{
}

void FarPanelPluginInfo::Make12Strings(char** dst, array<String^>^ src)
{
	for(int i = 11; i >= 0; --i)
	{
		delete dst[i];
		if (i >= src->Length)
			dst[i] = 0;
		else
			dst[i] = NewOem(src[i]);
	}
}

void FarPanelPluginInfo::Free12Strings(char** dst)
{
	for(int i = 11; i >= 0; --i)
	{
		delete[] dst[i];
		dst[i] = 0;
	}
}

#define FLAG(Prop, Flag) if (Prop) r |= Flag
int FarPanelPluginInfo::Flags()
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
	FLAG(UseAttrHighlighting, OPIF_USEATTRHIGHLIGHTING);
	FLAG(UseFilter, OPIF_USEFILTER);
	FLAG(UseHighlighting, OPIF_USEHIGHLIGHTING);
	FLAG(UseSortGroups, OPIF_USESORTGROUPS);
	return r;
}
#undef FLAG

void FarPanelPluginInfo::MakeInfoItems()
{
	if (m->InfoLines)
	{
		if (!_InfoItems)
		{
			m->InfoLinesNumber = 0;
			delete[] m->InfoLines;
			m->InfoLines = 0;
			return;
		}

		if (m->InfoLinesNumber < _InfoItems->Length)
		{
			delete[] m->InfoLines;
			m->InfoLines = 0;
		}
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
		StrToOem(s->Name, d.Text, 80);
		if (s->Data)
		{
			StrToOem(s->Data->ToString(), d.Data, 80);
			d.Separator = false;
		}
		else
		{
			d.Data[0] = 0;
			d.Separator = true;
		}
	}
}

void FarPanelPluginInfo::InfoItems::set(array<DataItem^>^ value)
{
	_InfoItems = value;
	if (m)
		MakeInfoItems();
}

#define SETKEYBAR(Name, Data)\
	void FarPanelPluginInfo::SetKeyBar##Name(array<String^>^ labels)\
{\
	_keyBar##Name = labels;\
	if (!m) return;\
	if (m->KeyBar)\
{\
	if (labels)\
	Make12Strings((char**)m->KeyBar->Data, labels);\
else\
	Free12Strings((char**)m->KeyBar->Data);\
	return;\
}\
	if (labels)\
{\
	m->KeyBar = new KeyBarTitles;\
	memset((void*)m->KeyBar, 0, sizeof(KeyBarTitles));\
	Make12Strings((char**)m->KeyBar->Data, labels);\
}\
}

SETKEYBAR(Alt, AltTitles)
SETKEYBAR(AltShift, AltShiftTitles)
SETKEYBAR(Ctrl, CtrlTitles)
SETKEYBAR(CtrlAlt, CtrlAltTitles)
SETKEYBAR(CtrlShift, CtrlShiftTitles)
SETKEYBAR(Main, Titles)
SETKEYBAR(Shift, ShiftTitles)

OpenPluginInfo& FarPanelPluginInfo::Make()
{
	if (m)
		return *m;

	m = new OpenPluginInfo;
	memset(m, 0, sizeof(*m));
	m->StructSize = sizeof(*m);

	m->Flags = Flags();

	m->StartSortOrder = _StartSortDesc;
	m->StartSortMode = int(_StartSortMode);
	m->StartPanelMode = int(_StartViewMode) + 0x30;

	m->CurDir = NewOem(_CurrentDirectory);
	m->Format = NewOem(_Format);
	m->HostFile = NewOem(_HostFile);
	m->PanelTitle = NewOem(_Title);

	SetKeyBarAlt(_keyBarAlt);
	SetKeyBarAltShift(_keyBarAltShift);
	SetKeyBarCtrl(_keyBarCtrl);
	SetKeyBarCtrlAlt(_keyBarCtrlAlt);
	SetKeyBarCtrlShift(_keyBarCtrlShift);
	SetKeyBarMain(_keyBarMain);
	SetKeyBarShift(_keyBarShift);

	if (_InfoItems)
		MakeInfoItems();

	return *m;
}

void FarPanelPluginInfo::Free()
{
	if (m)
	{
		delete[] m->CurDir;
		delete[] m->Format;
		delete[] m->HostFile;
		delete[] m->PanelTitle;

		delete[] m->InfoLines;

		if (m->KeyBar)
		{
			Free12Strings((char**)m->KeyBar->AltShiftTitles);
			Free12Strings((char**)m->KeyBar->AltTitles);
			Free12Strings((char**)m->KeyBar->CtrlAltTitles);
			Free12Strings((char**)m->KeyBar->CtrlShiftTitles);
			Free12Strings((char**)m->KeyBar->CtrlTitles);
			Free12Strings((char**)m->KeyBar->ShiftTitles);
			Free12Strings((char**)m->KeyBar->Titles);
			delete m->KeyBar;
		}

		delete m;
		m = 0;
	}
}

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
	PanelInfo pi;
	if (!TryBrief(pi))
		return false;
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

	DWORD key = pi.PanelRect.left == 0 ? (KeyCode::Ctrl | KeyCode::F1) : (KeyCode::Ctrl | KeyCode::F2);
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
	CBox sb(value);
	if (!Info.Control(_id, command, sb))
		throw gcnew OperationCanceledException();
}

String^ FarPanel::ToString()
{
	return Path;
}

IList<IFile^>^ FarPanel::Contents::get()
{
	PanelInfo pi; GetInfo(pi);
	List<IFile^>^ r = gcnew List<IFile^>(pi.ItemsNumber);
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

//! steps: open a panel; Tab; CtrlL; $Far.Panel used to fail
bool FarPanel::TryBrief(PanelInfo& pi)
{
	int command = _active ? FCTL_GETPANELSHORTINFO : FCTL_GETANOTHERPANELSHORTINFO;
	return Info.Control(_id, command, &pi) != 0;
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
	f->CreationTime = FileTimeToDateTime(item.FindData.ftCreationTime);
	f->LastAccessTime = FileTimeToDateTime(item.FindData.ftLastAccessTime);
	f->LastWriteTime = FileTimeToDateTime(item.FindData.ftLastWriteTime);
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
	CBox sb; sb.Reset(path);
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
//::FarPanelPlugin::
//

FarPanelPlugin::FarPanelPlugin()
: FarPanel(true)
, _files(gcnew List<IFile^>)
{
	_StartDirectory = Environment::CurrentDirectory;
}

void FarPanelPlugin::AssertOpen()
{
	if (Id <= 0) throw gcnew InvalidOperationException("Panel plugin is not opened.");
}

List<IFile^>^ FarPanelPlugin::ReplaceFiles(List<IFile^>^ files)
{
	List<IFile^>^ r = _files;
	_files = files;
	return r;
}

bool FarPanelPlugin::IsOpened::get()
{
	return Id > 0;
}

IList<IFile^>^ FarPanelPlugin::Files::get()
{
	return _files;
}

bool FarPanelPlugin::IsPlugin::get()
{
	return true;
}

IFile^ FarPanelPlugin::Current::get()
{
	AssertOpen();
	PanelInfo pi; GetInfo(pi);
	if (pi.ItemsNumber == 0)
		return nullptr;
	int fi = (int)(INT_PTR)pi.PanelItems[pi.CurrentItem].UserData;
	if (fi < 0)
		return nullptr;
	return _files[fi];
}

IList<IFile^>^ FarPanelPlugin::Contents::get()
{
	AssertOpen();
	PanelInfo pi; GetInfo(pi);
	List<IFile^>^ r = gcnew List<IFile^>(pi.ItemsNumber);
	for(int i = 0; i < pi.ItemsNumber; ++i)
	{
		int fi = (int)(INT_PTR)pi.PanelItems[i].UserData;
		if (fi >= 0)
			r->Add(_files[fi]);
	}
	return r;
}

IList<IFile^>^ FarPanelPlugin::Selected::get()
{
	AssertOpen();
	List<IFile^>^ r = gcnew List<IFile^>();
	PanelInfo pi; GetInfo(pi);
	for(int i = 0; i < pi.ItemsNumber; ++i)
	{
		if (pi.PanelItems[i].Flags & PPIF_SELECTED)
		{
			int fi = (int)(INT_PTR)pi.PanelItems[i].UserData;
			if (fi >= 0)
				r->Add(_files[fi]);
		}
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
		{
			int fi = (int)(INT_PTR)pi.PanelItems[i].UserData;
			if (fi >= 0)
				r->Add(_files[fi]);
		}
	}
	if (r->Count == 0)
	{
		if (pi.ItemsNumber > 0)
		{
			int fi = (int)(INT_PTR)pi.PanelItems[pi.CurrentItem].UserData;
			if (fi >= 0)
				r->Add(_files[fi]);
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
	return PanelSet::GetPanelPlugin2(this);
}

void FarPanelPlugin::Open(IPanelPlugin^ oldPanel)
{
	if (!oldPanel)
		throw gcnew ArgumentNullException("oldPanel");
	PanelSet::ReplacePanelPlugin((FarPanelPlugin^)oldPanel, this);
}

void FarPanelPlugin::Open()
{
	if (Id > 0) throw gcnew InvalidOperationException("Can't open the panel because it is already opened.");
	PanelSet::OpenPanelPlugin(this);
	if (_IsPushed)
	{
		PanelSet::_stack.Remove(this);
		_IsPushed = false;
	}
}

void FarPanelPlugin::Push()
{
	PanelSet::PushPanelPlugin(this);
	Id = 0; //??
}

}
