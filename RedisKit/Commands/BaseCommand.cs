using FarNet;
using StackExchange.Redis;
using System;
using System.Data.Common;
using System.Linq;
using System.Xml.Linq;

namespace RedisKit;

abstract class BaseCommand : AnyCommand
{
	protected IDatabase Database { get; }

    protected BaseCommand()
    {
        Database = DB.Open(GetRedisConfiguration());
    }

    protected BaseCommand(DbConnectionStringBuilder parameters)
	{
		Database = DB.Open(GetRedisConfiguration(parameters.GetString("Redis")));
	}

	static string GetRedisConfiguration(string? configuration = null)
	{
		if (string.IsNullOrWhiteSpace(configuration))
		{
			// get the default
			var name = Workings.Default.GetData().Configuration;
			configuration = Settings.Default.GetData().Configurations
				.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?
				.Text;
		}
		else
		{
			// try by name
			var text = Settings.Default.GetData().Configurations
				.FirstOrDefault(x => x.Name.Equals(configuration, StringComparison.OrdinalIgnoreCase))?
				.Text;
			if (text is { })
				configuration = text;
		}

		if (string.IsNullOrWhiteSpace(configuration))
			throw new ModuleException("Cannot get Redis configuration from parameters or settings.");

		configuration = Environment.ExpandEnvironmentVariables(configuration);

		return configuration;
	}

	protected override void Dispose(bool disposing)
	{
	}
}
