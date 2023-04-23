using FarNet;
using LibGit2Sharp;
using System.Data.Common;
using System.IO;

namespace GitKit;

sealed class CDCommand : BaseCommand
{
	readonly string _path;

	public CDCommand(Repository repo, DbConnectionStringBuilder parameters) : base(repo)
	{
		_path = parameters.GetValue("Path") ?? string.Empty;
	}

	public override void Invoke()
	{
		if (Far.Api.Window.Kind != WindowKind.Panels)
			Far.Api.Window.SetCurrentAt(-1);

		string path;
		var info = _repo.Info;
		if (_path == ".git")
		{
			path = info.Path;
		}
		else if (_path.StartsWith(".git/") || _path.StartsWith(".git\\"))
		{
			var inner = _path[5..].TrimStart('\\').TrimStart('/');
			path = Path.Combine(info.Path, inner);
		}
		else
		{
			var inner = _path.TrimStart('\\').TrimStart('/');
			path = Path.Combine(info.WorkingDirectory ?? info.Path, inner);
		}

		Far.Api.Panel!.CurrentDirectory = path;
	}
}
