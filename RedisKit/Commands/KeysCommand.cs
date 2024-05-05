using FarNet;
using System.Data.Common;
using System.Linq;

namespace RedisKit;

sealed class KeysCommand : BaseCommand
{
    readonly string? _mask;

	public KeysCommand(DbConnectionStringBuilder parameters) : base(parameters)
    {
        _mask = parameters.GetString(Host.Param.Mask);
    }

    public KeysCommand(string mask)
    {
        _mask = mask;
    }

    public override void Invoke()
	{
		new KeysExplorer(Database, _mask)
			.CreatePanel()
			.Open();
	}
}
