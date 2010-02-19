/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once
#undef CreateDialog

namespace FarNet
{;
ref class Far1 : public IFar
{
public:
	virtual property FarConfirmations Confirmations { FarConfirmations get(); }
	virtual property FarMacroState MacroState { FarMacroState get(); }
	virtual property IAnyEditor^ AnyEditor { IAnyEditor^ get(); }
	virtual property IAnyPanel^ Panel { IAnyPanel^ get(); }
	virtual property IAnyPanel^ Panel2 { IAnyPanel^ get(); }
	virtual property IAnyViewer^ AnyViewer { IAnyViewer^ get(); }
	virtual property IDialog^ Dialog { IDialog^ get(); }
	virtual property IEditor^ Editor { IEditor^ get(); }
	virtual property ILine^ CommandLine { ILine^ get(); }
	virtual property ILine^ Line { ILine^ get(); }
	virtual property IMacro^ Macro { IMacro^ get(); }
	virtual property int WindowCount { int get(); }
	virtual property IntPtr MainWindowHandle { IntPtr get(); }
	virtual property IViewer^ Viewer { IViewer^ get(); }
	virtual property String^ ActivePath { String^ get(); }
	virtual property String^ RegistryFarPath { String^ get(); }
	virtual property String^ RegistryPluginsPath { String^ get(); }
	virtual property Version^ FarVersion { System::Version^ get(); }
	virtual property Version^ FarNetVersion { System::Version^ get(); }
	virtual property WindowType WindowType { FarNet::WindowType get(); }
	virtual property IZoo^ Zoo { IZoo^ get(); }
public:
	virtual array<int>^ CreateKeySequence(String^ keys);
	virtual IDialog^ CreateDialog(int left, int top, int right, int bottom);
	virtual IEditor^ CreateEditor();
	virtual IInputBox^ CreateInputBox();
	virtual IListMenu^ CreateListMenu();
	virtual IMenu^ CreateMenu();
	virtual IPanel^ CreatePanel();
	virtual ISubsetForm^ CreateSubsetForm();
	virtual IViewer^ CreateViewer();
public:
	virtual array<IEditor^>^ Editors();
	virtual array<IViewer^>^ Viewers();
	virtual bool Commit();
	virtual Char CodeToChar(int code);
	virtual ConsoleColor GetPaletteBackground(PaletteColor paletteColor);
	virtual ConsoleColor GetPaletteForeground(PaletteColor paletteColor);
	virtual CultureInfo^ GetCurrentUICulture(bool update);
	virtual FarNet::WindowType GetWindowType(int index);
	virtual ICollection<String^>^ GetDialogHistory(String^ name);
	virtual ICollection<String^>^ GetHistory(String^ name);
	virtual ICollection<String^>^ GetHistory(String^ name, String^ filter);
	virtual int Message(String^ body, String^ header, MsgOptions options);
	virtual int Message(String^ body, String^ header, MsgOptions options, array<String^>^ buttons);
	virtual int Message(String^ body, String^ header, MsgOptions options, array<String^>^ buttons, String^ helpTopic);
	virtual int NameToKey(String^ key);
	virtual int SaveScreen(int x1, int y1, int x2, int y2);
	virtual IPanel^ FindPanel(Guid typeId);
	virtual IPanel^ FindPanel(Type^ hostType);
	virtual IWindowInfo^ GetWindowInfo(int index, bool full);
	virtual String^ Input(String^ prompt);
	virtual String^ Input(String^ prompt, String^ history);
	virtual String^ Input(String^ prompt, String^ history, String^ title);
	virtual String^ Input(String^ prompt, String^ history, String^ title, String^ text);
	virtual String^ KeyToName(int key);
	virtual String^ PasteFromClipboard();
	virtual String^ TempFolder() { return TempFolder(nullptr); }
	virtual String^ TempFolder(String^ prefix);
	virtual String^ TempName() { return TempName(nullptr); }
	virtual String^ TempName(String^ prefix);
	virtual void CopyToClipboard(String^ text);
	virtual void GetUserScreen();
	virtual void Message(String^ body);
	virtual void Message(String^ body, String^ header);
	virtual void PostJob(EventHandler^ handler);
	virtual void PostKeys(String^ keys);
	virtual void PostKeys(String^ keys, bool disableOutput);
	virtual void PostKeySequence(array<int>^ sequence);
	virtual void PostKeySequence(array<int>^ sequence, bool disableOutput);
	virtual void PostMacro(String^ macro);
	virtual void PostMacro(String^ macro, bool enableOutput, bool disablePlugins);
	virtual void PostStep(EventHandler^ handler);
	virtual void PostStepAfterKeys(String^ keys, EventHandler^ handler);
	virtual void PostStepAfterStep(EventHandler^ handler1, EventHandler^ handler2);
	virtual void PostText(String^ text);
	virtual void PostText(String^ text, bool disableOutput);
	virtual void Quit();
	virtual void Redraw();
	virtual void RegisterCommand(IModuleManager^ manager, Guid id, EventHandler<ModuleCommandEventArgs^>^ handler, ModuleCommandAttribute^ attribute);
	virtual void RegisterFiler(IModuleManager^ manager, Guid id, EventHandler<ModuleFilerEventArgs^>^ handler, ModuleFilerAttribute^ attribute);
	virtual void RegisterTool(IModuleManager^ manager, Guid id, EventHandler<ModuleToolEventArgs^>^ handler, ModuleToolAttribute^ attribute);
	virtual void RestoreScreen(int screen);
	virtual void Run(String^ command);
	virtual void SetCurrentWindow(int index);
	virtual void SetProgressState(TaskbarProgressBarState state);
	virtual void SetProgressValue(int currentValue, int maximumValue);
	virtual void SetUserScreen();
	virtual void ShowError(String^ title, Exception^ error);
	virtual void ShowHelp(String^ path, String^ topic, HelpOptions options);
	virtual void ShowPanelMenu(bool showPushCommand);
	virtual void Unregister(BaseModuleItem^ item);
	virtual void UnregisterCommand(EventHandler<ModuleCommandEventArgs^>^ handler);
	virtual void UnregisterFiler(EventHandler<ModuleFilerEventArgs^>^ handler);
	virtual void UnregisterTool(EventHandler<ModuleToolEventArgs^>^ handler);
	virtual void Write(String^ text);
	virtual void Write(String^ text, ConsoleColor foregroundColor);
	virtual void Write(String^ text, ConsoleColor foregroundColor, ConsoleColor backgroundColor);
	virtual void WritePalette(int left, int top, PaletteColor paletteColor, String^ text);
	virtual void WriteText(int left, int top, ConsoleColor foregroundColor, ConsoleColor backgroundColor, String^ text);
internal:
	static void Connect();
private:
	Far1() {}
	static Far1 Far;
};

}
