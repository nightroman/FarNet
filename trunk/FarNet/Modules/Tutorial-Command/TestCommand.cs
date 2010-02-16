
using System;
using FarNet;

[ModuleCommand(Name = Command.Name, Prefix = "Command")]
public class Command : ModuleCommand
{
	public const string Name = "Test command";

	public override void Invoke(object sender, ModuleCommandEventArgs e)
	{
		Far.Net.Message(e.Command);
	}
}
