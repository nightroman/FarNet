/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "Far0.h"
#include "Dialog.h"
#include "ModuleLoader.h"
#include "ModuleManager.h"
#include "Panel0.h"
#include "Wrappers.h"

namespace FarNet
{;
void Far0::Start()
{
	_hMutex = CreateMutex(NULL, FALSE, NULL);
	_hotkey = GetFarValue("PluginHotkeys\\Plugins/FarNet/FarNetMan.dll", "Hotkey", String::Empty)->ToString();
	ModuleLoader::LoadModules();
}

//! Don't use Far UI
void Far0::Stop()
{
	CloseHandle(_hMutex);
	ModuleLoader::UnloadModules();

	delete[] _pConfig;
	delete[] _pDisk;
	delete[] _pDialog;
	delete[] _pEditor;
	delete[] _pPanels;
	delete[] _pViewer;
	delete _prefixes;
}

void Far0::Free(ModuleToolOptions options)
{
	if (int(options & ModuleToolOptions::Config))
	{
		delete[] _pConfig;
		_pConfig = 0;
	}
	if (int(options & ModuleToolOptions::Disk))
	{
		delete[] _pDisk;
		_pDisk = 0;
	}
	if (int(options & ModuleToolOptions::Dialog))
	{
		delete[] _pDialog;
		_pDialog = 0;
	}
	if (int(options & ModuleToolOptions::Editor))
	{
		delete[] _pEditor;
		_pEditor = 0;
	}
	if (int(options & ModuleToolOptions::Panels))
	{
		delete[] _pPanels;
		_pPanels = 0;
	}
	if (int(options & ModuleToolOptions::Viewer))
	{
		delete[] _pViewer;
		_pViewer = 0;
	}
}

void Far0::RegisterTool(ModuleToolInfo^ tool)
{
	LOG_INFO("Register " + tool);

	ModuleToolOptions options = tool->Attribute->Options;
	if (int(options & ModuleToolOptions::Config))
	{
		delete[] _pConfig;
		_pConfig = 0;
		_toolConfig.Add(tool);
	}
	if (int(options & ModuleToolOptions::Disk))
	{
		delete[] _pDisk;
		_pDisk = 0;
		_toolDisk.Add(tool);
	}
	if (int(options & ModuleToolOptions::Dialog))
	{
		delete[] _pDialog;
		_pDialog = 0;
		_toolDialog.Add(tool);
	}
	if (int(options & ModuleToolOptions::Editor))
	{
		delete[] _pEditor;
		_pEditor = 0;
		_toolEditor.Add(tool);
	}
	if (int(options & ModuleToolOptions::Panels))
	{
		delete[] _pPanels;
		_pPanels = 0;
		_toolPanels.Add(tool);
	}
	if (int(options & ModuleToolOptions::Viewer))
	{
		delete[] _pViewer;
		_pViewer = 0;
		_toolViewer.Add(tool);
	}
}

void Far0::RegisterTools(IEnumerable<ModuleToolInfo^>^ tools)
{
	for each(ModuleToolInfo^ tool in tools)
		RegisterTool(tool);
}

static int RemoveByHandler(List<ModuleToolInfo^>^ list, EventHandler<ModuleToolEventArgs^>^ handler)
{
	int r = 0;
	for(int i = list->Count; --i >= 0;)
	{
		if (list[i]->HasHandler(handler))
		{
			++r;
			list->RemoveAt(i);
		}
	}
	return r;
}

void Far0::UnregisterTool(EventHandler<ModuleToolEventArgs^>^ handler)
{
	assert(handler != nullptr);
	LOG_INFO(String::Format("Unregister tool {0}", Log::Format(handler->Method)));

	if (RemoveByHandler(%_toolConfig, handler))
	{
		delete[] _pConfig;
		_pConfig = 0;
	}
	if (RemoveByHandler(%_toolDisk, handler))
	{
		delete[] _pDisk;
		_pDisk = 0;
	}
	if (RemoveByHandler(%_toolDialog, handler))
	{
		delete[] _pDialog;
		_pDialog = 0;
	}
	if (RemoveByHandler(%_toolEditor, handler))
	{
		delete[] _pEditor;
		_pEditor = 0;
	}
	if (RemoveByHandler(%_toolPanels, handler))
	{
		delete[] _pPanels;
		_pPanels = 0;
	}
	if (RemoveByHandler(%_toolViewer, handler))
	{
		delete[] _pViewer;
		_pViewer = 0;
	}
}

void Far0::RegisterCommand(IModuleManager^ manager, EventHandler<ModuleCommandEventArgs^>^ handler, ModuleCommandAttribute^ attribute)
{
	delete _prefixes;
	_prefixes = 0;
	ModuleCommandInfo^ it = gcnew ModuleCommandInfo((manager ? (ModuleManager^)manager : nullptr), handler, attribute);
	_registeredCommand.Add(it);

	LOG_INFO("Register " + it);
}

void Far0::RegisterCommands(IEnumerable<ModuleCommandInfo^>^ commands)
{
	delete _prefixes;
	_prefixes = 0;
	_registeredCommand.AddRange(commands);

	if (Log::Switch->TraceInfo)
	{
		for each(ModuleCommandInfo^ it in commands)
			LOG_INFO("Register " + it);
	}
}

void Far0::UnregisterCommand(EventHandler<ModuleCommandEventArgs^>^ handler)
{
	for(int i = _registeredCommand.Count; --i >= 0;)
	{
		if (_registeredCommand[i]->HasHandler(handler))
		{
			LOG_INFO("Unregister " + _registeredCommand[i]);

			delete _prefixes;
			_prefixes = 0;
			_registeredCommand.RemoveAt(i);
		}
	}
}

void Far0::RegisterFiler(IModuleManager^ manager, EventHandler<ModuleFilerEventArgs^>^ handler, ModuleFilerAttribute^ attribute)
{
	ModuleFilerInfo^ it = gcnew ModuleFilerInfo((manager ? (ModuleManager^)manager : nullptr), handler, attribute);
	_registeredFiler.Add(it);

	LOG_INFO("Register " + it);
}

void Far0::RegisterEditors(IEnumerable<ModuleEditorInfo^>^ editors)
{
	_registeredEditor.AddRange(editors);

	if (Log::Switch->TraceInfo)
	{
		for each(ModuleEditorInfo^ it in editors)
			LOG_INFO("Register " + it);
	}
}

void Far0::RegisterFilers(IEnumerable<ModuleFilerInfo^>^ filers)
{
	_registeredFiler.AddRange(filers);

	if (Log::Switch->TraceInfo)
	{
		for each(ModuleFilerInfo^ it in filers)
			LOG_INFO("Register " + it);
	}
}

void Far0::UnregisterFiler(EventHandler<ModuleFilerEventArgs^>^ handler)
{
	for(int i = _registeredFiler.Count; --i >= 0;)
	{
		if (_registeredFiler[i]->HasHandler(handler))
		{
			LOG_INFO("Unregister " + _registeredFiler[i]);

			_registeredFiler.RemoveAt(i);
		}
	}
}

void Far0::Run(String^ command)
{
	int colon = command->IndexOf(':', 1);
	if (colon < 0)
		return;

	for each(ModuleCommandInfo^ it in _registeredCommand)
	{
		String^ pref = it->Attribute->Prefix;
		if (colon != pref->Length || !command->StartsWith(pref, StringComparison::OrdinalIgnoreCase))
			continue;

		// invoke
		ModuleCommandEventArgs e;
		e.Command = command->Substring(colon + 1);
		it->Invoke(nullptr, %e);

		break;
	}
}

/*
It is called frequently to get information about menu and disk commands.

STOP:
Check the instance, FarNet may be "unloaded", return empty information,
but return flags, at least preloadable flag is absolutely important as cached.

// http://forum.farmanager.com/viewtopic.php?f=7&t=3890
// (?? it would be nice to have ACTL_POSTCALLBACK)
*/
void Far0::AsGetPluginInfo(PluginInfo* pi)
{
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

	// config
	{
		if (!_pConfig)
		{
			_pConfig = new CStr[_toolConfig.Count + 1];
			_pConfig[0].Set(Res::MenuPrefix);

			for(int i = _toolConfig.Count; --i >= 0;)
				_pConfig[i + 1].Set(Res::MenuPrefix + _toolConfig[i]->Alias(ModuleToolOptions::Config));
		}

		pi->PluginConfigStringsNumber = _toolConfig.Count + 1;
		pi->PluginConfigStrings = (const wchar_t**)_pConfig;
	}

	// disk (do not add .NET item!)
	{
		if (!_pDisk && _toolDisk.Count > 0)
		{
			_pDisk = new CStr[_toolDisk.Count];

			for(int i = _toolDisk.Count; --i >= 0;)
				_pDisk[i].Set(Res::MenuPrefix + _toolDisk[i]->Alias(ModuleToolOptions::Disk));
		}

		pi->DiskMenuStringsNumber = _toolDisk.Count;
		pi->DiskMenuStrings = (const wchar_t**)_pDisk;
	}

	// type
	switch(wi.Type)
	{
	case WTYPE_EDITOR:
		{
			if (!_pEditor)
			{
				_pEditor = new CStr[_toolEditor.Count + 1];
				_pEditor[0].Set(Res::MenuPrefix);

				for(int i = _toolEditor.Count; --i >= 0;)
					_pEditor[i + 1].Set(Res::MenuPrefix + _toolEditor[i]->Alias(ModuleToolOptions::Editor));
			}

			pi->PluginMenuStringsNumber = _toolEditor.Count + 1;
			pi->PluginMenuStrings = (const wchar_t**)_pEditor;
		}
		break;
	case WTYPE_PANELS:
		{
			if (!_pPanels)
			{
				_pPanels = new CStr[_toolPanels.Count + 1];
				_pPanels[0].Set(Res::MenuPrefix);

				for(int i = _toolPanels.Count; --i >= 0;)
					_pPanels[i + 1].Set(Res::MenuPrefix + _toolPanels[i]->Alias(ModuleToolOptions::Panels));
			}

			pi->PluginMenuStringsNumber = _toolPanels.Count + 1;
			pi->PluginMenuStrings = (const wchar_t**)_pPanels;
		}
		break;
	case WTYPE_VIEWER:
		{
			if (!_pViewer)
			{
				_pViewer = new CStr[_toolViewer.Count + 1];
				_pViewer[0].Set(Res::MenuPrefix);

				for(int i = _toolViewer.Count; --i >= 0;)
					_pViewer[i + 1].Set(Res::MenuPrefix + _toolViewer[i]->Alias(ModuleToolOptions::Viewer));
			}

			pi->PluginMenuStringsNumber = _toolViewer.Count + 1;
			pi->PluginMenuStrings = (const wchar_t**)_pViewer;
		}
		break;
	case WTYPE_DIALOG:
		{
			if (!_pDialog)
			{
				_pDialog = new CStr[_toolDialog.Count + 1];
				_pDialog[0].Set(Res::MenuPrefix);

				for(int i = _toolDialog.Count; --i >= 0;)
					_pDialog[i + 1].Set(Res::MenuPrefix + _toolDialog[i]->Alias(ModuleToolOptions::Dialog));
			}

			pi->PluginMenuStringsNumber = _toolDialog.Count + 1;
			pi->PluginMenuStrings = (const wchar_t**)_pDialog;
		}
		break;
	}

	if (_registeredCommand.Count)
	{
		if (_prefixes == 0)
		{
			String^ PrefString = String::Empty;
			for each(ModuleCommandInfo^ it in _registeredCommand)
			{
				if (PrefString->Length > 0)
					PrefString = String::Concat(PrefString, ":");
				PrefString = String::Concat(PrefString, it->Attribute->Prefix);
			}
			_prefixes = new CStr(PrefString);
		}

		pi->CommandPrefix = *_prefixes;
	}
}

void Far0::ProcessPrefixes(INT_PTR item)
{
	wchar_t* command = (wchar_t*)item;
	Run(gcnew String(command));
}

int Far0::GetPaletteColor(PaletteColor paletteColor)
{
	INT_PTR index = (INT_PTR)paletteColor;
	if (index < 0 || index >= COL_LASTPALETTECOLOR)
		throw gcnew ArgumentOutOfRangeException("paletteColor");
	return (int)Info.AdvControl(Info.ModuleNumber, ACTL_GETCOLOR, (void*)index);
}

Object^ Far0::GetFarValue(String^ keyPath, String^ valueName, Object^ defaultValue)
{
	RegistryKey^ key;
	try
	{
		key = Registry::CurrentUser->OpenSubKey(Far::Net->RegistryFarPath + "\\" + keyPath);
		return key ? key->GetValue(valueName, defaultValue) : defaultValue;
	}
	finally
	{
		if (key)
			key->Close();
	}
}

//::Far callbacks

bool Far0::AsConfigure(int itemIndex)
{
	if (itemIndex == 0)
	{
		OpenConfig();
		return true;
	}

	ModuleToolInfo^ tool = _toolConfig[itemIndex - 1];
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
	ValueUserScreen userscreen;

	try
	{
		ModuleFilerEventArgs^ e;
		for each(ModuleFilerInfo^ it in _registeredFiler)
		{
			// create?
			if (!name && !it->Attribute->Creates)
				continue;

			// mask?
			if (SS(it->Attribute->Mask) && !CompareNameEx(it->Attribute->Mask, name, true))
				continue;

			// arguments
			if (!e)
			{
				e = gcnew ModuleFilerEventArgs;
				e->Name = gcnew String(name);
				e->Mode = (OperationModes)opMode;
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
	}
}

HANDLE Far0::AsOpenPlugin(int from, INT_PTR item)
{
	Panel0::BeginOpenMode();
	ValueUserScreen userscreen;

	// call a plugin; it may create a panel waiting for opening
	try
	{
		switch(from)
		{
		case OPEN_COMMANDLINE:
			{
				LOG_AUTO(3, "OPEN_COMMANDLINE");

				ProcessPrefixes(item);
			}
			break;
		case OPEN_DISKMENU:
			{
				LOG_AUTO(3, "OPEN_DISKMENU");

				ModuleToolInfo^ tool = _toolDisk[(int)item];
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

				LOG_AUTO(3, "OPEN_PLUGINSMENU");

				ModuleToolInfo^ tool = _toolPanels[(int)item - 1];
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

				LOG_AUTO(3, "OPEN_EDITOR");

				ModuleToolInfo^ tool = _toolEditor[(int)item - 1];
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

				LOG_AUTO(3, "OPEN_VIEWER");

				ModuleToolInfo^ tool = _toolViewer[(int)item - 1];
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

				LOG_AUTO(3, "OPEN_DIALOG");

				ModuleToolInfo^ tool = _toolDialog[index - 1];
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
	}
}

void Far0::AssertHotkeys()
{
	if (!_hotkeys)
	{
		if (ES(_hotkey))
			throw gcnew OperationCanceledException(Res::ErrorNoHotKey);

		array<int>^ keys = gcnew array<int>(2);
		keys[1] = Far::Net->NameToKey(_hotkey);
		keys[0] = Far::Net->NameToKey("F11");
		_hotkeys = keys;
	}
}

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

	// show the panels menu or the message
	if (from == ModuleToolOptions::Panels)
		Far::Net->ShowPanelMenu(true);
	else
		Far::Net->Message("This menu is empty but it is used internally.", "FarNet");
}

void Far0::OpenConfig()
{
	IMenu^ menu = Far::Net->CreateMenu();
	menu->AutoAssignHotkeys = true;
	menu->HelpTopic = "MenuConfig";
	menu->Title = "Modules configuration";

	menu->Add(Res::ModuleCommands + " : " + (_registeredCommand.Count));
	menu->Add(Res::ModuleEditors + "  : " + (_registeredEditor.Count));
	menu->Add(Res::ModuleFilers + "   : " + (_registeredFiler.Count));
	menu->Add("Tools")->IsSeparator = true;
	menu->Add(Res::PanelsTools + "   : " + (_toolPanels.Count));
	menu->Add(Res::EditorTools + "   : " + (_toolEditor.Count));
	menu->Add(Res::ViewerTools + "   : " + (_toolViewer.Count));
	menu->Add(Res::DialogTools + "   : " + (_toolDialog.Count));
	menu->Add(Res::ConfigTools + "   : " + (_toolConfig.Count));
	menu->Add(Res::DiskTools + "     : " + (_toolDisk.Count));
	menu->Add("Settings")->IsSeparator = true;
	menu->Add("UI culture");

	while(menu->Show())
	{
		switch(menu->Selected)
		{
		case 0:
			if (_registeredCommand.Count)
				OnConfigCommand();
			break;
		case 1:
			if (_registeredEditor.Count)
				OnConfigEditor();
			break;
		case 2:
			if (_registeredFiler.Count)
				OnConfigFiler();
			break;
			// mind separator
		case 4:
			if (_toolPanels.Count)
				OnConfigTool(Res::PanelsTools, ModuleToolOptions::Panels, %_toolPanels);
			break;
		case 5:
			if (_toolEditor.Count)
				OnConfigTool(Res::EditorTools, ModuleToolOptions::Editor, %_toolEditor);
			break;
		case 6:
			if (_toolViewer.Count)
				OnConfigTool(Res::ViewerTools, ModuleToolOptions::Viewer, %_toolViewer);
			break;
		case 7:
			if (_toolDialog.Count)
				OnConfigTool(Res::DialogTools, ModuleToolOptions::Dialog, %_toolDialog);
			break;
		case 8:
			if (_toolConfig.Count)
				OnConfigTool(Res::ConfigTools, ModuleToolOptions::Config, %_toolConfig);
			break;
		case 9:
			if (_toolDisk.Count)
				OnConfigTool(Res::DiskTools, ModuleToolOptions::Disk, %_toolDisk);
			break;
		case 11:
			OnConfigUICulture();
			break;
		}
	}
}

void Far0::OnConfigUICulture()
{
	IMenu^ menu = Far::Net->CreateMenu();
	menu->Title = "Module UI culture";
	menu->HelpTopic = _helpTopic + "ConfigUICulture";
	menu->AutoAssignHotkeys = true;

	int width = 0;
	for each(String^ assemblyName in ModuleLoader::AssemblyNames)
		if (width < assemblyName->Length)
			width = assemblyName->Length;

	for(;;)
	{
		menu->Items->Clear();
		for each(String^ assemblyName in ModuleLoader::AssemblyNames)
			menu->Add(String::Format("{0} : {1}", assemblyName->PadRight(width), ModuleManager::GetFarNetValue(assemblyName , "UICulture", String::Empty)));

		if (!menu->Show())
			return;

		// get data to show
		String^ assemblyName = ModuleLoader::AssemblyNames[menu->Selected];
		String^ cultureName = ModuleManager::GetFarNetValue(assemblyName , "UICulture", String::Empty)->ToString();

		// show the input box
		IInputBox^ ib = Far::Net->CreateInputBox();
		ib->Title = assemblyName;
		ib->Prompt = "Culture name (empty = the Far culture)";
		ib->Text = cultureName;
		ib->History = "Culture";
		ib->HelpTopic = menu->HelpTopic;
		ib->EmptyEnabled = true;
		if (!ib->Show())
			continue;

		// set the culture (even the same, to refresh)
		cultureName = ib->Text->Trim();
		CultureInfo^ ci;
		try
		{
			// get the culture by name, it may throw
			ci = CultureInfo::GetCultureInfo(cultureName);

			// save the name from the culture, not from a user
			ModuleManager::SetFarNetValue(assemblyName , "UICulture", ci->Name);

			// use the current Far culture instead of invariant
			if (ci->Name->Length == 0)
				ci = GetCurrentUICulture(true);
			
			// update the module
			ModuleManager^ manager = ModuleLoader::GetModuleManager(assemblyName);
			manager->CurrentUICulture = ci;
		}
		catch(ArgumentException^)
		{
			Far::Net->Message("Unknown culture name.");
		}
	}
}

void Far0::OnConfigTool(String^ title, ModuleToolOptions option, List<ModuleToolInfo^>^ list)
{
	IMenu^ menu = Far::Net->CreateMenu();
	menu->Title = title;
	menu->HelpTopic = _helpTopic + (option == ModuleToolOptions::Disk ? "ConfigDisk" : "ConfigTool");

	ModuleToolInfo^ selected;
	List<ModuleToolInfo^> sorted(list);
	for(;;)
	{
		menu->Items->Clear();
		sorted.Sort(gcnew ModuleToolAliasComparer(option));
		for each(ModuleToolInfo^ it in sorted)
		{
			if (ES(it->Name))
				continue;
			if (it == selected)
				menu->Selected = menu->Items->Count;
			FarItem^ mi = menu->Add(Res::MenuPrefix + it->Alias(option) + " : " + it->Key);
			mi->Data = it;
		}

		// case: disk
		if (option == ModuleToolOptions::Disk)
		{
			while(menu->Show()) {}
			return;
		}

		// show others
		if (!menu->Show())
			return;

		FarItem^ mi = menu->Items[menu->Selected];
		selected = (ModuleToolInfo^)mi->Data;

		IInputBox^ ib = Far::Net->CreateInputBox();
		ib->Title = "Original: " + selected->Name;
		ib->Prompt = "New string (ampersand ~ hotkey)";
		ib->Text = selected->Alias(option);
		ib->HelpTopic = menu->HelpTopic;
		ib->EmptyEnabled = true;
		if (!ib->Show())
			continue;

		// restore the name on empty alias
		String^ alias = ib->Text->TrimEnd();
		if (alias->Length == 0)
			alias = selected->Name;

		// reset the alias
		Free(option);
		selected->Alias(option, alias);
	}
}

void Far0::OnConfigCommand()
{
	IMenu^ menu = Far::Net->CreateMenu();
	menu->AutoAssignHotkeys = true;
	menu->HelpTopic = "ConfigCommand";
	menu->Title = Res::ModuleCommands;

	for each(ModuleCommandInfo^ it in _registeredCommand)
	{
		FarItem^ mi = menu->Add(it->Attribute->Prefix->PadRight(4) + " " + it->Key);
		mi->Data = it;
	}

	while(menu->Show())
	{
		FarItem^ mi = menu->Items[menu->Selected];
		ModuleCommandInfo^ it = (ModuleCommandInfo^)mi->Data;

		IInputBox^ ib = Far::Net->CreateInputBox();
		ib->EmptyEnabled = true;
		ib->HelpTopic = _helpTopic + "ConfigCommand";
		ib->Prompt = "New prefix for " + it->Name;
		ib->Text = it->Attribute->Prefix;
		ib->Title = "Original prefix: " + it->DefaultPrefix;

		String^ alias = nullptr;
		while(ib->Show())
		{
			alias = ib->Text->Trim();
			if (alias->IndexOf(" ") >= 0 || alias->IndexOf(":") >= 0)
			{
				Far::Net->Message("Prefix must not contain ' ' or ':'.");
				alias = nullptr;
				continue;
			}
			break;
		}
		if (!alias)
			continue;

		// restore original on empty
		if (alias->Length == 0)
			alias = it->DefaultPrefix;

		// reset
		delete _prefixes;
		_prefixes = 0;
		it->SetPrefix(alias);
		mi->Text = alias->PadRight(4) + " " + it->Key;
	}
}

void Far0::OnConfigEditor()
{
	IMenu^ menu = Far::Net->CreateMenu();
	menu->AutoAssignHotkeys = true;
	menu->HelpTopic = "ConfigEditor";
	menu->Title = Res::ModuleEditors;

	for each(ModuleEditorInfo^ it in _registeredEditor)
	{
		FarItem^ mi = menu->Add(it->Key);
		mi->Data = it;
	}

	while(menu->Show())
	{
		FarItem^ mi = menu->Items[menu->Selected];
		ModuleEditorInfo^ it = (ModuleEditorInfo^)mi->Data;

		IInputBox^ ib = Far::Net->CreateInputBox();
		ib->EmptyEnabled = true;
		ib->HelpTopic = _helpTopic + "ConfigEditor";
		ib->History = "Masks";
		ib->Prompt = "New mask for " + it->Name;
		ib->Text = it->Attribute->Mask;
		ib->Title = "Original mask: " + it->DefaultMask;

		if (!ib->Show())
			return;
		String^ mask = ib->Text->Trim();

		// restore original on empty
		if (mask->Length == 0)
			mask = it->DefaultMask;

		// set
		it->SetMask(mask);
	}
}

void Far0::OnConfigFiler()
{
	IMenu^ menu = Far::Net->CreateMenu();
	menu->AutoAssignHotkeys = true;
	menu->HelpTopic = "ConfigFiler";
	menu->Title = Res::ModuleFilers;

	for each(ModuleFilerInfo^ it in _registeredFiler)
	{
		FarItem^ mi = menu->Add(it->Key);
		mi->Data = it;
	}

	while(menu->Show())
	{
		FarItem^ mi = menu->Items[menu->Selected];
		ModuleFilerInfo^ it = (ModuleFilerInfo^)mi->Data;

		IInputBox^ ib = Far::Net->CreateInputBox();
		ib->EmptyEnabled = true;
		ib->HelpTopic = _helpTopic + "ConfigFiler";
		ib->History = "Masks";
		ib->Prompt = "New mask for " + it->Name;
		ib->Text = it->Attribute->Mask;
		ib->Title = "Original mask: " + it->DefaultMask;

		if (!ib->Show())
			return;
		String^ mask = ib->Text->Trim();

		// restore original on empty
		if (mask->Length == 0)
			mask = it->DefaultMask;

		// set
		it->SetMask(mask);
	}
}

bool Far0::CompareName(String^ mask, const wchar_t* name, bool skipPath)
{
	for each(String^ s in mask->Split(gcnew array<Char>{',', ';'}, StringSplitOptions::RemoveEmptyEntries))
	{
		PIN_NE(pin, s);
		if (Info.CmpName(pin, name, skipPath))
			return true;
	}
	return false;
}

bool Far0::CompareNameEx(String^ mask, const wchar_t* name, bool skipPath)
{
	int i = mask->IndexOf('|');
	if (i < 0)
		return CompareName(mask, name, skipPath);
	return  CompareName(mask->Substring(0, i), name, skipPath) && !CompareName(mask->Substring(i + 1), name, skipPath);
}

void Far0::OnEditorOpened(IEditor^ editor)
{
	if (_registeredEditor.Count == 0)
		return;

	AutoEditorInfo ei;

	for each(ModuleEditorInfo^ it in _registeredEditor)
	{
		// mask?
		CBox fileName(Info.EditorControl(ECTL_GETFILENAME, 0));
		Info.EditorControl(ECTL_GETFILENAME, fileName);
		if (SS(it->Attribute->Mask) && !CompareNameEx(it->Attribute->Mask, fileName, true))
			continue;

		//! tradeoff: catch all to call other plugins, too
		try
		{
			it->Invoke(editor, nullptr);
		}
		catch(Exception^ e)
		{
			//! show plugin info, too
			Far::Net->ShowError(it->Key, e);
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

			LOG_AUTO(3, String::Format("AsProcessSynchroEvent: {0}", Log::Format(handler->Method)));

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
			LOG_INFO(String::Format("PostJob: skip already posted {0}", Log::Format(handler->Method)));
			return;
		}

		LOG_INFO(String::Format("PostJob: call ACTL_SYNCHRO and post {0}", Log::Format(handler->Method)));

		_syncHandlers.Add(handler);
		if (_syncHandlers.Count == 1)
			Info.AdvControl(Info.ModuleNumber, ACTL_SYNCHRO, 0);
	}
	finally
	{
		ReleaseMutex(_hMutex);
	}
}

#undef GetEnvironmentVariable
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

	// fallback
	return _currentUICulture = CultureInfo::InvariantCulture;
}

}
