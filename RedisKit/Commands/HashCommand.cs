using FarNet;
using RedisKit.Panels;
using StackExchange.Redis;
using System;

namespace RedisKit.Commands;

sealed class HashCommand(CommandParameters parameters) : BaseCommand(parameters)
{
	readonly RedisKey _key = parameters.GetRequiredString(Param.Key);

	public override void Invoke()
	{
		var type = Database.KeyType(_key);
		if (type != RedisType.Hash && type != RedisType.None)
			throw new InvalidOperationException($"Cannot open 'Hash', the key is '{type}'.");

		new HashExplorer(Database, _key)
			.CreatePanel()
			.Open();
	}
}
