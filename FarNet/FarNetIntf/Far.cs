/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

using FarManager.Forms;
using System.Collections.Generic;
using System.IO;
using System;

namespace FarManager
{
	/// <summary>
	/// Interface of Far manager. It is available in your plugin as property <see cref="BasePlugin.Far"/> of <see cref="BasePlugin"/>.
	/// It provides access to general Far data and functionality and creates UI and other objects like
	/// menus, input and message boxes, dialogs, editors, viewers and etc.
	/// </summary>
	public interface IFar
	{
		/// <summary>
		/// Path to the plugin folder.
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
		/// Adds a menu item to the Far disks menu.
		/// </summary>
		/// <param name="item">The menu item being registered.</param>
		/// <seealso cref="UnregisterPluginsDiskItem"/>
		void RegisterPluginsDiskItem(IPluginMenuItem item);
		/// <summary>
		/// Adds a menu item to the Far main plugins menu (F11).
		/// </summary>
		/// <param name="item">The menu item being registered.</param>
		/// <seealso cref="UnregisterPluginsMenuItem"/>
		void RegisterPluginsMenuItem(IPluginMenuItem item);
		/// <summary>
		/// Adds a menu item to the Far main plugins menu (F11).
		/// </summary>
		/// <param name="name">Name of a menu item.</param>
		/// <param name="onOpen">Handler called on selection of the menu item.</param>
		/// <returns>newly created item</returns>
		/// <seealso cref="UnregisterPluginsMenuItem"/>
		IPluginMenuItem RegisterPluginsMenuItem(string name, EventHandler<OpenPluginMenuItemEventArgs> onOpen);
		/// <summary>
		/// Call it to unregister the plugin disk item.
		/// </summary>
		/// <param name="item">Item being unregistered.</param>
		void UnregisterPluginsDiskItem(IPluginMenuItem item);
		/// <summary>
		/// Call it to unregister the plugin menu item.
		/// </summary>
		/// <param name="item">Item being unregistered.</param>
		void UnregisterPluginsMenuItem(IPluginMenuItem item);
		/// <summary>
		/// Creates a new plugin menu item.
		/// </summary>
		/// <returns>Created plugin menu item.</returns>
		/// <seealso cref="RegisterPluginsMenuItem(IPluginMenuItem)"/>
		/// <seealso cref="UnregisterPluginsMenuItem"/>
		IPluginMenuItem CreatePluginsMenuItem();
		/// <summary>
		/// Shows a message box.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <returns>True on Enter.</returns>
		bool Msg(string body);
		/// <summary>
		/// Shows a message box.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <param name="header">Message header.</param>
		/// <returns>True on Enter.</returns>
		bool Msg(string body, string header);
		/// <summary>
		/// Shows a message box with options.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <param name="header">Message header.</param>
		/// <param name="options">Message options.</param>
		/// <returns>Button index or -1 on Escape.</returns>
		int Msg(string body, string header, MessageOptions options);
		/// <summary>
		/// Shows a message box with options and buttons.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <param name="header">Message header.</param>
		/// <param name="options">Message options.</param>
		/// <param name="buttons">Message buttons.</param>
		/// <returns>Button index or -1 on Escape.</returns>
		int Msg(string body, string header, MessageOptions options, string[] buttons);
		/// <summary>
		/// Creates a new message box (<see cref="IMessage"/>).
		/// You have to set its properties and call <see cref="IMessage.Show"/>.
		/// Note that in most cases using one of <c>Msg</c> methods instead of this is enough.
		/// </summary>
		IMessage CreateMessage();
		/// <summary>
		/// Run specified command line (works only with Far.Net registered prefixes)
		/// </summary>
		///<param name="cmdLine">Command line</param>
		void Run(string cmdLine);
		/// <summary>
		/// Windows handle of Far
		/// </summary>
		int HWnd { get; }
		/// <summary>
		/// Far version
		/// </summary>
		Version Version { get; }
		/// <summary>
		/// Creates a new input box (<see cref="IInputBox"/>).
		/// You have to set its properties and call <see cref="IInputBox.Show"/>.
		/// </summary>
		IInputBox CreateInputBox();
		/// <summary>
		/// Creates a new menu (<see cref="IMenu"/>).
		/// You have to set its properties and call <see cref="IMenu.Show"/>.
		/// </summary>		
		IMenu CreateMenu();
		/// <summary>
		/// Virtual editor instance. Subscribe to its events when you want be 
		/// informed about all editors events.
		/// </summary>
		IAnyEditor AnyEditor { get; }
		/// <summary>
		/// String of word delimiters used in editors.
		/// </summary>
		string WordDiv { get; }
		/// <summary>
		/// Clipboard text.
		/// </summary>		
		string Clipboard { get; set; }
		/// <summary>
		/// Creates a new not yet opened editor.
		/// You have to set its properties and call <see cref="IEditor.Open"/>.
		/// </summary>
		IEditor CreateEditor();
		/// <summary>
		/// Creates a new not yet opened viewer.
		/// You have to set its properties and call <see cref="IViewer.Open"/>.
		/// </summary>
		IViewer CreateViewer();
		/// <summary>
		/// Posts keys to the Far keyboard queue.
		/// </summary>
		/// <param name="keys">String of keys.</param>
		/// <param name="disableOutput">Do not display processing on the screen.</param>
		void PostKeys(string keys, bool disableOutput);
		/// <summary>
		/// Posts literal text to the Far keyboard queue.
		/// </summary>
		/// <param name="text">Literal text. \t, \r, \n, \r\n are supported, too.</param>
		/// <param name="disableOutput">Do not display processing on the screen.</param>
		void PostText(string text, bool disableOutput);
		/// <summary>
		/// Creates a sequence of key codes from a string of keys.
		/// </summary>
		IList<int> CreateKeySequence(string keys);
		/// <summary>
		/// Posts a sequence of keys to the Far keyboard queue.
		/// </summary>
		/// <param name="sequence">Sequence of keys.</param>
		/// <param name="disableOutput">Do not display processing on the screen.</param>
		void PostKeySequence(IList<int> sequence, bool disableOutput);
		/// <summary>
		/// Converts a key string representation to the key code.
		/// </summary>
		int NameToKey(string key);
		/// <summary>
		/// Saves screen area.
		/// You have to always call <see cref="RestoreScreen"/>.
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="LTRB"]/*'/>
		/// <returns>A handle for restoring the screen.</returns>
		/// <remarks>
		/// If <c>right</c> and <c>bottom</c> are equal to -1,
		/// they are replaced with screen right and bottom coordinates.
		/// So <c>SaveScreen(0,0,-1,-1)</c> will save the entire screen.
		/// </remarks>
		int SaveScreen(int left, int top, int right, int bottom);
		/// <summary>
		/// Restores previously saved by <see cref="SaveScreen"/> screen area.
		/// </summary>
		/// <param name="screen">
		/// A handle received from <c>SaveScreen</c>.
		/// This handle is no longer usable after calling.
		/// </param>
		/// <remarks>
		/// To improve speed it redraws only the modified screen area.
		/// But if there was screen output produced by an external program, it can not correctly calculate this area.
		/// In that case you need first to call it <c>screen</c> = 0 and then call it as usually with screen handle. 
		/// </remarks>
		void RestoreScreen(int screen);
		/// <summary>
		/// Active editor or null if none.
		/// Normally you have to use this object instantly, i.e. do not keep it "for later use".
		/// </summary>
		IEditor Editor { get; }
		/// <summary>
		/// Collection of all editors.
		/// Be extremely careful working on not current editors: actually it is not recommended at all.
		/// </summary>
		ICollection<IEditor> Editors { get; }
		/// <summary>
		/// Current (active) panel.
		/// If it is a FAR.NET plugin panel it returns <see cref="IPanelPlugin"/>.
		/// </summary>
		IPanel Panel { get; }
		/// <summary>
		/// Another (passive) panel.
		/// If it is a FAR.NET plugin panel it returns <see cref="IPanelPlugin"/>.
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
		/// Copies the current screen contents to the Far user screen buffer
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
		/// Shows an error information in a message box.
		/// </summary>
		/// <param name="title">Message.</param>
		/// <param name="error">Exception.</param>
		void ShowError(string title, Exception error);
		/// <summary>
		/// Creates a new dialog (<see cref="IDialog"/>).
		/// You have to set its properties, add controls, add event handlers and then call <see cref="IDialog.Show"/>.
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="LTRB"]/*'/>
		/// <remarks>
		/// You can set <c>left</c> = -1 or <c>top</c> = -1 to be auto-calculated.
		/// In this case <c>right</c> or <c>bottom</c> should be width and height.
		/// </remarks>
		IDialog CreateDialog(int left, int top, int right, int bottom);
		/// <include file='doc.xml' path='docs/pp[@name="ShowHelp"]/*'/>
		void ShowHelp(string path, string topic, HelpOptions options);
		/// <summary>
		///  Writes text on the user screen (under panels).
		/// </summary>
		/// <param name="text">Text.</param>
		void Write(string text);
		/// <summary>
		///  Writes colored text on the user screen (under panels).
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="Colors"]/*'/>
		/// <param name="text">Text.</param>
		void Write(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor);
		/// <summary>
		///  Writes a string at the specified position.
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="LT"]/*'/>
		/// <include file='doc.xml' path='docs/pp[@name="Colors"]/*'/>
		/// <param name="text">Text.</param>
		void WriteText(int left, int top, ConsoleColor foregroundColor, ConsoleColor backgroundColor, string text);
		/// <summary>
		/// Gets FAR.NET panel plugin of the specified host type (see <see cref="IPanelPlugin.Host"/>).
		/// </summary>
		/// <param name="hostType">
		/// Type of the hosting class.
		/// If null any existing plugin is returned.
		/// If typeof(object) a plugin having any host is returned.
		/// </param>
		IPanelPlugin GetPanelPlugin(Type hostType);
		/// <summary>
		/// Creates new not yet opened panel plugin. You have to open it by <see cref="IPanelPlugin.Open()"/>
		/// </summary>
		IPanelPlugin CreatePanelPlugin();
		/// <summary>
		/// Creates a panel item.
		/// </summary>
		IFile CreatePanelItem();
		/// <summary>
		/// Creates a panel item from <c>FileSystemInfo</c> object.
		/// </summary>
		/// <param name="info">File system item info.</param>
		/// <param name="fullName">Use full name for panel item name.</param>
		IFile CreatePanelItem(FileSystemInfo info, bool fullName);
		/// <summary>
		/// Closes the current plugin panel. [FCTL_CLOSEPLUGIN]
		/// </summary>
		/// <param name="path">
		/// Name of the directory that will be set in the panel after closing the plugin (or {null|empty}).
		/// If the path doesn't exist FAR shows an error.
		/// </param>
		void ClosePanel(string path);
		/// <summary>
		/// Confirmation settings according to options in the "Confirmations" dialog. [ACTL_GETCONFIRMATIONS]
		/// </summary>
		FarConfirmations Confirmations { get; }
		/// <include file='doc.xml' path='docs/pp[@name="Include"]/*'/>
		/// <param name="prompt">Prompt text.</param>
		/// <returns>Entered text or null if cancelled.</returns>
		string Input(string prompt);
		/// <include file='doc.xml' path='docs/pp[@name="Include"]/*'/>
		/// <param name="prompt">Prompt text.</param>
		/// <param name="history">History string.</param>
		/// <returns>Entered text or null if cancelled.</returns>
		string Input(string prompt, string history);
		/// <include file='doc.xml' path='docs/pp[@name="Include"]/*'/>
		/// <param name="prompt">Prompt text.</param>
		/// <param name="history">History string.</param>
		/// <param name="title">Title of the box.</param>
		/// <returns>Entered text or null if cancelled.</returns>
		string Input(string prompt, string history, string title);
		/// <include file='doc.xml' path='docs/pp[@name="Include"]/*'/>
		/// <param name="prompt">Prompt text.</param>
		/// <param name="history">History string.</param>
		/// <param name="title">Title of the box.</param>
		/// <param name="text">Text to be edited.</param>
		/// <returns>Entered text or null if cancelled.</returns>
		string Input(string prompt, string history, string title, string text);
	}

	/// <summary>
	/// Options for <see cref="IFar.ShowHelp"/>.
	/// </summary>
	[Flags]
	public enum HelpOptions
	{
		/// <summary>
		/// Assume path is Info.ModuleName and show the topic from the help file of the calling plugin (it is Far.NET).
		/// If topic begins with a colon ':', the topic from the main Far help file is shown and path is ignored.
		/// </summary>
		None = 0x0,
		/// <summary>
		/// Path is ignored and the topic from the main Far help file is shown.
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
	/// Where plugin is opened from.
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
	/// Item of plugins menu (F11).
	/// </summary>
	public interface IPluginMenuItem
	{
		/// <summary>
		/// Is fired when menu item is opened.
		/// </summary>
		event EventHandler<OpenPluginMenuItemEventArgs> OnOpen;
		/// <summary>
		/// Name of menu item (caption in plugins menu).
		/// </summary>
		string Name { get; set; }
	}

	/// <summary>
	/// Delegate which takes a string parameter.
	/// </summary>
	public delegate void StringDelegate(string s);

	/// <summary>
	/// Arguments of <see cref="IPluginMenuItem.OnOpen"/> event.
	/// </summary>
	public sealed class OpenPluginMenuItemEventArgs : EventArgs
	{
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
		OpenFrom _from;
	}

	/// <summary>
	/// Information about the confirmation settings.
	/// Corresponds to options in the "Confirmations" dialog.
	/// </summary>
	[Flags]
	public enum FarConfirmations
	{
		/// <summary>
		/// Nothing.
		/// </summary>
		None = 0,
		/// <summary>
		/// Overwrite files when copying.
		/// </summary>
		CopyOverwrite = 0x1,
		/// <summary>
		/// Overwritte files when moving.
		/// </summary>
		MoveOverwrite = 0x2,
		/// <summary>
		/// Drag and drop.
		/// </summary>
		DragAndDrop = 0x4,
		/// <summary>
		/// Delete.
		/// </summary>
		Delete = 0x8,
		/// <summary>
		/// Delete non-empty folders.
		/// </summary>
		DeleteNonEmptyFolders = 0x10,
		/// <summary>
		/// Interrupt operation.
		/// </summary>
		InterruptOperation = 0x20,
		/// <summary>
		/// Disconnect network drive.
		/// </summary>
		DisconnectNetworkDrive = 0x40,
		/// <summary>
		/// Reload edited file.
		/// </summary>
		ReloadEditedFile = 0x80,
		/// <summary>
		/// Clear history list.
		/// </summary>
		ClearHistoryList = 0x100,
		/// <summary>
		/// Exit
		/// </summary>
		Exit = 0x200,
	}
}
