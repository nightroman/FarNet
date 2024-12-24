using FarNet;
using GitKit.About;
using GitKit.Commands;
using LibGit2Sharp;
using System.IO;
using System.Linq;

namespace GitKit.Panels;

class CommitsPanel : BasePanel<CommitsExplorer>
{
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

	public string BranchName => ((CommitsExplorer.BranchCommits)Explorer.Commits).BranchName;

	void PushBranch()
	{
		if (BranchName == Const.NoBranchName)
			throw new ModuleException($"Cannot push {Const.NoBranchName}.");

		using var repo = new Repository(GitDir);

		var branch = repo.Branches[BranchName];
		PushCommand.PushBranch(repo, branch);
	}

	void CompareCommits()
	{
		var (commitSha1, commitSha2) = GetSelectedDataRange(x => (x as CommitFile)?.CommitSha);
		if (commitSha2 is null)
			return;

		using var repo = new Repository(GitDir);

		var branch = repo.MyBranch(BranchName);

		var commit1 = commitSha1 is null ? branch.Tip : repo.Lookup<Commit>(commitSha1);
		var commit2 = repo.Lookup<Commit>(commitSha2);

		var commits = new Commit[] { commit1, commit2 }.OrderBy(x => x.Author.When).ToArray();

		CompareCommits(commits[0].Sha, commits[1].Sha);
	}

	void CreateBranch()
	{
		if (CurrentFile is not CommitFile file)
			return;

		var settings = Settings.Default.GetData();
		var hash = file.CommitSha[0..settings.ShaPrefixLength];
		var newName = Far.Api.Input(
			"New branch name",
			"GitBranch",
			$"Create new branch from {BranchName} {hash}",
			$"{Path.GetFileName(BranchName)}-{hash}");

		if (string.IsNullOrEmpty(newName))
			return;

		using var repo = new Repository(GitDir);

		var commit = repo.Lookup<Commit>(file.CommitSha);
		repo.CreateBranch(newName, commit);
	}

	void CopySha()
	{
		if (CurrentFile is CommitFile { CommitSha: { } commitSha })
			CopySha(commitSha);
	}

	internal override void AddMenu(IMenu menu)
	{
		menu.Add("Copy SHA-1", (s, e) => CopySha());

		if (Explorer.Commits is CommitsExplorer.BranchCommits)
		{
			menu.Add("Push branch", (s, e) => PushBranch());
			menu.Add("Create branch", (s, e) => CreateBranch());
			menu.Add("Compare commits", (s, e) => CompareCommits());
		}
	}
}
