// Callbacks and global objects

#include "stdafx.h"
#include "EditorManager.h"
#include "FarImpl.h"
#include "Utils.h"
using namespace System::Reflection;

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

// Gets a property value by name or null
Object^ Property(Object^ obj, String^ name)
{
	try
	{
		return obj->GetType()->InvokeMember(
			name, BindingFlags::GetProperty | BindingFlags::Public | BindingFlags::Instance, nullptr, obj, nullptr);
	}
	catch(...)
	{
		return nullptr;
	}
}

void ShowExceptionInfo(Exception^ e)
{
	String^ info = e->Message + "\n";

	if (e->GetType()->FullName->StartsWith("System.Management.Automation."))
	{
		Object^ er = Property(e, "ErrorRecord");
		if (er != nullptr)
		{
			Object^ ii = Property(er, "InvocationInfo");
			if (ii != nullptr)
			{
				Object^ pm = Property(ii, "PositionMessage");
				if (pm != nullptr)
					info = info + pm->ToString() + "\n\n";
			}
		}
	}

	String^ path = Path::GetTempFileName();
	File::WriteAllText(path, info + e->ToString());

	CStr title(e->GetType()->FullName);
	CStr filename(path);
	Info.Viewer(filename, title, -1, -1, -1, -1, VF_DELETEONLYFILEONCLOSE | VF_DISABLEHISTORY);
}

void handleException(Exception^ e)
{
	CStr typeName(e->GetType()->FullName);
	CStr text(e->Message);

	const char* Msg[7];
	Msg[0] = typeName;
	Msg[1] = text;
	Msg[2] = "Ok";
	Msg[3] = "Info";
	int button = Info.Message(Info.ModuleNumber, FMSG_WARNING | FMSG_LEFTALIGN, "Contents", Msg, 4, 2);

	if (button == 1)
		ShowExceptionInfo(e);
}

#define __START try{
#define __END }catch(Exception^ e){handleException(e);}

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
	return farImpl->editorManager->ProcessEditorInput(rec);
	__END;
	return 0;
}

int WINAPI _export ProcessEditorEvent(int type, void* param)
{
	__START;
	return farImpl->editorManager->ProcessEditorEvent(type, param);
	__END;
	return 0;
}
}
