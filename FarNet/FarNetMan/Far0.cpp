
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

// _110628_192511 Open from a quick view panel issue
// The `from` == OPEN_VIEWER. But the menu has been created for `window` == WTYPE_PANELS.
// The `item` is related to the panel handlers. But technically it is strange to call them for not really a panel.

#include "StdAfx.h"
#include "Far0.h"
#include "Dialog.h"
#include "Panel0.h"
#include "Panel2.h"
#include "Shelve.h"
#include "Wrappers.h"

namespace FarNet
{;
static PluginMenuItem _Config;
static PluginMenuItem _Dialog;
static PluginMenuItem _Disk;
static PluginMenuItem _Editor;
static PluginMenuItem _Panels;
static PluginMenuItem _Viewer;

ref class FarNetHost : Works::Host
{
public:
	virtual void RegisterProxyCommand(IModuleCommand^ info) override
	{
		Far0::RegisterProxyCommand(info);
	}

	virtual void RegisterProxyEditor(IModuleEditor^ info) override
	{
		Far0::RegisterProxyEditor(info);
	}

	virtual void RegisterProxyTool(IModuleTool^ info) override
	{
		Far0::RegisterProxyTool(info);
	}

	virtual void UnregisterProxyAction(IModuleAction^ action) override
	{
		Far0::UnregisterProxyAction(action);
	}

	virtual void UnregisterProxyTool(IModuleTool^ tool) override
	{
		Far0::UnregisterProxyTool(tool);
	}

	virtual void InvalidateProxyCommand() override
	{
		Far0::InvalidateProxyCommand();
	}
};

void Far0::FreePluginMenuItem(PluginMenuItem& p)
{
	if (p.Count == 0) return;

	for(int i = p.Count; --i >= 0;)
		delete p.Strings[i];

	delete p.Strings;
	p.Strings = 0;
	
	delete p.Guids;
	p.Guids = 0;
	
	p.Count = 0;
}

void Far0::Start()
{
	// init async operations
	_hMutex = CreateMutex(nullptr, FALSE, nullptr);

	// connect the host
	Works::Host::Instance = gcnew FarNetHost();

	// module path
	String^ path = Configuration::GetString(Configuration::Modules);
	if (!path)
		path = Environment::ExpandEnvironmentVariables("%FARHOME%\\FarNet\\Modules");

	// load
	Works::ModuleLoader loader;
	loader.LoadModules(path);
}

//! Don't use Far UI
void Far0::Stop()
{
	CloseHandle(_hMutex);
	Works::ModuleLoader::UnloadModules();

	FreePluginMenuItem(_Config);
	FreePluginMenuItem(_Disk);
	FreePluginMenuItem(_Dialog);
	FreePluginMenuItem(_Editor);
	FreePluginMenuItem(_Panels);
	FreePluginMenuItem(_Viewer);
	delete _prefixes;
}

void Far0::UnregisterProxyAction(IModuleAction^ action)
{
	// case: tool
	IModuleTool^ tool = dynamic_cast<IModuleTool^>(action);
	if (tool)
	{
		UnregisterProxyTool(tool);
		return;
	}

	Log::Source->TraceInformation("Unregister {0}", action);

	Works::Host::Actions->Remove(action->Id);

	IModuleCommand^ command = dynamic_cast<IModuleCommand^>(action);
	if (command)
	{
		_registeredCommand.Remove(command);
		delete _prefixes;
		_prefixes = 0;
		return;
	}

	IModuleEditor^ editor = dynamic_cast<IModuleEditor^>(action);
	if (editor)
	{
		_registeredEditor.Remove(editor);
		return;
	}
}

void Far0::UnregisterProxyTool(IModuleTool^ tool)
{
	Log::Source->TraceInformation("Unregister {0}", tool);

	Works::Host::Actions->Remove(tool->Id);

	InvalidateProxyTool(tool->Options);
}

void Far0::InvalidateProxyTool(ModuleToolOptions options)
{
	if (int(options & ModuleToolOptions::Config))
	{
		_toolConfig = nullptr;
		FreePluginMenuItem(_Config);
	}

	if (int(options & ModuleToolOptions::Dialog))
	{
		_toolDialog = nullptr;
		FreePluginMenuItem(_Dialog);
	}

	if (int(options & ModuleToolOptions::Disk))
	{
		_toolDisk = nullptr;
		FreePluginMenuItem(_Disk);
	}

	if (int(options & ModuleToolOptions::Editor))
	{
		_toolEditor = nullptr;
		FreePluginMenuItem(_Editor);
	}

	if (int(options & ModuleToolOptions::Panels))
	{
		_toolPanels = nullptr;
		FreePluginMenuItem(_Panels);
	}

	if (int(options & ModuleToolOptions::Viewer))
	{
		_toolViewer = nullptr;
		FreePluginMenuItem(_Viewer);
	}
}

void Far0::RegisterProxyCommand(IModuleCommand^ info)
{
	Log::Source->TraceInformation("Register {0}", info);

	Works::Host::Actions->Add(info->Id, info);

	_registeredCommand.Add(info);
	delete _prefixes;
	_prefixes = 0;
}

void Far0::RegisterProxyEditor(IModuleEditor^ info)
{
	Log::Source->TraceInformation("Register {0}", info);

	Works::Host::Actions->Add(info->Id, info);

	_registeredEditor.Add(info);
}

void Far0::RegisterProxyTool(IModuleTool^ info)
{
	Log::Source->TraceInformation("Register {0}", info);

	Works::Host::Actions->Add(info->Id, info);

	InvalidateProxyTool(info->Options);
}

/*
-- It is called frequently to get information about menu and disk commands.
-- It is not called when FarNet is unloaded.

callplugin()
http://forum.farmanager.com/viewtopic.php?f=7&t=3890

STOP
Do not ignore these methods even in stepping mode:
*) plugins can change this during stepping and Far has to be informed;
*) there is no more or less noticeable performance gain at all, really.
We still can do that with some global flags telling that something was changed, but with
not at all performance gain it would be more complexity for nothing. The code is disabled.
*/
void Far0::AsGetPluginInfo(PluginInfo* pi)
{
	// get window type, this is the only known way to get the current area
	// (?? wish to have the 'from' parameter)
	WindowKind windowKind = Wrap::WindowGetKind();

	//! Do not forget to add FarNet menus first -> alloc one more and use shifted indexes.

	//config// Add top menu items
	{
		if (!_toolConfig)
		{
			_toolConfig = Works::Host::GetTools(ModuleToolOptions::Config);
			
			_Config.Count = _toolConfig->Length + 1;
			GUID* guids = new GUID[_Config.Count];
			_Config.Guids = guids;
			wchar_t** strings = new wchar_t*[_Config.Count];
			_Config.Strings = strings;

			//! mind sort order of items on changing names
			guids[0] = MainGuid;
			strings[0] = NewChars(Res::Menu);

			for(int i = _toolConfig->Length; --i >= 0;)
			{
				guids[i + 1] = ToGUID(_toolConfig[i]->Id);
				strings[i + 1] = NewChars(GetMenuText(_toolConfig[i]));
			}
		}
		pi->PluginConfig = _Config;
	}

	// type
	switch(windowKind)
	{
	case WTYPE_DIALOG:
		{
			if (!_toolDialog)
			{
				_toolDialog = Works::Host::GetTools(ModuleToolOptions::Dialog);
				
				_Dialog.Count = _toolDialog->Length + 1;
				GUID* guids = new GUID[_Dialog.Count];
				_Dialog.Guids = guids;
				wchar_t** strings = new wchar_t*[_Dialog.Count];
				_Dialog.Strings = strings;

				guids[0] = MainGuid;
				strings[0] = NewChars(Res::Menu);

				for(int i = _toolDialog->Length; --i >= 0;)
				{
					guids[i + 1] = ToGUID(_toolDialog[i]->Id);
					strings[i + 1] = NewChars(GetMenuText(_toolDialog[i]));
				}
			}
			pi->PluginMenu = _Dialog;
		}
		break;
	case WTYPE_EDITOR:
		{
			if (!_toolEditor)
			{
				_toolEditor = Works::Host::GetTools(ModuleToolOptions::Editor);
				
				_Editor.Count = _toolEditor->Length + 1;
				GUID* guids = new GUID[_Editor.Count];
				_Editor.Guids = guids;
				wchar_t** strings = new wchar_t*[_Editor.Count];
				_Editor.Strings = strings;

				guids[0] = MainGuid;
				strings[0] = NewChars(Res::Menu);

				for(int i = _toolEditor->Length; --i >= 0;)
				{
					guids[i + 1] = ToGUID(_toolEditor[i]->Id);
					strings[i + 1] = NewChars(GetMenuText(_toolEditor[i]));
				}
			}
			pi->PluginMenu = _Editor;
		}
		break;
	case WTYPE_VIEWER:
		{
			if (!_toolViewer)
			{
				_toolViewer = Works::Host::GetTools(ModuleToolOptions::Viewer);
				
				_Viewer.Count = _toolViewer->Length + 1;
				GUID* guids = new GUID[_Viewer.Count];
				_Viewer.Guids = guids;
				wchar_t** strings = new wchar_t*[_Viewer.Count];
				_Viewer.Strings = strings;

				guids[0] = MainGuid;
				strings[0] = NewChars(Res::Menu);

				for(int i = _toolViewer->Length; --i >= 0;)
				{
					guids[i + 1] = ToGUID(_toolViewer[i]->Id);
					strings[i + 1] = NewChars(GetMenuText(_toolViewer[i]));
				}
			}
			pi->PluginMenu = _Viewer;
		}
		break;
	case WTYPE_PANELS: //_110628_192511
		// panels menu
		{
			if (!_toolPanels)
			{
				_toolPanels = Works::Host::GetTools(ModuleToolOptions::Panels);
				
				_Panels.Count = _toolPanels->Length + 1;
				GUID* guids = new GUID[_Panels.Count];
				_Panels.Guids = guids;
				wchar_t** strings = new wchar_t*[_Panels.Count];
				_Panels.Strings = strings;

				guids[0] = MainGuid;
				strings[0] = NewChars(Res::Menu);

				for(int i = _toolPanels->Length; --i >= 0;)
				{
					guids[i + 1] = ToGUID(_toolPanels[i]->Id);
					strings[i + 1] = NewChars(GetMenuText(_toolPanels[i]));
				}
			}
			pi->PluginMenu = _Panels;
		}
		// disk menu (do not add the .NET item!)
		{
			if (!_toolDisk)
			{
				_toolDisk = Works::Host::GetTools(ModuleToolOptions::Disk);
				if (_toolDisk->Length > 0)
				{
					_Disk.Count = _toolDisk->Length;
					GUID* guids = new GUID[_toolDisk->Length];
					_Disk.Guids = guids;
					wchar_t** strings = new wchar_t*[_toolDisk->Length];
					_Disk.Strings = strings;

					//! Use just Name, not menu text, and no prefix.
					for(int i = _toolDisk->Length; --i >= 0;)
					{
						guids[i] = ToGUID(_toolDisk[i]->Id);
						strings[i] = NewChars(_toolDisk[i]->Name);
					}
				}
			}
			pi->DiskMenu = _Disk;
		}
		break;
	}

	if (_registeredCommand.Count)
	{
		if (_prefixes == 0)
		{
			String^ PrefString = String::Empty;
			for each(IModuleCommand^ it in _registeredCommand)
			{
				if (PrefString->Length > 0)
					PrefString = String::Concat(PrefString, ":");
				PrefString = String::Concat(PrefString, it->Prefix);
			}
			_prefixes = new CStr(PrefString);
		}

		pi->CommandPrefix = *_prefixes;
	}
}

//::Far callbacks

bool Far0::AsConfigure(const ConfigureInfo* info) //config//
{
	Guid guid = FromGUID(*info->Guid);
	if (guid == FromGUID(MainGuid))
	{
		OpenConfig();
		return true;
	}

	//????? STOP: If it is called by [ShiftF9] from a F11-menu then Far sends the
	// index from that menu, not from our config items. There is nothing we can
	// do about it: the same method is called from the config menu. All we can
	// do is to check sanity of the index and ignore invalid values.
	//itemIndex -= CoreConfigItemCount;
	//if (itemIndex >= _toolConfig->Length)
	//	return false;

	IModuleTool^ tool = Far::Net->GetModuleTool(guid);
	if (!tool)
		return false;
	
	ModuleToolEventArgs e;
	e.From = ModuleToolOptions::Config;
	tool->Invoke(nullptr, %e);
	return e.Ignore ? false : true;
}

HANDLE Far0::AsOpen(const OpenInfo* info)
{
	Panel0::BeginOpenMode();
	ValueUserScreen userscreen; //_100514_000000

	// call a plugin; it may create a panel waiting for opening
	try
	{
		switch(info->OpenFrom)
		{
		default:
			{
				// _110118_073431
				if ((info->OpenFrom & (OPEN_FROMMACRO | OPEN_FROMMACROSTRING)) == (OPEN_FROMMACRO | OPEN_FROMMACROSTRING))
				{
					Log::Source->TraceInformation("OPEN_FROMMACRO");
					if (!InvokeCommand((const wchar_t*)info->Data, (MacroArea)(1 + (info->OpenFrom & OPEN_FROM_MASK)))) //_100201_110148
						return 0;
				}
			}
			break;
		case OPEN_ANALYSE:
			info = info;
			break;
		case OPEN_COMMANDLINE:
			{
				Log::Source->TraceInformation("OPEN_COMMANDLINE");
				InvokeCommand((const wchar_t*)info->Data, MacroArea::None);
			}
			break;
		case OPEN_LEFTDISKMENU:
		case OPEN_RIGHTDISKMENU:
			{
				Log::Source->TraceInformation("OPEN_DISKMENU");
				IModuleTool^ tool = Far::Net->GetModuleTool(FromGUID(*info->Guid));
				ModuleToolEventArgs e;
				e.From = ModuleToolOptions::Disk;
				e.IsLeft = info->OpenFrom == OPEN_LEFTDISKMENU;
				tool->Invoke(nullptr, %e);
			}
			break;
		case OPEN_PLUGINSMENU:
			{
				Guid guid = FromGUID(*info->Guid);
				if (guid == FromGUID(MainGuid))
				{
					OpenMenu(ModuleToolOptions::Panels);
					break;
				}

				Log::Source->TraceInformation("OPEN_PLUGINSMENU");

				IModuleTool^ tool = Far::Net->GetModuleTool(guid);
				ModuleToolEventArgs e;
				e.From = ModuleToolOptions::Panels;
				tool->Invoke(nullptr, %e);
			}
			break;
		case OPEN_EDITOR:
			{
				Guid guid = FromGUID(*info->Guid);
				if (guid == FromGUID(MainGuid))
				{
					OpenMenu(ModuleToolOptions::Editor);
					break;
				}

				Log::Source->TraceInformation("OPEN_EDITOR");
				IModuleTool^ tool = Far::Net->GetModuleTool(guid);
				ModuleToolEventArgs e;
				e.From = ModuleToolOptions::Editor;
				tool->Invoke(nullptr, %e);
			}
			break;
		case OPEN_VIEWER:
			{
				Guid guid = FromGUID(*info->Guid);
				if (guid == FromGUID(MainGuid))
				{
					OpenMenu(ModuleToolOptions::Viewer);
					break;
				}

				//_110628_192511
				if (Far::Net->Window->Kind == WindowKind::Panels)
					break;

				Log::Source->TraceInformation("OPEN_VIEWER");
				IModuleTool^ tool = Far::Net->GetModuleTool(guid);
				ModuleToolEventArgs e;
				e.From = ModuleToolOptions::Viewer;
				tool->Invoke(nullptr, %e);
			}
			break;
		//! STOP: dialog case is different
		case OPEN_DIALOG:
			{
				const OpenDlgPluginData* dd = (const OpenDlgPluginData*)info->Data;

				// just to be sure, see also _091127_112807
				FarDialog::_hDlgTop = dd->hDlg;

				Guid guid = FromGUID(*info->Guid);
				if (guid == FromGUID(MainGuid))
				{
					OpenMenu(ModuleToolOptions::Dialog);
					break;
				}

				Log::Source->TraceInformation("OPEN_DIALOG");
				IModuleTool^ tool = Far::Net->GetModuleTool(guid);
				ModuleToolEventArgs e;
				e.From = ModuleToolOptions::Dialog;
				tool->Invoke(nullptr, %e);
			}
			break;
		}

		// open a posted panel
		if (Panel0::PostedPanel)
		{
			HANDLE h = Panel0::AddPluginPanel(Panel0::PostedPanel);
			return h;
		}

		// don't open a panel
		return INVALID_HANDLE_VALUE;
	}
	finally
	{
		Panel0::EndOpenMode();
		if (userscreen.Get()) //_100514_000000
			Far::Net->UI->SaveUserScreen();
	}
}

void Far0::PostSelf()
{
	Far::Net->PostMacro("F11 Menu.Select(\"FarNet\", 2) Enter");
}

// _100411_022932 Why PostStep is better than PostJob: PostStep makes FarNet to
// be called from OpenPlugin, so that it can open panels and do most of needed
// tasks. PostJob does not allow opening panels, calling PostMacro, etc.
// To post steps as steps or as jobs depending on X is ugly.
// Thus, wait for a good CallPlugin or other features.
void Far0::PostStep(Action^ handler)
{
	// post handler and recall
	_handler = handler;
	PostSelf();
}

void Far0::PostStepAfterKeys(String^ keys, Action^ handler)
{
	// post the handler, keys and hotkeys
	_handler = handler;
	Far::Net->PostMacro(keys);
	PostSelf();
}

void Far0::PostStepAfterStep(Action^ handler1, Action^ handler2)
{
	// post the second handler, then keys, and invoke the first handler
	_handler = handler2;
	PostSelf();
	try
	{
		handler1->Invoke();
	}
	catch(...)
	{
		//! self is posted and going to call the menu anyway; so, use a fake step
		_handler = gcnew Action(&VoidStep);
		throw;
	}
}

void Far0::OpenMenu(ModuleToolOptions from)
{
	// process and drop a posted step handler
	if (_handler)
	{
		Action^ handler = _handler;
		_handler = nullptr;
		handler();
		return;
	}

	// menu
	ShowMenu(from);
}

void Far0::OpenConfig() //config//
{
	IMenu^ menu = Far::Net->CreateMenu();
	menu->AutoAssignHotkeys = true;
	menu->HelpTopic = "MenuConfig";
	menu->Title = "Modules configuration";

	List<IModuleTool^> tools(Works::Host::EnumTools());

	String^ format = "{0,-10} : {1,2}";
	menu->Add(String::Format(format, Res::ModuleCommands, _registeredCommand.Count));
	menu->Add(String::Format(format, Res::ModuleEditors, _registeredEditor.Count));
	menu->Add(String::Format(format, Res::ModuleTools, tools.Count));
	menu->Add("Settings")->IsSeparator = true;
	menu->Add("UI culture");

	while(menu->Show())
	{
		switch(menu->Selected)
		{
		case 0:
			if (_registeredCommand.Count)
				Works::ConfigCommand::Show(%_registeredCommand, Far0::_helpTopic + "ConfigCommand");
			break;
		case 1:
			if (_registeredEditor.Count)
				Works::ConfigEditor::Show(%_registeredEditor, Far0::_helpTopic + "ConfigEditor");
			break;
		case 2:
			if (tools.Count)
				Works::ConfigTool::Show(%tools, Far0::_helpTopic + "ConfigTool", gcnew Func<IModuleTool^, String^>(&Far0::GetMenuText));
			break;
		case 4: // mind separator
			Works::ConfigUICulture::Show(Works::ModuleLoader::GatherModuleManagers(), Far0::_helpTopic + "ConfigUICulture");
			break;
		}
	}
}

bool Far0::CompareName(String^ mask, const wchar_t* name, bool skipPath)
{
	for each(String^ s in mask->Split(gcnew array<Char>{',', ';'}, StringSplitOptions::RemoveEmptyEntries))
	{
		PIN_NE(pin, s);
		if (Info.FSF->ProcessName(pin, (wchar_t*)name, 0, skipPath ? (PN_CMPNAME | PN_SKIPPATH) : (PN_CMPNAME)))
			return true;
	}
	return false;
}

bool Far0::CompareNameExclude(String^ mask, const wchar_t* name, bool skipPath)
{
	int i = mask->IndexOf('|');
	if (i < 0)
		return CompareName(mask, name, skipPath);
	return  CompareName(mask->Substring(0, i), name, skipPath) && !CompareName(mask->Substring(i + 1), name, skipPath);
}

void Far0::InvokeModuleEditors(IEditor^ editor, const wchar_t* fileName)
{
	if (_registeredEditor.Count == 0)
		return;

	AutoEditorInfo ei;

	for each(IModuleEditor^ it in _registeredEditor)
	{
		// mask?
		if (SS(it->Mask) && !CompareNameExclude(it->Mask, fileName, true))
			continue;

		//! tradeoff: catch all in order to call the others, too
		try
		{
			it->Invoke(editor, nullptr);
		}
		catch(Exception^ e)
		{
			Far::Net->ShowError(it->Manager->ModuleName, e);
		}
	}
}

void Far0::AsProcessSynchroEvent(const ProcessSynchroEventInfo* info)
{
	if (info->Event != SE_COMMONSYNCHRO)
		return;

	Action^ jobs = nullptr;
	WaitForSingleObject(_hMutex, INFINITE);
	try
	{
		//! handlers can be added during invoking
		jobs = _jobs;
		_jobs = nullptr;
	}
	finally
	{
		ReleaseMutex(_hMutex);
	}

	// invoke out of the lock
	if (jobs)
	{
		Log::Source->TraceInformation("AsProcessSynchroEvent: invoking job(s): {0}", gcnew Works::DelegateToString(jobs));
		jobs();
	}
}

void Far0::PostJob(Action^ handler)
{
	if (!handler)
		throw gcnew ArgumentNullException("handler");

	Works::DelegateToString log(handler);

	WaitForSingleObject(_hMutex, INFINITE);
	try
	{
		if (_jobs && _jobs != Delegate::Remove(_jobs, handler))
		{
			Log::Source->TraceInformation("PostJob: skip existing job: {0}", %log);
			return;
		}

		if (_jobs)
		{
			Log::Source->TraceInformation("PostJob: post to the queue: {0}", %log);

			_jobs += handler;
		}
		else
		{
			Log::Source->TraceInformation("PostJob: post the head job: {0}", %log);

			_jobs = handler;
			Info.AdvControl(&MainGuid, ACTL_SYNCHRO, 0, 0);
		}
	}
	finally
	{
		ReleaseMutex(_hMutex);
	}
}

CultureInfo^ Far0::GetCurrentUICulture(bool update)
{
	// get cached value
	if (_currentUICulture && !update)
		return _currentUICulture;

	// FARLANG
	String^ lang = Environment::GetEnvironmentVariable("FARLANG");

	//! Known cases.
	//! It is important to use full names like "en-US" (not "en").
	//! Neutral names are not always suitable (e.g. "en" in PowerShell).
	if (lang == "English")
		return _currentUICulture = CultureInfo::GetCultureInfo("en-US");
	if (lang == "Russian")
		return _currentUICulture = CultureInfo::GetCultureInfo("ru-RU");
	if (lang == "Czech")
		return _currentUICulture = CultureInfo::GetCultureInfo("cs-CZ");
	if (lang == "German")
		return _currentUICulture = CultureInfo::GetCultureInfo("de-DE");
	if (lang == "Hungarian")
		return _currentUICulture = CultureInfo::GetCultureInfo("hu-HU");
	if (lang == "Polish")
		return _currentUICulture = CultureInfo::GetCultureInfo("pl-PL");

	// find by *display* name
	for each(CultureInfo^ ci in CultureInfo::GetCultures(CultureTypes::NeutralCultures))
	{
		if (ci->DisplayName == lang)
			return _currentUICulture = ci;
	}

	// fallback to invariant
	return _currentUICulture = CultureInfo::InvariantCulture;
}

void Far0::InvalidateProxyCommand()
{
	delete _prefixes;
	_prefixes = 0;
}

String^ Far0::GetMenuText(IModuleTool^ tool)
{
	return tool->Name;
}

void Far0::ShowMenu(ModuleToolOptions from)
{
	String^ sPanels = "&Panels...";
	String^ sEditors = "&Editors...";
	String^ sViewers = "&Viewers...";
	String^ sConsole = "&Console...";
	String^ sSettings = "&Settings...";

	IMenu^ menu = Far::Net->CreateMenu();
	menu->HelpTopic = "MenuMain";
	menu->Title = "FarNet";

	// Panels
	if (from == ModuleToolOptions::Panels)
		menu->Add(sPanels);

	// Editors
	// Viewers
	if (from != ModuleToolOptions::Dialog)
	{
		menu->Add(sEditors);
		menu->Add(sViewers);
	}

	// Console
	menu->Add(sConsole);

	// Settings
	if (from == ModuleToolOptions::Panels)
		menu->Add(sSettings);

	if (!menu->Show())
		return;

	String^ text = menu->Items[menu->Selected]->Text;

	if (Object::ReferenceEquals(text, sSettings))
		Works::Config::SettingsUI::ShowSettings(Works::ModuleLoader::EnumSettings());
	else if (Object::ReferenceEquals(text, sPanels))
		Works::PanelTools::ShowPanelsMenu();
	else if (Object::ReferenceEquals(text, sEditors))
		Works::EditorTools::ShowEditorsMenu();
	else if (Object::ReferenceEquals(text, sViewers))
		Works::EditorTools::ShowViewersMenu();
	else
		ShowConsoleMenu();
}

void Far0::ShowConsoleMenu()
{
	IMenu^ menu = Far::Net->CreateMenu();
	menu->HelpTopic = "MenuConsole";
	menu->Title = "Console";

	menu->Add("&Decrease font size");
	menu->Add("&Increase font size");

	menu->AddKey(KeyCode::Spacebar);

	while(menu->Show())
	{
		switch(menu->Selected)
		{
		case 0:
			ChangeFontSize(false);
			break;
		case 1:
			ChangeFontSize(true);
			break;
		}

		if (menu->Key)
			return;
	}
}

typedef BOOL (WINAPI *PGetCurrentConsoleFontEx)(__in HANDLE hConsoleOutput, __in BOOL bMaximumWindow, __out PCONSOLE_FONT_INFOEX lpConsoleCurrentFontEx);
typedef BOOL (WINAPI *PSetCurrentConsoleFontEx)(__in HANDLE hConsoleOutput, __in BOOL bMaximumWindow, __out PCONSOLE_FONT_INFOEX lpConsoleCurrentFontEx);
#define GetCurrentConsoleFontEx DoNotUseDueToXP
#define SetCurrentConsoleFontEx DoNotUseDueToXP
void Far0::ChangeFontSize(bool increase)
{
	static PGetCurrentConsoleFontEx fnGetCurrentConsoleFontEx = (PGetCurrentConsoleFontEx)GetProcAddress(GetModuleHandle(L"kernel32.dll"), "GetCurrentConsoleFontEx");
	static PSetCurrentConsoleFontEx fnSetCurrentConsoleFontEx = (PSetCurrentConsoleFontEx)GetProcAddress(GetModuleHandle(L"kernel32.dll"), "SetCurrentConsoleFontEx");

	// XP or something else is wrong
	if (!fnGetCurrentConsoleFontEx || !fnSetCurrentConsoleFontEx)
		return;

	CONSOLE_FONT_INFOEX font = { sizeof(CONSOLE_FONT_INFOEX) };
	if (!fnGetCurrentConsoleFontEx(GetStdHandle(STD_OUTPUT_HANDLE), FALSE, &font))
		return;

	SHORT height = font.dwFontSize.Y;
	for(SHORT delta = 1; delta < 10; ++delta)
	{
		font.dwFontSize.X = 0;
		font.dwFontSize.Y = height + (increase ? delta : -delta);
		if (!fnSetCurrentConsoleFontEx(GetStdHandle(STD_OUTPUT_HANDLE), FALSE, &font))
			continue;
		if (!fnGetCurrentConsoleFontEx(GetStdHandle(STD_OUTPUT_HANDLE), FALSE, &font))
			return;
		if (height != font.dwFontSize.Y)
			return;
	}
}

ref class CommandJob
{
	IModuleCommand^ _proxyCommand;
	ModuleCommandEventArgs^ _args;
public:
	CommandJob(IModuleCommand^ proxyCommand, ModuleCommandEventArgs^ args)
	{
		_proxyCommand = proxyCommand;
		_args = args;
	}
	void Invoke()
	{
		_proxyCommand->Invoke(nullptr, _args);
	}
};

bool Far0::InvokeCommand(const wchar_t* command, MacroArea area)
{
	// asynchronous command
	bool isAsync = command[0] == ':';
	if (isAsync)
		++command;
	bool isAsync2 = command[0] == ':';
	if (isAsync2)
		++command;

	// find the colon
	const wchar_t* colon = wcschr(command, ':');

	// missing colon is possible on CallPlugin
	if (!colon)
		throw gcnew InvalidOperationException("Invalid module command syntax.");

	// get the prefix, find and invoke the command handler
	String^ prefix = gcnew String(command, 0, (int)(colon - command));
	for each(IModuleCommand^ it in _registeredCommand)
	{
		if (!prefix->Equals(it->Prefix, StringComparison::OrdinalIgnoreCase))
			continue;

		ModuleCommandEventArgs^ e = gcnew ModuleCommandEventArgs();
		e->Command = gcnew String(colon + 1);
		e->MacroArea = area;

		// invoke later
		if (isAsync)
		{
			Action^ handler = gcnew Action(gcnew CommandJob(it, e), &CommandJob::Invoke);
			if (isAsync2)
				Far::Net->PostJob(handler);
			else
				Far::Net->PostStep(handler);
			return true;
		}

		// invoke now
		it->Invoke(nullptr, e);
		return e->Ignore ? false : true;
	}

	// A missing prefix is not a fatal error, e.g. a module is not istalled.
	// The calling macro should be able to recover on 0 result, so return false.
	return false;
}

}
