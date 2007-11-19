/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#include "StdAfx.h"
#include "FarImpl.h"
#include "CommandLine.h"
#include "Dialog.h"
#include "Editor.h"
#include "EditorManager.h"
#include "InputBox.h"
#include "Menu.h"
#include "Message.h"
#include "Panel.h"
#include "Viewer.h"
using namespace Microsoft::Win32;
using namespace System::Reflection;

namespace FarManagerImpl
{;
//
//::Far::
//

Far::Far()
: _editorManager(gcnew EditorManager)
{
}

void Far::StartFar()
{
	if (_instance) throw gcnew InvalidOperationException("Already started.");
	_instance = gcnew Far();
}

void Far::Start()
{
	// this has to be done after loading plugins
	RegisterPluginsDiskItem("FAR.NET", gcnew EventHandler<PluginMenuEventArgs^>(&Far::OnFarNetDisk));
	RegisterPluginsMenuItem("FAR.NET", gcnew EventHandler<PluginMenuEventArgs^>(&Far::OnFarNetMenu));
}

void Far::Stop()
{
	delete[] _configStrings;
	delete[] _diskStrings;
	delete[] _menuStrings;
	delete _prefixes;
	_instance = nullptr;
}

String^ Far::PluginFolderPath::get()
{
	String^ pluginPath = OemToStr(Info.ModuleName);
	return (gcnew FileInfo(pluginPath))->DirectoryName;
}

String^ Far::RootFar::get()
{
	String^ key = RootKey;
	return key->Substring(0, key->LastIndexOf('\\'));
}

String^ Far::RootKey::get()
{
	return OemToStr(Info.RootKey);
}

static void RemoveItem(List<PluginMenuItem^>^ list, String^ name, EventHandler<PluginMenuEventArgs^>^ handler)
{
	for(int i = list->Count; --i >= 0;)
	{
		PluginMenuItem^ p = list[i];
		if (name == p->Name && (!handler || handler == p->Handler))
			list->RemoveAt(i);
	}
}

void Far::RegisterPluginsConfigItem(String^ name, EventHandler<PluginMenuEventArgs^>^ handler)
{
	delete[] _configStrings;
	_configStrings = 0;
	_registeredConfigItems.Add(gcnew PluginMenuItem(name, handler));
}

void Far::UnregisterPluginsConfigItem(String^ name, EventHandler<PluginMenuEventArgs^>^ handler)
{
	delete[] _configStrings;
	_configStrings = 0;
	RemoveItem(%_registeredConfigItems, name, handler);
}

void Far::RegisterPluginsDiskItem(String^ name, EventHandler<PluginMenuEventArgs^>^ handler)
{
	delete[] _diskStrings;
	_diskStrings = 0;
	_registeredDiskItems.Add(gcnew PluginMenuItem(name, handler));
}

void Far::UnregisterPluginsDiskItem(String^ name, EventHandler<PluginMenuEventArgs^>^ handler)
{
	delete[] _diskStrings;
	_diskStrings = 0;
	RemoveItem(%_registeredDiskItems, name, handler);
}

void Far::RegisterPluginsMenuItem(String^ name, EventHandler<PluginMenuEventArgs^>^ handler)
{
	delete[] _menuStrings;
	_menuStrings = 0;
	_registeredMenuItems.Add(gcnew PluginMenuItem(name, handler));
}

void Far::UnregisterPluginsMenuItem(String^ name, EventHandler<PluginMenuEventArgs^>^ handler)
{
	delete[] _menuStrings;
	_menuStrings = 0;
	RemoveItem(%_registeredMenuItems, name, handler);
}

void Far::RegisterPrefix(String^ prefix, EventHandler<ExecutingEventArgs^>^ handler)
{
	delete _prefixes;
	_prefixes = 0;
	_registeredPrefixes[prefix] = handler;
}

void Far::UnregisterPrefix(String^ prefix)
{
	delete _prefixes;
	_prefixes = 0;
	_registeredPrefixes.Remove(prefix);
}

void Far::RegisterOpenFile(EventHandler<OpenFileEventArgs^>^ handler)
{
	_registeredOpenFile.Add(handler);
}

void Far::UnregisterOpenFile(EventHandler<OpenFileEventArgs^>^ handler)
{
	_registeredOpenFile.Remove(handler);
}

bool Far::Msg(String^ body)
{
	return Message::Show(body, nullptr, MessageOptions::Ok, nullptr) >= 0;
}

bool Far::Msg(String^ body, String^ header)
{
	return Message::Show(body, header, MessageOptions::Ok, nullptr) >= 0;
}

int Far::Msg(String^ body, String^ header, MessageOptions options)
{
	return Message::Show(body, header, options, nullptr);
}

int Far::Msg(String^ body, String^ header, MessageOptions options, array<String^>^ buttons)
{
	return Message::Show(body, header, options, buttons);
}

IMessage^ Far::CreateMessage()
{
	return gcnew Message();
}

void Far::Run(String^ command)
{
	int colon = command->IndexOf(':');
	if (colon < 0)
		return;

	for each(KeyValuePair<String^, EventHandler<ExecutingEventArgs^>^>^ i in _registeredPrefixes)
	{
		String^ pref = i->Key;
		if (colon != pref->Length || !command->StartsWith(pref))
			continue;
		EventHandler<ExecutingEventArgs^>^ handler = i->Value;
		ExecutingEventArgs e(command->Substring(colon + 1));
		handler(nullptr, %e);
		break;
	}
}

int Far::HWnd::get()
{
	return Info.AdvControl(Info.ModuleNumber, ACTL_GETFARHWND, nullptr);
}

System::Version^ Far::Version::get()
{
	DWORD vn;
	Info.AdvControl(Info.ModuleNumber, ACTL_GETFARVERSION, &vn);
	return gcnew System::Version((vn&0x0000ff00)>>8, vn&0x000000ff, (int)((long)vn&0xffff0000)>>16);
}

IMenu^ Far::CreateMenu()
{
	return gcnew Menu();
}

IListMenu^ Far::CreateListMenu()
{
	return gcnew ListMenu();
}

FarConfirmations Far::Confirmations::get()
{
	return (FarConfirmations)Info.AdvControl(Info.ModuleNumber, ACTL_GETCONFIRMATIONS, 0);
}

ICollection<IEditor^>^ Far::Editors::get()
{
	return _editorManager->Editors;
}

IAnyEditor^ Far::AnyEditor::get()
{
	return _editorManager->AnyEditor;
}

String^ Far::WordDiv::get()
{
	int length = Info.AdvControl(Info.ModuleNumber, ACTL_GETSYSWORDDIV, 0);
	CStr wd(length);
	Info.AdvControl(Info.ModuleNumber, ACTL_GETSYSWORDDIV, wd);
	return OemToStr(wd);
}

OpenFrom Far::From::get()
{
	return _from;
}

String^ Far::Clipboard::get()
{
	return OemToStr(Info.FSF->PasteFromClipboard());
}

void Far::Clipboard::set(String^ value)
{
	CStr pcvalue(value);
	Info.FSF->CopyToClipboard(pcvalue);
}

IEditor^ Far::CreateEditor()
{
	return _editorManager->CreateEditor();
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

void Far::PostKeySequence(array<int>^ sequence, bool disableOutput)
{
	if (sequence == nullptr)
		throw gcnew ArgumentNullException("sequence");
	if (sequence->Length == 0)
		return;

	KeySequence keySequence;
	keySequence.Count = sequence->Length;
	keySequence.Flags = disableOutput ? KSFLAGS_DISABLEOUTPUT : 0;
	keySequence.Sequence = new DWORD[keySequence.Count];
	DWORD* cur = keySequence.Sequence;
	for each(int i in sequence)
	{
		*cur = i;
		cur++;
	}
	try
	{
		if (!Info.AdvControl(Info.ModuleNumber, ACTL_POSTKEYSEQUENCE, &keySequence))
			throw gcnew OperationCanceledException();
	}
	finally
	{
		delete keySequence.Sequence;
	}
}

// don't throw on a wrong key, it is used for validation
int Far::NameToKey(String^ key)
{
	if (!key) throw gcnew ArgumentNullException("key");
	char buf[33];
	StrToOem(key, buf, sizeof(buf));
	return Info.FSF->FarNameToKey(buf);
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

	StringBuilder^ keys = gcnew StringBuilder();
	text = text->Replace(CV::CRLF, CV::LF)->Replace('\r', '\n');
	for each(Char c in text)
	{
		switch(c)
		{
		case ' ':
			keys->Append("Space ");
			break;
		case '\n':
			keys->Append("Enter ");
			break;
		case '\t':
			keys->Append("Tab ");
			break;
		default:
			keys->Append(c);
			keys->Append(" ");
			break;
		}
	}
	PostKeys(keys->ToString(), disableOutput);
}

int Far::SaveScreen(int x1, int y1, int x2, int y2)
{
	return (int)(INT_PTR)Info.SaveScreen(x1, y1, x2, y2);
}

void Far::RestoreScreen(int screen)
{
	Info.RestoreScreen((HANDLE)(INT_PTR)screen);
}

ILine^ Far::CommandLine::get()
{
	return gcnew FarCommandLine();
}

IEditor^ Far::Editor::get()
{
	return _editorManager->GetCurrentEditor();
}

IPanel^ Far::Panel::get()
{
	for (int i = 1; i < 3; ++i)
	{
		FarPanelPlugin^ p = PanelSet::_panels[i];
		if (p && p->IsActive)
			return p;
	}
	return gcnew FarPanel(true);
}

IPanel^ Far::Panel2::get()
{
	for (int i = 1; i < 3; ++i)
	{
		FarPanelPlugin^ p = PanelSet::_panels[i];
		if (p && !p->IsActive)
			return p;
	}
	return gcnew FarPanel(false);
}

IInputBox^ Far::CreateInputBox()
{
	return gcnew InputBox();
}

//! frequently called
void Far::AsGetPluginInfo(PluginInfo* pi)
{
	pi->StructSize = sizeof(PluginInfo);
	pi->Flags = PF_EDITOR | PF_VIEWER | PF_FULLCMDLINE | PF_PRELOAD;

	if (_registeredConfigItems.Count)
	{
		pi->PluginConfigStringsNumber = _registeredConfigItems.Count;
		if (_configStrings == 0)
		{
			_configStrings = new CStr[_registeredConfigItems.Count];
			for(int i = 0; i < _registeredConfigItems.Count; ++i)
			{
				PluginMenuItem^ item = _registeredConfigItems[i];
				_configStrings[i].Set(item->Name);
			}
		}
		pi->PluginConfigStrings = (char**)_configStrings;
	}
	
	if (_registeredDiskItems.Count)
	{
		pi->DiskMenuStringsNumber = _registeredDiskItems.Count;
		if (_diskStrings == 0)
		{
			_diskStrings = new CStr[_registeredDiskItems.Count];
			for(int i = 0; i < _registeredDiskItems.Count; ++i)
			{
				PluginMenuItem^ item = _registeredDiskItems[i];
				_diskStrings[i].Set(item->Name);
			}
		}
		pi->DiskMenuStrings = (char**)_diskStrings;
	}

	if (_registeredMenuItems.Count)
	{
		pi->PluginMenuStringsNumber = _registeredMenuItems.Count;
		if (_menuStrings == 0)
		{
			_menuStrings = new CStr[_registeredMenuItems.Count];
			for(int i = 0; i < _registeredMenuItems.Count; ++i)
			{
				PluginMenuItem^ item = _registeredMenuItems[i];
				_menuStrings[i].Set(item->Name);
			}
		}
		pi->PluginMenuStrings = (char**)_menuStrings;
	}

	if (_registeredPrefixes.Count)
	{
		if (_prefixes == 0)
		{
			String^ PrefString = String::Empty;
			for each(KeyValuePair<String^, EventHandler<ExecutingEventArgs^>^>^ i in _registeredPrefixes)
			{
				if (PrefString->Length > 0)
					PrefString = String::Concat(PrefString, ":");
				PrefString = String::Concat(PrefString, i->Key);
			}
			_prefixes = new CStr(PrefString);
		}
		pi->CommandPrefix = *_prefixes;
	}
}

void Far::ProcessPrefixes(INT_PTR item)
{
	char* command = (char*)item;
	Run(OemToStr(command));
}

void Far::GetUserScreen()
{
	Info.Control(INVALID_HANDLE_VALUE, FCTL_GETUSERSCREEN, 0);
}

void Far::SetUserScreen()
{
	Info.Control(INVALID_HANDLE_VALUE, FCTL_SETUSERSCREEN, 0);
}

ICollection<String^>^ Far::GetHistory(String^ name)
{
	String^ keyName = RootFar + "\\" + name;
	CStr sKeyName(keyName);

	HKEY hk;
	LONG lResult = ::RegOpenKeyEx(HKEY_CURRENT_USER, sKeyName, 0, KEY_READ, &hk);
	if (lResult != ERROR_SUCCESS)
		throw gcnew OperationCanceledException();

	char* cb = NULL;
	try
	{
		DWORD dwType = 0, dwCount = 0;
		lResult = ::RegQueryValueEx(hk, "Lines", NULL, &dwType, NULL, &dwCount);
		if (lResult != ERROR_SUCCESS || dwType != REG_BINARY)
			throw gcnew OperationCanceledException();

		cb = new char[dwCount];
		lResult = ::RegQueryValueEx(hk, "Lines", NULL, &dwType, (LPBYTE)cb, &dwCount);
		if (lResult != ERROR_SUCCESS)
			throw gcnew OperationCanceledException();

		List<String^>^ r = gcnew List<String^>();
		for(int i = 0; i < (int)dwCount;)
		{
			String^ s = OemToStr(cb + i);
			r->Add(s);
			i += s->Length + 1;
		}
		return r;
	}
	finally
	{
		delete cb;
		::RegCloseKey(hk);
	}
}

void ShowExceptionInfo(Exception^ e)
{
	String^ path = Path::GetTempFileName();
	File::WriteAllText(path, ExceptionInfo(e, false) + "\n" + e->ToString(), System::Text::Encoding::Unicode);

	// view file
	Viewer v;
	v.Title = e->GetType()->FullName;
	v.FileName = path;
	v.DeleteOnlyFileOnClose = true;
	v.DisableHistory = true;
	v.IsModal = true;
	v.Open();
}

void Far::ShowError(String^ title, Exception^ error)
{
	if (1 == Msg(
		error->Message,
		String::IsNullOrEmpty(title) ? error->GetType()->FullName : title,
		MessageOptions::LeftAligned | MessageOptions::Warning,
		gcnew array<String^>{"Ok", "Info"}))
	{
		ShowExceptionInfo(error);
	}
}

IDialog^ Far::CreateDialog(int left, int top, int right, int bottom)
{
	return gcnew FarDialog(left, top, right, bottom);
}

IViewer^ Far::CreateViewer()
{
	return gcnew Viewer();
}

void Far::WriteText(int left, int top, ConsoleColor foregroundColor, ConsoleColor backgroundColor, String^ text)
{
	CStr sText(text);
	Info.Text(left, top, int(foregroundColor)|(int(backgroundColor)<<4), sText);
}

void Far::ShowHelp(String^ path, String^ topic, HelpOptions options)
{
	CStr sPath; if (!String::IsNullOrEmpty(path)) sPath.Set(path);
	CStr sTopic; if (!String::IsNullOrEmpty(topic)) sTopic.Set(topic);
	Info.ShowHelp(sPath, sTopic, (int)options);
}

void Far::Write(String^ text)
{
	GetUserScreen();
	Console::Write(text);
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

IPanelPlugin^ Far::CreatePanelPlugin()
{
	return gcnew FarPanelPlugin();
}

IPanelPlugin^ Far::GetPanelPlugin(Type^ hostType)
{
	// case: any panel
	if (hostType == nullptr)
	{
		for (int i = 1; i < 3; ++i)
		{
			FarPanelPlugin^ p = PanelSet::_panels[i];
			if (p)
				return p;
		}
		return nullptr;
	}

	// panel with defined host type
	for (int i = 1; i < 3; ++i)
	{
		FarPanelPlugin^ p = PanelSet::_panels[i];
		if (p && p->Host)
		{
			Type^ type = p->Host->GetType();
			if (type == hostType || type->IsSubclassOf(hostType))
				return p;
		}
	}

	return nullptr;
}

IFile^ Far::CreatePanelItem()
{
	return gcnew FarFile();
}

IFile^ Far::CreatePanelItem(FileSystemInfo^ info, bool fullName)
{
	FarFile^ r = gcnew FarFile();
	if (info)
	{
		r->Name = fullName ? info->FullName : info->Name;
		r->CreationTime = info->CreationTime;
		r->LastAccessTime = info->LastAccessTime;
		r->LastWriteTime = info->LastWriteTime;
		r->SetAttributes(info->Attributes);
		if (!r->IsDirectory)
			r->Length = ((FileInfo^)info)->Length;
	}
	return r;
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

//::FAR Window managenent

public ref class FarWindowInfo : public IWindowInfo
{
public:
	FarWindowInfo(const WindowInfo& wi, bool full)
		: _Current(wi.Current != 0), _Modified(wi.Modified != 0), _Type((WindowType)wi.Type)
	{
		if (full)
		{
			_Name = OemToStr(wi.Name);
			_TypeName = OemToStr(wi.TypeName);
		}
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
	return Info.AdvControl(Info.ModuleNumber, ACTL_GETWINDOWCOUNT, 0);
}

IWindowInfo^ Far::GetWindowInfo(int index, bool full)
{
	WindowInfo wi;
	wi.Pos = index;
	if (!Info.AdvControl(Info.ModuleNumber, full ? ACTL_GETWINDOWINFO : ACTL_GETSHORTWINDOWINFO, &wi))
		throw gcnew InvalidOperationException("GetWindowInfo:" + index + " failed.");
	return gcnew FarWindowInfo(wi, full);
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
	code &= ~(KeyCode::Alt | KeyCode::Ctrl);
	return code < 0 || code > 255 ? 0 : OemToChar(char(code));
}

void Far::LoadMacros()
{
	ActlKeyMacro command;
	command.Command = MCMD_LOADALL;
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_KEYMACRO, &command))
		throw gcnew OperationCanceledException(__FUNCTION__ " failed.");
}

void Far::SaveMacros()
{
	ActlKeyMacro command;
	command.Command = MCMD_SAVEALL;
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_KEYMACRO, &command))
		throw gcnew OperationCanceledException(__FUNCTION__ " failed.");
}

void Far::PostMacro(String^ macro)
{
	PostMacro(macro, true, false);
}

void Far::PostMacro(String^ macro, bool disableOutput, bool noSendKeysToPlugins)
{
	if (!macro) throw gcnew ArgumentNullException("macro");
	
	CStr sMacro(macro);
	ActlKeyMacro command;
	command.Command = MCMD_POSTMACROSTRING;
	command.Param.PlainText.SequenceText = sMacro;
	command.Param.PlainText.Flags = 0;
	if (disableOutput)
		command.Param.PlainText.Flags |= KSFLAGS_DISABLEOUTPUT;
	if (noSendKeysToPlugins)
		command.Param.PlainText.Flags |= KSFLAGS_NOSENDKEYSTOPLUGINS;
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_KEYMACRO, &command))
		throw gcnew OperationCanceledException(__FUNCTION__ " failed.");
}

Object^ Far::GetPluginValue(String^ pluginName, String^ valueName, Object^ defaultValue)
{
	RegistryKey^ key1 = nullptr;
	RegistryKey^ key2 = nullptr;
	try
	{
		key1 = Registry::CurrentUser->CreateSubKey(RootKey);
		key2 = key1->CreateSubKey(pluginName);
		return key2->GetValue(valueName, defaultValue);
	}
	finally
	{
		if (key2)
			key2->Close();
		if (key1)
			key1->Close();
	}
}

void Far::SetPluginValue(String^ pluginName, String^ valueName, Object^ newValue)
{
	RegistryKey^ key1 = nullptr;
	RegistryKey^ key2 = nullptr;
	try
	{
		key1 = Registry::CurrentUser->CreateSubKey(RootKey);
		key2 = key1->CreateSubKey(pluginName);
		key2->SetValue(valueName, newValue);
	}
	finally
	{
		if (key2)
			key2->Close();
		if (key1)
			key1->Close();
	}
}

//::FAR callbacks

bool Far::AsConfigure(int itemIndex)
{
	PluginMenuItem^ item = (PluginMenuItem^)_registeredConfigItems[itemIndex];
	PluginMenuEventArgs e(OpenFrom::Other);
	item->Handler(item, %e);
	return e.Ignore ? false : true;
}

HANDLE Far::AsOpenFilePlugin(char* name, const unsigned char* data, int dataSize)
{
	if (_registeredOpenFile.Count == 0)
		return INVALID_HANDLE_VALUE;

	try
	{
		_canOpenPanelPlugin = true;

		OpenFileEventArgs e;
		e.Name = OemToStr(name);
		e.Data = gcnew array<Byte>(dataSize);
		for(int i = dataSize; --i >= 0;)
			e.Data[i] = data[i];

		for each(EventHandler<OpenFileEventArgs^>^ handler in _registeredOpenFile)
		{
			handler(this, %e);

			// open a waiting panel
			if (PanelSet::_panels[0])
			{
				HANDLE h = PanelSet::AddPanelPlugin(PanelSet::_panels[0]);
				return h;
			}
		}

		return INVALID_HANDLE_VALUE;
	}
	finally
	{
		// drop a waiting panel and set the global lock
		PanelSet::_panels[0] = nullptr;
		_canOpenPanelPlugin = false;
	}
}

HANDLE Far::AsOpenPlugin(int from, INT_PTR item)
{
	try
	{
		// where from
		_from = (OpenFrom)from;

		// call, plugin may create a panel waiting for opening
		if (from == OPEN_COMMANDLINE)
		{
			_canOpenPanelPlugin = true;
			ProcessPrefixes(item);
		}
		else if (from == OPEN_DISKMENU)
		{
			_canOpenPanelPlugin = true;
			PluginMenuItem^ menuItem = (PluginMenuItem^)_registeredDiskItems[(int)item];
			PluginMenuEventArgs e((OpenFrom)from);
			menuItem->Handler(menuItem, %e);
		}
		else if (from == OPEN_PLUGINSMENU || from == OPEN_EDITOR || from == OPEN_VIEWER)
		{
			_canOpenPanelPlugin = (from == OPEN_PLUGINSMENU);
			PluginMenuItem^ menuItem = (PluginMenuItem^)_registeredMenuItems[(int)item];
			PluginMenuEventArgs e((OpenFrom)from);
			menuItem->Handler(menuItem, %e);
		}

		// open a waiting panel
		if (PanelSet::_panels[0])
		{
			HANDLE h = PanelSet::AddPanelPlugin(PanelSet::_panels[0]);
			return h;
		}

		// don't open a panel
		return INVALID_HANDLE_VALUE;
	}
	finally
	{
		// drop a waiting panel and set the global lock
		PanelSet::_panels[0] = nullptr;
		_from = OpenFrom::Other;
		_canOpenPanelPlugin = false;
	}
}

array<IPanelPlugin^>^ Far::PushedPanels()
{
	array<IPanelPlugin^>^ r = gcnew array<IPanelPlugin^>(PanelSet::_stack.Count);
	for(int i = PanelSet::_stack.Count; --i >= 0;)
		r[i] = PanelSet::_stack[i];
	return r;
}

void Far::OnFarNetDisk(Object^ /*sender*/, PluginMenuEventArgs^ /*e*/)
{
	_instance->ShowPanelMenu(false);
}

void Far::OnFarNetMenu(Object^ /*sender*/, PluginMenuEventArgs^ /*e*/)
{
	_instance->ShowPanelMenu(true);
}

void Far::ShowPanelMenu(bool showPushCommand)
{
	Menu m;
	m.Title = "FAR.NET";
	m.AutoAssignHotkeys = true;
	m.ShowAmpersands = true;

	// "Push" command
	if (showPushCommand)
	{
		FarPanelPlugin^ pp = dynamic_cast<FarPanelPlugin^>(Panel);
		if (pp)
		{
			IMenuItem^ mi = m.Items->Add("Push the panel");
			mi->Data = pp;
		}
		else
		{
			showPushCommand = false;
		}
	}

	// pushed panels
	if (PanelSet::_stack.Count)
	{
		if (showPushCommand)
			m.Items->Add(String::Empty)->IsSeparator = true;
		for(int i = PanelSet::_stack.Count; --i >= 0;)
		{
			FarPanelPlugin^ pp = PanelSet::_stack[i];
			IMenuItem^ mi = m.Items->Add(JoinText(pp->_info.Title, pp->_info.CurrentDirectory));
			mi->Data = pp;
		}
	}
	
	// go
	if (!m.Show())
		return;

	// push
	if (showPushCommand && m.Selected == 0)
	{
		FarPanelPlugin^ pp = (FarPanelPlugin^)m.SelectedData;
		pp->Push();
		return;
	}

	// pop
	FarPanelPlugin^ pp = (FarPanelPlugin^)m.SelectedData;
	pp->Open();
}

}
