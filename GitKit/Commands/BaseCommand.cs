using FarNet;
using GitKit.Extras;
using LibGit2Sharp;
using System;
using System.IO;

namespace GitKit.Commands;

abstract class BaseCommand : AnyCommand
{
	protected RepositoryReference Reference { get; }
	protected Repository Repository { get; }

	protected BaseCommand(CommandParameters parameters)
	{
		Reference = RepositoryReference.GetReference(parameters.GetPathOrCurrentDirectory(Param.Repo));
		Repository = Reference.Instance;
	}

	protected BaseCommand(string path)
	{
		Reference = RepositoryReference.GetReference(path);
		Repository = Reference.Instance;
	}

	protected override void Dispose(bool disposing)
	{
		Reference.Dispose();
	}

	protected string? GetGitPathOrPath(
		CommandParameters parameters,
		Func<string?, string?> validate,
		bool returnNullIfRoot = false)
	{
		var path = parameters.GetString(Param.Path, ParameterOptions.ExpandVariables);
		var isGitPath = parameters.GetBool(Param.IsGitPath);
		if (isGitPath)
			return path;

		path = validate(path);
		if (path is null)
			return null;

		path = Far.Api.FS.GetFullPath(path);

		//! LibGit2 gets it with trailing backslash
		var workdir = Repository.Info.WorkingDirectory;

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
