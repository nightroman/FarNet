using FarNet;
using LibGit2Sharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace GitKit;

class ChangesPanel : BasePanel<ChangesExplorer>
{
	public ChangesPanel(ChangesExplorer explorer) : base(explorer)
	{
		Title = $"Changes {Repository.Info.WorkingDirectory}";
		SortMode = PanelSortMode.Unsorted;
		ViewMode = 0;

		var cn = new SetColumn { Kind = "N", Name = "Path" };
		var cd = new SetColumn { Kind = "Z", Name = "Status", Width = 10 };

		var plan0 = new PanelPlan { Columns = new FarColumn[] { cd, cn } };
		SetPlan(0, plan0);
	}

	protected override string HelpTopic => "changes-panel";

	public void EditChangeFile()
	{
		var change = CurrentFile?.Data as TreeEntryChanges;
		if (change is null || !change.Exists || Repository.Info.WorkingDirectory is not string workdir)
			return;

		var editor = Far.Api.CreateEditor();
		editor.FileName = Path.Combine(workdir, change.Path);
		editor.Open();
	}

	(string, bool) GetBlobFile(ObjectId oid, string path, bool exists)
	{
		if (!exists)
			return (string.Empty, false);

		var blob = Repository.Lookup<Blob>(oid);
		if (blob is null)
		{
			var file = Path.Combine(Repository.Info.WorkingDirectory, path);
			if (File.Exists(file))
				return (file, false);
			else
				return (string.Empty, false);
		}
		else
		{
			var file = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(oid.Sha, Path.GetExtension(path)));
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

		var (file1, kill1) = GetBlobFile(changes.OldOid, changes.OldPath, changes.OldExists);
		var (file2, kill2) = GetBlobFile(changes.Oid, changes.Path, changes.Exists);

		diffToolArguments = diffToolArguments.Replace("%1", file1).Replace("%2", file2);
		var process = Process.Start(diffTool, diffToolArguments);
		Task.Run(async () =>
		{
			await process.WaitForExitAsync();
			if (kill1)
				File.Delete(file1);
			if (kill2)
				File.Delete(file2);
		});
	}

	public override bool UIKeyPressed(KeyInfo key)
	{
		switch (key.VirtualKeyCode)
		{
			case KeyCode.Enter when key.Is():
				var changes = (TreeEntryChanges?)CurrentFile?.Data;
				if (changes is not null)
					ShowDiff(changes);
				return true;
		}

		return base.UIKeyPressed(key);
	}
}
