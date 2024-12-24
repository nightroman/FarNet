using StackExchange.Redis;

namespace RedisKit.Panels;

abstract class BasePanel<T>(T explorer) : AbcPanel(explorer) where T : BaseExplorer
{
	public IDatabase Database { get; } = explorer.Database;

	public new T Explorer => (T)base.Explorer;
}
