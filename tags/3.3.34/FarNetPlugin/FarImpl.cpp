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
#include "PluginSet.h"
#include "Viewer.h"
using namespace Microsoft::Win32;
using namespace System::Reflection;

namespace FarManagerImpl
{;
PluginAny::PluginAny(BasePlugin^ plugin, String^ name)
: _Name(name)
{
	if (plugin)
	{
		Assembly^ a = Assembly::GetAssembly(plugin->GetType());
		_Key = "FAR.NET\\" + Path::GetFileName(a->Location) + "\\" + _Name->Replace("\\", "/");
	}
	else
	{
		_Key = "FAR.NET\\:" + _Name->Replace("\\", "/");
	}
}

PluginTool::PluginTool(BasePlugin^ plugin, String^ name, EventHandler<ToolEventArgs^>^ handler, ToolOptions options)
: PluginAny(plugin, name)
, _Handler(handler)
, _Options(options)
{}

String^ PluginTool::Alias(ToolOptions option)
{
	if (ES(Name))
		return String::Empty;
	switch(option)
	{
	case ToolOptions::Config:
		if (ES(_AliasConfig))
			_AliasConfig = Far::Get()->GetPluginValue(Key, "Config", Name)->ToString();
		return _AliasConfig;
	case ToolOptions::Disk:
		if (ES(_AliasDisk))
			_AliasDisk = Far::Get()->GetPluginValue(Key, "Disk", Name)->ToString();
		return _AliasDisk;
	case ToolOptions::Editor:
		if (ES(_AliasEditor))
			_AliasEditor = Far::Get()->GetPluginValue(Key, "Editor", Name)->ToString();
		return _AliasEditor;
	case ToolOptions::Panels:
		if (ES(_AliasPanels))
			_AliasPanels = Far::Get()->GetPluginValue(Key, "Panels", Name)->ToString();
		return _AliasPanels;
	case ToolOptions::Viewer:
		if (ES(_AliasViewer))
			_AliasViewer = Far::Get()->GetPluginValue(Key, "Viewer", Name)->ToString();
		return _AliasViewer;
	default:
		throw gcnew InvalidOperationException("Unknown tool option.");
	}
}

void PluginTool::Alias(ToolOptions option, String^ value)
{
	if (ES(value))
		throw gcnew ArgumentException("'value' must not be empty.");

	switch(option)
	{
	case ToolOptions::Config:
		Far::Get()->SetPluginValue(Key, "Config", value);
		_AliasConfig = value;
		break;
	case ToolOptions::Disk:
		Far::Get()->SetPluginValue(Key, "Disk", value);
		_AliasDisk = value;
		break;
	case ToolOptions::Editor:
		Far::Get()->SetPluginValue(Key, "Editor", value);
		_AliasEditor = value;
		break;
	case ToolOptions::Panels:
		Far::Get()->SetPluginValue(Key, "Panels", value);
		_AliasPanels = value;
		break;
	case ToolOptions::Viewer:
		Far::Get()->SetPluginValue(Key, "Viewer", value);
		_AliasViewer = value;
		break;
	default:
		throw gcnew InvalidOperationException("Unknown tool option.");
	}
}

String^ PluginPrefix::Alias()
{
	if (ES(_Alias))
		_Alias = Far::Get()->GetPluginValue(Key, "Prefix", Prefix)->ToString();
	return _Alias;
}

void PluginPrefix::Alias(String^ value)
{
	if (ES(value))
		throw gcnew ArgumentException("'value' must not be empty.");

	Far::Get()->SetPluginValue(Key, "Prefix", value);
	_Alias = value;
}

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
	_instance->Start();
}

void Far::Start()
{
	RegisterTool(nullptr, String::Empty, gcnew EventHandler<ToolEventArgs^>(this, &Far::OnNetF11Menus), ToolOptions::F11Menus);
	RegisterTool(nullptr, String::Empty, gcnew EventHandler<ToolEventArgs^>(this, &Far::OnNetConfig), ToolOptions::Config);
	RegisterTool(nullptr, String::Empty, gcnew EventHandler<ToolEventArgs^>(this, &Far::OnNetDisk), ToolOptions::Disk);
	PluginSet::LoadPlugins();
}

//! Don't use FAR UI
void Far::Stop()
{
	PluginSet::UnloadPlugins();
	_instance = nullptr;

	delete[] _pConfig;
	delete[] _pDisk;
	delete[] _pEditor;
	delete[] _pPanels;
	delete[] _pViewer;
	delete _prefixes;
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
	if (_registeredTools.ContainsKey(handler))
		throw gcnew InvalidOperationException("Can't register the handler because it is already registered.");
	if (plugin && ES(name))
		throw gcnew ArgumentException("'name' must not be empty.");

	PluginTool^ tool = gcnew PluginTool(plugin, name, handler, options);
	_registeredTools.Add(handler, tool);

	if (int(options & ToolOptions::Config))
	{
		delete[] _pConfig;
		_pConfig = 0;
		_registeredConfig.Add(tool);
	}
	if (int(options & ToolOptions::Disk))
	{
		delete[] _pDisk;
		_pDisk = 0;
		_registeredDisk.Add(tool);
	}
	if (int(options & ToolOptions::Editor))
	{
		delete[] _pEditor;
		_pEditor = 0;
		_registeredEditor.Add(tool);
	}
	if (int(options & ToolOptions::Panels))
	{
		delete[] _pPanels;
		_pPanels = 0;
		_registeredPanels.Add(tool);
	}
	if (int(options & ToolOptions::Viewer))
	{
		delete[] _pViewer;
		_pViewer = 0;
		_registeredViewer.Add(tool);
	}
}

void Far::UnregisterTool(EventHandler<ToolEventArgs^>^ handler)
{
	if (_registeredTools.ContainsKey(handler))
	{
		PluginTool^ tool = _registeredTools[handler];
		_registeredTools.Remove(handler);

		ToolOptions options = tool->Options;
		if (int(options & ToolOptions::Config))
		{
			delete[] _pConfig;
			_pConfig = 0;
			_registeredConfig.Remove(tool);
		}
		if (int(options & ToolOptions::Disk))
		{
			delete[] _pDisk;
			_pDisk = 0;
			_registeredDisk.Remove(tool);
		}
		if (int(options & ToolOptions::Editor))
		{
			delete[] _pEditor;
			_pEditor = 0;
			_registeredEditor.Remove(tool);
		}
		if (int(options & ToolOptions::Panels))
		{
			delete[] _pPanels;
			_pPanels = 0;
			_registeredPanels.Remove(tool);
		}
		if (int(options & ToolOptions::Viewer))
		{
			delete[] _pViewer;
			_pViewer = 0;
			_registeredViewer.Remove(tool);
		}
	}
}

String^ Far::RegisterPrefix(BasePlugin^ plugin, String^ name, String^ prefix, EventHandler<ExecutingEventArgs^>^ handler)
{
	delete _prefixes;
	_prefixes = 0;
	PluginPrefix^ it = gcnew PluginPrefix(plugin, name, prefix, handler);
	_registeredPrefix.Add(it);
	return it->Alias();
}

void Far::UnregisterPrefix(EventHandler<ExecutingEventArgs^>^ handler)
{
	for(int i = _registeredPrefix.Count; --i >= 0;)
	{
		if (_registeredPrefix[i]->Handler == handler)
		{
			delete _prefixes;
			_prefixes = 0;
			_registeredPrefix.RemoveAt(i);
		}
	}
}

void Far::RegisterFile(BasePlugin^ plugin, String^ name, EventHandler<OpenFileEventArgs^>^ handler)
{
	_registeredFile.Add(gcnew PluginFile(plugin, name, handler));
}

void Far::UnregisterFile(EventHandler<OpenFileEventArgs^>^ handler)
{
	for(int i = _registeredFile.Count; --i >= 0;)
		if (_registeredFile[i]->Handler == handler)
			_registeredFile.RemoveAt(i);
}

void Far::Msg(String^ body)
{
	Message::Show(body, nullptr, MessageOptions::Ok, nullptr);
}

void Far::Msg(String^ body, String^ header)
{
	Message::Show(body, header, MessageOptions::Ok, nullptr);
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

	for each(PluginPrefix^ it in _registeredPrefix)
	{
		String^ pref = it->Alias();
		if (colon != pref->Length || !command->StartsWith(pref))
			continue;
		ExecutingEventArgs e(command->Substring(colon + 1));
		it->Handler(this, %e);
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

String^ Far::Clipboard::get() //?? obsolete
{
	return OemToStr(Info.FSF->PasteFromClipboard());
}

String^ Far::PasteFromClipboard()
{
	return OemToStr(Info.FSF->PasteFromClipboard());
}

void Far::Clipboard::set(String^ value) //?? obsolete
{
	CStr pcvalue(value);
	Info.FSF->CopyToClipboard(pcvalue);
}

void Far::CopyToClipboard(String^ text)
{
	CStr sText(text);
	Info.FSF->CopyToClipboard(sText);
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
	if (sequence == nullptr) throw gcnew ArgumentNullException("sequence");
	if (sequence->Length == 0)
		return;

	// local buffer for a small sequence
	const int smallCount = 50;
	DWORD keys[smallCount];

	KeySequence keySequence;
	keySequence.Count = sequence->Length;
	keySequence.Flags = disableOutput ? KSFLAGS_DISABLEOUTPUT : 0;
	keySequence.Sequence = keySequence.Count <= smallCount ? keys : new DWORD[keySequence.Count];
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
		if (keySequence.Sequence != keys)
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

	WindowInfo wi;
	wi.Pos = -1;
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_GETSHORTWINDOWINFO, &wi))
		wi.Type = -1;

	if (_registeredConfig.Count)
	{
		if (!_pConfig)
		{
			_pConfig = new CStr[_registeredConfig.Count];
			for(int i = _registeredConfig.Count; --i >= 0;)
				_pConfig[i].Set(Res::MenuPrefix + _registeredConfig[i]->Alias(ToolOptions::Config));
		}
		pi->PluginConfigStringsNumber = _registeredConfig.Count;
		pi->PluginConfigStrings = (const char**)_pConfig;
	}

	if (_registeredDisk.Count)
	{
		if (!_pDisk)
		{
			_pDisk = new CStr[_registeredDisk.Count];
			for(int i = _registeredDisk.Count; --i >= 0;)
				_pDisk[i].Set(Res::MenuPrefix + _registeredDisk[i]->Alias(ToolOptions::Disk));
		}
		pi->DiskMenuStringsNumber = _registeredDisk.Count;
		pi->DiskMenuStrings = (const char**)_pDisk;
	}

	switch(wi.Type)
	{
	case WTYPE_EDITOR:
		if (_registeredEditor.Count)
		{
			if (!_pEditor)
			{
				_pEditor = new CStr[_registeredEditor.Count];
				for(int i = _registeredEditor.Count; --i >= 0;)
					_pEditor[i].Set(Res::MenuPrefix + _registeredEditor[i]->Alias(ToolOptions::Editor));
			}
			pi->PluginMenuStringsNumber = _registeredEditor.Count;
			pi->PluginMenuStrings = (const char**)_pEditor;
		}
		break;
	case WTYPE_PANELS:
		if (_registeredPanels.Count)
		{
			if (!_pPanels)
			{
				_pPanels = new CStr[_registeredPanels.Count];
				for(int i = _registeredPanels.Count; --i >= 0;)
					_pPanels[i].Set(Res::MenuPrefix + _registeredPanels[i]->Alias(ToolOptions::Panels));
			}
			pi->PluginMenuStringsNumber = _registeredPanels.Count;
			pi->PluginMenuStrings = (const char**)_pPanels;
		}
		break;
	case WTYPE_VIEWER:
		if (_registeredViewer.Count)
		{
			if (!_pViewer)
			{
				_pViewer = new CStr[_registeredViewer.Count];
				for(int i = _registeredViewer.Count; --i >= 0;)
					_pViewer[i].Set(Res::MenuPrefix + _registeredViewer[i]->Alias(ToolOptions::Viewer));
			}
			pi->PluginMenuStringsNumber = _registeredViewer.Count;
			pi->PluginMenuStrings = (const char**)_pViewer;
		}
		break;
	}

	if (_registeredPrefix.Count)
	{
		if (_prefixes == 0)
		{
			String^ PrefString = String::Empty;
			for each(PluginPrefix^ it in _registeredPrefix)
			{
				if (PrefString->Length > 0)
					PrefString = String::Concat(PrefString, ":");
				PrefString = String::Concat(PrefString, it->Alias());
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
	switch(Msg(
		error->Message,
		String::IsNullOrEmpty(title) ? error->GetType()->FullName : title,
		MessageOptions::LeftAligned | MessageOptions::Warning,
		gcnew array<String^>{"Ok", "View Info", "Copy Info"}))
	{
	case 1:
		ShowExceptionInfo(error);
		return;
	case 2:
		CopyToClipboard(ExceptionInfo(error, false) + "\r\n" + error->ToString());
		return;
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
	if (!ValueUserScreen::Get())
	{
		ValueUserScreen::Set(true);
		GetUserScreen();
	}
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

WindowType Far::GetWindowType(int index)
{
	WindowInfo wi;
	wi.Pos = index;
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_GETSHORTWINDOWINFO, &wi))
		throw gcnew InvalidOperationException("GetWindowType:" + index + " failed.");
	return (WindowType)wi.Type;
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
	RegistryKey^ key1;
	RegistryKey^ key2;
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

Object^ Far::GetFarValue(String^ keyPath, String^ valueName, Object^ defaultValue)
{
	RegistryKey^ key1;
	RegistryKey^ key2;
	try
	{
		key1 = Registry::CurrentUser->OpenSubKey(RootFar);
		key2 = key1->OpenSubKey(keyPath);
		return key2->GetValue(valueName, defaultValue);
	}
	catch(...)
	{
		return defaultValue;
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
	RegistryKey^ key1;
	RegistryKey^ key2;
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
	PluginTool^ tool = _registeredConfig[itemIndex];
	ToolEventArgs e(ToolOptions::Config);
	tool->Handler(this, %e);
	return e.Ignore ? false : true;
}

HANDLE Far::AsOpenFilePlugin(char* name, const unsigned char* data, int dataSize)
{
	if (_registeredFile.Count == 0)
		return INVALID_HANDLE_VALUE;

	ValueCanOpenPanel canopenpanel(true);
	ValueUserScreen userscreen;

	try
	{
		OpenFileEventArgs e;
		e.Name = OemToStr(name);
		e.Data = gcnew array<Byte>(dataSize);
		for(int i = dataSize; --i >= 0;)
			e.Data[i] = data[i];

		for each(PluginFile^ it in _registeredFile)
		{
			it->Handler(this, %e);

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
	}
}

HANDLE Far::AsOpenPlugin(int from, INT_PTR item)
{
	ValueCanOpenPanel canopenpanel(true);
	ValueUserScreen userscreen;

	try
	{
		// call, plugin may create a panel waiting for opening
		switch(from)
		{
		case OPEN_COMMANDLINE:
			ProcessPrefixes(item);
			break;
		case OPEN_DISKMENU:
			{
				PluginTool^ tool = _registeredDisk[(int)item];
				ToolEventArgs e(ToolOptions::Disk);
				tool->Handler(this, %e);
			}
			break;
		case OPEN_PLUGINSMENU:
			{
				PluginTool^ tool = _registeredPanels[(int)item];
				ToolEventArgs e(ToolOptions::Panels);
				tool->Handler(this, %e);
			}
			break;
		case OPEN_EDITOR:
			{
				ValueCanOpenPanel::Set(false);
				PluginTool^ tool = _registeredEditor[(int)item];
				ToolEventArgs e(ToolOptions::Editor);
				tool->Handler(this, %e);
			}
			break;
		case OPEN_VIEWER:
			{
				ValueCanOpenPanel::Set(false);
				PluginTool^ tool = _registeredViewer[(int)item];
				ToolEventArgs e(ToolOptions::Viewer);
				tool->Handler(this, %e);
			}
			break;
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
		// drop a waiting panel
		PanelSet::_panels[0] = nullptr;
	}
}

array<IPanelPlugin^>^ Far::PushedPanels()
{
	array<IPanelPlugin^>^ r = gcnew array<IPanelPlugin^>(PanelSet::_stack.Count);
	for(int i = PanelSet::_stack.Count; --i >= 0;)
		r[i] = PanelSet::_stack[i];
	return r;
}

void Far::ShowPanelMenu(bool showPushCommand)
{
	Menu m;
	m.Title = "Push/show panels";
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

void Far::PostStep(EventHandler^ step)
{
	// make keys
	if (!_hotkeys)
	{
		if (ES(_hotkey))
		{
			_hotkey = GetFarValue("PluginHotkeys\\Plugins/Far.Net/FarNetPlugin.dll", "Hotkey", String::Empty)->ToString();
			if (ES(_hotkey))
				throw gcnew InvalidOperationException(Res::ErrorNoHotKey);
		}
		array<int>^ keys = gcnew array<int>(2);
		keys[1] = NameToKey(_hotkey);
		keys[0] = NameToKey("F11");
		_hotkeys = keys;
	}

	// post handler and keys
	_handler = step;
	PostKeySequence(_hotkeys);
}

void Far::OnNetF11Menus(Object^ /*sender*/, ToolEventArgs^ e)
{
	if (_handler) //??
	{
		EventHandler^ handler = _handler;
		_handler = nullptr;
		handler(nullptr, nullptr);
		return;
	}

	if (e->From == ToolOptions::Panels)
		ShowPanelMenu(true);
}

void Far::OnNetDisk(Object^ /*sender*/, ToolEventArgs^ /*e*/)
{
	ShowPanelMenu(false);
}

void Far::OnNetConfig(Object^ /*sender*/, ToolEventArgs^ /*e*/)
{
	Menu m;
	m.Title = "FAR.NET tools";
	m.AutoAssignHotkeys = true;

	m.Items->Add(Res::PanelsTools + " : " + (_registeredPanels.Count - 1));
	m.Items->Add(Res::EditorTools + " : " + (_registeredEditor.Count - 1));
	m.Items->Add(Res::ViewerTools + " : " + (_registeredViewer.Count - 1));
	m.Items->Add(Res::DiskTools + "   : " + (_registeredDisk.Count - 1));
	m.Items->Add(Res::ConfigTools + " : " + (_registeredConfig.Count - 1));
	m.Items->Add(Res::PrefixTools + " : " + (_registeredPrefix.Count));
	m.Items->Add(Res::FilePlugins + " : " + (_registeredFile.Count));

	while(m.Show())
	{
		switch(m.Selected)
		{
		case 0:
			if (_registeredPanels.Count)
				OnConfigTool(Res::PanelsTools, ToolOptions::Panels, %_registeredPanels);
			break;
		case 1:
			if (_registeredEditor.Count)
				OnConfigTool(Res::EditorTools, ToolOptions::Editor, %_registeredEditor);
			break;
		case 2:
			if (_registeredViewer.Count)
				OnConfigTool(Res::ViewerTools, ToolOptions::Viewer, %_registeredViewer);
			break;
		case 3:
			Msg("Under construction.");
			break;
		case 4:
			if (_registeredConfig.Count)
				OnConfigTool(Res::ConfigTools, ToolOptions::Config, %_registeredConfig);
			break;
		case 5:
			if (_registeredPrefix.Count)
				OnConfigPrefix();
			break;
		case 6:
			if (_registeredFile.Count)
				OnConfigFile();
			break;
		}
	}
}

void Far::OnConfigTool(String^ title, ToolOptions option, List<PluginTool^>^ list)
{
	Menu m;
	m.Title = title;
	m.HelpTopic = "EditMenuStrings";

	for each(PluginTool^ it in list)
	{
		if (ES(it->Name))
			continue;
		IMenuItem^ mi = m.Items->Add(Res::MenuPrefix + it->Alias(option) + " : " + it->Name);
		mi->Data = it;
	}

	while(m.Show())
	{
		IMenuItem^ mi = m.Items[m.Selected];
		PluginTool^ it = (PluginTool^)mi->Data;

		InputBox ib;
		ib.EmptyEnabled = true;
		ib.HelpTopic = _helpTopic + "EditMenuStrings";
		ib.Prompt = "New string (ampersand ~ hotkey)";
		ib.Text = it->Alias(option);
		ib.Title = "Original: " + it->Name;
		if (!ib.Show())
			continue;

		// restore the name on empty alias
		String^ alias = ib.Text->TrimEnd();
		if (alias->Length == 0)
			alias = it->Name;

		// reset the alias
		Free(option);
		it->Alias(option, alias);
		mi->Text = Res::MenuPrefix + alias + " : " + it->Name;
	}
}

void Far::OnConfigPrefix()
{
	Menu m;
	m.Title = Res::PrefixTools;
	m.AutoAssignHotkeys = true;
	m.HelpTopic = "EditPrefixes";

	for each(PluginPrefix^ it in _registeredPrefix)
	{
		IMenuItem^ mi = m.Items->Add(it->Alias()->PadRight(4) + " " + it->Name);
		mi->Data = it;
	}

	while(m.Show())
	{
		IMenuItem^ mi = m.Items[m.Selected];
		PluginPrefix^ it = (PluginPrefix^)mi->Data;

		InputBox ib;
		ib.EmptyEnabled = true;
		ib.HelpTopic = _helpTopic + "EditPrefixes";
		ib.Prompt = "New prefix for " + it->Name;
		ib.Text = it->Alias();
		ib.Title = "Original: " + it->Prefix;

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
			alias = it->Prefix;

		// reset
		delete _prefixes;
		_prefixes = 0;
		it->Alias(alias);
		mi->Text = alias->PadRight(4) + " " + it->Name;
	}
}

void Far::OnConfigFile()
{
	Menu m;
	m.Title = Res::FilePlugins;
	m.AutoAssignHotkeys = true;

	for each(PluginFile^ it in _registeredFile)
	{
		IMenuItem^ mi = m.Items->Add(it->Name);
		mi->Data = it;
	}

	while(m.Show())
	{}
}

}
