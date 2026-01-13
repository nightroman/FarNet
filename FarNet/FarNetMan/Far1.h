#pragma once
#include "CommandLine.h"
#include "Dialog.h"
#include "Editor.h"
#include "Editor0.h"
#include "Far0.h"
#include "History.h"
#include "InputBox.h"
#include "Menu.h"
#include "Message.h"
#include "Panel0.h"
#include "UI.h"
#include "Viewer0.h"
#include "Window.h"

namespace FarNet
{
// Far::Api instance.
ref class Far1 sealed : IFar
{
public: DEF_EVENT_ARGS_IMP(DirectoryChanged, _DirectoryChanged, DirectoryChangedEventArgs);
public:
	virtual property FarNet::MacroArea MacroArea { FarNet::MacroArea get() override; }
	virtual property FarNet::MacroState MacroState { FarNet::MacroState get() override; }
	virtual property IAnyEditor^ AnyEditor{ IAnyEditor ^ get() override { return % Editor0::_anyEditor; } }
	virtual property IAnyViewer^ AnyViewer{ IAnyViewer ^ get() override { return % Viewer0::_anyViewer; } }
	virtual property IDialog^ Dialog{ IDialog ^ get() override { return FarDialog::GetDialog(0); } }
	virtual property IEditor^ Editor{ IEditor ^ get() override { return Editor0::GetCurrentEditor(); } }
	virtual property IHistory^ History{ IHistory ^ get() override { return % FarNet::History::Instance; } }
	virtual property ILine^ CommandLine{ ILine ^ get() override { return % FarNet::CommandLine::Instance; } }
	virtual property ILine^ Line { ILine^ get() override; }
	virtual property IPanel^ Panel { IPanel^ get() override { return Panel0::GetPanel(true); } }
	virtual property IPanel^ Panel2 { IPanel^ get() override { return Panel0::GetPanel(false); }}
	virtual property IUserInterface^ UI{ IUserInterface ^ get() override { return % FarUI::Instance; } }
	virtual property IViewer^ Viewer{ IViewer ^ get() override { return Viewer0::GetCurrentViewer(); } }
	virtual property IWindow^ Window{ IWindow ^ get() override { return % FarNet::Window::Instance; } }
	virtual property String^ CurrentDirectory { String^ get() override; }
	virtual property Version^ FarNetVersion{ System::Version ^ get() override { return Far1::typeid->Assembly->GetName()->Version; } }
	virtual property Version^ FarVersion { System::Version^ get() override; }
public:
	virtual IDialog^ CreateDialog(int left, int top, int right, int bottom) override { return gcnew FarDialog(left, top, right, bottom); }
	virtual IEditor^ CreateEditor() override { return gcnew FarNet::Editor; }
	virtual IInputBox^ CreateInputBox() override { return gcnew InputBox; }
	virtual IListMenu^ CreateListMenu() override { return gcnew Works::ListMenu; }
	virtual IMenu^ CreateMenu() override { return gcnew Menu; }
	virtual IModuleManager^ GetModuleManager(String^ name) override { return Works::ModuleLoader::GetModuleManager(name); }
	virtual IViewer^ CreateViewer() override { return gcnew FarNet::Viewer; }
	virtual String^ GetFolderPath(SpecialFolder folder) override;
public:
	virtual array<FarNet::Panel^>^ Panels(Guid typeId) override { return Panel0::PanelsByGuid(typeId); }
	virtual array<FarNet::Panel^>^ Panels(Type^ type) override { return Panel0::PanelsByType(type); }
	virtual array<IEditor^>^ Editors() override { return Editor0::Editors(); }
	virtual array<IViewer^>^ Viewers() override { return Viewer0::Viewers(); }
	virtual bool IsMaskMatch(String^ path, String^ mask, bool full) override;
	virtual bool IsMaskValid(String^ mask) override;
	virtual CultureInfo^ GetCurrentUICulture(bool update) override { return Far0::GetCurrentUICulture(update); }
	virtual IModuleAction^ GetModuleAction(Guid id) override;
	virtual int Message(MessageArgs^ args) override { return Message::Show(args); }
	virtual KeyInfo^ NameToKeyInfo(String^ key) override;
	virtual Object^ GetSetting(FarSetting settingSet, String^ settingName) override;
	virtual String^ Input(String^ prompt, String^ history, String^ title, String^ text) override;
	virtual String^ KeyInfoToName(KeyInfo^ key) override;
	virtual String^ PasteFromClipboard() override;
	virtual String^ TempName(String^ prefix) override;
	virtual void CopyToClipboard(String^ text) override;
	virtual void InvokeCommand(String^ command) override;
	virtual void PostJob(Action^ handler) override { Far0::PostJob(handler); }
	virtual Task^ PostJobAsync(Action^ handler) override { return Far0::PostJobAsync(handler); }
	virtual void PostMacro(String^ macro, bool enableOutput, bool disablePlugins) override;
	virtual void PostStep(Action^ step) override { Far0::PostStep(step); }
	virtual void Quit() override;
	virtual void ShowError(String^ title, Exception^ error) override;
	virtual void ShowHelp(String^ path, String^ topic, HelpOptions options) override;
internal:
	static Far1 Instance;
private:
	Far1() {}
};
}
