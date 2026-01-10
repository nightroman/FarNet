using FarNet;
using FarNet.Tools;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace ScriptPS;

/*
	Example command:

	fn: script=ScriptPS; method=.Demo.Message ;; name=John Doe; age=42

	Points of interest:

	- How to get the PowerShellFar runspace and use on calls.
	- How to call PowerShell code with passed parameters.
	- How to get the result, optionally strongly typed.
*/

public static class Script
{
	public static void Message(string name = "unknown", int age = -1)
	{
		var runspace = (Runspace)PowerShellFar.Runspace;

		using var ps = PowerShell.Create();
		ps.Runspace = runspace;

		var res = ps
			.AddScript("""param($name, $age) $Far.Message("Hello $name, age $age.", 'ScriptPS', 'OkCancel')""")
            .AddArgument(name)
            .AddArgument(age)
            .Invoke<int>();

		Far.Api.Message(res[0] switch { 0 => "Ok", 1 => "Cancel", _ => "Escape" });
	}
}
