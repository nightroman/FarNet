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
		Title = "Changes";
		SortMode = PanelSortMode.Unsorted;
		ViewMode = 0;

		var cn = new SetColumn { Kind = "N", Name = "Path" };
		var cd = new SetColumn { Kind = "Z", Name = "Status", Width = 10 };

		var plan0 = new PanelPlan { Columns = new FarColumn[] { cd, cn } };
		SetPlan(0, plan0);
	}

	protected override string HelpTopic => "changes-panel";

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
		var merge = Environment.GetEnvironmentVariable("MERGE");
		if (merge is null)
			throw new ModuleException("Expected environment variable `MERGE`.");

		var (file1, kill1) = GetBlobFile(changes.OldOid, changes.OldPath, changes.OldExists);
		var (file2, kill2) = GetBlobFile(changes.Oid, changes.Path, changes.Exists);

		var process = Process.Start(merge, $"\"{file1}\" \"{file2}\"");
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
