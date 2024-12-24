using FarNet;
using RedisKit.Panels;
using StackExchange.Redis;
using System;

namespace RedisKit.Commands;

sealed class ListCommand(CommandParameters parameters) : BaseCommand(parameters)
{
	readonly RedisKey _key = parameters.GetRequiredString(Param.Key);

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
