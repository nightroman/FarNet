using System.Text.RegularExpressions;

namespace FarNet.Works;
#pragma warning disable 1591

// Methods for incremental filter with "and" parts separated by bullets.
public static class BulletFilter
{
	// Bullet
	const char Separator = '\x2022';

	// Adds a bullet as a new filter part, if not yet.
	public static string AddBullet(string filter)
	{
		if (filter.Length == 0 || filter[^1] == Separator)
			return filter;
		else
			return filter + Separator;
	}

	// Converts the filter to predicate.
	public static Predicate<string>? ToPredicate(string filter, PatternOptions options)
	{
		if (options == 0 || string.IsNullOrEmpty(filter))
			return null;

		var parts = filter.Split(Separator);

		bool isRegex = !options.HasFlag(PatternOptions.Literal) && parts.Any(s => s.Contains('*'));
		if (isRegex)
		{
			return ToRegex(parts, options).IsMatch;
		}
		else if (!options.HasFlag(PatternOptions.Prefix))
		{
			return s => parts.All(p => s.Contains(p, StringComparison.OrdinalIgnoreCase));
		}
		else
		{
			return s => s.StartsWith(parts[0], StringComparison.OrdinalIgnoreCase) && (parts.Length == 1 || parts.Skip(1).All(p => s.Contains(p, StringComparison.OrdinalIgnoreCase)));
		}
	}

	// Converts the filter to regex.
	private static Regex ToRegex(string[] parts, PatternOptions options)
	{
		var sum = string.Empty;

		for (int i = 0; i < parts.Length; i++)
		{
			var part = parts[i];
			if (part.Length == 0)
				continue;

			// literal or wildcard pattern
			var pattern = (options & PatternOptions.Literal) != 0 ? Regex.Escape(part) : WildcardToRegex(part);

			// prefix? add start anchor
			if (i == 0 && (options & PatternOptions.Prefix) != 0)
				pattern = "^" + pattern;

			// parts? make look-ahead
			if (parts.Length > 1)
				pattern = $"(?=.*?{pattern})";

			sum += pattern;
		}

		return new Regex(sum, RegexOptions.IgnoreCase);
	}

	// Converts the simple wildcard (*) to its regex pattern.
	static string WildcardToRegex(string wildcard)
	{
		return Regex.Escape(wildcard).Replace(@"\*", ".*?");
	}
}
