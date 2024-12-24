using FarNet;

namespace RedisKit;

public class Host : ModuleHost
{
	internal const string MyName = "RedisKit";

	internal static class History
	{
		public const string Key = "RedisKey";
		public const string Prefix = "RedisPrefix";
	}

	public static Host Instance { get; private set; } = null!;

	public Host()
	{
		Instance = this;
	}
}
