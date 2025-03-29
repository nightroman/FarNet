using System.Collections;
using System.Management.Automation;

namespace PowerShellFar;

static class AddDebuggerKit
{
	public static void ValidateAvailable()
	{
		if (0 == A.InvokeCode("Get-Command Add-Debugger.ps1 -Type ExternalScript -ErrorAction 0").Count)
			throw new InvalidOperationException("""
				Cannot find the required script Add-Debugger.ps1.
				Get Add-Debugger.ps1 -- https://www.powershellgallery.com/packages/Add-Debugger
				""");
	}

	public static void AddDebugger(PowerShell ps, Hashtable? parameters)
	{
		ps.AddCommand("Add-Debugger.ps1");
		if (parameters is { })
			ps.AddParameters(parameters);
		ps.Invoke();
		ps.Commands.Clear();
	}
}
