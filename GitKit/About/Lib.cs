using FarNet;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitKit.About;

public static class Lib
{
	public static string GetGitRoot(string path)
	{
		return Repository.Discover(path) ?? throw new ModuleException($"Not a git repository: {path}");
	}

	/// <summary>
	/// Gets (no branch) as <see cref="Repository.Head"/> or by name from <see cref="Repository.Branches"/>.
	/// </summary>
	public static Branch MyBranch(this Repository repo, string branchName)
	{
		return branchName == Const.NoBranchName ? repo.Head : repo.Branches[branchName];
	}

	public static Signature BuildSignature(Repository repo)
	{
		return repo.Config.BuildSignature(DateTimeOffset.UtcNow);
	}

	public static IEnumerable<Branch> GetBranchesContainingCommit(Repository repo, Commit commit)
	{
		var heads = repo.Refs;
		var headsContainingCommit = repo.Refs.ReachableFrom(heads, [commit]);
		return headsContainingCommit
			.Select(branchRef => repo.Branches[branchRef.CanonicalName]);
	}

	public static Commit GetExistingTip(Repository repo)
	{
		return repo.Head.Tip ?? throw new ModuleException("The repository has no commits.");
	}

	public static TreeChanges GetChanges(Repository repo)
	{
		return CompareTree(repo, repo.Head.Tip?.Tree, DiffTargets.Index | DiffTargets.WorkingDirectory);
	}

	public static TreeChanges CompareTree(Repository repo, Tree? oldTree, DiffTargets diffTargets)
	{
		return repo.Diff.Compare<TreeChanges>(oldTree, diffTargets);
	}

	public static TreeChanges CompareTrees(Repository repo, Tree? oldTree, Tree? newTree)
	{
		return repo.Diff.Compare<TreeChanges>(oldTree, newTree);
	}

	public static string ResolveRepositoryItemPath(Repository repo, string path)
	{
		var info = repo.Info;
		if (path == ".git")
		{
			path = info.Path;
		}
		else if (path.StartsWith(".git/") || path.StartsWith(@".git\"))
		{
			path = path[5..].TrimStart('\\').TrimStart('/');
			path = Path.Combine(info.Path, path);
		}
		else
		{
			path = path.TrimStart('\\').TrimStart('/');
			path = Path.Combine(info.WorkingDirectory ?? info.Path, path);
		}
		return Path.GetFullPath(Path.TrimEndingDirectorySeparator(path));
	}
}
