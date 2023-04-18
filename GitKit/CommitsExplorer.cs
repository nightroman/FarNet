using FarNet;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GitKit;

class CommitsExplorer : BaseExplorer
{
	public static Guid MyTypeId = new("80354846-50a0-4675-a418-e177f6747d30");
	public Branch Branch { get; }

	public CommitsExplorer(Repository repository, Branch branch) : base(repository, MyTypeId)
	{
		Branch = branch;
	}

	public override Panel CreatePanel()
	{
		return new CommitsPanel(this);
	}

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		IEnumerable<Commit> commits = Branch.Commits;

		if (args.Limit > 0)
			commits = commits.Skip(args.Offset).Take(args.Limit);

		Func<Commit, bool>? hasCommitMark = null;
		if (!Branch.IsRemote && args.Offset == 0)
		{
			if (Branch.TrackedBranch?.Tip is null)
			{
				var heads = Repository.Refs.Where(x => x.IsLocalBranch && x.CanonicalName != Branch.CanonicalName).ToList();
				if (heads.Count > 0)
					hasCommitMark = commit => Repository.Refs.ReachableFrom(heads, new[] { commit }).Any();
			}
			else
			{
				var trackedTip = Branch.TrackedBranch.Tip;
				hasCommitMark = commit => commit == trackedTip;
			}
		}

		var settings = Settings.Default.GetData();
		foreach (var commit in commits)
		{
			var file = new SetFile
			{
				Name = $"{commit.Sha[..settings.ShaPrefixLength]} {commit.Author.When:yyyy-MM-dd} {commit.Author.Name}: {commit.MessageShort}",
				LastWriteTime = commit.Author.When.DateTime,
				IsDirectory = true,
				Data = commit,
			};

			if (hasCommitMark is not null)
			{
				if (hasCommitMark(commit))
				{
					file.Owner = "=";
					hasCommitMark = null;
				}
			}

			yield return file;
		}
	}

	public override Explorer? ExploreDirectory(ExploreDirectoryEventArgs args)
	{
		var commit = (Commit)args.File.Data!;
		var tree1 = commit.Tree;

		//! null for the first commit
		var tree2 = commit.Parents.FirstOrDefault()?.Tree;

		var diff = Repository.Diff.Compare<TreeChanges>(tree2, tree1);
		return new ChangesExplorer(Repository, () => diff);
	}
}
