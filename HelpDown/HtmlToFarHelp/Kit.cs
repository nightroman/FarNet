
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
	}
}
