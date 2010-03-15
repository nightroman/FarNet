/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "stdafx.h"
#include "Dialog.h"
#include "Editor0.h"
#include "Far0.h"
#include "Far1.h"
#include "Panel0.h"
#include "Viewer0.h"

PluginStartupInfo Info;
static FarStandardFunctions FSF;

#define __START try {
#define __END } catch(Exception^ e) { Far::Net->ShowError(nullptr, e); }

/*
SetStartupInfo is normally called once when the plugin DLL has been loaded.
But more calls are possible, we have to ignore them.
*/
void WINAPI SetStartupInfoW(const PluginStartupInfo* psi)
{
	LOG_AUTO(Info, __FUNCTION__)
	{
		// deny
		if (Works::Host::State != Works::HostState::None)
		{
			Info.Control(INVALID_HANDLE_VALUE, FCTL_GETUSERSCREEN, 0, 0);
			Console::WriteLine("WARNING: FarNet cannot be loaded twice.");
			Info.Control(INVALID_HANDLE_VALUE, FCTL_SETUSERSCREEN, 0, 0);
			return;
		}

		// loading
		Works::Host::State = Works::HostState::Loading;

		Far1::Connect();

#ifdef TRACE_MEMORY
		StartTraceMemory();
#endif

		Info = *psi;
		FSF = *psi->FSF;
		Info.FSF = &FSF;

		__START;
		Far0::Start();
		__END;

		// loaded
		Works::Host::State = Works::HostState::Loaded;
	}
	LOG_END;
}

/*
Unloads modules and the plugin.
STOP: ensure it is "loaded".
*/
void WINAPI ExitFARW()
{
	LOG_AUTO(Info, __FUNCTION__)
	{
		if (Works::Host::State == Works::HostState::Loaded)
		{
			// unloading
			Works::Host::State = Works::HostState::Unloading;

			// don't try/catch, Far can't help
			Far0::Stop();

			// unloaded
			Works::Host::State = Works::HostState::Unloaded;

#ifdef TRACE_MEMORY
			StopTraceMemory();
#endif
		}
	}
	LOG_END;
}

/*
It is called frequently to get plugin actions info.
FarNet returns combined information for its modules.
STOP:
Exotic case: FarNet has been unloaded: return no action information,
do return flags, preloadable flag is absolutely important as cached.
*/
void WINAPI GetPluginInfoW(PluginInfo* pi)
{
	pi->StructSize = sizeof(PluginInfo);
	pi->Flags = PF_DIALOG | PF_EDITOR | PF_VIEWER | PF_FULLCMDLINE | PF_PRELOAD;

	if (Works::Host::State != Works::HostState::Loaded)
		return;

	__START;
	Far0::AsGetPluginInfo(pi);
	__END;
}

// It is called for an action; action result can be a new panel.
HANDLE WINAPI OpenPluginW(int from, INT_PTR item)
{
	__START;
	return Far0::AsOpenPlugin(from, item);
	__END;
	return INVALID_HANDLE_VALUE;
}

int WINAPI ConfigureW(int itemIndex)
{
	__START;
	return Far0::AsConfigure(itemIndex);
	__END;
	return false;
}

void WINAPI ClosePluginW(HANDLE hPlugin)
{
	__START;
	Panel0::AsClosePlugin(hPlugin);
	__END;
}

int WINAPI GetFilesW(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, const wchar_t** destPath, int opMode)
{
	__START;
	return Panel0::AsGetFiles(hPlugin, panelItem, itemsNumber, move, destPath, opMode);
	__END;
	return 0;
}

int WINAPI PutFilesW(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, const wchar_t* srcPath, int opMode)
{
	__START;
	return Panel0::AsPutFiles(hPlugin, panelItem, itemsNumber, move, srcPath, opMode);
	__END;
	return 0;
}

int WINAPI GetFindDataW(HANDLE hPlugin, PluginPanelItem** pPanelItem, int* pItemsNumber, int opMode)
{
	return Panel0::AsGetFindData(hPlugin, pPanelItem, pItemsNumber, opMode);
}

void WINAPI FreeFindDataW(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber)
{
	__START;
	Panel0::AsFreeFindData(hPlugin, panelItem, itemsNumber);
	__END;
}

void WINAPI GetOpenPluginInfoW(HANDLE hPlugin, OpenPluginInfo* info)
{
	__START;
	Panel0::AsGetOpenPluginInfo(hPlugin, info);
	__END;
}

int WINAPI SetDirectoryW(HANDLE hPlugin, const wchar_t* dir, int opMode)
{
	__START;
	return Panel0::AsSetDirectory(hPlugin, dir, opMode);
	__END;
	return false;
}

int WINAPI DeleteFilesW(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int opMode)
{
	__START;
	return Panel0::AsDeleteFiles(hPlugin, panelItem, itemsNumber, opMode);
	__END;
	return false;
}

int WINAPI MakeDirectoryW(HANDLE hPlugin, const wchar_t** name, int opMode)
{
	__START;
	return Panel0::AsMakeDirectory(hPlugin, name, opMode);
	__END;
	return 0;
}

HANDLE WINAPI OpenFilePluginW(wchar_t* name, const unsigned char* data, int dataSize, int opMode)
{
	__START;
	return Far0::AsOpenFilePlugin(name, data, dataSize, opMode);
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
	return Editor0::AsProcessEditorEvent(type, param);
	__END;
	return 0;
}

int WINAPI ProcessEditorInputW(const INPUT_RECORD* rec)
{
	__START;
	return Editor0::AsProcessEditorInput(rec);
	__END;
	return true; // on problems consider event as processed to avoid default actions
}

int WINAPI ProcessEventW(HANDLE hPlugin, int id, void* param)
{
	__START;
	return Panel0::AsProcessEvent(hPlugin, id, param);
	__END;
	return false;
}

int WINAPI ProcessKeyW(HANDLE hPlugin, int key, unsigned int controlState)
{
	__START;
	return Panel0::AsProcessKey(hPlugin, key, controlState);
	__END;
	return true; // ignore, there was a problem
}

int WINAPI ProcessSynchroEventW(int type, void* param)
{
	__START;
	Far0::AsProcessSynchroEvent(type, param);
	__END;
	return 0;
}

int WINAPI ProcessViewerEventW(int type, void* param)
{
	__START;
	return Viewer0::AsProcessViewerEvent(type, param);
	__END;
	return 0;
}
