
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

#include "stdafx.h"
#include "Far1.h"
#include "CommandLine.h"
#include "Dialog.h"
#include "Editor0.h"
#include "Far0.h"
#include "History.h"
#include "InputBox.h"
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

int Far1::Message(MessageArgs^ args)
{
	return Message::Show(args);
}

System::Version^ Far1::FarVersion::get()
{
	VersionInfo vi;
	Info.AdvControl(&MainGuid, ACTL_GETFARMANAGERVERSION, 0, &vi);
	return gcnew System::Version(vi.Major, vi.Minor, vi.Build, vi.Revision);
}

System::Version^ Far1::FarNetVersion::get()
{
	return Far1::typeid->Assembly->GetName()->Version;
}

IMenu^ Far1::CreateMenu()
{
	return gcnew Menu;
}

IListMenu^ Far1::CreateListMenu()
{
	return gcnew Works::ListMenu;
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

	// stop running macros
	String^ msgMacro = nullptr;
	FarNet::MacroState macro = MacroState;
	if (macro == FarNet::MacroState::Executing || macro == FarNet::MacroState::ExecutingCommon)
	{
		// log
		msgMacro = "A macro was stopped.";
		Log::Source->TraceEvent(TraceEventType::Warning, 0, msgMacro);

		// stop
		UI->Break();
	}

	// unwrap
	error = Works::Kit::UnwrapAggregateException(error);

	// quietly ignore PowerShell stopped pipelines, they are like cancels
	if (error->GetType()->FullName == "System.Management.Automation.PipelineStoppedException") //_110128_075844
		return;

	// trace
	Log::TraceException(error);

	// case: loaded
	if (g_AppState == AppState::Loaded)
	{
		Works::ErrorDialog::Show(title, error, msgMacro);
		return;
	}

	//! do not UI on loading
	//! cannot UI on unloading

	//! trace full, so users can report
	auto errorString = error->ToString();
	Log::TraceError(errorString);

	auto text = gcnew StringWriter;
	Log::FormatException(text, error);
	text->WriteLine(title);

	// on loading print errors
	if (g_AppState == AppState::Loading)
	{
		Far::Api->UI->Write(text->ToString(), ConsoleColor::Red);
	}
	else
	{
		text->Write(errorString);
		Far::Api->Message(text->ToString(), title, (MessageOptions::Gui | MessageOptions::Warning));
	}
}

IDialog^ Far1::CreateDialog(int left, int top, int right, int bottom)
{
	return gcnew FarDialog(left, top, right, bottom);
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

void Far1::PostStep(Action^ step)
{
	Far0::PostStep(step);
}

String^ Far1::TempName(String^ prefix)
{
	PIN_NE(pin, prefix);

	CBox box;
	while(box(Info.FSF->MkTemp(box, box.Size(), pin))) {}

	return gcnew String(box);
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

	arg.Flags |= KMFLAGS_SILENTCHECK;
	if (enableOutput)
		arg.Flags |= KMFLAGS_ENABLEOUTPUT;
	if (disablePlugins)
		arg.Flags |= KMFLAGS_NOSENDKEYSTOPLUGINS;

	if (Info.MacroControl(&MainGuid, MCTL_SENDSTRING, MSSC_POST, &arg))
		return;

	auto size = Info.MacroControl(&MainGuid, MCTL_GETLASTERROR, 0, 0);
	auto data = std::make_unique<char[]>(size);
	auto arg2 = (MacroParseResult*)data.get();
	arg2->StructSize = sizeof(MacroParseResult);
	Info.MacroControl(&MainGuid, MCTL_GETLASTERROR, size, arg2);
	String^ err = String::Format(
		"Error message: {0}\n"
		"Position: {1}, {2}\n"
		"Macro: {3}",
		gcnew String(arg2->ErrSrc),
		arg2->ErrPos.Y,
		arg2->ErrPos.X,
		macro);
	throw gcnew ArgumentException(err, "macro");
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
	switch (folder)
	{
	case SpecialFolder::LocalData:
		return Environment::GetEnvironmentVariable("FARLOCALPROFILE");
	case SpecialFolder::RoamingData:
		return Environment::GetEnvironmentVariable("FARPROFILE");
	default:
		throw gcnew ArgumentException("folder");
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

IHistory^ Far1::History::get()
{
	return %FarNet::History::Instance;
}

Object^ Far1::GetSetting(FarSetting settingSet, String^ settingName)
{
	Settings settings(FarGuid);

	FarSettingsItem arg;
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
