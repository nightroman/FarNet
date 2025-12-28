using FarNet;
using GitKit.Commands;

namespace GitKit;

[ModuleCommand(Name = Host.MyName, Prefix = "gk", Id = "15a36561-bf47-47a5-ae43-9729eda272a3")]
public class Command : ModuleCommand
{
	public override void Invoke(object sender, ModuleCommandEventArgs e)
	{
		InvokeSubcommand(e.Command, static (name, parameters) =>
		name switch
		{
			"blame" => new BlameCommand(parameters),
			"branches" => new BranchesCommand(parameters),
			"cd" => new CDCommand(parameters),
			"changes" => new ChangesCommand(parameters),
			"checkout" => new CheckoutCommand(parameters),
			"clone" => new CloneCommand(parameters),
			"commit" => new CommitCommand(parameters),
			"commits" => new CommitsCommand(parameters),
			"config" => new ConfigCommand(parameters),
			"edit" => new EditCommand(parameters),
			"init" => new InitCommand(parameters),
			"pull" => new PullCommand(parameters),
			"push" => new PushCommand(parameters),
			"status" => new StatusCommand(parameters),
			_ => null
		});
	}
}
