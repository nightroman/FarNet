using FarNet;
using FarNet.Redis;
using StackExchange.Redis;

namespace RedisKit.Panels;

abstract class BaseExplorer(IDatabase database, Guid typeId) : Explorer(typeId)
{
	public IDatabase Database { get; } = database;

	public IServer GetServer() =>
		AboutRedis.GetServer(Database.Multiplexer);

	protected abstract string PanelTitle();

	public override void EnterPanel(Panel panel)
	{
		panel.Title = PanelTitle();
	}
}
