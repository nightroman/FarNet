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
	Panel? _panel;

	internal class Options
	{
		public Kind Kind;
		public string? NewCommitSha;
		public string? OldCommitSha;
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

	public ChangesExplorer(string gitRoot, Options op) : base(gitRoot, MyTypeId)
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
		using var repo = new Repository(GitRoot);

		TreeChanges changes;
		string title;
		switch (_op.Kind)
		{
			case Kind.NotCommitted:
				{
					changes = Lib.GetChanges(repo);
					title = $"Not committed changes {repo.Info.WorkingDirectory}";
				}
				break;

			case Kind.NotStaged:
				{
					changes = repo.Diff.Compare<TreeChanges>();
					title = $"Not staged changes {repo.Info.WorkingDirectory}";
				}
				break;

			case Kind.Staged:
				{
					changes = repo.Diff.Compare<TreeChanges>(repo.Head.Tip?.Tree, DiffTargets.Index);
					title = $"Staged changes {repo.Info.WorkingDirectory}";
				}
				break;

			case Kind.Head:
				{
					var tip = repo.Head.Tip;
					changes = Lib.CompareTrees(repo, tip?.Parents.FirstOrDefault()?.Tree, tip?.Tree);
					title = $"Head commit: {tip?.MessageShort}";
				}
				break;

			case Kind.Last:
				{
					changes = Lib.GetChanges(repo);
					if (changes.Count > 0)
					{
						title = $"Last not committed {repo.Info.WorkingDirectory}";
					}
					else
					{
						var tip = repo.Head.Tip;
						changes = Lib.CompareTrees(repo, tip?.Parents.FirstOrDefault()?.Tree, tip?.Tree);
						title = $"Last commit: {tip?.MessageShort}";
					}
				}
				break;

			case Kind.CommitsRange:
				{
					var newCommit = _op.NewCommitSha is null ? null : repo.Lookup<Commit>(_op.NewCommitSha);
					var oldCommit = _op.OldCommitSha is null ? null : repo.Lookup<Commit>(_op.OldCommitSha);

					changes = Lib.CompareTrees(repo, oldCommit?.Tree, newCommit?.Tree);
					var settings = Settings.Default.GetData();
					var newId = _op.NewCommitSha is null ? "?" : _op.NewCommitSha[0..settings.ShaPrefixLength];
					if (_op.IsSingleCommit)
					{
						title = $"{newId}: {newCommit?.MessageShort}";
					}
					else
					{
						var oldId = _op.OldCommitSha is null ? "?" : _op.OldCommitSha[0..settings.ShaPrefixLength];
						title = $"{newId}/{oldId} {repo.Info.WorkingDirectory}";
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
			yield return new ChangeFile(change.Path, change.Status.ToString(), change);
		}
	}

	public override void GetContent(GetContentEventArgs args)
	{
		using var repo = new Repository(GitRoot);

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
