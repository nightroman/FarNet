// Callbacks and global objects

#include "stdafx.h"
#include "EditorManager.h"
#include "FarImpl.h"

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
gcroot<FarNetPlugMan::PluginManager^> pluginManager;

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
	pluginManager = gcnew FarNetPlugMan::PluginManager();
	pluginManager->Far = farImpl;
	__END;
}

// GetPluginInfo is called to get general plugin info.
void WINAPI _export GetPluginInfo(PluginInfo* pi)
{
	__START;
	farImpl->OnGetPluginInfo(pi);
	__END;
}

// OpenPlugin is called on new plagin instance.
HANDLE WINAPI _export OpenPlugin(int OpenFrom, int item)
{
	__START;
	return farImpl->OnOpenPlugin(OpenFrom, item);
	__END;
	return INVALID_HANDLE_VALUE;
}

void WINAPI _export ExitFAR()
{
	__START;
	farImpl = nullptr;
	pluginManager->Far = nullptr;
	pluginManager = nullptr;
	__END;
}

int WINAPI _export ProcessEditorInput(const INPUT_RECORD* rec)
{
	__START;
	return farImpl->_editorManager->ProcessEditorInput(rec);
	__END;
	return 0;
}

int WINAPI _export ProcessEditorEvent(int type, void* param)
{
	__START;
	return farImpl->_editorManager->ProcessEditorEvent(type, param);
	__END;
	return 0;
}
}
