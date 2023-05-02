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

	public static string? GetString(this DbConnectionStringBuilder parameters, string name, bool expand = false)
	{
		if (parameters.TryGetValue(name, out object? value))
		{
			parameters.Remove(name);
			return expand ? Environment.ExpandEnvironmentVariables((string)value) : (string)value;
		}
		else
		{
			return null;
		}
	}

	public static string GetStringRequired(this DbConnectionStringBuilder parameters, string name, bool expand = false)
	{
		return GetString(parameters, name, expand) ?? throw new ModuleException($"Missing required parameter '{name}'.");
	}

	public static bool GetBool(this DbConnectionStringBuilder parameters, string name)
	{
		var string1 = GetString(parameters, name);
		if (string1 is null)
			return false;

		if (bool.TryParse(string1, out bool bool1))
			return bool1;

		if (int.TryParse(string1, out int int1))
		{
			if (int1 == 1)
				return true;

			if (int1 == 0)
				return false;
		}

		throw new ModuleException($"Invalid parameter '{name}={string1}'. Valid values: true, false, 1, 0.");
	}

	public static T GetValue<T>(this DbConnectionStringBuilder parameters, string name)
	{
		var string1 = GetString(parameters, name);
		if (string1 is null)
			return default!;

		try
		{
			return (T)Convert.ChangeType(string1, typeof(T));
		}
		catch (Exception ex)
		{
			throw new ModuleException($"Invalid parameter '{name}={string1}': {ex.Message}");
		}
	}
}
