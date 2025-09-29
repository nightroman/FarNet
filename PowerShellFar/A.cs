using FarNet;
using System.Collections;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.Text;

namespace PowerShellFar;

internal static class A
{
	/// <summary>
	/// PowerShellFar actor.
	/// </summary>
	public static Actor Psf { get; private set; } = null!;

	public static bool IsMainSession => Runspace.DefaultRunspace.Id == 1;
	public static bool IsAsyncSession => Runspace.DefaultRunspace.Id > 1;

	public static void Connect()
	{
		Psf = new Actor();
		Psf.Connect();
	}

	public static void Disconnect()
	{
		Psf.Disconnect();
	}

	/// <summary>
	/// Executes the specified job synchronously if the session is main, otherwise posts and awaits it.
	/// </summary>
	public static void AwaitJob(Action job)
	{
		if (IsMainSession)
			job();
		else
			Tasks.Await(Tasks.Job(job));
	}

	// Shows an error.
	public static void Msg(Exception error)
	{
		Far.Api.Message(error.Message, "PowerShellFar error", MessageOptions.LeftAligned);
	}

	// Shows a message.
	public static void Message(string message)
	{
		Far.Api.Message(message, Res.Me, MessageOptions.LeftAligned);
	}

	// Creates standard Far viewer ready for opening (F3)
	public static IViewer CreateViewer(string filePath)
	{
		IViewer view = Far.Api.CreateViewer();
		view.FileName = filePath;
		return view;
	}

	// Sets an item property value as it is.
	public static void SetPropertyValue(string itemPath, string propertyName, object value)
	{
		//! setting PSPropertyInfo.Value is not working, so do use Set-ItemProperty
		InvokeCode(
			"Set-ItemProperty -LiteralPath $args[0] -Name $args[1] -Value $args[2] -ErrorAction Stop",
			itemPath, propertyName, value);
	}

	// Writes invocation errors.
	public static void WriteErrors(TextWriter writer, IEnumerable errors)
	{
		if (errors is null)
			return;

		foreach (object error in errors)
		{
			writer.WriteLine("ERROR:");
			writer.WriteLine(error.ToString());

			var asErrorRecord = Cast<ErrorRecord>.From(error);
			if (asErrorRecord != null && asErrorRecord.InvocationInfo != null)
				writer.WriteLine(asErrorRecord.InvocationInfo.PositionMessage.AsSpan().Trim());
		}
	}

	// Writes invocation exception.
	public static void WriteException(TextWriter writer, Exception ex)
	{
		if (ex is null)
			return;

		writer.WriteLine("ERROR:");
		writer.WriteLine(ex.GetType().Name + ":");
		writer.WriteLine(ex.Message);

		if (ex is RuntimeException asRuntimeException && asRuntimeException.ErrorRecord != null && asRuntimeException.ErrorRecord.InvocationInfo != null)
			writer.WriteLine(asRuntimeException.ErrorRecord.InvocationInfo.PositionMessage.AsSpan().Trim());
	}

	// Sets an item content, shows errors.
	public static bool SetContentUI(string itemPath, string text)
	{
		//! PSF only, can do in console:
		//! as a script: if param name -Value is used, can't set e.g. Alias:\%
		//! as a block: same problem
		//! so, use a command
		using var ps = Psf.NewPowerShell();
		ps.AddCommand("Set-Content")
			.AddParameter("LiteralPath", itemPath)
			.AddParameter("Value", text)
			.AddParameter(Prm.Force)
			.AddParameter(Prm.ErrorAction, ActionPreference.Continue);

		ps.Invoke();

		if (ShowError(ps))
			return false;
		return true;
	}

	// Outputs to the default formatter.
	public static void Out(PowerShell ps, IEnumerable input)
	{
		ps.Commands.AddCommand(OutHostCommand);
		ps.Invoke(input);
	}

	// Outputs an exception or its error record.
	public static void OutReason(PowerShell ps, Exception ex)
	{
		object error;
		if (ex is RuntimeException asRuntimeException)
			error = asRuntimeException.ErrorRecord;
		else
			error = ex;

		Out(ps, new[] { error });
	}

	// Shows errors, if any, in a message box and returns true, else just returns false.
	public static bool ShowError(PowerShell ps)
	{
		if (ps.Streams.Error.Count == 0)
			return false;

		StringBuilder sb = new();
		foreach (object o in ps.Streams.Error)
			sb.AppendLine(o.ToString());

		Far.Api.Message(sb.ToString(), "PowerShellFar error(s)", MessageOptions.LeftAligned);
		return true;
	}

	// Command for formatted output friendly for apps with interaction and colors.
	// "Out-Host" is not suitable for apps with interaction, e.g. more.com, git.exe.
	public static Command OutDefaultCommand { get; } = new("Out-Default")
	{
		MergeUnclaimedPreviousCommandResults = PipelineResultTypes.Output | PipelineResultTypes.Error
	};

	// Command for formatted output of everything.
	// "Out-Default" is not suitable for external apps, output goes to console.
	public static Command OutHostCommand { get; } = new("Out-Host")
	{
		MergeUnclaimedPreviousCommandResults = PipelineResultTypes.Output | PipelineResultTypes.Error
	};

	// Finds heuristically a property to be used to display the object.
	public static PSPropertyInfo? FindDisplayProperty(PSObject value)
	{
		//! Microsoft.PowerShell.Commands.Internal.Format.PSObjectHelper.GetDisplayNameExpression()

		PSPropertyInfo pi;
		pi = value.Properties[Word.Name];
		if (pi != null)
			return pi;
		pi = value.Properties[Word.Id];
		if (pi != null)
			return pi;
		pi = value.Properties[Word.Key];
		if (pi != null)
			return pi;

		ReadOnlyPSMemberInfoCollection<PSPropertyInfo> ppi;
		ppi = value.Properties.Match("*" + Word.Key);
		if (ppi.Count > 0)
			return ppi[0];
		ppi = value.Properties.Match("*" + Word.Name);
		if (ppi.Count > 0)
			return ppi[0];
		ppi = value.Properties.Match("*" + Word.Id);
		if (ppi.Count > 0)
			return ppi[0];

		return null;
	}

	// Gets the type common for all values or null.
	// Why check for MarshalByRefObject:
	// System.IO.DirectoryInfo, System.Diagnostics.Process in ps: (Get-Item .), (Get-Process -PID $PID) | op
	public static Type? FindCommonType(Collection<PSObject> values)
	{
		Type? result = null;
		foreach (PSObject value in values)
		{
			if (value is null)
				continue;

			var sample = value.BaseObject.GetType();
			if (result is null)
			{
				result = sample;
				continue;
			}

			while (!result.IsAssignableFrom(sample))
			{
				result = result.BaseType;
				if (result is null || result == typeof(object) || result == typeof(MarshalByRefObject))
					return null;
			}
		}
		return result;
	}

	//! Get-FormatData is very expensive (~50% on search), use cache.
	static Dictionary<string, TableControl?>? _CacheTableControl;

	// Finds an available table control.
	public static TableControl? FindTableControl(string typeName)
	{
		_CacheTableControl ??= [];

		ref TableControl? result = ref CollectionsMarshal.GetValueRefOrAddDefault(_CacheTableControl, typeName, out bool found);
		if (found)
			return result;

		// extended type definitions:
		foreach (var pso in InvokeCode("Get-FormatData -TypeName $args[0] -PowerShellVersion $PSVersionTable.PSVersion", typeName))
		{
			var typeDef = (ExtendedTypeDefinition)pso.BaseObject;
			foreach (var viewDef in typeDef.FormatViewDefinition)
			{
				// if it's table, cache and return it
				if (viewDef.Control is TableControl table)
				{
					result = table;
					return table;
				}
			}
		}

		// nothing, cached anyway as null
		return null;
	}

	// Robust Get-ChildItem.
	public static Collection<PSObject> GetChildItems(string literalPath)
	{
		//! If InvokeProvider.ChildItem.Get() fails (e.g. hklm:) then we get nothing at all.
		//! Get-ChildItem gets some items even on errors, that is much better than nothing.
		//! NB: exceptions on getting data: FarNet returns false and Far closes the panel.

		try
		{
			return Psf.Engine.InvokeProvider.ChildItem.Get([literalPath], false, true, true);
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

	// Gets the $FormatEnumerationLimit if it is sane or 4.
	public static int FormatEnumerationLimit
	{
		get
		{
			object value = Psf.Engine.SessionState.PSVariable.GetValue("FormatEnumerationLimit") ?? 4;
			if (value.GetType() != typeof(int))
				return 4;
			else
				return (int)value;
		}
	}

	public static void InvokePipelineForEach(IList<PSObject> input)
	{
		if (input.Count == 0)
			return;

		//_211231_6x keep and set $_ as a sample for TabExpansion
		var dash = Psf.Engine.SessionState.PSVariable.GetValue("_");
		Psf.Engine.SessionState.PSVariable.Set("_", input[0]);

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
			Psf.Run(args);
		}
		finally
		{
			// restore $_
			Psf.Engine.SessionState.PSVariable.Set("_", dash);
		}
	}

	// Invokes the script text and returns the result collection.
	internal static Collection<PSObject> InvokeCode(string code, params object?[] args)
	{
		return ScriptBlock.Create(code).Invoke(args);
	}

	internal static void SetPropertyFromTextUI(object target, PSPropertyInfo pi, string text)
	{
		bool isPSObject;
		object value;
		string type;
		if (pi.Value is PSObject ps)
		{
			isPSObject = true;
			type = ps.BaseObject.GetType().FullName!;
		}
		else
		{
			isPSObject = false;
			type = pi.TypeNameOfValue;
		}

		if (type == "System.Collections.ArrayList" || type.EndsWith(']'))
		{
			ArrayList lines = [.. FarNet.Works.Kit.SplitLines(text)];
			value = lines;
		}
		else
		{
			value = text;
		}

		if (isPSObject)
			value = PSObject.AsPSObject(value);

		try
		{
			pi.Value = value;
			MemberPanel.WhenMemberChanged(target);
		}
		catch (RuntimeException ex)
		{
			Far.Api.Message(ex.Message, "Setting property");
		}
	}

	// Collects names of files.
	internal static List<string> FileNameList(IList<FarFile> files)
	{
		var r = new List<string>
		{
			Capacity = files.Count
		};
		foreach (FarFile f in files)
			r.Add(f.Name);
		return r;
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

	internal static string SafeToString(object? value)
	{
		if (value is null)
			return string.Empty;

		try
		{
			return value.ToString()!;
		}
		catch (Exception ex)
		{
			Log.TraceException(ex);
			return $"<ERROR: {ex.Message}>";
		}
	}
}
