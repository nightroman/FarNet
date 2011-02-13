
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;
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
		public const string SplitLinePattern = "\r\n|[\r\n]";

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
			foreach (string line in Regex.Split(message.Replace('\t', ' '), SplitLinePattern))
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

	}
}
