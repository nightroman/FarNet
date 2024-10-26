using GitKit.Extras;
using LibGit2Sharp;
using System.Data.Common;

namespace GitKit.Commands;

sealed class InitCommand : AnyCommand
{
	readonly string _path;
	readonly bool _isBare;

	public InitCommand(DbConnectionStringBuilder parameters)
	{
		_path = Host.GetFullPath(parameters.GetString(Parameter.Path, true));
		_isBare = parameters.GetBool(Parameter.IsBare);
	}

	public override void Invoke()
	{
		Repository.Init(_path, _isBare);
	}
}
