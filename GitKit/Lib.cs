using FarNet;
using LibGit2Sharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitKit;

public static class Lib
{
	static string? TryGitRoot(string? path)
	{
		if (string.IsNullOrEmpty(path))
			return null;

		var git = path + "/.git";
		if (Directory.Exists(git) || File.Exists(git))
			return path;

		return TryGitRoot(Path.GetDirectoryName(path));
	}

	public static string GetGitRoot(string path)
	{
		return TryGitRoot(path) ?? throw new ModuleException($"Not a git repository: {path}");
	}

	public static IEnumerable<Branch> GetBranchesContainingCommit(Repository repo, Commit commit)
	{
		var localHeads = repo.Refs.Where(reference => reference.IsLocalBranch);

		var localHeadsContainingCommit = repo.Refs.ReachableFrom(localHeads, new[] { commit });

		return localHeadsContainingCommit
			.Select(branchRef => repo.Branches[branchRef.CanonicalName]);
	}
}
