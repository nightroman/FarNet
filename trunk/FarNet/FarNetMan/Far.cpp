/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#include "StdAfx.h"
#include "Far.h"
#include "Dialog.h"
#include "Editor0.h"
#include "InputBox.h"
#include "KeyMacroHost.h"
#include "ListMenu.h"
#include "Menu.h"
#include "Message.h"
#include "Panel0.h"
#include "Panel2.h"
#include "Plugin0.h"
#include "PluginInfo.h"
#include "Shelve.h"
#include "Viewer0.h"
#include "Zoo.h"

namespace FarNet
{;
Far::Far()
{}

void Far::StartFar()
{
	if (_instance)
		throw gcnew InvalidOperationException("Already started.");

	_instance = gcnew Far;
	_instance->Start();
}

void Far::Start()
{
	_hMutex = CreateMutex(NULL, FALSE, NULL);
	_hotkey = GetFarValue("PluginHotkeys\\Plugins/FarNet/FarNetMan.dll", "Hotkey", String::Empty)->ToString();
	Plugin0::LoadPlugins();
}

//! Don't use Far UI
void Far::Stop()
{
	CloseHandle(_hMutex);
	Plugin0::UnloadPlugins();
	_instance = nullptr;

	delete[] _pConfig;
	delete[] _pDisk;
	delete[] _pDialog;
	delete[] _pEditor;
	delete[] _pPanels;
	delete[] _pViewer;
	delete _prefixes;
}

String^ Far::ActivePath::get()
{
	IPanel^ panel = Panel;
	if (!panel)
		return String::Empty;

	FarNet::Panel2^ plugin = dynamic_cast<FarNet::Panel2^>(panel); 
	if (plugin)
		return plugin->ActivePath;
	
	return panel->Path;
}

String^ Far::PluginPath::get()
{
	String^ pluginPath = gcnew String(Info.ModuleName);
	return (gcnew FileInfo(pluginPath))->DirectoryName;
}

String^ Far::RootFar::get()
{
	String^ key = RootKey;
	return key->Substring(0, key->LastIndexOf('\\'));
}

String^ Far::RootKey::get()
{
	return gcnew String(Info.RootKey);
}

void Far::Free(ToolOptions options)
{
	if (int(options & ToolOptions::Config))
	{
		delete[] _pConfig;
		_pConfig = 0;
	}
	if (int(options & ToolOptions::Disk))
	{
		delete[] _pDisk;
		_pDisk = 0;
	}
	if (int(options & ToolOptions::Dialog))
	{
		delete[] _pDialog;
		_pDialog = 0;
	}
	if (int(options & ToolOptions::Editor))
	{
		delete[] _pEditor;
		_pEditor = 0;
	}
	if (int(options & ToolOptions::Panels))
	{
		delete[] _pPanels;
		_pPanels = 0;
	}
	if (int(options & ToolOptions::Viewer))
	{
		delete[] _pViewer;
		_pViewer = 0;
	}
}

void Far::RegisterTool(BasePlugin^ plugin, String^ name, EventHandler<ToolEventArgs^>^ handler, ToolOptions options)
{
	if (plugin && ES(name))
		throw gcnew ArgumentException("'name' must not be empty.");

	RegisterTool(gcnew ToolPluginInfo(plugin, name, handler, options));
}

void Far::RegisterTool(ToolPluginInfo^ tool)
{
	LOG_INFO("Register " + tool);

	ToolOptions options = tool->Options;
	if (int(options & ToolOptions::Config))
	{
		delete[] _pConfig;
		_pConfig = 0;
		_toolConfig.Add(tool);
	}
	if (int(options & ToolOptions::Disk))
	{
		delete[] _pDisk;
		_pDisk = 0;
		_toolDisk.Add(tool);
	}
	if (int(options & ToolOptions::Dialog))
	{
		delete[] _pDialog;
		_pDialog = 0;
		_toolDialog.Add(tool);
	}
	if (int(options & ToolOptions::Editor))
	{
		delete[] _pEditor;
		_pEditor = 0;
		_toolEditor.Add(tool);
	}
	if (int(options & ToolOptions::Panels))
	{
		delete[] _pPanels;
		_pPanels = 0;
		_toolPanels.Add(tool);
	}
	if (int(options & ToolOptions::Viewer))
	{
		delete[] _pViewer;
		_pViewer = 0;
		_toolViewer.Add(tool);
	}
}

void Far::RegisterTools(IEnumerable<ToolPluginInfo^>^ tools)
{
	for each(ToolPluginInfo^ tool in tools)
		RegisterTool(tool);
}

static int RemoveByHandler(List<ToolPluginInfo^>^ list, EventHandler<ToolEventArgs^>^ handler)
{
	int r = 0;
	for(int i = list->Count; --i >= 0;)
	{
		if (list[i]->Handler == handler)
		{
			++r;
			list->RemoveAt(i);
		}
	}
	return r;
}

void Far::UnregisterTool(EventHandler<ToolEventArgs^>^ handler)
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

String^ Far::RegisterCommand(BasePlugin^ plugin, String^ name, String^ prefix, EventHandler<CommandEventArgs^>^ handler)
{
	delete _prefixes;
	_prefixes = 0;
	CommandPluginInfo^ it = gcnew CommandPluginInfo(plugin, name, prefix, handler);
	_registeredCommand.Add(it);

	LOG_INFO("Register " + it);

	return it->Prefix;
}

void Far::RegisterCommands(IEnumerable<CommandPluginInfo^>^ commands)
{
	delete _prefixes;
	_prefixes = 0;
	_registeredCommand.AddRange(commands);

	if (Log::Switch->TraceInfo)
	{
		for each(CommandPluginInfo^ it in commands)
			LOG_INFO("Register " + it);
	}
}

void Far::UnregisterCommand(EventHandler<CommandEventArgs^>^ handler)
{
	for(int i = _registeredCommand.Count; --i >= 0;)
	{
		if (_registeredCommand[i]->Handler == handler)
		{
			LOG_INFO("Unregister " + _registeredCommand[i]);

			delete _prefixes;
			_prefixes = 0;
			_registeredCommand.RemoveAt(i);
		}
	}
}

void Far::RegisterFiler(BasePlugin^ plugin, String^ name, EventHandler<FilerEventArgs^>^ handler, String^ mask, bool creates)
{
	FilerPluginInfo^ it = gcnew FilerPluginInfo(plugin, name, handler, mask, creates);
	_registeredFiler.Add(it);

	LOG_INFO("Register " + it);
}

void Far::RegisterEditors(IEnumerable<EditorPluginInfo^>^ editors)
{
	_registeredEditor.AddRange(editors);

	if (Log::Switch->TraceInfo)
	{
		for each(EditorPluginInfo^ it in editors)
			LOG_INFO("Register " + it);
	}
}

void Far::RegisterFilers(IEnumerable<FilerPluginInfo^>^ filers)
{
	_registeredFiler.AddRange(filers);

	if (Log::Switch->TraceInfo)
	{
		for each(FilerPluginInfo^ it in filers)
			LOG_INFO("Register " + it);
	}
}

void Far::UnregisterFiler(EventHandler<FilerEventArgs^>^ handler)
{
	for(int i = _registeredFiler.Count; --i >= 0;)
	{
		if (_registeredFiler[i]->Handler == handler)
		{
			LOG_INFO("Unregister " + _registeredFiler[i]);

			_registeredFiler.RemoveAt(i);
		}
	}
}

void Far::Unregister(BasePlugin^ plugin)
{
	Plugin0::UnloadPlugin(plugin);
}

void Far::Msg(String^ body)
{
	Message::Show(body, nullptr, MsgOptions::Ok, nullptr, nullptr);
}

void Far::Msg(String^ body, String^ header)
{
	Message::Show(body, header, MsgOptions::Ok, nullptr, nullptr);
}

int Far::Msg(String^ body, String^ header, MsgOptions options)
{
	return Message::Show(body, header, options, nullptr, nullptr);
}

int Far::Msg(String^ body, String^ header, MsgOptions options, array<String^>^ buttons)
{
	return Message::Show(body, header, options, buttons, nullptr);
}

int Far::Msg(String^ body, String^ header, MsgOptions options, array<String^>^ buttons, String^ helpTopic)
{
	return Message::Show(body, header, options, buttons, helpTopic);
}

void Far::Run(String^ command)
{
	int colon = command->IndexOf(':', 1);
	if (colon < 0)
		return;

	for each(CommandPluginInfo^ it in _registeredCommand)
	{
		String^ pref = it->Prefix;
		if (colon != pref->Length || !command->StartsWith(pref, StringComparison::OrdinalIgnoreCase))
			continue;

		//! Notify before each command, because a plugin may have to set a command environment,
		//! e.g. PowerShellFar sets the default runspace once and location always.
		//! If there is a plugin, call it directly, else it has to be done by its handler.
		if (it->Plugin)
		{
			LOG_AUTO(3, String::Format("{0}.Invoking", it->Plugin));

			it->Plugin->Invoking();
		}

		// invoke
		{
			String^ code = command->Substring(colon + 1);

			LOG_AUTO(3, String::Format("{0} {1}", Log::Format(it->Handler->Method), code));

			CommandEventArgs e(code);
			it->Handler(this, %e);
		}

		break;
	}
}

IntPtr Far::HWnd::get()
{
	return (IntPtr)Info.AdvControl(Info.ModuleNumber, ACTL_GETFARHWND, nullptr);
}

System::Version^ Far::FarVersion::get()
{
	DWORD vn;
	Info.AdvControl(Info.ModuleNumber, ACTL_GETFARVERSION, &vn);
	return gcnew System::Version((vn&0x0000ff00)>>8, vn&0x000000ff, (int)((long)vn&0xffff0000)>>16);
}

System::Version^ Far::FarNetVersion::get()
{
	return Assembly::GetExecutingAssembly()->GetName()->Version;
}

IMenu^ Far::CreateMenu()
{
	return gcnew Menu;
}

IListMenu^ Far::CreateListMenu()
{
	return gcnew ListMenu;
}

FarConfirmations Far::Confirmations::get()
{
	return (FarConfirmations)Info.AdvControl(Info.ModuleNumber, ACTL_GETCONFIRMATIONS, 0);
}

FarMacroState Far::MacroState::get()
{
	ActlKeyMacro command;
	command.Command = MCMD_GETSTATE;
	return (FarMacroState)Info.AdvControl(Info.ModuleNumber, ACTL_KEYMACRO, &command);
}

array<IEditor^>^ Far::Editors()
{
	return Editor0::Editors();
}

array<IViewer^>^ Far::Viewers()
{
	return Viewer0::Viewers();
}

IAnyEditor^ Far::AnyEditor::get()
{
	return %Editor0::_anyEditor;
}

IAnyViewer^ Far::AnyViewer::get()
{
	return %Viewer0::_anyViewer;
}

String^ Far::PasteFromClipboard()
{
	wchar_t* buffer = Info.FSF->PasteFromClipboard();
	String^ r = gcnew String(buffer);
	Info.FSF->DeleteBuffer(buffer);
	return r;
}

void Far::CopyToClipboard(String^ text)
{
	PIN_NE(pin, text);
	Info.FSF->CopyToClipboard(pin);
}

IEditor^ Far::CreateEditor()
{
	return gcnew FarNet::Editor;
}

IViewer^ Far::CreateViewer()
{
	return gcnew FarNet::Viewer;
}

array<int>^ Far::CreateKeySequence(String^ keys)
{
	if (!keys) throw gcnew ArgumentNullException("keys");
	array<wchar_t>^ space = {' ', '\t', '\r', '\n'};
	array<String^>^ a = keys->Split(space, StringSplitOptions::RemoveEmptyEntries);
	array<int>^ r = gcnew array<int>(a->Length);
	for(int i = 0; i < a->Length; ++i)
	{
		int k = NameToKey(a[i]);
		if (k == -1)
			throw gcnew ArgumentException("Argument 'keys' contains invalid key: '" + a[i] + "'.");
		r[i] = k;
	}
	return r;
}

void Far::PostKeySequence(array<int>^ sequence)
{
	PostKeySequence(sequence, true);
}

//! [_090328_170110] KSFLAGS_NOSENDKEYSTOPLUGINS is not set,
//! but Tab for TabExpansion is not working in .ps1 editor, why?
void Far::PostKeySequence(array<int>^ sequence, bool disableOutput)
{
	if (sequence == nullptr) throw gcnew ArgumentNullException("sequence");
	if (sequence->Length == 0)
		return;

	// local buffer for a small sequence
	const int smallCount = 256;
	DWORD keys[smallCount];

	KeySequence keySequence;
	keySequence.Count = sequence->Length;
	keySequence.Flags = disableOutput ? KSFLAGS_DISABLEOUTPUT : 0;

	keySequence.Sequence = keySequence.Count <= smallCount ? keys : new DWORD[keySequence.Count];
	DWORD* cur = keySequence.Sequence;
	for each(int i in sequence)
	{
		*cur = i;
		++cur;
	}

	try
	{
		if (!Info.AdvControl(Info.ModuleNumber, ACTL_POSTKEYSEQUENCE, &keySequence))
			throw gcnew OperationCanceledException;
	}
	finally
	{
		if (keySequence.Sequence != keys)
			delete keySequence.Sequence;
	}
}

// don't throw on a wrong key, it is used for validation
int Far::NameToKey(String^ key)
{
	if (!key)
		throw gcnew ArgumentNullException("key");

	PIN_NE(pin, key);
	return Info.FSF->FarNameToKey(pin);
}

String^ Far::KeyToName(int key)
{
	wchar_t name[33];
	if (!Info.FSF->FarKeyToName(key, name, countof(name) - 1))
		return nullptr;
	return gcnew String(name);
}

void Far::PostKeys(String^ keys)
{
	PostKeys(keys, true);
}

void Far::PostKeys(String^ keys, bool disableOutput)
{
	if (keys == nullptr)
		throw gcnew ArgumentNullException("keys");

	keys = keys->Trim();
	PostKeySequence(CreateKeySequence(keys), disableOutput);
}

void Far::PostText(String^ text)
{
	PostText(text, true);
}

void Far::PostText(String^ text, bool disableOutput)
{
	if (text == nullptr)
		throw gcnew ArgumentNullException("text");

	StringBuilder keys;
	text = text->Replace(CV::CRLF, CV::LF)->Replace('\r', '\n');
	for each(Char c in text)
	{
		switch(c)
		{
		case ' ':
			keys.Append("Space ");
			break;
		case '\n':
			keys.Append("Enter ");
			break;
		case '\t':
			keys.Append("Tab ");
			break;
		default:
			keys.Append(c);
			keys.Append(" ");
			break;
		}
	}
	PostKeys(keys.ToString(), disableOutput);
}

int Far::SaveScreen(int x1, int y1, int x2, int y2)
{
	return (int)(INT_PTR)Info.SaveScreen(x1, y1, x2, y2);
}

void Far::RestoreScreen(int screen)
{
	Info.RestoreScreen((HANDLE)(INT_PTR)screen);
}

IKeyMacroHost^ Far::KeyMacro::get()
{
	return %KeyMacroHost::_instance;
}

ILine^ Far::Line::get()
{
	switch (WindowType)
	{
	case FarNet::WindowType::Editor:
		{
			IEditor^ editor = Editor;
			return editor->CurrentLine;
		}
	case FarNet::WindowType::Panels:
		{
			return CommandLine;
		}
	case FarNet::WindowType::Dialog:
		{
			IDialog^ dialog = Dialog;
			if (dialog) //?? need?
			{
				IControl^ control = dialog->Focused;
				IEdit^ edit = dynamic_cast<IEdit^>(control);
				if (edit)
					return edit->Line;
				IComboBox^ combo = dynamic_cast<IComboBox^>(control);
				if (combo)
					return combo->Line;
			}
			break;
		}
	}
	return nullptr;
}

IEditor^ Far::Editor::get()
{
	return Editor0::GetCurrentEditor();
}

IViewer^ Far::Viewer::get()
{
	return Viewer0::GetCurrentViewer();
}

IPanel^ Far::Panel::get()
{
	return Panel0::GetPanel(true);
}

IPanel^ Far::Panel2::get()
{
	return Panel0::GetPanel(false);
}

IInputBox^ Far::CreateInputBox()
{
	return gcnew InputBox;
}

/*
It is called frequently to get information about menu and disk commands.

STOP:
Check the instance, FarNet may be "unloaded", return empty information,
but return flags, at least preloadable flag is absolutely important as cached.

// http://forum.farmanager.com/viewtopic.php?f=7&t=3890
// (?? it would be nice to have ACTL_POSTCALLBACK)
*/
void Far::AsGetPluginInfo(PluginInfo* pi)
{
	pi->StructSize = sizeof(PluginInfo);

	pi->Flags = PF_DIALOG | PF_EDITOR | PF_VIEWER | PF_FULLCMDLINE | PF_PRELOAD;
	if (!_instance)
		return;

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
				_pConfig[i + 1].Set(Res::MenuPrefix + _toolConfig[i]->Alias(ToolOptions::Config));
		}

		pi->PluginConfigStringsNumber = _toolConfig.Count + 1;
		pi->PluginConfigStrings = (const wchar_t**)_pConfig;
	}

	// disk
	{
		if (!_pDisk)
		{
			_pDisk = new CStr[_toolDisk.Count + 1];
			_pDisk[0].Set(Res::MenuPrefix);

			for(int i = _toolDisk.Count; --i >= 0;)
				_pDisk[i + 1].Set(Res::MenuPrefix + _toolDisk[i]->Alias(ToolOptions::Disk));
		}

		pi->DiskMenuStringsNumber = _toolDisk.Count + 1;
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
					_pEditor[i + 1].Set(Res::MenuPrefix + _toolEditor[i]->Alias(ToolOptions::Editor));
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
					_pPanels[i + 1].Set(Res::MenuPrefix + _toolPanels[i]->Alias(ToolOptions::Panels));
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
					_pViewer[i + 1].Set(Res::MenuPrefix + _toolViewer[i]->Alias(ToolOptions::Viewer));
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
					_pDialog[i + 1].Set(Res::MenuPrefix + _toolDialog[i]->Alias(ToolOptions::Dialog));
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
			for each(CommandPluginInfo^ it in _registeredCommand)
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

void Far::ProcessPrefixes(INT_PTR item)
{
	wchar_t* command = (wchar_t*)item;
	Run(gcnew String(command));
}

void Far::GetUserScreen()
{
	Info.Control(INVALID_HANDLE_VALUE, FCTL_GETUSERSCREEN, 0, 0);
}

void Far::SetUserScreen()
{
	Info.Control(INVALID_HANDLE_VALUE, FCTL_SETUSERSCREEN, 0, 0);
}

//! Hack, not API.
// Avoid exceptions, return what we can get.
ICollection<String^>^ Far::GetDialogHistory(String^ name)
{
	List<String^>^ r = gcnew List<String^>;

	String^ keyName = RootFar + "\\SavedDialogHistory\\" + name;
	RegistryKey^ key = nullptr;
	try
	{
		key = Registry::CurrentUser->OpenSubKey(keyName);
		if (key)
		{
			for each(String^ name1 in key->GetValueNames())
			{
				if (String::Compare(name1, "Flags", StringComparison::OrdinalIgnoreCase) != 0)
					r->Add(key->GetValue(name1)->ToString());
			}
		}
	}
	finally
	{
		if (key)
			key->Close();
	}

	return r;
}

ICollection<String^>^ Far::GetHistory(String^ name)
{
	return GetHistory(name, nullptr);
}

//! Hack, not API.
// Avoid exceptions, return what we can get.
ICollection<String^>^ Far::GetHistory(String^ name, String^ filter)
{
	List<String^>^ r = gcnew List<String^>;

	String^ keyName = RootFar + "\\" + name;
	RegistryKey^ key = nullptr;
	try
	{
		key = Registry::CurrentUser->OpenSubKey(keyName);
		if (key)
		{
			array<String^>^ lines = reinterpret_cast<array<String^>^>(key->GetValue(L"Lines", nullptr));
			if (lines && lines->Length)
			{
				// capacity
				r->Capacity = lines->Length;

				String^ types = nullptr;
				if (SS(filter))
				{
					Object^ o = key->GetValue(L"Types", nullptr);
					if (o)
						types = o->ToString();
				}

				for(int i = lines->Length; --i >= 0;)
				{
					// filter
					if (types && i < types->Length)
					{
						if (filter->IndexOf(types[i]) < 0)
							continue;
					}

					// add
					r->Add(lines[i]);
				}
			}
		}
	}
	finally
	{
		if (key)
			key->Close();
	}

	return r;
}

void Far::ShowError(String^ title, Exception^ error)
{
	if (!error)
		throw gcnew ArgumentNullException("error");

	// log
	String^ info = Log::TraceException(error);

	int res = Msg(
		error->Message,
		String::IsNullOrEmpty(title) ? error->GetType()->FullName : title,
		MsgOptions::LeftAligned | MsgOptions::Warning,
		gcnew array<String^>{"Ok", "View Info", "Copy Info"});

	if (res != 1 && res != 2)
		return;

	if (!info)
		info = Log::FormatException(error);

	info += "\r\n" + error->StackTrace;

	if (res == 1)
	{
		Far::Instance->AnyViewer->ViewText(
			info,
			error->GetType()->FullName,
			OpenMode::Modal);
	}
	else
	{
		CopyToClipboard(info);
	}
}

IDialog^ Far::CreateDialog(int left, int top, int right, int bottom)
{
	return gcnew FarDialog(left, top, right, bottom);
}

int Far::GetPaletteColor(PaletteColor paletteColor)
{
	INT_PTR index = (INT_PTR)paletteColor;
	if (index < 0 || index >= COL_LASTPALETTECOLOR)
		throw gcnew ArgumentOutOfRangeException("paletteColor");
	return (int)Info.AdvControl(Info.ModuleNumber, ACTL_GETCOLOR, (void*)index);
}

void Far::WritePalette(int left, int top, PaletteColor paletteColor, String^ text)
{
	PIN_NE(pin, text);
	Info.Text(left, top, GetPaletteColor(paletteColor), pin);
}

void Far::WriteText(int left, int top, ConsoleColor foregroundColor, ConsoleColor backgroundColor, String^ text)
{
	PIN_NE(pin, text);
	Info.Text(left, top, int(foregroundColor)|(int(backgroundColor)<<4), pin);
}

void Far::ShowHelp(String^ path, String^ topic, HelpOptions options)
{
	PIN_NE(pinPath, path);
	PIN_NS(pinTopic, topic);

	Info.ShowHelp(pinPath, pinTopic, (int)options);
}

//! Console::Write writes some Unicode chars as '?'.
void Far::Write(String^ text)
{
	if (ES(text))
		return;

	if (!ValueUserScreen::Get())
	{
		ValueUserScreen::Set(true);
		GetUserScreen();
	}

	PIN_NE(pin, text);
	DWORD cch = text->Length;
	WriteConsole(GetStdHandle(STD_OUTPUT_HANDLE), pin, cch, &cch, NULL);

	SetUserScreen();
}

void Far::Write(String^ text, ConsoleColor foregroundColor)
{
	ConsoleColor fc = Console::ForegroundColor;
	Console::ForegroundColor = foregroundColor;
	Write(text);
	Console::ForegroundColor = fc;
}

void Far::Write(String^ text, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
{
	ConsoleColor fc = Console::ForegroundColor;
	ConsoleColor bc = Console::BackgroundColor;
	Console::ForegroundColor = foregroundColor;
	Console::BackgroundColor = backgroundColor;
	Write(text);
	Console::ForegroundColor = fc;
	Console::BackgroundColor = bc;
}

IPluginPanel^ Far::CreatePluginPanel()
{
	return gcnew FarNet::Panel2;
}

IPluginPanel^ Far::GetPluginPanel(Guid id)
{
	return Panel0::GetPluginPanel(id);
}

IPluginPanel^ Far::GetPluginPanel(Type^ hostType)
{
	return Panel0::GetPluginPanel(hostType);
}

String^ Far::Input(String^ prompt)
{
	return Input(prompt, nullptr, nullptr, String::Empty);
}

String^ Far::Input(String^ prompt, String^ history)
{
	return Input(prompt, history, nullptr, String::Empty);
}

String^ Far::Input(String^ prompt, String^ history, String^ title)
{
	return Input(prompt, history, title, String::Empty);
}

String^ Far::Input(String^ prompt, String^ history, String^ title, String^ text)
{
	InputBox ib;
	ib.Prompt = prompt;
	ib.Title = title;
	ib.Text = text;
	ib.History = history;
	ib.EmptyEnabled = true;
	return ib.Show() ? ib.Text : nullptr;
}

//::Far Window managenent

ref class FarWindowInfo : public IWindowInfo
{
public:
	FarWindowInfo(int index, bool full)
	{
		WindowInfo wi;
		wi.Pos = index;

		if (full)
		{
#pragma push_macro("ACTL_GETWINDOWINFO")
#undef ACTL_GETWINDOWINFO
			wi.Name = wi.TypeName = NULL;
			wi.NameSize = wi.TypeNameSize = 0;
			if (!Info.AdvControl(Info.ModuleNumber, ACTL_GETWINDOWINFO, &wi))
				throw gcnew InvalidOperationException("GetWindowInfo:" + index + " failed.");

			CBox name(wi.NameSize), typeName(wi.TypeNameSize);
			wi.Name = name;
			wi.TypeName = typeName;
			if (!Info.AdvControl(Info.ModuleNumber, ACTL_GETWINDOWINFO, &wi))
				throw gcnew InvalidOperationException("GetWindowInfo:" + index + " failed.");

			_Name = gcnew String(name);
			_TypeName = gcnew String(typeName);
#pragma pop_macro("ACTL_GETWINDOWINFO")
		}
		else
		{
			if (!Info.AdvControl(Info.ModuleNumber, ACTL_GETSHORTWINDOWINFO, &wi))
				throw gcnew InvalidOperationException("GetWindowInfo:" + index + " failed.");
		}

		_Current = wi.Current != 0;
		_Modified = wi.Modified != 0;
		_Type = (WindowType)wi.Type;
	}
	virtual property bool Current { bool get() { return _Current; } }
	virtual property bool Modified { bool get() { return _Modified; } }
	virtual property String^ Name { String^ get() { return _Name; } }
	virtual property String^ TypeName { String^ get() { return _TypeName; } }
	virtual property WindowType Type { WindowType get() { return _Type; } }
private:
	bool _Current;
	bool _Modified;
	String^ _Name;
	String^ _TypeName;
	WindowType _Type;
};

int Far::WindowCount::get()
{
	return (int)Info.AdvControl(Info.ModuleNumber, ACTL_GETWINDOWCOUNT, 0);
}

IWindowInfo^ Far::GetWindowInfo(int index, bool full)
{
	return gcnew FarWindowInfo(index, full);
}

WindowType Far::WindowType::get()
{
	WindowInfo wi;
	wi.Pos = -1;
	return Info.AdvControl(Info.ModuleNumber, ACTL_GETSHORTWINDOWINFO, &wi) ? (FarNet::WindowType)wi.Type : FarNet::WindowType::None;
}

WindowType Far::GetWindowType(int index)
{
	WindowInfo wi;
	wi.Pos = index;
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_GETSHORTWINDOWINFO, &wi))
		throw gcnew InvalidOperationException("GetWindowType:" + index + " failed.");
	return (FarNet::WindowType)wi.Type;
}

void Far::SetCurrentWindow(int index)
{
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_SETCURRENTWINDOW, (void*)(INT_PTR)index))
		throw gcnew InvalidOperationException("SetCurrentWindow:" + index + " failed.");
}

bool Far::Commit()
{
	return Info.AdvControl(Info.ModuleNumber, ACTL_COMMIT, 0) != 0;
}

Char Far::CodeToChar(int code)
{
	// get just the code
	code &= KeyMode::CodeMask;

	// not char
	if (code > 0xFFFF)
		return 0;

	// convert
	return Char(code);
}

Object^ Far::GetFarValue(String^ keyPath, String^ valueName, Object^ defaultValue)
{
	RegistryKey^ key;
	try
	{
		key = Registry::CurrentUser->OpenSubKey(RootFar + "\\" + keyPath);
		return key ? key->GetValue(valueName, defaultValue) : defaultValue;
	}
	finally
	{
		if (key)
			key->Close();
	}
}

Object^ Far::GetPluginValue(String^ pluginName, String^ valueName, Object^ defaultValue)
{
	RegistryKey^ key;
	try
	{
		key = Registry::CurrentUser->OpenSubKey(RootKey + "\\" + pluginName);
		return key ? key->GetValue(valueName, defaultValue) : defaultValue;
	}
	finally
	{
		if (key)
			key->Close();
	}
}

void Far::SetPluginValue(String^ pluginName, String^ valueName, Object^ newValue)
{
	RegistryKey^ key;
	try
	{
		key = Registry::CurrentUser->CreateSubKey(RootKey + "\\" + pluginName);
		key->SetValue(valueName, newValue);
	}
	finally
	{
		if (key)
			key->Close();
	}
}

//::Far callbacks

bool Far::AsConfigure(int itemIndex)
{
	if (itemIndex == 0)
	{
		OpenConfig();
		return true;
	}

	ToolPluginInfo^ tool = _toolConfig[itemIndex - 1];
	ToolEventArgs e(ToolOptions::Config);
	tool->Handler(this, %e);
	return e.Ignore ? false : true;
}

HANDLE Far::AsOpenFilePlugin(wchar_t* name, const unsigned char* data, int dataSize, int opMode)
{
	if (_registeredFiler.Count == 0)
		return INVALID_HANDLE_VALUE;

	Panel0::BeginOpenMode();
	ValueUserScreen userscreen;

	try
	{
		FilerEventArgs^ e;
		for each(FilerPluginInfo^ it in _registeredFiler)
		{
			// create?
			if (!name && !it->Creates)
				continue;

			// mask?
			if (SS(it->Mask) && !CompareNameEx(it->Mask, name, true))
				continue;

			// arguments
			if (!e)
				e = gcnew FilerEventArgs(gcnew String(name), gcnew UnmanagedMemoryStream((unsigned char*)data, dataSize, dataSize, FileAccess::Read), (OperationModes)opMode);
			else
				e->Data->Seek(0, SeekOrigin::Begin);

			// invoke
			it->Handler(this, e);

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

HANDLE Far::AsOpenPlugin(int from, INT_PTR item)
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
				if (item == 0)
				{
					OpenMenu(ToolOptions::Disk);
					break;
				}

				LOG_AUTO(3, "OPEN_DISKMENU");

				ToolPluginInfo^ tool = _toolDisk[(int)item - 1];
				ToolEventArgs e(ToolOptions::Disk);
				tool->Handler(this, %e);
			}
			break;
		case OPEN_PLUGINSMENU:
			{
				if (item == 0)
				{
					OpenMenu(ToolOptions::Panels);
					break;
				}

				LOG_AUTO(3, "OPEN_PLUGINSMENU");

				ToolPluginInfo^ tool = _toolPanels[(int)item - 1];
				ToolEventArgs e(ToolOptions::Panels);
				tool->Handler(this, %e);
			}
			break;
		case OPEN_EDITOR:
			{
				if (item == 0)
				{
					OpenMenu(ToolOptions::Editor);
					break;
				}

				LOG_AUTO(3, "OPEN_EDITOR");

				ToolPluginInfo^ tool = _toolEditor[(int)item - 1];
				ToolEventArgs e(ToolOptions::Editor);
				tool->Handler(this, %e);
			}
			break;
		case OPEN_VIEWER:
			{
				if (item == 0)
				{
					OpenMenu(ToolOptions::Viewer);
					break;
				}

				LOG_AUTO(3, "OPEN_VIEWER");

				ToolPluginInfo^ tool = _toolViewer[(int)item - 1];
				ToolEventArgs e(ToolOptions::Viewer);
				tool->Handler(this, %e);
			}
			break;
		//! STOP: dialog case is different
		case OPEN_DIALOG:
			{
				const OpenDlgPluginData* dd = (const OpenDlgPluginData*)item;
				int index = dd->ItemNumber;

				if (index == 0)
				{
					OpenMenu(ToolOptions::Dialog);
					break;
				}

				LOG_AUTO(3, "OPEN_DIALOG");

				ToolPluginInfo^ tool = _toolDialog[index - 1];
				ToolEventArgs e(ToolOptions::Dialog);
				FarDialog::_hDlgTop = dd->hDlg;
				tool->Handler(this, %e);
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

void Far::ShowPanelMenu(bool showPushCommand)
{
	String^ sPushShelveThePanel = "Push/Shelve the panel";
	String^ sSwitchFullScreen = "Switch full screen";
	String^ sClose = "Close the panel";
	
	Menu menu;
	menu.AutoAssignHotkeys = true;
	menu.HelpTopic = "MenuPanels";
	menu.ShowAmpersands = true;
	menu.Title = ".NET panel tools";
	menu.BreakKeys->Add(VKeyCode::Delete);

	FarItem^ mi;
	for(;; menu.Items->Clear())
	{
		// Push/Shelve
		if (showPushCommand)
		{
			IPanel^ panel = Panel;
			if (panel->IsPlugin)
			{
				FarNet::Panel2^ plugin = dynamic_cast<FarNet::Panel2^>(Panel);
				if (plugin)
				{
					mi = menu.Add(sPushShelveThePanel);
					mi->Data = plugin;

					mi = menu.Add(sSwitchFullScreen);
					mi->Data = plugin;
				}
				else
				{
					showPushCommand = false;
				}

				mi = menu.Add(sClose);
				mi->Data = panel;
			}
			else if (panel->Type == PanelType::File)
			{
				FarItem^ mi = menu.Add(sPushShelveThePanel);
				mi->Data = panel;
			}
		}

		// Pop/Unshelve
		if (ShelveInfo::_stack.Count)
		{
			menu.Add("Pop/Unshelve")->IsSeparator = true;

			for each(ShelveInfo^ si in ShelveInfo::_stack)
			{
				FarItem^ mi = menu.Add(si->Title);
				mi->Data = si;
			}
		}

		// go
		if (!menu.Show())
			return;

		FarItem^ item = menu.Items[menu.Selected];
		Object^ data = item->Data;

		// [Delete]:
		if (menu.BreakKey == VKeyCode::Delete)
		{
			// case: remove shelved file panel;
			// do not remove plugin panels because of their shutdown bypassed
			ShelveInfoPanel^ shelve = dynamic_cast<ShelveInfoPanel^>(data);
			if (shelve)
				ShelveInfo::_stack.Remove(shelve);

			continue;
		}

		// Push/Shelve
		if ((Object^)item->Text == (Object^)sPushShelveThePanel)
		{
			((Panel1^)data)->Push();
			return;
		}

		// Full screen:
		if ((Object^)item->Text == (Object^)sSwitchFullScreen)
		{
			FarNet::Panel2^ pp = (FarNet::Panel2^)data;
			pp->SwitchFullScreen();
			return;
		}

		// Close panel:
		if ((Object^)item->Text == (Object^)sClose)
		{
			((Panel1^)data)->Close();
			return;
		}

		// Pop/Unshelve
		ShelveInfo^ shelve = (ShelveInfo^)data;
		shelve->Unshelve();

		return;
	}
}

void Far::AssertHotkeys()
{
	if (!_hotkeys)
	{
		if (ES(_hotkey))
			throw gcnew OperationCanceledException(Res::ErrorNoHotKey);

		array<int>^ keys = gcnew array<int>(2);
		keys[1] = NameToKey(_hotkey);
		keys[0] = NameToKey("F11");
		_hotkeys = keys;
	}
}

void Far::PostStep(EventHandler^ handler)
{
	// ensure keys
	AssertHotkeys();

	// post handler and keys
	_handler = handler;
	PostKeySequence(_hotkeys);
}

void Far::PostStepAfterKeys(String^ keys, EventHandler^ handler)
{
	// ensure keys
	AssertHotkeys();

	// post the handler, keys and hotkeys
	_handler = handler;
	PostKeys(keys);
	PostKeySequence(_hotkeys);
}

void Far::PostStepAfterStep(EventHandler^ handler1, EventHandler^ handler2)
{
	// ensure keys
	AssertHotkeys();

	// post the second handler, keys and invoke the first handler
	_handler = handler2;
	PostKeySequence(_hotkeys);
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

void Far::OpenMenu(ToolOptions from)
{
	// process and drop a posted step handler
	if (_handler)
	{
		EventHandler^ handler = _handler;
		_handler = nullptr;
		handler(nullptr, nullptr);
		return;
	}

	// show panels menu
	if (from == ToolOptions::Panels)
		ShowPanelMenu(true);
}

void Far::OpenConfig()
{
	Menu menu;
	menu.AutoAssignHotkeys = true;
	menu.HelpTopic = "MenuConfig";
	menu.Title = "FarNet plugins";

	menu.Add(Res::CommandPlugins + " : " + (_registeredCommand.Count));
	menu.Add(Res::EditorPlugins + "  : " + (_registeredEditor.Count));
	menu.Add(Res::FilerPlugins + "   : " + (_registeredFiler.Count));
	menu.Add(String::Empty)->IsSeparator = true;
	menu.Add(Res::PanelsTools + "    : " + (_toolPanels.Count - 1));
	menu.Add(Res::EditorTools + "    : " + (_toolEditor.Count - 1));
	menu.Add(Res::ViewerTools + "    : " + (_toolViewer.Count - 1));
	menu.Add(Res::DialogTools + "    : " + (_toolDialog.Count - 1));
	menu.Add(Res::ConfigTools + "    : " + (_toolConfig.Count - 1));
	menu.Add(Res::DiskTools + "      : " + (_toolDisk.Count - 1));

	while(menu.Show())
	{
		switch(menu.Selected)
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
			if (_toolPanels.Count > 1)
				OnConfigTool(Res::PanelsTools, ToolOptions::Panels, %_toolPanels);
			break;
		case 5:
			if (_toolEditor.Count > 1)
				OnConfigTool(Res::EditorTools, ToolOptions::Editor, %_toolEditor);
			break;
		case 6:
			if (_toolViewer.Count > 1)
				OnConfigTool(Res::ViewerTools, ToolOptions::Viewer, %_toolViewer);
			break;
		case 7:
			if (_toolDialog.Count > 1)
				OnConfigTool(Res::DialogTools, ToolOptions::Dialog, %_toolDialog);
			break;
		case 8:
			if (_toolConfig.Count > 1)
				OnConfigTool(Res::ConfigTools, ToolOptions::Config, %_toolConfig);
			break;
		case 9:
			if (_toolDisk.Count > 1)
				OnConfigTool(Res::DiskTools, ToolOptions::Disk, %_toolDisk);
			break;
		}
	}
}

void Far::OnConfigTool(String^ title, ToolOptions option, List<ToolPluginInfo^>^ list)
{
	Menu menu;
	menu.Title = title;
	menu.HelpTopic = option == ToolOptions::Disk ? "ConfigDisk" : "ConfigTool";

	ToolPluginInfo^ selected;
	List<ToolPluginInfo^> sorted(list);
	for(;;)
	{
		menu.Items->Clear();
		sorted.Sort(gcnew ToolPluginAliasComparer(option));
		for each(ToolPluginInfo^ it in sorted)
		{
			if (ES(it->Name))
				continue;
			if (it == selected)
				menu.Selected = menu.Items->Count;
			FarItem^ mi = menu.Add(Res::MenuPrefix + it->Alias(option) + " : " + it->Key);
			mi->Data = it;
		}

		// case: disk
		if (option == ToolOptions::Disk)
		{
			while(menu.Show()) {}
			return;
		}

		// show others
		if (!menu.Show())
			return;

		FarItem^ mi = menu.Items[menu.Selected];
		selected = (ToolPluginInfo^)mi->Data;

		InputBox ib;
		ib.EmptyEnabled = true;
		ib.HelpTopic = _helpTopic + "ConfigTool";
		ib.Prompt = "New string (ampersand ~ hotkey)";
		ib.Text = selected->Alias(option);
		ib.Title = "Original: " + selected->Name;
		if (!ib.Show())
			continue;

		// restore the name on empty alias
		String^ alias = ib.Text->TrimEnd();
		if (alias->Length == 0)
			alias = selected->Name;

		// reset the alias
		Free(option);
		selected->Alias(option, alias);
	}
}

void Far::OnConfigCommand()
{
	Menu menu;
	menu.AutoAssignHotkeys = true;
	menu.HelpTopic = "ConfigCommand";
	menu.Title = Res::CommandPlugins;

	for each(CommandPluginInfo^ it in _registeredCommand)
	{
		FarItem^ mi = menu.Add(it->Prefix->PadRight(4) + " " + it->Key);
		mi->Data = it;
	}

	while(menu.Show())
	{
		FarItem^ mi = menu.Items[menu.Selected];
		CommandPluginInfo^ it = (CommandPluginInfo^)mi->Data;

		InputBox ib;
		ib.EmptyEnabled = true;
		ib.HelpTopic = _helpTopic + "ConfigCommand";
		ib.Prompt = "New prefix for " + it->Name;
		ib.Text = it->Prefix;
		ib.Title = "Original prefix: " + it->DefaultPrefix;

		String^ alias = nullptr;
		while(ib.Show())
		{
			alias = ib.Text->Trim();
			if (alias->IndexOf(" ") >= 0 || alias->IndexOf(":") >= 0)
			{
				Msg("Prefix must not contain ' ' or ':'.");
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
		it->Prefix = alias;
		mi->Text = alias->PadRight(4) + " " + it->Key;
		CommandPlugin^ command = dynamic_cast<CommandPlugin^>(it->Plugin);
		if (command)
			command->Prefix = alias;
	}
}

void Far::OnConfigEditor()
{
	Menu menu;
	menu.AutoAssignHotkeys = true;
	menu.HelpTopic = "ConfigEditor";
	menu.Title = Res::EditorPlugins;

	for each(EditorPluginInfo^ it in _registeredEditor)
	{
		FarItem^ mi = menu.Add(it->Key);
		mi->Data = it;
	}

	while(menu.Show())
	{
		FarItem^ mi = menu.Items[menu.Selected];
		EditorPluginInfo^ it = (EditorPluginInfo^)mi->Data;

		InputBox ib;
		ib.EmptyEnabled = true;
		ib.HelpTopic = _helpTopic + "ConfigEditor";
		ib.History = "Masks";
		ib.Prompt = "New mask for " + it->Name;
		ib.Text = it->Mask;
		ib.Title = "Original mask: " + it->DefaultMask;

		if (!ib.Show())
			return;
		String^ mask = ib.Text->Trim();

		// restore original on empty
		if (mask->Length == 0)
			mask = it->DefaultMask;

		// set
		it->Mask = mask;
		EditorPlugin^ filer = dynamic_cast<EditorPlugin^>(it->Plugin);
		if (filer)
			filer->Mask = mask;
	}
}

void Far::OnConfigFiler()
{
	Menu menu;
	menu.AutoAssignHotkeys = true;
	menu.HelpTopic = "ConfigFiler";
	menu.Title = Res::FilerPlugins;

	for each(FilerPluginInfo^ it in _registeredFiler)
	{
		FarItem^ mi = menu.Add(it->Key);
		mi->Data = it;
	}

	while(menu.Show())
	{
		FarItem^ mi = menu.Items[menu.Selected];
		FilerPluginInfo^ it = (FilerPluginInfo^)mi->Data;

		InputBox ib;
		ib.EmptyEnabled = true;
		ib.HelpTopic = _helpTopic + "ConfigFiler";
		ib.History = "Masks";
		ib.Prompt = "New mask for " + it->Name;
		ib.Text = it->Mask;
		ib.Title = "Original mask: " + it->DefaultMask;

		if (!ib.Show())
			return;
		String^ mask = ib.Text->Trim();

		// restore original on empty
		if (mask->Length == 0)
			mask = it->DefaultMask;

		// set
		it->Mask = mask;
		FilerPlugin^ filer = dynamic_cast<FilerPlugin^>(it->Plugin);
		if (filer)
			filer->Mask = mask;
	}
}

bool Far::CompareName(String^ mask, const wchar_t* name, bool skipPath)
{
	for each(String^ s in mask->Split(gcnew array<Char>{',', ';'}, StringSplitOptions::RemoveEmptyEntries))
	{
		PIN_NE(pin, s);
		if (Info.CmpName(pin, name, skipPath))
			return true;
	}
	return false;
}

bool Far::CompareNameEx(String^ mask, const wchar_t* name, bool skipPath)
{
	int i = mask->IndexOf('|');
	if (i < 0)
		return CompareName(mask, name, skipPath);
	return  CompareName(mask->Substring(0, i), name, skipPath) && !CompareName(mask->Substring(i + 1), name, skipPath);
}

void Far::OnEditorOpened(FarNet::Editor^ editor)
{
	if (_registeredEditor.Count == 0)
		return;

	AutoEditorInfo ei;

	for each(EditorPluginInfo^ it in _registeredEditor)
	{
		// mask?
		CBox fileName(Info.EditorControl(ECTL_GETFILENAME, 0));
		Info.EditorControl(ECTL_GETFILENAME, fileName);
		if (SS(it->Mask) && !CompareNameEx(it->Mask, fileName, true))
			continue;

		//! tradeoff: catch all to call other plugins, too
		try
		{
			it->Handler(editor, nullptr);
		}
		catch(Exception^ e)
		{
			//! show plugin info, too
			ShowError(it->Key, e);
		}
	}
}

void Far::Redraw()
{
	Info.AdvControl(Info.ModuleNumber, ACTL_REDRAWALL, 0);
}

String^ Far::TempName(String^ prefix)
{
	// lame, but should work
	wchar_t dest[MAX_PATH + 20];

	PIN_NE(pin, prefix);
	if (!Info.FSF->MkTemp(dest, countof(dest), pin))
		throw gcnew OperationCanceledException(__FUNCTION__);

	return gcnew String(dest);
}

String^ Far::TempFolder(String^ prefix)
{
	String^ r = TempName(prefix);
	Directory::CreateDirectory(r);
	return r;
}

IDialog^ Far::Dialog::get()
{
	return FarDialog::GetDialog();
}

ConsoleColor Far::GetPaletteBackground(PaletteColor paletteColor)
{
	int color = GetPaletteColor(paletteColor);
	return ConsoleColor(color >> 4);
}

ConsoleColor Far::GetPaletteForeground(PaletteColor paletteColor)
{
	int color = GetPaletteColor(paletteColor);
	return ConsoleColor(color & 0xF);
}

IZoo^ Far::Zoo::get()
{
	return gcnew FarNet::Zoo;
}

void Far::AsProcessSynchroEvent(int type, void* /*param*/)
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

void Far::PostJob(EventHandler^ handler)
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

}
