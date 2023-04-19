using LibGit2Sharp;

namespace GitKit;

sealed class BranchesCommand : BaseCommand
{
	public BranchesCommand(Repository repo) : base(repo)
	{
	}

	public override void Invoke()
	{
		new BranchesExplorer(_repo)
			.CreatePanel()
			.Open();
	}
}
