/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#include "StdAfx.h"
#include "FarImpl.h"
#include "FarPanel.h"
#include "Dialog.h"
#include "Editor.h"
#include "EditorManager.h"
#include "FarCommandLine.h"
#include "InputBox.h"
#include "Menu.h"
#include "Message.h"
#include "Viewer.h"
using namespace Microsoft::Win32;
using namespace System::Reflection;

namespace FarManagerImpl
{;
static List<IFile^>^ ItemsToFiles(IList<IFile^>^ files, PluginPanelItem* panelItem, int itemsNumber)
{
	List<IFile^>^ r = gcnew List<IFile^>();
	r->Capacity = itemsNumber;
	for(int i = 0; i < itemsNumber; ++i)
		r->Add(files[(int)(INT_PTR)panelItem[i].UserData]);
	return r;
}

//
//::Far::
//

Far::Far()
: _registeredConfigItems(gcnew List<PluginMenuItem^>())
, _registeredDiskItems(gcnew List<PluginMenuItem^>())
, _registeredMenuItems(gcnew List<PluginMenuItem^>())
, _registeredPrefixes(gcnew Dictionary<String^, StringDelegate^>())
, _editorManager(gcnew EditorManager())
, _panels(gcnew array<FarPanelPlugin^>(4))
{
}

void Far::Free()
{
	FreeMenuStrings();
	delete _prefixes;
	_prefixes = 0;
}

IPanelPlugin^ Far::GetPanelPlugin2(FarPanelPlugin^ plugin)
{
	for (int i = 1; i < 4; ++i)
	{
		FarPanelPlugin^ p = _panels[i];
		if (p && p != plugin)
			return p;
	}
	return nullptr;
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

void Far::RegisterPrefix(String^ prefix, StringDelegate^ handler)
{
	_registeredPrefixes->Add(prefix, handler);
}

void Far::RegisterPluginsConfigItem(String^ name, EventHandler<OpenPluginMenuItemEventArgs^>^ handler)
{
	_registeredConfigItems->Add(gcnew PluginMenuItem(name, handler));
}

void Far::RegisterPluginsDiskItem(String^ name, EventHandler<OpenPluginMenuItemEventArgs^>^ handler)
{
	_registeredDiskItems->Add(gcnew PluginMenuItem(name, handler));
}

void Far::RegisterPluginsMenuItem(String^ name, EventHandler<OpenPluginMenuItemEventArgs^>^ handler)
{
	_registeredMenuItems->Add(gcnew PluginMenuItem(name, handler));
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

	for each(KeyValuePair<String^, StringDelegate^>^ i in _registeredPrefixes)
	{
		String^ pref = i->Key;
		if (colon != pref->Length || !command->StartsWith(pref))
			continue;
		StringDelegate^ handler = i->Value;
		handler->Invoke(command->Substring(colon + 1));
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
	for (int i = 1; i < 4; ++i)
	{
		FarPanelPlugin^ p = _panels[i];
		if (p && p->IsActive)
			return p;
	}
	return gcnew FarPanel(true);
}

IPanel^ Far::Panel2::get()
{
	for (int i = 1; i < 4; ++i)
	{
		FarPanelPlugin^ p = _panels[i];
		if (p && !p->IsActive)
			return p;
	}
	return gcnew FarPanel(false);
}

IInputBox^ Far::CreateInputBox()
{
	return gcnew InputBox();
}

void Far::AsGetPluginInfo(PluginInfo* pi)
{
	pi->StructSize = sizeof(PluginInfo);
	pi->Flags = PF_EDITOR | PF_VIEWER | PF_FULLCMDLINE | PF_PRELOAD;

	CreateMenuStringsBlock();
	pi->DiskMenuStrings = (char**)_diskStrings;
	pi->DiskMenuStringsNumber = _registeredDiskItems->Count;
	pi->PluginMenuStrings = (char**)_menuStrings;
	pi->PluginMenuStringsNumber = _registeredMenuItems->Count;
	pi->PluginConfigStrings = (char**)_configStrings;
	pi->PluginConfigStringsNumber = _registeredConfigItems->Count;

	MakePrefixes();
	if (_registeredPrefixes->Count > 0)
		pi->CommandPrefix = _prefixes ? *_prefixes : NULL;
}

void Far::CreateMenuStringsBlock()
{
	FreeMenuStrings();

	_diskStrings = new CStr[_registeredDiskItems->Count];
	for(int i = 0; i < _registeredDiskItems->Count; ++i)
	{
		PluginMenuItem^ item = _registeredDiskItems[i];
		_diskStrings[i].Set(item->Name);
	}

	_menuStrings = new CStr[_registeredMenuItems->Count];
	for(int i = 0; i < _registeredMenuItems->Count; ++i)
	{
		PluginMenuItem^ item = _registeredMenuItems[i];
		_menuStrings[i].Set(item->Name);
	}

	_configStrings = new CStr[_registeredConfigItems->Count];
	for(int i = 0; i < _registeredConfigItems->Count; ++i)
	{
		PluginMenuItem^ item = _registeredConfigItems[i];
		_configStrings[i].Set(item->Name);
	}
}

void Far::FreeMenuStrings()
{
	delete[] _diskStrings;
	_diskStrings = NULL;

	delete[] _menuStrings;
	_menuStrings = NULL;

	delete[] _configStrings;
	_configStrings = NULL;
}

void Far::ProcessPrefixes(INT_PTR item)
{
	char* command = (char*)item;
	Run(OemToStr(command));
}

void Far::MakePrefixes()
{
	if (_registeredPrefixes->Count == 0)
		return;

	String^ PrefString = String::Empty;
	for each(KeyValuePair<String^, StringDelegate^>^ i in _registeredPrefixes)
	{
		if (PrefString->Length > 0)
			PrefString = String::Concat(PrefString, ":");
		PrefString = String::Concat(PrefString, i->Key);
	}
	delete _prefixes;
	_prefixes = new CStr(PrefString);
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

HANDLE Far::AddPanelPlugin(FarPanelPlugin^ plugin)
{
	for(int i = 1; i < 4; ++i)
	{
		if (_panels[i] == nullptr)
		{
			_panels[i] = plugin;
			plugin->Id = i;
			return (HANDLE)(INT_PTR)i;
		}
	}
	throw gcnew InvalidOperationException("Can't register plugin panel.");
}

IPanelPlugin^ Far::CreatePanelPlugin()
{
	return gcnew FarPanelPlugin();
}

//! it call Update/Redraw in some cases
void Far::ReplacePanelPlugin(FarPanelPlugin^ oldPanel, FarPanelPlugin^ newPanel)
{
	// check
	if (!oldPanel)
		throw gcnew ArgumentNullException("oldPanel");
	if (!newPanel)
		throw gcnew ArgumentNullException("newPanel");

	int id1 = oldPanel->Id;
	if (id1 < 1)
		throw gcnew InvalidOperationException("Old panel plugin must be opened.");

	if (newPanel->Id >= 1)
		throw gcnew InvalidOperationException("New panel plugin must not be opened.");

	// save old modes
	oldPanel->Info->StartSortDesc = oldPanel->ReverseSortOrder;
	oldPanel->Info->StartSortMode = oldPanel->SortMode;
	oldPanel->Info->StartViewMode = oldPanel->ViewMode;

	// disconnect old panel
	oldPanel->Id = 0;
	((FarPanelPluginInfo^)oldPanel->Info)->Free();

	// connect new panel
	_panels[id1] = newPanel;
	newPanel->Id = id1;

	// change panel modes
	if (newPanel->Info->StartViewMode != PanelViewMode::Undefined &&
		newPanel->Info->StartViewMode != oldPanel->Info->StartViewMode ||
		newPanel->Info->StartSortMode != PanelSortMode::Default && (
		newPanel->Info->StartSortMode != oldPanel->Info->StartSortMode ||
		newPanel->Info->StartSortDesc != oldPanel->Info->StartSortDesc))
	{
		// detach files to change modes with no files
		List<IFile^> dummy;
		List<IFile^>^ files = newPanel->ReplaceFiles(%dummy);
		newPanel->Update(false);

		// set only new modes
		if (newPanel->Info->StartViewMode != PanelViewMode::Undefined && newPanel->Info->StartViewMode != oldPanel->Info->StartViewMode)
			newPanel->ViewMode = newPanel->Info->StartViewMode;
		if (newPanel->Info->StartSortMode != PanelSortMode::Default)
		{
			if (newPanel->Info->StartSortMode != oldPanel->Info->StartSortMode)
				newPanel->SortMode = newPanel->Info->StartSortMode;
			if (newPanel->Info->StartSortDesc != oldPanel->Info->StartSortDesc)
				newPanel->ReverseSortOrder = newPanel->Info->StartSortDesc;
		}

		// restore original files
		newPanel->ReplaceFiles(files);
	}

	//! switch to new data and redraw, but not always: in some cases it will be done anyway, e.g. by FAR
	if (!_inAsSetDirectory)
	{
		newPanel->Update(false);
		newPanel->Redraw(0, 0);
	}
}

IPanelPlugin^ Far::GetPanelPlugin(Type^ hostType)
{
	// case: any panel
	if (hostType == nullptr)
	{
		for (int i = 1; i < 4; ++i)
		{
			FarPanelPlugin^ p = _panels[i];
			if (p)
				return p;
		}
		return nullptr;
	}

	// panel with defined host type
	for (int i = 1; i < 4; ++i)
	{
		FarPanelPlugin^ p = _panels[i];
		if (p && p->Host)
		{
			Type^ type = p->Host->GetType();
			if (type == hostType || type->IsSubclassOf(hostType))
				return p;
		}
	}

	return nullptr;
}

void Far::OpenPanelPlugin(FarPanelPlugin^ plugin)
{
	if (!_canOpenPanelPlugin) throw gcnew InvalidOperationException("Can't open a plugin panel at this moment.");
	_panels[0] = plugin;
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
	OpenPluginMenuItemEventArgs e(OpenFrom::Other);
	item->Handler(item, %e);
	return e.Ignore ? false : true;
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
			OpenPluginMenuItemEventArgs e((OpenFrom)from);
			menuItem->Handler(menuItem, %e);
		}
		else if (from == OPEN_PLUGINSMENU || from == OPEN_EDITOR || from == OPEN_VIEWER)
		{
			_canOpenPanelPlugin = (from == OPEN_PLUGINSMENU);
			PluginMenuItem^ menuItem = (PluginMenuItem^)_registeredMenuItems[(int)item];
			OpenPluginMenuItemEventArgs e((OpenFrom)from);
			menuItem->Handler(menuItem, %e);
		}

		// open a waiting panel
		if (_panels[0])
		{
			HANDLE h = AddPanelPlugin(_panels[0]);
			return h;
		}

		// don't open a panel
		return INVALID_HANDLE_VALUE;
	}
	finally
	{
		// drop a waiting panel and set the global lock
		_panels[0] = nullptr;
		_from = OpenFrom::Other;
		_canOpenPanelPlugin = false;
	}
}

void Far::AsGetOpenPluginInfo(HANDLE hPlugin, OpenPluginInfo* info)
{
	FarPanelPlugin^ plugin = _panels[(int)(INT_PTR)hPlugin];
	FarPanelPluginInfo^ pluginInfo = (FarPanelPluginInfo^)plugin->Info;
	if (plugin->_GettingInfo)
		plugin->_GettingInfo(plugin, nullptr);
	*info = pluginInfo->Make();
}

void Far::AsClosePlugin(HANDLE hPlugin)
{
	FarPanelPlugin^ plugin = _panels[(int)(INT_PTR)hPlugin];
	_panels[(int)(INT_PTR)hPlugin] = nullptr;
	if (plugin->_Closed)
		plugin->_Closed(plugin, nullptr);
	((FarPanelPluginInfo^)plugin->Info)->Free();
}

int Far::AsDeleteFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int opMode)
{
	FarPanelPlugin^ plugin = _panels[(int)(INT_PTR)hPlugin];
	if (!plugin->_DeletingFiles)
		return FALSE;
	IList<IFile^>^ files = ItemsToFiles(plugin->Files, panelItem, itemsNumber);
	FilesEventArgs e(files, (OperationModes)opMode, false);
	plugin->_DeletingFiles(plugin, %e);
	return e.Ignore ? FALSE : TRUE;
}

int Far::AsSetDirectory(HANDLE hPlugin, const char* dir, int opMode)
{
	_inAsSetDirectory = true;
	try
	{
		FarPanelPlugin^ plugin = _panels[(int)(INT_PTR)hPlugin];
		if (!plugin->_SettingDirectory)
			return TRUE;
		SettingDirectoryEventArgs e(OemToStr(dir), (OperationModes)opMode);
		plugin->_SettingDirectory(plugin, %e);
		return !e.Ignore;
	}
	finally
	{
		_inAsSetDirectory = false;
	}
}

int Far::AsProcessKey(HANDLE hPlugin, int key, unsigned int controlState)
{
	//! mind rare case: plugin in null already (e.g. closed by AltF12\select folder)
	FarPanelPlugin^ plugin = _panels[(int)(INT_PTR)hPlugin];
	if (!plugin || !plugin->_KeyPressed)
		return FALSE;

	PanelKeyEventArgs e((key & ~PKF_PREPROCESS), (KeyStates)controlState, (key & PKF_PREPROCESS) != 0);
	plugin->_KeyPressed(plugin, %e);
	return e.Ignore;
}

int Far::AsProcessEvent(HANDLE hPlugin, int id, void* param)
{
	FarPanelPlugin^ plugin = _panels[(int)(INT_PTR)hPlugin];
	switch(id)
	{
	case FE_BREAK:
		if (plugin->_CtrlBreakPressed)
			plugin->_CtrlBreakPressed(plugin, nullptr);
		break;
	case FE_CHANGEVIEWMODE:
		if (plugin->_ViewModeChanged)
		{
			ViewModeChangedEventArgs e(OemToStr((const char*)param));
			plugin->_ViewModeChanged(plugin, %e);
		}
		break;
	case FE_CLOSE:
		//? FE_CLOSE issues:
		// *) unwanted extra call on plugin commands entered in command line
		// *) may not be called at all e.g. if tmp panel is opened
		if (plugin->_Closing)
		{
			PanelEventArgs e(OperationModes::None);
			plugin->_Closing(plugin, %e);
			return e.Ignore;
		}
		break;
	case FE_COMMAND:
		if (plugin->_Executing)
		{
			//! We have to try\catch in here in order to return exactly what plugin returns.
			ExecutingEventArgs e(OemToStr((const char*)param));
			try
			{
				plugin->_Executing(plugin, %e);
			}
			catch(Exception^ exception)
			{
				ShowError("Event: Executing", exception);
			}
			return e.Ignore;
		}
		break;
	case FE_IDLE:
		if (plugin->_Idled)
			plugin->_Idled(plugin, nullptr);
		break;
	case FE_REDRAW:
		if (plugin->_Redrawing)
		{
			PanelEventArgs e(OperationModes::None);
			plugin->_Redrawing(plugin, %e);
			return e.Ignore;
		}
		break;
	}
	return FALSE;
}

int Far::AsMakeDirectory(HANDLE hPlugin, char* name, int opMode)
{
	FarPanelPlugin^ plugin = _panels[(int)(INT_PTR)hPlugin];
	if (!plugin->_MakingDirectory)
		return FALSE;
	MakingDirectoryEventArgs e(OemToStr(name), (OperationModes)opMode);
	plugin->_MakingDirectory(plugin, %e);
	return e.Ignore ? FALSE : TRUE;
}

int Far::AsGetFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, char* destPath, int opMode)
{
	FarPanelPlugin^ plugin = _panels[(int)(INT_PTR)hPlugin];
	if (!plugin->_GettingFiles)
		return 0;
	List<IFile^>^ files = ItemsToFiles(plugin->Files, panelItem, itemsNumber);
	GettingFilesEventArgs e(files, (OperationModes)opMode, move != 0, OemToStr(destPath));
	plugin->_GettingFiles(plugin, %e);
	return e.Ignore ? FALSE : TRUE;
}

int Far::AsPutFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, int opMode)
{
	FarPanelPlugin^ plugin = _panels[(int)(INT_PTR)hPlugin];
	if (!plugin->_PuttingFiles)
		return 0;
	List<IFile^>^ files = ItemsToFiles(plugin->Files, panelItem, itemsNumber);
	FilesEventArgs e(files, (OperationModes)opMode, move != 0);
	plugin->_PuttingFiles(plugin, %e);
	return e.Ignore ? FALSE : TRUE;
}

void Far::AsFreeFindData(PluginPanelItem* panelItem)
{
	delete[] (char*)panelItem;
}

int Far::AsGetFindData(HANDLE hPlugin, PluginPanelItem** pPanelItem, int* pItemsNumber, int opMode)
{
	try
	{
		FarPanelPlugin^ plugin = _panels[(int)(INT_PTR)hPlugin];
		if (plugin->_GettingData)
		{
			PanelEventArgs e((OperationModes)opMode);
			plugin->_GettingData(plugin, %e);
			if (e.Ignore)
				return FALSE;
		}
		(*pItemsNumber) = plugin->Files->Count;
		if ((*pItemsNumber) == 0)
		{
			(*pPanelItem) = NULL;
			return TRUE;
		}

		int sizeDesc = 0;
		for each(FarFile^ f in plugin->Files)
		{
			if (SS(f->Description))
				sizeDesc += f->Description->Length + 1;
		}
		int sizeFile = (*pItemsNumber)*sizeof(PluginPanelItem);
		char* buff = new char[sizeFile + sizeDesc];
		char* desc = buff + sizeFile;
		(*pPanelItem) = (PluginPanelItem*)buff;
		memset((*pPanelItem), 0, (*pItemsNumber)*sizeof(PluginPanelItem));

		int i = -1;
		for each(FarFile^ f in plugin->Files)
		{
			++i;

			PluginPanelItem& p = (*pPanelItem)[i];
			FAR_FIND_DATA& d = p.FindData;

			// names
			StrToOem(f->Name->Length >= MAX_PATH ? f->Name->Substring(0, MAX_PATH - 1) : f->Name, d.cFileName);
			if (!String::IsNullOrEmpty(f->AlternateName))
			{
				if (f->AlternateName->Length > 12)
					throw gcnew InvalidOperationException("Alternate name is longer than 12 chars.");
				StrToOem(f->AlternateName, d.cAlternateFileName);
			}

			// other
			d.dwFileAttributes = f->_flags;
			d.nFileSizeLow = (DWORD)(f->Length & 0xFFFFFFFF);
			d.nFileSizeHigh = (DWORD)(f->Length >> 32);
			d.ftCreationTime = DateTimeToFileTime(f->CreationTime);
			d.ftLastAccessTime = DateTimeToFileTime(f->LastAccessTime);
			d.ftLastWriteTime = DateTimeToFileTime(f->LastWriteTime);
			p.UserData = i;

			if (SS(f->Description))
			{
				p.Description = desc;
				desc += f->Description->Length + 1;
				StrToOem(f->Description, p.Description);
			}
		}
		return TRUE;
	}
	catch(Exception^ e)
	{
		if ((opMode & (OPM_FIND | OPM_SILENT)) == 0)
			ShowError(__FUNCTION__, e);
		return FALSE;
	}
}

}
