#pragma once
using namespace System::IO;
using namespace System::Text::RegularExpressions;
class CStr;

namespace FarManagerImpl
{;
ref class EditorManager;

public ref class Far : public IFar
{
public:
	virtual property IAnyEditor^ AnyEditor { IAnyEditor^ get(); }
	virtual property ICollection<IEditor^>^ Editors { ICollection<IEditor^>^ get(); }
	virtual property ICommandLine^ CommandLine { ICommandLine^ get(); }
	virtual property IEditor^ Editor { IEditor^ get(); }
	virtual property int HWnd { int get(); }
	virtual property IPanel^ AnotherPanel { IPanel^ get(); }
	virtual property IPanel^ Panel { IPanel^ get(); }
	virtual property IVersion^ Version { IVersion^ get(); }
	virtual property String^ Clipboard { String^ get(); void set(String^ value); }
	virtual property String^ PluginFolderPath { String^ get(); }
	virtual property String^ WordDiv { String^ get(); }
public:
	virtual bool Msg(String^ body);
	virtual bool Msg(String^ body,String^ header);
	virtual IEditor^ CreateEditor();
	virtual IInputBox^ CreateInputBox();
	virtual IList<int>^ CreateKeySequence(String^ keys);
	virtual IMenu^ CreateMenu();
	virtual IMessage^ CreateMessage();
	virtual int NameToKey(String^ key);
	virtual int SaveScreen(int x1, int y1, int x2, int y2);
	virtual IPluginMenuItem^ CreatePluginsMenuItem();
	virtual IPluginMenuItem^ RegisterPluginsMenuItem(String^ name, EventHandler<OpenPluginMenuItemEventArgs^>^ onOpen);
	virtual IRect^ CreateRect(int left, int top, int right, int bottom);
	virtual ITwoPoint^ CreateStream(int left, int top, int right, int bottom);
	virtual void PostKeys(String^ keys, bool disableOutput);
	virtual void PostKeySequence(IList<int>^ sequence,bool disableOutput);
	virtual void RegisterPluginsMenuItem(IPluginMenuItem^ item);
	virtual void RegisterPrefix(String^ prefix, StringDelegate^ handler);
	virtual void RestoreScreen(int screen);
	virtual void Run(String^ cmdLine);
	virtual void SetUserScreen();
	virtual void UnregisterPluginsMenuItem(IPluginMenuItem^ item);
public:
	Object^ Test();
internal:
	Far();
	~Far();
internal:
	EditorManager^ editorManager;
	void OnGetPluginInfo(PluginInfo* pi);
	HANDLE OnOpenPlugin(int from, int item);
private:
	void CreateMenuStringsBlock();
	void FreeMenuStrings();
	void ProcessPrefixes(int Item);
	void MakePrefixes();
private:
	CStr* _menuStrings;
	CStr* _prefixes;
	IPanel^ _panel;
	IPanel^ _anotherPanel;
	ICommandLine^ _commandLine;
	List<IPluginMenuItem^>^ _registeredMenuItems;
	Dictionary<String^, StringDelegate^>^ _registeredPrefixes;
};
}
