﻿using FarNet;
using StackExchange.Redis;

namespace RedisKit.Panels;

abstract class BaseExplorer(IDatabase database, Guid typeId) : Explorer(typeId)
{
	public IDatabase Database { get; } = database;

	public IServer GetServer() =>
		DB.GetServer(Database);

	protected abstract string PanelTitle();

	public override void EnterPanel(Panel panel)
	{
		panel.Title = PanelTitle();
	}
}
