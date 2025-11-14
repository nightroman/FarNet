using FarNet;
using System.Collections;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace PowerShellFar;

internal static class A
{
	private static FarUI _FarUI = null!;
	private static FarHost _FarHost = null!;
	private static PowerShell? _Pipeline;

	internal static Actor Psf { get; private set; } = null!;
	internal static Runspace Runspace { get; private set; } = null!;
	internal static EngineIntrinsics Engine { get; private set; } = null!;

	internal static bool IsMainSession => Runspace.DefaultRunspace.Id == 1;
	internal static bool IsAsyncSession => Runspace.DefaultRunspace.Id > 1;

	#region Life
	private static Task? _OpenRunspaceTask;

	internal static void Connect()
	{
		Psf = new Actor();

		// preload
		_OpenRunspaceTask = Task.Run(OpenRunspace);

		//! subscribe only, do not unsubscribe _110301_164313
		Console.CancelKeyPress += CancelKeyPress;
	}

	internal static void Disconnect()
	{
		// unsubscribe
		Far.Api.AnyEditor.Opened -= EditorKit.OnEditorOpened;
		Far.Api.AnyEditor.Opened -= EditorKit.OnEditorFirstOpening;

		// release menu
		UI.ActorMenu.Close();

		// kill host
		if (_FarHost != null)
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
				_FarUI = null!;
				_FarHost = null!;
				_Pipeline = null;
				Psf = null!;
				Runspace = null!;
				Engine = null!;
			}
		}
	}

	private static void OpenRunspace()
	{
		// UI and host
		_FarUI = new FarUI();
		_FarHost = new FarHost(_FarUI);

		// open runspace
		Runspace = RunspaceFactory.CreateRunspace(_FarHost, FarInitialSessionState.Instance);
		Runspace.ThreadOptions = PSThreadOptions.UseCurrentThread;
		Runspace.Open();

		//2025-09-29-0630 Eventually set for handlers. Do this in the main thread!
		Far.Api.PostJob(() => Runspace.DefaultRunspace = Runspace);

		// add the debug handler
		Runspace.Debugger.BreakpointUpdated += OnBreakpointUpdated;

		// Add the module path.
		// STOP: [_100127_182335 test]
		// *) Add before the profile, so that it can load modules.
		// *) Add after the opening so that standard paths are added.
		// *) Check for already added, e.g. when starting from another Far.
		var modulePathAdd = $"{Entry.RoamingData}\\Modules;";
		var modulePathNow = Environment.GetEnvironmentVariable(Word.PSModulePath) ?? string.Empty;
		if (!modulePathNow.Contains(modulePathAdd))
			Environment.SetEnvironmentVariable(Word.PSModulePath, modulePathAdd + modulePathNow);

		// Get engine once to avoid this: "A pipeline is already executing. Concurrent SessionStateProxy method call is not allowed."
		// Looks like a hack, but it works fine. Problem case: run Test-CallStack.ps1, Esc -> the error above.
		Engine = (EngineIntrinsics)Runspace.SessionStateProxy.PSVariable.GetValue(Word.ExecutionContext);
		Engine.SessionState.PSVariable.Set(new VariableArea());
		Engine.SessionState.PSVariable.Set(new VariablePath());

		// invoke profiles
		using FarHost.IgnoreApplications ignoreApplications = new();
		using var ps = NewPowerShell();

		// internal profile
		{
			using var stream = typeof(Actor).Assembly.GetManifestResourceStream("PowerShellFar.PowerShellFar.ps1");
			using var reader = new StreamReader(stream!, Encoding.UTF8);
			ps.AddScript(reader.ReadToEnd(), false).Invoke();
		}

		// user profile, run separately for better errors
		var profile = Entry.RoamingData + "\\Profile.ps1";
		if (File.Exists(profile))
		{
			ps.Commands.Clear();
			try
			{
				ps.AddCommand(profile, false).Invoke();
			}
			catch (RuntimeException ex)
			{
				throw new ModuleException($"Profile.ps1 error: {ex.Message}", ex);
			}
		}
	}

	// Completes runspace opening.
	// Called by FarNet on commands and PSF on some actions.
	internal static void Invoking()
	{
		// done?
		if (_OpenRunspaceTask is null)
			return;

		// done
		var task = _OpenRunspaceTask;
		_OpenRunspaceTask = null;

		// complete
		task.Await();

		//2025-09-29-0630 Set for early interop like `Start-Far "fs:exec ...". Do this in the main thread!
		Runspace.DefaultRunspace = Runspace;
	}

	// Sets the current location to predictable and expected.
	// Used in operations which potentially invoke user code.
	internal static void SyncPaths()
	{
		// skip running
		if (IsRunning)
			return;

		var currentDirectory = Far.Api.CurrentDirectory;

		//! avoid rather expensive SetLocation
		var pathIntrinsics = Engine.SessionState.Path;
		if (pathIntrinsics.CurrentLocation.Path == currentDirectory)
			return;

		// Set the current location. Note, PS Core works fine with long paths.
		// So do not catch, we should know why it fails and how to handle this.
		// Parameter is wildcard. Test: enter into a folder "[]" and invoke a command.
		pathIntrinsics.SetLocation(WildcardPattern.Escape(currentDirectory));
	}

	// Sets Far current directory from PowerShell current location.
	internal static void SyncPathsBack()
	{
		if (IsRunning || !Far.Api.HasPanels)
			return;

		var path = Engine.SessionState.Path.CurrentFileSystemLocation.Path;
		if (!path.Equals(Far.Api.CurrentDirectory, StringComparison.OrdinalIgnoreCase))
		{
			if (Far.Api.Panel is { } panel)
				panel.CurrentDirectory = path;
		}
	}

	// Stops the running pipeline.
	private static void CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
	{
		// ControlBreak?
		if (e.SpecialKey != ConsoleSpecialKey.ControlBreak)
			return;

		//! use copy
		var pipe = _Pipeline;
		if (pipe is null || pipe.InvocationStateInfo.State != PSInvocationState.Running)
			return;

		// stop; may be bad but unlikely after the above checks
		pipe.BeginStop(ar => ((PowerShell)ar.AsyncState!).EndStop(ar), pipe);
	}
	#endregion

	#region Breakpoints
	private static bool _isFirstBreakpoint = true;
	internal static HashSet<LineBreakpoint> Breakpoints { get; } = [];

	private static void OnBreakpointUpdated(object? sender, BreakpointUpdatedEventArgs e)
	{
		//! update first
		if (!string.IsNullOrEmpty(e.Breakpoint.Script))
		{
			if (e.Breakpoint is LineBreakpoint bp)
			{
				if (e.UpdateType == BreakpointUpdateType.Removed)
					Breakpoints.Remove(bp);
				else
					Breakpoints.Add(bp);
			}
		}

		//! then this with possible exceptions
		if (_isFirstBreakpoint && e.Breakpoint.Action is null)
		{
			_isFirstBreakpoint = false;

			if (!DebuggerKit.HasAnyDebugger(Runspace.Debugger))
			{
				DebuggerKit.ValidateAvailable();
				InvokeCode("Add-Debugger.ps1");
			}
		}
	}
	#endregion

	#region Run
	// Is it running?
	internal static bool IsRunning => _Pipeline is { } && _Pipeline.InvocationStateInfo.State == PSInvocationState.Running;

	// Current command (e.g. used as Out-FarPanel title).
	internal static string? MyCommand { get; private set; }

	/// <summary>
	/// Gets a new pipeline or nested one.
	/// </summary>
	/// <returns>Pipeline; it has to be disposed.</returns>
	internal static PowerShell NewPowerShell()
	{
		if (IsRunning)
			return _Pipeline!.CreateNestedPowerShell();

		_Pipeline = PowerShell.Create();
		_Pipeline.Runspace = Runspace;
		return _Pipeline;
	}

	/// <summary>
	/// Runs the PowerShell command pipeline.
	/// Null or empty commands are ignored.
	/// </summary>
	/// <returns>False if the code fails.</returns>
	internal static bool Run(RunArgs args)
	{
		var code = args.Code;
		if (string.IsNullOrEmpty(code))
			return true;

		// push writer
		if (args.Writer is null)
		{
			// use own lazy output
			_FarUI.PushWriter(new TranscriptOutputWriter());
		}
		else
		{
			// specified output
			_FarUI.PushWriter(args.Writer);
		}

		FarHost.IgnoreApplications? ignoreApplications = null;
		try
		{
			// progress
			_FarUI.IsProgressStarted = false;
			Far.Api.UI.SetProgressState(TaskbarProgressBarState.Indeterminate);

			// output and apps
			Command output;
			if (_FarUI.Writer is ConsoleOutputWriter)
			{
				output = OutDefaultCommand;
			}
			else
			{
				output = OutHostCommand;
				ignoreApplications = new();
			}

			// invoke command
			using var ps = NewPowerShell();
			MyCommand = code;
			var command = ps.Commands.AddScript(code, args.UseLocalScope);
			if (args.Arguments is { } arguments)
			{
				for (int i = 0; i < arguments.Length; ++i)
					command.AddArgument(arguments[i]);
			}
			if (args.UseTeeResult)
				command.AddCommand(TeeResultCommand);
			command.AddCommand(output);
			ps.Invoke();
			args.Reason = ps.InvocationStateInfo.Reason;

			return true;
		}
		catch (Exception reason)
		{
			if (FarNet.Works.ExitManager.IsExiting)
				throw;

			args.Reason = reason;
			if (args.NoOutReason)
				return false;

			var color1 = ConsoleColor.Black;
			try
			{
				// push console color
				if (args.Writer is ConsoleOutputWriter)
				{
					color1 = Far.Api.UI.ForegroundColor;
					Far.Api.UI.ShowUserScreen();
					Far.Api.UI.ForegroundColor = Settings.Default.ErrorForegroundColor;
				}

				// write the reason
				using var ps = NewPowerShell();
				OutReason(ps, reason);
			}
			finally
			{
				// pop console color
				if (color1 != ConsoleColor.Black)
					Far.Api.UI.ForegroundColor = color1;
			}

			return false;
		}
		finally
		{
			// restore apps
			ignoreApplications?.Dispose();

			// restore progress
			_FarUI.IsProgressStarted = false;
			Far.Api.UI.SetProgressState(TaskbarProgressBarState.NoProgress);

			MyCommand = null;

			// pop writer
			var usedWriter = _FarUI.PopWriter();
			if (args.Writer is null)
			{
				// it is the writer created here, view its file, if any
				var myWriter = (TranscriptOutputWriter)usedWriter;
				myWriter.Close();
				if (myWriter.FileName != null)
				{
					var viewer = Far.Api.CreateViewer();
					viewer.FileName = myWriter.FileName;
					//! code with \n may come from ReadLine editors
					viewer.Title = code.IndexOf('\n') < 0 ? code.Trim() : "Command output";
					viewer.DeleteSource = DeleteSource.File;
					viewer.Switching = Switching.Enabled;
					viewer.DisableHistory = true;
					viewer.CodePage = 1200;
					viewer.Open();
				}
			}
		}
	}
	#endregion

	/// <summary>
	/// Executes the specified job synchronously if the session is main, otherwise posts and awaits it.
	/// </summary>
	internal static void AwaitJob(Action job)
	{
		if (IsMainSession)
			job();
		else
			Tasks.Await(Tasks.Job(job));
	}

	// Shows an error.
	internal static void MyError(Exception error)
	{
		Far.Api.Message(error.Message, "PowerShellFar error");
	}

	// Shows a message.
	internal static void MyMessage(string message)
	{
		Far.Api.Message(message, Res.Me);
	}

	// Sets an item property value as it is.
	internal static void SetPropertyValue(string itemPath, string propertyName, object value)
	{
		//! setting PSPropertyInfo.Value is not working, so do use Set-ItemProperty
		InvokeCode(
			"Set-ItemProperty -LiteralPath $args[0] -Name $args[1] -Value $args[2] -ErrorAction Stop",
			itemPath, propertyName, value);
	}

	// Outputs to the default formatter.
	private static void Out(PowerShell ps, IEnumerable input)
	{
		ps.Commands.AddCommand(OutHostCommand);
		ps.Invoke(input);
	}

	// Outputs an exception or its error record.
	internal static void OutReason(PowerShell ps, Exception ex)
	{
		object error;
		if (ex is RuntimeException asRuntimeException)
			error = asRuntimeException.ErrorRecord;
		else
			error = ex;

		Out(ps, new[] { error });
	}

	// Shows errors, if any, in a message box and returns true, else just returns false.
	internal static bool ShowError(PowerShell ps)
	{
		if (ps.Streams.Error.Count == 0)
			return false;

		StringBuilder sb = new();
		foreach (object o in ps.Streams.Error)
			sb.AppendLine(o.ToString());

		Far.Api.Message(sb.ToString(), "PowerShellFar error(s)");
		return true;
	}

	// Command for formatted output friendly for apps with interaction and colors.
	// "Out-Host" is not suitable for apps with interaction, e.g. more.com, git.exe.
	private static Command OutDefaultCommand { get; } = new("Out-Default")
	{
		MergeUnclaimedPreviousCommandResults = PipelineResultTypes.Output | PipelineResultTypes.Error
	};

	// Command for formatted output of everything.
	// "Out-Default" is not suitable for external apps, output goes to console.
	internal static Command OutHostCommand { get; } = new("Out-Host")
	{
		MergeUnclaimedPreviousCommandResults = PipelineResultTypes.Output | PipelineResultTypes.Error
	};

	internal static Command TeeResultCommand { get; } = TeeResultCommandGet();
	private static Command TeeResultCommandGet()
	{
		var r = new Command("Tee-Object", false, true);
		r.Parameters.Add("Variable", "r");
		return r;
	}

	// Robust Get-ChildItem.
	internal static Collection<PSObject> GetChildItems(string literalPath)
	{
		//! If InvokeProvider.ChildItem.Get() fails (e.g. hklm:) then we get nothing at all.
		//! Get-ChildItem gets some items even on errors, that is much better than nothing.
		//! NB: exceptions on getting data: FarNet returns false and Far closes the panel.

		try
		{
			return Engine.InvokeProvider.ChildItem.Get([literalPath], false, true, true);
		}
		catch (Exception ex)
		{
			Log.TraceException(ex);
		}

		try
		{
			return InvokeCode("Get-ChildItem -LiteralPath $args[0] -Force -ErrorAction 0", literalPath);
		}
		catch (Exception ex)
		{
			Log.TraceException(ex);
		}

		return [];
	}

	internal static void InvokePipelineForEach(IList<PSObject> input)
	{
		if (input.Count == 0)
			return;

		//_211231_6x keep and set $_ as a sample for TabExpansion
		var dash = Engine.SessionState.PSVariable.GetValue("_");
		Engine.SessionState.PSVariable.Set("_", input[0]);

		try
		{
			// input code
			var ui = new UI.InputBox2($"For each $_ in {input.Count} selected:", Res.Me);
			ui.Edit.History = Res.HistoryApply;
			ui.Edit.UseLastHistory = true;

			var code = ui.Show();
			if (string.IsNullOrEmpty(code))
				return;

			// invoke with the input
			var args = new RunArgs("$args[0] | .{process{ " + code + " }}") { Arguments = [input] };
			Run(args);
		}
		finally
		{
			// restore $_
			Engine.SessionState.PSVariable.Set("_", dash);
		}
	}

	// Invokes the script text and returns the result collection.
	internal static Collection<PSObject> InvokeCode(string code, params object?[] args)
	{
		return ScriptBlock.Create(code).Invoke(args);
	}

	// Invokes Format-List with output to string.
	internal static string InvokeFormatList(object? data, bool full)
	{
		// suitable for DataRow, noisy data are excluded
		const string codeMain = @"
Format-List -InputObject $args[0] -ErrorAction 0 |
Out-String -Width $args[1]
";
		// suitable for Object, gets maximum information
		const string codeFull = @"
Format-List -InputObject $args[0] -Property * -Force -Expand Both -ErrorAction 0 |
Out-String -Width $args[1]
";
		return InvokeCode(full ? codeFull : codeMain, data, int.MaxValue)[0].ToString();
	}

	internal static void SetBreakpoint(string? script, int line, ScriptBlock? action)
	{
		string code = "Set-PSBreakpoint -Script $args[0] -Line $args[1]";
		if (action is { })
			code += " -Action $args[2]";

		try
		{
			InvokeCode(code, script, line, action);
		}
		catch (CmdletInvocationException ex) when (ex.InnerException is { } ex2)
		{
			throw ex2;
		}
	}

	internal static void RemoveBreakpoint(object breakpoint)
	{
		InvokeCode("Remove-PSBreakpoint -Breakpoint $args[0]", breakpoint);
	}

	internal static void DisableBreakpoint(object breakpoint)
	{
		InvokeCode("Disable-PSBreakpoint -Breakpoint $args[0]", breakpoint);
	}

	internal static object? SafePropertyValue(PSPropertyInfo pi)
	{
		//: 2024-11-18-1917 CIM cmdlets problems, especially bad in PS 7.5
		if (pi.Name == "CommandLine" && pi is PSScriptProperty scriptProperty && scriptProperty.GetterScript.ToString().Contains("Get-CimInstance"))
			return null;

		//! exceptions, e.g. exit code of running process
		try
		{
			return pi.Value;
		}
		catch (Exception ex)
		{
			Log.TraceException(ex);
			return $"<ERROR: {ex.Message}>";
		}
	}

	internal static object GetVariableValue(string name)
	{
		return Engine.SessionState.PSVariable.GetValue(name);
	}
}
