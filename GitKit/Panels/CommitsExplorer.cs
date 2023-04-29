using FarNet;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GitKit;

class CommitsExplorer : BaseExplorer
{
	public static Guid MyTypeId = new("80354846-50a0-4675-a418-e177f6747d30");

	public ICommits Data { get; private set; }

	public CommitsExplorer(Repository repository, Branch branch) : base(repository, MyTypeId)
	{
		Data = new BranchCommits(
			repository,
			branch,
			branch.IsCurrentRepositoryHead && Repository.Info.IsHeadDetached);
	}

	public CommitsExplorer(Repository repository, string path) : base(repository, MyTypeId)
	{
		Data = new PathCommits(repository, path);
	}

	public override Panel CreatePanel()
	{
		return new CommitsPanel(this)
		{
			Title = $"{Data.Title} {Repository.Info.WorkingDirectory}"
		};
	}

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		if (Data is BranchCommits data)
		{
			//! get fresh instance, e.g. important for marks after push
			//! it may have pseudo name (no branch), case bare repo
			var branch = data.IsHead ? Repository.Head : Repository.Branches[data.Branch.CanonicalName] ?? data.Branch;
			Data = data with { Branch = branch };
		}

		return Data.GetFiles(args);
	}

	public override Explorer? ExploreDirectory(ExploreDirectoryEventArgs args)
	{
		var newCommit = (Commit)args.File.Data!;

		//! null for the first commit
		var oldCommit = newCommit.Parents.FirstOrDefault();

		return new ChangesExplorer(Repository, new ChangesExplorer.Options
		{
			Kind = ChangesExplorer.Kind.CommitsRange,
			NewCommit = newCommit,
			OldCommit = oldCommit,
			IsSingleCommit = true,
			Path = (Data as PathCommits)?.Path
		});
	}

	static SetFile CreateFile(Commit commit, int shaPrefixLength)
	{
		return new SetFile
		{
			Name = $"{commit.Sha[..shaPrefixLength]} {commit.Author.When:yyyy-MM-dd} {commit.Author.Name}: {commit.MessageShort}",
			LastWriteTime = commit.Author.When.DateTime,
			Data = commit,
			IsDirectory = true,
		};
	}

	public interface ICommits
	{
		string Title { get; }
		IEnumerable<FarFile> GetFiles(GetFilesEventArgs args);
	}

	public record BranchCommits(Repository Repository, Branch Branch, bool IsHead) : ICommits
	{
		public string Title => $"{Branch.FriendlyName} branch";

		public IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
		{
			IEnumerable<Commit> commits = Branch.Commits;
			if (args.Limit > 0)
				commits = commits.Skip(args.Offset).Take(args.Limit);

			string? mark = null;
			Func<Commit, bool>? hasCommitMark = null;
			if (!Branch.IsRemote && args.Offset == 0)
			{
				if (Branch.TrackedBranch?.Tip is null)
				{
					var heads = Repository.Refs.Where(x => x.IsLocalBranch && x.CanonicalName != Branch.CanonicalName).ToList();
					if (heads.Count > 0)
					{
						mark = "#";
						hasCommitMark = commit => Repository.Refs.ReachableFrom(heads, new[] { commit }).Any();
					}
				}
				else
				{
					mark = "=";
					var trackedTip = Branch.TrackedBranch.Tip;
					hasCommitMark = commit => commit == trackedTip;
				}
			}

			var settings = Settings.Default.GetData();
			foreach (var commit in commits)
			{
				var file = CreateFile(commit, settings.ShaPrefixLength);
				if (hasCommitMark is not null)
				{
					if (hasCommitMark(commit))
					{
						file.Owner = mark;
						hasCommitMark = null;
					}
				}

				yield return file;
			}
		}
	}

	public class PathCommits : ICommits
	{
		public Repository Repository { get; }
		public string Path { get; }

		readonly CachedEnumerable<LogEntry> _commits;

		public string Title => System.IO.Path.GetFileName(Path);

		public PathCommits(Repository repository, string path)
		{
			Repository = repository;
			Path = path;

			//! FirstParentOnly=true avoids missing key exceptions and broken GetFiles in some cases (Colorer-schemes) but fails in others.
			//! Use topological sort, it works in so far known cases. https://github.com/libgit2/libgit2sharp/issues/1520
			var filter = new CommitFilter { SortBy = CommitSortStrategies.Topological };

			_commits = new(repository.Commits.QueryBy(path, filter));
		}

		public IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
		{
			IEnumerable<LogEntry> logs = _commits;
			if (args.Limit > 0)
				logs = logs.Skip(args.Offset).Take(args.Limit);

			var settings = Settings.Default.GetData();
			foreach (var log in logs)
			{
				var file = CreateFile(log.Commit, settings.ShaPrefixLength);

				if (log.Path != Path)
					file.Owner = "n";

				yield return file;
			}
		}
	}
}
