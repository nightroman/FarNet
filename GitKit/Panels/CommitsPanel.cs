using FarNet;
using LibGit2Sharp;
using System.IO;
using System.Linq;

namespace GitKit;

class CommitsPanel : BasePanel<CommitsExplorer>
{
	public CommitsPanel(CommitsExplorer explorer) : base(explorer)
	{
		SortMode = PanelSortMode.Unsorted;
		ViewMode = 0;

		var settings = Settings.Default.GetData();
		PageLimit = settings.CommitsPageLimit;

		var cn = new SetColumn { Kind = "N", Name = "Commit" };
		var cm = new SetColumn { Kind = "DM", Name = "Date" };
		var co = new SetColumn { Kind = "O", Name = " ", Width = 1 };

		var plan0 = new PanelPlan { Columns = new FarColumn[] { co, cn } };
		SetPlan(0, plan0);

		var plan9 = new PanelPlan { Columns = new FarColumn[] { co, cn, cm } };
		SetPlan((PanelViewMode)9, plan9);
	}

	protected override string HelpTopic => "commits-panel";

	public Branch Branch => ((CommitsExplorer.BranchCommits)Explorer.Data).Branch;

	void PushBranch()
	{
		PushCommand.PushBranch(Repository, Branch);
	}

	void CompareCommits()
	{
		var (data1, data2) = GetSelectedDataRange<Commit>();
		if (data2 is null)
			return;

		data1 ??= Branch.Tip;

		var commits = new Commit[] { data1, data2 }.OrderBy(x => x.Author.When).ToArray();

		CompareCommits(commits[0], commits[1]);
	}

	void CreateBranch()
	{
		var commit = CurrentFile?.Data as Commit;
		if (commit is null)
			return;

		var friendlyName = Branch.FriendlyName;
		var settings = Settings.Default.GetData();
		var hash = commit.Sha[0..settings.ShaPrefixLength];
		var newName = Far.Api.Input(
			"New branch name",
			"GitBranch",
			$"Create new branch from {friendlyName} {hash}",
			$"{Path.GetFileName(friendlyName)}-{hash}");

		if (string.IsNullOrEmpty(newName))
			return;

		Repository.CreateBranch(newName, commit);
	}

	internal override void AddMenu(IMenu menu)
	{
		if (Explorer.Data is CommitsExplorer.BranchCommits)
		{
			menu.Add("Push branch", (s, e) => PushBranch());
			menu.Add("Create branch", (s, e) => CreateBranch());
			menu.Add("Compare commits", (s, e) => CompareCommits());
		}
	}
}
