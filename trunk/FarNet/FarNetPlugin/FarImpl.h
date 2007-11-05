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
ref class FarPanelPlugin;

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
	virtual property IPanel^ AnotherPanel { IPanel^ get(); }
	virtual property IPanel^ Panel { IPanel^ get(); }
	virtual property OpenFrom From { OpenFrom get(); }
	virtual property String^ Clipboard { String^ get(); void set(String^ value); }
	virtual property String^ PluginFolderPath { String^ get(); }
	virtual property String^ RootFar { String^ get(); }
	virtual property String^ RootKey { String^ get(); }
	virtual property String^ WordDiv { String^ get(); }
	virtual property Version^ Version { System::Version^ get(); }
public:
	virtual bool Commit();
	virtual bool Msg(String^ body);
	virtual bool Msg(String^ body, String^ header);
	virtual Char CodeToChar(int code);
	virtual ICollection<String^>^ GetHistory(String^ name);
	virtual IDialog^ CreateDialog(int left, int top, int right, int bottom);
	virtual IEditor^ CreateEditor();
	virtual IFile^ CreatePanelItem();
	virtual IFile^ CreatePanelItem(FileSystemInfo^ info, bool fullName);
	virtual IInputBox^ CreateInputBox();
	virtual IList<int>^ CreateKeySequence(String^ keys);
	virtual IListMenu^ CreateListMenu();
	virtual IMenu^ CreateMenu();
	virtual IMessage^ CreateMessage();
	virtual int Msg(String^ body, String^ header, MessageOptions options);
	virtual int Msg(String^ body, String^ header, MessageOptions options, array<String^>^ buttons);
	virtual int NameToKey(String^ key);
	virtual int SaveScreen(int x1, int y1, int x2, int y2);
	virtual IPanelPlugin^ CreatePanelPlugin();
	virtual IPanelPlugin^ GetPanelPlugin(Type^ hostType);
	virtual IPluginMenuItem^ CreatePluginsMenuItem();
	virtual IPluginMenuItem^ RegisterPluginsMenuItem(String^ name, EventHandler<OpenPluginMenuItemEventArgs^>^ onOpen);
	virtual IViewer^ CreateViewer();
	virtual IWindowInfo^ GetWindowInfo(int index, bool full);
	virtual Object^ GetPluginValue(String^ pluginName, String^ valueName, Object^ defaultValue);
	virtual String^ Input(String^ prompt);
	virtual String^ Input(String^ prompt, String^ history);
	virtual String^ Input(String^ prompt, String^ history, String^ title);
	virtual String^ Input(String^ prompt, String^ history, String^ title, String^ text);
	virtual void GetUserScreen();
	virtual void LoadMacros();
	virtual void PostKeys(String^ keys);
	virtual void PostKeys(String^ keys, bool disableOutput);
	virtual void PostKeySequence(IList<int>^ sequence, bool disableOutput);
	virtual void PostText(String^ text);
	virtual void PostText(String^ text, bool disableOutput);
	virtual void RegisterPluginsConfigItem(IPluginMenuItem^ item);
	virtual void RegisterPluginsDiskItem(IPluginMenuItem^ item);
	virtual void RegisterPluginsMenuItem(IPluginMenuItem^ item);
	virtual void RegisterPrefix(String^ prefix, StringDelegate^ handler);
	virtual void RestoreScreen(int screen);
	virtual void Run(String^ command);
	virtual void SaveMacros();
	virtual void SetCurrentWindow(int index);
	virtual void SetPluginValue(String^ pluginName, String^ valueName, Object^ newValue);
	virtual void SetUserScreen();
	virtual void ShowError(String^ title, Exception^ error);
	virtual void ShowHelp(String^ path, String^ topic, HelpOptions options);
	virtual void Write(String^ text);
	virtual void Write(String^ text, ConsoleColor foregroundColor);
	virtual void Write(String^ text, ConsoleColor foregroundColor, ConsoleColor backgroundColor);
	virtual void WriteText(int left, int top, ConsoleColor foregroundColor, ConsoleColor backgroundColor, String^ text);
internal:
	Far();
	~Far();
	IPanelPlugin^ GetAnotherPanelPlugin(FarPanelPlugin^ plugin);
	void OpenPanelPlugin(FarPanelPlugin^ plugin);
	void ReplacePanelPlugin(FarPanelPlugin^ oldPanel, FarPanelPlugin^ newPanel);
internal:
	EditorManager^ _editorManager;
	static String^ _folder = Path::GetDirectoryName((Assembly::GetExecutingAssembly())->Location);
	static String^ _helpTopic = "<" + _folder + "\\>";
internal:
	bool AsConfigure(int itemIndex);
	HANDLE AsOpenPlugin(int from, INT_PTR item);
	int AsDeleteFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int opMode);
	int AsGetFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, char* destPath, int opMode);
	int AsGetFindData(HANDLE hPlugin, PluginPanelItem** pPanelItem, int* pItemsNumber, int opMode);
	int AsMakeDirectory(HANDLE hPlugin, char* name, int opMode);
	int AsProcessEvent(HANDLE hPlugin, int id, void* param);
	int AsProcessKey(HANDLE hPlugin, int key, unsigned int controlState);
	int AsPutFiles(HANDLE hPlugin, PluginPanelItem* panelItem, int itemsNumber, int move, int opMode);
	int AsSetDirectory(HANDLE hPlugin, const char* dir, int opMode);
	void AsClosePlugin(HANDLE hPlugin);
	void AsGetOpenPluginInfo(HANDLE hPlugin, OpenPluginInfo* info);
	void AsGetPluginInfo(PluginInfo* pi);
	static void AsFreeFindData(PluginPanelItem* panelItem, int itemsNumber);
private:
	HANDLE AddPanelPlugin(FarPanelPlugin^ plugin);
	void CreateMenuStringsBlock();
	void FreeMenuStrings();
	void ProcessPrefixes(INT_PTR item);
	void MakePrefixes();
private:
	CStr* _configStrings;
	CStr* _diskStrings;
	CStr* _menuStrings;
	CStr* _prefixes;
	List<IPluginMenuItem^>^ _registeredConfigItems;
	List<IPluginMenuItem^>^ _registeredDiskItems;
	List<IPluginMenuItem^>^ _registeredMenuItems;
	Dictionary<String^, StringDelegate^>^ _registeredPrefixes;
	OpenFrom _from;
	bool _canOpenPanelPlugin;
	array<FarPanelPlugin^>^ _panels;
};

// Far for anybody
Far^ GetFar();
}
