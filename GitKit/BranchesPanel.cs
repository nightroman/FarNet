using FarNet;
using LibGit2Sharp;
using System;
using System.IO;
using System.Linq;

namespace GitKit;

class BranchesPanel : BasePanel<BranchesExplorer>
{
	public BranchesPanel(BranchesExplorer explorer) : base(explorer)
	{
		Title = $"Branches {explorer.Repository.Info.WorkingDirectory}";
		SortMode = PanelSortMode.Unsorted;
		ViewMode = 0;

		var co = new SetColumn { Kind = "O", Name = "Current", Width = 1 };
		var cn = new SetColumn { Kind = "N", Name = "Branch" };
		var cd = new SetColumn { Kind = "Z", Name = "Commit" };

		var plan0 = new PanelPlan { Columns = new FarColumn[] { co, cn, cd } };
		SetPlan(0, plan0);
	}

	protected override string HelpTopic => "branches-panel";

	static void CloneBranch(ExplorerEventArgs args, Branch branch, Action action)
	{
		var friendlyName = branch.FriendlyName;
		var newName = Far.Api.Input(
			"New branch name",
			"GitBranch",
			$"Create new branch from '{friendlyName}'",
			Path.GetFileName(friendlyName));

		if (string.IsNullOrEmpty(newName))
		{
			args.Result = JobResult.Ignore;
			return;
		}

		args.Data = (branch, newName);
		action();
	}

	public override void UICloneFile(CloneFileEventArgs args)
	{
		var branch = (Branch)args.File.Data!;
		CloneBranch(args, branch, () => Explorer.CloneFile(args));
	}

	public override void UICreateFile(CreateFileEventArgs args)
	{
		var branch = Repository.Head;
		CloneBranch(args, branch, () => Explorer.CreateFile(args));
	}

	public override void UIDeleteFiles(DeleteFilesEventArgs args)
	{
		var text = $"Delete {args.Files.Count} branches:\n{string.Join("\n", args.Files.Select(x => x.Name))}";
		var op = MessageOptions.YesNo | MessageOptions.LeftAligned;
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
		var branch = (Branch)args.File.Data!;
		if (branch.IsRemote)
		{
			args.Result = JobResult.Ignore;
			return;
		}

		var newName = (Far.Api.Input("New branch name", "GitBranch", "Rename branch", branch.FriendlyName) ?? string.Empty).Trim();
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
			// checkout remote or local branch
			case KeyCode.Enter when key.IsShift():
				var branch = (Branch?)CurrentFile?.Data;
				if (branch is not null && !branch.IsCurrentRepositoryHead)
				{
					// create a new local branch from remote
					if (branch.IsRemote)
					{
						var newName = Path.GetFileName(branch.FriendlyName);
						branch = Repository.CreateBranch(newName, branch.Tip);
						PostName(newName);
					}

					// checkout local branch
					Commands.Checkout(Repository, branch);

					Update(true);
					Redraw();
				}
				return true;
		}

		return base.UIKeyPressed(key);
	}

	public void CompareBranches()
	{
		var (data1, data2) = GetSelectedDataRange<Branch>();
		if (data2 is null)
			return;

		data1 ??= Repository.Head;

		var commits = new Commit[] { data1.Tip, data2.Tip }.OrderBy(x => x.Author.When).ToArray();

		CompareCommits(commits[0], commits[1]);
	}

	public void MergeBranch()
	{
		if (Repository.Info.IsHeadDetached)
			return;

		var branch = CurrentFile?.Data as Branch;
		if (branch is null || branch.Tip == Repository.Head.Tip)
			return;

		if (0 != Far.Api.Message(
			$"Merge branch '{branch.FriendlyName}' into '{Repository.Head.FriendlyName}'?",
			Host.MyName,
			MessageOptions.YesNo))
			return;

		Repository.Merge(branch, Lib.BuildSignature(Repository));

		Update(true);
		Redraw();
	}
}
