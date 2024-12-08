using FarNet;
using RedisKit.Panels;
using StackExchange.Redis;
using System;

namespace RedisKit.Commands;

sealed class SetCommand(CommandParameters parameters) : BaseCommand(parameters)
{
	readonly RedisKey _key = parameters.GetRequiredString(Host.Param.Key);

	public override void Invoke()
	{
		var type = Database.KeyType(_key);
		if (type != RedisType.Set && type != RedisType.None)
			throw new InvalidOperationException($"Cannot open 'Set', the key is '{type}'.");

		new SetExplorer(Database, _key)
			.CreatePanel()
			.Open();
	}
}
