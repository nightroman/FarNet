/*
PowerShellFar plugin for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Security.Permissions;
using System.Threading;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// PowerShellFar tools exposed by the global variable <c>$Psf</c>.
	/// </summary>
	/// <remarks>
	/// Global PowerShell variables:
	/// <c>$Far</c> is an instance of <see cref="IFar"/>, it exposes FarNet tools.
	/// <c>$Psf</c> is the only instance of this class, it exposes PowerShellFar tools.
	/// <para>
	/// There is no 'Exiting' event because in PS V2 there is a native way using <c>Register-EngineEvent</c>, see examples.
	/// Do not use native Far UI in such a handler, it may not work on exiting. GUI dialogs still can be used.
	/// This way works for any workspace where <c>Register-EngineEvent</c> is called, so that
	/// it can be used by background jobs (PSF and PS), async consoles (local and remote), and etc.
	/// </para>
	/// </remarks>
	/// <example>
	/// <code>
	/// # Do some job on exiting
	/// Register-EngineEvent -SourceIdentifier PowerShell.Exiting -Action { $Far.Msg('See you', 'Exit', 'Gui') }
	/// </code>
	/// </example>
	public sealed class Actor
	{
		// protector
		internal Actor()
		{ }

		#region Life

		/// <summary>
		/// Called on connection internally.
		/// </summary>
		// This is the entry point. It calls OpenRunspace in the end. OpenRunspace adds cmdlets and opens a runspace async.
		// Concurrent methods:
		// *) OnRunspaceStateEvent() is invoked by PS when the runspace is opened or broken;
		// if it is opened it sets global variables and calls PSF and a user profiles; then is sets the flag for Invoking().
		// *) Invoking() is called by FarNet on a user action; it should wait for opened/broken runspace and continue or die.
		internal void Connect()
		{
			// new settings
			_settings = new Settings();

			// preload
			OpenRunspace(false);
		}

		/// <summary>
		/// Called on disconnection internally.
		/// If there are background jobs it shows a dialog about them.
		/// </summary>
		[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
		internal void Disconnect()
		{
			// editor events
			A.Far.AnyEditor.Opened -= EditorKit.OnEditorOpened1;
			A.Far.AnyEditor.Opened -= EditorKit.OnEditorOpened2;

			// kill menu
			UI.ActorMenu.Destroy();

			// kill remaining jobs
			//! after menus, before PS
			Job.StopJobsOnExit();

			// kill host
			if (FarHost != null)
			{
				try
				{
					//! it may be still opening, but Far is already closing, ignore this case,
					//! or we will see for a sec disappearing together with Far error message.
					if (Runspace.RunspaceStateInfo.State == RunspaceState.Opened)
						Runspace.Close();
				}
				finally
				{
					// detach all
					FarUI = null;
					FarHost = null;
					_engine_ = null;
					Pipeline = null;
					Runspace = null;
				}
			}
		}

		void OpenRunspace(bool sync)
		{
			// host and UI
			FarUI = new FarUI();
			FarHost = new FarHost(FarUI);

			// configuration
			RunspaceConfiguration configuration = RunspaceConfiguration.Create();

			// add cmdlets
			Commands.BaseCmdlet.AddCmdlets(configuration);

			// formats: can add now, but let's do it async in profile
			// >: Update-TypeData "$($Psf.AppHome)\PowerShellFar.types.ps1xml"
			// configuration.Types.Append(new TypeConfigurationEntry(Path.Combine(AppHome, "PowerShellFar.types.ps1xml")));

			// runspace
			Runspace = RunspaceFactory.CreateRunspace(FarHost, configuration);
			Runspace.StateChanged += OnRunspaceStateEvent;
			if (sync)
				Runspace.Open();
			else
				Runspace.OpenAsync();
		}

		// universal object for lock
		static readonly object _lock = new object();

		// use only in lock(_lock)
		bool _lock_runspace_opened_or_broken_;

		// tells to sleep
		bool IsRunspaceOpened
		{
			get
			{
				lock (_lock)
				{
					return _lock_runspace_opened_or_broken_;
				}
			}
		}

		//! Fatal error for posponed action.
		Exception _errorFatal;

		//!STOP!
		// With 'Opened' state it is called from another thread.
		// Also, it can be broken, e.g. x86 build may fail on x64 machine.
		void OnRunspaceStateEvent(object sender, RunspaceStateEventArgs e)
		{
			//! Carefully process events other than 'Opened'.
			if (e.RunspaceStateInfo.State != RunspaceState.Opened)
			{
				// alive? do nothing, wait for other events
				if (e.RunspaceStateInfo.State != RunspaceState.Broken)
					return;

				// broken; keep an error silently
				_errorFatal = e.RunspaceStateInfo.Reason;

				//! Set broken flag, so that waiting threads may continue.
				//! The last code, Invoking() can be waiting for this.
				lock (_lock)
				{
					_lock_runspace_opened_or_broken_ = true;
				}
				return;
			}

			//! If it is async then PS catches all and adds errors to $Error.
			//! Thus, we don't catch anything, because this is normally async.
			try
			{
				//! Bug (_090315_091325)
				//! Get engine once to avoid this: "A pipeline is already executing. Concurrent SessionStateProxy method call is not allowed."
				//! Looks like a hack, but it works fine. Problem case: run Test-CallStack-.ps1, Esc -> the error above.
				//! SVN tag 4.2.26
				_engine_ = Runspace.SessionStateProxy.PSVariable.GetValue("ExecutionContext") as EngineIntrinsics;

				// new variables
				PSVariable var1 = new PSVariable("Psf", this, ScopedItemOptions.AllScope | ScopedItemOptions.Constant);
				var1.Description = "PowerShellFar plugin object";
				Engine.SessionState.PSVariable.Set(var1);
				PSVariable var2 = new PSVariable("Far", A.Far, ScopedItemOptions.AllScope | ScopedItemOptions.Constant);
				var2.Description = "Far object exposed by FarNet";
				Engine.SessionState.PSVariable.Set(var2);

				// invoke internal profile (NB: there is trap in there) and startup code
				using (Pipeline p = Runspace.CreatePipeline())
				{
					p.Commands.AddScript(Resource.PowerShellFar);
					p.Invoke();
				}

				// invoke user startup code, separately for better diagnostics
				if (!string.IsNullOrEmpty(Settings.PluginStartupCode))
				{
					try
					{
						using (Pipeline p = Runspace.CreatePipeline())
						{
							p.Commands.AddScript(Settings.PluginStartupCode);
							p.Invoke();
						}
					}
					catch (RuntimeException ex)
					{
						string msg = Kit.Format(@"
Plugin startup code failed.

Code (see configuration):
{0}

Reason (see also $Error):
{1}
", Settings.PluginStartupCode, ex.Message);
						A.Far.Msg(msg, Res.Name, MsgOptions.Warning | MsgOptions.Gui | MsgOptions.Ok);
					}
				}
			}
			finally
			{
				//! The last code, Invoking() can be waiting for this.
				lock (_lock)
				{
					_lock_runspace_opened_or_broken_ = true;
				}
			}
		}

		/// <summary>
		/// Called by FarNet on command line and by PowerShellFar on its actions.
		/// </summary>
		/// <remarks>
		/// *) No Far (!) interaction is allowed, a macro can be in progress.
		/// *) It opens a runspace if not yet and waits for it.
		/// </remarks>
		internal void Invoking()
		{
			if (FarHost == null)
				OpenRunspace(true);

			//! If something went wrong, perhaps async, unregister and throw; hopefully we are detached completely after that.
			if (_errorFatal != null)
			{
				//! emergency
				Entry.Unregister();
				throw new PluginException(@"
PowerShell engine is not initialized due to fatal reasons and will be unloaded.
See also the section PROBLEMS AND SOLUTIONS in the Readme.txt for known issues.
", _errorFatal);
			}

			// complete opening
			if (Runspace.DefaultRunspace == null)
			{
				//! wait for initialization
				while (!IsRunspaceOpened)
					System.Threading.Thread.Sleep(100);

				//! set default runspace for handlers
				//! it has to be done in main thread
				Runspace.DefaultRunspace = Runspace;

				// add the debug handler
				//?? what if it is already added by profile?
				Runspace.Debugger.DebuggerStop += OnDebuggerStop;

				//! Check and notify about startup errors, remember: no interaction.
				ArrayList errors = Engine.SessionState.PSVariable.GetValue("Error") as ArrayList;
				if (errors != null && errors.Count > 0)
				{
					A.Far.Msg(@"
The startup code was invoked with errors.
View the error list or the variable $Error.
", "PowerShellFar startup errors", MsgOptions.Gui);
				}
			}
		}

		/// <summary>
		/// Sync provider location and current directory with Far state.
		/// </summary>
		/// <remarks>
		/// Returned system path (if not null) must be restored by a called.
		/// </remarks>
		internal string SyncPaths()
		{
			// don't on running
			if (IsRunning)
				return null;

			// don't on no panels mode
			IPanel panel = A.Far.Panel;
			if (panel == null)
				return null;

			// at first get both paths: for the current system directory and provider location
			string directory = A.Far.ActivePath;
			string location = null;
			if (panel.IsPlugin)
			{
				IPluginPanel plugin = panel as IPluginPanel;
				if (plugin != null)
				{
					ItemPanel itemPanel = plugin.Host as ItemPanel;
					if (itemPanel != null)
					{
						location = itemPanel.Location.Path;
					}
					else
					{
						FolderTree folderTree = plugin.Host as FolderTree;
						if (folderTree != null)
							location = panel.Path;
						else
							location = directory;
					}
				}
			}

			// to set yet unknown location to the directory
			if (location == null)
				location = directory;

			// set the current provider location; let's do it first, in case of failure
			// we can skip directory setting/restoring in cases when they are the same.
			bool okLocation = true;
			try
			{
				//! Parameter is wildcard. Test: enter into a container "[]" and invoke a command.
				Engine.SessionState.Path.SetLocation(Kit.EscapeWildcard(location));

				// drop failure info
				_failedInvokingLocationNew = null;
				_failedInvokingLocationOld = null;
			}
			catch
			{
				okLocation = false;

				// get the current
				string currentLocation = Engine.SessionState.Path.CurrentLocation.Path;

				// ask a user if he has not told to ignore this pair
				if (location != _failedInvokingLocationNew || currentLocation != _failedInvokingLocationOld)
				{
					string message = Kit.Format(@"
Cannot set the current location to
{0}

Continue with this current location?
{1}
", location, currentLocation);

					switch (A.Far.Msg(message, Res.Name, MsgOptions.GuiOnMacro | MsgOptions.AbortRetryIgnore | MsgOptions.Warning | MsgOptions.LeftAligned))
					{
						case 1:
							break;
						case 2:
							_failedInvokingLocationNew = location;
							_failedInvokingLocationOld = currentLocation;
							break;
						default:
							if (A.Far.MacroState != FarMacroState.None)
								A.Far.Zoo.Break();
							throw;
					}
				}
			}

			// do not try failed
			if (!okLocation && location == directory)
				return null;

			// get the current directory to be restored by a caller
			string currentDirectory = Directory.GetCurrentDirectory();

			// set the current directory to the active path to avoid confusions [_090929_061740]
			try
			{
				// try to set
				Directory.SetCurrentDirectory(directory);

				// drop failure info
				_failedInvokingDirectoryNew = null;
				_failedInvokingDirectoryOld = null;
			}
			catch
			{
				// ask a user if he has not told to ignore this pair
				if (directory != _failedInvokingDirectoryNew || currentDirectory != _failedInvokingDirectoryOld)
				{
					string message = Kit.Format(@"
Cannot set the current directory to
{0}

Continue with this current directory?
{1}
", directory, currentDirectory);

					switch (A.Far.Msg(message, Res.Name, MsgOptions.GuiOnMacro | MsgOptions.AbortRetryIgnore | MsgOptions.Warning | MsgOptions.LeftAligned))
					{
						case 1:
							currentDirectory = null;
							break;
						case 2:
							currentDirectory = null;
							_failedInvokingDirectoryNew = directory;
							_failedInvokingDirectoryOld = currentDirectory;
							break;
						default:
							if (A.Far.MacroState != FarMacroState.None)
								A.Far.Zoo.Break();
							throw;
					}
				}
			}

			// to be restored by a caller
			return currentDirectory;
		}
		string _failedInvokingDirectoryNew;
		string _failedInvokingDirectoryOld;
		string _failedInvokingLocationNew;
		string _failedInvokingLocationOld;

		// Installed before interactive invocation to watch [CtrlC] for stopping.
		void OnTimer(object state)
		{
			if (Pipeline == null)
				return;

			while (Console.KeyAvailable)
			{
				ConsoleKeyInfo k = Console.ReadKey(true);
				if (k.Key == ConsoleKey.C || k.Modifiers == ConsoleModifiers.Control)
					Pipeline.BeginStop(AsyncStop, Pipeline);
			}
		}
		void AsyncStop(IAsyncResult ar)
		{
			(ar.AsyncState as PowerShell).EndStop(ar);
		}

		#endregion

		Settings _settings;
		/// <summary>
		/// Gets the configuration settings and the session settings.
		/// </summary>
		/// <remarks>
		/// The configuration settings (<c>.Plugin*</c>) are used see the configuration dialog.
		/// The current session preferences are usually set in the profile.
		/// <para>
		/// See also .hlf topic [Plugin settings].
		/// </para>
		/// </remarks>
		public PowerShellFar.Settings Settings
		{
			get { return _settings; }
		}

		/// <summary>
		/// Gets a list of active editor lines.
		/// </summary>
		/// <remarks>
		/// Gets <see cref="IEditor.Selection"/> if stream selection exists,
		/// else <see cref="IEditor.Lines"/> if editor exists,
		/// else null.
		/// <para>
		/// You can not just get line strings but add lines (as strings) to the list,
		/// remove items and modify items (<see cref="ILine"/>).
		/// </para>
		/// </remarks>
		public ILines HotLines
		{
			get { return EditorKit.HotLines; }
		}

		/// <summary>
		/// Gets or sets the active text of active editor or editor line.
		/// </summary>
		/// <remarks>
		/// Gets or sets selected text if selection exists in the current editor or an editor line,
		/// else a line text if any kind of editor line is active.
		/// </remarks>
		public string HotText
		{
			get { return EditorKit.HotText; }
			set { EditorKit.HotText = value; }
		}

		/// <summary>
		/// Adds an action to all menus (Dialog, Editor, Panels, Viewer).
		/// </summary>
		/// <param name="text">Menu item text. Use ampersand to set a hotkey.</param>
		/// <param name="handler">Action script block. Keep it simple, normally it should just call other commands.</param>
		/// <remarks>
		/// Actions can be added any time, but the best practice is to add them from the startup code.
		/// <para>
		/// Use $null action to add a separator to menus.
		/// </para>
		/// </remarks>
		public void Action(string text, EventHandler handler)
		{
			UI.ActorMenu.AddUserTool(text, handler, ToolOptions.None);
		}

		/// <summary>
		/// Adds an action to the specified menus (combination of Dialog, Editor, Panels, Viewer).
		/// </summary>
		/// <param name="text">Menu item text. Use ampersand to set a hotkey.</param>
		/// <param name="handler">Action script block. Keep it simple, normally it should just call other commands.</param>
		/// <param name="area">Where this action should be available in a menu: combination of Dialog, Editor, Panels, Viewer.</param>
		/// <remarks>
		/// Actions can be added any time, but the best practice is to add them from the startup code.
		/// <para>
		/// Use $null action to add a separator to menus.
		/// </para>
		/// </remarks>
		public void Action(string text, EventHandler handler, ToolOptions area)
		{
			UI.ActorMenu.AddUserTool(text, handler, area);
		}

		/// <summary>
		/// Gets the editor or throws.
		/// </summary>
		/// <remarks>
		/// Gets the editor associated with the current window or throws, if none.
		/// It just helps to avoid boring checks in many editor scripts.
		/// </remarks>
		/// <exception cref="InvalidOperationException">Editor is not opened or its window is not current.</exception>
		public IEditor Editor()
		{
			IEditor editor = A.Far.Editor;
			if (editor == null || A.Far.WindowType != WindowType.Editor)
				throw new InvalidOperationException(Res.NeedsEditor);

			return editor;
		}

		/// <summary>
		/// Returns PowerShellFar home path. Designed for internal use.
		/// </summary>
		public string AppHome
		{
			get { return Path.GetDirectoryName((Assembly.GetExecutingAssembly()).Location); }
		}

		static string _AppData;
		/// <summary>
		/// Returns PowerShellFar data path and ensures once that the directory exists.
		/// </summary>
		public string AppData
		{
			get
			{
				if (_AppData == null)
				{
					string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Res.Name);
					if (!Directory.Exists(path))
						Directory.CreateDirectory(path);

					_AppData = path;
				}

				return _AppData;
			}
		}

		/// <summary>
		/// Returns PowerShellFar string for HelpTopic. Designed for internal use.
		/// </summary>
		public string HelpTopic
		{
			get { return "<" + AppHome + "\\>"; }
		}

		/// <summary>
		/// Shows an input dialog and returns entered PowerShell code.
		/// </summary>
		/// <remarks>
		/// It is called by the plugin menu command "Invoke input code". You may call it, too.
		/// It is just an input box for any text but it is designed for PowerShell code input,
		/// e.g. TabExpansion is enabled (by [Tab]).
		/// <para>
		/// The code is simply returned, if you want to execute it then call <see cref="InvokeInputCode"/>.
		/// </para>
		/// <para>
		/// When a macro is in progress a simple input box is used instead of the dialog.
		/// </para>
		/// </remarks>
		public string InputCode()
		{
			if (A.Far.MacroState == FarMacroState.None || A.Psf.Settings.Test == 100120151157)
			{
				// normal mode
				UI.InputDialog ui = new UI.InputDialog(Res.Name, Res.Name, "PowerShell code");
				ui.UICode.IsPath = true;
				ui.UICode.UseLastHistory = true;
				return ui.UIDialog.Show() ? ui.UICode.Text : null;
			}
			else
			{
				// macro mode
				return A.Far.Input(null);
			}
			
		}

		/// <summary>
		/// Prompts to input the code and invokes it.
		/// Called on "Invoke input code".
		/// </summary>
		/// <remarks>
		/// When a macro is in progress a simple input box is used instead of the dialog
		/// and invoked commands are not stored in the command history.
		/// <para>
		/// To input and get some code without invoking use the <see cref="InputCode"/> method.
		/// </para>
		/// </remarks>
		public void InvokeInputCode()
		{
			string code = InputCode();
			if (code != null)
				InvokePipeline(code, null, A.Far.MacroState == FarMacroState.None);
		}

		/// <summary>
		/// Invokes the selected text or the current line text in the editor or the command line.
		/// Called on "Invoke selected code".
		/// </summary>
		public void InvokeSelectedCode()
		{
			EditorKit.InvokeSelectedCode();
		}

		/// <summary>
		/// Checks whether it is possible to exit the session safely (may require user interaction).
		/// </summary>
		/// <returns>true if exit is safe.</returns>
		/// <remarks>
		/// If there are background jobs this methods calls <see cref="ShowJobs"/>
		/// so that you are prompted to remove jobs manually. If you do not remove all the jobs
		/// then the method returns false.
		/// <para>
		/// It can be used to prevent closing of Far by [F10] with existing background jobs.
		/// </para>
		/// </remarks>
		public bool CanExit()
		{
			return Job.CanExit();
		}

		/// <summary>
		/// Gets PowerShellFar commands from history.
		/// </summary>
		/// <remarks>
		/// PowerShellFar command history is absolutely different from PowerShell command history; PowerShell mechanism is not used
		/// internally and you should not use it, i.e. forget about <c>Add-History</c>, <c>$MaximumHistoryCount</c>, and etc. - you
		/// don't need them in PowerShellFar. The history is stored in the registry, so that commands can be used in other sessions.
		/// <para>
		/// Some standard history commands are partially implemented as internal functions.
		/// <c>Get-History</c> returns command strings, <c>Invoke-History</c> calls <see cref="ShowHistory"/>.
		/// </para>
		/// </remarks>
		/// <param name="count">Number of last commands to be returned. 0: all commands.</param>
		public IList<string> GetHistory(int count)
		{
			return History.GetLines(count);
		}

		/// <summary>
		/// Shows the configuration dialog.
		/// Called on selection of the plugin config item.
		/// </summary>
		/// <seealso cref="Settings"/>
		public bool ShowSettings()
		{
			return (new UI.SettingsDialog()).Show();
		}

		/// <summary>
		/// Shows a new modal editor console.
		/// </summary>
		/// <remarks>
		/// This method opens a modal editor console, it can be called in the middle of something to perform actions manually
		/// and then to continue interrupted execution on exit. Basically it is so called PowerShell nested prompt.
		/// </remarks>
		public void ShowConsole()
		{
			EditorConsole console = EditorConsole.CreateConsole(true);
			if (console != null)
				console.Editor.Open(OpenMode.Modal);
		}

		/// <summary>
		/// Shows a new editor console in specified mode.
		/// Called on "Editor console".
		/// </summary>
		public void ShowConsole(OpenMode mode)
		{
			EditorConsole console = EditorConsole.CreateConsole(true);
			if (console != null)
				console.Editor.Open(mode);
		}

		/// <summary>
		/// Shows a menu of available PowerShellFar panels to open.
		/// Called on "Power panel".
		/// </summary>
		public void ShowPanel()
		{
			string currentDirectory = A.Psf.SyncPaths();
			try
			{
				string drive = AnyPanel.SelectDrivePrompt(null);
				if (drive == null)
					return;

				AnyPanel ap;
				if (drive == "Folder &tree")
					ap = new FolderTree();
				else if (drive == "&Any objects")
					ap = new ObjectPanel();
				else
					ap = new ItemPanel(drive);
				ap.Show();
			}
			finally
			{
				A.SetCurrentDirectoryFinally(currentDirectory);
			}
		}

		/// <summary>
		/// Shows the background job list.
		/// Called on "Background jobs" and by <see cref="CanExit"/>.
		/// </summary>
		public void ShowJobs()
		{
			Job.ShowJobs();
		}

		/// <summary>
		/// Shows PowerShellFar command history and invokes or insert the selected command or text.
		/// Called on "Command history".
		/// </summary>
		/// <seealso cref="GetHistory"/>
		public void ShowHistory()
		{
			History.ShowHistory();
		}

		/// <summary>
		/// Shows a menu with available modules and registered snap-ins.
		/// Called on "Module+".
		/// </summary>
		public void ShowModules()
		{
			UI.ModulesMenu ui = new PowerShellFar.UI.ModulesMenu();
			ui.Show();
		}

		/// <summary>
		/// Shows PowerShell debugger tools menu.
		/// </summary>
		public void ShowDebugger()
		{
			UI.DebuggerMenu ui = new UI.DebuggerMenu();
			ui.Show();
		}

		/// <summary>
		/// Shows PowerShell errors.
		/// </summary>
		public void ShowErrors()
		{
			UI.ErrorsMenu ui = new UI.ErrorsMenu();
			ui.Show();
		}

		/// <summary>
		/// Shows PowerShell help, normally for the current token in any editor line.
		/// </summary>
		public void ShowHelp()
		{
			Help.ShowHelp();
		}

		/// <summary>
		/// Expands PowerShell code in an edit line.
		/// </summary>
		/// <param name="editLine">Editor line, command line or dialog edit box line; if null then <see cref="IFar.Line"/> is used.</param>
		/// <remarks>
		/// It implements so called TabExpansion using a menu and inserting a selected text into a current line being edited.
		/// The edit line can belong to the internal editor, the command line or a dialogs.
		/// <para>
		/// When it is called the first time it loads the script TabExpansion.ps1 from the plugin directory
		/// which installs the global function TabExpansion. After that this function is always called and
		/// returned selected text is inserted into the edit line.
		/// </para>
		/// </remarks>
		public void ExpandCode(ILine editLine)
		{
			EditorKit.ExpandCode(editLine);
		}

		// PS host
		FarHost FarHost;

		// PS UI
		FarUI FarUI;

		// PS runspace
		internal Runspace Runspace { get; private set; }

		// Main pipeline
		PowerShell Pipeline;

		// PS engine
		EngineIntrinsics _engine_;
		internal EngineIntrinsics Engine
		{
			get { return _engine_; }
		}

		/// <summary>
		/// Gets a new pipeline or nested one.
		/// </summary>
		/// <returns>Pipeline; it has to be disposed.</returns>
		internal PowerShell CreatePipeline()
		{
			if (IsRunning)
				return Pipeline.CreateNestedPowerShell();

			Pipeline = PowerShell.Create();
			Pipeline.Runspace = Runspace;
			return Pipeline;
		}

		/// <summary>
		/// Is it running?
		/// </summary>
		bool IsRunning
		{
			get { return Pipeline != null && Pipeline.InvocationStateInfo.State == PSInvocationState.Running; }
		}

		// Current command being invoked (e.g. used as Out-FarPanel title)
		internal string _myCommand;

		// Last invoked command (e.g. to reduce dupes in the history)
		internal string _myLastCommand;

		/// <summary>
		/// Invokes PowerShell code with pipeline.
		/// </summary>
		/// <param name="code">PowerShell code.</param>
		/// <param name="writer">Output writer or null.</param>
		/// <param name="addHistory">Add command to history.</param>
		internal bool InvokePipeline(string code, AnyOutputWriter writer, bool addHistory)
		{
			// result
			bool ok = true;

			// drop history cache
			History.Cache = null;

			// push writer
			FarUI.PushWriter(writer ?? new StringOutputWriter());

			// install timer
			Timer timer = new Timer(OnTimer, null, 3000, 1000);

			// invoke
			try
			{
				// win7 Indeterminate
				A.Far.SetProgressState(TaskbarProgressBarState.Indeterminate);

				// add history
				if (addHistory)
				{
					code = code.Trim();
					if (code.Length > 0 && code[code.Length - 1] != '#' && code != _myLastCommand)
						History.AddLine(code);
				}

				// invoke command
				using (PowerShell ps = CreatePipeline())
				{
					_myCommand = code;
					ps.Commands
						.AddScript(code)
						.AddCommand(A.OutCommand);
					ps.Invoke();
				}
			}
			catch (RuntimeException reason)
			{
				ok = false;
				using (PowerShell ps = CreatePipeline())
					A.OutReason(ps, reason);
			}
			finally
			{
				// stop timer
				timer.Dispose();

				// win7 NoProgress
				A.Far.SetProgressState(TaskbarProgressBarState.NoProgress);

				_myLastCommand = _myCommand;
				_myCommand = null;

				// pop writer
				//! this method might add 'StringOutputWriter' but it might be already consumed and replaced
				AnyOutputWriter usedWriter = FarUI.PopWriter();
				StringOutputWriter myWriter = usedWriter as StringOutputWriter;
				if (myWriter == null)
				{
					ExternalOutputWriter externalWriter = usedWriter as ExternalOutputWriter;
					if (externalWriter != null)
						externalWriter.Dispose();
				}
				else
				{
					// show output
					string output = myWriter.Output.ToString();

					// show results
					if (output.Length > 0)
					{
						// use log?
						WindowType wt = A.Far.WindowType;
						bool useLog = (wt != WindowType.Panels);
						if (!useLog)
						{
							// count lines
							int nNewLine = 0;
							int nMax = Console.WindowHeight - 3;
							IPanel p1 = A.Far.Panel;
							if (p1.IsVisible)
							{
								Place w = p1.Window;
								nMax -= w.Bottom;
							}
							foreach (char c in output)
							{
								if (c == '\n')
								{
									if (++nNewLine > nMax)
									{
										useLog = true;
										break;
									}
								}
							}
						}

						try
						{
							// use log file
							if (useLog)
							{
								// view output
								A.Far.AnyViewer.ViewText(output, code, OpenMode.None);
								output = string.Empty;
							}
						}
						finally
						{
							// write to console
							if (output.Length > 0)
								A.Far.Write(output);
						}
					}
				}

				// notify host
				FarHost.NotifyEndApplication();
			}
			return ok;
		}

		/// <summary>
		/// Provider settings.
		/// </summary>
		/// <remarks>
		/// These settings are optional but they help to configure appearance of provider data.
		/// See <c>Profile-.ps1</c> for examples.
		/// <para>
		/// Keys are provider names, e.g. 'FileSystem', 'Registry', 'Alias', and etc.
		/// Keys are case sensitive by default, but you can replace the hashtable with case insensitive (e.g. @{...}).
		/// </para>
		/// <para>
		/// Values are dictionaries with keys mapped to property names of <see cref="ItemPanel"/>,
		/// e.g. <see cref="TablePanel.Columns"/>, <see cref="TablePanel.ExcludeMembers"/>, and etc.
		/// Their values depend on that properties, see help.
		/// </para>
		/// </remarks>
		public IDictionary Providers
		{
			get { return _Providers; }
			set { _Providers = value == null ? new Hashtable() : value; }
		}
		IDictionary _Providers = new Hashtable();

		/// <summary>
		/// Invokes step processing.
		/// </summary>
		/// <remarks>
		/// This is a helper method to invoke a step sequence. Step sequence is usually kept in a step unit script.
		/// It is recommended to use some naming convension to distinguish between these scripts and the others.
		/// <para>
		/// For example, assume that step unit scripts are named as "*+.ps1". Then we can create Far Manager association:
		/// </para>
		/// <ul>
		/// <li>Mask: *+.ps1</li>
		/// <li>Command (Enter): >: $Psf.Go((&amp; '.\!.!')) #</li>
		/// </ul>
		/// <para>
		/// Having done this we can press enter on "*+.ps1" files and their steps will be invoked.
		/// </para>
		/// </remarks>
		public void Go(object[] steps)
		{
			Stepper stepper = new Stepper();
			stepper.Go(steps);
		}

		/// <summary>
		/// For internal use and development experiments.
		/// </summary>
		public Collection<PSObject> InvokeCode(string scriptText, params object[] args)
		{
			return Engine.InvokeCommand.NewScriptBlock(scriptText).Invoke(args);
		}

		/// <summary>
		/// Invokes a script from the current editor.
		/// </summary>
		/// <remarks>
		/// Output, if any, is written into a tmp file and an external viewer is started to view it.
		/// Why external: to be able to view output and at the same time perform some modal operations
		/// e.g. debugging. You can set your external viewer, see <see cref="PowerShellFar.Settings"/>.
		/// </remarks>
		public void InvokeScriptFromEditor()
		{
			EditorKit.InvokeScriptFromEditor(null);
		}

		void OnDebuggerStop(object sender, DebuggerStopEventArgs e)
		{
			// replace writer to external
			StringOutputWriter writer1 = FarUI.Writer as StringOutputWriter;
			if (writer1 != null)
			{
				ExternalOutputWriter writer2 = new ExternalOutputWriter();

				string output = writer1.Output.ToString();
				if (output.Length > 0)
					writer2.Append(output);

				FarUI.PopWriter();
				FarUI.PushWriter(writer2);
			}

			// show debug dialog
			UI.DebuggerDialog ui = new UI.DebuggerDialog(e);
			e.ResumeAction = ui.Show();
		}

		/// <summary>
		/// Gets currently running stepper instance if any or null.
		/// </summary>
		/// <remarks>
		/// It is designed mosttly for use from a step script block being processed.
		/// It's fine to use this in order to check stepping mode by not null result.
		/// </remarks>
		public Stepper Stepper { get { return Stepper.RunningInstance; } }

	}
}
