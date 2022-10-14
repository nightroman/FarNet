using FarNet;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace ScriptPS;

public static class Demo
{
    // Runs pure PowerShell code with arguments
    // fn: script=ScriptPS; method=ScriptPS.Demo.Message; unload=true :: name=John Doe; age=42
    public static void Message(string name = "unknown", int age = -1)
	{
		using var ps = PowerShell.Create();
		var res = ps
            .AddScript("'Hello {0}, {1}' -f $args[0], $args[1]")
            .AddArgument(name)
            .AddArgument(age)
            .Invoke<string>();
		Far.Api.Message(res[0]);
	}

	// Runs PowerShell code in PowerShellFar main session
	// fn: script=ScriptPS; method=ScriptPS.Demo.MessagePsf; unload=true
	public static void MessagePsf()
    {
        var manager = Far.Api.GetModuleManager("PowerShellFar");
        var runspace = (Runspace)manager.Interop("Runspace", null);
        using var ps = PowerShell.Create();
        ps.Runspace = runspace;
        ps
            .AddScript("$Far.Message('Hello from PowerShellFar')")
            .Invoke();
    }
}
