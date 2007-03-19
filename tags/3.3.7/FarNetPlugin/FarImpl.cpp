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
#include "PluginMenuItem.h"
#include "Viewer.h"
using namespace System::Reflection;

namespace FarManagerImpl
{;
Object^ Far::Test()
{
	return nullptr;
}

Far::Far()
: _menuStrings(NULL)
, _prefixes(NULL)
, _panel(gcnew FarPanel(true))
, _anotherPanel(gcnew FarPanel(false))
, _registeredMenuItems(gcnew List<IPluginMenuItem^>())
, _registeredPrefixes(gcnew Dictionary<String^, StringDelegate^>())
, _editorManager(gcnew EditorManager())
{
}

Far::~Far()
{
	FreeMenuStrings();
	delete _prefixes;
}

String^ Far::PluginFolderPath::get()
{
	String^ pluginPath = OemToStr(Info.ModuleName);
	return (gcnew FileInfo(pluginPath))->DirectoryName;
}

void Far::RegisterPrefix(String^ prefix, StringDelegate^ handler)
{
	_registeredPrefixes->Add(prefix, handler);
}

void Far::RegisterPluginsMenuItem(IPluginMenuItem^ item)
{
	_registeredMenuItems->Add(item);
}

IPluginMenuItem^ Far::RegisterPluginsMenuItem(String^ name, EventHandler<OpenPluginMenuItemEventArgs^>^ onOpen)
{
	IPluginMenuItem^ r = CreatePluginsMenuItem();
	r->Name = name;
	r->OnOpen += onOpen;
	RegisterPluginsMenuItem(r);
	return r;
}

void Far::UnregisterPluginsMenuItem(IPluginMenuItem^ item)
{
	_registeredMenuItems->Remove(item);
}

IPluginMenuItem^ Far::CreatePluginsMenuItem()
{
	return gcnew PluginMenuItem();
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

void Far::Run(String^ cmdLine)
{
	int colon = cmdLine->IndexOf(':');
	if (colon < 0)
		return;

	for each(KeyValuePair<String^, StringDelegate^>^ i in _registeredPrefixes)
	{
		String^ pref = i->Key;
		if (colon != pref->Length || !cmdLine->StartsWith(pref))
			continue;
		StringDelegate^ handler = i->Value;
		handler->Invoke(cmdLine->Substring(colon + 1));
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

IList<int>^ Far::CreateKeySequence(String^ keys)
{
	List<int>^ r = gcnew List<int>;
	if (keys->Length > 0)
	{
		array<wchar_t>^ space = {' '};
		array<String^>^ a = keys->Split(space);
		for each(String^ s in a)
		{
			int k = NameToKey(s);
			if (k == -1)
				throw gcnew ArgumentException("Argument 'keys' contains unknown key: " + s);
			r->Add(k);
		}
	}
	return r;
}

void Far::PostKeySequence(IList<int>^ sequence, bool disableOutput)
{
	if (sequence == nullptr)
		throw gcnew ArgumentNullException("sequence");
	if (sequence->Count == 0)
		return;

	KeySequence keySequence;
	keySequence.Count = sequence->Count;
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

int Far::NameToKey(String^ key)
{
	CStr sKey(key);
	return Info.FSF->FarNameToKey(sKey);
}

void Far::PostKeys(String^ keys, bool disableOutput)
{
	if (keys == nullptr)
		throw gcnew ArgumentNullException("keys");

	keys = keys->Trim();
	PostKeySequence(CreateKeySequence(keys), disableOutput);
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
	return (int)Info.SaveScreen(x1, y1, x2, y2);
}

void Far::RestoreScreen(int screen)
{
	Info.RestoreScreen((HANDLE)screen);
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
	return _panel;
}

IPanel^ Far::AnotherPanel::get()
{
	return _anotherPanel;
}

IInputBox^ Far::CreateInputBox()
{
	return gcnew InputBox();
}

void Far::OnGetPluginInfo(PluginInfo* pi)
{
	pi->StructSize = sizeof(PluginInfo);
	pi->Flags = PF_EDITOR | PF_VIEWER | PF_FULLCMDLINE | PF_PRELOAD;

	CreateMenuStringsBlock();
	pi->PluginMenuStrings = (char**)_menuStrings;
	pi->PluginMenuStringsNumber = _registeredMenuItems->Count;

	MakePrefixes();
	if (_registeredPrefixes->Count > 0)
		pi->CommandPrefix = _prefixes ? *_prefixes : NULL;
}

void Far::CreateMenuStringsBlock()
{
	FreeMenuStrings();
	_menuStrings = new CStr[_registeredMenuItems->Count];
	IPluginMenuItem^ Item;
	for(int i = 0; i < _registeredMenuItems->Count; ++i)
	{
		Item = dynamic_cast<IPluginMenuItem^>(_registeredMenuItems[i]);
		_menuStrings[i].Set(Item->Name);
	}
}

void Far::FreeMenuStrings()
{
	delete[] _menuStrings;
	_menuStrings = NULL;
}

HANDLE Far::OnOpenPlugin(int from, int item)
{
	if (from == OPEN_COMMANDLINE)
	{
		ProcessPrefixes(item);
	}
	else
	{
		if (from == OPEN_PLUGINSMENU || from == OPEN_EDITOR || from == OPEN_VIEWER)
		{
			IPluginMenuItem^ MenuItem = _registeredMenuItems[item];
			MenuItem->FireOnOpen(MenuItem, (OpenFrom)from);
		}
	}
	return INVALID_HANDLE_VALUE;
}

void Far::ProcessPrefixes(int Item)
{
	char* commandLine = (char* )Item;
	Run(OemToStr(commandLine));
}

void Far::MakePrefixes()
{
	if (_registeredPrefixes->Count > 0)
	{
		String^ PrefString = String::Empty;
		for each(KeyValuePair<String^, StringDelegate^>^ i in _registeredPrefixes)
		{
			if (PrefString->Length > 0)
				PrefString = String::Concat(PrefString, ":");
			PrefString = String::Concat(PrefString, i->Key);
		}
		_prefixes = new CStr(PrefString);
	}
	else
		_prefixes = NULL;
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
	String^ keyName = "Software\\Far\\" + name;
	CStr sKeyName(keyName);

	HKEY hk;
	LONG lResult = ::RegOpenKeyEx(HKEY_CURRENT_USER, sKeyName, 0, KEY_WRITE|KEY_READ, &hk);
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
	File::WriteAllText(path, ExceptionInfo(e) + "\n" + e->ToString(), System::Text::Encoding::Unicode);

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
	return gcnew FarDialog(this, left, top, right, bottom);
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
}
