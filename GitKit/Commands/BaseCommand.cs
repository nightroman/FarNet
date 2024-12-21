using FarNet;
using GitKit.About;
using LibGit2Sharp;
using System;

namespace GitKit.Commands;

abstract class BaseCommand : AbcCommand
{
	protected string GitRoot { get; }

	protected BaseCommand(CommandParameters parameters)
	{
		try { GitRoot = Lib.GetGitRoot(parameters.GetPathOrCurrentDirectory(Param.Repo)); }
		catch (Exception ex) { throw parameters.ParameterError(Param.Repo, ex.Message); }
	}

	protected static string? GetGitPathOrPath(
		Repository repo,
		string? path,
		bool isGitPath,
		Func<string?, string?> validate,
		bool returnNullIfRoot = false)
	{
		if (isGitPath)
			return path;

		path = validate(path);
		if (path is null)
			return null;

		path = Far.Api.FS.GetFullPath(path);

		//! LibGit2 gets it with trailing backslash
		var workdir = repo.Info.WorkingDirectory;

		if (path.StartsWith(workdir, StringComparison.OrdinalIgnoreCase))
		{
			path = path[workdir.Length..];
		}
		else if (returnNullIfRoot && path.Length == workdir.Length - 1 && workdir.StartsWith(path, StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}
		else
		{
			throw new ModuleException("Cannot resolve path: " + path);
		}

		return path.Replace('\\', '/');
	}
}
