using FarNet;
using GitKit.Commands;
using LibGit2Sharp;

namespace GitKit.Panels;

public class BranchesPanel : BasePanel
{
	public new BranchesExplorer MyExplorer => (BranchesExplorer)Explorer;

	public BranchesPanel(BranchesExplorer explorer) : base(explorer)
	{
		SortMode = PanelSortMode.Unsorted;

		var co = new SetColumn { Kind = "O", Name = " ", Width = 2 };
		var cn = new SetColumn { Kind = "N", Name = "Branch" };
		var cd = new SetColumn { Kind = "Z", Name = "Commit" };

		var plan0 = new PanelPlan { Columns = [co, cn, cd] };
		SetPlan(0, plan0);

		SetView(plan0);
	}

	protected override string HelpTopic => "branches-panel";

	string GetSampleBranchName(string branchName)
	{
		var index = branchName.IndexOf('/');
		if (index < 0)
			return branchName == Const.NoBranchName ? string.Empty : branchName;

		using var repo = new Repository(GitDir);

		var branch = repo.Branches[branchName];
		if (!branch.IsRemote)
			return branchName;

		return branchName[(index + 1)..];
	}

	string? InputNewBranchName(string branchName)
	{
		return Far.Api.Input(
			"New branch name",
			"GitBranch",
			$"Create new branch from '{branchName}'",
			GetSampleBranchName(branchName));
	}

	void CloneBranch(ExplorerEventArgs args, string branchName, Action action)
	{
		var newName = InputNewBranchName(branchName);
		if (string.IsNullOrEmpty(newName))
		{
			args.Result = JobResult.Ignore;
			return;
		}

		args.Data = (branchName, newName);
		action();
	}

	void CheckoutBranch()
	{
		var branchName = CurrentFile?.Name;
		if (branchName is null)
			return;

		using var repo = new Repository(GitDir);

		var branch = repo.MyBranch(branchName);
		if (branch.IsCurrentRepositoryHead)
			return;

		// create local tracked branch from remote
		if (branch.IsRemote)
		{
			var newName = InputNewBranchName(branchName);
			if (string.IsNullOrEmpty(newName))
				return;

			var newBranch = repo.CreateBranch(newName, branch.Tip);

			branch = repo.Branches.Update(
				newBranch,
				b => b.TrackedBranch = branch.CanonicalName);

			PostName(newName);
		}

		// checkout local branch
		if (!repo.Info.IsBare)
			LibGit2Sharp.Commands.Checkout(repo, branch);

		Update(true);
		Redraw();
	}

	void CompareBranches()
	{
		var (branchName1, branchName2) = GetSelectedDataRange(x => x.Name);
		if (branchName2 is null)
			return;

		using var repo = new Repository(GitDir);

		var branch1 = branchName1 is null ? repo.Head : repo.MyBranch(branchName1);
		var branch2 = repo.MyBranch(branchName2);

		var commits = new Commit[] { branch1.Tip, branch2.Tip }.OrderBy(x => x.Author.When).ToArray();

		CompareCommits(commits[0].Sha, commits[1].Sha);
	}

	void MergeBranch()
	{
		var branchName = CurrentFile?.Name;
		if (branchName is null)
			return;

		using var repo = new Repository(GitDir);

		if (repo.Info.IsHeadDetached)
			return;

		var branch = repo.Branches[branchName];

		if (branch.Tip == repo.Head.Tip)
			return;

		if (0 != Far.Api.Message(
			$"Merge branch '{branchName}' into '{repo.Head.FriendlyName}'?",
			Host.MyName,
			MessageOptions.YesNo))
			return;

		repo.Merge(branch, Lib.BuildSignature(repo));

		Update(true);
		Redraw();
	}

	void PushBranch()
	{
		var branchName = CurrentFile?.Name;
		if (branchName is null)
			return;

		if (branchName == Const.NoBranchName)
			throw new ModuleException($"Cannot push {Const.NoBranchName}.");

		using var repo = new Repository(GitDir);

		var branch = repo.Branches[branchName];
		PushCommand.PushBranch(repo, branch);
	}

	void CopyInfo()
	{
		var branchName = CurrentFile?.Name;
		if (branchName is null)
			return;

		using var repo = new Repository(GitDir);

		var branch = repo.MyBranch(branchName);
		if (branch.Tip is { } tip)
			UI.CopyInfo(repo, tip);
	}

	internal override void AddMenu(IMenu menu)
	{
		menu.Add(Const.CopyInfoMenu, (s, e) => CopyInfo());
		menu.Add(Const.PushBranch, (s, e) => PushBranch());
		menu.Add(Const.MergeBranch, (s, e) => MergeBranch());
		menu.Add(Const.CompareBranches, (s, e) => CompareBranches());
	}

	public override void UICloneFile(CloneFileEventArgs args)
	{
		var branchName = args.File.Name;
		CloneBranch(args, branchName, () => Explorer.CloneFile(args));
	}

	public override void UICreateFile(CreateFileEventArgs args)
	{
		using var repo = new Repository(GitDir);

		var branch = repo.Head;
		CloneBranch(args, branch.FriendlyName, () => Explorer.CreateFile(args));
	}

	public override void UIDeleteFiles(DeleteFilesEventArgs args)
	{
		var text = $"Delete branches ({args.Files.Count})\n{string.Join("\n", args.Files.Select(x => x.Name))}";
		var op = MessageOptions.YesNo;
		if (args.Force)
			op |= MessageOptions.Warning;

		if (0 != Far.Api.Message(text, Host.MyName, op))
		{
			args.Result = JobResult.Ignore;
			return;
		}

		Explorer.DeleteFiles(args);
	}

	public override void UIRenameFile(RenameFileEventArgs args)
	{
		var branchName = args.File.Name;
		if (branchName == Const.NoBranchName)
		{
			args.Result = JobResult.Ignore;
			return;
		}

		using var repo = new Repository(GitDir);

		var branch = repo.Branches[branchName];
		if (branch.IsRemote)
		{
			args.Result = JobResult.Ignore;
			return;
		}

		var newName = (Far.Api.Input("New branch name", "GitBranch", "Rename branch", branchName) ?? string.Empty).Trim();
		if (newName.Length == 0)
		{
			args.Result = JobResult.Ignore;
			return;
		}

		args.Data = newName;
		Explorer.RenameFile(args);
	}

	public override bool UIKeyPressed(KeyInfo key)
	{
		switch (key.VirtualKeyCode)
		{
			// checkout cursor branch
			case KeyCode.Enter when key.IsShift():
				CheckoutBranch();
				return true;
		}

		return base.UIKeyPressed(key);
	}
}
