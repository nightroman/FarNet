using FarNet;

namespace JsonKit;

public class Host : ModuleHost
{
	internal const string MyName = "JsonKit";

	internal static Host Instance { get; private set; } = null!;

	public Host()
	{
		Instance = this;
	}
}
