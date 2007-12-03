/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#pragma once
#undef CreateDialog

namespace FarNet
{;
ref class EditorManager;
ref class CommandPluginInfo;
ref class FilerPluginInfo;
ref class ToolPluginInfo;

public ref class Far : public IFar
{
public:
	virtual property FarConfirmations Confirmations { FarConfirmations get(); }
	virtual property IAnyEditor^ AnyEditor { IAnyEditor^ get(); }
	virtual property ICollection<IEditor^>^ Editors { ICollection<IEditor^>^ get(); }
	virtual property ILine^ CommandLine { ILine^ get(); }
	virtual property IEditor^ Editor { IEditor^ get(); }
	virtual property IntPtr HWnd { IntPtr get(); }
	virtual property int WindowCount { int get(); }
	virtual property IPanel^ Panel { IPanel^ get(); }
	virtual property IPanel^ Panel2 { IPanel^ get(); }
	virtual property String^ PluginFolderPath { String^ get(); }
	virtual property String^ RootFar { String^ get(); }
	virtual property String^ RootKey { String^ get(); }
	virtual property String^ WordDiv { String^ get(); }
	virtual property Version^ Version { System::Version^ get(); }
public:
	virtual array<int>^ CreateKeySequence(String^ keys);
	virtual array<IPanelPlugin^>^ PushedPanels();
	virtual bool Commit();
	virtual void Msg(String^ body);
	virtual void Msg(String^ body, String^ header);
	virtual Char CodeToChar(int code);
	virtual ICollection<String^>^ GetHistory(String^ name);
	virtual IDialog^ CreateDialog(int left, int top, int right, int bottom);
	virtual IEditor^ CreateEditor();
	virtual IFile^ CreatePanelItem();
	virtual IFile^ CreatePanelItem(FileSystemInfo^ info, bool fullName);
	virtual IInputBox^ CreateInputBox();
	virtual IListMenu^ CreateListMenu();
	virtual IMenu^ CreateMenu();
	virtual IMessage^ CreateMessage();
	virtual int Msg(String^ body, String^ header, MessageOptions options);
	virtual int Msg(String^ body, String^ header, MessageOptions options, array<String^>^ buttons);
	virtual int NameToKey(String^ key);
	virtual int SaveScreen(int x1, int y1, int x2, int y2);
	virtual IPanelPlugin^ CreatePanelPlugin();
	virtual IPanelPlugin^ GetPanelPlugin(Type^ hostType);
	virtual IViewer^ CreateViewer();
	virtual IWindowInfo^ GetWindowInfo(int index, bool full);
	virtual Object^ GetPluginValue(String^ pluginName, String^ valueName, Object^ defaultValue);
	virtual String^ Input(String^ prompt);
	virtual String^ Input(String^ prompt, String^ history);
	virtual String^ Input(String^ prompt, String^ history, String^ title);
	virtual String^ Input(String^ prompt, String^ history, String^ title, String^ text);
	virtual String^ PasteFromClipboard();
	virtual String^ RegisterCommand(BasePlugin^ plugin, String^ name, String^ prefix, EventHandler<CommandEventArgs^>^ handler);
	virtual void CopyToClipboard(String^ text);
	virtual void GetUserScreen();
	virtual void LoadMacros();
	virtual void PostKeys(String^ keys);
	virtual void PostKeys(String^ keys, bool disableOutput);
	virtual void PostKeySequence(array<int>^ sequence);
	virtual void PostKeySequence(array<int>^ sequence, bool disableOutput);
	virtual void PostMacro(String^ macro);
	virtual void PostMacro(String^ macro, bool disableOutput, bool noSendKeysToPlugins);
	virtual void PostStep(EventHandler^ step);
	virtual void PostText(String^ text);
	virtual void PostText(String^ text, bool disableOutput);
	virtual void RegisterFiler(BasePlugin^ plugin, String^ name, EventHandler<FilerEventArgs^>^ handler, String^ mask, bool creates);
	virtual void RegisterTool(BasePlugin^ plugin, String^ name, EventHandler<ToolEventArgs^>^ handler, ToolOptions options);
	virtual void RestoreScreen(int screen);
	virtual void Run(String^ command);
	virtual void SaveMacros();
	virtual void SetCurrentWindow(int index);
	virtual void SetPluginValue(String^ pluginName, String^ valueName, Object^ newValue);
	virtual void SetUserScreen();
	virtual void ShowError(String^ title, Exception^ error);
	virtual void ShowHelp(String^ path, String^ topic, HelpOptions options);
	virtual void ShowPanelMenu(bool showPushCommand);
	virtual void UnregisterCommand(EventHandler<CommandEventArgs^>^ handler);
	virtual void UnregisterFiler(EventHandler<FilerEventArgs^>^ handler);
	virtual void UnregisterTool(EventHandler<ToolEventArgs^>^ handler);
	virtual void Write(String^ text);
	virtual void Write(String^ text, ConsoleColor foregroundColor);
	virtual void Write(String^ text, ConsoleColor foregroundColor, ConsoleColor backgroundColor);
	virtual void WriteText(int left, int top, ConsoleColor foregroundColor, ConsoleColor backgroundColor, String^ text);
	virtual WindowType GetWindowType(int index);
internal:
	static property Far^ Instance { Far^ get() { return _instance; } }
	static void StartFar();
	void Stop();
internal:
	EditorManager^ _editorManager;
	static String^ _folder = Path::GetDirectoryName((Assembly::GetExecutingAssembly())->Location);
	static String^ _helpTopic = "<" + _folder + "\\>";
internal:
	bool AsConfigure(int itemIndex);
	HANDLE AsOpenFilePlugin(char* name, const unsigned char* data, int dataSize);
	HANDLE AsOpenPlugin(int from, INT_PTR item);
	void AsGetPluginInfo(PluginInfo* pi);
	void RegisterCommands(IEnumerable<CommandPluginInfo^>^ commands);
	void RegisterFilers(IEnumerable<FilerPluginInfo^>^ filers);
	void RegisterTool(ToolPluginInfo^ tool);
	void RegisterTools(IEnumerable<ToolPluginInfo^>^ tools);
	Object^ GetFarNetValue(String^ keyPath, String^ valueName, Object^ defaultValue) { return GetPluginValue("FAR.NET\\" + keyPath, valueName, defaultValue); }
	void SetFarNetValue(String^ keyPath, String^ valueName, Object^ value) { SetPluginValue("FAR.NET\\" + keyPath, valueName, value); }
private:
	Far();
	Object^ GetFarValue(String^ keyPath, String^ valueName, Object^ defaultValue);
	void Free(ToolOptions options);
	void OnConfigCommand();
	void OnConfigFiler();
	void OnConfigTool(String^ title, ToolOptions option, List<ToolPluginInfo^>^ list);
	void OnNetConfig(Object^ sender, ToolEventArgs^ e);
	void OnNetDisk(Object^ sender, ToolEventArgs^ e);
	void OnNetF11Menus(Object^ sender, ToolEventArgs^ e);
	void ProcessPrefixes(INT_PTR item);
	void Start();
private: // public candidates
	static bool CompareName(String^ mask, const char* name, bool skipPath);
private:
	// The instance
	static Far^ _instance;
private:
	CStr* _pConfig;
	CStr* _pDisk;
	CStr* _pEditor;
	CStr* _pPanels;
	CStr* _pViewer;
	CStr* _prefixes;
	List<CommandPluginInfo^> _registeredCommand;
	List<FilerPluginInfo^> _registeredFiler;
	List<ToolPluginInfo^> _registeredConfig;
	List<ToolPluginInfo^> _registeredDisk;
	List<ToolPluginInfo^> _registeredEditor;
	List<ToolPluginInfo^> _registeredPanels;
	List<ToolPluginInfo^> _registeredViewer;
private:
	String^ _hotkey;
	array<int>^ _hotkeys;
	EventHandler^ _handler;
};

}
