using FarNet;
using LibGit2Sharp;
using System;
using System.Data.Common;
using System.IO;

namespace GitKit;

sealed class CommitsCommand : BaseCommand
{
	readonly string? _path;

	public CommitsCommand(DbConnectionStringBuilder parameters) : base(parameters)
	{
		_path = parameters.GetValue("Path");
	}

	public CommitsCommand() : base(Far.Api.CurrentDirectory)
	{
		_path = "?";
	}

	string? ResolvePath()
	{
		if (_path is null)
			return null;

		var path = _path;
		if (path == "?")
			path = Far.Api.FS.CursorPath ?? Far.Api.CurrentDirectory;

		if (Path.IsPathRooted(path))
		{
			//! LibGit2 gets it with trailing backslash
			var workdir = Repository.Info.WorkingDirectory;

			path = Path.GetFullPath(path);
			if (path.StartsWith(workdir, StringComparison.OrdinalIgnoreCase))
			{
				path = path[workdir.Length..];
			}
			else if (path.Length == workdir.Length - 1 && workdir.StartsWith(path, StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}
			else
			{
				throw new ModuleException("Cannot resolve path.");
			}
		}

		return path.Replace('\\', '/');
	}

	public override void Invoke()
	{
		var path = ResolvePath();
		var explorer = path is null ? new CommitsExplorer(Repository, Repository.Head) : new CommitsExplorer(Repository, path);

		explorer
			.CreatePanel()
			.Open();
	}
}
