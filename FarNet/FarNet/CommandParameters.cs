
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace FarNet;

/// <summary>
/// Command with parameters using connection string syntax.
/// </summary>
/// <remarks>
/// <para>
/// <c>Get*</c> methods extract and remove specified parameters.
/// Thus, do not call <c>Get*</c> twice for the same parameter.
/// </para>
/// <para>
/// Ideally, call <c>Get*</c> for each command parameter.
/// Then call <see cref="ThrowUnknownParameters"/>.
/// </para>
/// </remarks>
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
			return new(string.Empty, []);

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
			Command: {command}
			Parameters: {parameters}
			{ex.Message}
			""");
		}
	}

	/// <summary>
	/// Creates module exceptions for parameter errors.
	/// </summary>
	/// <param name="name">Parameter name.</param>
	/// <param name="message">Error message.</param>
	/// <returns></returns>
	public ModuleException ParameterError(string name, string message)
	{
		return new ModuleException($"""
			Parameter '{name}': {message}
			Command: {Command}
			""");
	}

	/// <summary>
	/// Get the optional string and removes it.
	/// </summary>
	/// <param name="name">Parameter name.</param>
	/// <param name="options">Parameter options.</param>
	/// <returns>Parameter value or null.</returns>
	public string? GetString(string name, ParameterOptions options = ParameterOptions.None)
	{
		if (_parameters.TryGetValue(name, out object? raw))
		{
			_parameters.Remove(name);

			var value = (string)raw;
			if (options.HasFlag(ParameterOptions.ExpandVariables))
				value = Environment.ExpandEnvironmentVariables(value);

			if (options.HasFlag(ParameterOptions.GetFullPath))
				value = Far.Api.FS.GetFullPath(value);

			return value;
		}
		else
		{
			if (options.HasFlag(ParameterOptions.UseCursorPath))
				return Far.Api.FS.CursorPath;

			if (options.HasFlag(ParameterOptions.UseCursorFile))
				return Far.Api.FS.CursorFile?.FullName;

			if (options.HasFlag(ParameterOptions.UseCursorDirectory))
				return Far.Api.FS.CursorDirectory?.FullName;

			return null;
		}
	}

	/// <summary>
	/// Get the required string and removes it.
	/// </summary>
	/// <param name="name">Parameter name.</param>
	/// <param name="options">Parameter options.</param>
	/// <returns>Parameter value.</returns>
	public string GetRequiredString(string name, ParameterOptions options = ParameterOptions.None)
	{
		if (GetString(name, options) is { } result)
			return result;

		throw
			options.HasFlag(ParameterOptions.UseCursorPath) ? ParameterError(name, "Omitted requires the panel cursor item.") :
			options.HasFlag(ParameterOptions.UseCursorFile) ? ParameterError(name, "Omitted requires the panel cursor file.") :
			options.HasFlag(ParameterOptions.UseCursorDirectory) ? ParameterError(name, "Omitted requires the panel cursor directory.") :
			ParameterError(name, "Missing required.");
	}

	/// <summary>
	/// Calls <see cref="GetString"/> with <see cref="ParameterOptions.ExpandVariables"/> and <see cref="ParameterOptions.GetFullPath"/>.
	/// </summary>
	/// <param name="name">Parameter name.</param>
	/// <param name="options">Parameter options.</param>
	/// <returns>Full path or null.</returns>
	public string? GetPath(string name, ParameterOptions options = ParameterOptions.None)
	{
		return GetString(name, options | ParameterOptions.ExpandVariables | ParameterOptions.GetFullPath);
	}

	/// <summary>
	/// Calls <see cref="GetPath"/> and fails if it is null.
	/// </summary>
	/// <param name="name">Parameter name.</param>
	/// <param name="options">Parameter options.</param>
	/// <returns>Full path.</returns>
	public string GetRequiredPath(string name, ParameterOptions options = ParameterOptions.None)
	{
		return GetRequiredString(name, options | ParameterOptions.ExpandVariables | ParameterOptions.GetFullPath);
	}

	/// <summary>
	/// Calls <see cref="GetPath"/> and if it is null returns <see cref="IFar.CurrentDirectory"/>.
	/// </summary>
	/// <param name="name">Parameter name.</param>
	/// <param name="options">Parameter options.</param>
	/// <returns>Full path.</returns>
	public string GetPathOrCurrentDirectory(string name, ParameterOptions options = ParameterOptions.None)
	{
		return GetString(name, options | ParameterOptions.ExpandVariables | ParameterOptions.GetFullPath) ?? Far.Api.CurrentDirectory;
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
		Command: {Command}
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
			Command: {Command}
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
				Command: {Command}
				""");
		}
	}
}
