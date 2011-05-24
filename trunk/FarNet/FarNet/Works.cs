
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2011 FarNet Team
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using System.Text.RegularExpressions;

namespace FarNet.Works
{
	/// <summary>
	/// For internal use.
	/// </summary>
	public enum FormatMessageMode
	{
		/// <summary>
		/// Cut wide lines.
		/// </summary>
		Cut,
		/// <summary>
		/// Wrap lines by words.
		/// </summary>
		Word
	}

	/// <summary>
	/// For internal use.
	/// </summary>
	public static class Kit
	{
		/// <summary>
		/// For internal use.
		/// </summary>
		public static string[] SplitLines(string value)
		{
			if (value == null)
				return new string[] { string.Empty };

			//! Regex is twice slower
			return value.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
		}
		/// <summary>
		/// For internal use.
		/// </summary>
		/// <param name="lines">Output lines.</param>
		/// <param name="message">Input string.</param>
		/// <param name="width">Maximum line width.</param>
		/// <param name="height">Maximum line count (message text area height).</param>
		/// <param name="mode">Formatting mode.</param>
		/// <remarks>
		/// Formats the string as a limited number of lines of limited width.
		/// </remarks>
		public static void FormatMessage(IList<string> lines, string message, int width, int height, FormatMessageMode mode)
		{
			if (lines == null) throw new ArgumentNullException("lines");
			if (message == null) throw new ArgumentNullException("message");

			Regex format = null;
			foreach (var line in Kit.SplitLines(message.Replace('\t', ' ')))
			{
				if (line.Length <= width)
				{
					lines.Add(line);
				}
				else if (mode == FormatMessageMode.Cut)
				{
					lines.Add(line.Substring(0, width));
				}
				else
				{
					if (format == null)
						format = new Regex("(.{0," + width + "}(?:\\s|$))");
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
		/// <summary>
		/// For internal use. Hashes the files using the comparer, counts dupes.
		/// </summary>
		public static Dictionary<FarFile, int> HashFiles(IEnumerable files, IEqualityComparer<FarFile> comparer)
		{
			if (files == null) throw new ArgumentNullException("files");

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
	}
}
