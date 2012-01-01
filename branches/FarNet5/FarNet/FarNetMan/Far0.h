
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
	static void InvokeModuleEditors(IEditor^ editor, const wchar_t* fileName);
	static void RegisterProxyCommand(IModuleCommand^ info);
	static void RegisterProxyEditor(IModuleEditor^ info);
	static void RegisterProxyFiler(IModuleFiler^ info);
	static void RegisterProxyTool(IModuleTool^ info);
	static void Start();
	static void Stop();
	static void UnregisterProxyAction(IModuleAction^ action);
public:
	static bool InvokeCommand(const wchar_t* command, MacroArea area);
	static CultureInfo^ GetCurrentUICulture(bool update);
	static void ChangeFontSize(bool increase);
	static void PostJob(Action^ handler);
	static void PostStep(Action^ handler);
	static void PostStepAfterKeys(String^ keys, Action^ handler);
	static void PostStepAfterStep(Action^ handler1, Action^ handler2);
	static void ShowConsoleMenu();
	static void ShowMenu(ModuleToolOptions from);
public:
	static String^ _folder = Path::GetDirectoryName((Assembly::GetExecutingAssembly())->Location);
	static String^ _helpTopic = "<" + _folder + "\\>";
	static bool CompareNameExclude(String^ mask, const wchar_t* name, bool skipPath);
	static void InvalidateProxyCommand();
	static void UnregisterProxyTool(IModuleTool^ tool);
private:
	static bool CompareName(String^ mask, const wchar_t* name, bool skipPath);
	static void AssertHotkeys();
	static void OpenConfig();
	static void OpenMenu(ModuleToolOptions from);
	static void VoidStep() {}
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
	static Action^ _handler;
	// Sync
	static HANDLE _hMutex;
	static Action^ _jobs;
};

}
