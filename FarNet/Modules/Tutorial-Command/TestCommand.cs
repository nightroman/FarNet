
using FarNet;
using System;

[System.Runtime.InteropServices.Guid("63b00e54-bcda-4fa4-b1b8-d6bebaa4f548")]
[ModuleCommand(Name = Command.Name, Prefix = "Command")]
public class Command : ModuleCommand
{
	public const string Name = "Test command";

	public override void Invoke(object sender, ModuleCommandEventArgs e)
	{
		Far.Net.Message(e.Command);
	}
}
