
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2011 FarNet Team
*/

#include "StdAfx.h"
#include "Far0.h"
#include "Dialog.h"
#include "Panel0.h"
#include "Panel2.h"
#include "Shelve.h"
#include "Wrappers.h"

namespace FarNet
{;
const int CoreConfigItemCount = 1; //config//

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

	virtual void RegisterProxyFiler(IModuleFiler^ info) override
	{
		Far0::RegisterProxyFiler(info);
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

void Far0::Start()
{
	// init the registry path
	String^ path = gcnew String(Info.RootKey);
	Works::WinRegistry::RegistryPath = path->Substring(0, path->LastIndexOf('\\'));

	// init async operations
	_hMutex = CreateMutex(NULL, FALSE, NULL);

	// init my hotkey
	_hotkey = Works::WinRegistry::GetValue("PluginHotkeys\\Plugins/FarNet/FarNetMan.dll", "Hotkey", String::Empty)->ToString();

	// connect the host
	Works::Host::Instance = gcnew FarNetHost();

	// module path
	path = Configuration::GetString(Configuration::Modules);
	if (!path)
		path = Environment::ExpandEnvironmentVariables("%FARHOME%\\FarNet\\Modules");

	// load
	Works::ModuleLoader::LoadModules(path);
}

//! Don't use Far UI
void Far0::Stop()
{
	CloseHandle(_hMutex);
	Works::ModuleLoader::UnloadModules();

	delete[] _pConfig;
	delete[] _pDisk;
	delete[] _pDialog;
	delete[] _pEditor;
	delete[] _pPanels;
	delete[] _pViewer;
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

	IModuleFiler^ filer = dynamic_cast<IModuleFiler^>(action);
	if (filer)
	{
		_registeredFiler.Remove(filer);
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
		delete[] _pConfig;
		_pConfig = 0;
	}

	if (int(options & ModuleToolOptions::Dialog))
	{
		_toolDialog = nullptr;
		delete[] _pDialog;
		_pDialog = 0;
	}

	if (int(options & ModuleToolOptions::Disk))
	{
		_toolDisk = nullptr;
		delete[] _pDisk;
		_pDisk = 0;
	}

	if (int(options & ModuleToolOptions::Editor))
	{
		_toolEditor = nullptr;
		delete[] _pEditor;
		_pEditor = 0;
	}

	if (int(options & ModuleToolOptions::Panels))
	{
		_toolPanels = nullptr;
		delete[] _pPanels;
		_pPanels = 0;
	}

	if (int(options & ModuleToolOptions::Viewer))
	{
		_toolViewer = nullptr;
		delete[] _pViewer;
		_pViewer = 0;
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

void Far0::RegisterProxyFiler(IModuleFiler^ info)
{
	Log::Source->TraceInformation("Register {0}", info);

	Works::Host::Actions->Add(info->Id, info);

	_registeredFiler.Add(info);
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

// http://forum.farmanager.com/viewtopic.php?f=7&t=3890
// (?? it would be nice to have ACTL_POSTCALLBACK)
*/
void Far0::AsGetPluginInfo(PluginInfo* pi)
{
	// _110118_073431 SysID for CallPlugin. Not quite legal but there is no better effective way.
	pi->Reserved = 0xcd;

	//! STOP
	// Do not ignore these methods even in stepping mode:
	// *) plugins can change this during stepping and Far has to be informed;
	// *) there is no more or less noticeable performance gain at all, really.
	// We still can do that with some global flags telling that something was changed, but with
	// not at all performance gain it would be more complexity for nothing. The code is disabled.

	// get window type, this is the only known way to get the current area
	// (?? wish to have 'from' parameter)
	WindowInfo wi;
	wi.Pos = -1;
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_GETSHORTWINDOWINFO, &wi))
		wi.Type = -1;

	//! Do not forget to add FarNet menus first -> alloc one more and use shifted index.

	//config// Add top menu items
	{
		if (!_toolConfig)
		{
			_toolConfig = Works::Host::GetTools(ModuleToolOptions::Config);
			_pConfig = new CStr[_toolConfig->Length + CoreConfigItemCount];

			//! mind sort order of items on changing names
			_pConfig[0].Set(Res::MenuPrefix);

			for(int i = _toolConfig->Length; --i >= 0;)
				_pConfig[i + CoreConfigItemCount].Set(GetMenuText(_toolConfig[i]));
		}

		pi->PluginConfigStringsNumber = _toolConfig->Length + CoreConfigItemCount;
		pi->PluginConfigStrings = (const wchar_t**)_pConfig;
	}

	// disk (do not add .NET item!)
	{
		if (!_toolDisk)
		{
			_toolDisk = Works::Host::GetTools(ModuleToolOptions::Disk);
			if (_toolDisk->Length > 0)
			{
				_pDisk = new CStr[_toolDisk->Length];

				//! Use just Name, not menu text, and no prefix.
				for(int i = _toolDisk->Length; --i >= 0;)
					_pDisk[i].Set(_toolDisk[i]->Name);
			}
		}

		pi->DiskMenuStringsNumber = _toolDisk->Length;
		pi->DiskMenuStrings = (const wchar_t**)_pDisk;
	}

	// type
	switch(wi.Type)
	{
	case WTYPE_DIALOG:
		{
			if (!_toolDialog)
			{
				_toolDialog = Works::Host::GetTools(ModuleToolOptions::Dialog);
				_pDialog = new CStr[_toolDialog->Length + 1];
				_pDialog[0].Set(Res::MenuPrefix);

				for(int i = _toolDialog->Length; --i >= 0;)
					_pDialog[i + 1].Set(GetMenuText(_toolDialog[i]));
			}

			pi->PluginMenuStringsNumber = _toolDialog->Length + 1;
			pi->PluginMenuStrings = (const wchar_t**)_pDialog;
		}
		break;
	case WTYPE_EDITOR:
		{
			if (!_toolEditor)
			{
				_toolEditor = Works::Host::GetTools(ModuleToolOptions::Editor);
				_pEditor = new CStr[_toolEditor->Length + 1];
				_pEditor[0].Set(Res::MenuPrefix);

				for(int i = _toolEditor->Length; --i >= 0;)
					_pEditor[i + 1].Set(GetMenuText(_toolEditor[i]));
			}

			pi->PluginMenuStringsNumber = _toolEditor->Length + 1;
			pi->PluginMenuStrings = (const wchar_t**)_pEditor;
		}
		break;
	case WTYPE_PANELS:
		{
			if (!_toolPanels)
			{
				_toolPanels = Works::Host::GetTools(ModuleToolOptions::Panels);
				_pPanels = new CStr[_toolPanels->Length + 1];
				_pPanels[0].Set(Res::MenuPrefix);

				for(int i = _toolPanels->Length; --i >= 0;)
					_pPanels[i + 1].Set(GetMenuText(_toolPanels[i]));
			}

			pi->PluginMenuStringsNumber = _toolPanels->Length + 1;
			pi->PluginMenuStrings = (const wchar_t**)_pPanels;
		}
		break;
	case WTYPE_VIEWER:
		{
			if (!_toolViewer)
			{
				_toolViewer = Works::Host::GetTools(ModuleToolOptions::Viewer);
				_pViewer = new CStr[_toolViewer->Length + 1];
				_pViewer[0].Set(Res::MenuPrefix);

				for(int i = _toolViewer->Length; --i >= 0;)
					_pViewer[i + 1].Set(GetMenuText(_toolViewer[i]));
			}

			pi->PluginMenuStringsNumber = _toolViewer->Length + 1;
			pi->PluginMenuStrings = (const wchar_t**)_pViewer;
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

bool Far0::AsConfigure(int itemIndex) //config//
{
	if (itemIndex == 0)
	{
		OpenConfig();
		return true;
	}

	// STOP: If it is called by [ShiftF9] from a F11-menu then Far sends the
	// index from that menu, not from our config items. There is nothing we can
	// do about it: the same method is called from the config menu. All we can
	// do is to check sanity of the index and ignore invalid values.
	itemIndex -= CoreConfigItemCount;
	if (itemIndex >= _toolConfig->Length)
		return false;

	IModuleTool^ tool = _toolConfig[itemIndex];
	ModuleToolEventArgs e;
	e.From = ModuleToolOptions::Config;
	tool->Invoke(nullptr, %e);
	return e.Ignore ? false : true;
}

HANDLE Far0::AsOpenFilePlugin(wchar_t* name, const unsigned char* data, int dataSize, int opMode)
{
	if (_registeredFiler.Count == 0)
		return INVALID_HANDLE_VALUE;

	Panel0::BeginOpenMode();
	ValueUserScreen userscreen; //_100514_000000

	try
	{
		ModuleFilerEventArgs^ e;
		for each(IModuleFiler^ it in _registeredFiler)
		{
			// create?
			if (!name && !it->Creates)
				continue;

			// mask?
			if (SS(it->Mask) && !CompareNameExclude(it->Mask, name, true))
				continue;

			// arguments
			if (!e)
			{
				e = gcnew ModuleFilerEventArgs;
				e->Name = gcnew String(name);
				e->Mode = (ExplorerModes)opMode;
				e->Data = gcnew UnmanagedMemoryStream((unsigned char*)data, dataSize, dataSize, FileAccess::Read);
			}
			else
			{
				e->Data->Seek(0, SeekOrigin::Begin);
			}

			// invoke
			it->Invoke(nullptr, e);

			// open a posted panel
			if (Panel0::PostedPanel)
			{
				HANDLE h = Panel0::AddPluginPanel(Panel0::PostedPanel);
				return h;
			}
		}

		return INVALID_HANDLE_VALUE;
	}
	finally
	{
		Panel0::EndOpenMode();
		if (userscreen.Get()) //_100514_000000
			Far::Net->UI->SaveUserScreen();
	}
}

HANDLE Far0::AsOpenPlugin(int from, INT_PTR item)
{
	Panel0::BeginOpenMode();
	ValueUserScreen userscreen; //_100514_000000

	// call a plugin; it may create a panel waiting for opening
	try
	{
		switch(from)
		{
		default:
			{
				// _110118_073431
				if ((from & (OPEN_FROMMACRO | OPEN_FROMMACROSTRING)) == (OPEN_FROMMACRO | OPEN_FROMMACROSTRING))
				{
					Log::Source->TraceInformation("OPEN_FROMMACRO");
					if (!InvokeCommand((const wchar_t*)item, (MacroArea)(1 + (from & OPEN_FROM_MASK)))) //_100201_110148
						return 0;
				}
			}
			break;
		case OPEN_COMMANDLINE:
			{
				Log::Source->TraceInformation("OPEN_COMMANDLINE");
				InvokeCommand((const wchar_t*)item, MacroArea::None);
			}
			break;
		case OPEN_DISKMENU:
			{
				Log::Source->TraceInformation("OPEN_DISKMENU");
				IModuleTool^ tool = _toolDisk[(int)item];
				ModuleToolEventArgs e;
				e.From = ModuleToolOptions::Disk;
				tool->Invoke(nullptr, %e);
			}
			break;
		case OPEN_PLUGINSMENU:
			{
				if (item == 0)
				{
					OpenMenu(ModuleToolOptions::Panels);
					break;
				}

				Log::Source->TraceInformation("OPEN_PLUGINSMENU");

				IModuleTool^ tool = _toolPanels[(int)item - 1];
				ModuleToolEventArgs e;
				e.From = ModuleToolOptions::Panels;
				tool->Invoke(nullptr, %e);
			}
			break;
		case OPEN_EDITOR:
			{
				if (item == 0)
				{
					OpenMenu(ModuleToolOptions::Editor);
					break;
				}

				Log::Source->TraceInformation("OPEN_EDITOR");
				IModuleTool^ tool = _toolEditor[(int)item - 1];
				ModuleToolEventArgs e;
				e.From = ModuleToolOptions::Editor;
				tool->Invoke(nullptr, %e);
			}
			break;
		case OPEN_VIEWER:
			{
				if (item == 0)
				{
					OpenMenu(ModuleToolOptions::Viewer);
					break;
				}

				Log::Source->TraceInformation("OPEN_VIEWER");
				IModuleTool^ tool = _toolViewer[(int)item - 1];
				ModuleToolEventArgs e;
				e.From = ModuleToolOptions::Viewer;
				tool->Invoke(nullptr, %e);
			}
			break;
		//! STOP: dialog case is different
		case OPEN_DIALOG:
			{
				const OpenDlgPluginData* dd = (const OpenDlgPluginData*)item;

				// just to be sure, see also _091127_112807
				FarDialog::_hDlgTop = dd->hDlg;

				int index = dd->ItemNumber;
				if (index == 0)
				{
					OpenMenu(ModuleToolOptions::Dialog);
					break;
				}

				Log::Source->TraceInformation("OPEN_DIALOG");
				IModuleTool^ tool = _toolDialog[index - 1];
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

void Far0::AssertHotkeys()
{
	if (!_hotkeys)
	{
		if (ES(_hotkey))
			throw gcnew InvalidOperationException(Res::ErrorNoHotKey);

		array<int>^ keys = gcnew array<int>(2);
		keys[1] = Far::Net->NameToKey(_hotkey);
		keys[0] = Far::Net->NameToKey("F11");
		_hotkeys = keys;
	}
}

// _100411_022932 Why PostStep is better than PostJob: PostStep makes FarNet to
// be called from OpenPlugin, so that it can open panels and do most of needed
// tasks. PostJob does not allow to open panels, to call PostMacro, etc.
// Workarounds (to post steps as steps or as jobs depending on X) are not neat.
// Thus, wait for a good CallPlugin in Far or for some other new features.
void Far0::PostStep(EventHandler^ handler)
{
	// ensure keys
	AssertHotkeys();

	// post handler and keys
	_handler = handler;
	Far::Net->PostKeySequence(_hotkeys);
}

void Far0::PostStepAfterKeys(String^ keys, EventHandler^ handler)
{
	// ensure keys
	AssertHotkeys();

	// post the handler, keys and hotkeys
	_handler = handler;
	Far::Net->PostKeys(keys);
	Far::Net->PostKeySequence(_hotkeys);
}

void Far0::PostStepAfterStep(EventHandler^ handler1, EventHandler^ handler2)
{
	// ensure keys
	AssertHotkeys();

	// post the second handler, keys and invoke the first handler
	_handler = handler2;
	Far::Net->PostKeySequence(_hotkeys);
	try
	{
		handler1->Invoke(nullptr, nullptr);
	}
	catch(...)
	{
		//! 'F11 <hotkey>' is already posted and will trigger the menu; so, let's use a fake step
		_handler = gcnew EventHandler(&VoidStep);
		throw;
	}
}

void Far0::OpenMenu(ModuleToolOptions from)
{
	// process and drop a posted step handler
	if (_handler)
	{
		EventHandler^ handler = _handler;
		_handler = nullptr;
		handler(nullptr, nullptr);
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
	menu->Add(String::Format(format, Res::ModuleMenuTools, tools.Count));
	menu->Add(String::Format(format, Res::ModuleCommands, _registeredCommand.Count));
	menu->Add(String::Format(format, Res::ModuleEditors, _registeredEditor.Count));
	menu->Add(String::Format(format, Res::ModuleFilers, _registeredFiler.Count));
	menu->Add("Settings")->IsSeparator = true;
	menu->Add("UI culture");

	while(menu->Show())
	{
		switch(menu->Selected)
		{
		case 0:
			if (tools.Count)
				Works::ConfigTool::Show(%tools, Far0::_helpTopic + "ConfigTool", gcnew Works::GetMenuText(&Far0::GetMenuText));
			break;
		case 1:
			if (_registeredCommand.Count)
				Works::ConfigCommand::Show(%_registeredCommand, Far0::_helpTopic + "ConfigCommand");
			break;
		case 2:
			if (_registeredEditor.Count)
				Works::ConfigEditor::Show(%_registeredEditor, Far0::_helpTopic + "ConfigEditor");
			break;
		case 3:
			if (_registeredFiler.Count)
				Works::ConfigFiler::Show(%_registeredFiler, Far0::_helpTopic + "ConfigFiler");
			break;
		case 5: // mind separator
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

void Far0::AsProcessSynchroEvent(int type, void* /*param*/)
{
	if (type != SE_COMMONSYNCHRO)
		return;

	WaitForSingleObject(_hMutex, INFINITE);
	try
	{
		//! handlers can be added during calls, don't use 'for each'
		while(_syncHandlers.Count)
		{
			EventHandler^ handler = _syncHandlers[0];
			_syncHandlers.RemoveAt(0);

			Log::Source->TraceInformation("AsProcessSynchroEvent: {0}", gcnew LogHandler(handler));
			handler(nullptr, nullptr);
		}
	}
	finally
	{
		_syncHandlers.Clear();

		ReleaseMutex(_hMutex);
	}
}

void Far0::PostJob(EventHandler^ handler)
{
	if (!handler)
		throw gcnew ArgumentNullException("handler");

	WaitForSingleObject(_hMutex, INFINITE);
	try
	{
		if (_syncHandlers.IndexOf(handler) >= 0)
		{
			Log::Source->TraceInformation("PostJob: skip already posted {0}", gcnew LogHandler(handler));
			return;
		}

		Log::Source->TraceInformation("PostJob: call ACTL_SYNCHRO and post {0}", gcnew LogHandler(handler));

		_syncHandlers.Add(handler);
		if (_syncHandlers.Count == 1)
			Info.AdvControl(Info.ModuleNumber, ACTL_SYNCHRO, 0);
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

	// a few known cases
	if (lang == "English")
		return _currentUICulture = CultureInfo::GetCultureInfo("en");
	if (lang == "Russian")
		return _currentUICulture = CultureInfo::GetCultureInfo("ru");
	if (lang == "Czech")
		return _currentUICulture = CultureInfo::GetCultureInfo("cs");
	if (lang == "German")
		return _currentUICulture = CultureInfo::GetCultureInfo("de");
	if (lang == "Hungarian")
		return _currentUICulture = CultureInfo::GetCultureInfo("hu");
	if (lang == "Polish")
		return _currentUICulture = CultureInfo::GetCultureInfo("pl");

	// find by name
	for each(CultureInfo^ ci in CultureInfo::GetCultures(CultureTypes::NeutralCultures))
	{
		if (ci->EnglishName == lang)
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
	return String::Format("{0} &{1} {2}", Res::MenuPrefix, tool->Hotkey, tool->Name);
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
	menu->Title = ".NET tools";

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

	menu->BreakKeys->Add(VKeyCode::Spacebar);
	
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
		
		if (menu->BreakKey != VKeyCode::Spacebar)
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
	void Invoke(Object^, EventArgs^)
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
			EventHandler^ handler = gcnew EventHandler(gcnew CommandJob(it, e), &CommandJob::Invoke);
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
