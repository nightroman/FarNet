using FarNet;
using GitKit.About;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitKit.Panels;

class CommitsExplorer(string gitDir, string? branchName, string? itemPath) : BaseExplorer(gitDir, MyTypeId)
{
	public static Guid MyTypeId = new("80354846-50a0-4675-a418-e177f6747d30");

	public string? BranchName => branchName;
	public string? ItemPath => itemPath;

	public override Panel CreatePanel()
	{
		return new CommitsPanel(this);
	}

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		if (BranchName is { })
			return GetFilesByBranchName(GitDir, BranchName, args);

		if (ItemPath is { })
			return GetFilesByItemPath(GitDir, ItemPath, args);

		throw null!;
	}

	public override Explorer? ExploreDirectory(ExploreDirectoryEventArgs args)
	{
		using var repo = new Repository(GitDir);

		var file = (CommitFile)args.File;
		var newCommitSha = file.CommitSha;
		var newCommit = repo.Lookup<Commit>(newCommitSha);

		//! null for the first commit
		var oldCommit = newCommit.Parents.FirstOrDefault();

		return new ChangesExplorer(GitDir, new()
		{
			Kind = ChangesExplorer.Kind.CommitsRange,
			NewCommitSha = newCommitSha,
			OldCommitSha = oldCommit?.Sha,
			IsSingleCommit = true,
			ItemPath = ItemPath
		});
	}

	static CommitFile CreateFile(Commit commit, string? mark, int shaPrefixLength)
	{
		var commitSha = commit.Sha;
		return new CommitFile(
			$"{commitSha[..shaPrefixLength]} {commit.Author.When:yyyy-MM-dd} {commit.Author.Name}: {commit.MessageShort}",
			mark,
			commit.Author.When.DateTime,
			commitSha);
	}

	static IEnumerable<FarFile> GetFilesByBranchName(string gitDir, string branchName, GetFilesEventArgs args)
	{
		using var repo = new Repository(gitDir);

		// init panel
		if (args.Panel is { } panel && panel.Title is null)
			panel.Title = $"{branchName} branch {repo.Info.WorkingDirectory}";

		// branch may be null in a new repo
		var branch = repo.MyBranch(branchName);
		if (branch is null)
			yield break;

		IEnumerable<Commit> commits = branch.Commits;
		if (args.Limit > 0)
			commits = commits.Skip(args.Offset).Take(args.Limit);

		string? mark = null;
		Func<Commit, bool>? hasCommitMark = null;
		if (!branch.IsRemote && args.Offset == 0)
		{
			if (branch.TrackedBranch?.Tip is null)
			{
				var heads = repo.Refs.Where(x => x.IsLocalBranch && x.CanonicalName != branch.CanonicalName).ToList();
				if (heads.Count > 0)
				{
					mark = "#";
					hasCommitMark = commit => repo.Refs.ReachableFrom(heads, [commit]).Any();
				}
			}
			else
			{
				mark = "=";
				var trackedTip = branch.TrackedBranch.Tip;
				hasCommitMark = commit => commit == trackedTip;
			}
		}

		var settings = Settings.Default.GetData();
		foreach (var commit in commits)
		{
			string? commitMark = null;
			if (hasCommitMark is not null)
			{
				if (hasCommitMark(commit))
				{
					commitMark = mark;
					hasCommitMark = null;
				}
			}

			yield return CreateFile(commit, commitMark, settings.ShaPrefixLength);
		}
	}

	static IEnumerable<FarFile> GetFilesByItemPath(string gitDir, string itemPath, GetFilesEventArgs args)
	{
		using var repo = new Repository(gitDir);

		// init panel
		if (args.Panel is { } panel && panel.Title is null)
			panel.Title = $"{Path.GetFileName(itemPath)} {repo.Info.WorkingDirectory}";

		//! FirstParentOnly=true avoids missing key exceptions and broken GetFiles in some cases (Colorer-schemes) but fails in others.
		//! Use topological sort, it works in so far known cases. https://github.com/libgit2/libgit2sharp/issues/1520
		var filter = new CommitFilter { SortBy = CommitSortStrategies.Topological };

		IEnumerable<LogEntry> logs = repo.Commits.QueryBy(itemPath, filter);
		if (args.Limit > 0)
			logs = logs.Skip(args.Offset).Take(args.Limit);

		var settings = Settings.Default.GetData();
		foreach (var log in logs)
		{
			var mark = log.Path == itemPath ? null : "n";
			yield return CreateFile(log.Commit, mark, settings.ShaPrefixLength);
		}
	}
}
