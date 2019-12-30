
// Copyright (c) Roman Kuzmin
// http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Text.RegularExpressions;

namespace HtmlToFarHelp
{
	static class Kit
	{
		static readonly Regex _reWinNewLine = new Regex(@"(?<!\r)\n"); //|\r(?!\n)
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

		static readonly Regex _reSpaces = new Regex(" +"); //??
		public static bool HasSpaces(string value)
		{
			return _reSpaces.IsMatch(value);
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
	}
}
