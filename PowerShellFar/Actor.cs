using FarNet;
using System.Collections;

namespace PowerShellFar;
#pragma warning disable CA1822

/// <summary>
/// PowerShellFar tools exposed by the global variable <c>$Psf</c>.
/// </summary>
/// <remarks>
/// <para>
/// Global PowerShell variables:
/// <list>
/// <item>
/// <c>$Far</c> is the instance of <see cref="IFar"/>.
/// </item>
/// <item>
/// <c>$Psf</c> is the instance of this class.
/// </item>
/// </list>
/// </para>
/// <para>
/// There is no exiting event because there is a native way using <c>Register-EngineEvent</c>, see examples.
/// Do not use Far UI in a handler, it may not work on exiting. GUI dialogs still can be used.
/// This way works for any workspace where <c>Register-EngineEvent</c> is called.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// # Show some GUI dialog on exit
/// Register-EngineEvent -SourceIdentifier PowerShell.Exiting -Action { $Far.Message('See you', 'Exit', 'Gui') }
/// </code>
/// </example>
public sealed partial class Actor
{
	// guard
	internal Actor()
	{
	}

	/// <summary>
	/// Gets the configuration settings and the session settings.
	/// </summary>
	/// <remarks>
	/// Session preferences are usually set in the profile.
	/// Or change them temporary in the panel, command, script.
	/// <para>
	/// See also the manual [Settings].
	/// </para>
	/// </remarks>
	/// <example>
	/// <code>
	/// # Show settings in the panel
	/// Open-FarPanel $Psf.Settings
	/// </code>
	/// </example>
	public Settings Settings => Settings.Default;

	/// <summary>
	/// Gets or sets the active text of active editor or editor line.
	/// </summary>
	/// <remarks>
	/// Gets or sets selected text if selection exists in the current editor or an editor line,
	/// else a line text if any kind of editor line is active.
	/// </remarks>
	public string ActiveText
	{
		get => EditorKit.ActiveText;
		set => EditorKit.ActiveText = value;
	}

	/// <summary>
	/// Gets the active editor or throws an error.
	/// </summary>
	/// <remarks>
	/// This method gets the editor associated with the current window.
	/// If it is not an editor window then an error is thrown.
	/// <para>
	/// The method is used by scripts designed for editors.
	/// </para>
	/// </remarks>
	/// <exception cref="InvalidOperationException">The current window must be an editor.</exception>
	public IEditor Editor()
	{
		if (Far.Api.Window.Kind != WindowKind.Editor)
			throw new InvalidOperationException(Res.NeedsEditor);

		return Far.Api.Editor!;
	}

	/// <summary>
	/// Returns PowerShellFar home path. Designed for internal use.
	/// </summary>
	public string AppHome => Path.GetDirectoryName(typeof(Actor).Assembly.Location)!;

	/// <summary>
	/// Gets PowerShellFar commands from history.
	/// </summary>
	/// <remarks>
	/// PowerShellFar command history is absolutely different from PowerShell command history; PowerShell mechanism is not used
	/// internally and you should not use it, i.e. forget about <c>Add-History</c>, <c>$MaximumHistoryCount</c>, and etc. - you
	/// don't need them in PowerShellFar. The history is stored in a file, so that commands can be used in other sessions.
	/// <para>
	/// Some standard history commands are partially implemented as internal functions.
	/// <c>Get-History</c> returns command strings, <c>Invoke-History</c> calls <see cref="ShowHistory"/>.
	/// </para>
	/// </remarks>
	/// <param name="count">Number of last commands to be returned. 0: all commands.</param>
	public IList<string> GetHistory(int count)
	{
		var lines = HistoryKit.ReadLines();
		if (count <= 0 || count >= lines.Length)
			return lines;

		var list = new List<string>(lines);
		list.RemoveRange(0, list.Count - count);
		return list;
	}

	/// <summary>
	/// Shows a new main session interactive in the specified mode.
	/// </summary>
	/// <param name="mode">The editor open mode. Default: <see cref="OpenMode.Modal"/>.</param>
	/// <param name="session">Session kind: 0: main; 1: local; 2: remote.</param>
	/// <remarks>
	/// The modal interactive may be called in the middle of something to perform actions manually
	/// and then continue interrupted execution on exit. It is similar to PowerShell nested prompt.
	/// </remarks>
	public void ShowInteractive(OpenMode mode = OpenMode.Modal, int session = 0)
	{
		var interactive = Interactive.Create(session, false);
		interactive.Editor.Open(mode);
	}

	/// <summary>
	/// Shows a menu of available PowerShellFar panels to open.
	/// Called on "Power panel".
	/// </summary>
	public void ShowPanel()
	{
		A.SyncPaths();

		var drive = UI.SelectMenu.SelectPowerPanel();
		if (drive is null)
			return;

		AnyPanel panel;
		if (drive == UI.SelectMenu.TextFolderTree)
			panel = new FolderTree();
		else if (drive == UI.SelectMenu.TextAnyObjects)
			panel = new ObjectPanel();
		else
			panel = new ItemPanel(drive);

		panel.Open();
	}

	/// <summary>
	/// Shows PowerShell command history.
	/// Called on "Command history".
	/// </summary>
	/// <remarks>
	/// The selected command is invoked or inserted to known editors or command box.
	/// </remarks>
	/// <seealso cref="GetHistory"/>
	public void ShowHistory()
	{
		HistoryKit.ShowHistory();
	}

	/// <summary>
	/// Shows PowerShell debugger tools menu.
	/// Called on "Debugger".
	/// </summary>
	public void ShowDebugger()
	{
		var ui = new UI.DebuggerMenu();
		ui.Show();
	}

	/// <summary>
	/// Shows PowerShell errors.
	/// </summary>
	public void ShowErrors()
	{
		var ui = new UI.ErrorsMenu();
		ui.Show();
	}

	/// <summary>
	/// Shows help, normally for the current command or parameter in an editor line.
	/// </summary>
	/// <remarks>
	/// For the current token in an editor line (editor, editbox, cmdline) it gets help
	/// information and shows it in the viewer. In code editors and input code boxes
	/// this action is associated with [ShiftF1].
	/// </remarks>
	public void ShowHelp() => Help.ShowHelpForContext();

	/// <summary>
	/// Expands PowerShell code in the specified edit line.
	/// </summary>
	/// <param name="editLine">Editor line, command line or dialog edit box line; if null then <see cref="IFar.Line"/> is used.</param>
	public void ExpandCode(ILine? editLine) => EditorKit.ExpandCode(editLine, null);

	/// <summary>
	/// Provider settings.
	/// </summary>
	/// <remarks>
	/// These optional settings configure appearance of provider data.
	/// See <c>Profile-.ps1</c> for examples.
	/// <para>
	/// Keys are provider names, e.g. 'FileSystem', 'Registry', 'Alias', and etc.
	/// Keys are case sensitive by default, but you can replace the hashtable with case insensitive (e.g. @{...}).
	/// </para>
	/// <para>
	/// Values are dictionaries with keys mapped to property names of <see cref="ItemPanel"/>,
	/// e.g. <see cref="TablePanel.Columns"/>, <see cref="TablePanel.ExcludeMemberPattern"/>, and etc.
	/// Their values depend on that properties, see help.
	/// </para>
	/// </remarks>
	public IDictionary Providers
	{
		get => _Providers;
		set => _Providers = value ?? new Hashtable();
	}
	IDictionary _Providers = new Hashtable();

	/// <summary>
	/// Invokes the script opened in the current editor.
	/// </summary>
	/// <remarks>
	/// [F5] is the hardcoded shortcut. A different key can be used with a macro:
	/// see the example macro in the manual.
	/// <para>
	/// The action is the same as to invoke the script from the input command box
	/// but if the file is modified then it is saved before invoking.
	/// </para>
	/// </remarks>
	public void InvokeScriptFromEditor()
	{
		EditorKit.InvokeScriptFromEditor(null);
	}

	/// <summary>
	/// FarNet module manager of the PowerShellFar module.
	/// </summary>
	/// <remarks>
	/// It may be used in scripts in order to register new module actions, get/set the current UI culture, and etc.
	/// <para>
	/// In order to just get the current UI culture better use the standard PowerShell way:
	/// <c>$PSUICulture</c> or <c>$Host.CurrentUICulture</c>
	/// </para>
	/// </remarks>
	public IModuleManager Manager => Entry.Instance.Manager;

	#region CommandConsole
	private static UI.InputBox2 CreateCodeDialog()
	{
		var ui = new UI.InputBox2(Res.InvokeCommands, Res.Me);
		ui.Edit.History = Res.History;
		ui.Edit.UseLastHistory = true;
		return ui;
	}

	/// <summary>
	/// Shows an input dialog and returns entered PowerShell code.
	/// </summary>
	/// <remarks>
	/// It is called by the plugin menu command "Invoke commands". You may call it, too.
	/// It is just an input box for any text but it is designed for PowerShell code input,
	/// e.g. TabExpansion is enabled (by [Tab]).
	/// <para>
	/// The code is simply returned, if you want to execute it then call <see cref="InvokeInputCode"/>.
	/// </para>
	/// </remarks>
	public string? InputCode()
	{
		var ui = CreateCodeDialog();
		return ui.Show();
	}

	/// <summary>
	/// Prompts to input code and invokes it.
	/// </summary>
	public void InvokeInputCode()
	{
		InvokeInputCodePrivate(null);
	}

	internal static void InvokeInputCodePrivate(string? input)
	{
		var ui = CreateCodeDialog();
		if (input != null)
			ui.Edit.Text = input;
		var code = ui.Show();
		if (!string.IsNullOrEmpty(code))
			A.Run(new RunArgs(code));
	}

	/// <summary>
	/// Invokes the selected text or the current line text in the editor or the command line.
	/// Called on "Invoke selected".
	/// </summary>
	public void InvokeSelectedCode()
	{
		EditorKit.InvokeSelectedCode();
	}

	/// <summary>
	/// Prompts for PowerShell commands.
	/// Called on "Invoke commands".
	/// </summary>
	public async Task StartInvokeCommands()
	{
		var ui = CreateCodeDialog();
		var code = await ui.ShowAsync();
		if (!string.IsNullOrEmpty(code))
			await Tasks.Job(() => A.Run(new RunArgs(code) { UseTeeResult = true }));
	}

	/// <summary>
	/// Starts "Command console".
	/// </summary>
	public void StartCommandConsole()
	{
		_ = UI.ReadCommand.StartAsync();
	}

	/// <summary>
	/// Stops "Command console".
	/// </summary>
	public void StopCommandConsole()
	{
		UI.ReadCommand.Stop();
	}

	/// <summary>
	/// Starts "Command console" and waits for it.
	/// </summary>
	public Task RunCommandConsole()
	{
		StartCommandConsole();
		return FarNet.Works.Tasks2.Wait(nameof(StartCommandConsole), () =>
			Far.Api.Window.Kind == WindowKind.Dialog &&
			Far.Api.Dialog!.TypeId == new Guid(Guids.ReadCommandDialog));
	}
	#endregion
}
