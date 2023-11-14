
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

#pragma once

namespace FarNet
{
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
	static WaitHandle^ PostMacroWait(String^ macro);
public:
	static bool MatchMask(String^ mask, const wchar_t* name, bool skipPath);
	static bool InvokeCommand(const wchar_t* command, OPENFROM from);
	static CultureInfo^ GetCurrentUICulture(bool update);
	static void ChangeFontSize(bool increase);
	static void PostJob(Action^ job);
	static void PostStep(Action^ step);
	static void ShowConsoleMenu();
	static void ShowDrawersMenu();
	static void ShowMenu(ModuleToolOptions from);
public:
	static void InvalidateProxyCommand();
	static void UnregisterProxyTool(IModuleTool^ tool);
private:
	static void OpenConfig();
	static void OpenMenu(ModuleToolOptions from);
	static void PostSelf();
	static void InvalidateProxyTool(ModuleToolOptions options);
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
	/// Posted steps
	static Queue<Action^> _steps;
	/// Posted steps task
	static TaskCompletionSource<Object^>^ _stepsTask;
	/// Posted macro wait handles
	static Queue<ManualResetEvent^> _macroWait;
	/// Posted jobs
	static Queue<Action^> _jobs;
};
}
