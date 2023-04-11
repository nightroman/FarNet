using FarNet;
using LibGit2Sharp;
using System;
using System.Data.Common;

namespace GitKit;

public static class Parameters
{
	public static DbConnectionStringBuilder Parse(string text)
	{
		try
		{
			return new DbConnectionStringBuilder { ConnectionString = text };
		}
		catch (Exception ex)
		{
			throw new ModuleException($"Use semicolon separated key=value pairs. Error: {ex.Message}");
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

	public static void AssertNone(this DbConnectionStringBuilder parameters)
	{
		if (parameters.Count > 0)
			throw new ModuleException($"Unknown parameters: {parameters}");
	}
}
