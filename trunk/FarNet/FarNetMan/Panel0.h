
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
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
	static array<Panel^>^ PanelsByGuid(Guid typeId);
	static array<Panel^>^ PanelsByType(Type^ type);
	static HANDLE AddPluginPanel(Panel2^ plugin);
	static int AsDeleteFiles(const DeleteFilesInfo* info);
	static int AsGetFiles(GetFilesInfo* info);
	static int AsGetFindData(GetFindDataInfo* info);
	static int AsProcessPanelEvent(const ProcessPanelEventInfo* info);
	static int AsProcessPanelInput(const ProcessPanelInputInfo* info);
	static int AsPutFiles(PutFilesInfo* info);
	static int AsSetDirectory(const SetDirectoryInfo* info);
	static IPanel^ GetPanel(bool active);
	static Panel2^ GetPanel2(Panel2^ plugin);
	static void AsClosePanel(const ClosePanelInfo* info);
	static void AsFreeFindData(const FreeFindDataInfo* info);
	static void AsGetOpenPanelInfo(OpenPanelInfo* info);
	static void OpenPanel(Panel2^ plugin);
	static void PushPanel(Panel2^ plugin);
	static void ReplacePanel(Panel2^ oldPanel, Panel2^ newPanel);
	static void ShelvePanel(Panel1^ panel, bool modes);
private:
	Panel0() {}
	static Panel2^ HandleToPanel(HANDLE hPanel) { return _panels[(int)hPanel]; }
	static void RemovePanel(HANDLE hPanel) { _panels[(int)hPanel] = nullptr; }
private:
	// Posted [0] and opened [1..3] panels; i.e. size is 4, see AddPluginPanel().
	static array<Panel2^>^ _panels = gcnew array<Panel2^>(cPanels);
	static bool _inAsSetDirectory;
	static int _openMode;
};
}
