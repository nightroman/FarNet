
using FarNet;
using PowerShellFar.UI;
using System.Collections;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Reflection;

namespace PowerShellFar;

static class DebuggerKit
{
	public static bool HasAnyDebugger(Debugger debugger)
	{
		return typeof(Debugger).GetField("DebuggerStop", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(debugger) is Delegate;
	}

	public static void ValidateAvailable()
	{
		var res = A.InvokeCode("Get-Command Add-Debugger.ps1 -Type ExternalScript -ErrorAction Ignore");
		if (res.Count == 0)
		{
			throw new ModuleException("""
				Cannot find the required script Add-Debugger.ps1.
				Get Add-Debugger.ps1 -- https://www.powershellgallery.com/packages/Add-Debugger
				""");
		}
	}

	public static void AddDebugger(PowerShell ps, Hashtable? parameters)
	{
		ps.AddCommand("Add-Debugger.ps1");
		if (parameters is { })
			ps.AddParameters(parameters);
		ps.Invoke();
		ps.Commands.Clear();
	}

	internal static void OnLineBreakpoint()
	{
		if (Far.Api.Window.Kind != WindowKind.Editor)
			return;

		var editor = Far.Api.Editor;
		var breakpoints = A.InvokeCode("Get-PSBreakpoint");

		OnLineBreakpoint(editor, breakpoints, true);
	}

	internal static void OnLineBreakpoint(IEditor? editor, Collection<PSObject> breakpoints, bool noUI = false)
	{
		string? file = null;
		int line = 0;

		LineBreakpoint? bpFound = null;
		if (editor != null)
		{
			// location
			file = editor.FileName;
			line = editor.Caret.Y + 1;

			// find
			foreach (PSObject o in breakpoints!)
			{
				if (o.BaseObject is LineBreakpoint lbp && lbp.Action is null && line == lbp.Line && Kit.Equals(file, lbp.Script))
				{
					bpFound = lbp;
					break;
				}
			}

			// found?
			if (bpFound != null)
			{
				switch (Far.Api.Message("Breakpoint exists",
					"Line breakpoint",
					MessageOptions.None,
					[
						"&Remove",
						bpFound.Enabled ? "&Disable" : "&Enable",
						"&Modify",
						"&Add",
						"&Cancel",
					]))
				{
					case 0:
						A.RemoveBreakpoint(bpFound);
						return;
					case 1:
						if (bpFound.Enabled)
							A.DisableBreakpoint(bpFound);
						else
							A.InvokeCode("Enable-PSBreakpoint -Breakpoint $args[0]", bpFound);
						return;
					case 2:
						break;
					case 3:
						bpFound = null;
						break;
					default:
						return;
				}
			}
		}

		if (noUI)
		{
			// remove old
			if (bpFound != null)
				A.RemoveBreakpoint(bpFound);

			// set new
			A.SetBreakpoint(file, line, null);

			return;
		}

		// go
		var ui = new BreakpointDialog(0, file, line);
		if (!ui.Show())
			return;

		// remove old
		if (bpFound != null)
			A.RemoveBreakpoint(bpFound);

		// set new
		A.SetBreakpoint(ui.Script, int.Parse(ui.Matter, null), ui.Action);
	}
}
