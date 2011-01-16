/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "Panel0.h"
#include "Panel2.h"
#include "Shelve.h"
#include "Wrappers.h"

namespace FarNet
{;
// lame storage of new name
static CStr s_makingDirectory;

static List<FarFile^>^ ItemsToFiles(IList<FarFile^>^ files, List<String^>^ names, PluginPanelItem* panelItem, int itemsNumber)
{
	List<FarFile^>^ r = gcnew List<FarFile^>(itemsNumber);

	//? Far bug: alone dots has UserData = 0 no matter what was written there; so check the dots name
	if (itemsNumber == 1 && panelItem[0].UserData == 0 && wcscmp(panelItem[0].FindData.lpwszFileName, L"..") == 0)
		return r;

	for(int i = 0; i < itemsNumber; ++i)
	{
		int fi = (int)(INT_PTR)panelItem[i].UserData;
		if (fi >= 0)
		{
			r->Add(files[fi]);
			if (names)
				names->Add(gcnew String(panelItem[i].FindData.lpwszAlternateFileName));
		}
	}
	return r;
}

void Panel0::AsClosePlugin(HANDLE hPlugin)
{
	Log::Source->TraceInformation("ClosePlugin");

	// drop the lame storage
	s_makingDirectory.Set(nullptr);

	Panel2^ pp = _panels[(int)(INT_PTR)hPlugin];
	pp->_info.Free();
	_panels[(int)(INT_PTR)hPlugin] = nullptr;
	if (!pp->_Pushed && pp->_Closed)
		pp->_Closed(pp, nullptr);
}

int Panel0::AsDeleteFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int opMode)
{
	Log::Source->TraceInformation("DeleteFiles");

	Panel2^ pp = _panels[(int)(INT_PTR)hPlugin];
	if (!pp->_DeletingFiles)
		return false;

	IList<FarFile^>^ files = ItemsToFiles(pp->Files, nullptr, panelItem, itemsNumber);
	FilesEventArgs e(files, (OperationModes)opMode, false);
	pp->_DeletingFiles(pp, %e);

	return e.Ignore ? false : true;
}

void Panel0::AsFreeFindData(HANDLE /*hPlugin*/, PluginPanelItem* panelItem, int itemsNumber)
{
	Log::Source->TraceInformation("FreeFindData");

	for(int i = itemsNumber; --i >= 0;)
	{
		PluginPanelItem& item = panelItem[i];

		delete[] item.Owner;
		delete[] item.Description;
		delete[] item.FindData.lpwszAlternateFileName;
		delete[] item.FindData.lpwszFileName;

		if (item.CustomColumnData)
		{
			for(int j = item.CustomColumnNumber; --j >= 0;)
				delete[] item.CustomColumnData[j];

			delete[] item.CustomColumnData;
		}
	}

	delete[] panelItem;
}

//?? Parameter destPath can be changed, i.e. (*destPath) replaced. NYI here.
int Panel0::AsGetFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, const wchar_t** destPath, int opMode)
{
	Log::Source->TraceInformation("GetFiles");

	Panel2^ pp = _panels[(int)(INT_PTR)hPlugin];
	if (!pp->_GettingFiles)
		return 0;

	List<String^>^ names = pp->Info->AutoAlternateNames ? gcnew List<String^> : nullptr;
	List<FarFile^>^ files = ItemsToFiles(pp->Files, names, panelItem, itemsNumber);
	GettingFilesEventArgs e(files, names, (OperationModes)opMode, move != 0, gcnew String((*destPath)));
	pp->_GettingFiles(pp, %e);

	return e.Ignore ? false : true;
}

//! 090712. Allocation by chunks was originally used. But it turns out it does not improve
//! performance much (tested for 200000+ files). On the other hand allocation of large chunks
//! may fail due to memory fragmentation more frequently.
int Panel0::AsGetFindData(HANDLE hPlugin, PluginPanelItem** pPanelItem, int* pItemsNumber, int opMode)
{
	Log::Source->TraceInformation("GetFindData");

	try
	{
		Panel2^ pp = _panels[(int)(INT_PTR)hPlugin];

		// fake empty panel needed on switching modes, for example
		if (pp->_voidGettingData)
		{
			(*pItemsNumber) = 0;
			(*pPanelItem) = NULL;
			return true;
		}

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

		// alloc all
		(*pPanelItem) = new PluginPanelItem[nItem];
		memset((*pPanelItem), 0, nItem * sizeof(PluginPanelItem));

		// add dots
		int i = -1, fi = -1;
		if (pp->AddDots)
		{
			++i;
			wchar_t* dots = new wchar_t[3];
			dots[0] = dots[1] = '.'; dots[2] = '\0';
			PluginPanelItem& p = (*pPanelItem)[0];
			p.UserData = (DWORD_PTR)(-1);
			p.FindData.lpwszFileName = dots;
			p.Description = NewChars(pp->DotsDescription);
		}

		// add files
		for each(FarFile^ f in pp->Files)
		{
			++i;
			++fi;

			PluginPanelItem& p = (*pPanelItem)[i];
			FAR_FIND_DATA& d = p.FindData;

			// names
			d.lpwszFileName = NewChars(f->Name);
			p.Description = NewChars(f->Description);
			p.Owner = NewChars(f->Owner);

			// alternate name is special
			if (pp->Info->AutoAlternateNames && opMode == 0)
			{
				wchar_t buf[12]; // 12: 10=len(0xffffffff=4294967295) + 1=sign + 1=\0
				Info.FSF->itoa(i, buf, 10);
				int size = (int)wcslen(buf) + 1;
				wchar_t* alternate = new wchar_t[size];
				memcpy(alternate, buf, size * sizeof(wchar_t));
				d.lpwszAlternateFileName = alternate;
			}
			else
			{
				d.lpwszAlternateFileName = NewChars(f->AlternateName);
			}

			// other
			d.dwFileAttributes = (DWORD)f->Attributes;
			d.nFileSize = f->Length;
			d.ftCreationTime = DateTimeToFileTime(f->CreationTime);
			d.ftLastWriteTime = DateTimeToFileTime(f->LastWriteTime);
			d.ftLastAccessTime = DateTimeToFileTime(f->LastAccessTime);
			p.UserData = fi;

			// columns
			System::Collections::ICollection^ columns = f->Columns;
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
		return true;
	}
	catch(Exception^ e)
	{
		//?? .. else log error?
		if ((opMode & (OPM_FIND | OPM_SILENT)) == 0)
			Far::Net->ShowError(__FUNCTION__, e);

		return false;
	}
}

void Panel0::AsGetOpenPluginInfo(HANDLE hPlugin, OpenPluginInfo* info)
{
	//! it is called too often, lets use mode 4
	Log::Source->TraceEvent(TraceEventType::Verbose, 0, "GetOpenPluginInfo");

	// plugin panel
	Panel2^ pp = _panels[(int)(INT_PTR)hPlugin];

	//! pushed case
	//?? _091015_190130 Far calls this after Close(), perhaps a bug. How: push folder tree panel.
	if (pp->IsPushed)
	{
		info->StructSize = sizeof(OpenPluginInfo);
		return;
	}

	// trigger - allow to update info before making it for Far
	if (pp->_GettingInfo && !State::GetPanelInfo)
	{
		Log::Source->TraceEvent(TraceEventType::Verbose, 0, "GettingInfo");
		pp->_GettingInfo(pp, nullptr);
	}

	// make info
	*info = pp->_info.Make();
}

// Return values are 0, 1, -1. If 0 is returned Far shows a message "Cannot create".
// We do not want this. http://forum.farmanager.com/viewtopic.php?p=56846#p56846
int Panel0::AsMakeDirectory(HANDLE hPlugin, const wchar_t** name, int opMode)
{
	Log::Source->TraceInformation("MakeDirectory");

	// handler
	Panel2^ pp = _panels[(int)(INT_PTR)hPlugin];
	if (!pp->_MakingDirectory)
		return -1;

	// call
	String^ nameIn = gcnew String((*name));
	MakingDirectoryEventArgs e(nameIn, (OperationModes)opMode);
	pp->_MakingDirectory(pp, %e);
	if (e.Ignore)
		return -1;

	// return a new name
	if (0 == (opMode & OPM_SILENT) && e.Name != nameIn)
	{
		// use the lame storage
		s_makingDirectory.Set(e.Name);
		*name = s_makingDirectory;
	}

	// done
	return 1;
}

static bool _reenterOnRedrawing;
int Panel0::AsProcessEvent(HANDLE hPlugin, int id, void* param)
{
	Panel2^ pp = _panels[(int)(INT_PTR)hPlugin];
	switch(id)
	{
	case FE_BREAK:
		{
			Log::Source->TraceInformation("FE_BREAK");
			
			if (pp->_CtrlBreakPressed)
			{
				Log::Source->TraceInformation("CtrlBreakPressed");
				pp->_CtrlBreakPressed(pp, nullptr);
			}
		}
		break;
	case FE_CLOSE:
		{
			Log::Source->TraceInformation("FE_CLOSE");
			
			//? FE_CLOSE issues:
			// *) Bug [_090321_165608]: unwanted extra call on plugin commands entered in command line
			// http://bugs.farmanager.com/view.php?id=602
			// *) may not be called at all e.g. if tmp panel is opened
			if (!pp->_Pushed && pp->_Closing)
			{
				Log::Source->TraceInformation("Closing");
				PanelEventArgs e(OperationModes::None);
				pp->_Closing(pp, %e);
				return e.Ignore;
			}
		}
		break;
	case FE_COMMAND:
		{
			Log::Source->TraceInformation("FE_COMMAND");

			if (pp->_Executing)
			{
				Log::Source->TraceInformation("Executing");
				//! We have to try\catch in here in order to return exactly what plugin returns.
				ExecutingEventArgs e(gcnew String((const wchar_t*)param));
				try
				{
					pp->_Executing(pp, %e);
				}
				catch(Exception^ exception)
				{
					Far::Net->ShowError("Event: Executing", exception);
				}
				return e.Ignore;
			}
		}
		break;
	case FE_CHANGEVIEWMODE:
		{
			Log::Source->TraceInformation("FE_CHANGEVIEWMODE");

			if (pp->_ViewModeChanged)
			{
				Log::Source->TraceInformation("ViewModeChanged");
				ViewModeChangedEventArgs e(gcnew String((const wchar_t*)param));
				pp->_ViewModeChanged(pp, %e);
			}
		}
		break;
	case FE_IDLE:
		{
			Log::Source->TraceEvent(TraceEventType::Verbose, 0, "FE_IDLE");

			// 1) call the handler
			if (pp->_Idled)
			{
				Log::Source->TraceEvent(TraceEventType::Verbose, 0, "Idled");
				pp->_Idled(pp, nullptr);
			}

			// 2) update after the handler: if the panel has set both IdleUpdate and Idled
			// then in Idled it should not care of data updates, it is done after that.
			if (pp->IdleUpdate)
			{
				pp->Update(true);
				pp->Redraw();
			}
		}
		break;
	case FE_GOTFOCUS:
		{
			Log::Source->TraceEvent(TraceEventType::Verbose, 0, "FE_GOTFOCUS");

			if (pp->_GotFocus)
			{
				Log::Source->TraceEvent(TraceEventType::Verbose, 0, "GotFocus");
				pp->_GotFocus(pp, nullptr);
			}
		}
		break;
	case FE_KILLFOCUS:
		{
			Log::Source->TraceEvent(TraceEventType::Verbose, 0, "FE_KILLFOCUS");

			if (pp->_LosingFocus)
			{
				Log::Source->TraceEvent(TraceEventType::Verbose, 0, "LosingFocus");
				pp->_LosingFocus(pp, nullptr);
			}
		}
		break;
	case FE_REDRAW:
		{
			Log::Source->TraceEvent(TraceEventType::Verbose, 0, "FE_REDRAW");

			// 090411 Data are shown now. Drop this flag to allow normal processing.
			pp->_skipGettingData = false;

			// 090811 Internal work is in progress, do nothing
			if (pp->_voidGettingData)
				return false;

			if (_reenterOnRedrawing)
			{
				_reenterOnRedrawing = false;
				return false;
			}

			if (pp->_Redrawing)
			{
				Log::Source->TraceEvent(TraceEventType::Verbose, 0, "Redrawing");
				PanelEventArgs e(OperationModes::None);
				pp->_Redrawing(pp, %e);
				if (e.Ignore)
					return true;
			}

			int r = false;

			// post selection
			if (pp->_postSelected)
			{
				array<int>^ selected = pp->_postSelected;
				pp->_postSelected = nullptr;
				pp->SelectAt(selected);
			}

			// case: use data matcher
			if (pp->DataId && (pp->_postData || pp->_postFile && pp->_postFile->Data))
			{
				Object^ data = pp->_postData ? pp->_postData : pp->_postFile->Data;
				Object^ dataId = pp->DataId(data);
				
				pp->_postFile = nullptr;
				pp->_postData = nullptr;
				pp->_postName = nullptr;

				if (dataId)
				{
					int i = pp->AddDots ? 0 : -1;
					for each (FarFile^ f in pp->ShownFiles)
					{
						++i;
						if (dataId->Equals(pp->DataId(f->Data)))
						{
							_reenterOnRedrawing = true;
							pp->Redraw(i, -1);
							r = true;
							break;
						}
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
	}
	return false;
}

/*
#define INTERNAL_KEY_BASE_2 0x00030000
KEY_NONE=INTERNAL_KEY_BASE_2+1,
KEY_KILLFOCUS=INTERNAL_KEY_BASE_2+6,
KEY_GOTFOCUS
*/
int Panel0::AsProcessKey(HANDLE hPlugin, int key, unsigned int controlState)
{
	// extract the key code
	int code = key & ~PKF_PREPROCESS;
	
	// filter out not keys but kind of events (perhaps to make events later)
	if (code >= Wrap::GetEndKeyCode())
		return false;
	
	//! mind rare case: plugin in null, e.g. closed by [AltF12] + select folder
	Panel2^ pp = _panels[(int)(INT_PTR)hPlugin];
	if (!pp)
		return false;

	// preprocessing
	if ((key & PKF_PREPROCESS) != 0)
	{
		// panel has no handler:
		if (!pp->_KeyPressing)
			return false;

		// trigger the handler
		PanelKeyEventArgs e(code, (KeyStates)controlState);
		Log::Source->TraceEvent(TraceEventType::Verbose, 0, "KeyPressing {0}", %e);
		pp->_KeyPressing(pp, %e);
		return e.Ignore;
	}

	// Escaping handler
	if (pp->_Escaping)
	{
		// [Escape] is pressed:
		if (VK_ESCAPE == code && 0 == controlState)
		{
			// if cmdline is not empty then do nothing
			int size = Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINE, 0, 0);
			if (size > 1)
				return false;

			// trigger the handler
			PanelEventArgs e;
			Log::Source->TraceEvent(TraceEventType::Verbose, 0, "Escaping {0}", %e);
			pp->_Escaping(pp, %e);
			if (e.Ignore)
				return true;
		}
	}

	// panel has no handler:
	if (!pp->_KeyPressed)
		return false;

	// trigger the handler
	PanelKeyEventArgs e(code, (KeyStates)controlState);
	Log::Source->TraceEvent(TraceEventType::Verbose, 0, "KeyPressed {0}", %e);
	pp->_KeyPressed(pp, %e);
	return e.Ignore;
}

int Panel0::AsPutFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, const wchar_t* srcPath, int opMode)
{
	Log::Source->TraceInformation("PutFiles");

	Panel2^ pp = _panels[(int)(INT_PTR)hPlugin];
	if (!pp->_PuttingFiles)
		return 0;

	Panel2^ plugin2 = GetPanel2(pp);
	List<FarFile^>^ files;
	if (plugin2)
	{
		files = ItemsToFiles(plugin2->Files, nullptr, panelItem, itemsNumber);
	}
	else
	{
		files = gcnew List<FarFile^>(itemsNumber);
		for(int i = 0; i < itemsNumber; ++i)
		{
			//! we can use transient 'NativeFile' in here, but 'transient' it is not that safe
			files->Add(Panel1::ItemToFile(panelItem[i]));
		}
	}

	PuttingFilesEventArgs e(files, (OperationModes)opMode, move != 0, (srcPath ? gcnew String(srcPath) : String::Empty));
	pp->_PuttingFiles(pp, %e);
	return e.Ignore ? false : true;
}

int Panel0::AsSetDirectory(HANDLE hPlugin, const wchar_t* dir, int opMode)
{
	Log::Source->TraceInformation("SetDirectory");

	_inAsSetDirectory = true;
	try
	{
		Panel2^ pp = _panels[(int)(INT_PTR)hPlugin];
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

Panel1^ Panel0::GetPanel(bool active)
{
	// get info and return null (e.g. Far started with /e or /v)
	PanelInfo pi;
	if (!TryPanelInfo((active ? PANEL_ACTIVE : PANEL_PASSIVE), pi))
		return nullptr;

	if (!pi.Plugin)
		return gcnew Panel1(active);

	for (int i = 1; i < cPanels; ++i)
	{
		Panel2^ p = _panels[i];
		if (p && p->IsActive == active)
			return p;
	}

	return gcnew Panel1(true);
}

Panel2^ Panel0::GetPanel(Guid typeId)
{
	for (int i = 1; i < cPanels; ++i)
	{
		Panel2^ p = _panels[i];
		if (p && p->TypeId == typeId)
			return p;
	}
	return nullptr;
}

Panel2^ Panel0::GetPanel(Type^ hostType)
{
	// case: any panel
	if (hostType == nullptr)
	{
		for (int i = 1; i < cPanels; ++i)
		{
			Panel2^ p = _panels[i];
			if (p)
				return p;
		}
		return nullptr;
	}

	// panel with defined host type
	for (int i = 1; i < cPanels; ++i)
	{
		Panel2^ p = _panels[i];
		if (p && p->Host)
		{
			Type^ type = p->Host->GetType();
			if (type == hostType || type->IsSubclassOf(hostType))
				return p;
		}
	}

	return nullptr;
}

Panel2^ Panel0::GetPanel2(Panel2^ plugin)
{
	for (int i = 1; i < cPanels; ++i)
	{
		Panel2^ p = _panels[i];
		if (p && p != plugin)
			return p;
	}
	return nullptr;
}

// [0] is for a waiting panel;
// [1-3] are 2 for already opened and 1 for being added
//? create a test case: open 2 panels and try to open 1 more
HANDLE Panel0::AddPluginPanel(Panel2^ plugin)
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
	throw gcnew InvalidOperationException("Cannot add a panel.");
}

// EndOpenMode() must be called after
void Panel0::BeginOpenMode()
{
	if (_openMode < 0)
		throw gcnew InvalidOperationException("Negative open mode.");

	++_openMode;
}

// BeginOpenMode() must be called before
void Panel0::EndOpenMode()
{
	if (_openMode <= 0)
		throw gcnew InvalidOperationException("Not positive open mode.");

	if (--_openMode == 0)
		_panels[0] = nullptr;
}

void Panel0::OpenPluginPanel(Panel2^ plugin)
{
	// plugin must be called for opening
	if (_openMode == 0)
		throw gcnew InvalidOperationException("Cannot open a panel because a module is not called for opening.");

	// only one panel can be opened at a time
	if (_panels[0] && _panels[0] != plugin)
		throw gcnew InvalidOperationException("Cannot open a panel because another panel is already waiting.");

	// panels window should be current
	try
	{
		Far::Net->Window->SetCurrentAt(0);
	}
	catch(InvalidOperationException^ e)
	{
		throw gcnew InvalidOperationException("Cannot open a panel because panels window cannot be set current.", e);
	}

	_panels[0] = plugin;
}

//! it call Update/Redraw in some cases
void Panel0::ReplacePluginPanel(Panel2^ oldPanel, Panel2^ newPanel)
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
	oldPanel->Info->StartReverseSortOrder = oldPanel->ReverseSortOrder;
	oldPanel->Info->StartSortMode = oldPanel->SortMode;
	oldPanel->Info->StartViewMode = oldPanel->ViewMode;

	// disconnect old panel
	oldPanel->Handle = 0;
	((FarPanelInfo^)oldPanel->Info)->Free();

	// connect new panel
	_panels[id1] = newPanel;
	newPanel->Index = id1;

	// change panel modes
	if (newPanel->Info->StartViewMode != PanelViewMode::Undefined &&
		newPanel->Info->StartViewMode != oldPanel->Info->StartViewMode ||
		newPanel->Info->StartSortMode != PanelSortMode::Default && (
		newPanel->Info->StartSortMode != oldPanel->Info->StartSortMode ||
		newPanel->Info->StartReverseSortOrder != oldPanel->Info->StartReverseSortOrder))
	{
		// set void mode for switching panel modes
		newPanel->_voidGettingData = true;
		newPanel->Update(false);

		// set only new modes
		if (newPanel->Info->StartViewMode != PanelViewMode::Undefined && newPanel->Info->StartViewMode != oldPanel->Info->StartViewMode)
			newPanel->ViewMode = newPanel->Info->StartViewMode;
		if (newPanel->Info->StartSortMode != PanelSortMode::Default)
		{
			if (newPanel->Info->StartSortMode != oldPanel->Info->StartSortMode)
				newPanel->SortMode = newPanel->Info->StartSortMode;
			if (newPanel->Info->StartReverseSortOrder != oldPanel->Info->StartReverseSortOrder)
				newPanel->ReverseSortOrder = newPanel->Info->StartReverseSortOrder;
		}

		// drop void mode
		newPanel->_voidGettingData = false;
	}

	//! switch to new data and redraw, but not always: in some cases it will be done anyway, e.g. by Far
	if (!_inAsSetDirectory)
	{
		newPanel->Update(false);
		newPanel->Redraw(0, 0);
	}
}

void Panel0::PushPluginPanel(Panel2^ plugin)
{
	if (plugin->_Pushed)
		throw gcnew InvalidOperationException("Cannot push the panel because it is already pushed.");

	//! save current state effectively by Far API, not FarNet
	PanelInfo pi;
	GetPanelInfo(plugin->Handle, pi);

	// save modes
	plugin->_info.StartReverseSortOrder = (pi.Flags & PFLAGS_REVERSESORTORDER) != 0;
	plugin->_info.StartSortMode = (PanelSortMode)pi.SortMode;
	plugin->_info.StartViewMode = (PanelViewMode)pi.ViewMode;

	// current
	FarFile^ file = nullptr;
	if (pi.ItemsNumber > 0)
	{
		AutoPluginPanelItem item(plugin->Handle, pi.CurrentItem, ShownFile);
		int index = (int)item.Get().UserData;
		if (index >= 0 && index < plugin->Files->Count)
			file = plugin->Files[index];
	}

	// push
	plugin->_Pushed = gcnew ShelveInfoPlugin(plugin);
	Works::ShelveInfo::Stack->Add(plugin->_Pushed);

	// reset position, close
	// 090411 Was: Redraw(0, 0) + Close(). New way looks more effective and perhaps avoids some Far->FarNet calls.
	plugin->Close(".");
	if (file)
		plugin->PostFile(file);

	// drop handle
	plugin->Handle = 0;
}

void Panel0::ShelvePanel(Panel1^ panel, bool modes)
{
	Works::ShelveInfo::Stack->Add(gcnew ShelveInfoPanel(panel, modes));
}
}
