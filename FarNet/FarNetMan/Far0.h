
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class Far0
{
public:
	static bool AsConfigure(const ConfigureInfo* info);
	static HANDLE AsOpen(const OpenInfo* info);
	static void AsGetPluginInfo(PluginInfo* pi);
	static void AsProcessSynchroEvent(const ProcessSynchroEventInfo* info);
public:
	static void InvokeModuleEditors(IEditor^ editor, const wchar_t* fileName);
	static void RegisterProxyCommand(IModuleCommand^ info);
	static void RegisterProxyDrawer(IModuleDrawer^ info);
	static void RegisterProxyEditor(IModuleEditor^ info);
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
	static void PostStep2(Action^ handler1, Action^ handler2);
	static void ShowConsoleMenu();
	static void ShowDrawersMenu();
	static void ShowMenu(ModuleToolOptions from);
public:
	static String^ _folder = Path::GetDirectoryName((Assembly::GetExecutingAssembly())->Location);
	static String^ _helpTopic = "<" + _folder + "\\>";
	static bool CompareNameExclude(String^ mask, const wchar_t* name, bool skipPath);
	static void InvalidateProxyCommand();
	static void UnregisterProxyTool(IModuleTool^ tool);
private:
	static bool CompareName(String^ mask, const wchar_t* name, bool skipPath);
	static void OpenConfig();
	static void OpenMenu(ModuleToolOptions from);
	static void PostSelf();
	static void VoidStep() {}
	static void InvalidateProxyTool(ModuleToolOptions options);
	static String^ GetMenuText(IModuleTool^ tool);
private:
	static void FreePluginMenuItem(PluginMenuItem& p);
	static array<IModuleTool^>^ _toolConfig;
	static array<IModuleTool^>^ _toolDialog;
	static array<IModuleTool^>^ _toolDisk;
	static array<IModuleTool^>^ _toolEditor;
	static array<IModuleTool^>^ _toolPanels;
	static array<IModuleTool^>^ _toolViewer;
	static CStr* _prefixes;
	static List<IModuleCommand^> _registeredCommand;
	static List<IModuleDrawer^> _registeredDrawer;
	static List<IModuleEditor^> _registeredEditor;
private:
	static CultureInfo^ _currentUICulture;
	// Post
	static Action^ _handler;
	// Sync
	static HANDLE _hMutex;
	static Action^ _jobs;
};

}
