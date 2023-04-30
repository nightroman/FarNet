using FarNet;
using LibGit2Sharp;
using System.Linq;

namespace GitKit;

[ModuleCommand(Name = Host.MyName, Prefix = "gk", Id = "15a36561-bf47-47a5-ae43-9729eda272a3")]
public class Command : ModuleCommand
{
	public override void Invoke(object sender, ModuleCommandEventArgs e)
	{
		var (subcommand, parameters) = Parameters.Parse(e.Command);
		if (subcommand is null || parameters is null)
		{
			Host.Instance.ShowHelpTopic("commands");
			return;
		}

		try
		{
			using AnyCommand command = subcommand switch
			{
				"branches" => new BranchesCommand(parameters),
				"cd" => new CDCommand(parameters),
				"changes" => new ChangesCommand(parameters),
				"checkout" => new CheckoutCommand(parameters),
				"clone" => new CloneCommand(parameters),
				"commit" => new CommitCommand(parameters),
				"commits" => new CommitsCommand(parameters),
				"edit" => new EditCommand(parameters),
				"init" => new InitCommand(parameters),
				"pull" => new PullCommand(parameters),
				"push" => new PushCommand(parameters),
				"status" => new StatusCommand(parameters),
				_ => throw new ModuleException($"Unknown command 'gk:{subcommand}'.")
			};

			if (parameters.Count > 0)
			{
				var message = $"""
				Uknknown parameters
				Subcommand: {subcommand}
				Parameters: {string.Join(", ", parameters.Keys.Cast<string>())}
				""";
				throw new ModuleException(message);
			}

			command.Invoke();
		}
		catch (LibGit2SharpException ex)
		{
			throw new ModuleException(ex.Message, ex);
		}
	}
}
