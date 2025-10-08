using FarNet;
using GitKit.Commands;
using LibGit2Sharp;

namespace GitKit.Panels;

public class CommitsPanel : BasePanel
{
	public new CommitsExplorer MyExplorer => (CommitsExplorer)Explorer;

	public CommitsPanel(CommitsExplorer explorer) : base(explorer)
	{
		SortMode = PanelSortMode.Unsorted;

		var settings = Settings.Default.GetData();
		PageLimit = settings.CommitsPageLimit;

		var cn = new SetColumn { Kind = "N", Name = "Commit" };
		var co = new SetColumn { Kind = "O", Name = " ", Width = 1 };

		var plan0 = new PanelPlan { Columns = [co, cn] };
		SetPlan(0, plan0);

		SetView(plan0);
	}

	protected override string HelpTopic => "commits-panel";

	void CompareCommits(string branchName)
	{
		var (commitSha1, commitSha2) = GetSelectedDataRange(x => (x as CommitFile)?.CommitSha);
		if (commitSha2 is null)
			return;

		using var repo = new Repository(GitDir);

		var branch = repo.MyBranch(branchName);

		var commit1 = commitSha1 is null ? branch.Tip : repo.Lookup<Commit>(commitSha1);
		var commit2 = repo.Lookup<Commit>(commitSha2);

		var commits = new Commit[] { commit1, commit2 }.OrderBy(x => x.Author.When).ToArray();

		CompareCommits(commits[0].Sha, commits[1].Sha);
	}

	void CopyTip()
	{
		if (CurrentFile is CommitFile file)
		{
			using var repo = new Repository(GitDir);
			if (repo.Lookup<Commit>(file.CommitSha) is { } commit)
				UI.CopyTip(commit);
		}
	}

	void CreateBranch(string branchName)
	{
		if (CurrentFile is not CommitFile file)
			return;

		var settings = Settings.Default.GetData();
		var hash = file.CommitSha[0..settings.ShaPrefixLength];
		var newName = Far.Api.Input(
			"New branch name",
			"GitBranch",
			$"Create new branch from {branchName} {hash}",
			$"{Path.GetFileName(branchName)}-{hash}");

		if (string.IsNullOrEmpty(newName))
			return;

		using var repo = new Repository(GitDir);

		var commit = repo.Lookup<Commit>(file.CommitSha);
		repo.CreateBranch(newName, commit);
	}

	void PushBranch(string branchName)
	{
		if (branchName == Const.NoBranchName)
			throw new ModuleException($"Cannot push {Const.NoBranchName}.");

		using var repo = new Repository(GitDir);

		var branch = repo.Branches[branchName];
		PushCommand.PushBranch(repo, branch);
	}

	internal override void AddMenu(IMenu menu)
	{
		menu.Add(Const.TipInfo, (s, e) => CopyTip());

		if (MyExplorer.BranchName is { } branchName)
		{
			menu.Add(Const.PushBranch, (s, e) => PushBranch(branchName));
			menu.Add(Const.CreateBranch, (s, e) => CreateBranch(branchName));
			menu.Add(Const.CompareCommits, (s, e) => CompareCommits(branchName));
		}
	}
}
