using FarNet;
using GitKit.Extras;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GitKit.Panels;

class ChangesExplorer : BaseExplorer
{
	public static Guid MyTypeId = new("7b4c229a-949e-4100-856e-45c17d516d25");
	readonly Options _op;
	Panel? _panel;

	internal class Options
	{
		public Kind Kind;
		public Commit? NewCommit;
		public Commit? OldCommit;
		public bool IsSingleCommit;
		public string? Path;
	}

	internal enum Kind
	{
		CommitsRange,
		NotCommitted,
		NotStaged,
		Staged,
		Head,
		Last,
	}

	public ChangesExplorer(Repository repository, Options op) : base(repository, MyTypeId)
	{
		_op = op;
		CanGetContent = true;
	}

	public override Panel CreatePanel()
	{
		return new ChangesPanel(this);
	}

	public override void EnterPanel(Panel panel)
	{
		_panel = panel;

		panel.PostName(_op.Path);

		base.EnterPanel(panel);
	}

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		TreeChanges changes;
		string title;
		switch (_op.Kind)
		{
			case Kind.NotCommitted:
				{
					changes = Lib.GetChanges(Repository);
					title = $"Not committed changes {Repository.Info.WorkingDirectory}";
				}
				break;

			case Kind.NotStaged:
				{
					changes = Repository.Diff.Compare<TreeChanges>();
					title = $"Not staged changes {Repository.Info.WorkingDirectory}";
				}
				break;

			case Kind.Staged:
				{
					changes = Repository.Diff.Compare<TreeChanges>(Repository.Head.Tip?.Tree, DiffTargets.Index);
					title = $"Staged changes {Repository.Info.WorkingDirectory}";
				}
				break;

			case Kind.Head:
				{
					var tip = Repository.Head.Tip;
					changes = Lib.CompareTrees(Repository, tip?.Parents.FirstOrDefault()?.Tree, tip?.Tree);
					title = $"Head commit: {tip?.MessageShort}";
				}
				break;

			case Kind.Last:
				{
					changes = Lib.GetChanges(Repository);
					if (changes.Count > 0)
					{
						title = $"Last not committed {Repository.Info.WorkingDirectory}";
					}
					else
					{
						var tip = Repository.Head.Tip;
						changes = Lib.CompareTrees(Repository, tip?.Parents.FirstOrDefault()?.Tree, tip?.Tree);
						title = $"Last commit: {tip?.MessageShort}";
					}
				}
				break;

			case Kind.CommitsRange:
				{
					changes = Lib.CompareTrees(Repository, _op.OldCommit?.Tree, _op.NewCommit?.Tree);
					var settings = Settings.Default.GetData();
					var newId = _op.NewCommit is null ? "?" : _op.NewCommit.Sha[0..settings.ShaPrefixLength];
					if (_op.IsSingleCommit)
					{
						title = $"{newId}: {_op.NewCommit?.MessageShort}";
					}
					else
					{
						var oldId = _op.OldCommit is null ? "?" : _op.OldCommit.Sha[0..settings.ShaPrefixLength];
						title = $"{newId}/{oldId} {Repository.Info.WorkingDirectory}";
					}
				}
				break;

			default:
				throw null!;
		}

		if (_panel is not null)
			_panel.Title = title;

		//! Used to set renamed Name = new << old. This breaks `PostName`.
		//! Keep Name, it is useful as is. Use [CtrlA] to see old names.
		foreach (var change in changes)
		{
			yield return new SetFile
			{
				Name = change.Path,
				Description = change.Status.ToString(),
				Data = change,
			};
		}
	}

	public override void GetContent(GetContentEventArgs args)
	{
		var compareOptions = new CompareOptions { ContextLines = 3 };

		var changes = (TreeEntryChanges)args.File.Data!;
		var newBlob = Repository.Lookup<Blob>(changes.Oid);

		string text;
		if (newBlob is not null || changes.Mode == Mode.Nonexistent)
		{
			// changed committed files (new blob) or deleted files (old blob, no new blob)
			var oldBlob = Repository.Lookup<Blob>(changes.OldOid);
			var diff = Repository.Diff.Compare(oldBlob, newBlob, compareOptions);
			text = diff.Patch;
		}
		else
		{
			// other files including changed not committed (no new blob)
			var patch = Repository.Diff.Compare<Patch>([changes.Path], true, null, compareOptions);
			text = patch.Content;
		}

		args.CanSet = false;
		args.UseText = text;
		args.UseFileExtension = "diff";
	}
}
