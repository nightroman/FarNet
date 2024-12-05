using FarNet;
using JsonKit.Commands;
using System;
using System.Linq;

namespace JsonKit;

[ModuleCommand(Name = Host.MyName, Prefix = "jk", Id = "2b7c9109-0d36-49d6-8d29-da28d2a00cb1")]
public class Command : ModuleCommand
{
	public override void Invoke(object sender, ModuleCommandEventArgs e)
	{
		AnyCommand? command = null;
		try
		{
			var (subcommand, parameters) = Parameters.Parse(e.Command);

			command = subcommand switch
			{
				"open" => new OpenCommand(parameters),
				_ => throw new ModuleException($"Unknown command 'rk:{subcommand}'.")
			};

			if (parameters.Count > 0)
			{
				throw new ModuleException($"""
				Uknknown parameters
				Subcommand: {subcommand}
				Parameters: {string.Join(", ", parameters.Keys.Cast<string>())}
				""");
			}

			command.Invoke();
		}
		catch (ModuleException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw new ModuleException(ex.Message, ex);
		}
		finally
		{
			command?.Dispose();
		}
	}
}
