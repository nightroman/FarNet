/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class Far0
{
public:
	static bool AsConfigure(int itemIndex);
	static HANDLE AsOpenFilePlugin(wchar_t* name, const unsigned char* data, int dataSize, int opMode);
	static HANDLE AsOpenPlugin(int from, INT_PTR item);
	static void AsGetPluginInfo(PluginInfo* pi);
	static void AsProcessSynchroEvent(int type, void* param);
public:
	static void RegisterProxyCommand(IModuleCommand^ info);
	static void RegisterProxyEditor(IModuleEditor^ info);
	static void RegisterProxyFiler(IModuleFiler^ info);
	static void RegisterProxyTool(IModuleTool^ info);
	static void InvokeModuleEditors(IEditor^ editor, const wchar_t* fileName);
	static void Start();
	static void Stop();
	static void UnregisterProxyAction(IModuleAction^ action);
public:
	static CultureInfo^ GetCurrentUICulture(bool update);
	static void PostJob(EventHandler^ handler);
	static void PostStep(EventHandler^ handler);
	static void PostStepAfterKeys(String^ keys, EventHandler^ handler);
	static void PostStepAfterStep(EventHandler^ handler1, EventHandler^ handler2);
	static void Run(String^ command);
	static void ShowPanelMenu(bool showPushCommand);
public:
	static String^ _folder = Path::GetDirectoryName((Assembly::GetExecutingAssembly())->Location);
	static String^ _helpTopic = "<" + _folder + "\\>";
	static void InvalidateProxyCommand();
	static void UnregisterProxyTool(IModuleTool^ tool);
private:
	static bool CompareName(String^ mask, const wchar_t* name, bool skipPath);
	static bool CompareNameEx(String^ mask, const wchar_t* name, bool skipPath);
	static void AssertHotkeys();
	static void OpenConfig();
	static void OpenMenu(ModuleToolOptions from);
	static void ProcessPrefixes(INT_PTR item);
	static void VoidStep(Object^, EventArgs^) {}
	static void InvalidateProxyTool(ModuleToolOptions options);
	static String^ GetMenuText(IModuleTool^ tool);
private:
	static CStr* _pConfig;
	static CStr* _pDialog;
	static CStr* _pDisk;
	static CStr* _pEditor;
	static CStr* _pPanels;
	static CStr* _pViewer;
	static array<IModuleTool^>^ _toolConfig;
	static array<IModuleTool^>^ _toolDialog;
	static array<IModuleTool^>^ _toolDisk;
	static array<IModuleTool^>^ _toolEditor;
	static array<IModuleTool^>^ _toolPanels;
	static array<IModuleTool^>^ _toolViewer;
	static CStr* _prefixes;
	static List<IModuleCommand^> _registeredCommand;
	static List<IModuleEditor^> _registeredEditor;
	static List<IModuleFiler^> _registeredFiler;
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
