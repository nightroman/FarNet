using FarNet;

namespace JsonKit.Commands;

abstract class AbcCommand : Subcommand
{
	public const string ParamFile = "File";
	public const string ParamSelect = "Select";
}
