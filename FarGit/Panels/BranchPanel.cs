using FarNet;
using LibGit2Sharp;

namespace FarGit.Panels;

/// <summary>
/// Panel showing local and remote-tracking branches.
///
/// Key bindings:
///   Enter / F3  – switch to (checkout) the highlighted branch
///   F5          – merge highlighted branch into current branch
///   F7          – create a new branch from current HEAD
///   F8          – delete the highlighted branch
/// </summary>
public class BranchPanel : BasePanel
{
	public BranchPanel(BranchExplorer explorer) : base(explorer)
	{
		SortMode = PanelSortMode.Unsorted;

		var co = new SetColumn { Kind = "O", Name = " ", Width = 3 };
		var cn = new SetColumn { Kind = "N", Name = "Branch" };
		var cz = new SetColumn { Kind = "Z", Name = "Tracking status", Width = 20 };

		var plan0 = new PanelPlan { Columns = [co, cn, cz] };
		SetPlan(0, plan0);
		SetView(plan0);

		SetKeyBars([
			new KeyBar(KeyCode.F2, ControlKeyStates.None, "Menu",   "Panel actions"),
			new KeyBar(KeyCode.F3, ControlKeyStates.None, "Switch", "Switch to this branch"),
			new KeyBar(KeyCode.F4, ControlKeyStates.None, "Rename", "Rename branch"),
			new KeyBar(KeyCode.F5, ControlKeyStates.None, "Merge",  "Merge into current branch"),
			new KeyBar(KeyCode.F7, ControlKeyStates.None, "Create", "Create new branch"),
			new KeyBar(KeyCode.F8, ControlKeyStates.None, "Delete", "Delete branch"),
		]);
	}

	protected override string HelpTopic => "branch-panel";

	BranchFile? CurrentBranch => CurrentFile as BranchFile;

	// ── Checkout ──────────────────────────────────────────────────────────

	void CheckoutCurrent()
	{
		var f = CurrentBranch;
		if (f is null || f.IsCurrent) return;
		if (f.IsRemote)
		{
			Far.Api.Message(
				"This is a remote-tracking branch. To work on it, create a local branch:\n\n" +
				"  1. Press F7 to create a new branch\n" +
				"  2. Name it the same as the remote branch (without the remote prefix)\n\n" +
				"Or use the Create Branch guided workflow.",
				Const.ModuleName);
			return;
		}

		try
		{
			using var repo = UseRepository();
			var branch = repo.Branches[f.BranchName]
				?? throw new Exception($"Branch '{f.BranchName}' not found.");
			LibGit2Sharp.Commands.Checkout(repo, branch);
			Update(true); Redraw();
		}
		catch (CheckoutConflictException ex)
		{
			Far.Api.Message(
				$"Cannot switch: you have uncommitted changes that conflict.\n\n{ex.Message}\n\n" +
				"Tip: Stage and commit your changes first, or stash them\n" +
				"(Dashboard → Stash or Guide Me → Set work aside).",
				Const.ModuleName, MessageOptions.Warning);
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// ── Rename ────────────────────────────────────────────────────────────

	void RenameCurrent()
	{
		var f = CurrentBranch;
		if (f is null) return;
		if (f.IsRemote)
		{
			Far.Api.Message("Cannot rename remote-tracking branches.", Const.ModuleName);
			return;
		}
		if (f.IsCurrent)
		{
			// Renaming the current branch is allowed — HEAD will follow
		}

		var newName = Far.Api.Input(
			$"New name for branch '{f.BranchName}':",
			"FarGit-branch-rename",
			"Rename Branch",
			f.BranchName);
		if (string.IsNullOrWhiteSpace(newName) || newName.Trim() == f.BranchName) return;
		newName = newName.Trim();

		try
		{
			using var repo = UseRepository();
			repo.Branches.Rename(f.BranchName, newName);
			Update(true); Redraw();
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// ── Create ────────────────────────────────────────────────────────────

	void CreateBranch()
	{
		string currentBranch;
		using (var repo = UseRepository())
			currentBranch = repo.Head.FriendlyName;

		var name = Far.Api.Input(
			$"New branch name (branching from '{currentBranch}'):",
			"FarGit-branch",
			"Create Branch");

		if (string.IsNullOrWhiteSpace(name)) return;
		name = name.Trim();

		try
		{
			using var repo = UseRepository();
			var branch = repo.CreateBranch(name);
			LibGit2Sharp.Commands.Checkout(repo, branch);
			Update(true); Redraw();
			Far.Api.Message($"Switched to new branch '{name}'.", Const.ModuleName);
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// ── Merge ─────────────────────────────────────────────────────────────

	void MergeCurrent()
	{
		var f = CurrentBranch;
		if (f is null) return;

		string currentBranch;
		using (var repo = UseRepository())
			currentBranch = repo.Head.FriendlyName;

		if (f.BranchName == currentBranch)
		{
			Far.Api.Message("Cannot merge a branch into itself.", Const.ModuleName);
			return;
		}

		if (0 != Far.Api.Message(
			$"Merge '{f.BranchName}' into '{currentBranch}'?\n\n" +
			"This will combine the history of both branches.\n" +
			"If there are conflicts you will need to resolve them manually.",
			Const.ModuleName, MessageOptions.YesNo))
			return;

		try
		{
			using var repo = UseRepository();
			var branch = repo.Branches[f.BranchName]
				?? throw new Exception($"Branch '{f.BranchName}' not found.");
			var sig = Lib.BuildSignature(repo);
			var result = repo.Merge(branch, sig, new MergeOptions());

			switch (result.Status)
			{
				case MergeStatus.FastForward:
					Far.Api.Message($"Fast-forward merge complete.\nHEAD is now at {result.Commit?.Sha[..7]}.", Const.ModuleName);
					break;
				case MergeStatus.NonFastForward:
					Far.Api.Message($"Merge complete — a new merge commit was created.", Const.ModuleName);
					break;
				case MergeStatus.Conflicts:
					Far.Api.Message(
						"Merge resulted in conflicts.\n\n" +
						"Open the Status panel to see which files conflict.\n" +
						"Edit each conflicted file, resolve the markers, then\n" +
						"stage the file and commit to complete the merge.",
						Const.ModuleName, MessageOptions.Warning);
					break;
				case MergeStatus.UpToDate:
					Far.Api.Message("Already up to date — nothing to merge.", Const.ModuleName);
					break;
			}

			Update(true); Redraw();
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// ── Delete ────────────────────────────────────────────────────────────

	void DeleteCurrent()
	{
		var f = CurrentBranch;
		if (f is null) return;
		if (f.IsCurrent)
		{
			Far.Api.Message("Cannot delete the currently checked-out branch.", Const.ModuleName);
			return;
		}
		if (f.IsRemote)
		{
			Far.Api.Message(
				"Deleting remote-tracking branches from here is not supported.\n" +
				"Use the Remote panel to manage remote refs.",
				Const.ModuleName);
			return;
		}

		if (0 != Far.Api.Message(
			$"Delete local branch '{f.BranchName}'?\n\n" +
			"Any commits that exist only on this branch will be lost.",
			Const.ModuleName, MessageOptions.YesNo | MessageOptions.Warning))
			return;

		try
		{
			using var repo = UseRepository();
			repo.Branches.Remove(f.BranchName);
			Update(true); Redraw();
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// ── Menu / key handling ───────────────────────────────────────────────

	internal override void AddMenu(IMenu menu)
	{
		menu.Add(Const.BranchCheckout, (_, _) => CheckoutCurrent());
		menu.Add(Const.BranchMerge,    (_, _) => MergeCurrent());
		menu.Add(string.Empty).IsSeparator = true;
		menu.Add(Const.BranchCreate,   (_, _) => CreateBranch());
		menu.Add(Const.BranchRename,   (_, _) => RenameCurrent());
		menu.Add(Const.BranchDelete,   (_, _) => DeleteCurrent());
	}

	public override void UIOpenFile(FarFile file)
	{
		if (file is BranchFile) CheckoutCurrent();
	}

	public override bool UIKeyPressed(KeyInfo key)
	{
		switch (key.VirtualKeyCode)
		{
			case KeyCode.F3 when key.Is():
				CheckoutCurrent();
				return true;
			case KeyCode.F4 when key.Is():
				RenameCurrent();
				return true;
			case KeyCode.F5 when key.Is():
				MergeCurrent();
				return true;
			case KeyCode.F7 when key.Is():
				CreateBranch();
				return true;
			case KeyCode.F8 when key.Is():
				DeleteCurrent();
				return true;
		}
		return base.UIKeyPressed(key);
	}
}
