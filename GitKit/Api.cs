using FarNet;
using GitKit.Panels;
using LibGit2Sharp;

namespace GitKit;

public static class Api
{
	public static Repository? TryRepository()
	{
		return Repository.Discover(Far.Api.CurrentDirectory) is { } dir ? new Repository(dir) : null;
	}

	public static Repository UseRepository()
	{
		return new Repository(Lib.GetGitDir(Far.Api.CurrentDirectory));
	}

	public static void InvokeRepositoryAction(Action<Repository?> action)
	{
		using var repo = TryRepository();
		action(repo);
	}

	public static BasePanel BasePanel()
	{
		return Far.Api.Panel as BasePanel ?? throw new ModuleException("Expected reposotory panel.");
	}

	public static BranchesPanel BranchesPanel()
	{
		return Far.Api.Panel as BranchesPanel ?? throw new ModuleException("Expected branches panel.");
	}

	public static ChangesPanel ChangesPanel()
	{
		return Far.Api.Panel as ChangesPanel ?? throw new ModuleException("Expected changes panel.");
	}

	public static CommitsPanel CommitsPanel()
	{
		return Far.Api.Panel as CommitsPanel ?? throw new ModuleException("Expected commits panel.");
	}
}
