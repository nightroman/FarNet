
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using FarNet.Forms;

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
		public abstract Works.IPanelWorks WorksPanel(Panel panel, Explorer explorer);
		/// <summary>
		/// Gets any module command by its ID.
		/// </summary>
		public abstract IModuleCommand GetModuleCommand(Guid id);
		/// <summary>
		/// Gets any module tool by its ID.
		/// </summary>
		public abstract IModuleTool GetModuleTool(Guid id);
		/// <summary>
		/// Shows a message box.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <seealso cref="Message(string, string, MessageOptions, string[], string)"/>
		public void Message(string body) { Message(body, null, MessageOptions.Ok, null, null); }
		/// <summary>
		/// Shows a message box.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <param name="header">Message header.</param>
		/// <seealso cref="Message(string, string, MessageOptions, string[], string)"/>
		public void Message(string body, string header) { Message(body, header, MessageOptions.Ok, null, null); }
		/// <summary>
		/// Shows a message box with options.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <param name="header">Message header.</param>
		/// <param name="options">Message options.</param>
		/// <returns>Button index or -1 if canceled.</returns>
		/// <seealso cref="Message(string, string, MessageOptions, string[], string)"/>
		public int Message(string body, string header, MessageOptions options) { return Message(body, header, options, null, null); }
		/// <summary>
		/// Shows a message box with options and buttons.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <param name="header">Message header.</param>
		/// <param name="options">Message options.</param>
		/// <param name="buttons">Message buttons. Not supported with <c>Gui*</c> options.</param>
		/// <returns>Button index or -1 if canceled.</returns>
		/// <seealso cref="Message(string, string, MessageOptions, string[], string)"/>
		public int Message(string body, string header, MessageOptions options, string[] buttons) { return Message(body, header, options, buttons, null); }
		/// <summary>
		/// Shows a message box with options, buttons and help.
		/// </summary>
		/// <param name="body">Message text.</param>
		/// <param name="header">Message header.</param>
		/// <param name="options">Message options.</param>
		/// <param name="buttons">Message buttons. Not supported with <c>Gui*</c> options.</param>
		/// <param name="helpTopic">
		/// <include file='doc.xml' path='doc/HelpTopic/*'/>
		/// It is ignored in GUI and drawn messages.
		/// </param>
		/// <returns>Button index or -1 if canceled, or 0 in the drawn message.</returns>
		/// <remarks>
		/// <para>
		/// If the <see cref="MessageOptions.Draw"/> option is set then GUI or buttons are not allowed.
		/// A message box with no buttons is simply drawn and the execution continues immediately.
		/// The caller has to remove the message by redrawing or restoring the screen.
		/// </para>
		/// <para>
		/// If the <see cref="MessageOptions.Draw"/> option is not set then the message is modal and
		/// it shows at least the OK button if there are no buttons provided by the parameters.
		/// </para>
		/// <para>
		/// In extreme cases when a message contains too many or too long buttons
		/// a listbox dialog is used where the listbox items work as buttons.
		/// </para>
		/// </remarks>
		public abstract int Message(string body, string header, MessageOptions options, string[] buttons, string helpTopic);
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
		/// Posts literal text to the Far keyboard queue. Processing is not displayed.
		/// </summary>
		/// <param name="text">Literal text. \t, \r, \n, \r\n are translated to [Tab] and [Enter].</param>
		public void PostText(string text) { PostText(text, false); }
		/// <summary>
		/// Posts literal text to the Far keyboard queue.
		/// </summary>
		/// <param name="text">Literal text. \t, \r, \n, \r\n are translated to [Tab] and [Enter].</param>
		/// <param name="enableOutput">Tells to display processing.</param>
		public abstract void PostText(string text, bool enableOutput);
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
		public abstract KeyInfo NameToKeyInfo(string key);
		/// <summary>
		/// Converts a <see cref="KeyInfo"/> to its string representation. Returns null on errors.
		/// </summary>
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
		/// <summary>
		/// Gets confirmation settings (see Far "Confirmations" dialog).
		/// </summary>
		public abstract FarConfirmations Confirmations { get; }
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
		/// Posts a handler to be invoked when user code has finished and the core gets control.
		/// </summary>
		/// <param name="handler">Step handler.</param>
		/// <remarks>
		/// Some core operations are executed only when it gets control, i.e. when user code has finished.
		/// Thus, such operations cannot be invoked as a synchronous sequence.
		/// This method allows invoke them as an asynchronous sequence.
		/// <para>
		/// This mechanism works only when the plugins menu [F11] is available, because it is used internally for stepping.
		/// </para>
		/// <para>
		/// If a step handler starts modal UI without exiting (e.g. dialog) then use <see cref="PostStepAfterStep"/>
		/// if you have another step to be invoked in modal mode (e.g. in a dialog after opening).
		/// </para>
		/// </remarks>
		public abstract void PostStep(Action handler);
		/// <summary>
		/// Posts the keys that normally start modal UI and a handler which is invoked in that modal mode.
		/// </summary>
		/// <param name="keys">Keys starting modal UI.</param>
		/// <param name="handler">Handler to be called in modal mode.</param>
		public abstract void PostStepAfterKeys(string keys, Action handler);
		/// <summary>
		/// Invokes a handler that normally starts modal UI and posts another handler which is invoked in that modal mode.
		/// </summary>
		/// <param name="handler1">Handler starting modal UI.</param>
		/// <param name="handler2">Handler to be called in modal mode.</param>
		/// <remarks>
		/// Steps in <see cref="PostStep"/> work fine if they do not call something modal, like a dialog, for example.
		/// For this special case you should use this method: <b>handler1</b> normally calls something modal (a dialog)
		/// and <b>handler2</b> is posted to be invoked after that (when a dialog is opened).
		/// </remarks>
		public abstract void PostStepAfterStep(Action handler1, Action handler2);
		/// <summary>
		/// Posts a job that will be called by the core when it gets control.
		/// </summary>
		/// <param name="handler">Job handler to invoked.</param>
		/// <remarks>
		/// It is mostly designed for background job calls. Normally other threads are not allowed to call the core.
		/// Violation of this rule may lead to crashes and unpredictable results. This methods is thread safe and it
		/// allowes to post a delayed job that will be called from the main thread as soon as the core gets control.
		/// Thus, this posted job can call the core as usual.
		/// <para>
		/// This method should be used very carefully and only when it is really needed.
		/// </para>
		/// </remarks>
		public abstract void PostJob(Action handler);
		/// <summary>
		/// Gets the current macro area.
		/// </summary>
		public abstract MacroArea MacroArea { get; }
		/// <summary>
		/// Gets the current macro state.
		/// </summary>
		public abstract MacroState MacroState { get; }
		/// <summary>
		/// Generates full path for a temp file or directory in %TEMP% (nothing is created).
		/// </summary>
		/// <param name="prefix">If empty "FTMP" is generated otherwise at most 4 first characters are used and padded by "0".</param>
		/// <returns>Generated name.</returns>
		public abstract string TempName(string prefix);
		/// <summary>
		/// See <see cref="TempName(string)"/>
		/// </summary>
		public string TempName()
		{
			return TempName(null);
		}
		/// <summary>
		/// Creates a folder in %TEMP%.
		/// </summary>
		/// <param name="prefix">If empty "FTMP" is generated otherwise at most 4 first characters are used and padded by "0".</param>
		/// <returns>Full path of the created folder.</returns>
		public abstract string TempFolder(string prefix);
		/// <summary>
		/// See <see cref="TempFolder(string)"/>
		/// </summary>
		public string TempFolder()
		{
			return TempFolder(null);
		}
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
		/// Low level UI operator.
		/// </summary>
		public abstract IUserInterface UI { get; }
		/// <summary>
		/// Gets true if the string matches the pattern compatible with the core file masks.
		/// </summary>
		/// <param name="input">Input string.</param>
		/// <param name="pattern">The pattern: "include-wildcard|exclude-wildcard" or "/regex/".</param>
		/// <returns></returns>
		public abstract bool MatchPattern(string input, string pattern);
		/// <summary>
		/// For internal use. Gets the local or roamimg data directory path of the application.
		/// </summary>
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
	}
	/// <summary>
	/// Represents the thumbnail progress bar state.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
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
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2217:DoNotMarkEnumsWithFlags")]
	[Flags]
	public enum HelpOptions
	{
		/// <summary>
		/// Show the topic from the help file of the calling plugin
		/// (note: it is always FarNet and the path is <c>Far.Net.GetType().Assembly.Location</c>).
		/// If topic begins with a colon ':', the topic from the main Far help file is shown and path is ignored.
		/// </summary>
		None = 0x0,
		/// <summary>
		/// Path is ignored and the topic from the main Far help file is shown.
		/// In this case you do not need to start the topic with a colon ':'.
		/// </summary>
		Far = 1 << 0,
		/// <summary>
		/// Assume path specifies full path to a .hlf file (c:\path\filename).
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
	/// Specifies enumerated constants used to retrieve directory paths to system special folders.
	/// </summary>
	public enum SpecialFolder
	{
		/// <summary>
		/// The directory that serves as a common repository for application-specific data that is used by the current, non-roaming user.
		/// </summary>
		LocalData = 0,
		/// <summary>
		/// The directory that serves as a common repository for application-specific data for the current roaming user.
		/// </summary>
		RoamingData = 1
	}
}
