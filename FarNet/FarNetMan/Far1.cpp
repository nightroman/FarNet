/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "Far1.h"
#include "CommandLine.h"
#include "Editor0.h"
#include "Far0.h"
#include "InputBox.h"
#include "ListMenu.h"
#include "Macro.h"
#include "Menu.h"
#include "Message.h"
#include "ModuleLoader.h"
#include "ModuleManager.h"
#include "ModuleProxy.h"
#include "Panel0.h"
#include "Panel2.h"
#include "Shelve.h"
#include "SubsetForm.h"
#include "Viewer0.h"
#include "Zoo.h"

namespace FarNet
{;
void Far1::Connect()
{
	Far::Net = %Far;
}

String^ Far1::ActivePath::get()
{
	DWORD size = Info.FSF->GetCurrentDirectory(0, 0);
	CBox buf(size);
	Info.FSF->GetCurrentDirectory(size, buf);
	return gcnew String(buf);
}

String^ Far1::RegistryFarPath::get()
{
	String^ key = RegistryPluginsPath;
	return key->Substring(0, key->LastIndexOf('\\'));
}

String^ Far1::RegistryPluginsPath::get()
{
	return gcnew String(Info.RootKey);
}

IModuleCommand^ Far1::GetModuleCommand(Guid id)
{
	ProxyAction^ action;
	if (!ModuleLoader::Actions->TryGetValue(id, action))
		return nullptr;

	return (ProxyCommand^)action;
}

IModuleFiler^ Far1::GetModuleFiler(Guid id)
{
	ProxyAction^ action;
	if (!ModuleLoader::Actions->TryGetValue(id, action))
		return nullptr;

	return (ProxyFiler^)action;
}

IModuleTool^ Far1::GetModuleTool(Guid id)
{
	ProxyAction^ action;
	if (!ModuleLoader::Actions->TryGetValue(id, action))
		return nullptr;

	return (ProxyTool^)action;
}

void Far1::Message(String^ body)
{
	Message::Show(body, nullptr, MsgOptions::Ok, nullptr, nullptr);
}

void Far1::Message(String^ body, String^ header)
{
	Message::Show(body, header, MsgOptions::Ok, nullptr, nullptr);
}

int Far1::Message(String^ body, String^ header, MsgOptions options)
{
	return Message::Show(body, header, options, nullptr, nullptr);
}

int Far1::Message(String^ body, String^ header, MsgOptions options, array<String^>^ buttons)
{
	return Message::Show(body, header, options, buttons, nullptr);
}

int Far1::Message(String^ body, String^ header, MsgOptions options, array<String^>^ buttons, String^ helpTopic)
{
	return Message::Show(body, header, options, buttons, helpTopic);
}

void Far1::Run(String^ command)
{
	Far0::Run(command);
}

IntPtr Far1::MainWindowHandle::get()
{
	return (IntPtr)Info.AdvControl(Info.ModuleNumber, ACTL_GETFARHWND, nullptr);
}

System::Version^ Far1::FarVersion::get()
{
	DWORD vn;
	Info.AdvControl(Info.ModuleNumber, ACTL_GETFARVERSION, &vn);
	return gcnew System::Version((vn&0x0000ff00)>>8, vn&0x000000ff, (int)((long)vn&0xffff0000)>>16);
}

System::Version^ Far1::FarNetVersion::get()
{
	return Assembly::GetExecutingAssembly()->GetName()->Version;
}

IMenu^ Far1::CreateMenu()
{
	return gcnew Menu;
}

IListMenu^ Far1::CreateListMenu()
{
	return gcnew ListMenu;
}

FarConfirmations Far1::Confirmations::get()
{
	return (FarConfirmations)Info.AdvControl(Info.ModuleNumber, ACTL_GETCONFIRMATIONS, 0);
}

FarMacroState Far1::MacroState::get()
{
	ActlKeyMacro command;
	command.Command = MCMD_GETSTATE;
	return (FarMacroState)Info.AdvControl(Info.ModuleNumber, ACTL_KEYMACRO, &command);
}

array<IEditor^>^ Far1::Editors()
{
	return Editor0::Editors();
}

array<IViewer^>^ Far1::Viewers()
{
	return Viewer0::Viewers();
}

IAnyEditor^ Far1::AnyEditor::get()
{
	return %Editor0::_anyEditor;
}

IAnyViewer^ Far1::AnyViewer::get()
{
	return %Viewer0::_anyViewer;
}

String^ Far1::PasteFromClipboard()
{
	wchar_t* buffer = Info.FSF->PasteFromClipboard();
	String^ r = gcnew String(buffer);
	Info.FSF->DeleteBuffer(buffer);
	return r;
}

void Far1::CopyToClipboard(String^ text)
{
	PIN_NE(pin, text);
	Info.FSF->CopyToClipboard(pin);
}

IEditor^ Far1::CreateEditor()
{
	return gcnew FarNet::Editor;
}

IViewer^ Far1::CreateViewer()
{
	return gcnew FarNet::Viewer;
}

array<int>^ Far1::CreateKeySequence(String^ keys)
{
	if (!keys)
		throw gcnew ArgumentNullException("keys");
	
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

void Far1::PostKeySequence(array<int>^ sequence)
{
	PostKeySequence(sequence, true);
}

//! [_090328_170110] KSFLAGS_NOSENDKEYSTOPLUGINS is not set,
//! but Tab for TabExpansion is not working in .ps1 editor, why?
void Far1::PostKeySequence(array<int>^ sequence, bool disableOutput)
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

	DWORD* cur = keySequence.Count <= smallCount ? keys : new DWORD[keySequence.Count];
	keySequence.Sequence = cur;
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

// Don't throw on a wrong key, it is used for validation.
// See also:
// About AltXXXXX and etc.: http://forum.farmanager.com/viewtopic.php?f=8&t=5058
int Far1::NameToKey(String^ key)
{
	if (!key)
		throw gcnew ArgumentNullException("key");

	PIN_NE(pin, key);
	return Info.FSF->FarNameToKey(pin);
}

String^ Far1::KeyToName(int key)
{
	wchar_t name[33];
	if (!Info.FSF->FarKeyToName(key, name, countof(name) - 1))
		return nullptr;

	return gcnew String(name);
}

void Far1::PostKeys(String^ keys)
{
	PostKeys(keys, true);
}

void Far1::PostKeys(String^ keys, bool disableOutput)
{
	if (keys == nullptr)
		throw gcnew ArgumentNullException("keys");

	keys = keys->Trim();
	PostKeySequence(CreateKeySequence(keys), disableOutput);
}

void Far1::PostText(String^ text)
{
	PostText(text, true);
}

void Far1::PostText(String^ text, bool disableOutput)
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

int Far1::SaveScreen(int x1, int y1, int x2, int y2)
{
	return (int)(INT_PTR)Info.SaveScreen(x1, y1, x2, y2);
}

void Far1::RestoreScreen(int screen)
{
	Info.RestoreScreen((HANDLE)(INT_PTR)screen);
}

ILine^ Far1::Line::get()
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

IEditor^ Far1::Editor::get()
{
	return Editor0::GetCurrentEditor();
}

IViewer^ Far1::Viewer::get()
{
	return Viewer0::GetCurrentViewer();
}

IAnyPanel^ Far1::Panel::get()
{
	return Panel0::GetPanel(true);
}

IAnyPanel^ Far1::Panel2::get()
{
	return Panel0::GetPanel(false);
}

IInputBox^ Far1::CreateInputBox()
{
	return gcnew InputBox;
}

void Far1::GetUserScreen()
{
	Info.Control(INVALID_HANDLE_VALUE, FCTL_GETUSERSCREEN, 0, 0);
}

void Far1::SetUserScreen()
{
	Info.Control(INVALID_HANDLE_VALUE, FCTL_SETUSERSCREEN, 0, 0);
}

ICollection<String^>^ Far1::GetDialogHistory(String^ name)
{
	return GetHistory("SavedDialogHistory\\" + name, nullptr);
}

ICollection<String^>^ Far1::GetHistory(String^ name)
{
	return GetHistory(name, nullptr);
}

//! Hack, not API.
// Avoid exceptions, return what we can get.
ICollection<String^>^ Far1::GetHistory(String^ name, String^ filter)
{
	List<String^>^ r = gcnew List<String^>;

	String^ keyName = RegistryFarPath + "\\" + name;
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

void Far1::ShowError(String^ title, Exception^ error)
{
	// 091028 Do not throw on null, just ignore.
	if (!error)
		return;

	// stop a running macro
	String^ msgMacro = nullptr;
	FarMacroState macro = MacroState;
	if (macro == FarMacroState::Executing || macro == FarMacroState::ExecutingCommon)
	{
		// log
		msgMacro = "A macro has been stopped.";
		Log::TraceWarning(msgMacro);

		// stop
		Zoo->Break();
	}

	// log
	String^ info = Log::TraceException(error);

	// ask
	int res = Message(
		error->Message,
		String::IsNullOrEmpty(title) ? error->GetType()->FullName : title,
		MsgOptions::LeftAligned | MsgOptions::Warning,
		gcnew array<String^>{"Ok", "View info", "Copy info"});
	if (res < 1)
		return;

	// info to show
	if (!info)
		info = Log::FormatException(error);

	// add macro info
	if (msgMacro)
		info += "\r\n" + msgMacro + "\r\n";

	// add verbose information
	info += "\r\n" + error->ToString();

	// show or clip
	if (res == 1)
	{
		Far.AnyViewer->ViewText(
			info,
			error->GetType()->FullName,
			OpenMode::Modal);
	}
	else
	{
		CopyToClipboard(info);
	}
}

IDialog^ Far1::CreateDialog(int left, int top, int right, int bottom)
{
	return gcnew FarDialog(left, top, right, bottom);
}

void Far1::WritePalette(int left, int top, PaletteColor paletteColor, String^ text)
{
	PIN_NE(pin, text);
	Info.Text(left, top, Far0::GetPaletteColor(paletteColor), pin);
}

void Far1::WriteText(int left, int top, ConsoleColor foregroundColor, ConsoleColor backgroundColor, String^ text)
{
	PIN_NE(pin, text);
	Info.Text(left, top, int(foregroundColor)|(int(backgroundColor)<<4), pin);
}

void Far1::ShowHelp(String^ path, String^ topic, HelpOptions options)
{
	PIN_NE(pinPath, path);
	PIN_NS(pinTopic, topic);

	Info.ShowHelp(pinPath, pinTopic, (int)options);
}

//! Console::Write writes some Unicode chars as '?'.
void Far1::Write(String^ text)
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

void Far1::Write(String^ text, ConsoleColor foregroundColor)
{
	ConsoleColor fc = Console::ForegroundColor;
	Console::ForegroundColor = foregroundColor;
	Write(text);
	Console::ForegroundColor = fc;
}

void Far1::Write(String^ text, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
{
	ConsoleColor fc = Console::ForegroundColor;
	ConsoleColor bc = Console::BackgroundColor;
	Console::ForegroundColor = foregroundColor;
	Console::BackgroundColor = backgroundColor;
	Write(text);
	Console::ForegroundColor = fc;
	Console::BackgroundColor = bc;
}

IPanel^ Far1::CreatePanel()
{
	return gcnew FarNet::Panel2;
}

IPanel^ Far1::FindPanel(Guid typeId)
{
	return Panel0::GetPanel(typeId);
}

IPanel^ Far1::FindPanel(Type^ hostType)
{
	return Panel0::GetPanel(hostType);
}

String^ Far1::Input(String^ prompt)
{
	return Input(prompt, nullptr, nullptr, String::Empty);
}

String^ Far1::Input(String^ prompt, String^ history)
{
	return Input(prompt, history, nullptr, String::Empty);
}

String^ Far1::Input(String^ prompt, String^ history, String^ title)
{
	return Input(prompt, history, title, String::Empty);
}

String^ Far1::Input(String^ prompt, String^ history, String^ title, String^ text)
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

int Far1::WindowCount::get()
{
	return (int)Info.AdvControl(Info.ModuleNumber, ACTL_GETWINDOWCOUNT, 0);
}

IWindowInfo^ Far1::GetWindowInfo(int index, bool full)
{
	return gcnew FarWindowInfo(index, full);
}

WindowType Far1::WindowType::get()
{
	WindowInfo wi;
	wi.Pos = -1;
	return Info.AdvControl(Info.ModuleNumber, ACTL_GETSHORTWINDOWINFO, &wi) ? (FarNet::WindowType)wi.Type : FarNet::WindowType::None;
}

WindowType Far1::GetWindowType(int index)
{
	WindowInfo wi;
	wi.Pos = index;
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_GETSHORTWINDOWINFO, &wi))
		throw gcnew InvalidOperationException("GetWindowType:" + index + " failed.");
	return (FarNet::WindowType)wi.Type;
}

void Far1::SetCurrentWindow(int index)
{
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_SETCURRENTWINDOW, (void*)(INT_PTR)index))
		throw gcnew InvalidOperationException("SetCurrentWindow:" + index + " failed.");
}

bool Far1::Commit()
{
	return Info.AdvControl(Info.ModuleNumber, ACTL_COMMIT, 0) != 0;
}

Char Far1::CodeToChar(int code)
{
	// get just the code
	code &= KeyMode::CodeMask;

	// not char
	if (code > 0xFFFF)
		return 0;

	// convert
	return Char(code);
}

void Far1::ShowPanelMenu(bool showPushCommand) //???? do we need it public?
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
			IAnyPanel^ panel = Panel;
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
			Panel1^ panel = (Panel1^)data;
			
			//?? native plugin panel: go to the first item to work around "Far does not restore panel state",
			// this does not restore either but is still better than unexpected current item after exit.
			if (nullptr == dynamic_cast<FarNet::Panel2^>(panel))
				panel->Redraw(0, 0);
			
			((Panel1^)data)->Close();
			return;
		}

		// Pop/Unshelve
		ShelveInfo^ shelve = (ShelveInfo^)data;
		shelve->Unshelve();

		return;
	}
}

void Far1::PostStep(EventHandler^ handler)
{
	Far0::PostStep(handler);
}

void Far1::PostStepAfterKeys(String^ keys, EventHandler^ handler)
{
	Far0::PostStepAfterKeys(keys, handler);
}

void Far1::PostStepAfterStep(EventHandler^ handler1, EventHandler^ handler2)
{
	Far0::PostStepAfterStep(handler1, handler2);
}

void Far1::Redraw()
{
	Info.AdvControl(Info.ModuleNumber, ACTL_REDRAWALL, 0);
}

String^ Far1::TempName(String^ prefix)
{
	// reasonable buffer
	PIN_NE(pin, prefix);
	wchar_t buf[CBox::eBuf];
	int size = Info.FSF->MkTemp(buf, countof(buf), pin);
	if (size <= countof(buf))
		return gcnew String(buf);

	// larger buffer
	CBox box(size);
	Info.FSF->MkTemp(box, size, pin);
	return gcnew String(box);
}

String^ Far1::TempFolder(String^ prefix)
{
	String^ r = TempName(prefix);
	Directory::CreateDirectory(r);
	return r;
}

IDialog^ Far1::Dialog::get()
{
	return FarDialog::GetDialog();
}

ConsoleColor Far1::GetPaletteBackground(PaletteColor paletteColor)
{
	int color = Far0::GetPaletteColor(paletteColor);
	return ConsoleColor(color >> 4);
}

ConsoleColor Far1::GetPaletteForeground(PaletteColor paletteColor)
{
	int color = Far0::GetPaletteColor(paletteColor);
	return ConsoleColor(color & 0xF);
}

void Far1::PostJob(EventHandler^ handler)
{
	Far0::PostJob(handler);
}

void Far1::SetProgressState(TaskbarProgressBarState state)
{
	Info.AdvControl(Info.ModuleNumber, ACTL_SETPROGRESSSTATE, (void*)(INT_PTR)state);
}

void Far1::SetProgressValue(int currentValue, int maximumValue)
{
	PROGRESSVALUE arg;
	arg.Completed = currentValue;
	arg.Total = maximumValue;
	Info.AdvControl(Info.ModuleNumber, ACTL_SETPROGRESSVALUE, &arg);
}

CultureInfo^ Far1::GetCurrentUICulture(bool update)
{
	return Far0::GetCurrentUICulture(update);
}

void Far1::PostMacro(String^ macro)
{
	PostMacro(macro, false, false);
}

void Far1::PostMacro(String^ macro, bool enableOutput, bool disablePlugins)
{
	if (!macro)
		throw gcnew ArgumentNullException("macro");

	PIN_NE(pin, macro);
	ActlKeyMacro command;
	command.Command = MCMD_POSTMACROSTRING;
	command.Param.PlainText.SequenceText = (wchar_t*)pin;
	command.Param.PlainText.Flags = 0;
	if (!enableOutput)
		command.Param.PlainText.Flags |= KSFLAGS_DISABLEOUTPUT;
	if (disablePlugins)
		command.Param.PlainText.Flags |= KSFLAGS_NOSENDKEYSTOPLUGINS;
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_KEYMACRO, &command))
		throw gcnew OperationCanceledException(__FUNCTION__ " failed.");
}

void Far1::Quit()
{
	if (!ModuleLoader::CanExit())
		return;
	
	Info.AdvControl(Info.ModuleNumber, ACTL_QUIT, 0);
}

IZoo^ Far1::Zoo::get()
{
	return gcnew FarNet::Zoo;
}

ISubsetForm^ Far1::CreateSubsetForm()
{
	return gcnew FarSubsetForm();
}

IMacro^ Far1::Macro::get()
{
	return gcnew Macro0;
}

ILine^ Far1::CommandLine::get()
{
	return gcnew FarNet::CommandLine;
}

}
