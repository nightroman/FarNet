using FarNet;
using GitKit.About;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GitKit.Panels;

class ChangesExplorer : BaseExplorer
{
	public static Guid MyTypeId = new("7b4c229a-949e-4100-856e-45c17d516d25");
	readonly Options _op;

	internal class Options
	{
		public Kind Kind;
		public string? NewCommitSha;
		public string? OldCommitSha;
		public bool IsSingleCommit;
		public string? ItemPath;
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

	public ChangesExplorer(string gitDir, Options op) : base(gitDir, MyTypeId)
	{
		_op = op;
		CanGetContent = true;
	}

	public override Panel CreatePanel()
	{
		return new ChangesPanel(this);
	}

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		using var repo = new Repository(GitDir);

		// init panel
		ChangesPanel? panel = null;
		if (args.Panel is ChangesPanel test && test.Title is null)
		{
			panel = test;
			panel.PostName(_op.ItemPath);
			panel.GitWork = repo.Info.WorkingDirectory;
		}

		TreeChanges changes;
		switch (_op.Kind)
		{
			case Kind.NotCommitted:
				{
					changes = Lib.GetChanges(repo);

					if (panel is { })
						panel.Title = $"Not committed changes {panel.GitWork}";
				}
				break;

			case Kind.NotStaged:
				{
					changes = repo.Diff.Compare<TreeChanges>();

					if (panel is { })
						panel.Title = $"Not staged changes {panel.GitWork}";
				}
				break;

			case Kind.Staged:
				{
					changes = repo.Diff.Compare<TreeChanges>(repo.Head.Tip?.Tree, DiffTargets.Index);

					if (panel is { })
						panel.Title = $"Staged changes {panel.GitWork}";
				}
				break;

			case Kind.Head:
				{
					var tip = repo.Head.Tip;
					changes = Lib.CompareTrees(repo, tip?.Parents.FirstOrDefault()?.Tree, tip?.Tree);

					if (panel is { })
						panel.Title = $"Head commit: {tip?.MessageShort}";
				}
				break;

			case Kind.Last:
				{
					changes = Lib.GetChanges(repo);

					if (changes.Count > 0)
					{
						if (panel is { })
							panel.Title = $"Last not committed {panel.GitWork}";
					}
					else
					{
						var tip = repo.Head.Tip;
						changes = Lib.CompareTrees(repo, tip?.Parents.FirstOrDefault()?.Tree, tip?.Tree);

						if (panel is { })
							panel.Title = $"Last commit: {tip?.MessageShort}";
					}
				}
				break;

			case Kind.CommitsRange:
				{
					var newCommit = _op.NewCommitSha is null ? null : repo.Lookup<Commit>(_op.NewCommitSha);
					var oldCommit = _op.OldCommitSha is null ? null : repo.Lookup<Commit>(_op.OldCommitSha);

					changes = Lib.CompareTrees(repo, oldCommit?.Tree, newCommit?.Tree);

					if (panel is { })
					{
						var settings = Settings.Default.GetData();
						var newId = _op.NewCommitSha is null ? "?" : _op.NewCommitSha[0..settings.ShaPrefixLength];
						if (_op.IsSingleCommit)
						{
							panel.Title = $"{newId}: {newCommit?.MessageShort}";
						}
						else
						{
							var oldId = _op.OldCommitSha is null ? "?" : _op.OldCommitSha[0..settings.ShaPrefixLength];
							panel.Title = $"{newId}/{oldId} {panel.GitWork}";
						}
					}
				}
				break;

			default:
				throw null!;
		}

		//! Used to set renamed Name = new << old. This breaks `PostName`.
		//! Keep Name, it is useful as is. Use [CtrlA] to see old names.
		foreach (var change in changes)
		{
			yield return new ChangeFile(change.Path, change.Status.ToString(), change);
		}
	}

	public override void GetContent(GetContentEventArgs args)
	{
		using var repo = new Repository(GitDir);

		var compareOptions = new CompareOptions { ContextLines = 3 };

		var file = (ChangeFile)args.File;
		var change = file.Change;
		var newBlob = repo.Lookup<Blob>(change.Oid);

		string text;
		if (newBlob is not null || change.Mode == Mode.Nonexistent)
		{
			// changed committed files (new blob) or deleted files (old blob, no new blob)
			var oldBlob = repo.Lookup<Blob>(change.OldOid);
			var diff = repo.Diff.Compare(oldBlob, newBlob, compareOptions);
			text = diff.Patch;
		}
		else
		{
			// other files including changed not committed (no new blob)
			var patch = repo.Diff.Compare<Patch>([change.Path], true, null, compareOptions);
			text = patch.Content;
		}

		args.CanSet = false;
		args.UseText = text;
		args.UseFileExtension = "diff";
	}
}
