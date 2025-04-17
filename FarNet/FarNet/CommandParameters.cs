
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
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
	const string TextSeparator = ";;";

	readonly DbConnectionStringBuilder _parameters;

	/// <summary>
	/// Gets the command name.
	/// </summary>
	public ReadOnlySpan<char> Command { get; }

	/// <summary>
	/// Gets the trimmed text after ";;".
	/// </summary>
	public ReadOnlySpan<char> Text { get; }

	/// <summary>
	/// With "@" notation, gets the trimmed text after "?".
	/// </summary>
	public ReadOnlySpan<char> Text2 { get; }

	CommandParameters(
		ReadOnlySpan<char> command,
		ReadOnlySpan<char> text,
		ReadOnlySpan<char> text2,
		DbConnectionStringBuilder parameters)
	{
		Command = command;
		Text = text;
		Text2 = text2;
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
	/// Parses parameters with command and text.
	/// Use <c>@file</c> notation for reading from command files.
	/// </summary>
	/// <param name="commandLine">Command line with parameters.</param>
	/// <returns>Parsed parameters, command, text after ";;", text after "@...?".</returns>
	public static CommandParameters Parse(ReadOnlySpan<char> commandLine)
	{
		return Parse(commandLine, true);
	}

	/// <summary>
	/// Parses parameters with command and text.
	/// Use <c>@file</c> notation for reading from command files.
	/// </summary>
	/// <param name="commandLine">Command line with parameters.</param>
	/// <param name="hasCommand">Tells to parse the command, then parameters.</param>
	/// <returns>Parsed parameters, command (or empty), text after ";;", text after "@...?".</returns>
	public static CommandParameters Parse(ReadOnlySpan<char> commandLine, bool hasCommand)
	{
		ReadOnlySpan<char> text2 = default;
		if (commandLine.StartsWith('@'))
		{
			var index = commandLine.IndexOf('?');
			if (index > 0)
			{
				text2 = commandLine[(index + 1)..].Trim();
				commandLine = commandLine[0..index];
			}

			var file = Far.Api.GetFullPath(Environment.ExpandEnvironmentVariables(commandLine[1..].Trim().ToString()));
			try { commandLine = File.ReadAllText(file); }
			catch (Exception ex) { throw new ModuleException(ex.Message); }
		}

		ReadOnlySpan<char> command;
		if (hasCommand)
		{
			int index = 0;
			while (index < commandLine.Length && !char.IsWhiteSpace(commandLine[index]))
				++index;

			if (index == 0)
				return new(default, commandLine.Trim(), text2, null!);

			command = commandLine[0..index];
			commandLine = commandLine[index..].TrimStart();
		}
		else
		{
			command = default;
		}

		ReadOnlySpan<char> parameters;
		ReadOnlySpan<char> text;
		{
			int index = commandLine.IndexOf(TextSeparator);
			if (index < 0)
			{
				parameters = commandLine;
				text = default;
			}
			else
			{
				parameters = commandLine[0..index];
				text = commandLine[(index + TextSeparator.Length)..].Trim();
			}
		}

		try
		{
			return new(command, text, text2, ParseParameters(parameters.ToString()));
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
				value = Far.Api.GetFullPath(value);

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
	/// <param name="value">Default value.</param>
	/// <returns>Gets the value or default.</returns>
	/// <typeparam name="T">Enum or primitive suitable for <c>Convert.ChangeType</c>.</typeparam>
	public T GetValue<T>(string name, T value = default!)
	{
		var string1 = GetString(name);
		if (string1 is null)
			return value;

		try
		{
			if (typeof(T).IsEnum)
				return (T)Enum.Parse(typeof(T), string1);
			else
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
				.Append("Unknown parameters: ").Append(string.Join(", ", _parameters.Keys.Cast<string>())).ToString());
		}
	}
}
