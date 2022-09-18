
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace PowerShellFar;

static class A
{
	/// <summary>
	/// PowerShellFar actor.
	/// </summary>
	public static Actor Psf => _Psf_!;
	static Actor? _Psf_;

	public static void Connect(Actor? psf)
	{
		_Psf_ = psf;
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
				writer.WriteLine(Kit.PositionMessage(asErrorRecord.InvocationInfo.PositionMessage));
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
			writer.WriteLine(Kit.PositionMessage(asRuntimeException.ErrorRecord.InvocationInfo.PositionMessage));
	}

	// Sets an item content, shows errors.
	public static bool SetContentUI(string itemPath, string text)
	{
		//! PSF only, can do in console:
		//! as a script: if param name -Value is used, can't set e.g. Alias:\%
		//! as a block: same problem
		//! so, use a command
		using (var ps = Psf.NewPowerShell())
		{
			ps.AddCommand("Set-Content")
				.AddParameter("LiteralPath", itemPath)
				.AddParameter("Value", text)
				.AddParameter(Prm.Force)
				.AddParameter(Prm.ErrorAction, ActionPreference.Continue);

			ps.Invoke();

			if (ShowError(ps))
				return false;
		}
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
	public static Command OutDefaultCommand => new("Out-Default")
	{
		MergeUnclaimedPreviousCommandResults = PipelineResultTypes.Output | PipelineResultTypes.Error
	};

	// Command for formatted output of everything.
	// "Out-Default" is not suitable for external apps, output goes to console.
	public static Command OutHostCommand => new("Out-Host")
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
		// make/try cache
		if (_CacheTableControl is null)
		{
			_CacheTableControl = new Dictionary<string, TableControl?>();
		}
		else if (_CacheTableControl.TryGetValue(typeName, out TableControl? result))
		{
			return result;
		}

		// extended type definitions:
		foreach (var pso in InvokeCode("Get-FormatData -TypeName $args[0] -PowerShellVersion $PSVersionTable.PSVersion", typeName))
		{
			var typeDef = (ExtendedTypeDefinition)pso.BaseObject;
			foreach (var viewDef in typeDef.FormatViewDefinition)
			{
				// if it's table, cache and return it
				if (viewDef.Control is TableControl table)
				{
					_CacheTableControl.Add(typeName, table);
					return table;
				}
			}
		}

		// nothing, cache anyway and return null
		_CacheTableControl.Add(typeName, null);
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
			return Psf.Engine.InvokeProvider.ChildItem.Get(new string[] { literalPath }, false, true, true);
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

		return new Collection<PSObject>();
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
			var ui = new UI.InputDialog()
			{
				Title = Res.Me,
				History = Res.HistoryApply,
				UseLastHistory = true,
				Prompt = new string[] { "For each $_ in " + input.Count + " selected:" }
			};

			var code = ui.Show();
			if (string.IsNullOrEmpty(code))
				return;

			// invoke with the input
			var args = new RunArgs("$args[0] | .{process{ " + code + " }}") { Arguments = new object[] { input } };
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

		if (type == "System.Collections.ArrayList" || type.EndsWith("]", StringComparison.Ordinal))
		{
			ArrayList lines = new();
			foreach (var line in FarNet.Works.Kit.SplitLines(text))
				lines.Add(line);
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
		if (action != null)
			code += " -Action $args[2]";
		InvokeCode(code, script, line, action);
	}

	internal static void RemoveBreakpoint(object breakpoint)
	{
		InvokeCode("Remove-PSBreakpoint -Breakpoint $args[0]", breakpoint);
	}

	internal static void DisableBreakpoint(object breakpoint)
	{
		InvokeCode("Disable-PSBreakpoint -Breakpoint $args[0]", breakpoint);
	}

	internal static object SafePropertyValue(PSPropertyInfo pi)
	{
		//! exceptions, e.g. exit code of running process
		try
		{
			return pi.Value;
		}
		catch (GetValueException e)
		{
			return string.Format(null, "<ERROR: {0}>", e.Message);
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
		catch (Exception e)
		{
			return string.Format(null, "<ERROR: {0}>", e.Message);
		}
	}
}
