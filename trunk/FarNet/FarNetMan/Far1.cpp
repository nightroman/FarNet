
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

#include "StdAfx.h"
#include "Far1.h"
#include "CommandLine.h"
#include "Dialog.h"
#include "Editor0.h"
#include "Far0.h"
#include "History.h"
#include "InputBox.h"
#include "ListMenu.h"
#include "Menu.h"
#include "Message.h"
#include "Panel0.h"
#include "Panel2.h"
#include "Settings.h"
#include "UI.h"
#include "Viewer0.h"
#include "Window.h"

namespace FarNet
{;
void Far1::Connect()
{
	// the instance
	Far::Api = %Far;

	// initialize data paths
	_LocalData = Environment::GetEnvironmentVariable("FARLOCALPROFILE");
	_RoamingData = Environment::GetEnvironmentVariable("FARPROFILE");
}

String^ Far1::CurrentDirectory::get()
{
	CBox box;
	while(box(Info.FSF->GetCurrentDirectory(box.Size(), box))) {}

	return gcnew String(box);
}

IModuleAction^ Far1::GetModuleAction(Guid id)
{
	IModuleAction^ action;
	if (!Works::Host::Actions->TryGetValue(id, action))
		return nullptr;

	return action;
}

int Far1::Message(String^ body, String^ header, MessageOptions options, array<String^>^ buttons, String^ helpTopic)
{
	return Message::Show(body, header, options, buttons, helpTopic);
}

System::Version^ Far1::FarVersion::get()
{
	VersionInfo vi;
	Info.AdvControl(&MainGuid, ACTL_GETFARMANAGERVERSION, 0, &vi);
	return gcnew System::Version(vi.Major, vi.Minor, vi.Build, vi.Revision);
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

FarNet::MacroArea Far1::MacroArea::get()
{
	return (FarNet::MacroArea)(1 + Info.MacroControl(&MainGuid, MCTL_GETAREA, 0, 0));
}

FarNet::MacroState Far1::MacroState::get()
{
	return (FarNet::MacroState)Info.MacroControl(&MainGuid, MCTL_GETSTATE, 0, 0);
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
	CBox box(Info.FSF->PasteFromClipboard(FCT_ANY, 0, 0));
	if (box.Size() > 0)
		Info.FSF->PasteFromClipboard(FCT_ANY, box, box.Size());

	return gcnew String(box);
}

void Far1::CopyToClipboard(String^ text)
{
	PIN_NE(pin, text);
	Info.FSF->CopyToClipboard(FCT_STREAM, pin);
}

IEditor^ Far1::CreateEditor()
{
	return gcnew FarNet::Editor;
}

IViewer^ Far1::CreateViewer()
{
	return gcnew FarNet::Viewer;
}

KeyInfo^ Far1::NameToKeyInfo(String^ key)
{
	if (!key)
		throw gcnew ArgumentNullException("key");

	PIN_NE(pin, key);
	INPUT_RECORD ir;
	if (!Info.FSF->FarNameToInputRecord(pin, &ir) || ir.EventType != KEY_EVENT)
		return nullptr;

	return KeyInfoFromInputRecord(ir);
}

String^ Far1::KeyInfoToName(KeyInfo^ key)
{
	INPUT_RECORD ir;
	memset(&ir, 0, sizeof(ir));
	
	ir.EventType = KEY_EVENT;
	ir.Event.KeyEvent.wVirtualKeyCode = (WORD)key->VirtualKeyCode;
	ir.Event.KeyEvent.uChar.UnicodeChar = (WCHAR)key->Character;
	ir.Event.KeyEvent.dwControlKeyState = (DWORD)key->ControlKeyState;
	ir.Event.KeyEvent.bKeyDown = key->KeyDown;
	ir.Event.KeyEvent.wRepeatCount = 1;
	
	const size_t size = 100;
	wchar_t name[size] = {0};
	if (!Info.FSF->FarInputRecordToName(&ir, name, size))
		return nullptr;

	return gcnew String(name);
}

ILine^ Far1::Line::get()
{
	IDialog^ dialog = nullptr;
	switch (Window->Kind)
	{
	case FarNet::WindowKind::Editor:
		{
			IEditor^ editor = Editor;
			return editor->Line;
		}
	case FarNet::WindowKind::Panels:
		{
			return CommandLine;
		}
	case FarNet::WindowKind::Dialog:
		{
			dialog = Dialog;
		}
		break;
	case FarNet::WindowKind::Menu:
		{
			FarNet::MacroArea area = Far::Api->MacroArea;
			if (area == FarNet::MacroArea::ShellAutoCompletion)
				return CommandLine;

			if (area == FarNet::MacroArea::DialogAutoCompletion)
				dialog = Dialog;
		}
		break;
	}

	if (dialog)
	{
		IControl^ control = dialog->Focused;
		IEdit^ edit = dynamic_cast<IEdit^>(control);
		if (edit)
			return edit->Line;
		IComboBox^ combo = dynamic_cast<IComboBox^>(control);
		if (combo)
			return combo->Line;
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

IPanel^ Far1::Panel::get()
{
	return Panel0::GetPanel(true);
}

IPanel^ Far1::Panel2::get()
{
	return Panel0::GetPanel(false);
}

IInputBox^ Far1::CreateInputBox()
{
	return gcnew InputBox;
}

void Far1::ShowError(String^ title, Exception^ error)
{
	// 091028 Do not throw on null, just ignore.
	if (!error)
		return;

	// stop a running macro
	String^ msgMacro = nullptr;
	FarNet::MacroState macro = MacroState;
	if (macro == FarNet::MacroState::Executing || macro == FarNet::MacroState::ExecutingCommon)
	{
		// log
		msgMacro = "A macro has been stopped.";
		Log::Source->TraceEvent(TraceEventType::Warning, 0, msgMacro);

		// stop
		UI->Break();
	}

	// log
	String^ info = Log::TraceException(error);

	// case: not loaded
	//! non-stop loading
	//! no UI on unloading
	if (Works::Host::State != Works::HostState::Loaded)
	{
		//! trace the full string, so that a user can report this
		Log::TraceError(error->ToString());

		// info to show
		if (!info)
			info = Log::FormatException(error);

		// with title
		info += title + Environment::NewLine;

		if (Works::Host::State == Works::HostState::Loading)
			Far::Api->UI->Write(info, ConsoleColor::Red);
		else
			Far::Api->Message(info + Environment::NewLine + error->ToString(), title, (MessageOptions::Gui | MessageOptions::Warning));

		return;
	}

	// quiet: CtrlBreak in a dialog
	if (error->GetType()->FullName == "System.Management.Automation.PipelineStoppedException") //_110128_075844
		return;

	// ask
	int res = Message(
		error->Message,
		String::IsNullOrEmpty(title) ? error->GetType()->FullName : title,
		MessageOptions::LeftAligned | MessageOptions::Warning,
		gcnew array<String^>{"Ok", "More"});
	if (res < 1)
		return;

	// info to show
	if (!info)
		info = Log::FormatException(error);

	// add macro info
	if (msgMacro)
		info += Environment::NewLine + msgMacro + Environment::NewLine;

	// add verbose information
	info += Environment::NewLine + error->ToString();

	// locked editor
	EditTextArgs args;
	args.Text = info;
	args.Title = error->GetType()->FullName;
	args.IsLocked = true;
	Works::EditorTools::EditText(%args);
}

IDialog^ Far1::CreateDialog(int left, int top, int right, int bottom)
{
	return gcnew FarDialog(left, top, right, bottom);
}

Works::IPanelWorks^ Far1::WorksPanel(FarNet::Panel^ panel, Explorer^ explorer)
{
	return gcnew FarNet::Panel2(panel, explorer);
}

array<Panel^>^ Far1::Panels(Guid typeId)
{
	return Panel0::PanelsByGuid(typeId);
}

array<Panel^>^ Far1::Panels(Type^ type)
{
	return Panel0::PanelsByType(type);
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

void Far1::PostSteps(IEnumerable<Object^>^ steps)
{
	Far0::PostSteps(steps);
}

String^ Far1::TempName(String^ prefix)
{
	PIN_NE(pin, prefix);

	CBox box;
	while(box(Info.FSF->MkTemp(box, box.Size(), pin))) {}
	
	return gcnew String(box);
}

String^ Far1::TempFolder(String^ prefix)
{
	String^ dir = TempName(prefix);
	if (!Directory::Exists(dir))
		Directory::CreateDirectory(dir);
	return dir;
}

IDialog^ Far1::Dialog::get()
{
	return FarDialog::GetDialog();
}

void Far1::PostJob(Action^ handler)
{
	Far0::PostJob(handler);
}

CultureInfo^ Far1::GetCurrentUICulture(bool update)
{
	return Far0::GetCurrentUICulture(update);
}

void Far1::PostMacro(String^ macro, bool enableOutput, bool disablePlugins)
{
	if (!macro)
		throw gcnew ArgumentNullException("macro");

	PIN_NE(pin, macro);

	MacroSendMacroText arg;
	memset(&arg, 0, sizeof(arg));
	arg.StructSize = sizeof(arg);
	arg.SequenceText = pin;
	
	if (!enableOutput)
		arg.Flags |= KMFLAGS_DISABLEOUTPUT;
	if (disablePlugins)
		arg.Flags |= KMFLAGS_NOSENDKEYSTOPLUGINS;
	
	if (!Info.MacroControl(&MainGuid, MCTL_SENDSTRING, MSSC_POST, &arg))
		throw gcnew InvalidOperationException(__FUNCTION__ " failed.");
}

void Far1::Quit()
{
	if (!Works::ModuleLoader::CanExit())
		return;
	
	Info.AdvControl(&MainGuid, ACTL_QUIT, 0, 0);
}

ILine^ Far1::CommandLine::get()
{
	return gcnew FarNet::CommandLine;
}

IWindow^ Far1::Window::get()
{
	return %FarNet::Window::Instance;
}

// Implementation of Far methods.

IUserInterface^ Far1::UI::get()
{
	return %FarUI::Instance;
}

bool Far1::IsMaskMatch(String^ path, String^ mask)
{
	if (!path) throw gcnew ArgumentNullException("path");
	if (!mask) throw gcnew ArgumentNullException("mask");
	
	// match
	PIN_NE(pin, path);
	return Far0::MatchMask(mask, pin, true);
}

bool Far1::IsMaskValid(String^ mask)
{
	if (!mask) throw gcnew ArgumentNullException("mask");

	// check
	PIN_NE(pin, mask);
	return 0 != Info.FSF->ProcessName(pin, nullptr, 0, PN_CHECKMASK);
}

String^ Far1::GetFolderPath(SpecialFolder folder)
{
	switch(folder)
	{
	case SpecialFolder::LocalData: return _LocalData;
	case SpecialFolder::RoamingData: return _RoamingData;
	default: throw gcnew ArgumentException("folder");
	}
}

IModuleManager^ Far1::GetModuleManager(String^ name)
{
	return Works::ModuleLoader::GetModuleManager(name);
}

void Far1::ShowHelp(String^ path, String^ topic, HelpOptions options)
{
	PIN_NE(pinPath, path);
	PIN_NS(pinTopic, topic);

	Info.ShowHelp(pinPath, pinTopic, (int)options);
}

void Far1::ShowHelpTopic(String^ topic)
{
	String^ path = Path::GetDirectoryName(Assembly::GetCallingAssembly()->Location);

	PIN_NE(pinPath, path);
	PIN_NS(pinTopic, topic);

	Info.ShowHelp(pinPath, pinTopic, FHELP_CUSTOMPATH);
}

String^ Far1::GetHelpTopic(String^ topic)
{
	return "<" + Path::GetDirectoryName(Assembly::GetCallingAssembly()->Location) + "\\>" + topic;
}

IHistory^ Far1::History::get()
{
	return %FarNet::History::Instance;
}

Object^ Far1::GetSetting(FarSetting settingSet, String^ settingName)
{
	Settings settings(FarGuid);

	FarSettingsItem arg = {sizeof(arg)};
	if (!settings.Get((int)settingSet, settingName, arg))
		throw gcnew ArgumentException(String::Format("Cannot get setting: set = '{0}' name = '{1}'", settingSet, settingName));
	
	switch(arg.Type)
	{
	case FST_QWORD:
		return (System::Int64)arg.Number;
	case FST_STRING:
		return gcnew String(arg.String);
	case FST_DATA:
		array<Byte>^ arr = gcnew array<Byte>((int)arg.Data.Size);
		for(int i = arr->Length; --i >= 0;) arr[i] = ((char*)arg.Data.Data)[i];
		return arr;
	}
	return nullptr;
}

}
