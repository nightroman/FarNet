using FarNet;
using System;
using System.Data.Common;

namespace GitKit;

static class Parameters
{
	public static (string?, DbConnectionStringBuilder?) Parse(string command)
	{
		int index = 0;
		while (index < command.Length && !char.IsWhiteSpace(command[index]))
			++index;

		var subcommand = command[0..index];
		if (subcommand.Length == 0)
			return (null, null);

		while (index < command.Length && char.IsWhiteSpace(command[index]))
			++index;

		var parameters = command[index..];

		try
		{
			return (subcommand, new DbConnectionStringBuilder { ConnectionString = parameters });
		}
		catch (Exception ex)
		{
			var message = $"""
			Invalid parameters syntax
			Subcommand: {subcommand}
			Parameters: {parameters}
			{ex.Message}
			""";
			throw new ModuleException(message);
		}
	}

	public static string? GetValue(this DbConnectionStringBuilder parameters, string name)
	{
		if (parameters.TryGetValue(name, out object? value))
		{
			parameters.Remove(name);
			return (string)value;
		}
		else
		{
			return null;
		}
	}

	public static string GetRequired(this DbConnectionStringBuilder parameters, string name)
	{
		return GetValue(parameters, name) ?? throw new ModuleException($"Missing required parameter '{name}'.");
	}

	public static T GetValue<T>(this DbConnectionStringBuilder parameters, string name)
	{
		if (parameters.TryGetValue(name, out object? value))
		{
			parameters.Remove(name);
			try
			{
				return (T)Convert.ChangeType(value, typeof(T));
			}
			catch (Exception ex)
			{
				throw new ModuleException($"{name}: {ex.Message}");
			}
		}
		else
		{
			return default!;
		}
	}
}
