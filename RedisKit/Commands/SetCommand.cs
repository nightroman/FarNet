using FarNet;
using RedisKit.Panels;
using StackExchange.Redis;

namespace RedisKit.Commands;

sealed class SetCommand : BaseCommand
{
	readonly RedisKey _key;

	public SetCommand(CommandParameters parameters) : base(parameters)
	{
		_key = GetRequiredRedisKeyOfType(parameters, RedisType.Set);
	}

	public override void Invoke()
	{
		new SetExplorer(Database, _key)
			.CreatePanel()
			.Open();
	}
}
