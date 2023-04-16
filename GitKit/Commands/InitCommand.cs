using LibGit2Sharp;
using System.Data.Common;

namespace GitKit;

sealed class InitCommand : AnyCommand
{
	readonly string _path;
	readonly bool _isBare;

	public InitCommand(string value, DbConnectionStringBuilder parameters)
	{
		_path = Host.GetFullPath(value);
		_isBare = parameters.GetValue<bool>("IsBare");
	}

	public override void Invoke()
	{
		Repository.Init(_path, _isBare);
	}
}
