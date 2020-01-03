
// Copyright (c) Roman Kuzmin
// http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Text.RegularExpressions;

namespace HtmlToFarHelp
{
	static class Kit
	{
		static readonly Regex _reWinNewLine = new Regex(@"(?<!\r)\n");
		public static string FixNewLine(string value)
		{
			return _reWinNewLine.Replace(value, "\r\n");
		}

		static readonly string[] _splitLines = new string[] { "\r\n", "\n" };
		public static string[] TextToLines(string text)
		{
			return text.Split(_splitLines, StringSplitOptions.None);
		}

		static readonly Regex _reNewLine = new Regex(@"\r?\n");
		public static string EmphasisText(string text)
		{
			return _reNewLine.Replace(text, "#\r\n#");
		}

		public static bool HasSpaces(string value)
		{
			return value.IndexOf(' ') >= 0;
		}

		static readonly Regex _reUnindent = new Regex(@"\r?\n[\ \t]+");
		public static string UnindentText(string text)
		{
			return _reUnindent.Replace(text, "\r\n");
		}

		static readonly Regex _reOptions = new Regex(@"^\s*HLF:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		public static Match MatchOptions(string text)
		{
			return _reOptions.Match(text);
		}

		static readonly char[] TrimNewLine = new char[] { '\r', '\n' };
		public static string TrimStartNewLine(string text)
		{
			return text.TrimStart(TrimNewLine);
		}
		public static string TrimEndNewLine(string text)
		{
			return text.TrimEnd(TrimNewLine);
		}
	}
}
