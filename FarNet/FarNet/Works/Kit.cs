
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text.RegularExpressions;

namespace FarNet.Works;
#pragma warning disable 1591

public static class Kit
{
	static readonly string[] SplitLineSeparators = ["\r\n", "\r", "\n"];

	// Parses parameters string in connection string format, wraps exceptions.
	public static DbConnectionStringBuilder ParseParameters(string parameters)
	{
		try
		{
			return new DbConnectionStringBuilder { ConnectionString = parameters };
		}
		catch (Exception ex)
		{
			throw new ArgumentException($"Invalid parameters (connection string format):\r\n{parameters}\r\n{ex.Message}");
		}
	}

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
		foreach (var line in Kit.SplitLines(message.Replace('\t', ' ')))
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
				format ??= new Regex("(.{0," + width + "}(?:\\s|$))");
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
	public static bool IsInvalidFileName(string name)
	{
		if (string.IsNullOrEmpty(name))
			return true;

		_invalidName ??= new Regex("[" + Regex.Escape(new string(Path.GetInvalidFileNameChars())) + @"]|[\s.]$|^(?:CON|PRN|AUX|NUL|(?:COM|LPT)[1-9])$", RegexOptions.IgnoreCase);

		return _invalidName.IsMatch(name);
	}
	static Regex? _invalidName;

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
}
