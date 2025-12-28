using FarNet;
using LibGit2Sharp;

namespace GitKit.Commands;

sealed class InitCommand(CommandParameters parameters) : AbcCommand
{
	readonly string _path = parameters.GetPathOrCurrentDirectory(ParamPath);
	readonly bool _isBare = parameters.GetBool(ParamIsBare);

	public override void Invoke()
	{
		Repository.Init(_path, _isBare);
	}
}
