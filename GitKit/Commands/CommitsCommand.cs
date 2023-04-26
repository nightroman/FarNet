using FarNet;
using LibGit2Sharp;
using System;
using System.Data.Common;
using System.IO;

namespace GitKit;

sealed class CommitsCommand : BaseCommand
{
	readonly string? _path;

	public CommitsCommand(Repository repo, DbConnectionStringBuilder parameters) : base(repo)
	{
		_path = parameters.GetValue("Path");
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
			var workdir = _repo.Info.WorkingDirectory;
			path = Path.GetFullPath(path);

			if (!path.StartsWith(workdir, StringComparison.OrdinalIgnoreCase))
				throw new ModuleException("Cannot resolve path.");

			path = path[workdir.Length..];
		}

		return path.Replace('\\', '/');
	}

	public override void Invoke()
	{
		var path = ResolvePath();
		var explorer = path is null ? new CommitsExplorer(_repo, _repo.Head) : new CommitsExplorer(_repo, path);

		explorer
			.CreatePanel()
			.Open();
	}
}
