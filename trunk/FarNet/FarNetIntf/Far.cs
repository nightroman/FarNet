using FarManager.Forms;
using System.Collections.Generic;
using System;

namespace FarManager
{
	/// <summary>
	/// The main interface of Far manager
	/// </summary>
	public interface IFar
	{
		/// <summary>
		/// Path to folder where plugin dll situated
		/// </summary>
		string PluginFolderPath { get; }
		/// <summary>
		/// Registers a command line prefix.
		/// </summary>
		/// <remarks>
		/// Call this function at plugin connection if you want to handle command line prefixes.
		/// </remarks>
		/// <param name="prefix">Command prefix.</param>
		/// <param name="handler">Handler of a command.</param>
		void RegisterPrefix(string prefix, StringDelegate handler);
		/// <summary>
		/// Adds menu item to the main plugin menu (shown by F11)
		/// <seealso cref="UnregisterPluginsMenuItem"/>
		/// </summary>
		/// <param name="item">the menuitem being registered</param>
		void RegisterPluginsMenuItem(IPluginMenuItem item);
		/// <summary>
		/// Create and add menu item to the main plugin menu (shown by F11)
		/// <seealso cref="UnregisterPluginsMenuItem"/>
		/// </summary>
		/// <param name="name">name of menu item</param>
		/// <param name="onOpen">OnOpen event handler</param>
		/// <returns>newly created item</returns>
		IPluginMenuItem RegisterPluginsMenuItem(string name, EventHandler<OpenPluginMenuItemEventArgs> onOpen);
		/// <summary>
		/// Unregister plugin menu item
		/// </summary>
		/// <param name="item">item being unregistered</param>
		void UnregisterPluginsMenuItem(IPluginMenuItem item);
		/// <summary>
		/// Create new plugin menu item
		/// <seealso cref="RegisterPluginsMenuItem(IPluginMenuItem)"/>
		/// <seealso cref="UnregisterPluginsMenuItem"/>
		/// </summary>
		/// <returns>newly created plugin menu item</returns>
		IPluginMenuItem CreatePluginsMenuItem();
		/// <summary>
		/// Show message box.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <returns>True on Enter.</returns>
		bool Msg(string body);
		/// <summary>
		/// Show message box.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <param name="header">Message header.</param>
		/// <returns>True on Enter.</returns>
		bool Msg(string body, string header);
		/// <summary>
		/// Show message box with options.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <param name="header">Message header.</param>
		/// <param name="options">Message options.</param>
		/// <returns>Button index or -1 on Escape.</returns>
		int Msg(string body, string header, MessageOptions options);
		/// <summary>
		/// Show message box with options and buttons.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <param name="header">Message header.</param>
		/// <param name="options">Message options.</param>
		/// <param name="buttons">Message buttons.</param>
		/// <returns>Button index or -1 on Escape.</returns>
		int Msg(string body, string header, MessageOptions options, string[] buttons);
		/// <summary>
		/// Create IMessage implementation
		/// </summary>
		IMessage CreateMessage();
		/// <summary>
		/// Run specified command line (works only with Far.Net registered prefixes)
		/// </summary>
		///<param name="cmdLine">Command line</param>
		void Run(string cmdLine);
		/// <summary>
		/// Windows handle of FAR
		/// </summary>
		int HWnd { get; }
		/// <summary>
		/// FAR version
		/// </summary>
		Version Version { get; }
		/// <summary>
		/// Create InputBox implementation
		/// </summary>
		IInputBox CreateInputBox();
		/// <summary>
		/// Create Menu implementation
		/// </summary>		
		IMenu CreateMenu();
		/// <summary>
		/// Virtual editor instance. Subscribe to its events when you want be 
		/// informed about all editors events.
		/// </summary>
		IAnyEditor AnyEditor { get; }
		/// <summary>
		/// String of word delimiters
		/// </summary>
		string WordDiv { get; }
		/// <summary>
		/// Clipboard contents
		/// </summary>		
		string Clipboard { get; set; }
		/// <summary>
		/// Create new editor
		/// </summary>
		IEditor CreateEditor();
		/// <summary>
		/// Create new viewer
		/// </summary>
		IViewer CreateViewer();
		/// <summary>
		/// Post keys to the FAR keyboard queue.
		/// </summary>
		/// <param name="keys">String of keys.</param>
		/// <param name="disableOutput">Do not display processing on the screen.</param>
		void PostKeys(string keys, bool disableOutput);
		/// <summary>
		/// Post text to the FAR keyboard queue.
		/// </summary>
		/// <param name="text">Literal text. \t, \r, \n, \r\n are supported, too.</param>
		/// <param name="disableOutput">Do not display processing on the screen.</param>
		void PostText(string text, bool disableOutput);
		/// <summary>
		/// Sequence of key codes from string of keys.
		/// </summary>
		IList<int> CreateKeySequence(string keys);
		/// <summary>
		/// Post a sequence of keys to the FAR keyboard queue.
		/// </summary>
		/// <param name="sequence">Sequence of keys.</param>
		/// <param name="disableOutput">Do not display processing on the screen.</param>
		void PostKeySequence(IList<int> sequence, bool disableOutput);
		/// <summary>
		/// Convert key string representation to the key code
		/// </summary>
		int NameToKey(string key);
		/// <summary>
		/// Save screen area 
		/// </summary>
		int SaveScreen(int x1, int y1, int x2, int y2);
		/// <summary>
		/// Restore screen area
		/// </summary>
		void RestoreScreen(int screen);
		/// <summary>
		/// Active editor or null if none.
		/// </summary>
		IEditor Editor { get; }
		/// <summary>
		/// Editor collection. Be extremely careful working with it.
		/// </summary>
		ICollection<IEditor> Editors { get; }
		/// <summary>
		/// Current (active) panel.
		/// </summary>
		IPanel Panel { get; }
		/// <summary>
		/// Another (passive) panel.
		/// </summary>
		IPanel AnotherPanel { get; }
		/// <summary>
		/// The Far command line.
		/// </summary>
		/// <remarks>
		/// If a plugin is called from the command line (including a user menu (F2)
		/// then command line properties and methods may not work correctly; in
		/// this case consider to call a plugin operation from a plugin menu.
		/// Staring from Far 1.71.2192 you can set the entire command line text
		/// if you call a plugin from the command line (but not from a user menu).
		/// </remarks>
		ILine CommandLine { get; }
		/// <summary>
		/// Copies the current screen contents to the FAR user screen buffer
		/// (which is displayed when the panels are switched off).
		/// </summary>
		void SetUserScreen();
		/// <summary>
		/// Copies the current user screen buffer to console screen
		/// (which is displayed when the panels are switched off).
		/// VERSION: Far 1.71.2186.
		/// </summary>
		void GetUserScreen();
		/// <summary>
		/// Returns strings from history.
		/// </summary>
		/// <param name="name">History name. Standard values are: SavedHistory, SavedFolderHistory, SavedViewHistory</param>
		ICollection<string> GetHistory(string name);
		/// <summary>
		/// Show error information
		/// </summary>
		/// <param name="title">Message.</param>
		/// <param name="error">Exception.</param>
		void ShowError(string title, Exception error);
		/// <summary>
		/// Returns new <see cref="IDialog"/>.
		/// </summary>
		/// <param name="left">Left position or -1: auto.</param>
		/// <param name="top">Top line or -1: auto.</param>
		/// <param name="right">Right position or width if Left = -1.</param>
		/// <param name="bottom">Bootom line or height if Top = -1.</param>
		IDialog CreateDialog(int left, int top, int right, int bottom);
		/// <summary>
		///  Writes a text string to the screen.
		/// </summary>
		/// <param name="left">Left position.</param>
		/// <param name="top">Top line.</param>
		/// <param name="foregroundColor">Foreground color.</param>
		/// <param name="backgroundColor">Background color.</param>
		/// <param name="text">Text.</param>
		void WriteText(int left, int top, ConsoleColor foregroundColor, ConsoleColor backgroundColor, string text);
		/// <summary>
		/// Shows the specified topic from a given hlf-file.
		/// </summary>
		/// <param name="path">Help file location, see options.</param>
		/// <param name="topic">Help topic. If this parameter is empty, then the topic "Contents" is used.</param>
		/// <param name="options">Help options describing path and topic.</param>
		/// <remarks>
		/// The topic parameter describes a help topic and can be in one of the following formats:
		/** <code>
"Topic"
Reference to a topic in the plugins help file.

":Topic"
Reference to a topic from the main FAR Manager help file.

"&lt;FullPath&gt;Topic"
Reference to a topic in a help file located in a folder with full or relative
path of FullPath. An ending backslash must be added. The reference must not be
split on multiple lines. For example, the plugin Foo is located in folder
"D:\FAR\Plugins\Foo" and we need to show the topic "FooInfo" from its help file:
"&lt;D:\FAR\Plugins\Foo\&gt;FooInfo"
 
"&lt;FullModuleName&gt;Topic"
Reference to a topic in a help file located in the same folder as the plugin
with the relative or full path of FullModuleName. The reference must not be
split on mutiple lines. For example, we need to show the help topic "FooInfo"
from the help file of the plugin Foo "D:\FAR\Plugins\Foo\Foo.dll":
"&lt;D:\FAR\Plugins\Foo\Foo.dll&gt;FooInfo"
		</code> */
		/// </remarks>
		void ShowHelp(string path, string topic, HelpOptions options);
	}

	/// <summary>
	/// Options for <see cref="IFar.ShowHelp"/>.
	/// </summary>
	[Flags]
	public enum HelpOptions
	{
		/// <summary>
		/// Assume path is Info.ModuleName and show the topic from the help file of the calling plugin (it is Far.NET).
		/// If topic begins with a colon ':', the topic from the main FAR help file is shown and path is ignored.
		/// </summary>
		None = 0x0,
		/// <summary>
		/// Path is ignored and the topic from the main FAR help file is shown.
		/// In this case you do not need to start the topic with a colon ':'.
		/// </summary>
		Far = 1 << 0,
		/// <summary>
		/// Assume path specifies full path to a hlf-file (c:\path\filename).
		/// </summary>
		File = 1 << 1,
		/// <summary>
		/// Assume path specifies full path to a folder (c:\path) from which
		/// a help file is selected according to current language settings.
		/// </summary>
		Path = 1 << 2,
		/// <summary>
		/// If the topic is not found, try to show the "Contents" topic.
		/// This flag can be combined with other flags.
		/// </summary>
		UseContents = 0x40000000,
		/// <summary>
		/// Disable file or topic not found error messages for this function call.
		/// This flag can be combined with other flags.
		/// </summary>
		NoError = unchecked((int)0x80000000),
	}

	/// <summary>
	/// From where IPluginMenuItem was opened from
	/// </summary>
	public enum OpenFrom
	{
		/// <summary>
		/// Disk menu
		/// </summary>
		DiskMenu,
		/// <summary>
		/// Plugins menu (F11)
		/// </summary>
		PluginsMenu,
		/// <summary>
		/// Find list
		/// </summary>
		FindList,
		/// <summary>
		/// Shortcut
		/// </summary>
		Shortcut,
		/// <summary>
		/// Command line
		/// </summary>
		CommandLine,
		/// <summary>
		/// Editor
		/// </summary>
		Editor,
		/// <summary>
		/// Viewer
		/// </summary>
		Viewer,
	};

	/// <summary>
	/// Item of plugins menu (F11)
	/// </summary>
	public interface IPluginMenuItem
	{
		/// <summary>
		/// Is fired when menu item is opened
		/// </summary>
		event EventHandler<OpenPluginMenuItemEventArgs> OnOpen;
		/// <summary>
		/// Name of menu item (Caption)
		/// </summary>
		string Name { get; set; }
		/// <summary>
		/// Fire OnOpen event
		/// </summary>
		/// <param name="sender">IPluginMenuItem hich is opened</param>
		/// <param name="from">from where it is opened</param>
		void FireOnOpen(IPluginMenuItem sender, OpenFrom from);
	}

	/// <summary>
	/// Item of Far disk menu
	/// </summary>
	public interface IDiskMenuItem
	{
		/// <summary>
		/// Name (caption) of disk menu
		/// </summary>
		string Name { get; set; }
		/// <summary>
		/// Number of the item
		/// </summary>
		int Number { get; set; }
		/// <summary>
		/// Event fired when item is opened
		/// </summary>
		event EventHandler OnOpen;
		/// <summary>
		/// Fire Onopen event
		/// </summary>
		/// <param name="sender">IDiskMenuItem of menu</param>
		void FireOnOpen(IDiskMenuItem sender);
	}

	/// <summary>
	/// Delegate which takes 1 string parameter
	/// </summary>
	public delegate void StringDelegate(string s);

	/// <summary>
	/// Arguments of <see cref="IPluginMenuItem.OnOpen"/> event.
	/// </summary>
	public sealed class OpenPluginMenuItemEventArgs : EventArgs
	{
		OpenFrom _from;
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="from">See <see cref="From"/>.</param>
		public OpenPluginMenuItemEventArgs(OpenFrom from)
		{
			_from = from;
		}
		/// <summary>
		/// Where it is called from. See <see cref="OpenFrom"/>.
		/// </summary>
		public OpenFrom From
		{
			get { return _from; }
		}
	}
}
