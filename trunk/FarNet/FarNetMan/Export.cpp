
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
*/

#include "stdafx.h"
#include <initguid.h>
#include "Active.h"
#include "Dialog.h"
#include "Editor0.h"
#include "Far0.h"
#include "Far1.h"
#include "Panel0.h"
#include "Viewer0.h"

PluginStartupInfo Info;
static FarStandardFunctions FSF;

// {10435532-9BB3-487B-A045-B0E6ECAAB6BC}
DEFINE_GUID(MainGuid, 0x10435532, 0x9bb3, 0x487b, 0xa0, 0x45, 0xb0, 0xe6, 0xec, 0xaa, 0xb6, 0xbc);
DEFINE_GUID(FarGuid, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

#define __START try {
#define __END } catch(Exception^ e) { Far::Api->ShowError(nullptr, e); }

void WINAPI GetGlobalInfoW(struct GlobalInfo* info)
{
	info->MinFarVersion = MAKEFARVERSION(MinFarVersionMajor, MinFarVersionMinor, 0, MinFarVersionBuild, VS_RELEASE);
	info->Version = MAKEFARVERSION(5, 0, 0, 0, VS_RELEASE);
	info->Guid = MainGuid;
	info->Title = L"FarNet";
	info->Author = L"Roman Kuzmin";
	info->Description = L"FarNet module manager.";
}

/*
SetStartupInfo is normally called once when the plugin DLL has been loaded.
But more calls are possible, we have to ignore them.
*/
void WINAPI SetStartupInfoW(const PluginStartupInfo* psi)
{
	Log::Source->TraceInformation(__FUNCTION__ "{");
	try
	{
		// deny 2+ load
		if (Works::Host::State != Works::HostState::None)
		{
			Far::Api->Message("FarNet cannot be loaded twice.", "FarNet", MessageOptions::Warning);
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
	finally
	{
		Log::Source->TraceInformation(__FUNCTION__ "}");
	}
}

/*
Unloads modules and the plugin.
STOP: ensure it is "loaded".
*/
void WINAPI ExitFARW(const ExitInfo*)
{
	Log::Source->TraceInformation(__FUNCTION__ "{");
	try
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
	finally
	{
		Log::Source->TraceInformation(__FUNCTION__ "}");
	}
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
	pi->Flags = PF_DIALOG | PF_EDITOR | PF_VIEWER | PF_FULLCMDLINE | PF_PRELOAD;

	if (Works::Host::State != Works::HostState::Loaded)
		return;

	__START;
	Far0::AsGetPluginInfo(pi);
	__END;
}

// It is called for an action; action result can be a new panel.
HANDLE WINAPI OpenW(const OpenInfo* info)
{
	__START;
	return Far0::AsOpen(info);
	__END;
	return nullptr;
}

intptr_t WINAPI ConfigureW(const ConfigureInfo* info)
{
	__START;
	return Far0::AsConfigure(info);
	__END;
	return false;
}

void WINAPI ClosePanelW(const ClosePanelInfo* info)
{
	__START;
	Panel0::AsClosePanel(info);
	__END;
}

intptr_t WINAPI GetFilesW(GetFilesInfo* info)
{
	__START;
	return Panel0::AsGetFiles(info);
	__END;
	return 0;
}

int WINAPI PutFilesW(PutFilesInfo* info)
{
	__START;
	return Panel0::AsPutFiles(info);
	__END;
	return 0;
}

intptr_t WINAPI GetFindDataW(GetFindDataInfo* info)
{
	return Panel0::AsGetFindData(info);
}

void WINAPI FreeFindDataW(const FreeFindDataInfo* info)
{
	__START;
	Panel0::AsFreeFindData(info);
	__END;
}

void WINAPI GetOpenPanelInfoW(OpenPanelInfo* info)
{
	__START;
	Panel0::AsGetOpenPanelInfo(info);
	__END;
}

intptr_t WINAPI SetDirectoryW(const SetDirectoryInfo* info)
{
	__START;
	return Panel0::AsSetDirectory(info);
	__END;
	return false;
}

intptr_t WINAPI DeleteFilesW(const DeleteFilesInfo* info)
{
	__START;
	return Panel0::AsDeleteFiles(info);
	__END;
	return false;
}

intptr_t WINAPI ProcessEditorEventW(const ProcessEditorEventInfo* info)
{
	__START;
	return Editor0::AsProcessEditorEvent(info);
	__END;
	return 0;
}

intptr_t WINAPI ProcessEditorInputW(const ProcessEditorInputInfo* info)
{
	__START;
	return Editor0::AsProcessEditorInput(info);
	__END;
	return true; // on problems consider event as processed to avoid default actions
}

intptr_t WINAPI ProcessPanelEventW(const ProcessPanelEventInfo* info)
{
	__START;
	return Panel0::AsProcessPanelEvent(info);
	__END;
	return false;
}

intptr_t WINAPI ProcessPanelInputW(const ProcessPanelInputInfo* info)
{
	__START;
	return Panel0::AsProcessPanelInput(info);
	__END;
	return true; // ignore, there was a problem
}

intptr_t WINAPI ProcessSynchroEventW(const ProcessSynchroEventInfo* info)
{
	__START;
	Far0::AsProcessSynchroEvent(info);
	__END;
	return 0;
}

intptr_t WINAPI ProcessViewerEventW(const ProcessViewerEventInfo* info)
{
	__START;
	return Viewer0::AsProcessViewerEvent(info);
	__END;
	return 0;
}
