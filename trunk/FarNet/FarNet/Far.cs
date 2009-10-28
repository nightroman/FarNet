/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

using System;
using System.Collections.Generic;
using System.IO;
using FarNet.Forms;
using FarNet.Support;

namespace FarNet
{
	/// <summary>
	/// Interface of Far Manager.
	/// It is exposed for plugin derived classes as property <see cref="BasePlugin.Far"/>.
	/// It provides access to top level Far methods and objects or creates new Far objects like
	/// menus, input and message boxes, dialogs, editors, viewers, panels and etc.
	/// Further operations are performed on that objects.
	/// </summary>
	public interface IFar
	{
		/// <summary>
		/// Registers a handler invoked from one of Far menus.
		/// </summary>
		/// <param name="plugin">Plugin instance. It can be null, but is not recommended for standard cases.</param>
		/// <param name="name">Tool name and also the default menu item name. Recommended to be a unique name in the assembly.</param>
		/// <param name="handler">Tool handler.</param>
		/// <param name="options">Tool options.</param>
		void RegisterTool(BasePlugin plugin, string name, EventHandler<ToolEventArgs> handler, ToolOptions options);
		/// <summary>
		/// Unregisters a tool handler.
		/// </summary>
		/// <param name="handler">Tool handler.</param>
		void UnregisterTool(EventHandler<ToolEventArgs> handler);
		/// <summary>
		/// Registers a handler invoked from the command line by its prefix.
		/// If a plugin uses this prefix itself then it should use the returned value instead.
		/// </summary>
		/// <param name="plugin">Plugin instance. It can be null, but is not recommended for standard cases.</param>
		/// <param name="name">Command name and also the config menu item name. Recommended to be a unique name in the assembly.</param>
		/// <param name="prefix">Command prefix, see remarks.</param>
		/// <param name="handler">Command handler.</param>
		/// <returns>Actual prefix that is used by FarNet for this command.</returns>
		/// <remarks>
		/// Provided <c>prefix</c> is only a default suggestion, actual prefix used by FarNet can be different,
		/// e.g. changed by a user, so that use the returned prefix if the plugin uses it. Note that the plugin
		/// is not notified about prefix changes during the session. If it is really important (rare) then use
		/// <see cref="CommandPlugin"/> which can always have a fresh prefix set by a user.
		/// </remarks>
		string RegisterCommand(BasePlugin plugin, string name, string prefix, EventHandler<CommandEventArgs> handler);
		/// <summary>
		/// Unregisters a command handler.
		/// </summary>
		/// <param name="handler">Command handler.</param>
		void UnregisterCommand(EventHandler<CommandEventArgs> handler);
		/// <summary>
		/// Registers a handler invoked for a file. See <see cref="FilerEventArgs"/>.
		/// </summary>
		/// <param name="plugin">Plugin instance. It can be null, but is not recommended for standard cases.</param>
		/// <param name="name">Filer name and also the config menu items. Recommended to be a unique name in the assembly.</param>
		/// <param name="handler">Filer handler.</param>
		/// <param name="mask">File(s) mask, see <see cref="FilerPlugin.Mask"/>.</param>
		/// <param name="creates">Tells that the plugin also creates files.</param>
		void RegisterFiler(BasePlugin plugin, string name, EventHandler<FilerEventArgs> handler, string mask, bool creates);
		/// <summary>
		/// Unregisters a file handler.
		/// </summary>
		/// <param name="handler">Filer handler.</param>
		void UnregisterFiler(EventHandler<FilerEventArgs> handler);
		/// <summary>
		/// Unregisters a base plugin. This method should be called only in critical cases.
		/// </summary>
		void Unregister(BasePlugin plugin);
		/// <summary>
		/// Path of the plugin directory.
		/// </summary>
		string PluginPath { get; }
		/// <summary>
		/// Shows a message box.
		/// </summary>
		/// <param name="body">Message text.</param>
		void Msg(string body);
		/// <summary>
		/// Shows a message box.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <param name="header">Message header.</param>
		void Msg(string body, string header);
		/// <summary>
		/// Shows a message box with options.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <param name="header">Message header.</param>
		/// <param name="options">Message options.</param>
		/// <returns>Button index or -1 if cancelled.</returns>
		int Msg(string body, string header, MsgOptions options);
		/// <summary>
		/// Shows a message box with options and buttons.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <param name="header">Message header.</param>
		/// <param name="options">Message options.</param>
		/// <param name="buttons">Message buttons.</param>
		/// <returns>Button index or -1 if cancelled.</returns>
		int Msg(string body, string header, MsgOptions options, string[] buttons);
		/// <summary>
		/// Shows a message box with options, buttons and help.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <param name="header">Message header.</param>
		/// <param name="options">Message options.</param>
		/// <param name="buttons">Message buttons.</param>
		/// <param name="helpTopic">
		/// <include file='doc.xml' path='docs/pp[@name="HelpTopic"]/*'/>
		/// </param>
		/// <returns>Button index or -1 if cancelled.</returns>
		/// <remarks>
		/// In extreme cases when a message contains too many or too long buttons, then a message
		/// box is converted into a listbox dialog where listbox items work as buttons.
		/// </remarks>
		int Msg(string body, string header, MsgOptions options, string[] buttons, string helpTopic);
		/// <summary>
		/// Runs a command with a registered FarNet prefix.
		/// </summary>
		///<param name="command">Command with a prefix.</param>
		void Run(string command);
		/// <summary>
		/// Handle of Far window.
		/// </summary>
		IntPtr HWnd { get; }
		/// <summary>
		/// Far version.
		/// </summary>
		Version FarVersion { get; }
		/// <summary>
		/// FarNet version.
		/// </summary>
		Version FarNetVersion { get; }
		/// <summary>
		/// Creates a new input box.
		/// You have to set its properties and call <see cref="IInputBox.Show"/>.
		/// </summary>
		IInputBox CreateInputBox();
		/// <summary>
		/// Creates a new standard Far menu.
		/// You have to set its properties and call <see cref="IAnyMenu.Show"/>.
		/// </summary>
		IMenu CreateMenu();
		/// <summary>
		/// Creates a new menu implemented with <see cref="IListBox"/>.
		/// You have to set its properties and call <see cref="IAnyMenu.Show"/>.
		/// </summary>
		IListMenu CreateListMenu();
		/// <summary>
		/// Virtual editor instance.
		/// Subscribe to its events if you want to handle events of all editors.
		/// </summary>
		IAnyEditor AnyEditor { get; }
		/// <summary>
		/// Virtual viewer instance.
		/// Subscribe to its events if you want to handle events of all viewers.
		/// </summary>
		IAnyViewer AnyViewer { get; }
		/// <summary>
		/// Gets the clipboard text.
		/// </summary>
		string PasteFromClipboard();
		/// <summary>
		/// Sets the clipboard text.
		/// </summary>
		void CopyToClipboard(string text);
		/// <summary>
		/// Creates a new editor.
		/// You have to set its properties and call <see cref="IEditor.Open(OpenMode)"/>.
		/// </summary>
		IEditor CreateEditor();
		/// <summary>
		/// Creates a new viewer.
		/// You have to set its properties and call <see cref="IViewer.Open(OpenMode)"/>.
		/// </summary>
		IViewer CreateViewer();
		/// <summary>
		/// Posts keys to the Far keyboard queue. Processing is not displayed.
		/// </summary>
		/// <param name="keys">String of keys.</param>
		void PostKeys(string keys);
		/// <summary>
		/// Posts keys to the Far keyboard queue.
		/// </summary>
		/// <param name="keys">String of keys.</param>
		/// <param name="disableOutput">Do not display processing on the screen.</param>
		void PostKeys(string keys, bool disableOutput);
		/// <summary>
		/// Posts literal text to the Far keyboard queue. Processing is not displayed.
		/// </summary>
		/// <param name="text">Literal text. \t, \r, \n, \r\n are translated to [Tab] and [Enter].</param>
		void PostText(string text);
		/// <summary>
		/// Posts literal text to the Far keyboard queue.
		/// </summary>
		/// <param name="text">Literal text. \t, \r, \n, \r\n are translated to [Tab] and [Enter].</param>
		/// <param name="disableOutput">Do not display processing on the screen.</param>
		void PostText(string text, bool disableOutput);
		/// <summary>
		/// Creates a sequence of key codes from a string of keys.
		/// </summary>
		int[] CreateKeySequence(string keys);
		/// <summary>
		/// Posts a sequence of keys to the Far keyboard queue.
		/// Processing is not displayed.
		/// </summary>
		/// <param name="sequence">Sequence of keys.</param>
		void PostKeySequence(int[] sequence);
		/// <summary>
		/// Posts a sequence of keys to the Far keyboard queue.
		/// </summary>
		/// <param name="sequence">Sequence of keys.</param>
		/// <param name="disableOutput">Do not display processing on the screen.</param>
		void PostKeySequence(int[] sequence, bool disableOutput);
		/// <summary>
		/// Converts a key string representation to the internal <see cref="KeyCode"/>. Returns -1 on errors.
		/// </summary>
		int NameToKey(string key);
		/// <summary>
		/// Converts an internal <see cref="KeyCode"/> to string representation. Returns null on errors.
		/// </summary>
		string KeyToName(int key);
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
		/// Normally you have to use this object instantly.
		/// </summary>
		IEditor Editor { get; }
		/// <summary>
		/// Active viewer or null if none.
		/// Normally you have to use this object instantly.
		/// </summary>
		IViewer Viewer { get; }
		/// <summary>
		/// All editors.
		/// Be careful working with not current instance.
		/// </summary>
		IEditor[] Editors();
		/// <summary>
		/// All viewers.
		/// Be careful working with not current instance.
		/// </summary>
		IViewer[] Viewers();
		/// <summary>
		/// Active panel. It is null if Far started with /e or /v.
		/// </summary>
		/// <remarks>
		/// If it is a FarNet panel it returns <see cref="IPluginPanel"/>, you can keep this, but remember its state may change.
		/// Else it points to an active panel no matter if it changes, so that use this reference instantly.
		/// </remarks>
		IPanel Panel { get; }
		/// <summary>
		/// Passive panel. It is null if Far started with /e or /v.
		/// </summary>
		/// <remarks>
		/// If it is a FarNet panel it returns <see cref="IPluginPanel"/>, you can keep this, but remember its state may change.
		/// Else it points to a passive panel no matter if it changes, so that use this reference instantly.
		/// </remarks>
		IPanel Panel2 { get; }
		/// <summary>
		/// The command line instance.
		/// </summary>
		/// <remarks>
		/// If a plugin is called from the command line (including user menu (F2)
		/// then command line properties and methods may not work correctly; in
		/// this case consider to call a plugin operation from a plugin menu.
		/// Starting from Far 1.71.2192 you can set the entire command line text
		/// if you call a plugin from the command line (but not from a user menu).
		/// </remarks>
		ILine CommandLine { get; }
		/// <summary>
		/// Copies the current screen contents to the user screen buffer
		/// (which is displayed when the panels are switched off).
		/// </summary>
		void SetUserScreen();
		/// <summary>
		/// Copies the current user screen buffer to console screen
		/// (which is displayed when the panels are switched off).
		/// Far 1.71.2186.
		/// </summary>
		void GetUserScreen();
		/// <summary>
		/// Returns all strings from history.
		/// </summary>
		/// <param name="name">History name. Standard values are: SavedHistory, SavedFolderHistory, SavedViewHistory.</param>
		ICollection<string> GetHistory(string name);
		/// <summary>
		/// Returns strings from history by type.
		/// </summary>
		/// <param name="name">History name. Standard values are: SavedHistory, SavedFolderHistory, SavedViewHistory.</param>
		/// <param name="filter">
		/// Type filter: each character represents a type. For example for SavedViewHistory: 0: view; 1: edit; 2: external.
		/// </param>
		ICollection<string> GetHistory(string name, string filter);
		/// <summary>
		/// Returns strings from dialog control history.
		/// </summary>
		/// <param name="name">History name.</param>
		ICollection<string> GetDialogHistory(string name);
		/// <summary>
		/// Shows an error information in a message box which also stops any macro.
		/// </summary>
		/// <param name="title">Message.</param>
		/// <param name="exception">Exception.</param>
		/// <remarks>
		/// For safety sake: avoiding unexpected results on exceptions during a running
		/// macro this method stops a macro before showing an error dialog. That is why
		/// it should be called only in exceptional situations.
		/// </remarks>
		void ShowError(string title, Exception exception);
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
		/// <summary>
		/// Creates a dialog for selecting a subset of items.
		/// </summary>
		ISubsetForm CreateSubsetForm();
		/// <include file='doc.xml' path='docs/pp[@name="ShowHelp"]/*'/>
		void ShowHelp(string path, string topic, HelpOptions options);
		/// <summary>
		/// Writes text on the user screen (under panels).
		/// </summary>
		/// <param name="text">Text.</param>
		/// <remarks>
		/// Avoid <c>Console.Write*</c> methods.
		/// </remarks>
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
		/// Writes a string at the specified position using Far palette colors.
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="LT"]/*'/>
		/// <param name="paletteColor">Palette color.</param>
		/// <param name="text">Text.</param>
		void WritePalette(int left, int top, PaletteColor paletteColor, string text);
		/// <summary>
		/// Writes a string at the specified position with defined colors.
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="LT"]/*'/>
		/// <include file='doc.xml' path='docs/pp[@name="Colors"]/*'/>
		/// <param name="text">Text.</param>
		/// <seealso cref="IFar.GetPaletteForeground"/>
		/// <seealso cref="IFar.GetPaletteBackground"/>
		void WriteText(int left, int top, ConsoleColor foregroundColor, ConsoleColor backgroundColor, string text);
		/// <summary>
		/// Gets existing FarNet plugin panel with the specified host type
		/// (see <see cref="IPluginPanel.Host"/>).
		/// </summary>
		/// <param name="hostType">
		/// Type of the hosting class.
		/// If it is null then any plugin panel is returned.
		/// If it is <c>typeof(object)</c> then any plugin panel having a host is returned.
		/// </param>
		IPluginPanel GetPluginPanel(Type hostType);
		/// <summary>
		/// Gets existing FarNet plugin panel with the specified ID or returns null.
		/// </summary>
		/// <param name="id">Panel ID. It is normally assigned by a creator.</param>
		/// <seealso cref="IPluginPanel.TypeId"/>
		IPluginPanel GetPluginPanel(Guid id);
		/// <summary>
		/// Creates a new panel.
		/// If it is opened on the same plugin call then call <see cref="IPluginPanel.Open()"/>
		/// as soon as possible and then only configure the panel and other data.
		/// </summary>
		IPluginPanel CreatePluginPanel();
		/// <summary>
		/// Confirmation settings according to options in the "Confirmations" dialog.
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
		/// Registry root key of Far settings taking into account a user (command line parameter /u).
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
		/// <param name="pluginName">Plugin name (registry key).</param>
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
		/// Count of open Far windows.
		/// </summary>
		/// <remarks>
		/// There is at least one window (panels, editor or viewer).
		/// </remarks>
		int WindowCount { get; }
		/// <summary>
		/// Allows to switch to a specific Far Manager window.
		/// </summary>
		/// <param name="index">Window index. See <see cref="WindowCount"/>.</param>
		/// <remarks>
		/// The switching will not occur untill <see cref="Commit"/> is called or Far receives control.
		/// </remarks>
		void SetCurrentWindow(int index);
		/// <summary>
		/// "Commits" the results of the last operation with Far windows
		/// (e.g. <see cref="SetCurrentWindow"/>).
		/// </summary>
		/// <returns>true on success.</returns>
		bool Commit();
		/// <summary>
		/// Gets information about a Far Manager window.
		/// </summary>
		/// <param name="index">Window index; -1 ~ current. See <see cref="WindowCount"/>.</param>
		/// <param name="full">
		/// If it is false <see>IWindowInfo.Name</see> and <see>IWindowInfo.TypeName</see> are not filled.
		/// </param>
		IWindowInfo GetWindowInfo(int index, bool full);
		/// <summary>
		/// Type of the current window, the same as <see cref="GetWindowType"/> with parameter -1.
		/// </summary>
		WindowType WindowType { get; }
		/// <summary>
		/// Gets type of a window specified by an index.
		/// </summary>
		/// <param name="index">
		/// Window index or -1 for the current window, same as <see cref="WindowType"/>.
		/// See <see cref="WindowCount"/>.
		/// </param>
		WindowType GetWindowType(int index);
		/// <summary>
		/// Converts an internal key code to a 'printable' char. <see cref="KeyCode"/>
		/// </summary>
		/// <remarks>
		/// If the code does not correspond to a 'printable' char then 0 is returned.
		/// Note: chars below space are returned as they are because they are sort of 'printable'.
		/// </remarks>
		char CodeToChar(int code);
		/// <summary>
		/// Shows FarNet panel menu.
		/// </summary>
		/// <param name="showPushCommand">Show "Push" command.</param>
		void ShowPanelMenu(bool showPushCommand);
		/// <summary>
		/// Posts a handler to be invoked when user code has finished and Far gets control.
		/// </summary>
		/// <param name="handler">Step handler.</param>
		/// <remarks>
		/// Many Far operations are executed only when Far gets control, i.e. when user code has finished.
		/// Thus, normally you can not performs several such operations together. This method may help.
		/// <para>
		/// This mechanism work only when plugins menu is available ([F11]), because it is used internally for stepping.
		/// Ensure any FarNet hotkey in the Far plugins menu. Use [F11] for plugins menu, [F4] to set a hotkey there.
		/// </para>
		/// <para>
		/// If a step handler starts modal UI without exiting (e.g. dialog) then use <see cref="PostStepAfterStep"/>
		/// if you have another step to be invoked in modal mode (e.g. in a dialog after opening).
		/// </para>
		/// </remarks>
		void PostStep(EventHandler handler);
		/// <summary>
		/// Posts the keys that normally start modal UI and a handler which is invoked in that modal mode.
		/// </summary>
		/// <param name="keys">Keys starting modal UI.</param>
		/// <param name="handler">Handler to be called in modal mode.</param>
		void PostStepAfterKeys(string keys, EventHandler handler);
		/// <summary>
		/// Invokes a handler that normally starts modal UI and posts another handler which is invoked in that modal mode.
		/// </summary>
		/// <param name="handler1">Handler starting modal UI.</param>
		/// <param name="handler2">Handler to be called in modal mode.</param>
		/// <remarks>
		/// Steps in <see cref="PostStep"/> work fine if they do not call something modal, like a dialog, for example.
		/// For this special case you should use this method: <b>handler1</b> normally calls something modal (dialog)
		/// and <b>handler2</b> is posted to be invoked after that (e.g. when a dialog is opened).
		/// </remarks>
		void PostStepAfterStep(EventHandler handler1, EventHandler handler2);
		/// <summary>
		/// Posts a job that will be called by the Far main thread when Far gets control.
		/// </summary>
		/// <param name="handler">Job handler to invoked.</param>
		/// <remarks>
		/// It is mostly designed for background job calls. Normally other threads are not allowed to call FarNet.
		/// Violation of this rule may lead to crashes and unpredictable results. This methods is thread safe and it
		/// allowes to post a delayed job that will be called from the main thread as soon as Far gets input control.
		/// Thus, this posted job can use FarNet as usual.
		/// <para>
		/// This method should be used very carefully and only when it is really needed.
		/// </para>
		/// </remarks>
		void PostJob(EventHandler handler);
		/// <summary>
		/// Current macro state.
		/// </summary>
		FarMacroState MacroState { get; }
		/// <summary>
		/// Redraws all windows. Far 1.71.2315
		/// </summary>
		/// <remarks>
		/// Example: you open an editor (assume it is modal) from a dialog;
		/// when you exit the editor you have to call this, otherwise only the dialog area is refreshed by Far.
		/// </remarks>
		void Redraw();
		/// <summary>
		/// Generates full path for a temp file or directory in %TEMP% (nothing is created).
		/// </summary>
		/// <param name="prefix">If empty "FTMP" is generated otherwise at most 4 first characters are used and padded by "0".</param>
		/// <returns>Generated name.</returns>
		string TempName(string prefix);
		/// <summary>
		/// See <see cref="TempName(string)"/>
		/// </summary>
		string TempName();
		/// <summary>
		/// Creates a folder in %TEMP%.
		/// </summary>
		/// <param name="prefix">If empty "FTMP" is generated otherwise at most 4 first characters are used and padded by "0".</param>
		/// <returns>Full path of the created folder.</returns>
		string TempFolder(string prefix);
		/// <summary>
		/// See <see cref="TempFolder(string)"/>
		/// </summary>
		/// <returns></returns>
		string TempFolder();
		/// <summary>
		/// The current dialog. STOP: be sure that a dialog exists otherwise effects are not predictable.
		/// </summary>
		IDialog Dialog { get; }
		/// <summary>
		/// The current editor or dialog edit box line or the command line.
		/// It is null if there is no current editor line available.
		/// </summary>
		ILine Line { get; }
		/// <summary>
		/// Key macros host.
		/// </summary>
		IKeyMacroHost KeyMacro { get; }
		/// <summary>
		/// Gets background color of Far palette.
		/// </summary>
		/// <param name="paletteColor">Palette color.</param>
		ConsoleColor GetPaletteBackground(PaletteColor paletteColor);
		/// <summary>
		/// Gets foreground color of Far palette.
		/// </summary>
		/// <param name="paletteColor">Palette color.</param>
		ConsoleColor GetPaletteForeground(PaletteColor paletteColor);
		/// <summary>
		/// Gets the internal active path.
		/// </summary>
		/// <remarks>
		/// The process current directory is not related to panels paths at all (Far 2.0.1145).
		/// and normally plugins should forget about the current directory, they should use this path.
		/// It should be used as the default for plugin file system operations, just like Far uses it.
		/// </remarks>
		string ActivePath { get; }
		/// <summary>
		/// For internal use.
		/// </summary>
		IZoo Zoo { get; }
	}

	/// <summary>
	/// Options for <see cref="IFar.ShowHelp"/>.
	/// </summary>
	[Flags]
	public enum HelpOptions
	{
		/// <summary>
		/// Assume path is Info.ModuleName and show the topic from the help file of the calling plugin (it is FarNet).
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
		DeleteNotEmptyFolders = 0x10,
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
	/// Far window types.
	/// </summary>
	public enum WindowType
	{
		/// <summary>
		/// Dummy.
		/// </summary>
		None,
		/// <summary>
		/// File panels.
		/// </summary>
		Panels,
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
	/// Contains information about one Far window. See <see cref="IFar.GetWindowInfo"/>.
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
		/// Name of the window type, depends on the current Far language.
		/// </summary>
		string TypeName { get; }
		/// <summary>
		/// Window title:
		/// viewer, editor: the file name;
		/// panels: selected file name;
		/// help: .hlf file path;
		/// menu, dialog: header.
		/// </summary>
		string Name { get; }
	}

	/// <summary>
	/// States of macro processing.
	/// </summary>
	public enum FarMacroState
	{
		/// <summary>
		/// No processing.
		/// </summary>
		None,
		/// <summary>
		/// A macro is in progress with plugins excluded.
		/// </summary>
		Executing,
		/// <summary>
		/// A macro is in progress with plugins included.
		/// </summary>
		ExecutingCommon,
		/// <summary>
		/// A macro is in been recorded with plugins excluded.
		/// </summary>
		Recording,
		/// <summary>
		/// A macro is in been recorded with plugins included.
		/// </summary>
		RecordingCommon
	}
}
