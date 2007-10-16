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
	/// Interface of FAR Manager.
	/// It is available in a plugin as property <see cref="BasePlugin.Far"/> of <see cref="BasePlugin"/>.
	/// It provides top level access to FAR data and functionality and creates objects like
	/// menus, input and message boxes, dialogs, editors, viewers, panels with
	/// their own properties and methods.
	/// </summary>
	public interface IFar
	{
		/// <summary>
		/// Path to the plugin folder.
		/// </summary>
		string PluginFolderPath { get; }
		/// <summary>
		/// Registers a command line prefix.
		/// It should be called only from <see cref="BasePlugin.Connect"/>.
		/// </summary>
		/// <param name="prefix">Command prefix.</param>
		/// <param name="handler">Handler of a command.</param>
		void RegisterPrefix(string prefix, StringDelegate handler);
		/// <summary>
		/// Adds a menu item to the FAR plugin configuration menu.
		/// It should be called only from <see cref="BasePlugin.Connect"/>.
		/// </summary>
		/// <param name="item">The menu item being registered.</param>
		void RegisterPluginsConfigItem(IPluginMenuItem item);
		/// <summary>
		/// Adds a menu item to the FAR disks menu.
		/// It should be called only from <see cref="BasePlugin.Connect"/>.
		/// </summary>
		/// <param name="item">The menu item being registered.</param>
		void RegisterPluginsDiskItem(IPluginMenuItem item);
		/// <summary>
		/// Adds a menu item to the FAR main plugins menu (F11).
		/// It should be called only from <see cref="BasePlugin.Connect"/>.
		/// </summary>
		/// <param name="item">The menu item being registered.</param>
		void RegisterPluginsMenuItem(IPluginMenuItem item);
		/// <summary>
		/// Adds a menu item to the FAR main plugins menu (F11).
		/// It should be called only from <see cref="BasePlugin.Connect"/>.
		/// </summary>
		/// <param name="name">Name of a menu item.</param>
		/// <param name="onOpen">Handler called on selection of the menu item.</param>
		/// <returns>Created and registered item.</returns>
		IPluginMenuItem RegisterPluginsMenuItem(string name, EventHandler<OpenPluginMenuItemEventArgs> onOpen);
		/// <summary>
		/// Creates a new plugin menu item used by
		/// <see cref="RegisterPluginsConfigItem"/>,
		/// <see cref="RegisterPluginsDiskItem"/>,
		/// <see cref="RegisterPluginsMenuItem(IPluginMenuItem)"/>.
		/// </summary>
		/// <returns>Created plugin menu item.</returns>
		IPluginMenuItem CreatePluginsMenuItem();
		/// <summary>
		/// Shows a message box.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <returns>false if cancelled.</returns>
		bool Msg(string body);
		/// <summary>
		/// Shows a message box.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <param name="header">Message header.</param>
		/// <returns>false if cancelled.</returns>
		bool Msg(string body, string header);
		/// <summary>
		/// Shows a message box with options.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <param name="header">Message header.</param>
		/// <param name="options">Message options.</param>
		/// <returns>Button index or -1 if cancelled.</returns>
		int Msg(string body, string header, MessageOptions options);
		/// <summary>
		/// Shows a message box with options and buttons.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <param name="header">Message header.</param>
		/// <param name="options">Message options.</param>
		/// <param name="buttons">Message buttons.</param>
		/// <returns>Button index or -1 if cancelled.</returns>
		int Msg(string body, string header, MessageOptions options, string[] buttons);
		/// <summary>
		/// Creates a new message box.
		/// You have to set its properties and call <see cref="IMessage.Show"/>.
		/// Note that in most cases using one of <c>Msg</c> methods instead of this is enough.
		/// </summary>
		IMessage CreateMessage();
		/// <summary>
		/// Runs a command with a registered Far.NET prefix.
		/// </summary>
		///<param name="command">Command with a prefix.</param>
		void Run(string command);
		/// <summary>
		/// Handle of FAR window.
		/// </summary>
		int HWnd { get; }
		/// <summary>
		/// FAR version.
		/// </summary>
		Version Version { get; }
		/// <summary>
		/// Creates a new input box.
		/// You have to set its properties and call <see cref="IInputBox.Show"/>.
		/// </summary>
		IInputBox CreateInputBox();
		/// <summary>
		/// Creates a new menu.
		/// You have to set its properties and call <see cref="IMenu.Show"/>.
		/// </summary>		
		IMenu CreateMenu();
		/// <summary>
		/// Virtual editor instance.
		/// Subscribe to its events if you want to handle events of all editors.
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
		/// Creates a new editor.
		/// You have to set its properties and call <see cref="IEditor.Open"/>.
		/// </summary>
		IEditor CreateEditor();
		/// <summary>
		/// Creates a new viewer.
		/// You have to set its properties and call <see cref="IViewer.Open"/>.
		/// </summary>
		IViewer CreateViewer();
		/// <summary>
		/// Posts keys to the FAR keyboard queue. Processing is not displayed.
		/// </summary>
		/// <param name="keys">String of keys.</param>
		void PostKeys(string keys);
		/// <summary>
		/// Posts keys to the FAR keyboard queue.
		/// </summary>
		/// <param name="keys">String of keys.</param>
		/// <param name="disableOutput">Do not display processing on the screen.</param>
		void PostKeys(string keys, bool disableOutput);
		/// <summary>
		/// Posts literal text to the FAR keyboard queue. Processing is not displayed.
		/// </summary>
		/// <param name="text">Literal text. \t, \r, \n, \r\n are translated to [Tab] and [Enter].</param>
		void PostText(string text);
		/// <summary>
		/// Posts literal text to the FAR keyboard queue.
		/// </summary>
		/// <param name="text">Literal text. \t, \r, \n, \r\n are translated to [Tab] and [Enter].</param>
		/// <param name="disableOutput">Do not display processing on the screen.</param>
		void PostText(string text, bool disableOutput);
		/// <summary>
		/// Creates a sequence of key codes from a string of keys.
		/// </summary>
		IList<int> CreateKeySequence(string keys);
		/// <summary>
		/// Posts a sequence of keys to the FAR keyboard queue.
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
		/// You always have to call <see cref="RestoreScreen"/>.
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
		/// For performance sake it redraws only the modified screen area.
		/// But if there was screen output produced by an external program,
		/// it can't calculate this area correctly. In that case you have to
		/// call it with <c>screen</c> = 0 and then with an actual screen handle. 
		/// </remarks>
		void RestoreScreen(int screen);
		/// <summary>
		/// Active editor or null if none.
		/// Normally you have to use this object instantly, do not store it "for later use".
		/// </summary>
		IEditor Editor { get; }
		/// <summary>
		/// Collection of all editors.
		/// Be careful working on not current editors because many
		/// properties and methods are designed for a current editor only.
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
		/// FAR command line.
		/// </summary>
		/// <remarks>
		/// If a plugin is called from the command line (including user menu (F2)
		/// then command line properties and methods may not work correctly; in
		/// this case consider to call a plugin operation from a plugin menu.
		/// Starting from FAR 1.71.2192 you can set the entire command line text
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
		/// VERSION: FAR 1.71.2186.
		/// </summary>
		void GetUserScreen();
		/// <summary>
		/// Returns strings from history.
		/// </summary>
		/// <param name="name">
		/// History name. Standard values are:
		/// SavedHistory, SavedFolderHistory, SavedViewHistory
		/// </param>
		ICollection<string> GetHistory(string name);
		/// <summary>
		/// Shows an error information in a message box.
		/// </summary>
		/// <param name="title">Message.</param>
		/// <param name="error">Exception.</param>
		void ShowError(string title, Exception error);
		/// <summary>
		/// Creates a new dialog.
		/// You have to set its properties, add controls, event handlers and then call <see cref="IDialog.Show"/>.
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
		/// Writes text on the user screen (under panels).
		/// </summary>
		/// <param name="text">Text.</param>
		void Write(string text);
		/// <summary>
		/// Writes colored text on the user screen (under panels).
		/// </summary>
		/// <param name="text">Text.</param>
		/// <param name="foregroundColor">Text color.</param>
		void Write(string text, ConsoleColor foregroundColor);
		/// <summary>
		/// Writes colored text on the user screen (under panels).
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="Colors"]/*'/>
		/// <param name="text">Text.</param>
		void Write(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor);
		/// <summary>
		/// Writes a string at the specified position.
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="LT"]/*'/>
		/// <include file='doc.xml' path='docs/pp[@name="Colors"]/*'/>
		/// <param name="text">Text.</param>
		void WriteText(int left, int top, ConsoleColor foregroundColor, ConsoleColor backgroundColor, string text);
		/// <summary>
		/// Gets existing FAR.NET plugin panel with the specified host type
		/// (see <see cref="IPanelPlugin.Host"/>).
		/// </summary>
		/// <param name="hostType">
		/// Type of the hosting class.
		/// If it is null then any plugin panel is returned.
		/// If it is <c>typeof(object)</c> then any plugin panel having a host is returned.
		/// </param>
		IPanelPlugin GetPanelPlugin(Type hostType);
		/// <summary>
		/// Creates a new panel plugin.
		/// You have to configure it and then open by <see cref="IPanelPlugin.Open()"/>.
		/// </summary>
		IPanelPlugin CreatePanelPlugin();
		/// <summary>
		/// Creates a new empty panel item.
		/// </summary>
		IFile CreatePanelItem();
		/// <summary>
		/// Creates a panel item from <c>FileSystemInfo</c> object.
		/// </summary>
		/// <param name="info">File system item info.</param>
		/// <param name="fullName">Use full name for panel item name.</param>
		IFile CreatePanelItem(FileSystemInfo info, bool fullName);
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
		/// <summary>
		/// Where plugin is opened from.
		/// </summary>
		OpenFrom From { get; }
		/// <summary>
		/// Registry root key of FAR settings taking into account a user (command line parameter /u).
		/// </summary>
		string RootFar { get; }
		/// <summary>
		/// Registry root key, where plugins can save their parameters.
		/// Do not save parameters directly in this key, create your own subkey here
		/// or use <see cref="GetPluginValue"/> and <see cref="SetPluginValue"/>.
		/// </summary>
		string RootKey { get; }
		/// <summary>
		/// Gets a plugin value from the registry.
		/// </summary>
		/// <param name="pluginName">Plugin and registry key name. The key is created if it does not exist.</param>
		/// <param name="valueName">Value name.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>Found or default value.</returns>
		object GetPluginValue(string pluginName, string valueName, object defaultValue);
		/// <summary>
		/// Sets a plugin value in the registry.
		/// </summary>
		/// <param name="pluginName">Plugin and registry key name. The key is created if it does not exist.</param>
		/// <param name="valueName">Value name.</param>
		/// <param name="newValue">New value to be set.</param>
		void SetPluginValue(string pluginName, string valueName, object newValue);
		/// <summary>
		/// Count of open FAR windows. [ACTL_GETWINDOWCOUNT]
		/// </summary>
		/// <remarks>
		/// There is at least one window (panels, editor or viewer).
		/// </remarks>
		int WindowCount { get; }
		/// <summary>
		/// Allows to switch to a specific FAR Manager window. [ACTL_SETCURRENTWINDOW]
		/// </summary>
		/// <param name="index">Window index. See <see cref="WindowCount"/>.</param>
		/// <remarks>
		/// The switching will not occur untill <see cref="Commit"/> is called or FAR receives control.
		/// </remarks>
		void SetCurrentWindow(int index);
		/// <summary>
		/// "Commits" the results of the last operation with FAR windows
		/// (e.g. <see cref="SetCurrentWindow"/>). [ACTL_COMMIT]
		/// </summary>
		/// <returns>true on success.</returns>
		bool Commit();
		/// <summary>
		/// Gets information about a FAR Manager window. [ACTL_GETWINDOWINFO ACTL_GETSHORTWINDOWINFO]
		/// </summary>
		/// <param name="index">Window index; -1 ~ current. See <see cref="WindowCount"/>.</param>
		/// <param name="full">
		/// If it is false <see>IWindowInfo.Name</see> and <see>IWindowInfo.TypeName</see> are not filled.
		/// </param>
		IWindowInfo GetWindowInfo(int index, bool full);
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
		/// <summary>
		/// Other event (actually can be from editor, viewer, command line, etc.)
		/// </summary>
		Other
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
			_From = from;
		}
		/// <summary>
		/// Where it is called from. See <see cref="OpenFrom"/>.
		/// </summary>
		public OpenFrom From
		{
			get { return _From; }
		}
		OpenFrom _From;
		/// <summary>
		/// Tells to ignore results, e.g. when configuration dialog is cancelled.
		/// </summary>
		public bool Ignore
		{
			get { return _Ignore; }
			set { _Ignore = value; }
		}
		bool _Ignore;
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

	/// <summary>
	/// FAR window types. [WINDOWINFO_TYPE]
	/// </summary>
	public enum WindowType
	{
		/// <summary>
		/// Dummy.
		/// </summary>
		None = 0,
		/// <summary>
		/// File panels.
		/// </summary>
		Panels = 1,
		/// <summary>
		/// Internal viewer window.
		/// </summary>
		Viewer,
		/// <summary>
		/// Internal editor window.
		/// </summary>
		Editor,
		/// <summary>
		/// Dialog.
		/// </summary>
		Dialog,
		/// <summary>
		/// Menu.
		/// </summary>
		Menu,
		/// <summary>
		/// Help window.
		/// </summary>
		Help
	}

	/// <summary>
	/// Contains information about one FAR window. See <see cref="IFar.GetWindowInfo"/>. [WindowInfo]
	/// </summary>
	public interface IWindowInfo
	{
		/// <summary>
		/// Window type.
		/// </summary>
		WindowType Type { get; }
		/// <summary>
		/// Modification flag. Valid only for editor window.
		/// </summary>
		bool Modified { get; }
		/// <summary>
		/// Is the window active?
		/// </summary>
		bool Current { get; }
		/// <summary>
		/// Name of the window type, depends on the current FAR language.
		/// </summary>
		string TypeName { get; }
		/// <summary>
		/// Window title:
		/// viewer\editor: the file name;
		/// panels: selected file name;
		/// help: HLF file path;
		/// menu\dialog: header.
		/// </summary>
		string Name { get; }
	}
}
