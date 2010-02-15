
using System;
using FarNet;

// The command uses with default name and prefix, the class name "cmd1".
public class Cmd1 : ModuleCommand
{
	// The command handler shows the prefix and the command
	public override void Invoke(object sender, ModuleCommandEventArgs e)
	{
		Far.Net.Message(e.Prefix + ":" + e.Command);
	}
}

// This command provides the name and the prefix. Invoke() is the same.
[ModuleCommand(Name = "Test command", Prefix = "cmd")]
public class Cmd2 : Cmd1
{
}
