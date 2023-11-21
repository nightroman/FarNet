using System.Text.RegularExpressions;

namespace PowerShellFar;

static partial class MyRegex
{
	[GeneratedRegex(@"(\s*)(?:(\w+):)?\s*")]
	public static partial Regex CommandWithPrefix();

	[GeneratedRegex(@"^(.*[!;\(\{\|""'']*)\$(global:|script:|private:)?(\w*)$", RegexOptions.IgnoreCase)]
	public static partial Regex CompleteVariable();

	[GeneratedRegex(@"(?:^|\s)(\S+)$")]
	public static partial Regex CompleteWord();

	[GeneratedRegex(@"ErrorActionPreference.*Stop:\s*(.*)")]
	public static partial Regex ErrorActionPreference();

	[GeneratedRegex(@"[\r\n\t]+")]
	public static partial Regex NewLinesAndTabs();

	[GeneratedRegex(@"\s+")]
	public static partial Regex Spaces();

	[GeneratedRegex(@"([`\[\]\*\?])")]
	public static partial Regex WildcardChar();
}
