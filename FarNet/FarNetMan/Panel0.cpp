
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

#include "StdAfx.h"
#include "Panel0.h"
#include "Panel2.h"
#include "Shelve.h"
#include "Wrappers.h"

namespace FarNet
{;
// This is the very first called method
int Panel0::AsGetFindData(GetFindDataInfo* info)
{
	Panel2^ pp = HandleToPanel(info->hPanel);
	return pp->AsGetFindData(info);
}

void Panel0::AsFreeFindData(const FreeFindDataInfo* info)
{
	Panel2^ pp = HandleToPanel(info->hPanel); //???? need? can it be static managed by Address -> Data map including Panel2^
	Log::Source->TraceInformation("FreeFindDataW Address='{0:x}' Location='{1}'", (long)info->PanelItem, pp->CurrentLocation);

	for(int i = (int)info->ItemsNumber; --i >= 0;)
	{
		PluginPanelItem& item = info->PanelItem[i];

		delete[] item.Owner;
		delete[] item.Description;
		delete[] item.AlternateFileName;
		delete[] item.FileName;

		if (item.CustomColumnData)
		{
			for(int j = (int)item.CustomColumnNumber; --j >= 0;)
				delete[] item.CustomColumnData[j];

			delete[] item.CustomColumnData;
		}
	}

	delete[] info->PanelItem;
}

int Panel0::AsSetDirectory(const SetDirectoryInfo* info)
{
	Panel2^ pp = HandleToPanel(info->hPanel);
	_inAsSetDirectory = true;
	try
	{
		return pp->AsSetDirectory(info);
	}
	finally
	{
		_inAsSetDirectory = false;
	}
}

/*
?? NYI: Parameter destPath can be changed, i.e. (*destPath) replaced.
?? NYI: Not used return value -1 (stopped by a user).

It is called on F5/F6 when a target panel is a native plugin or an explorer
cannot export files but can get content and a target is a native file panel
(FarNet targets accept files themselves).
*/
int Panel0::AsGetFiles(GetFilesInfo* info)
{
	info->StructSize = sizeof(*info);

	Panel2^ pp = HandleToPanel(info->hPanel);
	ExplorerModes mode = (ExplorerModes)info->OpMode;

	Log::Source->TraceInformation("GetFilesW Mode='{0}'", mode);

	Explorer^ explorer = pp->Host->Explorer;
	if (!explorer->CanGetContent)
		return 0;

	// modes
	const bool qview = 0 != int(mode & ExplorerModes::QuickView);
	const bool silent = int(mode & ExplorerModes::Silent);

	// process bad names? - do not if silent or the target is plugin
	const bool processBadNames = qview || (!silent && !Far::Api->Panel2->IsPlugin);

	// delete files on move?
	const bool deleteFiles = info->Move && explorer->CanDeleteFiles;
	List<FarFile^>^ filesToDelete;
	if (deleteFiles)
		filesToDelete = gcnew List<FarFile^>;

	// collect files
	List<String^>^ names = qview ? gcnew List<String^> : nullptr;
	List<FarFile^>^ files = pp->ItemsToFiles(names, info->PanelItem, (int)info->ItemsNumber);

	// copy files
	String^ destination = gcnew String(info->DestPath);
	for(int i = 0; i < files->Count; ++i)
	{
		String^ fileName;
		if (qview)
		{
			// use the alias
			fileName = names[i];
		}
		else
		{
			// use the name, check/fix invalid
			fileName = files[i]->Name;
			if (Works::Kit::IsInvalidFileName(fileName))
			{
				if (!processBadNames)
					continue;

				fileName = Works::Kit::FixInvalidFileName(fileName);
				if (!fileName)
					continue;
			}
		}

		// export file
		fileName = Path::Combine(destination, fileName);
		GetContentEventArgs^ argsJob = Panel::WorksExportExplorerFile(explorer, pp->Host, mode, files[i], fileName);
		if (!argsJob || argsJob->Result != JobResult::Done)
			continue;

		// copy existing file
		if (SS(argsJob->UseFileName))
			File::Copy(argsJob->UseFileName, fileName, true);

		// collect to delete
		if (deleteFiles)
			filesToDelete->Add(files[i]);
	}

	// delete collected files
	if (deleteFiles && filesToDelete->Count > 0)
	{
		DeleteFilesEventArgs args(mode, filesToDelete, false);
		pp->Host->UIDeleteFiles(%args);
	}

	return 1;
}

//! It is called on [F5], [F6] only from a native/plugin panel to a module panel.
int Panel0::AsPutFiles(PutFilesInfo* info)
{
	info->StructSize = sizeof(*info);

	Panel2^ pp = HandleToPanel(info->hPanel);
	ExplorerModes mode = (ExplorerModes)info->OpMode;

	Log::Source->TraceInformation("PutFilesW Mode='{0}'", mode);

	if (!pp->Host->Explorer->CanImportFiles)
		return 0;

	List<FarFile^>^ files = gcnew List<FarFile^>((int)info->ItemsNumber);
	for(int i = 0; i < (int)info->ItemsNumber; ++i)
		files->Add(Panel1::ItemToFile(info->PanelItem[i]));

	ImportFilesEventArgs args(
		mode,
		files,
		info->Move != 0,
		info->SrcPath ? gcnew String(info->SrcPath) : String::Empty);

	pp->Host->UIImportFiles(%args);

	// done:
	if (args.Result == JobResult::Done)
		return 1;

	// failed:
	if (args.Result != JobResult::Incomplete || args.FilesToStay->Count == 0)
		return 0;

	// incomplete:

	// drop selection flags
	for(int i = (int)info->ItemsNumber; --i >= 0;)
		info->PanelItem[i].Flags &= ~PPIF_SELECTED;

	// restore selection flags
	for each(FarFile^ file in args.FilesToStay)
	{
		int index = args.Files->IndexOf(file);
		if (index >= 0 && index < (int)info->ItemsNumber)
			info->PanelItem[index].Flags |= PPIF_SELECTED;
	}

	return -1;
}

//! It is called on move, too? I.e. is Move = Copy + Delete?
int Panel0::AsDeleteFiles(const DeleteFilesInfo* info)
{
	Panel2^ pp = HandleToPanel(info->hPanel);
	ExplorerModes mode = (ExplorerModes)info->OpMode;

	Log::Source->TraceInformation("DeleteFilesW Mode='{0}'", mode);

	Explorer^ explorer = pp->Host->Explorer;
	if (!explorer->CanDeleteFiles)
		return 0;

	DeleteFilesEventArgs args(mode, pp->ItemsToFiles(nullptr, info->PanelItem, (int)info->ItemsNumber), false);
	pp->Host->UIDeleteFiles(%args);

	return args.Result == JobResult::Ignore ? 0 : 1;
}

void Panel0::AsClosePanel(const ClosePanelInfo* info)
{
	Log::Source->TraceInformation("ClosePluginW");

	// disconnect
	Panel2^ pp = HandleToPanel(info->hPanel);
	pp->Free();
	RemovePanel(info->hPanel);

	// done for pushed
	if (pp->_Pushed)
		return;

	// clean the whole panel stack
	for(Panel^ panel = pp->Host; panel; panel = panel->Parent)
	{
		try
		{
			panel->UIClosed();
		}
		catch(Exception^ ex)
		{
			Far::Api->ShowError("UIClosed", ex);
		}
	}
}

//! It is called too often, log Verbose
void Panel0::AsGetOpenPanelInfo(OpenPanelInfo* info)
{
	Log::Source->TraceEvent(TraceEventType::Verbose, 0, "GetOpenPluginInfoW");
	info->StructSize = sizeof(*info);

	// plugin panel
	Panel2^ pp = HandleToPanel(info->hPanel);

	//! pushed case
	//?? _091015_190130 Far calls this after Close(), perhaps a bug. How: push folder tree panel.
	if (pp->IsPushed)
		return;

	// trigger - to update info before making it for Far
	if (!State::GetPanelInfo)
	{
		Log::Source->TraceEvent(TraceEventType::Verbose, 0, "UpdateInfo");
		pp->Host->UIUpdateInfo();
	}

	// make info
	*info = pp->Make();
}

static bool _reenterOnRedrawing;
int Panel0::AsProcessPanelEvent(const ProcessPanelEventInfo* info)
{
	Panel2^ pp = HandleToPanel(info->hPanel);
	switch(info->Event)
	{
	case FE_BREAK:
		{
			Log::Source->TraceInformation("FE_BREAK");

			pp->Host->UICtrlBreak();
		}
		break;
	case FE_CLOSE:
		{
			Log::Source->TraceInformation("FE_CLOSE");

			//_090321_165608 FE_CLOSE issues
			if (!pp->_Pushed)
			{
				PanelEventArgs e;
				pp->Host->UIClosing(%e);
				return e.Ignore;
			}
		}
		break;
	case FE_COMMAND:
		{
			Log::Source->TraceInformation("FE_COMMAND");

			if (pp->Host->WorksInvokingCommand(nullptr))
			{
				CommandLineEventArgs e(gcnew String((const wchar_t*)info->Param));
				Log::Source->TraceInformation("InvokingCommand: {0}", e.Command);

				//! Try\catch in order to return exactly what the module returns.
				try
				{
					// call
					pp->Host->WorksInvokingCommand(%e);

					// clear the command line
					if (e.Ignore)
						Far::Api->CommandLine->Text = String::Empty;
				}
				catch(Exception^ exception)
				{
					Far::Api->ShowError("Event: Executing", exception);
				}

				return e.Ignore ? 1 : 0;
			}
		}
		break;
	case FE_CHANGEVIEWMODE:
		{
			Log::Source->TraceInformation("FE_CHANGEVIEWMODE");

			ViewChangedEventArgs e(gcnew String((const wchar_t*)info->Param));
			pp->Host->UIViewChanged(%e);
		}
		break;
	case FE_IDLE:
		{
			Log::Source->TraceEvent(TraceEventType::Verbose, 0, "FE_IDLE");

			// 1) call
			pp->Host->UIIdle();

			// 2) update after the handler: if the panel has set both IdleUpdate and Idled
			// then in Idled it should not care of data updates, it is done after that.
			if (pp->Host->IdleUpdate)
			{
				pp->Update(true);
				pp->Redraw();
			}
		}
		break;
	case FE_GOTFOCUS:
		{
			Log::Source->TraceEvent(TraceEventType::Verbose, 0, "FE_GOTFOCUS");

			pp->Host->UIGotFocus();
		}
		break;
	case FE_KILLFOCUS:
		{
			Log::Source->TraceEvent(TraceEventType::Verbose, 0, "FE_KILLFOCUS");

			pp->Host->UILosingFocus();
		}
		break;
	case FE_REDRAW:
		{
			Log::Source->TraceEvent(TraceEventType::Verbose, 0, "FE_REDRAW");

			// 090411 Data are shown now. Drop this flag to allow normal processing.
			pp->_skipUpdateFiles = false;

			// 090811 Internal work is in progress, do nothing
			if (pp->_voidUpdateFiles)
				return 0;

			if (_reenterOnRedrawing)
			{
				_reenterOnRedrawing = false;
				return 0;
			}

			PanelEventArgs e;
			pp->Host->UIRedrawing(%e);
			if (e.Ignore)
				return 1;

			// post selection
			if (pp->_postSelected)
			{
				array<int>^ selected = pp->_postSelected;
				pp->_postSelected = nullptr;
				pp->SelectAt(selected);
			}

			// case 1: find posted data
			if (pp->_postData)
			{
				Object^ data = pp->_postData;

				pp->_postData = nullptr;
				pp->_postFile = nullptr;
				pp->_postName = nullptr;

				IList<FarFile^>^ files = pp->ShownList;
				for(int n = files->Count, i = pp->HasDots ? 1 : 0; i < n; ++i)
				{
					if (data == files[i]->Data)
					{
						_reenterOnRedrawing = true;
						pp->Redraw(i, -1);
						return 1;
					}
				}

				return 0;
			}

			// case 2: find posted name
			if (pp->_postName)
			{
				String^ name = pp->_postName;

				pp->_postFile = nullptr;
				pp->_postName = nullptr;

				IList<FarFile^>^ files = pp->ShownList;
				for(int n = files->Count, i = pp->HasDots ? 1 : 0; i < n; ++i)
				{
					if (name == files[i]->Name)
					{
						_reenterOnRedrawing = true;
						pp->Redraw(i, -1);
						return 1;
					}
				}

				return 0;
			}

			// else: find posted file
			if (!pp->_postFile)
				return 0;

			IEqualityComparer<FarFile^>^ comparer = pp->Host->Explorer->FileComparer;
			FarFile^ file = pp->_postFile;
			pp->_postFile = nullptr;

			IList<FarFile^>^ files = pp->ShownList;
			for(int n = files->Count, i = pp->HasDots ? 1 : 0; i < n; ++i)
			{
				if (comparer->Equals(file, files[i]))
				{
					_reenterOnRedrawing = true;
					pp->Redraw(i, -1);
					return 1;
				}
			}

			return 0;
		}
		break;
	}
	return 0;
}

int Panel0::AsProcessPanelInput(const ProcessPanelInputInfo* info)
{
	// ignore not key events
	const INPUT_RECORD& ir = info->Rec;
	if (ir.EventType != KEY_EVENT)
		return 0;

	//! mind rare case: panel is null: closed by [AltF12] + select folder
	Panel2^ pp = HandleToPanel(info->hPanel);
	if (!pp)
		return 0;

	// args, log
	KeyEventArgs e(KeyInfoFromInputRecord(ir));
	Log::Source->TraceEvent(TraceEventType::Information, 0, "KeyPressed {0}", %e);

	// 1. event; handlers work first of all
	pp->Host->WorksKeyPressed(%e);
	if (e.Ignore)
		return 1;

	// 2. method; default or custom virtual methods
	if (pp->Host->UIKeyPressed(e.Key))
		return 1;

	// 3. escape; special not yet handled case
	if ((e.Key->Is(KeyCode::Escape) || e.Key->IsShift(KeyCode::Escape)) && Far::Api->CommandLine->Length == 0)
	{
		pp->Host->WorksEscaping(%e);
		if (e.Ignore)
			return 1;

		pp->Host->UIEscape(e.Key->IsShift());
		return 1;
	}

	// CtrlR
	if (e.Key->IsCtrl(KeyCode::R))
		pp->Host->NeedsNewFiles = true;

	return 0;
}

IPanel^ Panel0::GetPanel(bool active)
{
	// get info and return null (e.g. Far started with /e or /v)
	PanelInfo pi;
	if (!TryPanelInfo((active ? PANEL_ACTIVE : PANEL_PASSIVE), pi))
		return nullptr;

	if (0 == (pi.Flags & PFLAGS_PLUGIN))
		return gcnew Panel1(active);

	for (int i = 1; i < cPanels; ++i)
	{
		Panel2^ p = _panels[i];
		if (p && p->IsActive == active)
			return p->Host;
	}

	return gcnew Panel1(true);
}

array<Panel^>^ Panel0::PanelsByGuid(Guid typeId)
{
	List<FarNet::Panel^> list;
	for (int i = 1; i < cPanels; ++i)
	{
		Panel2^ core = _panels[i];
		if (core)
		{
			Panel^ panel = core->Host;
			if (panel->TypeId == typeId)
				list.Add(panel);
		}
	}
	return list.ToArray();
}

array<Panel^>^ Panel0::PanelsByType(Type^ type)
{
	List<FarNet::Panel^> list;
	for (int i = 1; i < cPanels; ++i)
	{
		Panel2^ core = _panels[i];
		if (core)
		{
			Panel^ panel = core->Host;
			Type^ panelType;
			if (!type || type == (panelType = panel->GetType()) || type->IsSubclassOf(panelType))
				list.Add(panel);
		}
	}
	return list.ToArray();
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

void Panel0::OpenPanel(Panel2^ plugin)
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
		Far::Api->Window->SetCurrentAt(0);
	}
	catch(InvalidOperationException^ e)
	{
		throw gcnew InvalidOperationException("Cannot open a panel because panels window cannot be set current.", e);
	}

	_panels[0] = plugin;
}

//! it call Update/Redraw in some cases
void Panel0::ReplacePanel(Panel2^ oldPanel, Panel2^ newPanel)
{
	// check
	if (!oldPanel) throw gcnew ArgumentNullException("oldPanel");
	if (!newPanel) throw gcnew ArgumentNullException("newPanel");

	int id1 = oldPanel->Index;
	if (id1 < 1)
		throw gcnew InvalidOperationException("Old panel must be opened.");

	if (newPanel->Index >= 1)
		throw gcnew InvalidOperationException("New panel must be not opened.");

	//_110210_081347 active info
	newPanel->_ActiveInfo = oldPanel->_ActiveInfo; //???? is it done twice?

	// save old modes
	oldPanel->StartSortMode = oldPanel->SortMode;
	oldPanel->StartViewMode = oldPanel->ViewMode;

	// disconnect old panel
	oldPanel->Handle = 0;
	oldPanel->Free();

	// connect new panel
	_panels[id1] = newPanel;
	newPanel->Index = id1;

	// change panel modes
	if (newPanel->StartViewMode != PanelViewMode::Undefined &&
		newPanel->StartViewMode != oldPanel->StartViewMode ||
		newPanel->StartSortMode != PanelSortMode::Default &&
		newPanel->StartSortMode != oldPanel->StartSortMode)
	{
		// set void mode for switching panel modes
		newPanel->_voidUpdateFiles = true;
		newPanel->Update(false);

		// set only new modes
		if (newPanel->StartViewMode != PanelViewMode::Undefined && newPanel->StartViewMode != oldPanel->StartViewMode)
			newPanel->ViewMode = newPanel->StartViewMode;
		if (newPanel->StartSortMode != PanelSortMode::Default)
		{
			if (newPanel->StartSortMode != oldPanel->StartSortMode)
				newPanel->SortMode = newPanel->StartSortMode;
		}

		// drop void mode
		newPanel->_voidUpdateFiles = false;
	}

	//! switch to new data and redraw, but not always: in some cases it will be done anyway, e.g. by Far
	if (!_inAsSetDirectory)
	{
		newPanel->Update(false);
		newPanel->Redraw(0, 0);
	}
}

void Panel0::PushPanel(Panel2^ plugin)
{
	if (plugin->_Pushed)
		throw gcnew InvalidOperationException("Cannot push the panel because it is already pushed.");

	//! save current state effectively by Far API, not FarNet
	PanelInfo pi;
	GetPanelInfo(plugin->Handle, pi);

	// save modes
	bool reversed = (pi.Flags & PFLAGS_REVERSESORTORDER) != 0;
	plugin->StartSortMode = (PanelSortMode)(reversed ? -pi.SortMode : pi.SortMode);
	plugin->StartViewMode = (PanelViewMode)pi.ViewMode;

	// current
	FarFile^ file = pi.ItemsNumber == 0 ? nullptr : plugin->CurrentFile;

	// push
	plugin->_Pushed = gcnew ShelveInfoModule(plugin);
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
	Works::ShelveInfo::Stack->Add(gcnew ShelveInfoNative(panel, modes));
}

}
