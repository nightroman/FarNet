using FarNet;
using LibGit2Sharp;
using System;
using System.Data.Common;

namespace GitKit;

[ModuleCommand(Name = Host.MyName, Prefix = "gk", Id = "15a36561-bf47-47a5-ae43-9729eda272a3")]
public class Command : ModuleCommand
{
	public override void Invoke(object sender, ModuleCommandEventArgs e)
	{
		int index = 0;
		var command = e.Command;
		while (index < command.Length && char.IsLetter(command[index]))
			++index;

		string subcommand = command[0..index];
		DbConnectionStringBuilder parameters;
		try
		{
			parameters = new() { ConnectionString = command[index..] };
		}
		catch (ArgumentException ex)
		{
			throw new ModuleException($"Invalid parameters: {ex.Message}");
		}

		Invoke(subcommand, parameters);
	}

	static void Invoke(string subcommand, DbConnectionStringBuilder parameters)
	{
		Repository? repo = null;
		try
		{
			AnyCommand? command = subcommand switch
			{
				"clone" => new CloneCommand(parameters),
				"init" => new InitCommand(parameters),
				_ => null
			};

			if (command is null)
			{
				repo = RepositoryFactory.Instance(Host.GetFullPath(parameters.GetValue("repo")));
				command = subcommand switch
				{
					"" => new StatusCommand(repo),
					"branches" => new BranchesCommand(repo),
					"cd" => new CDCommand(repo, parameters),
					"changes" => new ChangesCommand(repo),
					"checkout" => new CheckoutCommand(repo, parameters),
					"commit" => new CommitCommand(repo, parameters),
					"commits" => new CommitsCommand(repo),
					"pull" => new PullCommand(repo),
					"push" => new PushCommand(repo),
					_ => throw new ModuleException($"Unknown command '{subcommand}'.")
				};
			}

			parameters.AssertNone();
			command.Invoke();
		}
		catch (LibGit2SharpException ex)
		{
			throw new ModuleException(ex.Message, ex);
		}
		finally
		{
			repo?.Release();
		}
	}
}
