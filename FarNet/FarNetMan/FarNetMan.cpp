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

namespace FarNet
{;
enum EMessage
{
	MTitle,
	MMessage1,
	MMessage2,
	MMessage3,
	MMessage4,
	MButton,
};

#define __START try {
#define __END } catch(Exception^ e) { Far::Instance->ShowError(nullptr, e); }

// SetStartupInfo is called once, after the plugin DLL is loaded.
void WINAPI SetStartupInfoW(const PluginStartupInfo* psi)
{
#ifdef TEST1
	StartTest1();
#endif

	Info = *psi;
	FSF = *psi->FSF;
	Info.FSF = &FSF;

	__START;
	Far::StartFar();
	__END;
}

// Unloads sub-plugins and the main plugin
void WINAPI ExitFARW()
{
	// don't try/catch, FAR can't help
	Far::Instance->Stop();

#ifdef TEST1
	StopTest1();
#endif
}

// GetPluginInfo is called to get general plugin info.
// It passes in joined information about all plugins.
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

void WINAPI FreeFindDataW(HANDLE /*hPlugin*/, PluginPanelItem* panelItem, int /*itemsNumber*/)
{
	__START;
	PanelSet::AsFreeFindData(panelItem);
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

}
