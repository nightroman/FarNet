
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

static List<FarFile^>^ ItemsToFiles(bool pureFiles, IList<FarFile^>^ files, IList<String^>^ names, PluginPanelItem* panelItem, int itemsNumber)
{
	List<FarFile^>^ r = gcnew List<FarFile^>(itemsNumber);

	//? Far bug: alone dots has UserData = 0 no matter what was written there; so check the dots name
	if (itemsNumber == 1 && panelItem[0].UserData == 0 && wcscmp(panelItem[0].FindData.lpwszFileName, L"..") == 0)
		return r;

	// pure case
	if (pureFiles)
	{
		for(int i = 0; i < itemsNumber; ++i)
			r->Add(Panel1::ItemToFile(panelItem[i]));
		return r;
	}

	// data case
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

// This is the very first called method
//! 090712. Allocation by chunks was originally used. But it turns out it does not improve
//! performance much (tested for 200000+ files). On the other hand allocation of large chunks
//! may fail due to memory fragmentation more frequently.
int Panel0::AsGetFindData(HANDLE hPlugin, PluginPanelItem** pPanelItem, int* pItemsNumber, int opMode)
{
	Panel2^ pp = _panels[(int)(INT_PTR)hPlugin];
	ExplorerModes mode = (ExplorerModes)opMode;
	const bool IsExploreLocation = pp->IsExploreLocation;

	Log::Source->TraceInformation("GetFindDataW Mode='{0}' Location='{1}'", mode, pp->PanelDirectory);

	try
	{
		// fake empty panel needed on switching modes, for example
		if (pp->_voidUpdateFiles)
		{
			Log::Source->TraceInformation("GetFindDataW fake empty panel");
			(*pItemsNumber) = 0;
			(*pPanelItem) = NULL;
			return 1;
		}

		// the Find mode
		const bool isFind = 0 != (opMode & OPM_FIND);
		if (isFind && !IsExploreLocation && pp->Host->Explorer) //???? eventually to disable for all
			return 0;

		//_110121_150249
		// the files to use to return data; now it keeps the old files to be restored on find
		IList<FarFile^>^ files = pp->Files;

		// get the new files
		if (!pp->_skipUpdateFiles)
		{
			// drop the original files for the Find mode
			if (isFind)
				pp->Files = gcnew List<FarFile^>();

			// get files
			try
			{
				if (pp->Host->Explorer)
				{
					ExplorerArgs args(mode);
					pp->Files = pp->Host->Explorer->Explore(%args);
				}
				else
				{
					PanelEventArgs e;
					e.Mode = mode;
					pp->Host->WorksUpdateFiles(%e);
					if (e.Ignore)
						return 0;
				}
			}
			finally
			{
				if (isFind)
				{
					// use new files, restore old
					IList<FarFile^>^ tmp = pp->Files;
					pp->Files = files;
					files = tmp;
				}
				else
				{
					// use new files, forget old
					files = pp->Files;
				}
			}
		}

		// all item number
		int nItem = files->Count;
		if (pp->HasDots)
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
		Log::Source->TraceInformation("GetFindDataW Address='{0:x}'", (long)(*pPanelItem));

		// add dots
		int itemIndex = -1, fileIndex = -1;
		if (pp->HasDots)
		{
			++itemIndex;
			wchar_t* dots = new wchar_t[3];
			dots[0] = dots[1] = '.'; dots[2] = '\0';
			PluginPanelItem& p = (*pPanelItem)[0];
			p.UserData = (DWORD_PTR)(-1);
			p.FindData.lpwszFileName = dots;
			p.Description = NewChars(pp->Host->DotsDescription);
		}

		// add files
		for each(FarFile^ f in files)
		{
			++itemIndex;
			++fileIndex;

			PluginPanelItem& p = (*pPanelItem)[itemIndex];
			FAR_FIND_DATA& d = p.FindData;

			// names
			d.lpwszFileName = NewChars(f->Name);
			p.Description = NewChars(f->Description);
			p.Owner = NewChars(f->Owner);

			// alternate names are for QView and this is important
			if (opMode == 0 && !IsExploreLocation)
			{
				wchar_t buf[12]; // 12: 10=len(0xffffffff=4294967295) + 1=sign + 1=\0
				Info.FSF->itoa(fileIndex, buf, 10);
				int size = (int)wcslen(buf) + 1;
				wchar_t* alternate = new wchar_t[size];
				memcpy(alternate, buf, size * sizeof(wchar_t));
				d.lpwszAlternateFileName = alternate;
			}
			else
			{
				d.lpwszAlternateFileName = NULL;
			}

			// other
			d.dwFileAttributes = (DWORD)f->Attributes;
			d.nFileSize = f->Length;
			d.ftCreationTime = DateTimeToFileTime(f->CreationTime);
			d.ftLastWriteTime = DateTimeToFileTime(f->LastWriteTime);
			d.ftLastAccessTime = DateTimeToFileTime(f->LastAccessTime);
			//_110121_150249 Set -1 in the Find mode
			p.UserData = (isFind || IsExploreLocation) ? -1 : fileIndex;

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

		// drop pure files
		if (IsExploreLocation)
			pp->Files = nullptr;

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

void Panel0::AsFreeFindData(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber)
{
	Panel2^ pp = _panels[(int)(INT_PTR)hPlugin]; //???? need? can it be static managed by Address -> Data map including Panel2^
	Log::Source->TraceInformation("FreeFindDataW Address='{0:x}' Location='{1}'", (long)panelItem, pp->PanelDirectory);

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

int Panel0::AsSetDirectory(HANDLE hPlugin, const wchar_t* dir, int opMode)
{
	Panel2^ pp = _panels[(int)(INT_PTR)hPlugin];
	ExplorerModes mode = (ExplorerModes)opMode;
	String^ directory = gcnew String(dir);

	Log::Source->TraceInformation("SetDirectoryW Mode='{0}' Name='{1}'", mode, directory);

	_inAsSetDirectory = true;
	try
	{
		Explorer^ explorer1 = pp->Host->Explorer;
		const bool IsExploreLocation = pp->IsExploreLocation;
		if (explorer1) //???? eventually use only explorers and disable Find for all
		{
			if (!IsExploreLocation && 0 != (opMode & OPM_FIND))
				return 0;
			
			Explorer^ explorer2;
			if (directory == "\\")
			{
				ExplorerArgs args(mode);
				explorer2 = explorer1->ExploreRoot(%args);
				if (!explorer2)
				{
					Panel^ mp = pp->Host;
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
			}
			else if (directory == "..")
			{
				ExplorerArgs args(mode);
				explorer2 = explorer1->ExploreParent(%args);
				if (!explorer2)
				{
					if (!pp->Host->Parent)
						return 0;

					pp->Host->CloseChild();
					return 1;
				}
			}
			else if (IsExploreLocation)
			{
				ExploreLocationArgs args(mode, directory);
				explorer2 = explorer1->ExploreLocation(%args);
			}
			else
			{
				ExploreDirectoryArgs args(mode, pp->Host->CurrentFile);
				explorer2 = explorer1->ExploreDirectory(%args);
			}
					
			if (!explorer2)
				return 0;

			ReplaceExplorer(pp->Host, explorer2);
			
			return 1;
		}

		if (!pp->Host->WorksSetPanelDirectory(nullptr))
			return 1;
		
		SetDirectoryEventArgs e;
		e.Mode = mode;
		e.Name = directory;
		
		pp->Host->WorksSetPanelDirectory(%e);
		return e.Ignore ? 0 : 1;
	}
	finally
	{
		_inAsSetDirectory = false;
	}
}

//?? NYI: Parameter destPath can be changed, i.e. (*destPath) replaced.
//?? NYI: Not used return value -1 (stopped by a user).
int Panel0::AsGetFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, const wchar_t** destPath, int opMode)
{
	Panel2^ pp = _panels[(int)(INT_PTR)hPlugin];
	ExplorerModes mode = (ExplorerModes)opMode;

	Log::Source->TraceInformation("GetFilesW Mode='{0}'", mode);

	//????? && (opMode & (OPM_VIEW | OPM_QUICKVIEW | OPM_EDIT)) limited modes; need only for QView or *modal* requirement?
	// MB needed for Copy to native plugins or FS. But how to be with names: use a flag SystemNames? Call a virtual method?
	// SystemNames looks good. If names are bad, it's a big issue for exchange via FS anyway.
	// And if SystemNames is ON then we do not generate aliases internally. Think.
	Explorer^ explorer = pp->Host->Explorer;
	if (explorer)
	{
		ExportFilesEventArgs e;
		e.Mode = mode;
		if (!pp->IsExploreLocation)
			e.Names = gcnew List<String^>; //1
		e.Files = ItemsToFiles(pp->IsExploreLocation, pp->Files, e.Names, panelItem, itemsNumber); //2
		e.Move = move != 0;
		e.Destination = gcnew String((*destPath));
			
		for(int i = 0; i < itemsNumber; ++i)
		{
			String^ fileName = pp->IsExploreLocation
				? Path::Combine(e.Destination, e.Files[i]->Name)
				: Path::Combine(e.Destination, e.Names[i]);
			ExportFileArgs args(mode, e.Files[i], fileName);
			pp->Host->Explorer->ExportFile(%args);
		}
			
		return 1;
	}

	if (!pp->Host->WorksExportFiles(nullptr))
		return 0;

	ExportFilesEventArgs e;
	e.Mode = mode;
	e.Names = gcnew List<String^>;
	e.Files = ItemsToFiles(false, pp->Files, e.Names, panelItem, itemsNumber);
	e.Move = move != 0;
	e.Destination = gcnew String((*destPath));

	pp->Host->WorksExportFiles(%e);
	return e.Ignore ? 0 : 1;
}

int Panel0::AsPutFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, const wchar_t* srcPath, int opMode)
{
	Panel2^ pp1 = _panels[(int)(INT_PTR)hPlugin];
	ExplorerModes mode = (ExplorerModes)opMode;
	
	Log::Source->TraceInformation("PutFilesW Mode='{0}'", mode);

	Panel2^ pp2 = GetPanel2(pp1);
	List<FarFile^>^ files;
	if (pp2 && !pp2->IsExploreLocation)
	{
		files = ItemsToFiles(false, pp2->Files, nullptr, panelItem, itemsNumber);
	}
	else
	{
		files = gcnew List<FarFile^>(itemsNumber);
		for(int i = 0; i < itemsNumber; ++i)
			files->Add(Panel1::ItemToFile(panelItem[i]));
	}

	if (!pp1->Host->WorksImportFiles(nullptr))
		return 0;

	ImportFilesEventArgs e;
	e.Mode = mode;
	e.Files = files;
	e.Move = move != 0;
	e.Source = srcPath ? gcnew String(srcPath) : String::Empty;
	
	pp1->Host->WorksImportFiles(%e);
	return e.Ignore ? 0 : 1;
}

//! It is called on move, too? I.e. is Move = Copy + Delete?
int Panel0::AsDeleteFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int opMode)
{
	Panel2^ pp = _panels[(int)(INT_PTR)hPlugin];
	ExplorerModes mode = (ExplorerModes)opMode;

	Log::Source->TraceInformation("DeleteFilesW Mode='{0}'", mode);

	Explorer^ explorer = pp->Host->Explorer;
	if (explorer)
	{
		if (!int(explorer->Function & ExplorerFunctions::DeleteFiles))
			return 0;

		DeleteFilesArgs args(mode, ItemsToFiles(pp->IsExploreLocation, pp->Files, nullptr, panelItem, itemsNumber));
		explorer->DeleteFiles(%args);
		return args.Result == JobResult::Ignore ? 0 : 1;
	}

	if (!pp->Host->WorksDeleteFiles(nullptr))
		return 0;

	FilesEventArgs e;
	e.Mode = mode;
	e.Files = ItemsToFiles(false, pp->Files, nullptr, panelItem, itemsNumber);

	pp->Host->WorksDeleteFiles(%e);
	return e.Ignore ? 0 : 1;
}

// Return values are 0, 1, -1. If 0 is returned Far shows a message "Cannot create".
// We do not want this. http://forum.farmanager.com/viewtopic.php?p=56846#p56846
int Panel0::AsMakeDirectory(HANDLE hPlugin, const wchar_t** name, int opMode)
{
	Panel2^ pp = _panels[(int)(INT_PTR)hPlugin];
	ExplorerModes mode = (ExplorerModes)opMode;

	Log::Source->TraceInformation("MakeDirectoryW Mode='{0}'", mode);

	if (!pp->Host->WorksMakeDirectory(nullptr))
		return -1;

	String^ name1 = gcnew String((*name));
	MakeDirectoryEventArgs e;
	e.Mode = mode;
	e.Name = name1;
	
	pp->Host->WorksMakeDirectory(%e);
	if (e.Ignore)
		return -1;

	// return a new name
	if (0 == (opMode & OPM_SILENT) && e.Name != name1)
	{
		// use the lame storage
		s_makingDirectory.Set(e.Name);
		*name = s_makingDirectory;
	}

	// done
	return 1;
}

void Panel0::AsClosePlugin(HANDLE hPlugin)
{
	Log::Source->TraceInformation("ClosePluginW");

	// drop the lame storage
	s_makingDirectory.Set(nullptr);

	// disconnect
	Panel2^ pp = _panels[(int)(INT_PTR)hPlugin];
	pp->Free();
	_panels[(int)(INT_PTR)hPlugin] = nullptr;
	
	// done for pushed
	if (pp->_Pushed)
		return;

	// clean all
	pp->Host->WorksClosed(true);
}

//! It is called too often, log Verbose
void Panel0::AsGetOpenPluginInfo(HANDLE hPlugin, OpenPluginInfo* info)
{
	Log::Source->TraceEvent(TraceEventType::Verbose, 0, "GetOpenPluginInfoW");

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
	if (!State::GetPanelInfo)
	{
		Log::Source->TraceEvent(TraceEventType::Verbose, 0, "UpdateInfo");
		pp->Host->WorksUpdateInfo();
	}

	// make info
	*info = pp->Make();
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

			pp->Host->WorksCtrlBreak();
		}
		break;
	case FE_CLOSE:
		{
			Log::Source->TraceInformation("FE_CLOSE");

			//? FE_CLOSE issues:
			// *) Bug [_090321_165608]: unwanted extra call on plugin commands entered in command line
			// http://bugs.farmanager.com/view.php?id=602
			// *) may not be called at all e.g. if tmp panel is opened
			if (!pp->_Pushed)
			{
				Log::Source->TraceInformation("Closing");
				PanelEventArgs e;
				pp->Host->WorksClosing(%e);
				return e.Ignore;
			}
		}
		break;
	case FE_COMMAND:
		{
			Log::Source->TraceInformation("FE_COMMAND");

			if (pp->Host->WorksInvokingCommand(nullptr))
			{
				CommandLineEventArgs e;
				e.Command = gcnew String((const wchar_t*)param);
				Log::Source->TraceInformation("InvokingCommand: {0}", e.Command);
				
				//! Try\catch in order to return exactly what the module returns.
				try
				{
					// call
					pp->Host->WorksInvokingCommand(%e);

					// clear the command line
					if (e.Ignore)
						Far::Net->CommandLine->Text = String::Empty;
				}
				catch(Exception^ exception)
				{
					Far::Net->ShowError("Event: Executing", exception);
				}

				return e.Ignore ? 1 : 0;
			}
		}
		break;
	case FE_CHANGEVIEWMODE:
		{
			Log::Source->TraceInformation("FE_CHANGEVIEWMODE");

			if (pp->Host->WorksViewChanged(nullptr))
			{
				ViewChangedEventArgs e;
				e.Columns = gcnew String((const wchar_t*)param);
				pp->Host->WorksViewChanged(%e);
			}
		}
		break;
	case FE_IDLE:
		{
			Log::Source->TraceEvent(TraceEventType::Verbose, 0, "FE_IDLE");

			// 1) call
			pp->Host->WorksIdled();

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

			pp->Host->WorksGotFocus();
		}
		break;
	case FE_KILLFOCUS:
		{
			Log::Source->TraceEvent(TraceEventType::Verbose, 0, "FE_KILLFOCUS");

			pp->Host->WorksLosingFocus();
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
			pp->Host->WorksRedrawing(%e);
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
			IEqualityComparer<FarFile^>^ comparer = pp->Host->Explorer ? pp->Host->Explorer->FileComparer : pp->Host->FileComparer;
			FarFile^ file = pp->_postFile;
			pp->_postFile = nullptr;
			if (comparer)
			{
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
			}
			
			return 0;
		}
		break;
	}
	return 0;
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
		return 0;

	//! mind rare case: plugin in null, e.g. closed by [AltF12] + select folder
	Panel2^ pp = _panels[(int)(INT_PTR)hPlugin];
	if (!pp)
		return 0;

	// preprocessing
	if ((key & PKF_PREPROCESS) != 0)
	{
		if (!pp->Host->WorksKeyPressing(nullptr))
			return 0;
		
		PanelKeyEventArgs e;
		e.Code = code;
		e.State = (KeyStates)controlState;
		Log::Source->TraceEvent(TraceEventType::Verbose, 0, "KeyPressing {0}", %e);
		
		pp->Host->WorksKeyPressing(%e);
		return e.Ignore ? 1 : 0;
	}

	PanelKeyEventArgs e;
	e.Code = code;
	e.State = (KeyStates)controlState;
	Log::Source->TraceEvent(TraceEventType::Verbose, 0, "KeyPressed {0}", %e);
	
	pp->Host->UIKeyPressed(%e);
	return e.Ignore ? 1 : 0;
}

IPanel^ Panel0::GetPanel(bool active)
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
			return p->Host;
	}

	return gcnew Panel1(true);
}

Panel2^ Panel0::GetPanel(Guid typeId)
{
	for (int i = 1; i < cPanels; ++i)
	{
		Panel2^ pp = _panels[i];
		if (pp && pp->Host->TypeId == typeId)
			return pp;
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
		Far::Net->Window->SetCurrentAt(0);
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
	FarFile^ file = nullptr;
	if (pi.ItemsNumber > 0)
	{
		AutoPluginPanelItem item(plugin->Handle, pi.CurrentItem, ShownFile);
		int index = (int)item.Get().UserData;
		if (index >= 0 && index < plugin->Files->Count)
			file = plugin->Files[index];
	}

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

// Module panel method candidate
void Panel0::ReplaceExplorer(Panel^ panel, Explorer^ explorer)
{
	// explorers must get new explorers
	if ((Object^)explorer == (Object^)panel->Explorer)
		throw gcnew InvalidOperationException("The same explorer object is not expected.");

	// make the panel
	Panel^ newPanel = nullptr;

	// reuse or make
	if (panel->ExplorerTypeId == explorer->TypeId)
	{
		// reuse
		newPanel = panel;
	}
	else
	{
		// make custom or default
		newPanel = explorer->CreatePanel();
		if (!newPanel)
			newPanel = gcnew Panel();
	}

	// attach the explorer
	newPanel->Explorer = explorer;
	String^ location = explorer->Location;
	if (location->Length)
		newPanel->PanelDirectory = location;
	else
		newPanel->PanelDirectory = "*"; //????

	// same panel? update and reuse
	if (newPanel == panel)
	{
		explorer->UpdatePanel(newPanel);
		return;
	}

	// setup and update the panel
	{
		explorer->UpdatePanel(newPanel);
		if (ES(newPanel->PanelDirectory))
			newPanel->PanelDirectory = "*"; //???? chain of explorer names?
	}

	// open new as child
	newPanel->OpenChild(panel);
}

}
