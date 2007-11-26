/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#pragma once
#undef CreateDialog
using namespace System::IO;
using namespace System::Text::RegularExpressions;
class CStr;

namespace FarManagerImpl
{;
ref class EditorManager;

ref class PluginAny abstract
{
public:
	property String^ Name { String^ get() { return _Name; } }
protected:
	PluginAny(BasePlugin^ plugin, String^ name);
	property String^ Key { String^ get() { return _Key; } }
private:
	String^ _Key;
	String^ _Name;
};

ref class PluginTool : PluginAny
{
public:
	PluginTool(BasePlugin^ plugin, String^ name, EventHandler<ToolEventArgs^>^ handler, ToolOptions options);
	property EventHandler<ToolEventArgs^>^ Handler { EventHandler<ToolEventArgs^>^ get() { return _Handler; } }
	property ToolOptions Options { ToolOptions get() { return _Options; } }
	String^ Alias(ToolOptions option);
	void Alias(ToolOptions option, String^ value);
private:
	EventHandler<ToolEventArgs^>^ _Handler;
	ToolOptions _Options;
	String^ _AliasConfig;
	String^ _AliasDisk;
	String^ _AliasEditor;
	String^ _AliasPanels;
	String^ _AliasViewer;
};

ref class PluginPrefix : PluginAny
{
public:
	PluginPrefix(BasePlugin^ plugin, String^ name, String^ prefix, EventHandler<ExecutingEventArgs^>^ handler) : PluginAny(plugin, name), _Prefix(prefix), _Handler(handler) {}
	property String^ Prefix { String^ get() { return _Prefix; } }
	property EventHandler<ExecutingEventArgs^>^ Handler { EventHandler<ExecutingEventArgs^>^ get() { return _Handler; } }
	String^ Alias();
	void Alias(String^ value);
private:
	String^ _Prefix;
	EventHandler<ExecutingEventArgs^>^ _Handler;
	String^ _Alias;
};

ref class PluginFile : PluginAny
{
public:
	PluginFile(BasePlugin^ plugin, String^ name, EventHandler<OpenFileEventArgs^>^ handler) : PluginAny(plugin, name), _Handler(handler) {}
	property EventHandler<OpenFileEventArgs^>^ Handler { EventHandler<OpenFileEventArgs^>^ get() { return _Handler; } }
private:
	EventHandler<OpenFileEventArgs^>^ _Handler;
};

public ref class Far : public IFar
{
public:
	virtual property FarConfirmations Confirmations { FarConfirmations get(); }
	virtual property IAnyEditor^ AnyEditor { IAnyEditor^ get(); }
	virtual property ICollection<IEditor^>^ Editors { ICollection<IEditor^>^ get(); }
	virtual property ILine^ CommandLine { ILine^ get(); }
	virtual property IEditor^ Editor { IEditor^ get(); }
	virtual property int HWnd { int get(); }
	virtual property int WindowCount { int get(); }
	virtual property IPanel^ Panel { IPanel^ get(); }
	virtual property IPanel^ Panel2 { IPanel^ get(); }
	virtual property String^ Clipboard { String^ get(); void set(String^ value); }
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
	virtual String^ RegisterPrefix(BasePlugin^ plugin, String^ name, String^ prefix, EventHandler<ExecutingEventArgs^>^ handler);
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
	virtual void RegisterFile(BasePlugin^ plugin, String^ name, EventHandler<OpenFileEventArgs^>^ handler);
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
	virtual void UnregisterFile(EventHandler<OpenFileEventArgs^>^ handler);
	virtual void UnregisterPrefix(EventHandler<ExecutingEventArgs^>^ handler);
	virtual void UnregisterTool(EventHandler<ToolEventArgs^>^ handler);
	virtual void Write(String^ text);
	virtual void Write(String^ text, ConsoleColor foregroundColor);
	virtual void Write(String^ text, ConsoleColor foregroundColor, ConsoleColor backgroundColor);
	virtual void WriteText(int left, int top, ConsoleColor foregroundColor, ConsoleColor backgroundColor, String^ text);
	virtual WindowType GetWindowType(int index);
internal:
	static Far^ Get() { return _instance; }
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
private:
	Far();
	void Start();
	void Free(ToolOptions options);
	void ProcessPrefixes(INT_PTR item);
	Object^ GetFarValue(String^ keyPath, String^ valueName, Object^ defaultValue);
	void OnNetConfig(Object^ sender, ToolEventArgs^ e);
	void OnNetF11Menus(Object^ sender, ToolEventArgs^ e);
	void OnNetDisk(Object^ sender, ToolEventArgs^ e);
	void OnConfigFile();
	void OnConfigPrefix();
	void OnConfigTool(String^ title, ToolOptions option, List<PluginTool^>^ list);
private:
	static Far^ _instance;
	CStr* _pConfig;
	CStr* _pDisk;
	CStr* _pEditor;
	CStr* _pPanels;
	CStr* _pViewer;
	CStr* _prefixes;
	List<PluginFile^> _registeredFile;
	List<PluginPrefix^> _registeredPrefix;
	List<PluginTool^> _registeredConfig;
	List<PluginTool^> _registeredDisk;
	List<PluginTool^> _registeredEditor;
	List<PluginTool^> _registeredPanels;
	List<PluginTool^> _registeredViewer;
	Dictionary<EventHandler<ToolEventArgs^>^, PluginTool^> _registeredTools;
	String^ _hotkey;
	array<int>^ _hotkeys;
	EventHandler^ _handler;
};

}
