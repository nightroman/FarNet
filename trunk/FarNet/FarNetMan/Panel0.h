/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
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
	static Panel1^ GetPanel(bool active);
	static Panel2^ GetPluginPanel(Guid id);
	static Panel2^ GetPluginPanel(Type^ hostType);
	static Panel2^ GetPluginPanel2(Panel2^ plugin);
	static void AsClosePlugin(HANDLE hPlugin);
	static void AsFreeFindData(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber);
	static void AsGetOpenPluginInfo(HANDLE hPlugin, OpenPluginInfo* info);
	static void OpenPluginPanel(Panel2^ plugin);
	static void PushPluginPanel(Panel2^ plugin);
	static void ReplacePluginPanel(Panel2^ oldPanel, Panel2^ newPanel);
	static void ShelvePanel(Panel1^ panel, bool modes);
private:
	Panel0() {}
private:
	// Posted [0] and opened [1..3] panels; i.e. size is 4, see AddPluginPanel().
	static array<Panel2^>^ _panels = gcnew array<Panel2^>(cPanels);
	static bool _inAsSetDirectory;
	static int _openMode;
};
}
