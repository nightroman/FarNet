using FarNet;
using JsonKit.Commands;

namespace JsonKit;

[ModuleCommand(Name = Host.MyName, Prefix = "jk", Id = "2b7c9109-0d36-49d6-8d29-da28d2a00cb1")]
public class Command : ModuleCommand
{
	public override void Invoke(object sender, ModuleCommandEventArgs e)
	{
		try
		{
			var parameters = CommandParameters.Parse(e.Command);
			AbcCommand command = parameters.Command switch
			{
				"open" => new OpenCommand(parameters),
				_ => throw new ModuleException($"Unknown command '{parameters.Command}'.")
			};

			parameters.ThrowUnknownParameters();
			command.Invoke();
		}
		catch (Exception ex)
		{
			throw new ModuleException(ex.Message, ex);
		}
	}
}
