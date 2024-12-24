using FarNet;
using GitKit.Panels;

namespace GitKit.Commands;

sealed class BranchesCommand(CommandParameters parameters) : BaseCommand(parameters)
{
	public override void Invoke()
	{
		new BranchesExplorer(GitDir)
			.CreatePanel()
			.Open();
	}
}
