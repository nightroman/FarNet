
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
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
	static bool MatchMask(String^ mask, const wchar_t* name, bool skipPath);
	static bool InvokeCommand(const wchar_t* command, bool isMacro);
	static CultureInfo^ GetCurrentUICulture(bool update);
	static void ChangeFontSize(bool increase);
	static void PostJob(Action^ handler);
	static void PostSteps(IEnumerable<Object^>^ steps);
	static void ShowConsoleMenu();
	static void ShowDrawersMenu();
	static void ShowMenu(ModuleToolOptions from);
public:
	static String^ _folder = Path::GetDirectoryName((Assembly::GetExecutingAssembly())->Location);
	static String^ _helpTopic = "<" + _folder + "\\>";
	static void InvalidateProxyCommand();
	static void UnregisterProxyTool(IModuleTool^ tool);
private:
	static void OpenConfig();
	static void OpenMenu(ModuleToolOptions from);
	static void PostSelf();
	static void InvalidateProxyTool(ModuleToolOptions options);
	static String^ GetMenuText(IModuleTool^ tool);
	static void DisposeSteps();
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
	// Steps
	static bool _skipStep;
	static int _levelPostSelf;
	static Stack<IEnumerator<Object^>^>^ _steps;
	// Sync
	static HANDLE _hMutex;
	static Action^ _jobs;
};

}
