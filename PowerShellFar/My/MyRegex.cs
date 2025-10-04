using System.Text.RegularExpressions;

namespace PowerShellFar;

static partial class MyRegex
{
	[GeneratedRegex(@"^(.*[!;\(\{\|""'']*)\$(global:|script:|private:)?(\w*)$", RegexOptions.IgnoreCase)]
	public static partial Regex CompleteVariable();

	[GeneratedRegex(@"(?:^|\s)(\S+)$")]
	public static partial Regex CompleteWord();

	[GeneratedRegex(@"ErrorActionPreference.*Stop:\s*(.*)")]
	public static partial Regex ErrorActionPreference();

	[GeneratedRegex(@"\s+")]
	public static partial Regex Spaces();
}
