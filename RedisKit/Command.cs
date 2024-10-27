using FarNet;
using RedisKit.Commands;
using System;
using System.Linq;

namespace RedisKit;

[ModuleCommand(Name = Host.MyName, Prefix = "rk", Id = "dc4bb7f5-a3b2-42f9-9ef5-4aef0c47e03b")]
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
				"keys" => new KeysCommand(parameters),
				"tree" => new TreeCommand(parameters),
				"edit" => new EditCommand(parameters),
				"hash" => new HashCommand(parameters),
				"list" => new ListCommand(parameters),
				"set" => new SetCommand(parameters),
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
