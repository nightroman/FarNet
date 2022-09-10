
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

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

	// Converts the filter to regex.
	public static Regex? ToRegex(string filter, PatternOptions options)
	{
		if (options == 0 || string.IsNullOrEmpty(filter))
			return null;

		var sum = string.Empty;
		var parts = filter.Split(Separator);

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
