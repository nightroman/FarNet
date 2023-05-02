using FarNet;
using LibGit2Sharp;
using System;
using System.Data.Common;
using System.IO;

namespace GitKit;

abstract class BaseCommand : AnyCommand
{
	protected RepositoryReference Reference { get; }
	protected Repository Repository { get; }

	protected BaseCommand(DbConnectionStringBuilder parameters)
	{
		Reference = RepositoryReference.GetReference(Host.GetFullPath(parameters.GetString(Parameter.Repo, true)));
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
		DbConnectionStringBuilder parameters,
		Func<string?, string?> validate,
		bool returnNullIfRoot = false)
	{
		var path = parameters.GetString(Parameter.Path, true);
		var isGitPath = parameters.GetBool(Parameter.IsGitPath);
		if (isGitPath)
			return path;

		path = validate(path);
		if (path is null)
			return null;

		if (!Path.IsPathRooted(path))
			path = Path.Combine(Far.Api.CurrentDirectory, path);

		// normalize
		path = Path.GetFullPath(path);

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
