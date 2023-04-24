using FarNet;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GitKit;

class ChangesExplorer : BaseExplorer
{
	public static Guid MyTypeId = new("7b4c229a-949e-4100-856e-45c17d516d25");
	readonly Kind? _kind;
	readonly Commit? _oldCommit;
	readonly Commit? _newCommit;
	Panel? _panel;

	public enum Kind
	{
		NotCommitted,
		NotStaged,
		Staged,
		Head,
		Last,
	}

	public ChangesExplorer(Repository repository, Kind kind) : this(repository, kind, null, null)
	{
	}

	public ChangesExplorer(Repository repository, Commit? oldCommit, Commit? newCommit) : this(repository, null, oldCommit, newCommit)
	{
	}

	ChangesExplorer(Repository repository, Kind? kind, Commit? oldCommit, Commit? newCommit) : base(repository, MyTypeId)
	{
		_kind = kind;
		_oldCommit = oldCommit;
		_newCommit = newCommit;

		CanGetContent = true;
	}

	public override Panel CreatePanel()
	{
		return new ChangesPanel(this);
	}

	public override void EnterPanel(Panel panel)
	{
		_panel = panel;
		base.EnterPanel(panel);
	}

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		TreeChanges changes;
		string title;
		switch (_kind)
		{
			case Kind.NotCommitted:
				{
					changes = Lib.GetChanges(Repository);
					title = "Not committed changes";
				}
				break;

			case Kind.NotStaged:
				{
					changes = Repository.Diff.Compare<TreeChanges>();
					title = "Not staged changes";
				}
				break;

			case Kind.Staged:
				{
					changes = Repository.Diff.Compare<TreeChanges>(Repository.Head.Tip?.Tree, DiffTargets.Index);
					title = "Staged changes";
				}
				break;

			case Kind.Head:
				{
					var tip = Repository.Head.Tip;
					changes = Lib.CompareTrees(Repository, tip?.Parents.FirstOrDefault()?.Tree, tip?.Tree);
					title = "Head commit";
				}
				break;

			case Kind.Last:
				{
					changes = Lib.GetChanges(Repository);
					if (changes.Count > 0)
					{
						title = "Last not committed";
					}
					else
					{
						var tip = Repository.Head.Tip;
						changes = Lib.CompareTrees(Repository, tip?.Parents.FirstOrDefault()?.Tree, tip?.Tree);
						title = "Last committed";
					}
				}
				break;

			default:
				{
					changes = Lib.CompareTrees(Repository, _oldCommit?.Tree, _newCommit?.Tree);

					var settings = Settings.Default.GetData();
					var oldId = _oldCommit is null ? "?" : _oldCommit.Sha[0..settings.ShaPrefixLength];
					var newId = _newCommit is null ? "?" : _newCommit.Sha[0..settings.ShaPrefixLength];
					title = $"{oldId} {newId}";
				}
				break;
		}

		if (_panel is not null)
			_panel.Title = $"{title} {Repository.Info.WorkingDirectory}";

		foreach (var change in changes)
		{
			var file = new SetFile
			{
				Description = change.Status.ToString(),
				Data = change,
			};

			if (change.Status == ChangeKind.Renamed)
				file.Name = $"{change.Path} << {change.OldPath}";
			else
				file.Name = change.Path;

			yield return file;
		}
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
