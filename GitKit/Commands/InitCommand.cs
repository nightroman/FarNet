using LibGit2Sharp;
using System.Data.Common;

namespace GitKit;

sealed class InitCommand : AnyCommand
{
	readonly string _path;
	readonly bool _isBare;

	public InitCommand(DbConnectionStringBuilder parameters)
	{
		_path = Host.GetFullPath(parameters.GetValue("Path"));
		_isBare = parameters.GetValue<bool>("IsBare");
	}

	public override void Invoke()
	{
		Repository.Init(_path, _isBare);
	}
}
