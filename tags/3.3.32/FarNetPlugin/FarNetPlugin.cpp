/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#include "stdafx.h"
#include "EditorManager.h"
#include "FarImpl.h"
#include "PluginManager.h"

PluginStartupInfo Info;
static FarStandardFunctions FSF;

namespace FarManagerImpl
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

gcroot<Far^> theFar;
gcroot<PluginManager^> thePlugins;

// Far for anybody
Far^ GetFar()
{
	return theFar;
}

#define __START try {
#define __END } catch(Exception^ e) { theFar->ShowError(nullptr, e); }

// SetStartupInfo is called once, after the plugin DLL is loaded.
// It loads the main plugin and found sub-plugins.
void WINAPI _export SetStartupInfo(const PluginStartupInfo* psi)
{
#ifdef TEST1
	StartTest1();
#endif

	Info = *psi;
	FSF = *psi->FSF;
	Info.FSF = &FSF;

	__START;
	theFar = gcnew Far;
	thePlugins = gcnew PluginManager(theFar);
	thePlugins->LoadPlugins();
	__END;
}

// Unloads sub-plugins and the main plugin
void WINAPI _export ExitFAR()
{
	__START;
	thePlugins->UnloadPlugins();
	thePlugins = nullptr;
	theFar->Free();
	theFar = nullptr;
	__END;

#ifdef TEST1
	StopTest1();
#endif
}

// GetPluginInfo is called to get general plugin info.
// It passes in joined information about all plugins.
void WINAPI _export GetPluginInfo(PluginInfo* pi)
{
	__START;
	theFar->AsGetPluginInfo(pi);
	__END;
}

// OpenPlugin is called to create a new plugin instance or do a job.
HANDLE WINAPI _export OpenPlugin(int from, INT_PTR item)
{
	__START;
	return theFar->AsOpenPlugin(from, item);
	__END;
	return INVALID_HANDLE_VALUE;
}

int WINAPI _export Configure(int itemIndex)
{
	__START;
	return theFar->AsConfigure(itemIndex);
	__END;
	return false;
}

int WINAPI _export ProcessEditorInput(const INPUT_RECORD* rec)
{
	__START;
	return theFar->_editorManager->AsProcessEditorInput(rec);
	__END;
	return 0;
}

int WINAPI _export ProcessEditorEvent(int type, void* param)
{
	__START;
	return theFar->_editorManager->AsProcessEditorEvent(type, param);
	__END;
	return 0;
}

void WINAPI _export ClosePlugin(HANDLE hPlugin)
{
	__START;
	theFar->AsClosePlugin(hPlugin);
	__END;
}

int WINAPI _export GetFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, char* destPath, int opMode)
{
	__START;
	return theFar->AsGetFiles(hPlugin, panelItem, itemsNumber, move, destPath, opMode);
	__END;
	return 0;
}

int WINAPI _export PutFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, int opMode)
{
	__START;
	return theFar->AsPutFiles(hPlugin, panelItem, itemsNumber, move, opMode);
	__END;
	return 0;
}

int WINAPI _export GetFindData(HANDLE hPlugin, PluginPanelItem** pPanelItem, int* pItemsNumber, int opMode)
{
	return theFar->AsGetFindData(hPlugin, pPanelItem, pItemsNumber, opMode);
}

void WINAPI _export FreeFindData(HANDLE /*hPlugin*/, PluginPanelItem* panelItem, int /*itemsNumber*/)
{
	__START;
	theFar->AsFreeFindData(panelItem);
	__END;
}

void WINAPI _export GetOpenPluginInfo(HANDLE hPlugin, OpenPluginInfo* info)
{
	__START;
	theFar->AsGetOpenPluginInfo(hPlugin, info);
	__END;
}

int WINAPI _export SetDirectory(HANDLE hPlugin, const char* dir, int opMode)
{
	__START;
	return theFar->AsSetDirectory(hPlugin, dir, opMode);
	__END;
	return FALSE;
}

int WINAPI _export DeleteFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int opMode)
{
	__START;
	return theFar->AsDeleteFiles(hPlugin, panelItem, itemsNumber, opMode);
	__END;
	return FALSE;
}

int WINAPI _export ProcessKey(HANDLE hPlugin, int key, unsigned int controlState)
{
	__START;
	return theFar->AsProcessKey(hPlugin, key, controlState);
	__END;
	return TRUE; // TRUE: ignore the key because there was a problem
}

int WINAPI _export ProcessEvent(HANDLE hPlugin, int id, void* param)
{
	__START;
	return theFar->AsProcessEvent(hPlugin, id, param);
	__END;
	return FALSE;
}

int WINAPI _export MakeDirectory(HANDLE hPlugin, char* name, int opMode)
{
	__START;
	return theFar->AsMakeDirectory(hPlugin, name, opMode);
	__END;
	return 0;
}

}
