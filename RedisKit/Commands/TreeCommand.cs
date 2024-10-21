using System.Data.Common;

namespace RedisKit;

sealed class TreeCommand : BaseCommand
{
	readonly string _colon;
	readonly string? _root;

	public TreeCommand(DbConnectionStringBuilder parameters) : base(parameters)
    {
		_colon = parameters.GetString(Host.Param.Colon) ?? ":";
		_root = parameters.GetString(Host.Param.Root);

		if (_root is { })
		{
			// Remove just one trailing colon, probably prefix copy/paste.
			// This also allows `:` as the empty name (vs null) for `:*`.
			if (_root.EndsWith(_colon))
				_root = _root[..^_colon.Length];
		}
	}

    public override void Invoke()
	{
		new KeysExplorer(Database, _colon, _root)
			.CreatePanel()
			.Open();
	}
}
