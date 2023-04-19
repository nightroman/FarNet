using LibGit2Sharp;

namespace GitKit;

sealed class ChangesCommand : BaseCommand
{
	public ChangesCommand(Repository repo) : base(repo)
	{
	}

	public override void Invoke()
	{
		Lib.GetExistingTip(_repo);
		new ChangesExplorer(_repo, () => Lib.GetChanges(_repo))
			.CreatePanel()
			.Open();
	}
}
