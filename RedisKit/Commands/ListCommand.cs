using FarNet;
using RedisKit.Panels;
using StackExchange.Redis;

namespace RedisKit.Commands;

sealed class ListCommand : BaseCommand
{
	readonly RedisKey _key;

	public ListCommand(CommandParameters parameters) : base(parameters)
	{
		_key = GetRequiredRedisKeyOfType(parameters, RedisType.List);
	}

	public override void Invoke()
	{
		new ListExplorer(Database, _key)
			.CreatePanel()
			.Open();
	}
}
