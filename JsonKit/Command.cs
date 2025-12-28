using FarNet;
using JsonKit.Commands;

namespace JsonKit;

[ModuleCommand(Name = Host.MyName, Prefix = "jk", Id = "2b7c9109-0d36-49d6-8d29-da28d2a00cb1")]
public class Command : ModuleCommand
{
	public override void Invoke(object sender, ModuleCommandEventArgs e)
	{
		InvokeSubcommand(e.Command, static (name, parameters) =>
		name switch
		{
			"open" => new OpenCommand(parameters),
			_ => null
		});
	}
}
