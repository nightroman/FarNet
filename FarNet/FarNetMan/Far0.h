
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

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
	static Task^ WaitSteps();
public:
	static bool MatchMask(String^ mask, const wchar_t* name, bool skipPath);
	static bool InvokeCommand(const wchar_t* command, bool isMacro);
	static CultureInfo^ GetCurrentUICulture(bool update);
	static void ChangeFontSize(bool increase);
	static void PostJob(Action^ job);
	static void PostStep(Action^ step);
	static void ShowConsoleMenu();
	static void ShowDrawersMenu();
	static void ShowMenu(ModuleToolOptions from);
public:
	static String^ _folder = Path::GetDirectoryName(Far0::typeid->Assembly->Location);
	static String^ _helpTopic = "<" + _folder + "\\>";
	static void InvalidateProxyCommand();
	static void UnregisterProxyTool(IModuleTool^ tool);
private:
	static void OpenConfig();
	static void OpenMenu(ModuleToolOptions from);
	static void PostSelf();
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
	/// Posted steps queue
	static Queue<Action^> _stepsQueue;
	/// Posted steps task source for WaitSteps()
	static TaskCompletionSource<Object^>^ _stepsTaskSource;
	// Sync
	static HANDLE _hMutex;
	static intptr_t _nextJobId;
	static Dictionary<intptr_t, Action^> _jobs;
};

}
