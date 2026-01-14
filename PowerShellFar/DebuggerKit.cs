using FarNet;
using FarNet.Works;
using PowerShellFar.UI;
using System.Collections;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.CompilerServices;

namespace PowerShellFar;

static class DebuggerKit
{
	[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "IsDebuggerStopEventSubscribed")]
	public static extern bool IsDebuggerStopEventSubscribed(Debugger instance);

	public static bool HasDebugger(Runspace runspace)
	{
		return IsDebuggerStopEventSubscribed(runspace.Debugger);
	}

	public static void ValidateAvailable()
	{
		var res = A.InvokeCode("Get-Command Add-Debugger.ps1 -Type ExternalScript -ErrorAction Ignore");
		if (res.Count == 0)
		{
			throw new ModuleException("""
				Found no attached debugger, breakpoints with no actions will not be hit.
				Tried to add and could not find the recommended default Add-Debugger.ps1.
				See PSGallery -- https://www.powershellgallery.com/packages/Add-Debugger
				""");
		}
	}

	public static void AddDebugger(Runspace runspace, Hashtable? parameters)
	{
		using var ps = PowerShell.Create(runspace);
		ps.AddCommand("Add-Debugger.ps1");
		if (parameters is { } && parameters.Count > 0)
			ps.AddParameters(parameters);
		ps.Invoke();
	}

	public static void AttachDebugger()
	{
		if (HasDebugger(A.Runspace))
		{
			Far.Api.Message("The debugger is already attached to 'main'.", "Debugger");
			return;
		}

		while (!HasDebugger(A.Runspace))
		{
			var r = AttachDebuggerDialog.Show(A.Runspace);
			if (r == AttachDebuggerDialog.Cancel)
				return;

			if (r == AttachDebuggerDialog.AddDebugger)
			{
				if (0 == A.InvokeCode("Get-Command Add-Debugger.ps1 -Type ExternalScript -ErrorAction Ignore").Count)
				{
					Far.Api.Message("""
						Cannot find "Add-Debugger.ps1" in the path.
						Get the script from PSGallery and place in the path.
						https://www.powershellgallery.com/packages/Add-Debugger
						""",
						"Debugger");
				}
				else
				{
					A.InvokeCode("Add-Debugger.ps1");
				}
			}
		}
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
		if (editor is { })
		{
			// location
			file = editor.FileName;
			line = editor.Caret.Y + 1;

			// find
			foreach (PSObject o in breakpoints!)
			{
				if (o.BaseObject is LineBreakpoint lbp && lbp.Action is null && line == lbp.Line && Kit.EqualsIgnoreCase(file, lbp.Script))
				{
					bpFound = lbp;
					break;
				}
			}

			// found?
			if (bpFound is { })
			{
				switch (Far.Api.Message(
					bpFound.ToString(),
					"Line breakpoint",
					MessageOptions.None,
					[
						"&Remove",
						bpFound.Enabled ? "&Disable" : "&Enable",
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
							A.EnableBreakpoint(bpFound);
						return;
					default:
						return;
				}
			}
		}

		if (noUI)
		{
			// remove old
			if (bpFound is { })
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
		if (bpFound is { })
			A.RemoveBreakpoint(bpFound);

		// set new
		A.SetBreakpoint(ui.Script, int.Parse(ui.Matter, null), ui.Action);
	}
}
