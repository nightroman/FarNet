using FarNet;
using System.Data.Common;
using System.Linq;

namespace RedisKit;

sealed class KeysCommand : BaseCommand
{
	static readonly char[] s_pattern = [ '*', '?', '[', ']' ];
    readonly string? _fix;
    readonly string? _mask;

	public KeysCommand(DbConnectionStringBuilder parameters) : base(parameters)
    {
        _fix = parameters.GetString(Host.Param.Fix);
        _mask = parameters.GetString(Host.Param.Mask);

		if (_fix?.IndexOfAny(s_pattern) >= 0)
			throw new ModuleException($"Parameter 'fix' contains invalid symbols: {string.Join(' ', s_pattern.Select(x => $"'{x}'"))}.");
    }

    public KeysCommand(string mask)
    {
        _mask = mask;
    }

    public override void Invoke()
	{
		new KeysExplorer(Database, _fix, _mask)
			.CreatePanel()
			.Open();
	}
}
