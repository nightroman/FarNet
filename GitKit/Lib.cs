using FarNet;
using LibGit2Sharp;
using System.Collections.Generic;
using System.Linq;

namespace GitKit;

public static class Lib
{
	public static string GetGitRoot(string path)
	{
		return Repository.Discover(path) ?? throw new ModuleException($"Not a git repository: {path}");
	}

	public static IEnumerable<Branch> GetBranchesContainingCommit(Repository repo, Commit commit)
	{
		var localHeads = repo.Refs.Where(reference => reference.IsLocalBranch);

		var localHeadsContainingCommit = repo.Refs.ReachableFrom(localHeads, new[] { commit });

		return localHeadsContainingCommit
			.Select(branchRef => repo.Branches[branchRef.CanonicalName]);
	}
}
