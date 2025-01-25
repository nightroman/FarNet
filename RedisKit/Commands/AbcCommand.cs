namespace RedisKit.Commands;

abstract class AbcCommand
{
	public static class Param
	{
		public const string Colon = "Colon";
		public const string Eol = "Eol";
		public const string Key = "Key";
		public const string Mask = "Mask";
		public const string Redis = "Redis";
		public const string Root = "Root";
	}

	public abstract void Invoke();
}
