
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

// _110628_192511 Open from a quick view panel issue
// The `from` == OPEN_VIEWER. But the menu has been created for `window` == WTYPE_PANELS.
// The `item` is related to the panel handlers. But technically it is strange to call them for not really a panel.

#include "stdafx.h"
#include "Far0.h"
#include "Dialog.h"
#include "Editor.h"
#include "Panel0.h"
#include "Panel2.h"
#include "Shelve.h"
#include "Wrappers.h"

namespace FarNet
{
static PluginMenuItem _Config;
static PluginMenuItem _Dialog;
static PluginMenuItem _Disk;
static PluginMenuItem _Editor;
static PluginMenuItem _Panels;
static PluginMenuItem _Viewer;

ref class Host : Works::Host
{
public:
	virtual void RegisterProxyCommand(IModuleCommand^ info) override
	{
		Far0::RegisterProxyCommand(info);
	}
	virtual void RegisterProxyDrawer(IModuleDrawer^ info) override
	{
		Far0::RegisterProxyDrawer(info);
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
internal:
	static Host Instance;
private:
	Host() {}
};

ref class Far2 : Works::Far2
{
public:
	virtual FarNet::Works::IPanelWorks^ CreatePanel(Panel^ panel, Explorer^ explorer) override
	{
		return gcnew Panel2(panel, explorer);
	}
	virtual Task^ WaitSteps() override
	{
		return Far0::WaitSteps();
	}
	virtual WaitHandle^ PostMacroWait(String^ macro) override
	{
		return Far0::PostMacroWait(macro);
	}
internal:
	static Far2 Instance;
private:
	Far2() {}
};

void Far0::FreePluginMenuItem(PluginMenuItem& p)
{
	if (p.Count == 0) return;

	for(int i = (int)p.Count; --i >= 0;)
		delete p.Strings[i];

	delete p.Strings;
	p.Strings = 0;

	delete p.Guids;
	p.Guids = 0;

	p.Count = 0;
}

void Far0::Start()
{
	// inject
	Works::Host::Instance = % Host::Instance;
	Works::Far2::Api = % Far2::Instance;

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
	{
		IModuleTool^ it = dynamic_cast<IModuleTool^>(action);
		if (it)
		{
			UnregisterProxyTool(it);
			return;
		}
	}

	Works::Host::Actions->Remove(action->Id);

	{
		IModuleCommand^ it = dynamic_cast<IModuleCommand^>(action);
		if (it)
		{
			_registeredCommand.Remove(it);
			delete _prefixes;
			_prefixes = 0;
			return;
		}
	}
	{
		IModuleEditor^ it = dynamic_cast<IModuleEditor^>(action);
		if (it)
		{
			_registeredEditor.Remove(it);
			return;
		}
	}
	{
		IModuleDrawer^ it = dynamic_cast<IModuleDrawer^>(action);
		if (it)
		{
			_registeredDrawer.Remove(it);
			return;
		}
	}
}

void Far0::UnregisterProxyTool(IModuleTool^ tool)
{
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
	Works::Host::Actions->Add(info->Id, info);

	_registeredCommand.Add(info);
	delete _prefixes;
	_prefixes = 0;
}

void Far0::RegisterProxyDrawer(IModuleDrawer^ info)
{
	Works::Host::Actions->Add(info->Id, info);

	_registeredDrawer.Add(info);
}

void Far0::RegisterProxyEditor(IModuleEditor^ info)
{
	Works::Host::Actions->Add(info->Id, info);

	_registeredEditor.Add(info);
}

void Far0::RegisterProxyTool(IModuleTool^ info)
{
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
	case WTYPE_VMENU:
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
	IModuleTool^ tool = (IModuleTool^)Far::Api->GetModuleAction(guid);
	if (tool && (tool->Options & ModuleToolOptions::Config))
	{
		ModuleToolEventArgs e;
		e.From = ModuleToolOptions::Config;
		tool->Invoke(nullptr, %e);
		return e.Ignore ? false : true;
	}

	OpenConfig();
	return true;
}

// It may create a panel waiting for opening.
HANDLE Far0::AsOpen(const OpenInfo* info)
{
	// case: macro
	if (info->OpenFrom == OPEN_FROMMACRO)
	{
		//! do not trace yet, it is called often on tests
		OpenMacroInfo* mi = (OpenMacroInfo*)info->Data;

		// we expect a string but users may call with garbage
		if (mi->Count == 0 || mi->Values[0].Type != FMVT_STRING)
			return 0;

		// the command
		auto command = mi->Values[0].String;

		// called to signal some macro completion
		if (lstrcmpW(command, L"signal_macro") == 0)
		{
			auto wait = _macroWait.Dequeue();
			wait->Set();
			return 0;
		}

		// normal command
		Log::Source->TraceInformation("OPEN_FROMMACRO");
		if (InvokeCommand(command, true))
			return (HANDLE)1;
		else
			return 0;
	}

	// now we can open panels
	Panel0::BeginOpenMode();
	try
	{
		switch(info->OpenFrom)
		{
		case OPEN_COMMANDLINE:
			{
				Log::Source->TraceInformation("OPEN_COMMANDLINE");
				InvokeCommand(((OpenCommandLineInfo*)info->Data)->CommandLine, false);
			}
			break;
		case OPEN_LEFTDISKMENU:
		case OPEN_RIGHTDISKMENU:
			{
				Log::Source->TraceInformation("OPEN_DISKMENU");
				IModuleTool^ tool = (IModuleTool^)Far::Api->GetModuleAction(FromGUID(*info->Guid));
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

				IModuleTool^ tool = (IModuleTool^)Far::Api->GetModuleAction(guid);
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
				IModuleTool^ tool = (IModuleTool^)Far::Api->GetModuleAction(guid);
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
				if (Far::Api->Window->Kind == WindowKind::Panels)
					break;

				Log::Source->TraceInformation("OPEN_VIEWER");
				IModuleTool^ tool = (IModuleTool^)Far::Api->GetModuleAction(guid);
				ModuleToolEventArgs e;
				e.From = ModuleToolOptions::Viewer;
				tool->Invoke(nullptr, %e);
			}
			break;
		//! STOP: dialog case is different
		case OPEN_DIALOG:
			{
				Guid guid = FromGUID(*info->Guid);
				if (guid == FromGUID(MainGuid))
				{
					OpenMenu(ModuleToolOptions::Dialog);
					break;
				}

				Log::Source->TraceInformation("OPEN_DIALOG");
				IModuleTool^ tool = (IModuleTool^)Far::Api->GetModuleAction(guid);
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

		// no panel
		return nullptr;
	}
	finally
	{
		Panel0::EndOpenMode();
	}
}

void Far0::OpenConfig() //config//
{
	IMenu^ menu = Far::Api->CreateMenu();
	menu->AutoAssignHotkeys = true;
	menu->HelpTopic = "config-menu";
	menu->Title = "Modules configuration";

	List<IModuleTool^> tools(Works::Host::EnumTools());

	String^ format = "{0,-10} : {1,2}";
	menu->Add(String::Format(format, Res::ModuleCommands, _registeredCommand.Count));
	menu->Add(String::Format(format, Res::ModuleDrawers, _registeredDrawer.Count));
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
				Works::ConfigCommand::Show(%_registeredCommand, Far0::HelpTopic() + "configure-commands");
			break;
		case 1:
			if (_registeredDrawer.Count)
				Works::ConfigDrawer::Show(%_registeredDrawer, Far0::HelpTopic() + "configure-drawers");
			break;
		case 2:
			if (_registeredEditor.Count)
				Works::ConfigEditor::Show(%_registeredEditor, Far0::HelpTopic() + "configure-editors");
			break;
		case 3:
			if (tools.Count)
				Works::ConfigTool::Show(%tools, Far0::HelpTopic() + "configure-tools", gcnew Func<IModuleTool^, String^>(&Far0::GetMenuText));
			break;
		case 5: // +2, mind separator
			Works::ConfigUICulture::Show(Works::ModuleLoader::GatherModuleManagers(), Far0::HelpTopic() + "module-ui-culture");
			break;
		}
	}
}

bool Far0::MatchMask(String^ mask, const wchar_t* name, bool skipPath)
{
	PIN_NE(pin, mask);
	return 0 != Info.FSF->ProcessName(pin, (wchar_t*)name, 0, skipPath ? (PN_CMPNAMELIST | PN_SKIPPATH) : (PN_CMPNAMELIST));
}

void Far0::InvokeModuleEditors(IEditor^ editor, const wchar_t* fileName)
{
	if (_registeredDrawer.Count == 0 && _registeredEditor.Count == 0)
		return;

	AutoEditorInfo ei(-1);

	for each(IModuleDrawer^ it in _registeredDrawer)
	{
		// match
		if (ES(it->Mask) || !MatchMask(it->Mask, fileName, true))
			continue;

		// catch all in order to add the others
		try
		{
			((Editor^)editor)->AddDrawer(it);
		}
		catch(Exception^ e)
		{
			Far::Api->ShowError(it->Manager->ModuleName, e);
		}
	}

	for each(IModuleEditor^ it in _registeredEditor)
	{
		// match
		if (ES(it->Mask) || !MatchMask(it->Mask, fileName, true))
			continue;

		// catch all in order to call the others
		try
		{
			it->Invoke(editor, nullptr);
		}
		catch(Exception^ e)
		{
			Far::Api->ShowError(it->Manager->ModuleName, e);
		}
	}
}

void Far0::AsProcessSynchroEvent(const ProcessSynchroEventInfo* info)
{
	if (info->Event != SE_COMMONSYNCHRO)
		return;

	Action^ job = nullptr;
	Monitor::Enter(% _jobs);
	try
	{
		job = _jobs.Dequeue();
	}
	finally
	{
		Monitor::Exit(% _jobs);
	}

	job();
}

void Far0::PostJob(Action^ job)
{
	if (!job)
		throw gcnew ArgumentNullException("job");

	Monitor::Enter(% _jobs);
	try
	{
		_jobs.Enqueue(job);
		Info.AdvControl(&MainGuid, ACTL_SYNCHRO, 0, 0);
	}
	finally
	{
		Monitor::Exit(% _jobs);
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
	String^ sPanels = "&Panels";
	String^ sDrawers = "&Drawers";
	String^ sEditors = "&Editors";
	String^ sViewers = "&Viewers";
	String^ sConsole = "&Console";
	String^ sSettings = "&Settings";

	IMenu^ menu = Far::Api->CreateMenu();
	menu->HelpTopic = "plugin-menu";
	menu->Title = "FarNet";

	// Panels
	if (from == ModuleToolOptions::Panels)
		menu->Add(sPanels);

	// Editors
	// Viewers
	if (from != ModuleToolOptions::Dialog)
	{
		if (from == ModuleToolOptions::Editor)
			menu->Add(sDrawers);
		menu->Add(sEditors);
		menu->Add(sViewers);
	}

	// Console
	menu->Add(sConsole);

	// Settings
	menu->Add(sSettings);

	if (!menu->Show())
		return;

	String^ text = menu->Items[menu->Selected]->Text;

	if (Object::ReferenceEquals(text, sSettings))
		Works::Config::SettingsUI::ShowSettings(Works::ModuleLoader::GatherModuleManagers());
	else if (Object::ReferenceEquals(text, sPanels))
		Works::PanelTools::ShowPanelsMenu();
	else if (Object::ReferenceEquals(text, sEditors))
		Works::EditorTools::ShowEditorsMenu();
	else if (Object::ReferenceEquals(text, sViewers))
		Works::EditorTools::ShowViewersMenu();
	else if (Object::ReferenceEquals(text, sDrawers))
		ShowDrawersMenu();
	else
		ShowConsoleMenu();
}

void Far0::ShowDrawersMenu()
{
	Editor^ editor = (Editor^)Far::Api->Editor;

	IMenu^ menu = Far::Api->CreateMenu();
	menu->Title = "Drawers";
	menu->HelpTopic = "drawers-menu";

	for each(IModuleDrawer^ drawer in _registeredDrawer)
	{
		FarItem^ item = menu->Add(drawer->Name);
		item->Data = drawer;
		if (editor->_drawers && editor->_drawers->ContainsKey(drawer->Id))
			item->Checked = true;
	}

	if (!menu->Show())
		return;

	FarItem^ item = menu->Items[menu->Selected];
	if (item->Checked)
		editor->RemoveDrawer(((IModuleDrawer^)item->Data)->Id);
	else
		editor->AddDrawer((IModuleDrawer^)item->Data);
}

void Far0::ShowConsoleMenu()
{
	IMenu^ menu = Far::Api->CreateMenu();
	menu->HelpTopic = "console-menu";
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

	CONSOLE_FONT_INFOEX font = {sizeof(CONSOLE_FONT_INFOEX)};
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

bool Far0::InvokeCommand(const wchar_t* command, bool isMacro)
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

	// missing colon is possible from macro
	if (!colon)
		throw gcnew InvalidOperationException("Invalid module command syntax.");

	// get the prefix, find and invoke the command handler
	String^ prefix = gcnew String(command, 0, (int)(colon - command));
	for each(IModuleCommand^ it in _registeredCommand)
	{
		if (!prefix->Equals(it->Prefix, StringComparison::OrdinalIgnoreCase))
			continue;

		ModuleCommandEventArgs^ e = gcnew ModuleCommandEventArgs(gcnew String(colon + 1));
		e->IsMacro = isMacro;

		// invoke later
		if (isAsync)
		{
			Action^ handler = gcnew Action(gcnew CommandJob(it, e), &CommandJob::Invoke);
			if (isAsync2)
				Far::Api->PostStep(handler);
			else
				Far::Api->PostJob(handler);
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

// Plugin.Menu is not a replacement for F11, it is less predictable on posted keys and async jobs.
void Far0::PostSelf()
{
	Far::Api->PostMacro("Keys('F11') Menu.Select('FarNet', 2) Keys('Enter')", Works::Kit::MacroOutput, false);
}

//! must be sync call
void Far0::PostStep(Action^ step)
{
	_steps.Enqueue(step);
	if (_steps.Count == 1)
	{
		_stepsTask = gcnew TaskCompletionSource<Object^>;
		PostSelf();
	}
}

//! may be async call
Task^ Far0::WaitSteps()
{
	//! cache, if it is nulled by main then `->Task` NRE
	auto task = _stepsTask;
	if (task)
		return task->Task;
	else
		return Task::FromResult<Object^>(nullptr);
}

//! must be sync call, so we use the queue, first posted/added will be first signaled/removed
WaitHandle^ Far0::PostMacroWait(String^ macro)
{
	//! post 2 macros instead of 1 combined because:
	//! - clear syntax error messages without 2nd part
	//! - combined does not work any faster, so KISS
	Far::Api->PostMacro(macro);
	Far::Api->PostMacro("Plugin.SyncCall('10435532-9BB3-487B-A045-B0E6ECAAB6BC', 'signal_macro')");

	// add and return wait handle, it will be signaled and removed when signal_macro is called
	auto wait = gcnew ManualResetEvent(false);
	_macroWait.Enqueue(wait);
	return wait;
}

void Far0::OpenMenu(ModuleToolOptions from)
{
	// normal call for the menu
	if (_steps.Count == 0)
	{
		ShowMenu(from);
		return;
	}

	// invoke one posted step
	try
	{
		//! peek, to avoid premature PostSelf in PostStep possibly called by this step
		auto step = _steps.Peek();
		
		// invoke this step
		step();

		// now dequeue and PostSelf if there are more steps
		_steps.Dequeue();
		if (_steps.Count > 0)
			PostSelf();
	}
	catch (...)
	{
		// discard steps and re-throw
		_steps.Clear();
		throw;
	}
	finally
	{
		// complete the task
		if (_steps.Count == 0)
		{
			auto task = _stepsTask;
			_stepsTask = nullptr;
			task->SetResult(nullptr);
		}
	}
}
}
