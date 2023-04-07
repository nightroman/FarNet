using FarNet;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GitKit;

class ChangesExplorer : BaseExplorer
{
	public static Guid MyTypeId = new("7b4c229a-949e-4100-856e-45c17d516d25");
	public IReadOnlyList<TreeEntryChanges> Changes { get; }

	public ChangesExplorer(Repository repository, TreeChanges changes) : base(repository, MyTypeId)
	{
		CanGetContent = true;

		Changes = changes.ToList();
	}

	public override Panel CreatePanel()
	{
		return new ChangesPanel(this);
	}

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		return Changes
			.Select(x => new SetFile
			{
				Name = x.Path,
				Description = x.Status.ToString(),
				Data = x,
			});
	}

	public override void GetContent(GetContentEventArgs args)
	{
		var compareOptions = new CompareOptions { ContextLines = 3 };

		var changes = (TreeEntryChanges)args.File.Data!;
		var newBlob = Repository.Lookup<Blob>(changes.Oid);

		string text;
		if (newBlob is null)
		{
			var patch = Repository.Diff.Compare<Patch>(new string[] { changes.Path }, true, null, compareOptions);
			text = patch.Content;
		}
		else
		{
			var oldBlob = Repository.Lookup<Blob>(changes.OldOid);
			var diff = Repository.Diff.Compare(oldBlob, newBlob, compareOptions);
			text = diff.Patch;
		}

		args.CanSet = false;
		args.UseText = text;
		args.UseFileExtension = "diff";
	}
}
