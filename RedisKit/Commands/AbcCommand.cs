using FarNet;

namespace RedisKit.Commands;

abstract class AbcCommand : Subcommand
{
	public const string ParamColon = "Colon";
	public const string ParamDB = "DB";
	public const string ParamEol = "Eol";
	public const string ParamKey = "Key";
	public const string ParamMask = "Mask";
	public const string ParamRedis = "Redis";
	public const string ParamRoot = "Root";
}
