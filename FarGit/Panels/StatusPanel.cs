using FarNet;
using LibGit2Sharp;

namespace FarGit.Panels;

/// <summary>
/// Panel showing the git working-tree status, grouped into three sections:
///   Staged — indexed changes ready to commit
///   Unstaged — working-tree changes not yet staged
///   Untracked — new files not yet tracked
///
/// Key bindings (shown in the key bar at the bottom):
///   Enter / F3   – view diff patch for the current file
///   F4           – open file in editor
///   F5           – stage current / selected files
///   F6           – unstage current / selected files
///   F7           – open commit dialog
///   F8           – revert changes (with confirmation)
///   ShiftF7      – amend last commit
///   Space        – mark/unmark files for batch stage/unstage
/// </summary>
public class StatusPanel : BasePanel
{
	public StatusPanel(StatusExplorer explorer) : base(explorer)
	{
		SortMode = PanelSortMode.Unsorted;

		var co = new SetColumn { Kind = "O", Name = "  ", Width = 2 };
		var cd = new SetColumn { Kind = "Z", Name = "Status", Width = 10 };
		var cn = new SetColumn { Kind = "N", Name = "File" };

		var plan0 = new PanelPlan { Columns = [co, cd, cn] };
		SetPlan(0, plan0);
		SetView(plan0);

		SetKeyBars([
			new KeyBar(KeyCode.F2, ControlKeyStates.None,         "Menu",    "Panel actions"),
			new KeyBar(KeyCode.F3, ControlKeyStates.None,         "Diff",    "View diff patch"),
			new KeyBar(KeyCode.F4, ControlKeyStates.None,         "Edit",    "Edit file in editor"),
			new KeyBar(KeyCode.F5, ControlKeyStates.None,         "Stage",   "Stage selected/current"),
			new KeyBar(KeyCode.F6, ControlKeyStates.None,         "Unstage", "Unstage selected/current"),
			new KeyBar(KeyCode.F7, ControlKeyStates.None,         "Commit",  "Commit staged changes"),
			new KeyBar(KeyCode.F8, ControlKeyStates.None,         "Revert!", "Revert changes (cannot undo)"),
			new KeyBar(KeyCode.F7, ControlKeyStates.ShiftPressed, "Amend",   "Amend last commit"),
		]);
	}

	protected override string HelpTopic => "status-panel";

	IReadOnlyList<StatusFile> GetActiveFiles()
	{
		var marked = GetSelectedFiles().OfType<StatusFile>().ToList();
		if (marked.Count > 0) return marked;
		return CurrentFile is StatusFile sf ? [sf] : [];
	}

	static void OpenPatchViewer(string content, string title)
	{
		content = content.Replace("\uFEFF", string.Empty);
		var tmp = Path.ChangeExtension(Path.GetTempFileName(), ".diff");
		File.WriteAllText(tmp, content, System.Text.Encoding.UTF8);

		var viewer = Far.Api.CreateViewer();
		viewer.FileName = tmp;
		viewer.Title = title;
		viewer.DeleteSource = DeleteSource.File;
		viewer.Open();
	}

	void ShowDiff()
	{
		if (CurrentFile is not StatusFile file) return;

		using var repo = UseRepository();

		string patch;
		if (file.IsStaged)
		{
			var changes = repo.Diff.Compare<Patch>([file.Name], false);
			patch = changes.Content;
		}
		else if (file.IsUntracked)
		{
			Far.Api.Message("No diff available for untracked files.", Const.ModuleName);
			return;
		}
		else
		{
			var changes = repo.Diff.Compare<Patch>([file.Name], true);
			patch = changes.Content;
		}

		OpenPatchViewer(patch, $"Diff: {file.Name}");
	}

	void EditFile()
	{
		if (CurrentFile is not StatusFile file) return;
		using var repo = UseRepository();
		var work = repo.Info.WorkingDirectory;
		if (work is null) return;

		var editor = Far.Api.CreateEditor();
		editor.FileName = Path.Join(work, file.Name);
		editor.Open();
	}

	void StageSelected()
	{
		var files = GetActiveFiles();
		if (files.Count == 0) return;
		using var repo = UseRepository();
		foreach (var f in files)
			LibGit2Sharp.Commands.Stage(repo, f.Name);
		Update(true);
		Redraw();
	}

	void UnstageSelected()
	{
		var files = GetActiveFiles().Where(f => f.IsStaged).ToList();
		if (files.Count == 0) return;
		using var repo = UseRepository();
		foreach (var f in files)
			LibGit2Sharp.Commands.Unstage(repo, f.Name);
		Update(true);
		Redraw();
	}

	void RevertSelected()
	{
		var files = GetActiveFiles().Where(f => !f.IsUntracked).ToList();
		if (files.Count == 0)
		{
			Far.Api.Message("Nothing to revert. Untracked files are not affected.", Const.ModuleName);
			return;
		}

		var names = string.Join("\n", files.Select(f => $"  {f.Owner}  {f.Name}"));
		if (0 != Far.Api.Message(
			$"Revert changes — this cannot be undone!\n\n{names}",
			Const.ModuleName,
			MessageOptions.YesNo | MessageOptions.Warning))
			return;

		using var repo = UseRepository();
		foreach (var f in files.Where(f => f.IsStaged))
			LibGit2Sharp.Commands.Unstage(repo, f.Name);
		repo.CheckoutPaths("HEAD", files.Select(f => f.Name), new CheckoutOptions());
		Update(true);
		Redraw();
	}

	void OpenCommit(bool amend) => Commands.Commit.Open(GitDir, amend);

	internal override void AddMenu(IMenu menu)
	{
		menu.Add(Const.DiffFile, (_, _) => ShowDiff());
		menu.Add(Const.EditFile, (_, _) => EditFile());
		menu.Add(string.Empty).IsSeparator = true;
		menu.Add(Const.StageFile, (_, _) => StageSelected());
		menu.Add(Const.UnstageFile, (_, _) => UnstageSelected());
		menu.Add(Const.StageAll, (_, _) =>
		{
			using var repo = UseRepository();
			LibGit2Sharp.Commands.Stage(repo, "*");
			Update(true); Redraw();
		});
		menu.Add(Const.UnstageAll, (_, _) =>
		{
			using var repo = UseRepository();
			LibGit2Sharp.Commands.Unstage(repo, "*");
			Update(true); Redraw();
		});
		menu.Add(string.Empty).IsSeparator = true;
		menu.Add(Const.RevertFiles, (_, _) => RevertSelected());
		menu.Add(string.Empty).IsSeparator = true;
		menu.Add(Const.MenuCommit, (_, _) => OpenCommit(false));
		menu.Add(Const.MenuAmend, (_, _) => OpenCommit(true));
	}

	public override void UIOpenFile(FarFile file)
	{
		if (file is StatusFile) ShowDiff();
	}

	public override bool UIKeyPressed(KeyInfo key)
	{
		switch (key.VirtualKeyCode)
		{
			case KeyCode.F3 when key.Is():
				ShowDiff();
				return true;
			case KeyCode.F4 when key.Is():
				EditFile();
				return true;
			case KeyCode.F5 when key.Is():
				StageSelected();
				return true;
			case KeyCode.F6 when key.Is():
				UnstageSelected();
				return true;
			case KeyCode.F7 when key.Is():
				OpenCommit(false);
				return true;
			case KeyCode.F7 when key.IsShift():
				OpenCommit(true);
				return true;
			case KeyCode.F8 when key.Is():
				RevertSelected();
				return true;
		}

		return base.UIKeyPressed(key);
	}
}
