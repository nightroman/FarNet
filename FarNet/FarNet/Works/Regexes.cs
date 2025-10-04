using System.Text.RegularExpressions;

namespace FarNet.Works;
#pragma warning disable 1591

public static partial class Regexes
{
	[GeneratedRegex(@"[\t\r\n]+")]
	public static partial Regex TabsAndNewLines();
}
