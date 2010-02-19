
using FarNet;
using System;
using System.Runtime.InteropServices;

[ModuleCommand(Name = Command.Name, Prefix = "Command")]
[Guid("63b00e54-bcda-4fa4-b1b8-d6bebaa4f548")]
public class Command : ModuleCommand
{
	public const string Name = "Test command";

	public override void Invoke(object sender, ModuleCommandEventArgs e)
	{
		Far.Net.Message(e.Command);
	}
}
