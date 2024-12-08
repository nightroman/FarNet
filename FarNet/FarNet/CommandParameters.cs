using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;

namespace FarNet;

/// <summary>
/// Command with parameters using connection string syntax.
/// </summary>
public class CommandParameters
{
	readonly DbConnectionStringBuilder _parameters;

	/// <summary>
	/// Gets the command name.
	/// </summary>
	public string Command { get; }

	private CommandParameters(string command, DbConnectionStringBuilder parameters)
	{
		Command = command;
		_parameters = parameters;
	}

	/// <summary>
	/// Parses the command line with parameters.
	/// </summary>
	/// <param name="commandLine">Command line with parameters.</param>
	/// <returns>Parsed command with parameters.</returns>
	public static CommandParameters Parse(string commandLine)
	{
		int index = 0;
		while (index < commandLine.Length && !char.IsWhiteSpace(commandLine[index]))
			++index;

		if (index == 0)
			return new(string.Empty, new());

		var command = commandLine[0..index];

		while (index < commandLine.Length && char.IsWhiteSpace(commandLine[index]))
			++index;

		var parameters = commandLine[index..];

		try
		{
			return new(command, new DbConnectionStringBuilder { ConnectionString = parameters });
		}
		catch (Exception ex)
		{
			throw new ModuleException($"""
			Invalid parameters syntax.
			Subcommand: {command}
			Parameters: {parameters}
			{ex.Message}
			""");
		}
	}

	/// <summary>
	/// Get the optional string and removes it.
	/// </summary>
	/// <param name="name">Parameter name.</param>
	/// <param name="expandVariables">Tells to expand environment variables.</param>
	/// <param name="resolveFullPath">Tells to resolve the path to full.</param>
	/// <returns>Parameter value or null.</returns>
	public string? GetString(string name, bool expandVariables = false, bool resolveFullPath = false)
	{
		if (!_parameters.TryGetValue(name, out object? raw))
			return null;

		_parameters.Remove(name);

		var value = (string)raw;
		if (expandVariables)
			value = Environment.ExpandEnvironmentVariables(value);

		if (resolveFullPath)
			value = Path.GetFullPath(value, Far.Api.CurrentDirectory);

		return value;
	}

	/// <summary>
	/// Get the required string and removes it.
	/// </summary>
	/// <param name="name">Parameter name.</param>
	/// <param name="expandVariables">Tells to expand environment variables.</param>
	/// <param name="resolveFullPath">Tells to resolve the path to full.</param>
	/// <returns>Parameter value.</returns>
	public string GetRequiredString(string name, bool expandVariables = false, bool resolveFullPath = false)
	{
		return GetString(name, expandVariables, resolveFullPath)
			?? throw new ModuleException($"""
			Missing required parameter '{name}'.
			Subcommand: {Command}
			""");
	}

	/// <summary>
	/// Get the optional bool (default is false) and removes it.
	/// </summary>
	/// <param name="name">Parameter name.</param>
	/// <returns>Gets true or false.</returns>
	public bool GetBool(string name)
	{
		var string1 = GetString(name);
		if (string1 == null)
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

		throw new ModuleException($"""
		Invalid parameter '{name}={string1}'.
		Subcommand: {Command}
		Valid values are true, false, 1, 0.
		""");
	}

	/// <summary>
	/// Get the optional T value (or T default) and removes it.
	/// </summary>
	/// <param name="name">Parameter name.</param>
	/// <returns>Gets the value or T default.</returns>
	/// <typeparam name="T">A type suitable for <c>Convert.ChangeType</c> from string.</typeparam>
	public T GetValue<T>(string name)
	{
		var string1 = GetString(name);
		if (string1 == null)
			return default!;

		try
		{
			return (T)Convert.ChangeType(string1, typeof(T));
		}
		catch (Exception ex)
		{
			throw new ModuleException($"""
			Invalid parameter '{name}={string1}'.
			Subcommand: {Command}
			{ex.Message}
			""");
		}
	}

	/// <summary>
	/// Throws if unknown parameters left unused.
	/// </summary>
	public void ThrowUnknownParameters()
	{
		if (_parameters.Count > 0)
		{
			throw new ModuleException($"""
				Uknknown parameters: {string.Join(", ", _parameters.Keys.Cast<string>())}
				Subcommand: {Command}
				""");
		}
	}
}
