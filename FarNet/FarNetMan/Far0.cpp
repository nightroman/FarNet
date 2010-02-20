/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "Far0.h"
#include "Dialog.h"
#include "ModuleLoader.h"
#include "ModuleManager.h"
#include "ModuleProxy.h"
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

void Far0::UnregisterModuleAction(Guid id)
{
	ProxyAction^ action;
	if (!ModuleLoader::Actions->TryGetValue(id, action))
		return;

	LOG_INFO("Unregister " + action);

	ModuleLoader::Actions->Remove(id);

	ProxyCommand^ command = dynamic_cast<ProxyCommand^>(action);
	if (command)
	{
		_registeredCommand.Remove(command);
		delete _prefixes;
		_prefixes = 0;
		return;
	}

	ProxyEditor^ editor = dynamic_cast<ProxyEditor^>(action);
	if (editor)
	{
		_registeredEditor.Remove(editor);
		return;
	}

	ProxyFiler^ filer = dynamic_cast<ProxyFiler^>(action);
	if (filer)
	{
		_registeredFiler.Remove(filer);
		return;
	}

	ProxyTool^ tool = (ProxyTool^)action;
	if (_toolConfig.Remove(tool))
	{
		delete[] _pConfig;
		_pConfig = 0;
	}
	if (_toolDisk.Remove(tool))
	{
		delete[] _pDisk;
		_pDisk = 0;
	}
	if (_toolDialog.Remove(tool))
	{
		delete[] _pDialog;
		_pDialog = 0;
	}
	if (_toolEditor.Remove(tool))
	{
		delete[] _pEditor;
		_pEditor = 0;
	}
	if (_toolPanels.Remove(tool))
	{
		delete[] _pPanels;
		_pPanels = 0;
	}
	if (_toolViewer.Remove(tool))
	{
		delete[] _pViewer;
		_pViewer = 0;
	}
}

void Far0::AddModuleCommandInfo(ProxyCommand^ info)
{
	LOG_INFO("Register " + info);
	ModuleLoader::Actions->Add(info->Id, info);

	_registeredCommand.Add(info);
	delete _prefixes;
	_prefixes = 0;
}

void Far0::AddModuleEditorInfo(ProxyEditor^ info)
{
	LOG_INFO("Register " + info);
	ModuleLoader::Actions->Add(info->Id, info);

	_registeredEditor.Add(info);
}

void Far0::AddModuleFilerInfo(ProxyFiler^ info)
{
	LOG_INFO("Register " + info);
	ModuleLoader::Actions->Add(info->Id, info);

	_registeredFiler.Add(info);
}

void Far0::AddModuleToolInfo(ProxyTool^ info)
{
	LOG_INFO("Register " + info);
	ModuleLoader::Actions->Add(info->Id, info);

	ModuleToolOptions options = info->Attribute->Options;
	if (int(options & ModuleToolOptions::Config))
	{
		_toolConfig.Add(info);
		delete[] _pConfig;
		_pConfig = 0;
	}
	if (int(options & ModuleToolOptions::Disk))
	{
		_toolDisk.Add(info);
		delete[] _pDisk;
		_pDisk = 0;
	}
	if (int(options & ModuleToolOptions::Dialog))
	{
		_toolDialog.Add(info);
		delete[] _pDialog;
		_pDialog = 0;
	}
	if (int(options & ModuleToolOptions::Editor))
	{
		_toolEditor.Add(info);
		delete[] _pEditor;
		_pEditor = 0;
	}
	if (int(options & ModuleToolOptions::Panels))
	{
		_toolPanels.Add(info);
		delete[] _pPanels;
		_pPanels = 0;
	}
	if (int(options & ModuleToolOptions::Viewer))
	{
		_toolViewer.Add(info);
		delete[] _pViewer;
		_pViewer = 0;
	}
}

void Far0::Run(String^ command)
{
	int colon = command->IndexOf(':', 1);
	if (colon < 0)
		return;

	for each(ProxyCommand^ it in _registeredCommand)
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
				_pConfig[i + 1].Set(Res::MenuPrefix + _toolConfig[i]->GetMenuText());
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
				_pDisk[i].Set(Res::MenuPrefix + _toolDisk[i]->GetMenuText());
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
					_pEditor[i + 1].Set(Res::MenuPrefix + _toolEditor[i]->GetMenuText());
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
					_pPanels[i + 1].Set(Res::MenuPrefix + _toolPanels[i]->GetMenuText());
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
					_pViewer[i + 1].Set(Res::MenuPrefix + _toolViewer[i]->GetMenuText());
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
					_pDialog[i + 1].Set(Res::MenuPrefix + _toolDialog[i]->GetMenuText());
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
			for each(ProxyCommand^ it in _registeredCommand)
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
	RegistryKey^ key = nullptr;
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

	//???? if it is called by [ShiftF9] from a plugin menu then Far uses index from plugin menu, not config
	if (--itemIndex >= _toolConfig.Count)
		return false;
	
	ProxyTool^ tool = _toolConfig[itemIndex];
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
		for each(ProxyFiler^ it in _registeredFiler)
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

				ProxyTool^ tool = _toolDisk[(int)item];
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

				ProxyTool^ tool = _toolPanels[(int)item - 1];
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

				ProxyTool^ tool = _toolEditor[(int)item - 1];
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

				ProxyTool^ tool = _toolViewer[(int)item - 1];
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

				ProxyTool^ tool = _toolDialog[index - 1];
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

static void AddTools(List<ProxyTool^>^ destination, List<ProxyTool^>^ source)
{
	for each(ProxyTool^ it in source)
		if (!destination->Contains(it))
			destination->Add(it);
}

void Far0::OpenConfig()
{
	IMenu^ menu = Far::Net->CreateMenu();
	menu->AutoAssignHotkeys = true;
	menu->HelpTopic = "MenuConfig";
	menu->Title = "Modules configuration";

	List<ProxyTool^> tools;
	AddTools(%tools, %_toolConfig);
	AddTools(%tools, %_toolDialog);
	AddTools(%tools, %_toolDisk);
	AddTools(%tools, %_toolEditor);
	AddTools(%tools, %_toolPanels);
	AddTools(%tools, %_toolViewer);

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
				OnConfigTool(%tools);
			break;
		case 1:
			if (_registeredCommand.Count)
				OnConfigCommand();
			break;
		case 2:
			if (_registeredEditor.Count)
				OnConfigEditor();
			break;
		case 3:
			if (_registeredFiler.Count)
				OnConfigFiler();
			break;
			// mind separator
		case 5:
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
			menu->Add(String::Format("{0} : {1}", assemblyName->PadRight(width), ModuleManager::LoadFarNetValue(assemblyName , "UICulture", String::Empty)));

		if (!menu->Show())
			return;

		// get data to show
		String^ assemblyName = ModuleLoader::AssemblyNames[menu->Selected];
		String^ cultureName = ModuleManager::LoadFarNetValue(assemblyName , "UICulture", String::Empty)->ToString();

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
			ModuleManager::SaveFarNetValue(assemblyName , "UICulture", ci->Name);

			// use the current Far culture instead of invariant
			if (ci->Name->Length == 0)
				ci = GetCurrentUICulture(true);
			
			// update the module
			ModuleManager^ manager = ModuleLoader::GetModuleManager(assemblyName);
			manager->CurrentUICulture = ci;

			// notify
			if (manager->CachedResources)
				Far::Net->Message("Some UI strings will be updated only when Far restarts.");
		}
		catch(ArgumentException^)
		{
			Far::Net->Message("Unknown culture name.");
		}
	}
}

ref class ModuleToolComparer : IComparer<ProxyTool^>
{
public:
	virtual int Compare(ProxyTool^ x, ProxyTool^ y)
	{
		return String::Compare(x->GetMenuText(), y->GetMenuText(), true, Far::Net->GetCurrentUICulture(false));
	}
};

void Far0::OnConfigTool(List<ProxyTool^>^ tools)
{
	IMenu^ menu = Far::Net->CreateMenu();
	menu->Title = "Menu tools";
	menu->HelpTopic = _helpTopic + "ConfigTool";

	int widthName = 0;
	int widthAttr = 0;
	for each(ProxyTool^ it in tools)
	{
		if (widthName < it->Attribute->Name->Length)
			widthName = it->Attribute->Name->Length;
		if (widthAttr < it->Attribute->Options.ToString()->Length)
			widthAttr = it->Attribute->Options.ToString()->Length;
	}
	widthName += 3;
	String^ format = Res::MenuPrefix + "{0,-" + widthName + "} : {1,-" + widthAttr + "} : {2}";

	for(;;)
	{
		// reset
		menu->Items->Clear();
		tools->Sort(gcnew ModuleToolComparer);

		// fill
		for each(ProxyTool^ it in tools)
		{
			FarItem^ mi = menu->Add(String::Format(format, it->GetMenuText(), it->Attribute->Options, it->Key));
			mi->Data = it;
		}

		// show
		if (!menu->Show())
			return;

		// the tool
		ProxyTool^ tool = (ProxyTool^)menu->SelectedData;

		// dialog
		IInputBox^ ib = Far::Net->CreateInputBox();
		ib->Title = "Tool options";
		ib->Prompt = "Hotkey";
		ib->Text = tool->HotkeyChar == ' ' ? String::Empty : gcnew String(gcnew array<Char> { tool->HotkeyChar });
		ib->HelpTopic = menu->HelpTopic;
		ib->EmptyEnabled = true;
		if (!ib->Show())
			continue;

		// change the hotkey
		tool->SetHotkey(ib->Text);

		// reset the menus to be updated in the affected areas
		Free(tool->Attribute->Options);
	}
}

void Far0::OnConfigCommand()
{
	IMenu^ menu = Far::Net->CreateMenu();
	menu->AutoAssignHotkeys = true;
	menu->HelpTopic = "ConfigCommand";
	menu->Title = Res::ModuleCommands;

	for(;;)
	{
		int lenPref = 0;
		int lenName = 0;
		for each(ProxyCommand^ it in _registeredCommand)
		{
			if (lenPref < it->Attribute->Prefix->Length)
				lenPref = it->Attribute->Prefix->Length;
			if (lenName < it->Attribute->Name->Length)
				lenName = it->Attribute->Name->Length;
		}
		String^ format = "{0,-" + lenPref + "} : {1,-" + lenName + "} : {2}";

		menu->Items->Clear();
		for each(ProxyCommand^ it in _registeredCommand)
		{
			FarItem^ mi = menu->Add(String::Format(format, it->Attribute->Prefix, it->Attribute->Name, it->Key));
			mi->Data = it;
		}

		if (!menu->Show())
			return;

		FarItem^ mi = menu->Items[menu->Selected];
		ProxyCommand^ it = (ProxyCommand^)mi->Data;

		IInputBox^ ib = Far::Net->CreateInputBox();
		ib->EmptyEnabled = true;
		ib->HelpTopic = _helpTopic + "ConfigCommand";
		ib->Prompt = "New prefix for: " + it->Name;
		ib->Text = it->Attribute->Prefix;
		ib->Title = "Original prefix: " + it->DefaultPrefix;

		String^ prefix = nullptr;
		while(ib->Show())
		{
			prefix = ib->Text->Trim();
			if (prefix->IndexOf(" ") >= 0 || prefix->IndexOf(":") >= 0)
			{
				Far::Net->Message("Prefix must not contain ' ' or ':'.");
				prefix = nullptr;
				continue;
			}
			break;
		}
		if (!prefix)
			continue;

		// restore original on empty
		if (prefix->Length == 0)
			prefix = it->DefaultPrefix;

		// reset
		delete _prefixes;
		_prefixes = 0;
		it->SetPrefix(prefix);
	}
}

void Far0::OnConfigEditor()
{
	IMenu^ menu = Far::Net->CreateMenu();
	menu->AutoAssignHotkeys = true;
	menu->HelpTopic = "ConfigEditor";
	menu->Title = Res::ModuleEditors;

	for each(ProxyEditor^ it in _registeredEditor)
	{
		FarItem^ mi = menu->Add(it->Key);
		mi->Data = it;
	}

	while(menu->Show())
	{
		FarItem^ mi = menu->Items[menu->Selected];
		ProxyEditor^ it = (ProxyEditor^)mi->Data;

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

	for each(ProxyFiler^ it in _registeredFiler)
	{
		FarItem^ mi = menu->Add(it->Key);
		mi->Data = it;
	}

	while(menu->Show())
	{
		FarItem^ mi = menu->Items[menu->Selected];
		ProxyFiler^ it = (ProxyFiler^)mi->Data;

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

	for each(ProxyEditor^ it in _registeredEditor)
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
