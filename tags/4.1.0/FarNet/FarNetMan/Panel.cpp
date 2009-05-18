/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#include "StdAfx.h"
#include "Panel.h"
#include "Far.h"
#include "Wrappers.h"

namespace FarNet
{;
// Transient wrapper
ref class NativeFile sealed : public FarFile
{
private:
	const PluginPanelItem& m;
internal:
	NativeFile(const PluginPanelItem& item) : m(item)
	{}
public:
	//! PS V2 CTP3 formatter shows these properties in reversed order, before base class properties
	virtual property FileAttributes Attributes
	{
		FileAttributes get() override
		{
			return (FileAttributes)m.FindData.dwFileAttributes;
		}
	}
	virtual property Int64 Length
	{
		Int64 get() override
		{
			return m.FindData.nFileSize;
		}
	}
	virtual property DateTime LastWriteTime
	{
		DateTime get() override
		{
			return FileTimeToDateTime(m.FindData.ftLastWriteTime);
		}
	}
	virtual property DateTime LastAccessTime
	{
		DateTime get() override
		{
			return FileTimeToDateTime(m.FindData.ftLastAccessTime);
		}
	}
	virtual property DateTime CreationTime
	{
		DateTime get() override
		{
			return FileTimeToDateTime(m.FindData.ftCreationTime);
		}
	}
	virtual property String^ AlternateName
	{
		String^ get() override
		{
			return gcnew String(m.FindData.lpwszAlternateFileName);
		}
	}
	virtual property String^ Owner
	{
		String^ get() override
		{
			return gcnew String(m.Owner);
		}
	}
	virtual property String^ Description
	{
		String^ get() override
		{
			return gcnew String(m.Description);
		}
	}
	virtual property String^ Name
	{
		String^ get() override
		{
			return gcnew String(m.FindData.lpwszFileName);
		}
	}
};

#pragma region Kit

static List<FarFile^>^ ItemsToFiles(IList<FarFile^>^ files, PluginPanelItem* panelItem, int itemsNumber)
{
	List<FarFile^>^ r = gcnew List<FarFile^>(itemsNumber);

	//? FAR bug: alone dots has UserData = 0 no matter what was written there; so check the dots name
	if (itemsNumber == 1 && panelItem[0].UserData == 0 && wcscmp(panelItem[0].FindData.lpwszFileName, L"..") == 0)
		return r;

	for(int i = 0; i < itemsNumber; ++i)
	{
		int fi = (int)(INT_PTR)panelItem[i].UserData;
		if (fi >= 0)
			r->Add(files[fi]);
	}
	return r;
}

#pragma endregion

#pragma region PanelSet

void PanelSet::AsClosePlugin(HANDLE hPlugin)
{
	LOG_AUTO(3, "ClosePlugin");

	FarPluginPanel^ pp = _panels[(int)(INT_PTR)hPlugin];
	pp->_info.Free();
	_panels[(int)(INT_PTR)hPlugin] = nullptr;
	if (!pp->_IsPushed && pp->_Closed)
		pp->_Closed(pp, nullptr);
}

int PanelSet::AsDeleteFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int opMode)
{
	LOG_AUTO(3, "DeleteFiles");

	FarPluginPanel^ pp = _panels[(int)(INT_PTR)hPlugin];
	if (!pp->_DeletingFiles)
		return false;

	IList<FarFile^>^ files = ItemsToFiles(pp->Files, panelItem, itemsNumber);
	FilesEventArgs e(files, (OperationModes)opMode, false);
	pp->_DeletingFiles(pp, %e);

	return e.Ignore ? false : true;
}

void PanelSet::AsFreeFindData(PluginPanelItem* panelItem)
{
	LOG_AUTO(3, "FreeFindData");

	delete[] (char*)panelItem;
}

//?? Parameter destPath can be changed, i.e. (*destPath) replaced. NYI here.
int PanelSet::AsGetFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, const wchar_t** destPath, int opMode)
{
	LOG_AUTO(3, "GetFiles");

	FarPluginPanel^ pp = _panels[(int)(INT_PTR)hPlugin];
	if (!pp->_GettingFiles)
		return 0;

	List<FarFile^>^ files = ItemsToFiles(pp->Files, panelItem, itemsNumber);
	GettingFilesEventArgs e(files, (OperationModes)opMode, move != 0, gcnew String((*destPath)));
	pp->_GettingFiles(pp, %e);

	return e.Ignore ? false : true;
}

static const wchar_t s_dots[] = L"..";
int PanelSet::AsGetFindData(HANDLE hPlugin, PluginPanelItem** pPanelItem, int* pItemsNumber, int opMode)
{
	LOG_AUTO(3, "GetFindData");

	try
	{
		FarPluginPanel^ pp = _panels[(int)(INT_PTR)hPlugin];

		if (pp->_GettingData && !pp->_skipGettingData)
		{
			PanelEventArgs e((OperationModes)opMode);
			pp->_GettingData(pp, %e);
			if (e.Ignore)
				return false;
		}

		// all item number
		int nItem = pp->Files->Count;
		if (pp->AddDots)
			++nItem;
		(*pItemsNumber) = nItem;
		if (nItem == 0)
		{
			(*pPanelItem) = NULL;
			return true;
		}

		// calculate size
		int countChars = 0;
		if (pp->AddDots && SS(pp->DotsDescription))
			countChars += pp->DotsDescription->Length + 1;
		for each(FarFile^ f in pp->Files)
		{
			if (SS(f->Name))
				countChars += f->Name->Length + 1;
			if (SS(f->Description))
				countChars += f->Description->Length + 1;
			if (SS(f->Owner))
				countChars += f->Owner->Length + 1;
			if (SS(f->AlternateName))
				countChars += f->AlternateName->Length + 1;
		}

		// alloc all
		int sizeFile = nItem * sizeof(PluginPanelItem);
		char* buff = new char[sizeFile + countChars * sizeof(wchar_t)];
		wchar_t* data = (wchar_t*)(buff + sizeFile);
		(*pPanelItem) = (PluginPanelItem*)buff;
		memset((*pPanelItem), 0, nItem * sizeof(PluginPanelItem));

		// add dots
		int i = -1, fi = -1;
		if (pp->AddDots)
		{
			++i;
			PluginPanelItem& p = (*pPanelItem)[0];
			p.UserData = (DWORD_PTR)-1;
			p.FindData.lpwszFileName = (wchar_t*)s_dots;
			if (SS(pp->DotsDescription))
			{
				CopyStringToChars(pp->DotsDescription, data);
				p.Description = data;
				data += pp->DotsDescription->Length + 1;
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
			if (SS(f->Name))
			{
				CopyStringToChars(f->Name, data);
				d.lpwszFileName = data;
				data += f->Name->Length + 1;
			}
			if (SS(f->Description))
			{
				CopyStringToChars(f->Description, data);
				p.Description = data;
				data += f->Description->Length + 1;
			}
			if (SS(f->Owner))
			{
				CopyStringToChars(f->Owner, data);
				p.Owner = data;
				data += f->Owner->Length + 1;
			}
			if (SS(f->AlternateName))
			{
				CopyStringToChars(f->AlternateName, data);
				d.lpwszAlternateFileName = data;
				data += f->AlternateName->Length + 1;
			}

			// other
			d.dwFileAttributes = (DWORD)f->Attributes;
			d.nFileSize = f->Length;
			d.ftCreationTime = DateTimeToFileTime(f->CreationTime);
			d.ftLastWriteTime = DateTimeToFileTime(f->LastWriteTime);
			d.ftLastAccessTime = DateTimeToFileTime(f->LastAccessTime);
			p.UserData = fi;
		}
		return true;
	}
	catch(Exception^ e)
	{
		//??? .. else log error?
		if ((opMode & (OPM_FIND | OPM_SILENT)) == 0)
			Far::Instance->ShowError(__FUNCTION__, e);

		return false;
	}
}

void PanelSet::AsGetOpenPluginInfo(HANDLE hPlugin, OpenPluginInfo* info)
{
	LOG_AUTO(3, "GetOpenPluginInfo");

	// plugin panel
	FarPluginPanel^ pp = _panels[(int)(INT_PTR)hPlugin];

	//! pushed case
	//?? Far calls this after Close(), perhaps a bug. How: push folder tree panel.
	if (pp->IsPushed)
	{
		info->StructSize = sizeof(OpenPluginInfo);
		return;
	}

	// trigger - allow to update info before making it for Far
	if (pp->_GettingInfo && !State::GetPanelInfo)
		pp->_GettingInfo(pp, nullptr);

	// make info
	*info = pp->_info.Make();
}

//?? Parameter name can be changed, i.e. (*name) replaced. NYI here.
int PanelSet::AsMakeDirectory(HANDLE hPlugin, const wchar_t** name, int opMode)
{
	LOG_AUTO(3, "MakeDirectory");

	FarPluginPanel^ pp = _panels[(int)(INT_PTR)hPlugin];
	if (!pp->_MakingDirectory)
		return false;

	MakingDirectoryEventArgs e(gcnew String((*name)), (OperationModes)opMode);
	pp->_MakingDirectory(pp, %e);

	return e.Ignore ? false : true;
}

static bool _reenterOnRedrawing;
int PanelSet::AsProcessEvent(HANDLE hPlugin, int id, void* param)
{
	FarPluginPanel^ pp = _panels[(int)(INT_PTR)hPlugin];
	switch(id)
	{
	case FE_IDLE:
		{
			LOG_AUTO(4, "FE_IDLE");

			if (pp->IdleUpdate)
			{
				pp->Update(true);
				pp->Redraw();
			}
			if (pp->_Idled)
				pp->_Idled(pp, nullptr);
		}
		break;
	case FE_CHANGEVIEWMODE:
		{
			LOG_AUTO(3, "FE_CHANGEVIEWMODE");

			if (pp->_ViewModeChanged)
			{
				ViewModeChangedEventArgs e(gcnew String((const wchar_t*)param));
				pp->_ViewModeChanged(pp, %e);
			}
		}
		break;
	case FE_CLOSE:
		{
			LOG_AUTO(3, "FE_CLOSE");

			//? FE_CLOSE issues:
			// *) Bug [_090321_165608]: unwanted extra call on plugin commands entered in command line
			// http://bugs.farmanager.com/view.php?id=602
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
		{
			LOG_AUTO(3, "FE_COMMAND");

			if (pp->_Executing)
			{
				//! We have to try\catch in here in order to return exactly what plugin returns.
				ExecutingEventArgs e(gcnew String((const wchar_t*)param));
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
		{
			LOG_AUTO(3, "FE_REDRAW");

			// ??? 090411 Data are shown now. Drop this flag to allow normal processing.
			pp->_skipGettingData = false;

			if (_reenterOnRedrawing)
			{
				_reenterOnRedrawing = false;
				return false;
			}

			if (pp->_Redrawing)
			{
				PanelEventArgs e(OperationModes::None);
				pp->_Redrawing(pp, %e);
				if (e.Ignore)
					return true;
			}

			int r = false;

			// case: use data matcher
			if (pp->DataComparison && (pp->_postData || pp->_postFile && pp->_postFile->Data))
			{
				Object^ data = pp->_postData ? pp->_postData : pp->_postFile->Data;
				pp->_postFile = nullptr;
				pp->_postData = nullptr;
				pp->_postName = nullptr;

				int i = pp->AddDots ? 0 : -1;
				for each (FarFile^ f in pp->ShownFiles)
				{
					++i;
					if (pp->DataComparison(data, f->Data) == 0)
					{
						_reenterOnRedrawing = true;
						pp->Redraw(i, -1);
						r = true;
						break;
					}
				}

				return r;
			}

			// case: check posted data
			if (pp->_postData)
			{
				pp->_postFile = nullptr;
				pp->_postName = nullptr;

				int i = pp->AddDots ? 0 : -1;
				for each (FarFile^ f in pp->ShownFiles)
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
				for each (FarFile^ f in pp->ShownFiles)
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
				for each (FarFile^ f in pp->ShownFiles)
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
		{
			LOG_AUTO(3, "FE_GOTFOCUS");

			if (pp->_GotFocus)
				pp->_GotFocus(pp, nullptr);
		}
		break;
	case FE_KILLFOCUS:
		{
			LOG_AUTO(3, "FE_KILLFOCUS");

			if (pp->_LosingFocus)
				pp->_LosingFocus(pp, nullptr);
		}
		break;
	case FE_BREAK:
		{
			LOG_AUTO(3, "FE_BREAK");

			if (pp->_CtrlBreakPressed)
				pp->_CtrlBreakPressed(pp, nullptr);
		}
		break;
	}
	return false;
}

int PanelSet::AsProcessKey(HANDLE hPlugin, int key, unsigned int controlState)
{
	LOG_AUTO(4, "ProcessKey");

	//! mind rare case: plugin in null already (e.g. closed by AltF12\select folder)
	FarPluginPanel^ pp = _panels[(int)(INT_PTR)hPlugin];
	if (!pp)
		return false;

	// Escaping handler
	if (pp->_Escaping)
	{
		// [Escape] is pressed:
		if (VK_ESCAPE == key && 0 == controlState)
		{
			// if cmdline is not empty then do nothing
			int size = Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINE, 0, 0);
			if (size > 1)
				return false;

			// trigger the handler
			PanelEventArgs e;
			pp->_Escaping(pp, %e);
			if (e.Ignore)
				return true;
		}
	}

	// panel has no handler:
	if (!pp->_KeyPressed)
		return false;

	// trigger the handler
	PanelKeyEventArgs e((key & ~PKF_PREPROCESS), (KeyStates)controlState, (key & PKF_PREPROCESS) != 0);
	pp->_KeyPressed(pp, %e);
	return e.Ignore;
}

int PanelSet::AsPutFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, int opMode)
{
	LOG_AUTO(3, "PutFiles");

	FarPluginPanel^ pp = _panels[(int)(INT_PTR)hPlugin];
	if (!pp->_PuttingFiles)
		return 0;

	FarPluginPanel^ plugin2 = GetPluginPanel2(pp);
	List<FarFile^>^ files;
	if (plugin2)
	{
		files = ItemsToFiles(plugin2->Files, panelItem, itemsNumber);
	}
	else
	{
		files = gcnew List<FarFile^>(itemsNumber);
		for(int i = 0; i < itemsNumber; ++i)
		{
			//! we can use transient 'NativeFile' in here, but 'transient' it is not that safe
			files->Add(FarPanel::ItemToFile(panelItem[i]));
		}
	}

	FilesEventArgs e(files, (OperationModes)opMode, move != 0);
	pp->_PuttingFiles(pp, %e);
	return e.Ignore ? false : true;
}

int PanelSet::AsSetDirectory(HANDLE hPlugin, const wchar_t* dir, int opMode)
{
	LOG_AUTO(3, "SetDirectory");

	_inAsSetDirectory = true;
	try
	{
		FarPluginPanel^ pp = _panels[(int)(INT_PTR)hPlugin];
		if (!pp->_SettingDirectory)
			return true;
		SettingDirectoryEventArgs e(gcnew String(dir), (OperationModes)opMode);
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
	if (!TryPanelInfo((active ? PANEL_ACTIVE : PANEL_PASSIVE), pi))
		return nullptr;

	if (!pi.Plugin)
		return gcnew FarPanel(active);

	for (int i = 1; i < cPanels; ++i)
	{
		FarPluginPanel^ p = _panels[i];
		if (p && p->IsActive == active)
			return p;
	}

	return gcnew FarPanel(true);
}

FarPluginPanel^ PanelSet::GetPluginPanel(Guid id)
{
	for (int i = 1; i < cPanels; ++i)
	{
		FarPluginPanel^ p = _panels[i];
		if (p && p->Id == id)
			return p;
	}
	return nullptr;
}

FarPluginPanel^ PanelSet::GetPluginPanel(Type^ hostType)
{
	// case: any panel
	if (hostType == nullptr)
	{
		for (int i = 1; i < cPanels; ++i)
		{
			FarPluginPanel^ p = _panels[i];
			if (p)
				return p;
		}
		return nullptr;
	}

	// panel with defined host type
	for (int i = 1; i < cPanels; ++i)
	{
		FarPluginPanel^ p = _panels[i];
		if (p && p->Host)
		{
			Type^ type = p->Host->GetType();
			if (type == hostType || type->IsSubclassOf(hostType))
				return p;
		}
	}

	return nullptr;
}

FarPluginPanel^ PanelSet::GetPluginPanel2(FarPluginPanel^ plugin)
{
	for (int i = 1; i < cPanels; ++i)
	{
		FarPluginPanel^ p = _panels[i];
		if (p && p != plugin)
			return p;
	}
	return nullptr;
}

// [0] is for a waiting panel;
// [1-3] are 2 for already opened and 1 for being added
//? create a test case: open 2 panels and try to open 1 more
HANDLE PanelSet::AddPluginPanel(FarPluginPanel^ plugin)
{
	for(int i = 1; i < cPanels; ++i)
	{
		if (_panels[i] == nullptr)
		{
			_panels[i] = plugin;
			plugin->Index = i;
			return plugin->Handle;
		}
	}
	throw gcnew InvalidOperationException("Cannot add a plugin panel.");
}

// EndOpenMode() must be called after
void PanelSet::BeginOpenMode()
{
	if (_openMode < 0)
		throw gcnew InvalidOperationException("Negative open mode.");

	++_openMode;
}

// BeginOpenMode() must be called before
void PanelSet::EndOpenMode()
{
	if (_openMode <= 0)
		throw gcnew InvalidOperationException("Not positive open mode.");

	if (--_openMode == 0)
		_panels[0] = nullptr;
}

void PanelSet::OpenPluginPanel(FarPluginPanel^ plugin)
{
	// plugin must be called for opening
	if (_openMode == 0)
		throw gcnew InvalidOperationException("Cannot open a panel because a plugin is not called for opening.");

	// only one panel can be opened at a time
	if (_panels[0] && _panels[0] != plugin)
		throw gcnew InvalidOperationException("Cannot open a panel because another panel is already waiting.");

	// panels window should be current
	try
	{
		Far::Instance->SetCurrentWindow(0);
	}
	catch(InvalidOperationException^ e)
	{
		throw gcnew InvalidOperationException("Cannot open a panel because panels window cannot be set current.", e);
	}

	_panels[0] = plugin;
}

//! it call Update/Redraw in some cases
void PanelSet::ReplacePluginPanel(FarPluginPanel^ oldPanel, FarPluginPanel^ newPanel)
{
	// check
	if (!oldPanel)
		throw gcnew ArgumentNullException("oldPanel");
	if (!newPanel)
		throw gcnew ArgumentNullException("newPanel");

	int id1 = oldPanel->Index;
	if (id1 < 1)
		throw gcnew InvalidOperationException("Old panel must be opened.");

	if (newPanel->Index >= 1)
		throw gcnew InvalidOperationException("New panel must be not opened.");

	// save old modes
	oldPanel->Info->StartSortDesc = oldPanel->ReverseSortOrder;
	oldPanel->Info->StartSortMode = oldPanel->SortMode;
	oldPanel->Info->StartViewMode = oldPanel->ViewMode;

	// disconnect old panel
	oldPanel->Handle = 0;
	((FarPluginPanelInfo^)oldPanel->Info)->Free();

	// connect new panel
	_panels[id1] = newPanel;
	newPanel->Index = id1;

	// change panel modes
	if (newPanel->Info->StartViewMode != PanelViewMode::Undefined &&
		newPanel->Info->StartViewMode != oldPanel->Info->StartViewMode ||
		newPanel->Info->StartSortMode != PanelSortMode::Default && (
		newPanel->Info->StartSortMode != oldPanel->Info->StartSortMode ||
		newPanel->Info->StartSortDesc != oldPanel->Info->StartSortDesc))
	{
		// detach files to change modes with no files
		List<FarFile^> dummy;
		List<FarFile^>^ files = newPanel->ReplaceFiles(%dummy);
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
	if (!_inAsSetDirectory)
	{
		newPanel->Update(false);
		newPanel->Redraw(0, 0);
	}
}

void PanelSet::PushPluginPanel(FarPluginPanel^ plugin)
{
	if (plugin->_IsPushed)
		throw gcnew InvalidOperationException("Cannot push the panel because it is already pushed.");

	//! save current state effectively by Far API, not FarNet
	PanelInfo pi;
	GetPanelInfo(plugin->Handle, pi);

	// save modes
	plugin->_info.StartSortDesc = (pi.Flags & PFLAGS_REVERSESORTORDER) != 0;
	plugin->_info.StartSortMode = (PanelSortMode)pi.SortMode;
	plugin->_info.StartViewMode = (PanelViewMode)pi.ViewMode;

	// current
	FarFile^ file = nullptr;
	if (pi.ItemsNumber > 0)
	{
		AutoPluginPanelItem item(plugin->Handle, pi.CurrentItem, false);
		int index = (int)item.Get().UserData;
		if (index >= 0 && index < plugin->Files->Count)
			file = plugin->Files[index];
	}

	// push
	plugin->_IsPushed = true;
	_stack.Add(plugin);

	// reset position, close
	// 090411 Was: Redraw(0, 0) + Close(). New way looks more effective and perhaps avoids some Far->FarNet calls.
	plugin->Close(".");
	if (file)
		plugin->PostFile(file);

	// drop handle
	plugin->Handle = 0;
}

#pragma endregion

#pragma region FarPluginPanelInfo

FarPluginPanelInfo::FarPluginPanelInfo()
: _StartViewMode(PanelViewMode::Undefined)
{
}

void FarPluginPanelInfo::Make12Strings(wchar_t** dst, array<String^>^ src)
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

void FarPluginPanelInfo::Free12Strings(wchar_t* const dst[12])
{
	for(int i = 11; i >= 0; --i)
		delete[] dst[i];
}

#define FLAG(Prop, Flag) if (Prop) r |= Flag
int FarPluginPanelInfo::Flags()
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

void FarPluginPanelInfo::CreateInfoLines()
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

void FarPluginPanelInfo::DeleteInfoLines()
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

PanelModeInfo^ FarPluginPanelInfo::GetMode(PanelViewMode viewMode)
{
	int i = int(viewMode);
	if (i < 0 || i > 9)
		throw gcnew ArgumentException("viewMode");

	if (!_Modes)
		return nullptr;

	return _Modes[i];
}

//???
void InitPanelMode(::PanelMode& d, PanelModeInfo^ s)
{
	assert(s != nullptr);

	d.ColumnTypes = NewChars(s->ColumnTypes);
	d.ColumnWidths = NewChars(s->ColumnWidths);
	d.StatusColumnTypes = NewChars(s->StatusColumnTypes);
	d.StatusColumnWidths = NewChars(s->StatusColumnWidths);

	if (s->ColumnTitles && s->ColumnTitles->Length)
	{
		d.ColumnTitles = new wchar_t*[s->ColumnTitles->Length + 1];
		d.ColumnTitles[s->ColumnTitles->Length] = 0;
		for(int i = s->ColumnTitles->Length; --i >= 0;)
			d.ColumnTitles[i] = NewChars(s->ColumnTitles[i]);
	}

	d.DetailedStatus = s->IsDetailedStatus;
	d.FullScreen = s->IsFullScreen;
}

//???
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

void FarPluginPanelInfo::SetMode(PanelViewMode viewMode, PanelModeInfo^ modeInfo)
{
	// index
	int i = int(viewMode);
	if (i < 0 || i > 9)
		throw gcnew ArgumentOutOfRangeException("viewMode");

	// types
	if (ES(modeInfo->ColumnTypes))
		throw gcnew ArgumentException("Column types must be defined.");

	// titles
	if (modeInfo->ColumnTitles)
	{
		// eval column number by types delimited by comma
		int nb = 1;
		for each(char c in modeInfo->ColumnTypes)
			if (c == ',')
				++nb;

		// test title number (or Far will read crap)
		// '<' will do, but let it be '!='
		if (modeInfo->ColumnTitles->Length != nb)
			throw gcnew ArgumentException("Column titles number does not match column types.");
	}

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

void FarPluginPanelInfo::CreateModes()
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

void FarPluginPanelInfo::DeleteModes()
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

void FarPluginPanelInfo::InfoItems::set(array<DataItem^>^ value)
{
	_InfoItems = value;
	if (m)
	{
		DeleteInfoLines();
		CreateInfoLines();
	}
}

#define SETKEYBAR(Name, Data)\
	void FarPluginPanelInfo::SetKeyBar##Name(array<String^>^ labels)\
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

OpenPluginInfo& FarPluginPanelInfo::Make()
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

	m->CurDir = NewChars(_CurrentDirectory);
	m->Format = NewChars(_Format);
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

void FarPluginPanelInfo::Free()
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

#pragma endregion

#pragma region FarPanel

FarPanel::FarPanel(bool current)
: _handle(current ? PANEL_ACTIVE : PANEL_PASSIVE)
{
}

HANDLE FarPanel::Handle::get()
{
	return _handle;
}

void FarPanel::Handle::set(HANDLE value)
{
	_handle = value;
}

bool FarPanel::IsActive::get()
{
	PanelInfo pi;
	if (!TryPanelInfo(_handle, pi))
		return false;

	return pi.Focus != 0;
}

bool FarPanel::IsLeft::get()
{
	PanelInfo pi;
	if (!TryPanelInfo(_handle, pi))
		return false;

	return (pi.Flags & PFLAGS_PANELLEFT) != 0;
}

bool FarPanel::IsPlugin::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return pi.Plugin != 0;
}

bool FarPanel::IsVisible::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);
	return pi.Visible != 0;
}

void FarPanel::IsVisible::set(bool value)
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
FarFile^ FarPanel::CurrentFile::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	if (pi.ItemsNumber == 0)
		return nullptr;

	AutoPluginPanelItem item(_handle, pi.CurrentItem, false);

	return ItemToFile(item.Get());
}

int FarPanel::CurrentIndex::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return pi.ItemsNumber ? pi.CurrentItem : -1;
}

int FarPanel::TopIndex::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return pi.ItemsNumber ? pi.TopPanelItem : -1;
}

Place FarPanel::Window::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	Place r;
	r.Left = pi.PanelRect.left; r.Top = pi.PanelRect.top;
	r.Right = pi.PanelRect.right; r.Bottom = pi.PanelRect.bottom;
	return r;
}

Point FarPanel::Frame::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return pi.ItemsNumber ? Point(pi.CurrentItem, pi.TopPanelItem) : Point(-1, -1);
}

PanelSortMode FarPanel::SortMode::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (PanelSortMode)pi.SortMode;
}

void FarPanel::SortMode::set(PanelSortMode value)
{
	Info.Control(_handle, FCTL_SETSORTMODE, (int)value, NULL);
}

PanelViewMode FarPanel::ViewMode::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (PanelViewMode)pi.ViewMode;
}

void FarPanel::ViewMode::set(PanelViewMode value)
{
	Info.Control(_handle, FCTL_SETVIEWMODE, (int)value, NULL);
}

String^ FarPanel::Path::get()
{
	int size = Info.Control(_handle, FCTL_GETCURRENTDIRECTORY, 0, NULL);
	CBox buf(size);
	Info.Control(_handle, FCTL_GETCURRENTDIRECTORY, size, (LONG_PTR)(wchar_t*)buf);
	return gcnew String(buf);
}

void FarPanel::Path::set(String^ value)
{
	if (value == nullptr)
		throw gcnew ArgumentNullException("value");
	if (!Directory::Exists(value))
		throw gcnew ArgumentException("Directory '" + value + "' does not exist.");

	PIN_NE(pin, value);
	if (!Info.Control(_handle, FCTL_SETPANELDIR, 0, (LONG_PTR)pin))
		throw gcnew OperationCanceledException;
}

String^ FarPanel::ToString()
{
	return Path;
}

IList<FarFile^>^ FarPanel::ShownFiles::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	List<FarFile^>^ r = gcnew List<FarFile^>(pi.ItemsNumber);
	for(int i = 0; i < pi.ItemsNumber; ++i)
	{
		AutoPluginPanelItem item(_handle, i, false);
		r->Add(ItemToFile(item.Get()));
	}

	return r;
}

IList<FarFile^>^ FarPanel::SelectedFiles::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	List<FarFile^>^ r = gcnew List<FarFile^>(pi.SelectedItemsNumber);
	for(int i = 0; i < pi.SelectedItemsNumber; ++i)
	{
		AutoPluginPanelItem item(_handle, i, true);
		r->Add(ItemToFile(item.Get()));
	}

	return r;
}

PanelType FarPanel::Type::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (PanelType)pi.PanelType;
}

SetFile^ FarPanel::ItemToFile(const PluginPanelItem& item)
{
	SetFile^ f = gcnew SetFile;

	f->Name = gcnew String(item.FindData.lpwszFileName);
	f->AlternateName = gcnew String(item.FindData.lpwszAlternateFileName);
	f->Description = gcnew String(item.Description);
	f->Owner = gcnew String(item.Owner);

	f->Attributes = (FileAttributes)item.FindData.dwFileAttributes;
	f->CreationTime = FileTimeToDateTime(item.FindData.ftCreationTime);
	f->LastAccessTime = FileTimeToDateTime(item.FindData.ftLastAccessTime);
	f->LastWriteTime = FileTimeToDateTime(item.FindData.ftLastWriteTime);
	f->Length = item.FindData.nFileSize;

	return f;
}

bool FarPanel::ShowHidden::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (pi.Flags & PFLAGS_SHOWHIDDEN) != 0;
}

bool FarPanel::Highlight::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (pi.Flags & PFLAGS_HIGHLIGHT) != 0;
}

bool FarPanel::ReverseSortOrder::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (pi.Flags & PFLAGS_REVERSESORTORDER) != 0;
}

void FarPanel::ReverseSortOrder::set(bool value)
{
	Info.Control(_handle, FCTL_SETSORTORDER, (int)value, NULL);
}

bool FarPanel::UseSortGroups::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (pi.Flags & PFLAGS_USESORTGROUPS) != 0;
}

bool FarPanel::SelectedFirst::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (pi.Flags & PFLAGS_SELECTEDFIRST) != 0;
}

bool FarPanel::NumericSort::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (pi.Flags & PFLAGS_NUMERICSORT) != 0;
}

void FarPanel::NumericSort::set(bool value)
{
	Info.Control(_handle, FCTL_SETNUMERICSORT, (int)value, NULL);
}

bool FarPanel::RealNames::get()
{
	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	return (pi.Flags & PFLAGS_REALNAMES) != 0;
}

void FarPanel::Close()
{
	Info.Control(_handle, FCTL_CLOSEPLUGIN, 0, NULL);
}

void FarPanel::Close(String^ path)
{
	PIN_NE(pin, path);
	Info.Control(_handle, FCTL_CLOSEPLUGIN, 0, (LONG_PTR)(const wchar_t*)pin);
}

void FarPanel::GoToName(String^ name)
{
	if (!name)
		throw gcnew ArgumentNullException("name");

	// well, empty names are technically possible, but it is weird, ignore this
	if (name->Length == 0)
		return;

	PanelInfo pi;
	GetPanelInfo(_handle, pi);

	PIN_NE(pin, name);
	for(int i = 0; i < pi.ItemsNumber; ++i)
	{
		AutoPluginPanelItem item(_handle, i, false);
		if (Info.FSF->LStricmp(pin, item.Get().FindData.lpwszFileName) == 0 || Info.FSF->LStricmp(pin, item.Get().FindData.lpwszAlternateFileName) == 0)
		{
			Redraw(i, 0);
			break;
		}
	}
}

void FarPanel::GoToPath(String^ path)
{
	if (!path)
		throw gcnew ArgumentNullException("path");

	//! can be nullptr, e.g. for '\'
	String^ dir =  IO::Path::GetDirectoryName(path);
	if (!dir && (path->StartsWith("\\") || path->StartsWith("/")))
		dir = "\\";
	if (dir && dir->Length)
	{
		Path = dir;
		Redraw();
	}

	String^ name = IO::Path::GetFileName(path);
	GoToName(name);
}

void FarPanel::Redraw()
{
	Info.Control(_handle, FCTL_REDRAWPANEL, 0, NULL);
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
	Info.Control(_handle, FCTL_REDRAWPANEL, 0, (LONG_PTR)&pri);
}

void FarPanel::Update(bool keepSelection)
{
	Info.Control(_handle, FCTL_UPDATEPANEL, keepSelection, NULL);
}

#pragma endregion

#pragma region FarPluginPanel

FarPluginPanel::FarPluginPanel()
: FarPanel(true)
, _files(gcnew List<FarFile^>)
{
	_StartDirectory = Environment::CurrentDirectory;
}

void FarPluginPanel::AssertOpen()
{
	if (Index <= 0)
		throw gcnew InvalidOperationException("Expected opened plugin panel.");
}

/*
??? or works?
?? It works only for panels that have the current mode defined,
because Far does not provide this info and we do not want to hack
Far:\Panel\ViewModes\ModeX, though it should work, more likely.
For now we just do nothing for not defined modes.
To submit a wish?
*/
void FarPluginPanel::SwitchFullScreen()
{
	// get
	PanelViewMode iViewMode = ViewMode;
	PanelModeInfo^ mode = Info->GetMode(iViewMode);
	if (!mode)
	{
		mode = gcnew PanelModeInfo;
		{
			int size = ::Info.Control(Handle, FCTL_GETCOLUMNTYPES, 0, NULL);
			CBox buf(size);
			::Info.Control(Handle, FCTL_GETCOLUMNTYPES, size, (LONG_PTR)(wchar_t*)buf);
			mode->ColumnTypes = gcnew String(buf);
		}
		{
			int size = ::Info.Control(Handle, FCTL_GETCOLUMNWIDTHS, 0, NULL);
			CBox buf(size);
			::Info.Control(Handle, FCTL_GETCOLUMNWIDTHS, size, (LONG_PTR)(wchar_t*)buf);
			mode->ColumnWidths = gcnew String(buf);
		}
		array<String^>^ types = mode->ColumnTypes->Split(',');
		array<String^>^ widths = mode->ColumnWidths->Split(',');
		String^ w = String::Empty;
		for(int iType = 0; iType < types->Length; ++iType)
		{
			String^ w1;
			if (types[iType] == "N" || types[iType] == "Z" || types[iType] == "O")
				w1 = "0";
			else
				w1 = widths[iType];
			if (iType == 0)
				w = w1;
			else
				w = "," + w1;
			mode->ColumnWidths = w;
		}
	}

	// switch
	mode->IsFullScreen = !mode->IsFullScreen;

	// set
	Info->SetMode(iViewMode, mode);
	Redraw();
}

List<FarFile^>^ FarPluginPanel::ReplaceFiles(List<FarFile^>^ files)
{
	List<FarFile^>^ r = _files;
	_files = files;
	return r;
}

bool FarPluginPanel::IsOpened::get()
{
	return Index > 0;
}

IList<FarFile^>^ FarPluginPanel::Files::get()
{
	return _files;
}

bool FarPluginPanel::IsPlugin::get()
{
	return true;
}

Guid FarPluginPanel::Id::get()
{
	return _Id;
}

void FarPluginPanel::Id::set(Guid value)
{
	if (_Id != Guid::Empty)
		throw gcnew InvalidOperationException("Id cannot be set twice.");

	_Id = value;
}

//! see remark for FarPanel::CurrentFile::get()
FarFile^ FarPluginPanel::CurrentFile::get()
{
	AssertOpen();

	PanelInfo pi;
	GetPanelInfo(Handle, pi);

	if (pi.ItemsNumber == 0)
		return nullptr;

	AutoPluginPanelItem item(Handle, pi.CurrentItem, false);
	int fi = (int)(INT_PTR)item.Get().UserData;
	if (fi < 0)
		return nullptr;

	// 090411 Extra sanity test and watch.
	// See State::GetPanelInfo - this approach fixes the problem, but let's watch for a while.
	if (fi >= _files->Count)
	{
		assert(0);
		return nullptr;
	}

	return _files[fi];
}

IList<FarFile^>^ FarPluginPanel::ShownFiles::get()
{
	AssertOpen();

	PanelInfo pi;
	GetPanelInfo(Handle, pi);

	List<FarFile^>^ r = gcnew List<FarFile^>(pi.ItemsNumber);
	for(int i = 0; i < pi.ItemsNumber; ++i)
	{
		AutoPluginPanelItem item(Handle, i, false);
		int fi = (int)(INT_PTR)item.Get().UserData;
		if (fi >= 0)
			r->Add(_files[fi]);
	}

	return r;
}

IList<FarFile^>^ FarPluginPanel::SelectedFiles::get()
{
	AssertOpen();

	PanelInfo pi;
	GetPanelInfo(Handle, pi);

	List<FarFile^>^ r = gcnew List<FarFile^>(pi.SelectedItemsNumber);
	for(int i = 0; i < pi.SelectedItemsNumber; ++i)
	{
		AutoPluginPanelItem item(Handle, i, true);
		int fi = (int)(INT_PTR)item.Get().UserData;
		if (fi >= 0)
			r->Add(_files[fi]);
	}

	return r;
}

String^ FarPluginPanel::Path::get()
{
	return _info.CurrentDirectory;
}

void FarPluginPanel::Path::set(String^ value)
{
	if (value == nullptr)
		throw gcnew ArgumentNullException("value");

	if (!_SettingDirectory)
	{
		if (!Directory::Exists(value))
			throw gcnew ArgumentException("Directory '" + value + "' does not exist.");
		Close(value);
		return;
	}

	SettingDirectoryEventArgs e(value, OperationModes::None);
	_SettingDirectory(this, %e);
	if (!e.Ignore)
	{
		Update(false);
		Redraw();
	}
}

String^ FarPluginPanel::StartDirectory::get()
{
	return _StartDirectory;
}

void FarPluginPanel::StartDirectory::set(String^ value)
{
	_StartDirectory = value;
}

IPluginPanel^ FarPluginPanel::AnotherPanel::get()
{
	return PanelSet::GetPluginPanel2(this);
}

void FarPluginPanel::Open(IPluginPanel^ oldPanel)
{
	if (!oldPanel)
		throw gcnew ArgumentNullException("oldPanel");

	PanelSet::ReplacePluginPanel((FarPluginPanel^)oldPanel, this);
}

void FarPluginPanel::Open()
{
	if (Index > 0)
		throw gcnew InvalidOperationException("Cannot open the panel because it is already opened.");

	PanelSet::OpenPluginPanel(this);
	if (_IsPushed)
	{
		PanelSet::_stack.Remove(this);
		_skipGettingData = true;
		_IsPushed = false;
	}
}

void FarPluginPanel::Push()
{
	PanelSet::PushPluginPanel(this);
}

#pragma endregion
}
