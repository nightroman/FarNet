using FarNet;
using RedisKit.Commands;

namespace RedisKit;

[ModuleCommand(Name = Host.MyName, Prefix = "rk", Id = "dc4bb7f5-a3b2-42f9-9ef5-4aef0c47e03b")]
public class Command : ModuleCommand
{
	public override void Invoke(object sender, ModuleCommandEventArgs e)
	{
		try
		{
			var parameters = CommandParameters.Parse(e.Command);
			AbcCommand command = parameters.Command switch
			{
				"edit" => new EditCommand(parameters),
				"hash" => new HashCommand(parameters),
				"keys" => new KeysCommand(parameters),
				"list" => new ListCommand(parameters),
				"set" => new SetCommand(parameters),
				"tree" => new TreeCommand(parameters),
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
