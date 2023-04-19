using LibGit2Sharp;

namespace GitKit;

sealed class CommitsCommand : BaseCommand
{
	public CommitsCommand(Repository repo) : base(repo)
	{
	}

	public override void Invoke()
	{
		new CommitsExplorer(_repo, _repo.Head)
			.CreatePanel()
			.Open();
	}
}
