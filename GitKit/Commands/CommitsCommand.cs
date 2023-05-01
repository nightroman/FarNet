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
		_path = GetGitPathOrPath(
			parameters,
			returnNullIfRoot: true,
			validate: path => path == "?" ? Far.Api.FS.CursorPath ?? Far.Api.CurrentDirectory : path);
	}

	public override void Invoke()
	{
		var explorer = _path is null ? new CommitsExplorer(Repository, Repository.Head) : new CommitsExplorer(Repository, _path);

		explorer
			.CreatePanel()
			.Open();
	}
}
