using System.Collections;
using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace FarNet.Works;
#pragma warning disable 1591

public static class Kit
{
	static readonly string[] SplitLineSeparators = ["\r\n", "\n", "\r"];

	// For IndexOfAny(), etc.
	public static char[] NewLineChars { get; } = ['\r', '\n'];

	// Joins two strings with a space. Either string may be null or empty.
	public static string JoinText(string? head, string? tail)
	{
		if (string.IsNullOrEmpty(head))
			return tail ?? string.Empty;
		if (string.IsNullOrEmpty(tail))
			return head ?? string.Empty;
		return head + " " + tail;
	}

	public static Exception UnwrapAggregateException(Exception exn)
	{
		if (exn is AggregateException aggregate && aggregate.InnerExceptions.Count == 1)
			return aggregate.InnerExceptions[0];
		else
			return exn;
	}

	// %TEMP%\GUID.tmp or GUID.extension
	public static string TempFileName(string? extension)
	{
		var name = Guid.NewGuid().ToString("N");
		if (string.IsNullOrEmpty(extension))
			name += ".tmp";
		else if (extension[0] == '.')
			name += extension;
		else
			name += "." + extension;
		return Path.GetTempPath() + name;
	}

	public static string[] SplitLines(string value)
	{
		if (value == null)
			return [string.Empty];

		//! Regex is twice slower
		return value.Split(SplitLineSeparators, StringSplitOptions.None);
	}

	// Formats the string as a limited number of lines of limited width.
	// lines Output lines.
	// message Input string.
	// width Maximum line width.
	// height Maximum line count (message text area height).
	// mode Formatting mode.
	public static void FormatMessage(IList<string> lines, string message, int width, int height, FormatMessageMode mode)
	{
		ArgumentNullException.ThrowIfNull(lines);
		ArgumentNullException.ThrowIfNull(message);

		Regex? format = null;
		foreach (var line in SplitLines(message.Replace('\t', ' ')))
		{
			if (line.Length <= width)
			{
				lines.Add(line);
			}
			else if (mode == FormatMessageMode.Cut)
			{
				lines.Add(line[..width]);
			}
			else
			{
				format ??= new Regex($"(.{{0,{width}}}(?:\\{(mode == FormatMessageMode.Space ? 's' : 'W')}|$))");
				string[] s3 = format.Split(line);
				foreach (string s2 in s3)
				{
					if (s2.Length > 0)
					{
						lines.Add(s2);
						if (lines.Count >= height)
							return;
					}
				}
			}
			if (lines.Count >= height)
				return;
		}
	}

	// Hashes the files using the comparer, counts dupes.
	public static Dictionary<FarFile, int> HashFiles(IEnumerable files, IEqualityComparer<FarFile> comparer)
	{
		ArgumentNullException.ThrowIfNull(files);

		var hash = new Dictionary<FarFile, int>(comparer);
		foreach (FarFile file in files)
		{
			try
			{
				hash.Add(file, 1);
			}
			catch (ArgumentException)
			{
				++hash[file];
			}
		}
		return hash;
	}

	// Gets true if a string is not a valid file system file name.
	public static bool IsInvalidFileName(string? name)
	{
		if (string.IsNullOrEmpty(name))
			return true;

		if (name[^1] == '.' || char.IsWhiteSpace(name[^1]))
			return true;

		_invalidFileNameChars ??= Path.GetInvalidFileNameChars();
		if (name.IndexOfAny(_invalidFileNameChars) >= 0)
			return true;

		_invalidNames ??= FrozenSet.Create(StringComparer.OrdinalIgnoreCase, [
			"CON", "PRN", "AUX", "NUL",
			"COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
			"LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
		]);

		return _invalidNames.Contains(name);
	}
	static char[]? _invalidFileNameChars;
	static FrozenSet<string>? _invalidNames;

	// Interactively fixes an invalid file name.
	// name An invalid file name.
	// returns A valid file name or null if canceled.
	public static string? FixInvalidFileName(string? name)
	{
		for (; ; )
		{
			name = Far.Api.Input("Correct file name", null, "Invalid file name", name);
			if (null == name)
				return null;

			if (IsInvalidFileName(name))
				continue;

			return name;
		}
	}

	// Gets or sets the default macro output mode.
	public static bool MacroOutput { get; set; }

	/// <summary>
	/// Splits the command line to prefix and command.
	/// </summary>
	/// <param name="commandLine">Input.</param>
	/// <param name="prefix">Prefix with all around spaces or just spaces.</param>
	/// <param name="command">Command text with trimmed start.</param>
	/// <param name="isPrefix">Prefix filter.</param>
	public static void SplitCommandWithPrefix(
		ReadOnlySpan<char> commandLine,
		out ReadOnlySpan<char> prefix,
		out ReadOnlySpan<char> command,
		Predicate<ReadOnlySpan<char>> isPrefix)
	{
		// skip spaces
		int index1 = 0;
		while (index1 < commandLine.Length && (char.IsWhiteSpace(commandLine[index1]) || commandLine[index1] == '@'))
			++index1;

		// skip word
		int index2 = index1;
		while (index2 < commandLine.Length && char.IsLetterOrDigit(commandLine[index2]))
			++index2;

		// has prefix?
		if (index2 > 0 && index2 < commandLine.Length && commandLine[index2] == ':')
		{
			prefix = commandLine[index1..index2];
			if (isPrefix(prefix))
			{
				// skip spaces after ':'
				++index2;
				while (index2 < commandLine.Length && char.IsWhiteSpace(commandLine[index2]))
					++index2;

				prefix = commandLine[0..index2];
				command = commandLine[index2..];
				return;
			}
		}

		// unknown or no prefix
		prefix = commandLine[0..index1];
		command = commandLine[index1..];
	}
}
