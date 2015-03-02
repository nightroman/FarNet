
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
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
	/// Register-EngineEvent -SourceIdentifier PowerShell.Exiting -Action { $Far.Message('See you', 'Exit', 'Gui') }
	/// </code>
	/// </example>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public sealed class Actor
	{
		// guard
		internal Actor()
		{ }
		/// <summary>
		/// Stops the running pipeline.
		/// </summary>
		void CancelKeyPress(object sender, ConsoleCancelEventArgs e) //_110128_075844
		{
			// ControlBreak?
			if (e.SpecialKey != ConsoleSpecialKey.ControlBreak)
				return;

			//! use copy
			var pipe = Pipeline;
			if (pipe == null || pipe.InvocationStateInfo.State != PSInvocationState.Running)
				return;

			// stop; it still can be bad but chances are low after the above checks
			pipe.BeginStop(AsyncStop, pipe);
		}
		void AsyncStop(IAsyncResult ar) //_110128_075844
		{
			(ar.AsyncState as PowerShell).EndStop(ar);
		}
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
			// preload
			OpenRunspace(false);

			// subscribe
			// editor events: OnEditorOpened1 should be called always and first
			// do Invoking() (at least for TabExpansion) and the startup code
			Far.Api.AnyEditor.Opened += EditorKit.OnEditorOpened1;
			Far.Api.AnyEditor.Opened += EditorKit.OnEditorOpened2;

			//! subscribe only _110301_164313
			Console.CancelKeyPress += CancelKeyPress; //_110128_075844
		}

		/// <summary>
		/// Called on disconnection internally.
		/// If there are background jobs it shows a dialog about them.
		/// </summary>
		[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
		internal void Disconnect()
		{
			//! do not unsubscribe _110301_164313
			//Console.CancelKeyPress -= CancelKeyPress; //_110128_075844

			// unsubscribe
			Far.Api.AnyEditor.Opened -= EditorKit.OnEditorOpened2;
			Far.Api.AnyEditor.Opened -= EditorKit.OnEditorOpened1;

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

			// initial state
			var state = InitialSessionState.CreateDefault();

			// can run scripts regadless of execution policy
			state.AuthorizationManager = new AuthorizationManager(Res.Me);

			// cmdlets
			Commands.BaseCmdlet.AddCmdlets(state);

			// apartment
			state.ApartmentState = Environment.GetEnvironmentVariable("PSF.ApartmentState") == "MTA" ? ApartmentState.MTA : ApartmentState.STA;

			// open/start runspace
			Runspace = RunspaceFactory.CreateRunspace(FarHost, state);
			Runspace.StateChanged += OnRunspaceStateEvent;
			if (sync)
				Runspace.Open();
			else
				Runspace.OpenAsync();
		}

		// Tells that loading is over
		bool _isRunspaceOpenedOrBroken;

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

				//! Set the broken flag, waiting threads may continue.
				//! The last code, Invoking() may be waiting for this.
				_isRunspaceOpenedOrBroken = true;
				return;
			}

			// Add the module path.
			// STOP: [_100127_182335 test]
			// *) Add before the profile, so that it can load modules.
			// *) Add after the core loading so that standard paths are added.
			// *) Check for already added, e.g. when starting from another Far.
			var modulePathAdd = string.Concat(AppHome, "\\Modules;");
			var modulePathNow = Environment.GetEnvironmentVariable(Word.PSModulePath);
			if (!modulePathNow.Contains(modulePathAdd))
				Environment.SetEnvironmentVariable(Word.PSModulePath, modulePathAdd + modulePathNow);

			//! If it is async then PS catches all and adds errors to $Error.
			//! Thus, we don't catch anything, because this is normally async.
			string message = null;
			try
			{
				//_090315_091325
				// Get engine once to avoid this: "A pipeline is already executing. Concurrent SessionStateProxy method call is not allowed."
				// Looks like a hack, but it works fine. Problem case: run Test-CallStack-.ps1, Esc -> the error above.
				// SVN tag 4.2.26
				_engine_ = Runspace.SessionStateProxy.PSVariable.GetValue(Word.ExecutionContext) as EngineIntrinsics;

				// get version
				try
				{
					_PSVersion = (Version)((IDictionary)Runspace.SessionStateProxy.PSVariable.GetValue("PSVersionTable"))["PSVersion"];
				}
				catch
				{
					throw new InvalidOperationException("Cannot get PowerShell version.");
				}

				// new variables
				PSVariable var1 = new PSVariable("Psf", this, ScopedItemOptions.AllScope | ScopedItemOptions.Constant);
				var1.Description = "Exposes PowerShellFar.";
				Engine.SessionState.PSVariable.Set(var1);
				PSVariable var2 = new PSVariable("Far", Far.Api, ScopedItemOptions.AllScope | ScopedItemOptions.Constant);
				var2.Description = "Exposes FarNet.";
				Engine.SessionState.PSVariable.Set(var2);

				// invoke profiles
				using (var ps = NewPowerShell())
				{
					// internal profile (NB: there is trap in there)
					ps.AddScript(Resource.PowerShellFar, false).Invoke();

					// user profile, separately for better diagnostics
					var profile = Path.Combine(A.Psf.Manager.GetFolderPath(SpecialFolder.RoamingData, true), "Profile.ps1");
					if (File.Exists(profile))
					{
						ps.Commands.Clear();
						try
						{
							ps.AddCommand(profile, false).Invoke();
						}
						catch (RuntimeException ex)
						{
							message = string.Format(null, @"
Error in the profile:
{0}

Error message:
{1}

See $Error for details.
", profile, ex.Message);
						}
					}
				}
			}
			finally
			{
				// GUI message
				if (message != null)
					Far.Api.Message(message, Res.Me, MessageOptions.Warning | MessageOptions.Gui | MessageOptions.Ok);

				//! The last code, Invoking() may be waiting for this.
				_isRunspaceOpenedOrBroken = true;
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
				throw new PowerShellFarException(@"
The engine was not successfully initialized and will be unloaded.
For known issues see 'Problems and solutions' in the FarNet manual.
", _errorFatal);
			}

			// complete opening
			if (Runspace.DefaultRunspace == null)
			{
				//! wait while loading
				while (!_isRunspaceOpenedOrBroken)
					System.Threading.Thread.Sleep(100);

				//! set default runspace for handlers
				//! it has to be done in main thread
				Runspace.DefaultRunspace = Runspace;

				// add the debug handler
				Runspace.Debugger.DebuggerStop += OnDebuggerStop;
				Runspace.Debugger.BreakpointUpdated += OnBreakpointUpdated;
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
			IPanel panel = Far.Api.Panel;
			if (panel == null)
				return null;

			// at first get both paths: for the current system directory and provider location
			string directory = Far.Api.CurrentDirectory;
			string location = null;
			if (panel.IsPlugin)
			{
				Panel plugin = panel as Panel;
				if (plugin != null)
				{
					var itemPanel = plugin as ItemPanel;
					if (itemPanel != null)
					{
						location = itemPanel.Explorer.Location;
					}
					else
					{
						FolderTree folderTree = plugin as FolderTree;
						if (folderTree != null)
						{
							location = panel.CurrentDirectory;
							if (location == "*") //_130117_234326
								location = directory;
						}
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
					string message = string.Format(null, @"
Cannot set the current location to
{0}

Continue with this current location?
{1}
", location, currentLocation);

					switch (Far.Api.Message(message, Res.Me, MessageOptions.GuiOnMacro | MessageOptions.AbortRetryIgnore | MessageOptions.Warning | MessageOptions.LeftAligned))
					{
						case 1:
							break;
						case 2:
							_failedInvokingLocationNew = location;
							_failedInvokingLocationOld = currentLocation;
							break;
						default:
							if (Far.Api.MacroState != MacroState.None)
								Far.Api.UI.Break();
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
					string message = string.Format(null, @"
Cannot set the current directory to
{0}

Continue with this current directory?
{1}
", directory, currentDirectory);

					switch (Far.Api.Message(message, Res.Me, MessageOptions.GuiOnMacro | MessageOptions.AbortRetryIgnore | MessageOptions.Warning | MessageOptions.LeftAligned))
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
							if (Far.Api.MacroState != MacroState.None)
								Far.Api.UI.Break();
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

		#endregion
		/// <summary>
		/// Gets the configuration settings and the session settings.
		/// </summary>
		/// <remarks>
		/// Permanent settings are changed in the module settings panel.
		/// Session preferences are usually set in the profile.
		/// <para>
		/// See also the manual [Settings].
		/// </para>
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public Settings Settings
		{
			get { return Settings.Default; }
		}
		/// <summary>
		/// Gets or sets the active text of active editor or editor line.
		/// </summary>
		/// <remarks>
		/// Gets or sets selected text if selection exists in the current editor or an editor line,
		/// else a line text if any kind of editor line is active.
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public string ActiveText
		{
			get { return EditorKit.ActiveText; }
			set { EditorKit.ActiveText = value; }
		}
		/// <summary>
		/// Adds an action to all menus (Dialog, Editor, Panels, Viewer).
		/// </summary>
		/// <param name="text">Menu item text. Use ampersand to set a hotkey.</param>
		/// <param name="click">Action script block. Keep it simple, normally it should just call other commands.</param>
		/// <remarks>
		/// Actions can be added any time, but the best practice is to add them from the startup code.
		/// <para>
		/// Use $null action to add a separator to menus.
		/// </para>
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public void Action(string text, EventHandler<MenuEventArgs> click)
		{
			UI.ActorMenu.AddUserTool(text, click, ModuleToolOptions.None);
		}
		/// <summary>
		/// Adds an action to the specified menus (combination of Dialog, Editor, Panels, Viewer).
		/// </summary>
		/// <param name="text">Menu item text. Use ampersand to set a hotkey.</param>
		/// <param name="click">Action script block. Keep it simple, normally it should just call other commands.</param>
		/// <param name="area">Where this action should be available in a menu: combination of Dialog, Editor, Panels, Viewer.</param>
		/// <remarks>
		/// Actions can be added any time, but the best practice is to add them from the startup code.
		/// <para>
		/// Use $null action to add a separator to menus.
		/// </para>
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public void Action(string text, EventHandler<MenuEventArgs> click, ModuleToolOptions area)
		{
			UI.ActorMenu.AddUserTool(text, click, area);
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public IEditor Editor()
		{
			if (Far.Api.Window.Kind != WindowKind.Editor)
				throw new InvalidOperationException(Res.NeedsEditor);

			return Far.Api.Editor;
		}
		/// <summary>
		/// Returns PowerShellFar home path. Designed for internal use.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public string AppHome
		{
			get { return Path.GetDirectoryName((Assembly.GetExecutingAssembly()).Location); }
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "PowerShellFar")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public string InputCode()
		{
			var ui = new UI.InputDialog() { Caption = Res.Me, History = Res.History, UseLastHistory = true, Prompt = new string[] { Res.InvokeCommands } };
			return ui.Show() ? ui.Text : null;
		}
		/// <summary>
		/// Prompts to input code and invokes it.
		/// Called on "Invoke commands".
		/// </summary>
		/// <remarks>
		/// If it is called during a macro then commands are not added to the history.
		/// Note: use of <c>Plugin.Call()</c> (see the FarNet manual) is often better for macros.
		/// <para>
		/// In order to input and get the code without invoking use the <see cref="InputCode"/> method.
		/// </para>
		/// </remarks>
		public void InvokeInputCode()
		{
			string code = InputCode();
			if (code != null)
				Act(code, null, Far.Api.MacroState == MacroState.None);
		}
		/// <summary>
		/// Starts console mode (async).
		/// Called on "Command console".
		/// </summary>
		public void StartConsole()
		{
			UI.InputConsole.Start();
		}
		/// <summary>
		/// Invokes the selected text or the current line text in the editor or the command line.
		/// Called on "Invoke selected".
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
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
		/// don't need them in PowerShellFar. The history is stored in a file, so that commands can be used in other sessions.
		/// <para>
		/// Some standard history commands are partially implemented as internal functions.
		/// <c>Get-History</c> returns command strings, <c>Invoke-History</c> calls <see cref="ShowHistory"/>.
		/// </para>
		/// </remarks>
		/// <param name="count">Number of last commands to be returned. 0: all commands.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public IList<string> GetHistory(int count)
		{
			var lines = History.ReadLines();
			if (count <= 0 || count >= lines.Length)
				return lines;

			var list = new List<string>(lines);
			list.RemoveRange(0, list.Count - count);
			return list;
		}
		/// <summary>
		/// Shows a new modal editor console.
		/// </summary>
		/// <remarks>
		/// This method opens a modal editor console, it can be called in the middle of something to perform actions manually
		/// and then to continue interrupted execution on exit. Basically it is so called PowerShell nested prompt.
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
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
		/// <param name="mode">The editor open mode.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public void ShowPanel()
		{
			string currentDirectory = A.Psf.SyncPaths();
			try
			{
				string drive = UI.SelectMenu.SelectDrive(null, true);
				if (drive == null)
					return;

				AnyPanel ap;
				if (drive == "Folder &tree")
					ap = new FolderTree();
				else if (drive == "&Any objects")
					ap = new ObjectPanel();
				else
					ap = new ItemPanel(drive);
				ap.Open();
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public void ShowJobs()
		{
			Job.ShowJobs();
		}
		/// <summary>
		/// Shows PowerShellFar command history and invokes or insert the selected command or text.
		/// Called on "Command history".
		/// </summary>
		/// <seealso cref="GetHistory"/>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public void ShowHistory()
		{
			History.ShowHistory();
		}
		/// <summary>
		/// Shows PowerShell debugger tools menu.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public void ShowDebugger()
		{
			var ui = new UI.DebuggerMenu();
			ui.Show();
		}
		/// <summary>
		/// Shows PowerShell errors.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
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
		/// information and shows it in the viewer. In code editors (*.ps1, *.psm1, *.psd1,
		/// *.psfconsole, input code boxes) this action is associated with [ShiftF1].
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public void ShowHelp()
		{
			Help.ShowHelpForContext();
		}
		/// <summary>
		/// Expands PowerShell code in an edit line.
		/// </summary>
		/// <param name="editLine">Editor line, command line or dialog edit box line; if null then <see cref="IFar.Line"/> is used.</param>
		/// <remarks>
		/// It implements so called TabExpansion using a menu and inserting a selected text into a current line being edited.
		/// The edit line can belong to the internal editor, the command line or a dialogs.
		/// <para>
		/// When it is called the first time it loads the script TabExpansion.ps1 from the module directory
		/// which installs the global function TabExpansion. After that this function is always called and
		/// returned selected text is inserted into the edit line.
		/// </para>
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public void ExpandCode(ILine editLine)
		{
			EditorKit.ExpandCode(editLine, null);
		}
		// PS host
		FarHost FarHost;
		// PS UI
		internal FarUI FarUI;
		// PS runspace
		internal Runspace Runspace { get; private set; }
		// Main pipeline
		PowerShell Pipeline;
		// PS engine
		EngineIntrinsics _engine_;
		internal EngineIntrinsics Engine { get { return _engine_; } }
		// PS version
		Version _PSVersion;
		internal Version PSVersion { get { return _PSVersion; } }
		/// <summary>
		/// Gets a new pipeline or nested one.
		/// </summary>
		/// <returns>Pipeline; it has to be disposed.</returns>
		internal PowerShell NewPowerShell()
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
		internal bool IsRunning
		{
			get { return Pipeline != null && Pipeline.InvocationStateInfo.State == PSInvocationState.Running; }
		}
		// Current command being invoked (e.g. used as Out-FarPanel title)
		internal string _myCommand;
		// Last invoked command (e.g. to reduce dupes in the history)
		internal string _myLastCommand;
		/// <summary>
		/// Invokes PowerShell command with pipeline.
		/// </summary>
		/// <param name="code">PowerShell code.</param>
		/// <param name="writer">Output writer or null.</param>
		/// <param name="addHistory">Add command to history.</param>
		internal bool Act(string code, OutputWriter writer, bool addHistory)
		{
			// result
			bool ok = true;

			// drop history cache
			History.Cache = null;

			// push writer
			if (writer != null)
			{
				// predefined output
				FarUI.PushWriter(writer);
			}
			else
			{
				// use own output to be shown later
				FarUI.PushWriter(new TranscriptOutputWriter());
			}

			// invoke
			try
			{
				// win7 Indeterminate
				Far.Api.UI.SetProgressState(TaskbarProgressBarState.Indeterminate);

				// add history
				if (addHistory)
				{
					code = code.Trim();
					if (code.Length > 0 && code[code.Length - 1] != '#' && code != _myLastCommand)
						History.AddLine(code);
				}

				// invoke command
				using (var ps = NewPowerShell())
				{
					_myCommand = code;
					//TODO We may need a mode with Out-Host even for console, e.g. to transcribe apps output
					var output = FarUI.Writer is ConsoleOutputWriter ? A.OutDefaultCommand : A.OutHostCommand;
					ps.Commands.AddScript(code).AddCommand(output);
					ps.Invoke();
				}
			}
			catch (RuntimeException reason)
			{
				ok = false;
				ConsoleColor color1 = ConsoleColor.Black;
				try
				{
					// push console color
					if (writer is ConsoleOutputWriter)
					{
						color1 = Far.Api.UI.ForegroundColor;
						Far.Api.UI.ForegroundColor = Settings.ErrorForegroundColor;
					}

					// write the reason
					using (var ps = NewPowerShell())
						A.OutReason(ps, reason);
				}
				finally
				{
					// pop console color
					if (color1 != ConsoleColor.Black)
					{
						Far.Api.UI.ForegroundColor = color1;
					}
				}
			}
			finally
			{
				// win7 NoProgress
				Far.Api.UI.SetProgressState(TaskbarProgressBarState.NoProgress);

				_myLastCommand = _myCommand;
				_myCommand = null;

				// pop writer
				OutputWriter usedWriter = FarUI.PopWriter();
				if (writer == null)
				{
					// it is the writer created locally;
					// view its file, if any
					var myWriter = (TranscriptOutputWriter)usedWriter;
					myWriter.Close();
					if (myWriter.FileName != null)
					{
						var viewer = Far.Api.CreateViewer();
						viewer.Title = code;
						viewer.FileName = myWriter.FileName;
						viewer.DeleteSource = DeleteSource.File;
						viewer.Switching = Switching.Enabled;
						viewer.DisableHistory = true;
						viewer.CodePage = 1200;
						viewer.Open();
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public IDictionary Providers
		{
			get { return _Providers; }
			set { _Providers = value == null ? new Hashtable() : value; }
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public void InvokeScriptFromEditor()
		{
			EditorKit.InvokeScriptBeingEdited(null);
		}
		HashSet<LineBreakpoint> _breakpoints_;
		internal HashSet<LineBreakpoint> Breakpoints { get { return _breakpoints_ ?? (_breakpoints_ = new HashSet<LineBreakpoint>()); } }
		void OnBreakpointUpdated(object sender, BreakpointUpdatedEventArgs e)
		{
			if (!string.IsNullOrEmpty(e.Breakpoint.Script))
			{
				var bp = e.Breakpoint as LineBreakpoint;
				if (bp != null)
				{
					if (e.UpdateType == BreakpointUpdateType.Removed)
						Breakpoints.Remove(bp);
					else
						Breakpoints.Add(bp);
				}
			}
		}
		// Show debug dialog
		void OnDebuggerStop(object sender, DebuggerStopEventArgs e)
		{
			var ui = new UI.DebuggerDialog(e);

			// viewer writer?
			var writer = FarUI.Writer as TranscriptOutputWriter;

			// no? if console writer and transcript then use transcript
			if (writer == null && FarUI.Writer is ConsoleOutputWriter && Transcript != null)
				writer = Transcript;

			// add View handler
			if (writer != null)
			{
				ui.OnView = delegate
				{
					// ensure file
					if (writer.FileName == null)
						writer.Write(string.Empty);

					// view file
					Zoo.StartExternalViewer(writer.FileName);
				};
			}

			e.ResumeAction = ui.Show();
		}
		/// <summary>
		/// Gets currently running stepper instance if any or null.
		/// </summary>
		/// <remarks>
		/// It is designed mosttly for use from a step script block being processed.
		/// It's fine to use this in order to check stepping mode by not null result.
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public Stepper Stepper { get { return Stepper.RunningInstance; } }
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public IModuleManager Manager
		{
			get { return Entry.Instance.Manager; }
		}
		/// <summary>
		/// Transcript writer, may be null.
		/// </summary>
		internal TranscriptOutputWriter Transcript { get; set; }
	}
}
