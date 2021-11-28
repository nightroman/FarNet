
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet.Forms;
using System;
using System.Globalization;
using System.IO;

namespace FarNet
{
	/// <summary>
	/// Main interface which exposes top entries of the FarNet object model.
	/// </summary>
	/// <remarks>
	/// The only instance of this class is exposed as the static property <see cref="Far.Api"/> of the class <see cref="Far"/>.
	/// <para>
	/// The exposed instance provides access to top level Far methods and objects or creates new Far objects like
	/// menus, input and message boxes, dialogs, editors, viewers, panels and etc.
	/// Further operations are performed on that objects.
	/// </para>
	/// </remarks>
	public abstract class IFar
	{
		/// <summary>
		/// Gets a module action by its ID. Null is returned if the ID is not found.
		/// </summary>
		/// <param name="id">The module action ID.</param>
		public abstract IModuleAction GetModuleAction(Guid id);
		/// <summary>
		/// Shows a message box.
		/// </summary>
		/// <param name="text">Message text.</param>
		/// <seealso cref="Message(MessageArgs)"/>
		public void Message(string text)
		{ Message(new MessageArgs() { Text = text, Options = MessageOptions.Ok }); }
		/// <summary>
		/// Shows a message box.
		/// </summary>
		/// <param name="text">Message text.</param>
		/// <param name="caption">Message caption.</param>
		/// <seealso cref="Message(MessageArgs)"/>
		public void Message(string text, string caption)
		{ Message(new MessageArgs() { Text = text, Caption = caption, Options = MessageOptions.Ok }); }
		/// <summary>
		/// Shows a message box with options.
		/// </summary>
		/// <param name="text">Message text.</param>
		/// <param name="caption">Message caption.</param>
		/// <param name="options">Message options.</param>
		/// <returns>The selected button index, or -1 on cancel, or 0 on drawn message.</returns>
		/// <seealso cref="Message(MessageArgs)"/>
		public int Message(string text, string caption, MessageOptions options)
		{ return Message(new MessageArgs() { Text = text, Caption = caption, Options = options }); }
		/// <summary>
		/// Shows a message box with options and buttons.
		/// </summary>
		/// <param name="text">Message text.</param>
		/// <param name="caption">Message caption.</param>
		/// <param name="options">Message options.</param>
		/// <param name="buttons">Message buttons. Not supported with <c>Gui*</c> options.</param>
		/// <returns>The selected button index, or -1 on cancel, or 0 on drawn message.</returns>
		/// <seealso cref="Message(MessageArgs)"/>
		public int Message(string text, string caption, MessageOptions options, string[] buttons)
		{ return Message(new MessageArgs() { Text = text, Caption = caption, Options = options, Buttons = buttons }); }
		/// <summary>
		/// Shows a message box with options, buttons, and help.
		/// </summary>
		/// <param name="text">Message text.</param>
		/// <param name="caption">Message caption.</param>
		/// <param name="options">Message options.</param>
		/// <param name="buttons">Message buttons. Not supported with <c>Gui*</c> options.</param>
		/// <param name="helpTopic">
		/// <include file='doc.xml' path='doc/HelpTopic/*'/>
		/// It is ignored in GUI and drawn messages.
		/// </param>
		/// <returns>The selected button index, or -1 on cancel, or 0 on drawn message.</returns>
		/// <seealso cref="Message(MessageArgs)"/>
		public int Message(string text, string caption, MessageOptions options, string[] buttons, string helpTopic)
		{ return Message(new MessageArgs() { Text = text, Caption = caption, Options = options, Buttons = buttons, HelpTopic = helpTopic }); }
		/// <summary>
		/// Shows a message box with the specified parameters.
		/// </summary>
		/// <param name="args">The parameters.</param>
		/// <returns>The selected button index, or -1 on cancel, or 0 on drawn message.</returns>
		/// <remarks>
		/// <para>
		/// If the <see cref="MessageOptions.Draw"/> option is set then GUI or buttons are not allowed.
		/// A message box with no buttons is simply drawn and the execution continues immediately.
		/// The caller has to remove the message by redrawing or restoring the screen.
		/// </para>
		/// <para>
		/// If the <see cref="MessageOptions.Draw"/> option is not set then the message is modal and
		/// it shows at least the button OK if there are no buttons provided by the parameters.
		/// </para>
		/// <para>
		/// In extreme cases with too many or too long buttons
		/// a list box is used in order to represent buttons.
		/// </para>
		/// </remarks>
		public abstract int Message(MessageArgs args);
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
		/// <param name="text">The text to be sent to the clipboard.</param>
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
		/// Posts a macro to Far. Processing is not displayed. Keys are sent to editor plugins.
		/// </summary>
		/// <param name="macro">Macro text.</param>
		public void PostMacro(string macro) { PostMacro(macro, false, false); }
		/// <summary>
		/// Posts a macro to Far.
		/// </summary>
		/// <param name="macro">Macro text.</param>
		/// <param name="enableOutput">Tells to display processing.</param>
		/// <param name="disablePlugins">Don't send keystrokes to editor plugins.</param>
		public abstract void PostMacro(string macro, bool enableOutput, bool disablePlugins);
		/// <summary>
		/// Converts a key string representation to <see cref="KeyInfo"/>. Returns null on errors.
		/// </summary>
		/// <param name="key">The key name to convert.</param>
		public abstract KeyInfo NameToKeyInfo(string key);
		/// <summary>
		/// Converts a <see cref="KeyInfo"/> to its string representation. Returns null on errors.
		/// </summary>
		/// <param name="key">The key info to convert.</param>
		public abstract string KeyInfoToName(KeyInfo key);
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
		/// If it is a module panel it returns <see cref="Panel"/>, you can keep its reference for later use,
		/// just remember that its state may change and it can be even closed.
		/// <para>
		/// If it is not a FarNet panel then you use this object instantly and do not keep it.
		/// </para>
		/// </remarks>
		public abstract IPanel Panel { get; }
		/// <summary>
		/// Gets the passive panel or null if Far started with /e or /v.
		/// </summary>
		/// <remarks>
		/// See remarks for the active panel (<see cref="Panel"/>).
		/// </remarks>
		public abstract IPanel Panel2 { get; }
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
		/// <include file='doc.xml' path='doc/LTRB/*'/>
		/// <remarks>
		/// You can set <c>left</c> = -1 or <c>top</c> = -1 to be auto-calculated.
		/// In this case <c>right</c> or <c>bottom</c> should be width and height.
		/// </remarks>
		public abstract IDialog CreateDialog(int left, int top, int right, int bottom);
		/// <include file='doc.xml' path='doc/ShowHelp/*'/>
		public abstract void ShowHelp(string path, string topic, HelpOptions options);
		/// <summary>
		/// Shows the help topic from a help file located in the directory of the calling assembly.
		/// </summary>
		/// <param name="topic">The help topic.</param>
		public abstract void ShowHelpTopic(string topic);
		/// <summary>
		/// Formats the help topic path for <c>HelpTopic</c> properties of various UI classes.
		/// </summary>
		/// <param name="topic">Module help topic name.</param>
		/// <returns>Help topic path formatted for the core.</returns>
		/// <remarks>
		/// The help topic path is formatted for a help file located in the directory of the calling assembly.
		/// Normally it is a module assembly but it can be any other, e.g. a shared library or a sub-module.
		/// <para>
		/// This method is enough for typical use cases and <c>HelpTopic</c> strings are formatted internally.
		/// In special cases see <see cref="ShowHelp"/> for help topic format details.
		/// </para>
		/// </remarks>
		public abstract string GetHelpTopic(string topic);
		/// <summary>
		/// Returns opened module panels having optionally specified type.
		/// </summary>
		/// <param name="type">The panel class type. Use null for any module panel.</param>
		public abstract Panel[] Panels(Type type);
		/// <summary>
		/// Returns opened module panels having specified type ID.
		/// </summary>
		/// <param name="typeId">The panel type ID (normally assigned on creation).</param>
		public abstract Panel[] Panels(Guid typeId);
		/// <include file='doc.xml' path='doc/Include/*'/>
		/// <param name="prompt">Prompt text.</param>
		/// <returns>Entered text or null if canceled.</returns>
		public string Input(string prompt) { return Input(prompt, null, null, string.Empty); }
		/// <include file='doc.xml' path='doc/Include/*'/>
		/// <param name="prompt">Prompt text.</param>
		/// <param name="history">History string.</param>
		/// <returns>Entered text or null if canceled.</returns>
		public string Input(string prompt, string history) { return Input(prompt, history, null, string.Empty); }
		/// <include file='doc.xml' path='doc/Include/*'/>
		/// <param name="prompt">Prompt text.</param>
		/// <param name="history">History string.</param>
		/// <param name="title">Title of the box.</param>
		/// <returns>Entered text or null if canceled.</returns>
		public string Input(string prompt, string history, string title) { return Input(prompt, history, title, string.Empty); }
		/// <include file='doc.xml' path='doc/Include/*'/>
		/// <param name="prompt">Prompt text.</param>
		/// <param name="history">History string.</param>
		/// <param name="title">Title of the box.</param>
		/// <param name="text">Text to be edited.</param>
		/// <returns>Entered text or null if canceled.</returns>
		public abstract string Input(string prompt, string history, string title, string text);
		/// <summary>
		/// Posts the job to be called when the core gets control.
		/// </summary>
		/// <param name="job">The job action.</param>
		/// <remarks>
		/// It is mostly designed for background job calls. Normally other threads are not allowed to call the core.
		/// Violation of this rule may lead to crashes and unpredictable results. This method is thread safe. It is
		/// used to post a job that will be called from the main thread as soon as the core gets control.
		/// The posted job can call the core as usual.
		/// </remarks>
		public abstract void PostJob(Action job);
		/// <summary>
		/// Posts the step, an actions called later from the plugin menu.
		/// </summary>
		/// <param name="step">The step action.</param>
		/// <remarks>
		/// This method uses macros to open the plugin menu which calls the
		/// posted action. This step call may be needed for opening panels
		/// in unusual scenarios.
		/// <para>
		/// Unlike jobs, steps must not be posted from parallel threads.
		/// </para>
		/// </remarks>
		public abstract void PostStep(Action step);
		/// <summary>
		/// Gets the current macro area.
		/// </summary>
		public abstract MacroArea MacroArea { get; }
		/// <summary>
		/// Gets the current macro state.
		/// </summary>
		public abstract MacroState MacroState { get; }
		/// <summary>
		/// Generates the full path of an item in %TEMP% without creating it.
		/// </summary>
		/// <param name="prefix">If it is null or empty then "FAR" is used.</param>
		/// <returns>Generated path.</returns>
		public abstract string TempName(string prefix);
		/// <summary>
		/// See <see cref="TempName(string)"/>
		/// </summary>
		public string TempName()
		{
			return TempName(null);
		}
		/// <summary>
		/// Gets the most recent opened dialog or null.
		/// </summary>
		/// <remarks>
		/// If there are no opened dialogs then it gets null.
		/// <para>
		/// Mantis 2241: This call from not the main thread hangs if a menu is opened after a dialog.
		/// </para>
		/// </remarks>
		public abstract IDialog Dialog { get; }
		/// <summary>
		/// Gets the current editor or dialog edit box line or the command line.
		/// </summary>
		/// <remarks>
		/// It returns null if there is no current editor line available.
		/// <para>
		/// Mantis 2241: This call from not the main thread hangs if a menu is opened, e.g. autocomplete.
		/// </para>
		/// </remarks>
		public abstract ILine Line { get; }
		/// <summary>
		/// Gets the internal current directory.
		/// </summary>
		/// <remarks>
		/// The process current directory is not related to panels paths at all (Far 2.0.1145).
		/// and normally modules should forget about the current directory, they should use this path.
		/// It should be used as the default path for file system operations (e.g. where to create a new file).
		/// </remarks>
		public abstract string CurrentDirectory { get; }
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
		/// Gets the low level UI operator.
		/// </summary>
		public abstract IUserInterface UI { get; }
		/// <summary>
		/// Gets true if a file path matches a Far Manager file mask.
		/// </summary>
		/// <param name="path">Input file path.</param>
		/// <param name="mask">Mask: "include-wildcard-list[|exclude-wildcard-list]" or "/regex/[option]".</param>
		public abstract bool IsMaskMatch(string path, string mask);
		/// <summary>
		/// Gets true if a Far Manager file mask is valid.
		/// </summary>
		/// <param name="mask">The mask to test.</param>
		public abstract bool IsMaskValid(string mask);
		/// <summary>
		/// INTERNAL Gets the local or roamimg data directory path of the application.
		/// </summary>
		/// <param name="folder">The special folder.</param>
		/// <remarks>
		/// This method and directories are used by the core and not designed for modules.
		/// Modules should use <see cref="IModuleManager.GetFolderPath"/> in order to get their data directories.
		/// </remarks>
		public abstract string GetFolderPath(SpecialFolder folder);
		/// <summary>
		/// Returns the manager of a module specified by its name.
		/// </summary>
		/// <param name="name">The module name.</param>
		public abstract IModuleManager GetModuleManager(string name);
		/// <summary>
		/// Returns the manager of a module specified by any type from it.
		/// </summary>
		/// <param name="type">Any type from the module assembly.</param>
		public IModuleManager GetModuleManager(Type type)
		{
			//! Assembly.GetName().Name: 1) slower; 2) does not guarantee the same name.
			if (type == null) throw new ArgumentNullException("type");
			return GetModuleManager(Path.GetFileNameWithoutExtension(type.Assembly.Location));
		}
		/// <summary>
		/// Gets the history operator.
		/// </summary>
		public abstract IHistory History { get; }
		/// <summary>
		/// Gets the specified setting value.
		/// </summary>
		/// <param name="settingSet">Setting set.</param>
		/// <param name="settingName">Setting name.</param>
		/// <returns>Requested value (long, string, or byte[]).</returns>
		/// <exception cref="ArgumentException">The specified set or name is invalid.</exception>
		public abstract object GetSetting(FarSetting settingSet, string settingName);
	}

	/// <summary>
	/// Holder of the global <see cref="IFar"/> host instance.
	/// </summary>
	public static class Far
	{
		static IFar _Host;
		/// <summary>
		/// The global <see cref="IFar"/> instance.
		/// </summary>
		public static IFar Api
		{
			get { return _Host; }
			set
			{
				if (_Host != null) throw new InvalidOperationException();
				_Host = value;
			}
		}
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
		/// Show the topic from the help file of the plugin.
		/// In FarNet it is <c>Far.Api.GetType().Assembly.Location</c>.
		/// If a topic begins with a colon the a topic from the main Far help file is shown.
		/// </summary>
		None = 0x0,
		/// <summary>
		/// Path is ignored and the topic from the main Far help file is shown.
		/// In this case you do not need to start the topic with a colon ':'.
		/// </summary>
		Far = 1 << 0,
		/// <summary>
		/// Assume path specifies full path to a HLF file (c:\path\filename).
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
	/// States of macro processing.
	/// </summary>
	public enum MacroState
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

	/// <summary>
	/// Special folder constants.
	/// </summary>
	public enum SpecialFolder
	{
		/// <summary>
		/// Local data folder, %FARLOCALPROFILE%.
		/// </summary>
		LocalData = 0,
		/// <summary>
		/// Roaming data folder, %FARPROFILE%.
		/// </summary>
		RoamingData = 1
	}

	/// <summary>
	/// Far Manager settings.
	/// </summary>
	public enum FarSetting
	{
		///
		None,
		/// <summary>
		/// Confirmation settings:
		/// Copy, Move, RO, Drag, Delete, DeleteFolder, Esc, HistoryClear, Exit, RemoveConnection.
		/// </summary>
		Confirmations = 16,
		/// <summary>
		/// System settings:
		/// DeleteToRecycleBin, CopyOpened, PluginMaxReadData, ScanJunction.
		/// </summary>
		System = 17,
		/// <summary>
		/// Panels settings:
		/// ShowHidden.
		/// </summary>
		Panels = 18,
		/// <summary>
		/// Editor settings:
		/// WordDiv.
		/// </summary>
		Editor = 19,
		/// <summary>
		/// Screen settings:
		/// KeyBar.
		/// </summary>
		Screen = 20,
		/// <summary>
		/// Dialog settings:
		/// EditBlock, EULBsClear, DelRemovesBlocks.
		/// </summary>
		Dialog = 21,
		/// <summary>
		/// Interface settings:
		/// ShowMenuBar.
		/// </summary>
		Interface = 22,
		/// <summary>
		/// Panel layout settings:
		/// ColumnTitles, StatusLine, SortMode.
		/// </summary>
		PanelLayout = 23,
	}
}
