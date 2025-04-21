using System.Text.RegularExpressions;

namespace RightWords;

static class Kit
{
	public static ReadOnlySpan<char> CleanWord(ReadOnlySpan<char> word, Regex? remove)
	{
		if (remove is null)
			return word;

		if (!remove.IsMatch(word))
			return word;

		return remove.Replace(word.ToString(), string.Empty);
	}

	public static Match? MatchCaretOrNext(Regex regex, string input, int caret)
	{
		Match match = regex.Match(input);
		while (match.Success)
		{
			if (caret > match.Index + match.Length)
				match = match.NextMatch();
			else
				return match;
		}
		return null;
	}

	public static ValueMatch MatchCaret(Regex regex, ReadOnlySpan<char> input, int caret)
	{
		foreach (var match in regex.EnumerateMatches(input))
		{
			if (caret > match.Index + match.Length)
				continue;
			else if (caret < match.Index)
				return default;
			else
				return match;
		}
		return default;
	}

	public static bool HasMatch(MatchCollection? matches, int index, int length)
	{
		if (matches is { })
		{
			foreach (Match m in matches)
			{
				if (index >= m.Index && index + length <= m.Index + m.Length)
					return true;
			}
		}

		return false;
	}

	public static MatchCollection? GetMatches(Regex? regex, string text)
	{
		return regex?.Matches(text);
	}
}
