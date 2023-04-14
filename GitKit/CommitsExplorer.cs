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

		var settings = Settings.Default.GetData();
		return commits
			.Select(x => new SetFile
			{
				Name = $"{x.Sha[..settings.ShaPrefixLength]} {x.Author.When:yyyy-MM-dd} {x.Author.Name}: {x.MessageShort}",
				LastWriteTime = x.Author.When.DateTime,
				IsDirectory = true,
				Data = x,
			});
	}

	public override Explorer? ExploreDirectory(ExploreDirectoryEventArgs args)
	{
		var commit = (Commit)args.File.Data!;
		var tree1 = commit.Tree;
		var tree2 = commit.Parents.First().Tree;
		var diff = Repository.Diff.Compare<TreeChanges>(tree2, tree1);
		return new ChangesExplorer(Repository, () => diff);
	}
}
