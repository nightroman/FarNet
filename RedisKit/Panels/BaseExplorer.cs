using FarNet;
using StackExchange.Redis;
using System;

namespace RedisKit;

abstract class BaseExplorer(IDatabase database, Guid typeId) : Explorer(typeId)
{
    public IDatabase Database { get; } = database;

	public override void EnterPanel(Panel panel)
	{
		panel.Title = ToString()!;
	}
}
