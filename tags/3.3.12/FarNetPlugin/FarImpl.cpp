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
#include "PluginMenuItem.h"
#include "Viewer.h"
using namespace System::Reflection;

namespace FarManagerImpl
{;
ref class FarPanelPluginInfo : IPanelPluginInfo
{
internal:
	FarPanelPluginInfo()
		: m(new OpenPluginInfo)
	{
		memset(m, 0, sizeof(*m));
		m->StructSize = sizeof(*m);
	}
	~FarPanelPluginInfo()
	{
		if (m)
		{
			delete m->CurDir;
			delete m->Format;
			delete m->HostFile;
			delete m->PanelTitle;
			delete m;
		}
	}
	OpenPluginInfo& Get()
	{
		return *m;
	}
public:
	virtual property bool AddDots { bool get(); void set(bool value); }
	virtual property bool CompareFatTime { bool get(); void set(bool value); }
	virtual property bool ConfirmClose;
	virtual property bool ExternalDelete { bool get(); void set(bool value); }
	virtual property bool ExternalGet { bool get(); void set(bool value); }
	virtual property bool ExternalMakeDirectory { bool get(); void set(bool value); }
	virtual property bool ExternalPut { bool get(); void set(bool value); }
	virtual property bool PreserveCase { bool get(); void set(bool value); }
	virtual property bool RawSelection { bool get(); void set(bool value); }
	virtual property bool RealNames { bool get(); void set(bool value); }
	virtual property bool ShowNamesOnly { bool get(); void set(bool value); }
	virtual property bool RightAligned { bool get(); void set(bool value); }
	virtual property bool UseAttrHighlighting { bool get(); void set(bool value); }
	virtual property bool UseFilter { bool get(); void set(bool value); }
	virtual property bool UseHighlighting { bool get(); void set(bool value); }
	virtual property bool UseSortGroups { bool get(); void set(bool value); }
	virtual property String^ CurrentDirectory { String^ get(); void set(String^ value); }
	virtual property String^ Format { String^ get(); void set(String^ value); }
	virtual property String^ HostFile { String^ get(); void set(String^ value); }
	virtual property String^ Title { String^ get(); void set(String^ value); }
	virtual property bool StartSortDesc
	{
		bool get() { return m->StartSortOrder != 0; }
		void set(bool value) { m->StartSortOrder = value; }
	}
	virtual property PanelSortMode StartSortMode
	{
		PanelSortMode get() { return (PanelSortMode)(m->StartSortMode - 0x30); }
		void set(PanelSortMode value) { m->StartSortMode = (int)value; }
	}
	virtual property PanelViewMode StartViewMode
	{
		PanelViewMode get() { return (PanelViewMode)m->StartPanelMode; }
		void set(PanelViewMode value) { m->StartPanelMode = (int)value + 0x30; }
	}
private:
	OpenPluginInfo* m;
};

// Flags
#define DEF_FLAG(Prop, Flag)\
bool FarPanelPluginInfo::Prop::get() { return (m->Flags & (Flag)) != 0; }\
void FarPanelPluginInfo::Prop::set(bool value) { { if (value) m->Flags |= Flag; else m->Flags &= ~Flag; } }
DEF_FLAG(AddDots, OPIF_ADDDOTS);
DEF_FLAG(CompareFatTime, OPIF_COMPAREFATTIME);
DEF_FLAG(ExternalDelete, OPIF_EXTERNALDELETE);
DEF_FLAG(ExternalGet, OPIF_EXTERNALGET);
DEF_FLAG(ExternalMakeDirectory, OPIF_EXTERNALMKDIR);
DEF_FLAG(ExternalPut, OPIF_EXTERNALPUT);
DEF_FLAG(PreserveCase, OPIF_SHOWPRESERVECASE);
DEF_FLAG(RawSelection, OPIF_RAWSELECTION);
DEF_FLAG(RealNames, OPIF_REALNAMES);
DEF_FLAG(RightAligned, OPIF_SHOWRIGHTALIGNNAMES);
DEF_FLAG(ShowNamesOnly, OPIF_SHOWNAMESONLY);
DEF_FLAG(UseAttrHighlighting, OPIF_USEATTRHIGHLIGHTING);
DEF_FLAG(UseFilter, OPIF_USEFILTER);
DEF_FLAG(UseHighlighting, OPIF_USEHIGHLIGHTING);
DEF_FLAG(UseSortGroups, OPIF_USESORTGROUPS);

// Strings
#define DEF_STRING(Prop, Field)\
String^ FarPanelPluginInfo::Prop::get()\
{\
	if (!m->Field || !m->Field[0])\
		return nullptr;\
	return OemToStr(m->Field);\
}\
void FarPanelPluginInfo::Prop::set(String^ value)\
{\
	if (m->Field)\
		delete m->Field;\
	if (String::IsNullOrEmpty(value))\
	{\
		m->Field = NULL;\
		return;\
	}\
	m->Field = new char[value->Length + 1];\
	StrToOem(value, (char*)m->Field);\
}
DEF_STRING(CurrentDirectory, CurDir);
DEF_STRING(Format, Format);
DEF_STRING(HostFile, HostFile);
DEF_STRING(Title, PanelTitle);

ref class FarPanelPlugin : IPanelPlugin
{
public:
	virtual property int Id
	{
		int get() { return _id; }
	}
	virtual property IPanelPluginInfo^ Info
	{
		IPanelPluginInfo^ get() { return %_info; }
	}
	virtual property IList<IFile^>^ Files
	{
		IList<IFile^>^ get() { return %_files; }
	}
	virtual property Object^ Data;
public: DEF_EVENT(GettingInfo, _GettingInfo);
public: DEF_EVENT(Closed, _Closed);
public: DEF_EVENT(CtrlBreakPressed, _CtrlBreakPressed);
public: DEF_EVENT(Idled, _Idled);
public: DEF_EVENT_ARGS(Closing, _Closing, PanelEventArgs);
public: DEF_EVENT_ARGS(DeletingFiles, _DeletingFiles, FilesEventArgs);
public: DEF_EVENT_ARGS(Executing, _Executing, ExecutingEventArgs);
public: DEF_EVENT_ARGS(GettingData, _GettingData, PanelEventArgs);
public: DEF_EVENT_ARGS(GettingFiles, _GettingFiles, GettingFilesEventArgs);
public: DEF_EVENT_ARGS(KeyPressed, _KeyPressed, PanelKeyEventArgs);
public: DEF_EVENT_ARGS(MakingDirectory, _MakingDirectory, MakingDirectoryEventArgs);
public: DEF_EVENT_ARGS(PuttingFiles, _PuttingFiles, FilesEventArgs);
public: DEF_EVENT_ARGS(Redrawing, _Redrawing, PanelEventArgs);
public: DEF_EVENT_ARGS(SettingDirectory, _SettingDirectory, SettingDirectoryEventArgs);
public: DEF_EVENT_ARGS(ViewModeChanged, _ViewModeChanged, ViewModeChangedEventArgs);
internal:
	int _id;
private:
	FarPanelPluginInfo _info;
	List<IFile^> _files;
};

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

void Far::AsGetPluginInfo(PluginInfo* pi)
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

HANDLE Far::AddPlugin(FarPanelPlugin^ plugin)
{
	if (plugin->Id > 0)
		return (HANDLE)plugin->Id;

	int i = _plugins.LastIndexOf(nullptr);
	if (i > 0)
	{
		plugin->_id = i;
		_plugins[i] = plugin;
		return (HANDLE)i;
	}

	if (_plugins.Count == 0)
		_plugins.Add(nullptr);
	plugin->_id = _plugins.Count; // before Add()!
	_plugins.Add(plugin);
	return (HANDLE)plugin->_id;
}

IPanelPlugin^ Far::CreatePanelPlugin(bool open)
{
	FarPanelPlugin^ r = gcnew FarPanelPlugin();
	if (open)
		_pluginToOpen = r;
	return r;
}

void Far::OpenPanelPlugin(IPanelPlugin^ plugin)
{
	_pluginToOpen = plugin;
}

IFile^ Far::CreatePanelItem()
{
	return gcnew StoredFile();
}

IFile^ Far::CreatePanelItem(FileSystemInfo^ info, bool fullName)
{
	StoredFile^ r = gcnew StoredFile();
	if (info)
	{
		r->Name = fullName ? info->FullName : info->Name;
		r->CreationTime = info->CreationTime;
		r->LastAccessTime = info->LastAccessTime;
		r->LastWriteTime = info->LastWriteTime;
		r->SetAttributes(info->Attributes);
		if (!r->IsDirectory)
			r->Size = ((FileInfo^)info)->Length;
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

void Far::ClosePanel(String^ path)
{
	CStr sb;
	if (!String::IsNullOrEmpty(path))
		sb.Set(path);

	Info.Control(INVALID_HANDLE_VALUE, FCTL_CLOSEPLUGIN, sb);
}

HANDLE Far::AsOpenPlugin(int from, int item)
{
	_pluginToOpen = nullptr;
	if (from == OPEN_COMMANDLINE)
	{
		ProcessPrefixes(item);
	}
	else
	{
		if (from == OPEN_PLUGINSMENU || from == OPEN_EDITOR || from == OPEN_VIEWER)
		{
			PluginMenuItem^ menuItem = (PluginMenuItem^)_registeredMenuItems[item];
			OpenPluginMenuItemEventArgs e((OpenFrom)from);
			menuItem->_OnOpen(menuItem, %e);
		}
	}
	if (_pluginToOpen)
	{
		HANDLE h = AddPlugin((FarPanelPlugin^)_pluginToOpen);
		_pluginToOpen = nullptr;
		return h;
	}
	return INVALID_HANDLE_VALUE;
}

void Far::AsClosePlugin(HANDLE hPlugin)
{
	FarPanelPlugin^ plugin = _plugins[(int)hPlugin];
	_plugins[(int)hPlugin] = nullptr;
	if (plugin->_Closed)
		plugin->_Closed(plugin, nullptr);
}

void Far::AsGetOpenPluginInfo(HANDLE hPlugin, OpenPluginInfo* info)
{
	FarPanelPlugin^ plugin = _plugins[(int)hPlugin];
	FarPanelPluginInfo^ info1 = (FarPanelPluginInfo^)plugin->Info;
	if (plugin->_GettingInfo)
		plugin->_GettingInfo(plugin, nullptr);
	*info = info1->Get();
}

static List<IFile^>^ ItemsToFiles(PluginPanelItem* panelItem, int itemsNumber)
{
	List<IFile^>^ r = gcnew List<IFile^>();
	r->Capacity = itemsNumber;
	for(int i = 0; i < itemsNumber; ++i)
		r->Add(gcnew StoredFile(panelItem[i]));
	return r;
}

int Far::AsDeleteFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int opMode)
{
	FarPanelPlugin^ plugin = _plugins[(int)hPlugin];
	if (!plugin->_DeletingFiles)
		return FALSE;
	List<IFile^>^ files = ItemsToFiles(panelItem, itemsNumber);
	FilesEventArgs e(files, (OperationModes)opMode, false);
	plugin->_DeletingFiles(plugin, %e);
	return e.Ignore ? FALSE : TRUE;
}

int Far::AsGetFindData(HANDLE hPlugin, PluginPanelItem** pPanelItem, int* pItemsNumber, int opMode)
{
	try
	{
		FarPanelPlugin^ plugin = _plugins[(int)hPlugin];
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
		(*pPanelItem) = new PluginPanelItem[(*pItemsNumber)];
		memset((*pPanelItem), 0, (*pItemsNumber)*sizeof(PluginPanelItem));
		int i = -1;
		for each(StoredFile^ f in plugin->Files)
		{
			++i;
			PluginPanelItem& p = (*pPanelItem)[i];
			WIN32_FIND_DATA& d = p.FindData;

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
			d.nFileSizeLow = (DWORD)(f->Size & 0xFFFFFFFF);
			d.nFileSizeHigh = (DWORD)(f->Size >> 32);
			d.ftCreationTime = dt2ft(f->CreationTime);
			d.ftLastAccessTime = dt2ft(f->LastAccessTime);
			d.ftLastWriteTime = dt2ft(f->LastWriteTime);
			p.UserData = f->Tag;

			if (!String::IsNullOrEmpty(f->Description))
			{
				p.Description = new char[f->Description->Length + 1];
				StrToOem(f->Description, p.Description);
			}
		}
		return TRUE;
	}
	catch(Exception^ e)
	{
		if ((opMode & (OPM_FIND|OPM_SILENT)) == 0)
			ShowError(__FUNCTION__, e);
		return FALSE;
	}
}

void Far::AsFreeFindData(PluginPanelItem* panelItem, int itemsNumber)
{
	for(int i = itemsNumber; --i >= 0;)
	{
		PluginPanelItem& p = panelItem[i];
		delete p.Description;
	}
	delete panelItem;
}

int Far::AsSetDirectory(HANDLE hPlugin, const char* dir, int opMode)
{
	FarPanelPlugin^ plugin = _plugins[(int)hPlugin];
	if (!plugin->_SettingDirectory)
		return TRUE;
	SettingDirectoryEventArgs e(OemToStr(dir), (OperationModes)opMode);
	plugin->_SettingDirectory(plugin, %e);
	return !e.Ignore;
}

int Far::AsProcessKey(HANDLE hPlugin, int key, unsigned int controlState)
{
	FarPanelPlugin^ plugin = _plugins[(int)hPlugin];
	if (!plugin->_KeyPressed)
		return FALSE;

	PanelKeyEventArgs e((key & ~PKF_PREPROCESS), (KeyStates)controlState, (key & PKF_PREPROCESS) != 0);
	plugin->_KeyPressed(plugin, %e);
	return e.Ignore;
}

int Far::AsProcessEvent(HANDLE hPlugin, int id, void* param)
{
	FarPanelPlugin^ plugin = _plugins[(int)hPlugin];
	switch(id)
	{
	case FE_CHANGEVIEWMODE:
		if (plugin->_ViewModeChanged)
		{
			ViewModeChangedEventArgs e(OemToStr((const char*)param));
			plugin->_ViewModeChanged(plugin, %e);
		}
		break;
	case FE_REDRAW:
		if (plugin->_Redrawing)
		{
			PanelEventArgs e(OperationModes::None);
			plugin->_Redrawing(plugin, %e);
			return e.Ignore;
		}
		break;
	case FE_IDLE:
		if (plugin->_Idled)
			plugin->_Idled(plugin, nullptr);
		break;
	case FE_CLOSE:
		if (plugin->_Closing)
		{
			PanelEventArgs e(OperationModes::None);
			plugin->_Closing(plugin, %e);
			return e.Ignore;
		}
		if (plugin->Info->ConfirmClose)
		{
			if (Msg("Close the panel?", plugin->Info->Title, MessageOptions::YesNo))
				return TRUE;
		}
		break;
	case FE_BREAK:
		if (plugin->_CtrlBreakPressed)
			plugin->_CtrlBreakPressed(plugin, nullptr);
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
	}
	return FALSE;
}

int Far::AsMakeDirectory(HANDLE hPlugin, char* name, int opMode)
{
	FarPanelPlugin^ plugin = _plugins[(int)hPlugin];
	if (!plugin->_MakingDirectory)
		return FALSE;
	MakingDirectoryEventArgs e(OemToStr(name), (OperationModes)opMode);
	plugin->_MakingDirectory(plugin, %e);
	return e.Ignore ? FALSE : TRUE;
}

int Far::AsGetFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, char* destPath, int opMode)
{
	FarPanelPlugin^ plugin = _plugins[(int)hPlugin];
	if (!plugin->_GettingFiles)
		return 0;
	List<IFile^>^ files = ItemsToFiles(panelItem, itemsNumber);
	GettingFilesEventArgs e(files, (OperationModes)opMode, move != 0, OemToStr(destPath));
	plugin->_GettingFiles(plugin, %e);
	return e.Ignore ? FALSE : TRUE;
}

int Far::AsPutFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, int opMode)
{
	FarPanelPlugin^ plugin = _plugins[(int)hPlugin];
	if (!plugin->_PuttingFiles)
		return 0;
	List<IFile^>^ files = ItemsToFiles(panelItem, itemsNumber);
	FilesEventArgs e(files, (OperationModes)opMode, move != 0);
	plugin->_PuttingFiles(plugin, %e);
	return e.Ignore ? FALSE : TRUE;
}

}
