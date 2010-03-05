/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using FarNet.Forms;
using FarNet.Support;

namespace FarNet
{
	/// <summary>
	/// Holder of the global <see cref="IFar"/> host instance.
	/// </summary>
	public static class Far
	{
		/// <summary>
		/// The global <see cref="IFar"/> instance.
		/// </summary>
		public static IFar Net
		{
			get { return _Host; }
			set
			{
				if (_Host == null)
					_Host = value;
				else
					throw new InvalidOperationException();
			}
		}
		static IFar _Host;
	}

	/// <summary>
	/// Main interface which exposes top entries of the FarNet object model.
	/// </summary>
	/// <remarks>
	/// The only instance of this class is exposed as the static property <see cref="Far.Net"/> of the class <see cref="Far"/>.
	/// Both names <c>Far</c> and <c>Net</c> are symbolic and make sense only when used together as <c>Far.Net</c>.
	/// <para>
	/// The exposed instance provides access to top level Far methods and objects or creates new Far objects like
	/// menus, input and message boxes, dialogs, editors, viewers, panels and etc.
	/// Further operations are performed on that objects.
	/// </para>
	/// </remarks>
	public abstract class IFar
	{
		/// <summary>
		/// For internal use.
		/// </summary>
		public abstract IZoo Zoo { get; }
		/// <summary>
		/// Gets any module command by its ID.
		/// </summary>
		public abstract IModuleCommand GetModuleCommand(Guid id);
		/// <summary>
		/// Gets any module filer by its ID.
		/// </summary>
		public abstract IModuleFiler GetModuleFiler(Guid id);
		/// <summary>
		/// Gets any module tool by its ID.
		/// </summary>
		public abstract IModuleTool GetModuleTool(Guid id);
		/// <summary>
		/// Shows a message box.
		/// </summary>
		/// <param name="body">Message text.</param>
		public abstract void Message(string body);
		/// <summary>
		/// Shows a message box.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <param name="header">Message header.</param>
		public abstract void Message(string body, string header);
		/// <summary>
		/// Shows a message box with options.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <param name="header">Message header.</param>
		/// <param name="options">Message options.</param>
		/// <returns>Button index or -1 if cancelled.</returns>
		public abstract int Message(string body, string header, MsgOptions options);
		/// <summary>
		/// Shows a message box with options and buttons.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <param name="header">Message header.</param>
		/// <param name="options">Message options.</param>
		/// <param name="buttons">Message buttons. Not supported with <c>Gui*</c> options.</param>
		/// <returns>Button index or -1 if cancelled.</returns>
		public abstract int Message(string body, string header, MsgOptions options, string[] buttons);
		/// <summary>
		/// Shows a message box with options, buttons and help.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <param name="header">Message header.</param>
		/// <param name="options">Message options.</param>
		/// <param name="buttons">Message buttons. Not supported with <c>Gui*</c> options.</param>
		/// <param name="helpTopic">
		/// <include file='doc.xml' path='docs/pp[@name="HelpTopic"]/*'/>
		/// It is ignored in a GUI message.
		/// </param>
		/// <returns>Button index or -1 if cancelled.</returns>
		/// <remarks>
		/// In extreme cases when a message contains too many or too long buttons, then a message
		/// box is converted into a listbox dialog where listbox items work as buttons.
		/// </remarks>
		public abstract int Message(string body, string header, MsgOptions options, string[] buttons, string helpTopic);
		/// <summary>
		/// Runs a command with a registered FarNet prefix.
		/// </summary>
		/// <param name="command">Command with a prefix of any FarNet module.</param>
		public abstract void Run(string command);
		/// <summary>
		/// Gets the Far main window handle.
		/// </summary>
		public abstract IntPtr MainWindowHandle { get; }
		/// <summary>
		/// Gets Far version.
		/// </summary>
		public abstract Version FarVersion { get; }
		/// <summary>
		/// Gets FarNet version.
		/// </summary>
		public abstract Version FarNetVersion { get; }
		/// <summary>
		/// Creates a new input box.
		/// You set its properties and call <see cref="IInputBox.Show"/>.
		/// </summary>
		public abstract IInputBox CreateInputBox();
		/// <summary>
		/// Creates a new standard Far menu.
		/// You set its properties and call <see cref="IAnyMenu.Show"/>.
		/// </summary>
		public abstract IMenu CreateMenu();
		/// <summary>
		/// Creates a new menu implemented with <see cref="IListBox"/>.
		/// You set its properties and call <see cref="IAnyMenu.Show"/>.
		/// </summary>
		public abstract IListMenu CreateListMenu();
		/// <summary>
		/// Gets the object with global editor events, settings and tools.
		/// </summary>
		/// <remarks>
		/// Members of the returned object deal with global editor events, settings and tools.
		/// Subscribe to its events if you want to handle some events in the same way for all editors.
		/// </remarks>
		public abstract IAnyEditor AnyEditor { get; }
		/// <summary>
		/// Gets the object with global viewer events, settings and tools.
		/// </summary>
		/// <remarks>
		/// Members of the returned object deal with global viewer events, settings and tools.
		/// Subscribe to its events if you want to handle some events in the same way for all viewers.
		/// </remarks>
		public abstract IAnyViewer AnyViewer { get; }
		/// <summary>
		/// Gets the clipboard text.
		/// </summary>
		public abstract string PasteFromClipboard();
		/// <summary>
		/// Sets the clipboard text.
		/// </summary>
		public abstract void CopyToClipboard(string text);
		/// <summary>
		/// Creates a new editor.
		/// You set its properties and call <see cref="IEditor.Open(OpenMode)"/>.
		/// </summary>
		public abstract IEditor CreateEditor();
		/// <summary>
		/// Creates a new viewer.
		/// You set its properties and call <see cref="IViewer.Open(OpenMode)"/>.
		/// </summary>
		public abstract IViewer CreateViewer();
		/// <summary>
		/// Posts keys to the Far keyboard queue. Processing is not displayed.
		/// </summary>
		/// <param name="keys">String of keys.</param>
		public abstract void PostKeys(string keys);
		/// <summary>
		/// Posts keys to the Far keyboard queue.
		/// </summary>
		/// <param name="keys">String of keys.</param>
		/// <param name="disableOutput">Do not display processing on the screen.</param>
		public abstract void PostKeys(string keys, bool disableOutput);
		/// <summary>
		/// Posts literal text to the Far keyboard queue. Processing is not displayed.
		/// </summary>
		/// <param name="text">Literal text. \t, \r, \n, \r\n are translated to [Tab] and [Enter].</param>
		public abstract void PostText(string text);
		/// <summary>
		/// Posts literal text to the Far keyboard queue.
		/// </summary>
		/// <param name="text">Literal text. \t, \r, \n, \r\n are translated to [Tab] and [Enter].</param>
		/// <param name="disableOutput">Do not display processing on the screen.</param>
		public abstract void PostText(string text, bool disableOutput);
		/// <summary>
		/// Creates a sequence of key codes from a string of keys.
		/// </summary>
		public abstract int[] CreateKeySequence(string keys);
		/// <summary>
		/// Posts a sequence of keys to the Far keyboard queue.
		/// Processing is not displayed.
		/// </summary>
		/// <param name="sequence">Sequence of keys.</param>
		public abstract void PostKeySequence(int[] sequence);
		/// <summary>
		/// Posts a sequence of keys to the Far keyboard queue.
		/// </summary>
		/// <param name="sequence">Sequence of keys.</param>
		/// <param name="disableOutput">Do not display processing on the screen.</param>
		public abstract void PostKeySequence(int[] sequence, bool disableOutput);
		/// <summary>
		/// Posts a macro to Far.
		/// Processing is not displayed, and keys are sent to editor plugins.
		/// </summary>
		/// <param name="macro">Macro text.</param>
		public abstract void PostMacro(string macro);
		/// <summary>
		/// Posts a macro to Far.
		/// </summary>
		/// <param name="macro">Macro text.</param>
		/// <param name="enableOutput">Enable screen output during macro playback.</param>
		/// <param name="disablePlugins">Don't send keystrokes to editor plugins.</param>
		public abstract void PostMacro(string macro, bool enableOutput, bool disablePlugins);
		/// <summary>
		/// Converts a key string representation to the internal <see cref="KeyCode"/>. Returns -1 on errors.
		/// </summary>
		public abstract int NameToKey(string key);
		/// <summary>
		/// Converts an internal <see cref="KeyCode"/> to string representation. Returns null on errors.
		/// </summary>
		public abstract string KeyToName(int key);
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
		public abstract int SaveScreen(int left, int top, int right, int bottom);
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
		public abstract void RestoreScreen(int screen);
		/// <summary>
		/// Gets the current editor or null if none.
		/// </summary>
		/// <remarks>
		/// Normally you use this object instantly and do not keep it for later use.
		/// Next time when you work on the current editor request this object again.
		/// </remarks>
		public abstract IEditor Editor { get; }
		/// <summary>
		/// Gets the current viewer or null if none.
		/// </summary>
		/// <remarks>
		/// Normally you use this object instantly and do not keep it for later use.
		/// Next time when you work on the current viewer request this object again.
		/// </remarks>
		public abstract IViewer Viewer { get; }
		/// <summary>
		/// Gets the list of all editors. Use it sparingly.
		/// </summary>
		/// <remarks>
		/// Work on not current editor instances is strongly not recommended.
		/// Still, this list provides access to them all, so be careful.
		/// </remarks>
		public abstract IEditor[] Editors();
		/// <summary>
		/// Gets the list of all viewers. Use it sparingly.
		/// </summary>
		/// <remarks>
		/// Work on not current viewer instances is strongly not recommended.
		/// Still, this list provides access to them all, so be careful.
		/// </remarks>
		public abstract IViewer[] Viewers();
		/// <summary>
		/// Gets the active panel or null if Far started with /e or /v.
		/// </summary>
		/// <remarks>
		/// If it is a FarNet panel it returns <see cref="IPanel"/>, you can keep its reference for later use,
		/// just remember that its state may change and it can be even closed.
		/// <para>
		/// If it is not a FarNet panel then you use this object instantly and do not keep it.
		/// </para>
		/// </remarks>
		public abstract IAnyPanel Panel { get; }
		/// <summary>
		/// Gets the passive panel or null if Far started with /e or /v.
		/// </summary>
		/// <remarks>
		/// See remarks for the active panel (<see cref="Panel"/>).
		/// </remarks>
		public abstract IAnyPanel Panel2 { get; }
		/// <summary>
		/// Gets the command line operator.
		/// </summary>
		/// <remarks>
		/// If a module is called from the command line (including user menu [F2])
		/// then command line properties and methods may not work correctly. In
		/// this case consider to call an operation from the plugins menu [F11].
		/// <para>
		/// A module can set the entire command line text if it is called
		/// from the command line but not from the user menu.
		/// </para>
		/// </remarks>
		public abstract ILine CommandLine { get; }
		/// <summary>
		/// Copies the current screen contents to the user screen buffer
		/// (which is displayed when the panels are switched off).
		/// </summary>
		public abstract void SetUserScreen();
		/// <summary>
		/// Copies the current user screen buffer to console screen
		/// (which is displayed when the panels are switched off).
		/// </summary>
		public abstract void GetUserScreen();
		/// <summary>
		/// Returns all strings from history.
		/// </summary>
		/// <param name="name">History name. Standard values are: SavedHistory, SavedFolderHistory, SavedViewHistory.</param>
		public abstract ICollection<string> GetHistory(string name);
		/// <summary>
		/// Returns strings from history by type.
		/// </summary>
		/// <param name="name">History name. Standard values are: SavedHistory, SavedFolderHistory, SavedViewHistory.</param>
		/// <param name="filter">
		/// Type filter: each character represents a type. For example for SavedViewHistory: 0: view; 1: edit; 2: external.
		/// </param>
		public abstract ICollection<string> GetHistory(string name, string filter);
		/// <summary>
		/// Returns strings from dialog control history.
		/// </summary>
		/// <param name="name">History name.</param>
		public abstract ICollection<string> GetDialogHistory(string name);
		/// <summary>
		/// Shows an error information in a message box which also stops any macro.
		/// </summary>
		/// <param name="title">Message.</param>
		/// <param name="exception">Exception.</param>
		/// <remarks>
		/// For safety sake: avoiding unexpected results on exceptions during a running
		/// macro this method stops a macro before showing an error dialog. That is why
		/// this method should be called only in exceptional situations.
		/// <para>
		/// Basically it is called internally on all exceptions not handled by modules
		/// but it is as well designed for direct calls, too.
		/// </para>
		/// <seealso cref="ModuleException"/>
		/// </remarks>
		public abstract void ShowError(string title, Exception exception);
		/// <summary>
		/// Creates a new dialog.
		/// You set its properties, add controls, event handlers and then call <see cref="IDialog.Show"/>.
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="LTRB"]/*'/>
		/// <remarks>
		/// You can set <c>left</c> = -1 or <c>top</c> = -1 to be auto-calculated.
		/// In this case <c>right</c> or <c>bottom</c> should be width and height.
		/// </remarks>
		public abstract IDialog CreateDialog(int left, int top, int right, int bottom);
		/// <summary>
		/// Creates a dialog for selecting a subset of items.
		/// </summary>
		public abstract ISubsetForm CreateSubsetForm();
		/// <include file='doc.xml' path='docs/pp[@name="ShowHelp"]/*'/>
		public abstract void ShowHelp(string path, string topic, HelpOptions options);
		/// <summary>
		/// Writes text on the user screen (under panels).
		/// </summary>
		/// <param name="text">Text.</param>
		/// <remarks>
		/// Avoid <c>Console.Write*</c> methods.
		/// </remarks>
		public abstract void Write(string text);
		/// <summary>
		/// Writes colored text on the user screen (under panels).
		/// </summary>
		/// <param name="text">Text.</param>
		/// <param name="foregroundColor">Text color.</param>
		public abstract void Write(string text, ConsoleColor foregroundColor);
		/// <summary>
		/// Writes colored text on the user screen (under panels).
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="Colors"]/*'/>
		/// <param name="text">Text.</param>
		public abstract void Write(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor);
		/// <summary>
		/// Writes a string at the specified position using Far palette colors.
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="LT"]/*'/>
		/// <param name="paletteColor">Palette color.</param>
		/// <param name="text">Text.</param>
		public abstract void WritePalette(int left, int top, PaletteColor paletteColor, string text);
		/// <summary>
		/// Writes a string at the specified position with defined colors.
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="LT"]/*'/>
		/// <include file='doc.xml' path='docs/pp[@name="Colors"]/*'/>
		/// <param name="text">Text.</param>
		/// <seealso cref="IFar.GetPaletteForeground"/>
		/// <seealso cref="IFar.GetPaletteBackground"/>
		public abstract void WriteText(int left, int top, ConsoleColor foregroundColor, ConsoleColor backgroundColor, string text);
		/// <summary>
		/// Finds an existing module panel with the specified host (see <see cref="IPanel.Host"/>).
		/// </summary>
		/// <param name="hostType">
		/// Type of the hosting class.
		/// If it is null then any module panel is returned.
		/// If it is <c>typeof(object)</c> then any module panel having a host is returned.
		/// </param>
		public abstract IPanel FindPanel(Type hostType);
		/// <summary>
		/// Finds an existing module panel with the specified type ID or returns null.
		/// </summary>
		/// <param name="typeId">Panel type ID. It is normally assigned by a creator.</param>
		/// <seealso cref="IPanel.TypeId"/>
		public abstract IPanel FindPanel(Guid typeId);
		/// <summary>
		/// Creates a new panel.
		/// </summary>
		/// <remarks>
		/// If the panel should be opened when Far gets control then consider to call
		/// <see cref="IPanel.Open()"/> as soon as possible to be sure that this is allowed.
		/// Then you may configure the panel and other data. Actual panel opening is performed
		/// only when the module call is over.
		/// </remarks>
		public abstract IPanel CreatePanel();
		/// <summary>
		/// Gets confirmation settings (see Far "Confirmations" dialog).
		/// </summary>
		public abstract FarConfirmations Confirmations { get; }
		/// <include file='doc.xml' path='docs/pp[@name="Include"]/*'/>
		/// <param name="prompt">Prompt text.</param>
		/// <returns>Entered text or null if cancelled.</returns>
		public abstract string Input(string prompt);
		/// <include file='doc.xml' path='docs/pp[@name="Include"]/*'/>
		/// <param name="prompt">Prompt text.</param>
		/// <param name="history">History string.</param>
		/// <returns>Entered text or null if cancelled.</returns>
		public abstract string Input(string prompt, string history);
		/// <include file='doc.xml' path='docs/pp[@name="Include"]/*'/>
		/// <param name="prompt">Prompt text.</param>
		/// <param name="history">History string.</param>
		/// <param name="title">Title of the box.</param>
		/// <returns>Entered text or null if cancelled.</returns>
		public abstract string Input(string prompt, string history, string title);
		/// <include file='doc.xml' path='docs/pp[@name="Include"]/*'/>
		/// <param name="prompt">Prompt text.</param>
		/// <param name="history">History string.</param>
		/// <param name="title">Title of the box.</param>
		/// <param name="text">Text to be edited.</param>
		/// <returns>Entered text or null if cancelled.</returns>
		public abstract string Input(string prompt, string history, string title, string text);
		/// <summary>
		/// Converts an internal key code to a 'printable' char. <see cref="KeyCode"/>
		/// </summary>
		/// <remarks>
		/// If the code does not correspond to a 'printable' char then 0 is returned.
		/// Note: chars below space are returned as they are because they are sort of 'printable'.
		/// </remarks>
		public abstract char CodeToChar(int code);
		/// <summary>
		/// Posts a handler to be invoked when user code has finished and Far gets control.
		/// </summary>
		/// <param name="handler">Step handler.</param>
		/// <remarks>
		/// Many Far operations are executed only when Far gets control, i.e. when user code has finished.
		/// Thus, normally you can not performs several such operations together. This method may help.
		/// <para>
		/// This mechanism works only when the plugins menu [F11] is available, because it is used internally for stepping.
		/// Ensure some FarNet hotkey in the plugins menu. Use [F11] for menu, [F4] to set a hotkey there.
		/// </para>
		/// <para>
		/// If a step handler starts modal UI without exiting (e.g. dialog) then use <see cref="PostStepAfterStep"/>
		/// if you have another step to be invoked in modal mode (e.g. in a dialog after opening).
		/// </para>
		/// </remarks>
		public abstract void PostStep(EventHandler handler);
		/// <summary>
		/// Posts the keys that normally start modal UI and a handler which is invoked in that modal mode.
		/// </summary>
		/// <param name="keys">Keys starting modal UI.</param>
		/// <param name="handler">Handler to be called in modal mode.</param>
		public abstract void PostStepAfterKeys(string keys, EventHandler handler);
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
		public abstract void PostStepAfterStep(EventHandler handler1, EventHandler handler2);
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
		public abstract void PostJob(EventHandler handler);
		/// <summary>
		/// Gets the current macro state.
		/// </summary>
		public abstract FarMacroState MacroState { get; }
		/// <summary>
		/// Redraws all windows.
		/// </summary>
		/// <remarks>
		/// Example: you open an editor (assume it is modal) from a dialog;
		/// when you exit the editor you have to call this, otherwise only the dialog area is refreshed by Far.
		/// </remarks>
		public abstract void Redraw();
		/// <summary>
		/// Generates full path for a temp file or directory in %TEMP% (nothing is created).
		/// </summary>
		/// <param name="prefix">If empty "FTMP" is generated otherwise at most 4 first characters are used and padded by "0".</param>
		/// <returns>Generated name.</returns>
		public abstract string TempName(string prefix);
		/// <summary>
		/// See <see cref="TempName(string)"/>
		/// </summary>
		public abstract string TempName();
		/// <summary>
		/// Creates a folder in %TEMP%.
		/// </summary>
		/// <param name="prefix">If empty "FTMP" is generated otherwise at most 4 first characters are used and padded by "0".</param>
		/// <returns>Full path of the created folder.</returns>
		public abstract string TempFolder(string prefix);
		/// <summary>
		/// See <see cref="TempFolder(string)"/>
		/// </summary>
		public abstract string TempFolder();
		/// <summary>
		/// Gets the current dialog operator. Use it sparingly.
		/// </summary>
		/// <remarks>
		/// STOP: Be sure that a dialog exists otherwise effects are not predictable.
		/// </remarks>
		public abstract IDialog Dialog { get; }
		/// <summary>
		/// Gets the current editor or dialog edit box line or the command line.
		/// </summary>
		/// <remarks>
		/// It is null if there is no current editor line available.
		/// </remarks>
		public abstract ILine Line { get; }
		/// <summary>
		/// Gets macro operator.
		/// </summary>
		public abstract IMacro Macro { get; }
		/// <summary>
		/// Returns background color of Far palette.
		/// </summary>
		/// <param name="paletteColor">Palette color.</param>
		public abstract ConsoleColor GetPaletteBackground(PaletteColor paletteColor);
		/// <summary>
		/// Returns foreground color of Far palette.
		/// </summary>
		/// <param name="paletteColor">Palette color.</param>
		public abstract ConsoleColor GetPaletteForeground(PaletteColor paletteColor);
		/// <summary>
		/// Gets the internal active path.
		/// </summary>
		/// <remarks>
		/// The process current directory is not related to panels paths at all (Far 2.0.1145).
		/// and normally modules should forget about the current directory, they should use this path.
		/// It should be used as the default path for file system operations (e.g. where to create a new file).
		/// </remarks>
		public abstract string ActivePath { get; }
		/// <summary>
		/// Sets the type and state of the progress indicator displayed on a taskbar button of the main application window.
		/// </summary>
		/// <param name="state">Progress state of the progress button.</param>
		public abstract void SetProgressState(TaskbarProgressBarState state);
		/// <summary>
		/// Displays or updates a progress bar hosted in a taskbar button of the main application window
		/// to show the specific percentage completed of the full operation.
		/// </summary>
		/// <param name="currentValue">Indicates the proportion of the operation that has been completed.</param>
		/// <param name="maximumValue">Specifies the value <c>currentValue</c> will have when the operation is complete.</param>
		public abstract void SetProgressValue(int currentValue, int maximumValue);
		/// <summary>
		/// Returns the current UI culture.
		/// </summary>
		/// <param name="update">Tells to update the internal cached value.</param>
		/// <returns>The current UI culture (cached or updated).</returns>
		public abstract CultureInfo GetCurrentUICulture(bool update);
		/// <summary>
		/// Tells Far to exit if it is possible.
		/// </summary>
		/// <remarks>
		/// Before sending this request to Far it calls <see cref="ModuleHost.CanExit"/> for each module.
		/// If all modules return true then Far is called. If there is an editor with not saved changes
		/// then Far asks a user how to proceed and, in fact, a user may continue work in Far.
		/// </remarks>
		public abstract void Quit();
		/// <summary>
		/// Gets the window operator.
		/// </summary>
		public abstract IWindow Window { get; }
		/// <summary>
		/// Opens the virtual registry key to access the FarNet host data.
		/// </summary>
		/// <param name="name">Name or path of the key to open. If it is null or empty then the root key is opened.</param>
		/// <param name="writable">Set to true if you need write access to the key.</param>
		/// <returns>The requested key or null if the key for reading does not exist.</returns>
		/// <remarks>
		/// The returned key has to be disposed after use by <c>Dispose()</c>.
		/// <para>
		/// This method should not be used in standard cases because it operates on global data dependent on the FarNet host.
		/// Modules should access their local data by <see cref="IModuleManager.OpenRegistryKey"/> of their module managers.
		/// </para>
		/// <para>
		/// For the Far Manager host the root key in the Windows registry is <c>HKEY_CURRENT_USER\Software\Far2</c>
		/// or <c>HKEY_CURRENT_USER\Software\Far2\Users\Xyz</c> if Far is started with the parameter /u Xyz.
		/// </para>
		/// </remarks>
		public abstract IRegistryKey OpenRegistryKey(string name, bool writable);
	}

	/// <summary>
	/// Represents the thumbnail progress bar state.
	/// </summary>
	public enum TaskbarProgressBarState
	{
		/// <summary>
		/// No progress is displayed.
		/// </summary>
		NoProgress = 0,
		/// <summary>
		/// The progress is indeterminate (marquee).
		/// </summary>
		Indeterminate = 1,
		/// <summary>
		/// Normal progress is displayed.
		/// </summary>
		Normal = 2,
		/// <summary>
		/// An error occurred (red).
		/// </summary>
		Error = 4,
		/// <summary>
		/// The operation is paused (yellow).
		/// </summary>
		Paused = 8
	}

	/// <summary>
	/// Options for <see cref="IFar.ShowHelp"/>.
	/// </summary>
	[Flags]
	public enum HelpOptions
	{
		/// <summary>
		/// Show the topic from the help file of the calling plugin (note: it is always FarNet).
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
	/// States of macro processing.
	/// </summary>
	public enum FarMacroState
	{
		/// <summary>
		/// No processing.
		/// </summary>
		None,
		/// <summary>
		/// Executing with plugins excluded.
		/// </summary>
		Executing,
		/// <summary>
		/// Executing with plugins included.
		/// </summary>
		ExecutingCommon,
		/// <summary>
		/// Recording with plugins excluded.
		/// </summary>
		Recording,
		/// <summary>
		/// Recording with plugins included.
		/// </summary>
		RecordingCommon
	}

}
