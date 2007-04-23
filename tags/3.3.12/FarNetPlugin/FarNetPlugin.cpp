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

gcroot<Far^> farImpl;
gcroot<PluginManager^> pluginManager;

// Far for anybody
Far^ GetFar()
{
	return farImpl;
}

// GetMsg gets a message string from a lang file. Here is the wrapper.
char* GetMsg(int MsgId)
{
	return (char*)Info.GetMsg(Info.ModuleNumber, MsgId);
}

#define __START try {
#define __END } catch(Exception^ e) { farImpl->ShowError(nullptr, e); }

// SetStartupInfo is called once, after the DLL module is loaded to memory.
void WINAPI _export SetStartupInfo(const PluginStartupInfo* psi)
{
	Info = *psi;
	FSF = *psi->FSF;
	Info.FSF = &FSF;
	__START;
	farImpl = gcnew Far();
	pluginManager = gcnew PluginManager(farImpl);
	pluginManager->LoadPlugins();
	__END;
}

// GetPluginInfo is called to get general plugin info.
void WINAPI _export GetPluginInfo(PluginInfo* pi)
{
	__START;
	farImpl->AsGetPluginInfo(pi);
	__END;
}

// OpenPlugin is called on new plugin instance.
HANDLE WINAPI _export OpenPlugin(int OpenFrom, int item)
{
	__START;
	return farImpl->AsOpenPlugin(OpenFrom, item);
	__END;
	return INVALID_HANDLE_VALUE;
}

void WINAPI _export ExitFAR()
{
	__START;
	farImpl = nullptr;
	pluginManager->UnloadPlugins();
	pluginManager = nullptr;
	__END;
}

int WINAPI _export ProcessEditorInput(const INPUT_RECORD* rec)
{
	__START;
	return farImpl->_editorManager->AsProcessEditorInput(rec);
	__END;
	return 0;
}

int WINAPI _export ProcessEditorEvent(int type, void* param)
{
	__START;
	return farImpl->_editorManager->AsProcessEditorEvent(type, param);
	__END;
	return 0;
}

void WINAPI _export ClosePlugin(HANDLE hPlugin)
{
	__START;
	farImpl->AsClosePlugin(hPlugin);
	__END;
}

int WINAPI _export GetFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, char* destPath, int opMode)
{
	__START;
	return farImpl->AsGetFiles(hPlugin, panelItem, itemsNumber, move, destPath, opMode);
	__END;
	return 0;
}

int WINAPI _export PutFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, int opMode)
{
	__START;
	return farImpl->AsPutFiles(hPlugin, panelItem, itemsNumber, move, opMode);
	__END;
	return 0;
}

int WINAPI _export GetFindData(HANDLE hPlugin, PluginPanelItem** pPanelItem, int* pItemsNumber, int opMode)
{
	return farImpl->AsGetFindData(hPlugin, pPanelItem, pItemsNumber, opMode);
}

void WINAPI _export FreeFindData(HANDLE /*hPlugin*/, PluginPanelItem* panelItem, int itemsNumber)
{
	__START;
	farImpl->AsFreeFindData(panelItem, itemsNumber);
	__END;
}

void WINAPI _export GetOpenPluginInfo(HANDLE hPlugin, OpenPluginInfo* info)
{
	__START;
	farImpl->AsGetOpenPluginInfo(hPlugin, info);
	__END;
}

int WINAPI _export SetDirectory(HANDLE hPlugin, const char* dir, int opMode)
{
	__START;
	return farImpl->AsSetDirectory(hPlugin, dir, opMode);
	__END;
	return FALSE;
}

int WINAPI _export DeleteFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int opMode)
{
	__START;
	return farImpl->AsDeleteFiles(hPlugin, panelItem, itemsNumber, opMode);
	__END;
	return FALSE;
}

int WINAPI _export ProcessKey(HANDLE hPlugin, int key, unsigned int controlState)
{
	__START;
	return farImpl->AsProcessKey(hPlugin, key, controlState);
	__END;
	return TRUE; // TRUE: ignore the key because there was a problem
}

int WINAPI _export ProcessEvent(HANDLE hPlugin, int id, void* param)
{
	__START;
	return farImpl->AsProcessEvent(hPlugin, id, param);
	__END;
	return FALSE;
}

int WINAPI _export MakeDirectory(HANDLE hPlugin, char* name, int opMode)
{
	__START;
	return farImpl->AsMakeDirectory(hPlugin, name, opMode);
	__END;
	return 0;
}

}
