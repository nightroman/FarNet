
using FarNet;
using GitKit.About;
using LibGit2Sharp;
using System.Diagnostics;

namespace GitKit.Panels;

class ChangesPanel : BasePanel<ChangesExplorer>
{
	/// <summary>
	/// Set on getting files, null for bare repo.
	/// </summary>
	internal string? GitWork { get; set; }

	public ChangesPanel(ChangesExplorer explorer) : base(explorer)
	{
		SortMode = PanelSortMode.Unsorted;

		var cn = new SetColumn { Kind = "N", Name = "Path" };
		var cd = new SetColumn { Kind = "Z", Name = "Status", Width = 10 };

		var plan0 = new PanelPlan { Columns = [cd, cn] };
		SetPlan(0, plan0);

		SetView(plan0);
	}

	protected override string HelpTopic => "changes-panel";

	void EditChangeFile()
	{
		if (CurrentFile is not ChangeFile file || !file.Change.Exists || GitWork is null)
			return;

		var editor = Far.Api.CreateEditor();
		editor.FileName = Path.Join(GitWork, file.Change.Path);
		editor.Open();
	}

	void OpenCommitLog()
	{
		if (CurrentFile is not ChangeFile file)
			return;

		var change = file.Change;
		var path = change.Exists ? change.Path : change.OldExists ? change.OldPath : null;
		if (path is null)
			return;

		new CommitsExplorer(GitDir, null, path).CreatePanel().OpenChild(this);
	}

	(string, bool) GetBlobFile(ObjectId oid, string path, bool exists, int shaPrefixLength, bool useCurrent)
	{
		if (!exists)
			return (string.Empty, false);

		using var repo = new Repository(GitDir);

		var blob = repo.Lookup<Blob>(oid);
		if (blob is null || useCurrent)
		{
			if (GitWork is null)
				return (string.Empty, false);

			var file = Path.Join(GitWork, path);
			if (File.Exists(file))
				return (file, false);
			else
				return (string.Empty, false);
		}
		else
		{
			var name = Path.GetFileNameWithoutExtension(path) + '.' + oid.Sha[0..shaPrefixLength] + Path.GetExtension(path);
			var file = Path.Join(Path.GetTempPath(), name);
			using var stream = File.OpenWrite(file);
			blob.GetContentStream().CopyTo(stream);
			return (file, true);
		}
	}

	void ShowDiff(TreeEntryChanges changes)
	{
		var settings = Settings.Default.GetData();
		var diffTool = Environment.ExpandEnvironmentVariables(settings.DiffTool);
		var diffToolArguments = Environment.ExpandEnvironmentVariables(settings.DiffToolArguments);
		if (string.IsNullOrEmpty(diffTool) || string.IsNullOrEmpty(diffToolArguments))
			throw new ModuleException($"Please define settings '{nameof(settings.DiffTool)}' and '{nameof(settings.DiffToolArguments)}'.");

		var (file1, kill1) = GetBlobFile(changes.OldOid, changes.OldPath, changes.OldExists, settings.ShaPrefixLength, false);
		var (file2, kill2) = GetBlobFile(changes.Oid, changes.Path, changes.Exists, settings.ShaPrefixLength, MyExplorer.IsLast);

		diffToolArguments = diffToolArguments.Replace("%1", file1).Replace("%2", file2);
		var process = Process.Start(diffTool, diffToolArguments);

		if (kill1 || kill2)
		{
			Task.Run(async () =>
			{
				await process.WaitForExitAsync();
				await Task.Delay(2000);
				if (kill1)
					File.Delete(file1);
				if (kill2)
					File.Delete(file2);
			});
		}
	}

	internal override void AddMenu(IMenu menu)
	{
		menu.Add(Const.CommitLog, (s, e) => OpenCommitLog());
		menu.Add(Const.EditFile, (s, e) => EditChangeFile());
	}

	public override void UIOpenFile(FarFile file)
	{
		var change = ((ChangeFile)file).Change;
		if (change.Mode == Mode.NonExecutableFile || change.Mode == Mode.Nonexistent && change.OldMode == Mode.NonExecutableFile)
			ShowDiff(change);
		else
			Far.Api.Message($"Cannot show diff {change.OldMode} -> {change.Mode}", Host.MyName);
	}

	public override bool UIKeyPressed(KeyInfo key)
	{
		switch (key.VirtualKeyCode)
		{
			case KeyCode.F4 when key.IsAlt():
				EditChangeFile();
				return true;
		}

		return base.UIKeyPressed(key);
	}
}
