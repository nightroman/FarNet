/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once
#undef CreateDialog

namespace FarNet
{;
ref class ProxyAction;
ref class ProxyCommand;
ref class ProxyEditor;
ref class ProxyFiler;
ref class ProxyTool;

ref class Far0
{
public: 
	static bool AsConfigure(int itemIndex);
	static HANDLE AsOpenFilePlugin(wchar_t* name, const unsigned char* data, int dataSize, int opMode);
	static HANDLE AsOpenPlugin(int from, INT_PTR item);
	static void AsGetPluginInfo(PluginInfo* pi);
	static void AsProcessSynchroEvent(int type, void* param);
public:
	static void RegisterProxyCommand(ProxyCommand^ info);
	static void RegisterProxyEditor(ProxyEditor^ info);
	static void RegisterProxyFiler(ProxyFiler^ info);
	static void RegisterProxyTool(ProxyTool^ info);
	static void InvokeModuleEditors(IEditor^ editor, const wchar_t* fileName);
	static void Start();
	static void Stop();
	static void UnregisterProxyAction(ProxyAction^ action);
public:
	static CultureInfo^ GetCurrentUICulture(bool update);
	static int GetPaletteColor(PaletteColor paletteColor);
	static void PostJob(EventHandler^ handler);
	static void PostStep(EventHandler^ handler);
	static void PostStepAfterKeys(String^ keys, EventHandler^ handler);
	static void PostStepAfterStep(EventHandler^ handler1, EventHandler^ handler2);
	static void Run(String^ command);
	static void ShowPanelMenu(bool showPushCommand);
public:
	static String^ _folder = Path::GetDirectoryName((Assembly::GetExecutingAssembly())->Location);
	static String^ _helpTopic = "<" + _folder + "\\>";
private:
	static bool CompareName(String^ mask, const wchar_t* name, bool skipPath);
	static bool CompareNameEx(String^ mask, const wchar_t* name, bool skipPath);
	static void AssertHotkeys();
	static void ConfigCommand();
	static void ConfigEditor();
	static void ConfigFiler();
	static void ConfigTool(List<ProxyTool^>^ tools);
	static void ConfigUICulture();
	static void OpenConfig();
	static void OpenMenu(ModuleToolOptions from);
	static void ProcessPrefixes(INT_PTR item);
	static void VoidStep(Object^, EventArgs^) {}
	static void UnregisterProxyTool(ProxyTool^ tool);
	static void InvalidateProxyTool(ModuleToolOptions options);
private:
	static CStr* _pConfig;
	static CStr* _pDialog;
	static CStr* _pDisk;
	static CStr* _pEditor;
	static CStr* _pPanels;
	static CStr* _pViewer;
	static array<ProxyTool^>^ _toolConfig;
	static array<ProxyTool^>^ _toolDialog;
	static array<ProxyTool^>^ _toolDisk;
	static array<ProxyTool^>^ _toolEditor;
	static array<ProxyTool^>^ _toolPanels;
	static array<ProxyTool^>^ _toolViewer;
	static CStr* _prefixes;
	static List<ProxyCommand^> _registeredCommand;
	static List<ProxyEditor^> _registeredEditor;
	static List<ProxyFiler^> _registeredFiler;
private:
	static CultureInfo^ _currentUICulture;
	// Post
	static String^ _hotkey;
	static array<int>^ _hotkeys;
	static EventHandler^ _handler;
	// Sync
	static HANDLE _hMutex;
	static List<EventHandler^> _syncHandlers;
};

}
