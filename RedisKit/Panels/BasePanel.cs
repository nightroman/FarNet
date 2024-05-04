using StackExchange.Redis;

namespace RedisKit;

abstract class BasePanel<T> : AnyPanel where T : BaseExplorer
{
	public IDatabase Repository { get; }

	public new T Explorer => (T)base.Explorer;

	public BasePanel(T explorer) : base(explorer)
	{
		Repository = explorer.Database;
	}

	public override void Open()
	{
		base.Open();
	}

	public override void UIClosed()
	{
		base.UIClosed();
	}
}
