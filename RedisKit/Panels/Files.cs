using FarNet;
using StackExchange.Redis;

namespace RedisKit.Panels;

static class Files
{
	public record FileDataFolder(string Prefix);
	public static FileDataFolder DataFolder(this FarFile file) => (FileDataFolder)file.Data!;

	public record FileDataKey(RedisKey Key);
	public static FileDataKey DataKey(this FarFile file) => (FileDataKey)file.Data!;

	public record ArgsDataName(string Name);
	public static ArgsDataName DataName(this ExplorerEventArgs args) => (ArgsDataName)args.Data!;

	public record KeyInput(string Name, string? Prefix);
}
