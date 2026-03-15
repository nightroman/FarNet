using FarNet;
using LibGit2Sharp;

namespace FarGit.Workflows;

/// <summary>
/// Guided step-by-step workflows for git newcomers.
/// Each workflow explains what git is doing at every step in plain English.
/// </summary>
static class Wizard
{
	// ── Entry point ──────────────────────────────────────────────────────────

	public static void Show(string gitDir, Panels.DashboardPanel panel)
	{
		var menu = Far.Api.CreateMenu();
		menu.Title = "Git Guide  —  What do you want to do?";
		menu.Add("Save my changes  (stage + commit)");
		menu.Add("Undo changes to a file");
		menu.Add("Undo my last commit");
		menu.Add("Set work aside temporarily  (stash)");
		menu.Add("Mark a release version  (tag)");
		menu.Add("See what has changed  (status)");
		menu.Add("─────────────────────────────").IsSeparator = true;
		menu.Add("Start a new branch");
		menu.Add("Switch to a different branch");
		menu.Add("Combine branches  (merge)");
		menu.Add("─────────────────────────────").IsSeparator = true;
		menu.Add("Get latest changes from remote  (pull)");
		menu.Add("Share my work  (push)");
		menu.Add("Get a copy of a repository  (clone)");
		menu.Add("Connect this repo to a remote  (add remote)");

		if (!menu.Show()) return;

		switch (menu.Selected)
		{
			case 0:  SaveChanges(gitDir, panel);      break;
			case 1:  UndoChanges(gitDir, panel);      break;
			case 2:  UndoLastCommit(gitDir, panel);   break;
			case 3:  StashWork(gitDir, panel);         break;
			case 4:  MarkVersion(gitDir, panel);       break;
			case 5:  ReviewStatus(gitDir, panel);      break;
			// separators are 6 and 10 — Selected skips them
			case 7:  CreateBranch(gitDir, panel);      break;
			case 8:  SwitchBranch(gitDir, panel);      break;
			case 9:  MergeBranch(gitDir, panel);       break;
			case 11: PullLatest(gitDir, panel);        break;
			case 12: PushWork(gitDir, panel);          break;
			case 13: CloneRepo(panel);                 break;
			case 14: ConnectToRemote(gitDir, panel);   break;
		}
	}

	// ── Helper ───────────────────────────────────────────────────────────────

	/// <summary>
	/// Shows a multi-line info/step message with Continue / Cancel buttons.
	/// Returns false if the user pressed Cancel or Escape.
	/// </summary>
	static bool Step(string body, string title = "Git Guide")
		=> 0 == Far.Api.Message(body, title, (MessageOptions)0, ["Continue", "Cancel"]);

	/// <summary>Shows an info message with only an OK button.</summary>
	static void Info(string body, string title = "Git Guide")
		=> Far.Api.Message(body, title);

	// ── Workflow 1: Save My Changes ──────────────────────────────────────────

	static void SaveChanges(string gitDir, Panels.DashboardPanel panel)
	{
		if (!Step(
			"SAVE MY CHANGES\n\n" +
			"Git saves work in two steps:\n\n" +
			"  1. STAGE  — choose which changed files to include\n" +
			"  2. COMMIT — seal them with a short description\n\n" +
			"Think of staging like putting items into a box\n" +
			"before you seal and label it.\n\n" +
			"Press Continue, Esc to cancel."))
			return;

		int stagedCount, changedCount;
		using (var repo = new Repository(gitDir))
		{
			var status = repo.RetrieveStatus(new StatusOptions { IncludeUntracked = true });
			stagedCount  = status.Count(e => Lib.StagedSymbol(e.State)   != " ");
			changedCount = status.Count(e => Lib.UnstagedSymbol(e.State) != " ")
			             + status.Untracked.Count();
		}

		if (stagedCount == 0 && changedCount == 0)
		{
			Info("Your working tree is already clean — nothing to save!\n\n" +
			     "All your changes have already been committed.");
			return;
		}

		if (stagedCount == 0)
		{
			if (!Step(
				$"STEP 1 of 2: STAGE\n\n" +
				$"You have {changedCount} changed file(s) that are not yet staged.\n\n" +
				$"Staging means: tell git 'include these changes in my next save'.\n\n" +
				$"Press Continue to stage ALL changed files now.\n\n" +
				$"Tip: If you only want to save some files, press Cancel here,\n" +
				$"open Status, select files with the Space bar, then press F5."))
				return;

			using var repo = new Repository(gitDir);
			LibGit2Sharp.Commands.Stage(repo, "*");
			stagedCount = changedCount;
		}
		else
		{
			Step($"STEP 1 of 2: STAGE\n\n" +
			     $"You already have {stagedCount} file(s) staged and ready to commit.\n\n" +
			     $"Skipping staging — moving on to commit.");
		}

		if (!Step(
			"STEP 2 of 2: COMMIT\n\n" +
			$"You are about to commit {stagedCount} file(s).\n\n" +
			"Next you will write a short commit message. Good examples:\n\n" +
			"  • Fix login button on mobile\n" +
			"  • Add dark mode support\n" +
			"  • Update README with install instructions\n\n" +
			"Keep it short but meaningful — future-you will thank you!\n\n" +
			"Press Continue to open the commit editor."))
			return;

		Commands.Commit.Open(gitDir, false);
		panel.Update(true);
		panel.Redraw();
	}

	// ── Workflow 2: Undo Changes ─────────────────────────────────────────────

	static void UndoChanges(string gitDir, Panels.DashboardPanel panel)
	{
		if (!Step(
			"UNDO CHANGES TO A FILE\n\n" +
			"Git can restore any file to the state it was in\n" +
			"at your last commit. This is called a 'revert'.\n\n" +
			"⚠ WARNING: This permanently discards any changes\n" +
			"you have made since your last commit. There is no\n" +
			"undo for this operation.\n\n" +
			"If you want to keep your changes but just set them\n" +
			"aside temporarily, use 'Set work aside (stash)' instead.\n\n" +
			"Press Continue to open the Status panel."))
			return;

		if (!Step(
			"HOW TO UNDO A FILE\n\n" +
			"In the Status panel that is about to open:\n\n" +
			"  1. Use the arrow keys to highlight the file you want to undo\n" +
			"  2. Press F8  (labeled 'Revert!' at the bottom of the screen)\n" +
			"  3. Confirm when prompted\n\n" +
			"To undo several files at once:\n" +
			"  • Press Space on each file to mark it (a bullet appears)\n" +
			"  • Then press F8 to revert all marked files\n\n" +
			"Press Continue to open Status now."))
			return;

		new Panels.StatusExplorer(gitDir).CreatePanel().OpenChild(panel);
	}

	// ── Workflow 3: Undo Last Commit ─────────────────────────────────────────

	static void UndoLastCommit(string gitDir, Panels.DashboardPanel panel)
	{
		string sha7, msg, branch;
		bool hasRemote;
		using (var repo = new Repository(gitDir))
		{
			if (repo.Head.Tip is null)
			{
				Info("There are no commits to undo.");
				return;
			}
			sha7      = repo.Head.Tip.Sha[..7];
			msg       = repo.Head.Tip.MessageShort;
			branch    = repo.Head.FriendlyName;
			hasRemote = repo.Head.IsTracking;
		}

		var warning = hasRemote
			? "⚠ Your branch is linked to a remote.\n" +
			  "Only do this if you have NOT pushed this commit yet!\n" +
			  "Undoing a pushed commit causes problems for anyone\n" +
			  "who already pulled it.\n\n"
			: "";

		if (!Step(
			"UNDO MY LAST COMMIT\n\n" +
			"This removes the last commit from the history of\n" +
			$"'{branch}', but KEEPS all the file changes staged\n" +
			"so you can edit and re-commit them.\n\n" +
			"Think of it as 'unpacking' the commit back into\n" +
			"your working area.\n\n" +
			$"{warning}" +
			$"Last commit:  {sha7}  \"{msg}\"\n\n" +
			"Press Continue to undo it."))
			return;

		try
		{
			Commands.RemoteOps.ResetSoft(gitDir);

			Info($"Done! Commit {sha7} has been undone.\n\n" +
			     "Your file changes are still here — staged and ready.\n" +
			     "You can edit them and commit again when you're ready.");

			panel.Update(true);
			panel.Redraw();
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// ── Workflow 4: Stash Work ───────────────────────────────────────────────

	static void StashWork(string gitDir, Panels.DashboardPanel panel)
	{
		if (!Step(
			"SET WORK ASIDE TEMPORARILY  (Stash)\n\n" +
			"A stash is like a clipboard for your work-in-progress.\n\n" +
			"Use it when you need to:\n" +
			"  • Switch tasks without losing what you're working on\n" +
			"  • Pull updates from a colleague without conflicts\n" +
			"  • Try something else and come back later\n\n" +
			"Your changes are saved privately and your files go back\n" +
			"to the last commit state. You can restore them any time.\n\n" +
			"Press Continue to create a stash now, Cancel to abort."))
			return;

		int changedCount;
		using (var repo = new Repository(gitDir))
		{
			var status = repo.RetrieveStatus(new StatusOptions { IncludeUntracked = true });
			changedCount = status.Count(e => Lib.StagedSymbol(e.State)   != " ")
			             + status.Count(e => Lib.UnstagedSymbol(e.State) != " ")
			             + status.Untracked.Count();
		}

		if (changedCount == 0)
		{
			Info("Your working tree is clean — there is nothing to stash!\n\n" +
			     "Make some changes first, then come back here.");
			return;
		}

		var description = Far.Api.Input(
			$"Give this stash a short description (optional):\n(e.g. 'WIP: login refactor')",
			"FarGit-stash",
			"Create Stash");

		if (description is null) return;
		if (string.IsNullOrWhiteSpace(description)) description = "WIP";

		try
		{
			using var repo = new Repository(gitDir);
			var sig = Lib.BuildSignature(repo);
			repo.Stashes.Add(sig, description,
				StashModifiers.IncludeUntracked | StashModifiers.KeepIndex);

			Info($"Stash created: \"{description}\"\n\n" +
			     $"Your {changedCount} changed file(s) are safely stored.\n" +
			     $"Your working directory is now clean.\n\n" +
			     $"To restore your work later:\n" +
			     $"  • Open the Stash panel from the Dashboard\n" +
			     $"  • Highlight your stash and press F5 (Apply) or F6 (Pop)");

			panel.Update(true);
			panel.Redraw();
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// ── Workflow 4: Mark a Version ───────────────────────────────────────────

	static void MarkVersion(string gitDir, Panels.DashboardPanel panel)
	{
		if (!Step(
			"MARK A RELEASE VERSION  (Tag)\n\n" +
			"A tag is a named bookmark on a specific commit.\n" +
			"Use tags to mark important moments like releases.\n\n" +
			"Common conventions:\n" +
			"  • v1.0   — version 1.0 release\n" +
			"  • v2.3.1 — patch release\n" +
			"  • beta-1 — pre-release\n\n" +
			"The tag will point to your most recent commit (HEAD).\n\n" +
			"Press Continue to create a tag, Cancel to abort."))
			return;

		string tipSha, tipMsg;
		using (var repo = new Repository(gitDir))
		{
			if (repo.Head.Tip is null)
			{
				Info("There are no commits yet — make your first commit before tagging.");
				return;
			}
			tipSha = repo.Head.Tip.Sha[..7];
			tipMsg = repo.Head.Tip.MessageShort;
		}

		var tagName = Far.Api.Input(
			$"Tag name for commit {tipSha} \"{tipMsg}\":\n(e.g. v1.0)",
			"FarGit-tag",
			"Create Tag");

		if (string.IsNullOrWhiteSpace(tagName)) return;
		tagName = tagName.Trim();

		try
		{
			using var repo = new Repository(gitDir);
			repo.ApplyTag(tagName);

			Info($"Tag '{tagName}' created on commit {tipSha}\n\n" +
			     $"\"{tipMsg}\"\n\n" +
			     $"You can view and manage all tags from the Dashboard\n" +
			     $"by pressing Enter on the Tags row.");

			panel.Update(true);
			panel.Redraw();
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// ── Workflow 5: Review Status ────────────────────────────────────────────

	static void ReviewStatus(string gitDir, Panels.DashboardPanel panel)
	{
		int stagedCount, unstagedCount, untrackedCount;
		string branch;
		using (var repo = new Repository(gitDir))
		{
			branch = repo.Head.FriendlyName;
			var status = repo.RetrieveStatus(new StatusOptions { IncludeUntracked = true });
			stagedCount    = status.Count(e => Lib.StagedSymbol(e.State)   != " ");
			unstagedCount  = status.Count(e => Lib.UnstagedSymbol(e.State) != " ");
			untrackedCount = status.Untracked.Count();
		}

		var summary = new System.Text.StringBuilder();
		summary.AppendLine("WHAT HAS CHANGED?\n");
		summary.AppendLine($"Branch: {branch}\n");

		if (stagedCount == 0 && unstagedCount == 0 && untrackedCount == 0)
			summary.AppendLine("Everything is clean — no pending changes.");
		else
		{
			if (stagedCount   > 0) summary.AppendLine($"  Staged:    {stagedCount} file(s)    ← ready to commit");
			if (unstagedCount > 0) summary.AppendLine($"  Unstaged:  {unstagedCount} file(s)  ← changed, not yet staged");
			if (untrackedCount > 0) summary.AppendLine($"  Untracked: {untrackedCount} file(s) ← new, not yet tracked");

			summary.AppendLine();
			if (stagedCount > 0)
				summary.AppendLine("You have staged changes ready to commit  (press F7).");
			else
				summary.AppendLine("Nothing staged yet. Use 'Save my changes' to stage and commit.");
		}

		summary.Append("\nPress Continue to open the Status panel for details.");
		if (!Step(summary.ToString())) return;

		if (!Step(
			"READING THE STATUS PANEL\n\n" +
			"Files are grouped into three sections:\n\n" +
			"  Staged    — changes included in your next commit\n" +
			"  Unstaged  — changes NOT yet included\n" +
			"  Untracked — brand-new files git doesn't know about yet\n\n" +
			"Useful keys in the Status panel:\n" +
			"  F3  — see exactly what changed in a file (diff)\n" +
			"  F4  — open the file in the editor\n" +
			"  F5  — stage the highlighted file\n" +
			"  F7  — commit staged changes\n" +
			"  F8  — revert (undo) changes — cannot be undone!\n\n" +
			"Press Continue to open Status now."))
			return;

		new Panels.StatusExplorer(gitDir).CreatePanel().OpenChild(panel);
	}

	// ── Workflow 6: Create Branch ────────────────────────────────────────────

	static void CreateBranch(string gitDir, Panels.DashboardPanel panel)
	{
		string currentBranch;
		using (var repo = new Repository(gitDir))
			currentBranch = repo.Head.FriendlyName;

		if (!Step(
			"START A NEW BRANCH\n\n" +
			"A branch is your own isolated workspace.\n" +
			"Changes you make on a branch don't affect other branches\n" +
			"until you deliberately merge them.\n\n" +
			$"You are currently on: '{currentBranch}'\n\n" +
			"The new branch will start from exactly where you are now.\n\n" +
			"Good branch naming:\n" +
			"  • feature/user-login\n" +
			"  • bugfix/reset-password\n" +
			"  • hotfix/crash-on-start\n\n" +
			"Press Continue to name and create the branch."))
			return;

		var name = Far.Api.Input(
			$"New branch name (branching from '{currentBranch}'):",
			"FarGit-branch",
			"Create Branch");

		if (string.IsNullOrWhiteSpace(name)) return;
		name = name.Trim();

		try
		{
			using var repo = new Repository(gitDir);
			var branch = repo.CreateBranch(name);
			LibGit2Sharp.Commands.Checkout(repo, branch);

			Info($"You are now on branch '{name}'.\n\n" +
			     $"Any commits you make will be on this branch.\n" +
			     $"'{currentBranch}' remains exactly as it was.\n\n" +
			     $"When you're done, use 'Combine branches (merge)' to\n" +
			     $"bring your changes back into '{currentBranch}'.");

			panel.Update(true);
			panel.Redraw();
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// ── Workflow 7: Switch Branch ────────────────────────────────────────────

	static void SwitchBranch(string gitDir, Panels.DashboardPanel panel)
	{
		List<string> branchNames;
		string currentBranch;

		using (var repo = new Repository(gitDir))
		{
			currentBranch = repo.Head.FriendlyName;
			branchNames = repo.Branches
				.Where(b => !b.IsRemote && !b.IsCurrentRepositoryHead)
				.OrderBy(b => b.FriendlyName)
				.Select(b => b.FriendlyName)
				.ToList();
		}

		if (branchNames.Count == 0)
		{
			Info($"There are no other branches to switch to.\n\n" +
			     $"You only have '{currentBranch}'. Use 'Start a new branch'\n" +
			     $"to create one.");
			return;
		}

		if (!Step(
			"SWITCH TO A DIFFERENT BRANCH\n\n" +
			"Switching branches changes the files you see to match\n" +
			"the state of that branch.\n\n" +
			"⚠ Important: Commit or stash your current changes first!\n" +
			"Uncommitted changes may conflict with the branch you're switching to.\n\n" +
			$"Currently on: '{currentBranch}'\n\n" +
			"Press Continue to pick a branch."))
			return;

		var menu = Far.Api.CreateMenu();
		menu.Title = "Switch to branch:";
		foreach (var b in branchNames)
			menu.Add(b);

		if (!menu.Show()) return;
		var target = branchNames[menu.Selected];

		try
		{
			using var repo = new Repository(gitDir);
			var branch = repo.Branches[target];
			LibGit2Sharp.Commands.Checkout(repo, branch);

			Info($"Switched to '{target}'.\n\n" +
			     $"Your files now reflect the state of this branch.\n" +
			     $"Any new commits will be added to '{target}'.");

			panel.Update(true);
			panel.Redraw();
		}
		catch (CheckoutConflictException ex)
		{
			Far.Api.Message(
				$"Cannot switch — you have uncommitted changes that would conflict:\n\n{ex.Message}\n\n" +
				"Tip: Stage and commit your changes, or use\n" +
				"'Set work aside temporarily (stash)' first.",
				Const.ModuleName, MessageOptions.Warning);
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// ── Workflow 8: Merge Branch ─────────────────────────────────────────────

	static void MergeBranch(string gitDir, Panels.DashboardPanel panel)
	{
		string currentBranch;
		List<string> branchNames;

		using (var repo = new Repository(gitDir))
		{
			currentBranch = repo.Head.FriendlyName;
			branchNames = repo.Branches
				.Where(b => !b.IsRemote && !b.IsCurrentRepositoryHead)
				.OrderBy(b => b.FriendlyName)
				.Select(b => b.FriendlyName)
				.ToList();
		}

		if (branchNames.Count == 0)
		{
			Info("There are no other branches to merge from.");
			return;
		}

		if (!Step(
			"COMBINE BRANCHES  (Merge)\n\n" +
			"Merging takes the commits from another branch and brings\n" +
			"them into your current branch.\n\n" +
			$"Currently on: '{currentBranch}'\n\n" +
			"You will pick which branch to merge INTO this one.\n\n" +
			"What can happen:\n" +
			"  • CLEAN: git merges automatically, nothing to do\n" +
			"  • CONFLICTS: same parts changed in both branches,\n" +
			"    you'll need to choose which version to keep\n\n" +
			"Press Continue to pick the branch to merge in."))
			return;

		var menu = Far.Api.CreateMenu();
		menu.Title = $"Merge into '{currentBranch}' from:";
		foreach (var b in branchNames)
			menu.Add(b);

		if (!menu.Show()) return;
		var source = branchNames[menu.Selected];

		try
		{
			using var repo = new Repository(gitDir);
			var sig = Lib.BuildSignature(repo);
			var result = repo.Merge(repo.Branches[source], sig, new MergeOptions());

			switch (result.Status)
			{
				case MergeStatus.FastForward:
					Info($"Merge complete!\n\n" +
					     $"'{source}' was merged into '{currentBranch}' cleanly.\n" +
					     $"(Fast-forward: no separate merge commit needed.)");
					break;

				case MergeStatus.NonFastForward:
					Info($"Merge complete!\n\n" +
					     $"A merge commit was created combining '{source}'\n" +
					     $"and '{currentBranch}'.");
					break;

				case MergeStatus.Conflicts:
					Info("Merge has CONFLICTS.\n\n" +
					     "Some files were changed on both branches and git\n" +
					     "couldn't automatically decide which version to use.\n\n" +
					     "To resolve:\n" +
					     "  1. Open Status (Dashboard → Staged/Unstaged)\n" +
					     "  2. Open each conflicted file in the editor (F4)\n" +
					     "  3. Look for <<<<<<< markers and edit to keep the right code\n" +
					     "  4. Delete all conflict markers\n" +
					     "  5. Stage the file (F5) and commit (F7)");
					break;

				case MergeStatus.UpToDate:
					Info($"Nothing to merge — '{currentBranch}' already\n" +
					     $"contains all commits from '{source}'.");
					break;
			}

			panel.Update(true);
			panel.Redraw();
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// ── Workflow 9: Pull Latest ──────────────────────────────────────────────

	static void PullLatest(string gitDir, Panels.DashboardPanel panel)
	{
		string branch, remoteName;
		using (var repo = new Repository(gitDir))
		{
			branch     = repo.Head.FriendlyName;
			remoteName = repo.Head.IsTracking
				? (repo.Head.TrackedBranch.RemoteName ?? "origin")
				: "origin";
		}

		if (!Step(
			"GET LATEST CHANGES FROM REMOTE  (Pull)\n\n" +
			"Pull downloads the latest commits from the server and\n" +
			"merges them into your current branch.\n\n" +
			$"Branch:  {branch}\n" +
			$"Remote:  {remoteName}\n\n" +
			"Best practice: commit or stash your changes first!\n" +
			"Pulling with uncommitted changes can cause conflicts.\n\n" +
			"If credentials are needed, you will be prompted.\n" +
			"For GitHub: use a Personal Access Token (PAT), not your password.\n\n" +
			"Press Continue to pull now."))
			return;

		try
		{
			var output = Commands.RemoteOps.Pull(gitDir);
			Info($"Pull complete!\n\n{output}");
			panel.Update(true);
			panel.Redraw();
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// ── Workflow 10: Push Work ───────────────────────────────────────────────

	static void PushWork(string gitDir, Panels.DashboardPanel panel)
	{
		string branch;
		bool hasTracking;
		using (var repo = new Repository(gitDir))
		{
			branch      = repo.Head.FriendlyName;
			hasTracking = repo.Head.IsTracking;
		}

		var trackingNote = hasTracking
			? "Your branch is already linked to a remote — the push will go there."
			: "This branch has no remote link yet.\nYou'll be asked which remote to push to (usually 'origin').";

		if (!Step(
			"SHARE MY WORK  (Push)\n\n" +
			"Push uploads your local commits to the remote server\n" +
			"so your team can see them (or so you have a backup).\n\n" +
			$"Branch: {branch}\n\n" +
			$"{trackingNote}\n\n" +
			"If credentials are needed, you will be prompted.\n" +
			"For GitHub: use a Personal Access Token (PAT), not your password.\n\n" +
			"Press Continue to push now."))
			return;

		try
		{
			Commands.RemoteOps.Push(gitDir);
			Info($"Push complete!\n\n" +
			     $"Your commits on '{branch}' are now on the remote server.");

			panel.Update(true);
			panel.Redraw();
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// ── Workflow 11: Clone Repository ────────────────────────────────────────

	public static void CloneRepo(Panels.DashboardPanel? panel)
	{
		if (!Step(
			"GET A COPY OF A REPOSITORY  (Clone)\n\n" +
			"Cloning downloads a complete copy of a git repository\n" +
			"to your machine, including all history and branches.\n\n" +
			"You need:\n" +
			"  1. The repository URL  (from GitHub, GitLab, etc.)\n" +
			"  2. A local folder to clone into\n\n" +
			"Finding the URL on GitHub:\n" +
			"  • Open the repo page on github.com\n" +
			"  • Click the green 'Code' button\n" +
			"  • Copy the HTTPS URL\n\n" +
			"Press Continue to enter the URL and destination."))
			return;

		var url = Far.Api.Input(
			"Repository URL:\n(e.g. https://github.com/user/project.git)",
			"FarGit-clone-url",
			"Clone Repository");

		if (string.IsNullOrWhiteSpace(url)) return;

		var defaultDir = Far.Api.CurrentDirectory;
		var localPath = Far.Api.Input(
			"Local destination folder:\n(repository will be cloned into a subfolder here)",
			"FarGit-clone-path",
			"Clone Repository") ?? defaultDir;

		if (string.IsNullOrWhiteSpace(localPath)) return;

		try
		{
			var result = Commands.RemoteOps.Clone(url.Trim(), localPath.Trim());

			if (0 == Far.Api.Message(
				$"Clone complete!\n\nRepository cloned to:\n{result}\n\nOpen it in FarGit now?",
				"Clone Repository", (MessageOptions)0, ["Open", "Later"]))
			{
				var gitDir = Lib.GetGitDir(result);
				new Panels.DashboardExplorer(gitDir).CreatePanel().Open();
			}
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// ── Workflow 12: Connect to Remote ───────────────────────────────────────

	static void ConnectToRemote(string gitDir, Panels.DashboardPanel panel)
	{
		List<string> existingRemotes;
		string folderName;
		using (var repo = new Repository(gitDir))
		{
			existingRemotes = repo.Network.Remotes.Select(r => r.Name).ToList();
			folderName = Path.GetFileName(
				repo.Info.WorkingDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
		}

		var intro = existingRemotes.Count == 0
			? "CONNECT THIS REPO TO A REMOTE\n\n" +
			  "A 'remote' is a copy of your repo on a hosting service like\n" +
			  "GitHub, GitLab, or Azure DevOps.\n\n" +
			  "Connecting lets you:\n" +
			  "  • Back up your work online\n" +
			  "  • Share code with teammates\n" +
			  "  • Access your code from any machine\n\n" +
			  "Press Continue to choose how to connect."
			: "ADD ANOTHER REMOTE\n\n" +
			  $"You already have {existingRemotes.Count} remote(s) connected:\n" +
			  string.Join("\n", existingRemotes.Select(r => $"  • {r}")) + "\n\n" +
			  "Press Continue to add another, or Cancel to stop.";

		if (!Step(intro)) return;

		var methodMenu = Far.Api.CreateMenu();
		methodMenu.Title = "How do you want to connect?";
		methodMenu.Add("Create a new repo on GitHub for me");
		methodMenu.Add("I already have a repo — enter its URL");
		if (!methodMenu.Show()) return;

		if (methodMenu.Selected == 0)
			ConnectViaGitHub(gitDir, folderName, panel);
		else
			ConnectViaUrl(gitDir, panel);
	}

	static void ConnectViaGitHub(string gitDir, string suggestedName, Panels.DashboardPanel panel)
	{
		// ── Get or prompt for GitHub PAT ─────────────────────────────────────
		var stored = Commands.Credentials.Get("github.com");
		string pat;

		if (stored is { } creds)
		{
			pat = creds.Token;
		}
		else
		{
			if (!Step(
				"GITHUB PERSONAL ACCESS TOKEN\n\n" +
				"To create a GitHub repo, FarGit needs a Personal Access Token (PAT).\n\n" +
				"How to create one:\n" +
				"  1. Open github.com → click your avatar → Settings\n" +
				"  2. Scroll to Developer settings (bottom of left sidebar)\n" +
				"  3. Personal access tokens → Tokens (classic)\n" +
				"  4. Generate new token (classic)\n" +
				"  5. Give it a name, check the 'repo' scope, click Generate\n" +
				"  6. Copy the token  (starts with 'ghp_')\n\n" +
				"FarGit will save it encrypted so you only need to do this once.\n\n" +
				"Press Continue to enter your token."))
				return;

			pat = Far.Api.Input(
				"GitHub Personal Access Token  (starts with 'ghp_'):",
				"FarGit-github-pat",
				"GitHub Token") ?? "";
			if (string.IsNullOrWhiteSpace(pat)) return;
			pat = pat.Trim();

			// Validate PAT and save with returned username
			try
			{
				var username = Commands.GitHubApi.GetUsername(pat);
				Commands.Credentials.Set("github.com", username, pat);
			}
			catch (Exception ex)
			{
				Far.Api.Message(
					$"Could not verify token:\n\n{ex.Message}\n\n" +
					"Double-check that you copied the full token and that it has 'repo' scope.",
					Const.ModuleName, MessageOptions.Warning);
				return;
			}
		}

		// ── Ask for repo details ──────────────────────────────────────────────
		var repoName = Far.Api.Input(
			"Repository name on GitHub:",
			"FarGit-github-repo-name",
			"Create GitHub Repo",
			suggestedName);
		if (string.IsNullOrWhiteSpace(repoName)) return;
		repoName = repoName.Trim();

		var visMenu = Far.Api.CreateMenu();
		visMenu.Title = "Repository visibility:";
		visMenu.Add("Public  (anyone can see it)");
		visMenu.Add("Private  (only you and collaborators)");
		if (!visMenu.Show()) return;
		var isPrivate = visMenu.Selected == 1;

		// ── Create repo, wire up remote, offer to push ────────────────────────
		try
		{
			var cloneUrl = Commands.GitHubApi.CreateRepo(pat, repoName, isPrivate);

			using (var repo = new Repository(gitDir))
				repo.Network.Remotes.Add("origin", cloneUrl);

			if (0 == Far.Api.Message(
				$"'{repoName}' created on GitHub!\n\n{cloneUrl}\n\nPush local commits now?",
				"GitHub Repo Created", (MessageOptions)0, ["Push now", "Later"]))
			{
				var output = Commands.RemoteOps.Push(gitDir, "origin");
				Info($"Push complete!\n\n{output}");
			}

			panel.Update(true);
			panel.Redraw();
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	static void ConnectViaUrl(string gitDir, Panels.DashboardPanel panel)
	{
		var remoteName = Far.Api.Input(
			"Remote name  (use 'origin' if this is your main server):",
			"FarGit-remote-name",
			"Connect to Remote");
		if (string.IsNullOrWhiteSpace(remoteName)) return;
		remoteName = remoteName.Trim();

		var url = Far.Api.Input(
			$"URL for '{remoteName}':\n(e.g. https://github.com/you/your-repo.git)",
			"FarGit-remote-url",
			"Connect to Remote");
		if (string.IsNullOrWhiteSpace(url)) return;
		url = url.Trim();

		try
		{
			using (var repo = new Repository(gitDir))
				repo.Network.Remotes.Add(remoteName, url);

			if (0 == Far.Api.Message(
				$"Remote '{remoteName}' connected!\n\n{url}\n\n" +
				"Push local commits now?\n" +
				"(If credentials are needed, git will prompt you.)",
				"Connect to Remote", (MessageOptions)0, ["Push now", "Later"]))
			{
				var output = Commands.RemoteOps.Push(gitDir, remoteName);
				Info($"Push complete!\n\n{output}");
			}

			panel.Update(true);
			panel.Redraw();
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}
}
