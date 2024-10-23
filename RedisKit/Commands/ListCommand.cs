using RedisKit.Panels;
using StackExchange.Redis;
using System;
using System.Data.Common;

namespace RedisKit.Commands;

sealed class ListCommand(DbConnectionStringBuilder parameters) : BaseCommand(parameters)
{
	readonly RedisKey _key = parameters.GetRequiredString(Host.Param.Key);

	public override void Invoke()
	{
		var type = Database.KeyType(_key);
		if (type != RedisType.List && type != RedisType.None)
			throw new InvalidOperationException($"Cannot open 'List', the key is '{type}'.");

		new ListExplorer(Database, _key)
			.CreatePanel()
			.Open();
	}
}
