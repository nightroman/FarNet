using FarNet;
using LibGit2Sharp;

namespace GitKit.Commands;

abstract class BaseCommand : AbcCommand
{
	protected string GitDir { get; }

	protected BaseCommand(CommandParameters parameters)
	{
		try { GitDir = Lib.GetGitDir(parameters.GetPathOrCurrentDirectory(ParamRepo)); }
		catch (Exception ex) { throw parameters.ParameterError(ParamRepo, ex.Message); }
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

		path = Far.Api.GetFullPath(path);

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
