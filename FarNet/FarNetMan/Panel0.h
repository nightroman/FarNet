
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class Panel1;
ref class Panel2;

const int cPanels = 4;
ref class Panel0
{
internal:
	static property Panel2^ PostedPanel { Panel2^ get() { return _panels[0]; } }
internal:
	static void BeginOpenMode();
	static void EndOpenMode();
	static HANDLE AddPluginPanel(Panel2^ plugin);
	static int AsDeleteFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int opMode);
	static int AsGetFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, const wchar_t** destPath, int opMode);
	static int AsGetFindData(HANDLE hPlugin, PluginPanelItem** pPanelItem, int* pItemsNumber, int opMode);
	static int AsMakeDirectory(HANDLE hPlugin, const wchar_t** name, int opMode);
	static int AsProcessEvent(HANDLE hPlugin, int id, void* param);
	static int AsProcessKey(HANDLE hPlugin, int key, unsigned int controlState);
	static int AsPutFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, const wchar_t* srcPath, int opMode);
	static int AsSetDirectory(HANDLE hPlugin, const wchar_t* dir, int opMode);
	static IPanel^ GetPanel(bool active);
	static Panel2^ GetPanel(Guid typeId);
	static Panel2^ GetPanel(Type^ type);
	static Panel2^ GetPanel2(Panel2^ plugin);
	static void AsClosePlugin(HANDLE hPlugin);
	static void AsFreeFindData(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber);
	static void AsGetOpenPluginInfo(HANDLE hPlugin, OpenPluginInfo* info);
	static void OpenPanel(Panel2^ plugin);
	static void PushPanel(Panel2^ plugin);
	static void ReplacePanel(Panel2^ oldPanel, Panel2^ newPanel);
	static void ShelvePanel(Panel1^ panel, bool modes);
	static Panel2^ GetPanel(int worksId) { return _panels[worksId]; }
private:
	Panel0() {}
	static void ReplaceExplorer(Panel^ panel, Explorer^ explorer, String^ postName); 
private:
	// Posted [0] and opened [1..3] panels; i.e. size is 4, see AddPluginPanel().
	static array<Panel2^>^ _panels = gcnew array<Panel2^>(cPanels);
	static bool _inAsSetDirectory;
	static int _openMode;
};
}
