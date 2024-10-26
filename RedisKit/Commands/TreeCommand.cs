using RedisKit.Panels;
using System.Data.Common;

namespace RedisKit.Commands;

sealed class TreeCommand : BaseCommand
{
	readonly string _colon;
	readonly string? _root;

	public TreeCommand(DbConnectionStringBuilder parameters) : base(parameters)
	{
		_colon = parameters.GetString(Host.Param.Colon) ?? ":";
		_root = parameters.GetString(Host.Param.Root);

		// ensure root ends with colon
		if (_root is { } && !_root.EndsWith(_colon))
			_root += _colon;
	}

	public override void Invoke()
	{
		new KeysExplorer(Database, _colon, _root, null)
			.CreatePanel()
			.Open();
	}
}
