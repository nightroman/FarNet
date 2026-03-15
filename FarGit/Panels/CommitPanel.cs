using FarNet;
using LibGit2Sharp;

namespace FarGit.Panels;

/// <summary>
/// Panel showing the commit timeline for the current branch.
/// Most recent commit is at the top.
/// Branch and tag labels appear on the commits they point to.
///
/// Key bindings:
///   Enter / F3  – view what changed in this commit (diff)
/// </summary>
public class CommitPanel : BasePanel
{
	public CommitPanel(CommitExplorer explorer) : base(explorer)
	{
		SortMode = PanelSortMode.Unsorted;

		var co = new SetColumn { Kind = "O", Name = "Author",  Width = 16 };
		var cd = new SetColumn { Kind = "D", Name = "Date",    Width = 12 };
		var cn = new SetColumn { Kind = "N", Name = "Commit" };
		var cz = new SetColumn { Kind = "Z", Name = "Branches / Tags", Width = 30 };

		var plan0 = new PanelPlan { Columns = [co, cd, cn, cz] };
		SetPlan(0, plan0);
		SetView(plan0);

		SetKeyBars([
			new KeyBar(KeyCode.F2, ControlKeyStates.None, "Menu",   "Panel actions"),
			new KeyBar(KeyCode.F3, ControlKeyStates.None, "Diff",   "View changes introduced by this commit"),
			new KeyBar(KeyCode.F9, ControlKeyStates.None, "Revert", "Create a new commit that undoes this one"),
		]);
	}

	protected override string HelpTopic => "history-panel";

	// ── Show diff ─────────────────────────────────────────────────────────

	void ShowCommitDiff()
	{
		if (CurrentFile is not CommitFile f) return;

		try
		{
			using var repo = UseRepository();
			var commit = repo.Lookup<Commit>(f.Sha);
			if (commit is null)
			{
				Far.Api.Message($"Commit {f.Sha[..7]} not found.", Const.ModuleName, MessageOptions.Warning);
				return;
			}

			Patch patch;
			if (commit.Parents.Any())
				patch = repo.Diff.Compare<Patch>(commit.Parents.First().Tree, commit.Tree);
			else
				patch = repo.Diff.Compare<Patch>(null, commit.Tree); // root commit

			var sb = new System.Text.StringBuilder();
			sb.AppendLine($"commit {commit.Sha}");
			if (!string.IsNullOrEmpty(f.Labels))
				sb.AppendLine($"Labels: {f.Labels}");
			sb.AppendLine($"Author: {commit.Author.Name} <{commit.Author.Email}>");
			sb.AppendLine($"Date:   {commit.Author.When:yyyy-MM-dd HH:mm:ss zzz}");
			sb.AppendLine();

			// Full commit message, indented 4 spaces per git convention
			foreach (var line in commit.Message.TrimEnd().Split('\n'))
				sb.AppendLine($"    {line.TrimEnd()}");

			var diffContent = patch.Content.Replace("\uFEFF", string.Empty);
			if (!string.IsNullOrWhiteSpace(diffContent))
			{
				sb.AppendLine();
				sb.AppendLine(new string('─', 72));
				sb.Append(diffContent);
			}
			else
			{
				sb.AppendLine();
				sb.AppendLine("    (no file changes in this commit)");
			}

			var tmp = Path.ChangeExtension(Path.GetTempFileName(), ".diff");
			File.WriteAllText(tmp, sb.ToString(), System.Text.Encoding.UTF8);

			var viewer = Far.Api.CreateViewer();
			viewer.FileName = tmp;
			viewer.Title = $"{commit.Sha[..7]}  {commit.MessageShort}";
			viewer.DeleteSource = DeleteSource.File;
			viewer.Open();
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// ── Create branch from commit ─────────────────────────────────────────

	void CreateBranchHere()
	{
		if (CurrentFile is not CommitFile f) return;

		var sha7 = f.Sha[..7];
		var name = Far.Api.Input(
			$"New branch name (starting at commit {sha7}):",
			"FarGit-branch",
			"Create Branch from Commit");
		if (string.IsNullOrWhiteSpace(name)) return;
		name = name.Trim();

		try
		{
			using var repo = UseRepository();
			var commit = repo.Lookup<Commit>(f.Sha)
				?? throw new Exception($"Commit {sha7} not found.");
			repo.CreateBranch(name, commit);
			Update(true); Redraw();
			Far.Api.Message($"Branch '{name}' created at {sha7}.", Const.ModuleName);
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// ── Tag this commit ───────────────────────────────────────────────────

	void TagHere()
	{
		if (CurrentFile is not CommitFile f) return;

		var sha7 = f.Sha[..7];
		var name = Far.Api.Input(
			$"Tag name for commit {sha7}:",
			"FarGit-tag",
			"Tag Commit");
		if (string.IsNullOrWhiteSpace(name)) return;
		name = name.Trim();

		try
		{
			using var repo = UseRepository();
			repo.ApplyTag(name, f.Sha);
			Update(true); Redraw();
			Far.Api.Message($"Tag '{name}' created on {sha7}.", Const.ModuleName);
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// ── Revert ────────────────────────────────────────────────────────────

	void RevertCommit()
	{
		if (CurrentFile is not CommitFile f) return;

		var sha7 = f.Sha[..7];
		// Strip the "abc1234  " prefix that CommitFile.Name includes
		var msg  = f.Name.Length > 9 ? f.Name[9..] : f.Name;

		if (0 != Far.Api.Message(
			$"Revert commit {sha7}?\n\n" +
			$"  \"{msg}\"\n\n" +
			"This creates a NEW commit that undoes the changes\n" +
			"from the selected commit. Your history is preserved\n" +
			"and it is safe even for already-pushed commits.\n\n" +
			"If this isn't the last commit, git may ask you to\n" +
			"resolve conflicts first.",
			"Revert Commit", MessageOptions.YesNo))
			return;

		try
		{
			var output = Commands.RemoteOps.Revert(GitDir, f.Sha);
			Update(true); Redraw();
			Far.Api.Message(output, $"Reverted {sha7}");
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// ── Menu / key handling ───────────────────────────────────────────────

	internal override void AddMenu(IMenu menu)
	{
		menu.Add("&View commit diff",          (_, _) => ShowCommitDiff());
		menu.Add(string.Empty).IsSeparator = true;
		menu.Add("&Create branch from here...", (_, _) => CreateBranchHere());
		menu.Add("&Tag this commit...",          (_, _) => TagHere());
		menu.Add(string.Empty).IsSeparator = true;
		menu.Add("Re&vert this commit",          (_, _) => RevertCommit());
	}

	public override void UIOpenFile(FarFile file)
	{
		if (file is CommitFile) ShowCommitDiff();
	}

	public override bool UIKeyPressed(KeyInfo key)
	{
		switch (key.VirtualKeyCode)
		{
			case KeyCode.F3 when key.Is():
				ShowCommitDiff();
				return true;
			case KeyCode.F9 when key.Is():
				RevertCommit();
				return true;
		}
		return base.UIKeyPressed(key);
	}
}
