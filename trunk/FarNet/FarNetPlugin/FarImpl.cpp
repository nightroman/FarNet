#include "StdAfx.h"
#include "FarImpl.h"
#include "FarPanel.h"
#include "FarVersion.h"
#include "Editor.h"
#include "EditorManager.h"
#include "FarCommandLine.h"
#include "InputBox.h"
#include "Menu.h"
#include "Message.h"
#include "PluginMenuItem.h"
#include "Utils.h"

namespace FarManagerImpl
{;
Far::Far()
{
	_panel = gcnew FarPanel(true);
	_anotherPanel = gcnew FarPanel(false);
	_commandLine = gcnew FarCommandLine();
	_registeredMenuItems = gcnew List<IPluginMenuItem^>();
	_menuStrings = nullptr;
	_prefixes = NULL;
	_registeredPrefixes = gcnew Dictionary<String^, StringDelegate^>();
	editorManager = gcnew EditorManager();
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

bool Far::Msg(String^ body,String^ header)
{
	IMessage^ m = CreateMessage();
	array<String^>^ ss = Regex::Split(body, "\r\n");
	m->Body->AddRange(ss);
	m->Header = header;
	m->Buttons->Add("Ok");
	return m->Show();
}

bool Far::Msg(String^ body)
{
	return Msg(body, nullptr);
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

	Dictionary<String^, StringDelegate^>::Enumerator^ i = _registeredPrefixes->GetEnumerator();
	while(i->MoveNext())
	{
		String^ pref = i->Current.Key;
		if (colon != pref->Length || !cmdLine->StartsWith(pref))
			continue;
		StringDelegate^ handler = i->Current.Value;
		handler->Invoke(cmdLine->Substring(colon + 1));
		break;
	}
}

int Far::HWnd::get()
{
	return Info.AdvControl(Info.ModuleNumber, ACTL_GETFARHWND, nullptr);
}

IVersion^ Far::Version::get()
{
	DWORD vn;
	Info.AdvControl(Info.ModuleNumber, ACTL_GETFARVERSION, &vn);
	return gcnew FarVersion((vn&0x0000ff00)>>8, vn&0x000000ff, (int)((long)vn&0xffff0000)>>16);
}

IMenu^ Far::CreateMenu()
{
	return gcnew Menu();
}

ICollection<IEditor^>^ Far::Editors::get()
{
	return editorManager->Editors;
}

IAnyEditor^ Far::AnyEditor::get()
{
	return editorManager->AnyEditor;
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
	STR_ARG(value);
	Info.FSF->CopyToClipboard(pcvalue);
}

IEditor^ Far::CreateEditor()
{
	return editorManager->CreateEditor();
}

IRect^ Far::CreateRect(int left, int top, int right, int bottom)
{
	return gcnew Rect(left, top, right, bottom);
}

ITwoPoint^ Far::CreateStream(int left, int top, int right, int bottom)
{
	return gcnew FarManager::Impl::Stream(left, top, right, bottom);
}

IList<int>^ Far::CreateKeySequence(String^ keys)
{
	List<int>^ r = gcnew List<int>;
	array<wchar_t>^ space = {' '};
	array<String^>^ a = keys->Split(space);
	for(int i = 0; i < a->Length; ++i)
	{
		int k = NameToKey(a[i]);
		if (k == -1)
			throw gcnew ArgumentException("Argument 'keys' contains unknown key: " + a[i]);
		r->Add(k);
	}
	return r;
}

void Far::PostKeySequence(IList<int>^ sequence, bool disableOutput)
{
	if (sequence == nullptr)
		throw gcnew ArgumentNullException("sequence");

	KeySequence keySequence;
	keySequence.Count = sequence->Count;
	keySequence.Flags = disableOutput?KSFLAGS_DISABLEOUTPUT:0;
	keySequence.Sequence = new DWORD[keySequence.Count];
	Collections::IEnumerator^ i = sequence->GetEnumerator();
	DWORD* cur = keySequence.Sequence;
	while(i->MoveNext())
	{
		Int32^ curV = dynamic_cast<Int32^>(i->Current);
		DWORD value = *curV;
		*cur = value;
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
	PostKeySequence(CreateKeySequence(keys), disableOutput);
}

int Far::SaveScreen(int x1, int y1, int x2, int y2)
{
	return (int)Info.SaveScreen(x1, y1, x2, y2);
}

void Far::RestoreScreen(int screen)
{
	Info.RestoreScreen((HANDLE)screen);
}

ICommandLine^ Far::CommandLine::get()
{
	return _commandLine;
}

IEditor^ Far::Editor::get()
{
	return editorManager->GetCurrentEditor();
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
	pi->Flags = PF_EDITOR + PF_VIEWER + PF_FULLCMDLINE + PF_PRELOAD;

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
	for(int i = 0; i < _registeredMenuItems->Count; i++)
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
		Dictionary<String^, StringDelegate^>::Enumerator^ i = _registeredPrefixes->GetEnumerator();
		while(i->MoveNext())
		{
			if (PrefString->Length > 0)
				PrefString = String::Concat(PrefString, ":");
			PrefString = String::Concat(PrefString, i->Current.Key);
		}
		_prefixes = new CStr(PrefString);
	}
	else
		_prefixes = NULL;
}

void Far::SetUserScreen()
{
	Info.Control(INVALID_HANDLE_VALUE, FCTL_SETUSERSCREEN, 0);
}

Object^ Far::Test()
{
	return nullptr;
}
}
