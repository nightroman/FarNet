
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
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
#include "UI.h"
#include "Viewer0.h"
#include "Window.h"

namespace FarNet
{;
void Far1::Connect()
{
	// the instance
	Far::Net = %Far;

	// initialize data paths
	_LocalData = Environment::GetEnvironmentVariable("FARLOCALPROFILE");
	_RoamingData = Environment::GetEnvironmentVariable("FARPROFILE");
}

String^ Far1::CurrentDirectory::get()
{
	size_t size = Info.FSF->GetCurrentDirectory(0, 0);
	CBox buf(size);
	Info.FSF->GetCurrentDirectory(size, buf);
	return gcnew String(buf);
}

IModuleCommand^ Far1::GetModuleCommand(Guid id)
{
	IModuleAction^ action;
	if (!Works::Host::Actions->TryGetValue(id, action))
		return nullptr;

	return (IModuleCommand^)action;
}

IModuleTool^ Far1::GetModuleTool(Guid id)
{
	IModuleAction^ action;
	if (!Works::Host::Actions->TryGetValue(id, action))
		return nullptr;

	return (IModuleTool^)action;
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

FarConfirmations Far1::Confirmations::get()
{
	return (FarConfirmations)Info.AdvControl(&MainGuid, ACTL_GETCONFIRMATIONS, 0, 0);
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

void Far1::PostText(String^ text, bool enableOutput)
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
	PostMacro(keys.ToString(), enableOutput, false);
}

ILine^ Far1::Line::get()
{
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
			Far::Net->UI->Write(info, ConsoleColor::Red);
		else
			Far::Net->Message(info + Environment::NewLine + error->ToString(), title, (MessageOptions::Gui | MessageOptions::Warning));

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
	Works::EditorTools::EditText(info, error->GetType()->FullName, true);
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

void Far1::PostStep(Action^ handler)
{
	Far0::PostStep(handler);
}

void Far1::PostStepAfterKeys(String^ keys, Action^ handler)
{
	Far0::PostStepAfterKeys(keys, handler);
}

void Far1::PostStepAfterStep(Action^ handler1, Action^ handler2)
{
	Far0::PostStepAfterStep(handler1, handler2);
}

String^ Far1::TempName(String^ prefix)
{
	// reasonable buffer
	PIN_NE(pin, prefix);
	wchar_t buf[CBox::eBuf];
	size_t size = Info.FSF->MkTemp(buf, countof(buf), pin);
	if (size <= countof(buf))
		return gcnew String(buf);

	// larger buffer
	CBox box(size);
	Info.FSF->MkTemp(box, size, pin);
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

bool Far1::MatchPattern(String^ input, String^ pattern)
{
	if (!input) throw gcnew ArgumentNullException("input");
	
	// empty
	if (ES(pattern))
		return true;

	// regex
	if (pattern->StartsWith("/") && pattern->EndsWith("/"))
		return Regex::IsMatch(input, pattern->Substring(1, pattern->Length - 2), RegexOptions::IgnoreCase);

	// wildcard
	PIN_NE(pin, input);
	return Far0::CompareNameExclude(pattern, pin, false);
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

}
