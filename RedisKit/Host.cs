using FarNet;

namespace RedisKit;

public class Host : ModuleHost
{
	internal const string MyName = "RedisKit";

	internal static class Param
	{
		public const string Colon = "colon";
		public const string Key = "key";
		public const string Mask = "mask";
		public const string Redis = "redis";
		public const string Root = "root";
	}

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
