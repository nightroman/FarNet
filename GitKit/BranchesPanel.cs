using FarNet;
using LibGit2Sharp;
using System;
using System.Linq;

namespace GitKit;

class BranchesPanel : BasePanel<BranchesExplorer>
{
	public BranchesPanel(BranchesExplorer explorer) : base(explorer)
	{
		Title = $"Branches {explorer.Repository.Info.WorkingDirectory}";
		SortMode = PanelSortMode.Unsorted;
		ViewMode = 0;

		var co = new SetColumn { Kind = "O", Name = " ", Width = 2 };
		var cn = new SetColumn { Kind = "N", Name = "Branch" };
		var cd = new SetColumn { Kind = "Z", Name = "Commit" };

		var plan0 = new PanelPlan { Columns = new FarColumn[] { co, cn, cd } };
		SetPlan(0, plan0);
	}

	protected override string HelpTopic => "branches-panel";

	static string GetSampleBranchName(Branch branch)
	{
		var name = branch.FriendlyName;
		if (!branch.IsRemote)
			return name;

		var index = name.IndexOf('/');
		return index < 0 ? name : name[(index + 1)..];
	}

	static string? InputNewBranchName(Branch branch)
	{
		return Far.Api.Input(
			"New branch name",
			"GitBranch",
			$"Create new branch from '{branch.FriendlyName}'",
			GetSampleBranchName(branch));
	}

	static void CloneBranch(ExplorerEventArgs args, Branch branch, Action action)
	{
		var newName = InputNewBranchName(branch);
		if (string.IsNullOrEmpty(newName))
		{
			args.Result = JobResult.Ignore;
			return;
		}

		args.Data = (branch, newName);
		action();
	}

	void CheckoutBranch()
	{
		if (CurrentFile?.Data is not Branch branch || branch.IsCurrentRepositoryHead)
			return;

		// create local tracked branch from remote
		if (branch.IsRemote)
		{
			var newName = InputNewBranchName(branch);
			if (string.IsNullOrEmpty(newName))
				return;

			var newBranch = Repository.CreateBranch(newName, branch.Tip);

			branch = Repository.Branches.Update(
				newBranch,
				b => b.TrackedBranch = branch.CanonicalName);

			PostName(newName);
		}

		// checkout local branch
		Commands.Checkout(Repository, branch);

		Update(true);
		Redraw();
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

		if (CurrentFile?.Data is not Branch branch || branch.Tip == Repository.Head.Tip)
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

	public void PushBranch()
	{
		if (CurrentFile?.Data is not Branch branch || branch.IsRemote)
			return;

		var changes = Lib.GetChanges(Repository);
		if (changes.Count > 0)
			throw new ModuleException($"Cannot push: {changes.Count} not committed changes.");

		if (0 != Far.Api.Message(
			$"Push branch '{branch.FriendlyName}'?",
			Host.MyName,
			MessageOptions.YesNo))
			return;

		var op = new PushOptions
		{
			CredentialsProvider = Lib.GitCredentialsHandler
		};

		if (branch.TrackedBranch is null)
		{
			var menu = Far.Api.CreateListMenu();
			menu.Title = "Select remote";
			menu.UsualMargins = true;
			foreach(var it in Repository.Network.Remotes)
				menu.Add(it.Name).Data = it;

			if (!menu.Show() || menu.SelectedData is not Remote remote)
				return;

			branch = Repository.Branches.Update(
				branch,
				b => b.Remote = remote.Name,
				b => b.UpstreamBranch = branch.CanonicalName);
		}

		Repository.Network.Push(branch, op);

		Update(true);
		Redraw();
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
			// checkout cursor branch
			case KeyCode.Enter when key.IsShift():
				CheckoutBranch();
				return true;
		}

		return base.UIKeyPressed(key);
	}
}
