using StackExchange.Redis;

namespace RedisKit;

abstract class BasePanel<T>(T explorer) : AnyPanel(explorer) where T : BaseExplorer
{
	public IDatabase Database { get; } = explorer.Database;
	public new T Explorer => (T)base.Explorer;
}
