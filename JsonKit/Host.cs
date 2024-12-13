using FarNet;

namespace JsonKit;

public class Host : ModuleHost
{
	internal const string MyName = "JsonKit";

	internal static class Param
	{
		public const string File = "file";
		public const string Select = "select";
	}

	internal static Host Instance { get; private set; } = null!;

	public Host()
	{
		Instance = this;
	}
}
