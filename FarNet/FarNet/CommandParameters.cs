
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

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
[Experimental("FarNet250101")]
public readonly ref struct CommandParameters
{
	readonly DbConnectionStringBuilder _parameters;

	/// <summary>
	/// Gets the command name.
	/// </summary>
	public ReadOnlySpan<char> Command { get; }

	/// <summary>
	/// Gets the separated text.
	/// </summary>
	public ReadOnlySpan<char> Text { get; }

	CommandParameters(ReadOnlySpan<char> command, ReadOnlySpan<char> text, DbConnectionStringBuilder parameters)
	{
		Command = command;
		Text = text;
		_parameters = parameters;
	}

	/// <summary>
	/// Parses parameters.
	/// </summary>
	/// <param name="parameters">Parameters string.</param>
	/// <returns>Parsed parameters string builder.</returns>
	public static DbConnectionStringBuilder ParseParameters(string parameters)
	{
		try
		{
			return new DbConnectionStringBuilder { ConnectionString = parameters };
		}
		catch (Exception ex)
		{
			throw new ModuleException($"""
			Invalid parameters: {parameters}
			{ex.Message}
			""");
		}
	}

	/// <summary>
	/// Parses command with parameters.
	/// </summary>
	/// <param name="commandLine">Command line with parameters.</param>
	/// <returns>Parsed parameters and command.</returns>
	public static CommandParameters Parse(ReadOnlySpan<char> commandLine)
	{
		return Parse(commandLine, true, null);
	}

	/// <summary>
	/// Parses parameters with optional command and optional text.
	/// </summary>
	/// <param name="commandLine">Command line with parameters.</param>
	/// <param name="hasCommand">Tells to separate the leading command.</param>
	/// <param name="textSeparator">Tells to separate the trailing text.</param>
	/// <returns>Parsed parameters.</returns>
	public static CommandParameters Parse(ReadOnlySpan<char> commandLine, bool hasCommand, string? textSeparator)
	{
		ReadOnlySpan<char> command;
		if (hasCommand)
		{
			int index = 0;
			while (index < commandLine.Length && !char.IsWhiteSpace(commandLine[index]))
				++index;

			if (index == 0)
				return new(default, commandLine.TrimStart(), null!);

			command = commandLine[0..index];
			commandLine = commandLine[index..].TrimStart();
		}
		else
		{
			command = default;
		}

		ReadOnlySpan<char> parameters;
		ReadOnlySpan<char> text;
		if (textSeparator is { })
		{
			var index = commandLine.IndexOf(textSeparator);
			if (index < 0)
			{
				parameters = commandLine;
				text = default;
			}
			else
			{
				parameters = commandLine[0..index];
				text = commandLine[(index + textSeparator.Length)..].TrimStart();
			}
		}
		else
		{
			parameters = commandLine;
			text = default;
		}

		try
		{
			return new(command, text, ParseParameters(parameters.ToString()));
		}
		catch (Exception ex)
		{
			throw new ModuleException(ErrorBuilder(command)
				.Append(ex.Message).ToString());
		}
	}

	static StringBuilder ErrorBuilder(ReadOnlySpan<char> command)
	{
		var sb = new StringBuilder();
		if (command.Length > 0)
			sb.Append("Command: ").Append(command).AppendLine();
		return sb;
	}

	/// <summary>
	/// Creates module exceptions for parameter errors.
	/// </summary>
	/// <param name="name">Parameter name.</param>
	/// <param name="message">Error message.</param>
	/// <returns></returns>
	public ModuleException ParameterError(string name, string message)
	{
		return new ModuleException(ErrorBuilder(Command)
			.Append("Parameter '").Append(name).Append("': ").Append(message).ToString());
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

		throw ParameterError(name, $"Invalid value '{string1}': valid values: true, false, 1, 0.");
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
		if (string1 is null)
			return default!;

		try
		{
			return (T)Convert.ChangeType(string1, typeof(T));
		}
		catch (Exception ex)
		{
			throw ParameterError(name, $"Invalid value '{string1}': {ex.Message}");
		}
	}

	/// <summary>
	/// Throws if unknown parameters left unused.
	/// </summary>
	public void ThrowUnknownParameters()
	{
		if (_parameters.Count > 0)
		{
			throw new ModuleException(ErrorBuilder(Command)
				.Append("Uknknown parameters: ").Append(string.Join(", ", _parameters.Keys.Cast<string>())).ToString());
		}
	}
}
