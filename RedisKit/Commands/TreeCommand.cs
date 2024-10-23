using RedisKit.Panels;
using System.Data.Common;

namespace RedisKit.Commands;

sealed class TreeCommand : BaseCommand
{
	readonly string _colon;
	readonly string? _prefix;

	public TreeCommand(DbConnectionStringBuilder parameters) : base(parameters)
	{
		_colon = parameters.GetString(Host.Param.Colon) ?? ":";
		_prefix = parameters.GetString(Host.Param.Root);

		// root -> prefix
		if (_prefix is { })
		{
			if (!_prefix.EndsWith(_colon))
				_prefix += _colon;
		}
	}

	public override void Invoke()
	{
		new KeysExplorer(Database, _colon, _prefix)
			.CreatePanel()
			.Open();
	}
}
