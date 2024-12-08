using FarNet;
using GitKit.Extras;
using LibGit2Sharp;

namespace GitKit.Commands;

sealed class InitCommand(CommandParameters parameters) : AnyCommand
{
	readonly string _path = Host.GetFullPath(parameters.GetString(Parameter.Path, true));
	readonly bool _isBare = parameters.GetBool(Parameter.IsBare);

	public override void Invoke()
	{
		Repository.Init(_path, _isBare);
	}
}
