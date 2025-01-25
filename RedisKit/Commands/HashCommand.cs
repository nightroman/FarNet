using FarNet;
using RedisKit.Panels;
using StackExchange.Redis;

namespace RedisKit.Commands;

sealed class HashCommand : BaseCommand
{
	readonly RedisKey _key;
	readonly bool _eol;

	public HashCommand(CommandParameters parameters) : base(parameters)
	{
		_key = GetRequiredRedisKeyOfType(parameters, RedisType.Hash);
		_eol = parameters.GetBool(Param.Eol);
	}

	public override void Invoke()
	{
		new HashExplorer(Database, _key, _eol)
			.CreatePanel()
			.Open();
	}
}
