/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once
#undef CreateDialog

namespace FarNet
{;
ref class CommandPluginInfo;
ref class Editor;
ref class Editor0;
ref class EditorPluginInfo;
ref class FilerPluginInfo;
ref class ToolPluginInfo;
ref class Viewer;
ref class Viewer0;

ref class Far : public IFar
{
public:
	virtual property FarConfirmations Confirmations { FarConfirmations get(); }
	virtual property FarMacroState MacroState { FarMacroState get(); }
	virtual property IAnyEditor^ AnyEditor { IAnyEditor^ get(); }
	virtual property IAnyViewer^ AnyViewer { IAnyViewer^ get(); }
	virtual property IDialog^ Dialog { IDialog^ get(); }
	virtual property IEditor^ Editor { IEditor^ get(); }
	virtual property IKeyMacroHost^ KeyMacro { IKeyMacroHost^ get(); }
	virtual property ILine^ CommandLine { ILine^ get(); }
	virtual property ILine^ Line { ILine^ get(); }
	virtual property int WindowCount { int get(); }
	virtual property IntPtr HWnd { IntPtr get(); }
	virtual property IPanel^ Panel { IPanel^ get(); }
	virtual property IPanel^ Panel2 { IPanel^ get(); }
	virtual property IViewer^ Viewer { IViewer^ get(); }
	virtual property String^ ActivePath { String^ get(); }
	virtual property String^ PluginPath { String^ get(); }
	virtual property String^ RootFar { String^ get(); }
	virtual property String^ RootKey { String^ get(); }
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
	virtual IPluginPanel^ CreatePluginPanel();
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
	virtual ICollection<String^>^ GetDialogHistory(String^ name);
	virtual ICollection<String^>^ GetHistory(String^ name);
	virtual ICollection<String^>^ GetHistory(String^ name, String^ filter);
	virtual int Msg(String^ body, String^ header, MsgOptions options);
	virtual int Msg(String^ body, String^ header, MsgOptions options, array<String^>^ buttons);
	virtual int Msg(String^ body, String^ header, MsgOptions options, array<String^>^ buttons, String^ helpTopic);
	virtual int NameToKey(String^ key);
	virtual int SaveScreen(int x1, int y1, int x2, int y2);
	virtual IPluginPanel^ GetPluginPanel(Guid id);
	virtual IPluginPanel^ GetPluginPanel(Type^ hostType);
	virtual IWindowInfo^ GetWindowInfo(int index, bool full);
	virtual Object^ GetPluginValue(String^ pluginName, String^ valueName, Object^ defaultValue);
	virtual String^ Input(String^ prompt);
	virtual String^ Input(String^ prompt, String^ history);
	virtual String^ Input(String^ prompt, String^ history, String^ title);
	virtual String^ Input(String^ prompt, String^ history, String^ title, String^ text);
	virtual String^ KeyToName(int key);
	virtual String^ PasteFromClipboard();
	virtual String^ RegisterCommand(BasePlugin^ plugin, String^ name, String^ prefix, EventHandler<CommandEventArgs^>^ handler);
	virtual String^ TempFolder() { return TempFolder(nullptr); }
	virtual String^ TempFolder(String^ prefix);
	virtual String^ TempName() { return TempName(nullptr); }
	virtual String^ TempName(String^ prefix);
	virtual void CopyToClipboard(String^ text);
	virtual void GetUserScreen();
	virtual void Msg(String^ body);
	virtual void Msg(String^ body, String^ header);
	virtual void PostKeys(String^ keys);
	virtual void PostKeys(String^ keys, bool disableOutput);
	virtual void PostKeySequence(array<int>^ sequence);
	virtual void PostKeySequence(array<int>^ sequence, bool disableOutput);
	virtual void PostStep(EventHandler^ handler);
	virtual void PostStepAfterKeys(String^ keys, EventHandler^ handler);
	virtual void PostStepAfterStep(EventHandler^ handler1, EventHandler^ handler2);
	virtual void PostText(String^ text);
	virtual void PostText(String^ text, bool disableOutput);
	virtual void Redraw();
	virtual void RegisterFiler(BasePlugin^ plugin, String^ name, EventHandler<FilerEventArgs^>^ handler, String^ mask, bool creates);
	virtual void RegisterTool(BasePlugin^ plugin, String^ name, EventHandler<ToolEventArgs^>^ handler, ToolOptions options);
	virtual void RestoreScreen(int screen);
	virtual void Run(String^ command);
	virtual void SetCurrentWindow(int index);
	virtual void SetPluginValue(String^ pluginName, String^ valueName, Object^ newValue);
	virtual void SetProgressState(TaskbarProgressBarState state);
	virtual void SetProgressValue(int currentValue, int maximumValue);
	virtual void SetUserScreen();
	virtual void ShowError(String^ title, Exception^ error);
	virtual void ShowHelp(String^ path, String^ topic, HelpOptions options);
	virtual void ShowPanelMenu(bool showPushCommand);
	virtual void Unregister(BasePlugin^ plugin);
	virtual void UnregisterCommand(EventHandler<CommandEventArgs^>^ handler);
	virtual void UnregisterFiler(EventHandler<FilerEventArgs^>^ handler);
	virtual void UnregisterTool(EventHandler<ToolEventArgs^>^ handler);
	virtual void Write(String^ text);
	virtual void Write(String^ text, ConsoleColor foregroundColor);
	virtual void Write(String^ text, ConsoleColor foregroundColor, ConsoleColor backgroundColor);
	virtual void WritePalette(int left, int top, PaletteColor paletteColor, String^ text);
	virtual void WriteText(int left, int top, ConsoleColor foregroundColor, ConsoleColor backgroundColor, String^ text);
	virtual FarNet::WindowType GetWindowType(int index);
	virtual void PostJob(EventHandler^ handler);
internal:
	static property Far^ Instance { Far^ get() { return _instance; } }
	static void StartFar();
	void Stop();
	void OnEditorOpened(FarNet::Editor^ editor);
internal:
	static String^ _folder = Path::GetDirectoryName((Assembly::GetExecutingAssembly())->Location);
	static String^ _helpTopic = "<" + _folder + "\\>";
internal:
	bool AsConfigure(int itemIndex);
	HANDLE AsOpenFilePlugin(wchar_t* name, const unsigned char* data, int dataSize, int opMode);
	HANDLE AsOpenPlugin(int from, INT_PTR item);
	void AsGetPluginInfo(PluginInfo* pi);
	void AsProcessSynchroEvent(int type, void* param);
	void RegisterCommands(IEnumerable<CommandPluginInfo^>^ commands);
	void RegisterEditors(IEnumerable<EditorPluginInfo^>^ editors);
	void RegisterFilers(IEnumerable<FilerPluginInfo^>^ filers);
	void RegisterTool(ToolPluginInfo^ tool);
	void RegisterTools(IEnumerable<ToolPluginInfo^>^ tools);
	Object^ GetFarNetValue(String^ keyPath, String^ valueName, Object^ defaultValue) { return GetPluginValue("FarNet\\" + keyPath, valueName, defaultValue); }
	void SetFarNetValue(String^ keyPath, String^ valueName, Object^ value) { SetPluginValue("FarNet\\" + keyPath, valueName, value); }
private:
	Far();
	Object^ GetFarValue(String^ keyPath, String^ valueName, Object^ defaultValue);
	void AssertHotkeys();
	void Free(ToolOptions options);
	void OnConfigCommand();
	void OnConfigEditor();
	void OnConfigFiler();
	void OnConfigTool(String^ title, ToolOptions option, List<ToolPluginInfo^>^ list);
	void OnConfigUICulture();
	void OpenConfig();
	void OpenMenu(ToolOptions from);
	void ProcessPrefixes(INT_PTR item);
	void Start();
private:
	static bool CompareName(String^ mask, const wchar_t* name, bool skipPath);
	static bool CompareNameEx(String^ mask, const wchar_t* name, bool skipPath);
	static int GetPaletteColor(PaletteColor paletteColor);
	static void VoidStep(Object^, EventArgs^) {}
private:
	// The instance
	static Far^ _instance;
private:
	CStr* _pConfig;
	CStr* _pDisk;
	CStr* _pDialog;
	CStr* _pEditor;
	CStr* _pPanels;
	CStr* _pViewer;
	CStr* _prefixes;
	List<CommandPluginInfo^> _registeredCommand;
	List<EditorPluginInfo^> _registeredEditor;
	List<FilerPluginInfo^> _registeredFiler;
	List<ToolPluginInfo^> _toolConfig;
	List<ToolPluginInfo^> _toolDisk;
	List<ToolPluginInfo^> _toolDialog;
	List<ToolPluginInfo^> _toolEditor;
	List<ToolPluginInfo^> _toolPanels;
	List<ToolPluginInfo^> _toolViewer;
private:
	String^ _hotkey;
	array<int>^ _hotkeys;
	EventHandler^ _handler;
	CultureInfo^ _currentUICulture;
	
	HANDLE _hMutex;
	List<EventHandler^> _syncHandlers;
};

}
