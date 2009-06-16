/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#include "stdafx.h"
#include "Dialog.h"
#include "EditorHost.h"
#include "Far.h"
#include "Panel.h"
#include "ViewerHost.h"

PluginStartupInfo Info;
static FarStandardFunctions FSF;
static bool s_loaded, s_unloaded;

#define __START try {
#define __END } catch(Exception^ e) { Far::Instance->ShowError(nullptr, e); }

/*
SetStartupInfo is normally called once when the plugin DLL has been loaded.
But more calls are possible, we have to ignore them.
*/
void WINAPI SetStartupInfoW(const PluginStartupInfo* psi)
{
	LOG_AUTO(3, __FUNCTION__);

	// case: loaded
	if (s_loaded)
	{
		Info.Control(INVALID_HANDLE_VALUE, FCTL_GETUSERSCREEN, 0, 0);
		Console::WriteLine("WARNING: FarNet has been already loaded.");
		Info.Control(INVALID_HANDLE_VALUE, FCTL_SETUSERSCREEN, 0, 0);
		return;
	}

	// case: unloaded
	if (s_unloaded)
	{
		Info.Control(INVALID_HANDLE_VALUE, FCTL_GETUSERSCREEN, 0, 0);
		Console::WriteLine("WARNING: FarNet has been unloaded before and the second load is not supported.");
		Info.Control(INVALID_HANDLE_VALUE, FCTL_SETUSERSCREEN, 0, 0);
		return;
	}

	// load!
	s_loaded = true;

#ifdef TRACE_MEMORY
	StartTraceMemory();
#endif

	Info = *psi;
	FSF = *psi->FSF;
	Info.FSF = &FSF;

	__START;
	Far::StartFar();
	__END;
}

/*
Unloads sub-plugins and the main plugin.
STOP: ensure it is "loaded".
*/
void WINAPI ExitFARW()
{
	LOG_AUTO(3, __FUNCTION__);

	if (s_loaded)
	{
		// set flags
		s_loaded = false;
		s_unloaded = true;

		// don't try/catch, FAR can't help
		Far::Instance->Stop();

#ifdef TRACE_MEMORY
		StopTraceMemory();
#endif
	}
}

/*
GetPluginInfo is called to get general plugin info.
FarNet returns joined information about its plugins.
STOP: exotic case: FarNet has been "unloaded", return empty information.
*/
void WINAPI GetPluginInfoW(PluginInfo* pi)
{
	__START;
	Far::Instance->AsGetPluginInfo(pi);
	__END;
}

// OpenPlugin is called to create a new plugin instance or do a job.
HANDLE WINAPI OpenPluginW(int from, INT_PTR item)
{
	__START;
	return Far::Instance->AsOpenPlugin(from, item);
	__END;
	return INVALID_HANDLE_VALUE;
}

int WINAPI ConfigureW(int itemIndex)
{
	__START;
	return Far::Instance->AsConfigure(itemIndex);
	__END;
	return false;
}

void WINAPI ClosePluginW(HANDLE hPlugin)
{
	__START;
	PanelSet::AsClosePlugin(hPlugin);
	__END;
}

int WINAPI GetFilesW(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, const wchar_t** destPath, int opMode)
{
	__START;
	return PanelSet::AsGetFiles(hPlugin, panelItem, itemsNumber, move, destPath, opMode);
	__END;
	return 0;
}

int WINAPI PutFilesW(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, int opMode)
{
	__START;
	return PanelSet::AsPutFiles(hPlugin, panelItem, itemsNumber, move, opMode);
	__END;
	return 0;
}

int WINAPI GetFindDataW(HANDLE hPlugin, PluginPanelItem** pPanelItem, int* pItemsNumber, int opMode)
{
	return PanelSet::AsGetFindData(hPlugin, pPanelItem, pItemsNumber, opMode);
}

void WINAPI FreeFindDataW(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber)
{
	__START;
	PanelSet::AsFreeFindData(hPlugin, panelItem, itemsNumber);
	__END;
}

void WINAPI GetOpenPluginInfoW(HANDLE hPlugin, OpenPluginInfo* info)
{
	__START;
	PanelSet::AsGetOpenPluginInfo(hPlugin, info);
	__END;
}

int WINAPI SetDirectoryW(HANDLE hPlugin, const wchar_t* dir, int opMode)
{
	__START;
	return PanelSet::AsSetDirectory(hPlugin, dir, opMode);
	__END;
	return false;
}

int WINAPI DeleteFilesW(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int opMode)
{
	__START;
	return PanelSet::AsDeleteFiles(hPlugin, panelItem, itemsNumber, opMode);
	__END;
	return false;
}

int WINAPI MakeDirectoryW(HANDLE hPlugin, const wchar_t** name, int opMode)
{
	__START;
	return PanelSet::AsMakeDirectory(hPlugin, name, opMode);
	__END;
	return 0;
}

HANDLE WINAPI OpenFilePluginW(wchar_t* name, const unsigned char* data, int dataSize, int opMode)
{
	__START;
	return Far::Instance->AsOpenFilePlugin(name, data, dataSize, opMode);
	__END;
	return INVALID_HANDLE_VALUE;
}

int WINAPI ProcessDialogEventW(int id, void* param)
{
	__START;
	return FarDialog::AsProcessDialogEvent(id, param);
	__END;
	return true; // ignore, there was a problem
}

int WINAPI ProcessEditorEventW(int type, void* param)
{
	__START;
	return EditorHost::AsProcessEditorEvent(type, param);
	__END;
	return 0;
}

int WINAPI ProcessEditorInputW(const INPUT_RECORD* rec)
{
	__START;
	return EditorHost::AsProcessEditorInput(rec);
	__END;
	return 0;
}

int WINAPI ProcessEventW(HANDLE hPlugin, int id, void* param)
{
	__START;
	return PanelSet::AsProcessEvent(hPlugin, id, param);
	__END;
	return false;
}

int WINAPI ProcessKeyW(HANDLE hPlugin, int key, unsigned int controlState)
{
	__START;
	return PanelSet::AsProcessKey(hPlugin, key, controlState);
	__END;
	return true; // ignore, there was a problem
}

int WINAPI ProcessViewerEventW(int type, void* param)
{
	__START;
	return ViewerHost::AsProcessViewerEvent(type, param);
	__END;
	return 0;
}
