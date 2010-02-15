/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once
#undef CreateDialog

namespace FarNet
{;
ref class ModuleCommandInfo;
ref class ModuleEditorInfo;
ref class ModuleFilerInfo;
ref class ModuleToolInfo;

ref class Far0
{
public: 
	static bool AsConfigure(int itemIndex);
	static HANDLE AsOpenFilePlugin(wchar_t* name, const unsigned char* data, int dataSize, int opMode);
	static HANDLE AsOpenPlugin(int from, INT_PTR item);
	static void AsGetPluginInfo(PluginInfo* pi);
	static void AsProcessSynchroEvent(int type, void* param);
public: 
	static void RegisterCommands(IEnumerable<ModuleCommandInfo^>^ commands);
	static void RegisterEditors(IEnumerable<ModuleEditorInfo^>^ editors);
	static void RegisterFilers(IEnumerable<ModuleFilerInfo^>^ filers);
	static void RegisterTool(ModuleToolInfo^ tool);
	static void RegisterTools(IEnumerable<ModuleToolInfo^>^ tools);
	static void Start();
	static void Stop();
	static void UnregisterCommand(EventHandler<ModuleCommandEventArgs^>^ handler);
	static void UnregisterFiler(EventHandler<ModuleFilerEventArgs^>^ handler);
	static void UnregisterTool(EventHandler<ModuleToolEventArgs^>^ handler);
public:
	static CultureInfo^ GetCurrentUICulture(bool update);
	static int GetPaletteColor(PaletteColor paletteColor);
	static String^ RegisterCommand(BaseModuleEntry^ entry, String^ name, String^ prefix, EventHandler<ModuleCommandEventArgs^>^ handler);
	static void OnEditorOpened(IEditor^ editor);
	static void PostJob(EventHandler^ handler);
	static void PostStep(EventHandler^ handler);
	static void PostStepAfterKeys(String^ keys, EventHandler^ handler);
	static void PostStepAfterStep(EventHandler^ handler1, EventHandler^ handler2);
	static void RegisterFiler(BaseModuleEntry^ entry, String^ name, EventHandler<ModuleFilerEventArgs^>^ handler, String^ mask, bool creates);
	static void Run(String^ command);
public:
	static String^ _folder = Path::GetDirectoryName((Assembly::GetExecutingAssembly())->Location);
	static String^ _helpTopic = "<" + _folder + "\\>";
private:
	static bool CompareName(String^ mask, const wchar_t* name, bool skipPath);
	static bool CompareNameEx(String^ mask, const wchar_t* name, bool skipPath);
	static Object^ GetFarValue(String^ keyPath, String^ valueName, Object^ defaultValue);
	static void AssertHotkeys();
	static void Free(ModuleToolOptions options);
	static void OnConfigCommand();
	static void OnConfigEditor();
	static void OnConfigFiler();
	static void OnConfigTool(String^ title, ModuleToolOptions option, List<ModuleToolInfo^>^ list);
	static void OnConfigUICulture();
	static void OpenConfig();
	static void OpenMenu(ModuleToolOptions from);
	static void ProcessPrefixes(INT_PTR item);
	static void VoidStep(Object^, EventArgs^) {}
private:
	static CStr* _pConfig;
	static CStr* _pDisk;
	static CStr* _pDialog;
	static CStr* _pEditor;
	static CStr* _pPanels;
	static CStr* _pViewer;
	static CStr* _prefixes;
	static List<ModuleCommandInfo^> _registeredCommand;
	static List<ModuleEditorInfo^> _registeredEditor;
	static List<ModuleFilerInfo^> _registeredFiler;
	static List<ModuleToolInfo^> _toolConfig;
	static List<ModuleToolInfo^> _toolDisk;
	static List<ModuleToolInfo^> _toolDialog;
	static List<ModuleToolInfo^> _toolEditor;
	static List<ModuleToolInfo^> _toolPanels;
	static List<ModuleToolInfo^> _toolViewer;
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
