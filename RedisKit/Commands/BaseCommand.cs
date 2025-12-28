using FarNet;
using StackExchange.Redis;

namespace RedisKit.Commands;

abstract class BaseCommand : AbcCommand
{
	protected IDatabase Database { get; }

	protected BaseCommand(CommandParameters parameters)
	{
		var config = parameters.GetString(ParamRedis);
		var index = parameters.GetValue(ParamDB, -1);
		Database = OpenDatabase(GetRedisConfiguration(config), index);
	}

	protected RedisKey GetRequiredRedisKeyOfType(CommandParameters parameters, RedisType expectedType)
	{
		RedisKey key = parameters.GetRequiredString(ParamKey);

		var actualType = Database.KeyType(key);
		if (actualType != expectedType && actualType != RedisType.None)
			throw parameters.ParameterError(ParamKey, $"The existing key is '{actualType}', not '{expectedType}'.");

		return key;
	}

	static IDatabase OpenDatabase(string configuration, int index)
	{
		try
		{
			return DB.Open(configuration, index);
		}
		catch (Exception ex)
		{
			throw new ModuleException($"Cannot connect Redis '{configuration}'.", ex);
		}
	}

	static string GetRedisConfiguration(string? configuration = null)
	{
		if (string.IsNullOrWhiteSpace(configuration))
		{
			// get from settings
			var configurations = Settings.Default.GetData().Configurations;
			var name = Workings.Default.GetData().Configuration;
			if (string.IsNullOrWhiteSpace(name))
			{
				// get the head
				configuration = configurations.FirstOrDefault()?.Text;
			}
			else
			{
				// find by name
				configuration = configurations.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Text
					?? throw new ModuleException($"""
					Cannot find Redis configuration '{name}' in settings.
					Select existing using F11 / RedisKit / Configuration.
					""");
			}
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
}
