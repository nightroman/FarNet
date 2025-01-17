﻿using FarNet;
using StackExchange.Redis;
using System;
using System.Linq;

namespace RedisKit.Commands;

abstract class BaseCommand : AbcCommand
{
	protected IDatabase Database { get; }

	protected BaseCommand()
	{
		Database = OpenDatabase(GetRedisConfiguration());
	}

	protected BaseCommand(CommandParameters parameters)
	{
		Database = OpenDatabase(GetRedisConfiguration(parameters.GetString(Param.Redis)));
	}

	static IDatabase OpenDatabase(string configuration)
	{
		try
		{
			return DB.Open(configuration);
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
				configuration = configurations
					.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
					?.Text
					?? throw new ModuleException($"Cannot find Redis configuration '{name}' in settings.");
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
