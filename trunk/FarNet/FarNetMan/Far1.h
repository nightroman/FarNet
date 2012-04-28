
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class Far1 sealed : IFar
{
public:
	virtual property FarNet::MacroArea MacroArea { FarNet::MacroArea get() override; }
	virtual property FarNet::MacroState MacroState { FarNet::MacroState get() override; }
	virtual property IAnyEditor^ AnyEditor { IAnyEditor^ get() override; }
	virtual property IAnyViewer^ AnyViewer { IAnyViewer^ get() override; }
	virtual property IDialog^ Dialog { IDialog^ get() override; }
	virtual property IEditor^ Editor { IEditor^ get() override; }
	virtual property IHistory^ History { IHistory^ get() override; }
	virtual property ILine^ CommandLine { ILine^ get() override; }
	virtual property ILine^ Line { ILine^ get() override; }
	virtual property IPanel^ Panel { IPanel^ get() override; }
	virtual property IPanel^ Panel2 { IPanel^ get() override; }
	virtual property IUserInterface^ UI { IUserInterface^ get() override; }
	virtual property IViewer^ Viewer { IViewer^ get() override; }
	virtual property IWindow^ Window { IWindow^ get() override; }
	virtual property String^ CurrentDirectory { String^ get() override; }
	virtual property Version^ FarNetVersion { System::Version^ get() override; }
	virtual property Version^ FarVersion { System::Version^ get() override; }
public:
	virtual IDialog^ CreateDialog(int left, int top, int right, int bottom) override;
	virtual IEditor^ CreateEditor() override;
	virtual IInputBox^ CreateInputBox() override;
	virtual IListMenu^ CreateListMenu() override;
	virtual IMenu^ CreateMenu() override;
	virtual IModuleManager^ GetModuleManager(String^ name) override;
	virtual IViewer^ CreateViewer() override;
	virtual String^ GetFolderPath(SpecialFolder folder) override;
	virtual Works::IPanelWorks^ WorksPanel(FarNet::Panel^ panel, Explorer^ explorer) override;
public:
	virtual array<FarNet::Panel^>^ Panels(Guid typeId) override;
	virtual array<FarNet::Panel^>^ Panels(Type^ type) override;
	virtual array<IEditor^>^ Editors() override;
	virtual array<IViewer^>^ Viewers() override;
	virtual bool IsMaskMatch(String^ path, String^ mask) override;
	virtual bool IsMaskValid(String^ mask) override;
	virtual CultureInfo^ GetCurrentUICulture(bool update) override;
	virtual IModuleAction^ GetModuleAction(Guid id) override;
	virtual int Message(String^ body, String^ header, MessageOptions options, array<String^>^ buttons, String^ helpTopic) override;
	virtual KeyInfo^ NameToKeyInfo(String^ key) override;
	virtual Object^ GetSetting(FarSetting settingSet, String^ settingName) override;
	[MethodImpl(MethodImplOptions::NoInlining)]
	virtual String^ GetHelpTopic(String^ topic) override;
	virtual String^ Input(String^ prompt, String^ history, String^ title, String^ text) override;
	virtual String^ KeyInfoToName(KeyInfo^ key) override;
	virtual String^ PasteFromClipboard() override;
	virtual String^ TempFolder(String^ prefix) override;
	virtual String^ TempName(String^ prefix) override;
	virtual void CopyToClipboard(String^ text) override;
	virtual void PostJob(Action^ handler) override;
	virtual void PostMacro(String^ macro, bool enableOutput, bool disablePlugins) override;
	virtual void PostSteps(IEnumerable<Object^>^ steps) override;
	virtual void Quit() override;
	virtual void ShowError(String^ title, Exception^ error) override;
	virtual void ShowHelp(String^ path, String^ topic, HelpOptions options) override;
	[MethodImpl(MethodImplOptions::NoInlining)]
	virtual void ShowHelpTopic(String^ topic) override;
internal:
	static void Connect();
private:
	Far1() {}
	// Instance
	static Far1 Far;
	// Paths
	static String^ _LocalData;
	static String^ _RoamingData;
};
}
