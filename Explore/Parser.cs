using FarNet;
using System.Text;

namespace Explore;

public static class Parser
{
	// 2025-02-09-1643
	public static IList<string> Tokenize(string text, string? last = null)
	{
		var list = new List<string>();
		var reader = new StringReader(text);
		var sb = new StringBuilder(text.Length);
		bool quote = false;
		for (; ; )
		{
			int n = reader.Read();

			// end of text
			if (n < 0)
			{
				// error on quote
				if (quote)
					throw new InvalidOperationException("Expected '\"', found end of text.");

				// add the token
				if (sb.Length > 0)
					list.Add(sb.ToString());

				// done
				return list;
			}

			// white space
			if (n < 33)
			{
				// quote, add the space
				if (quote)
				{
					sb.Append((char)n);
					continue;
				}

				// token started, complete
				if (sb.Length > 0)
				{
					// add the token
					var token = sb.ToString();
					list.Add(token);

					// the last token? add the rest of text
					if (string.Equals(token, last, StringComparison.OrdinalIgnoreCase))
					{
						list.Add(reader.ReadToEnd().Trim());
						return list;
					}

					// reset
					sb.Length = 0;
					continue;
				}

				// skip white space
				continue;
			}

			// quote
			if (n == '"')
			{
				// started
				if (quote)
				{
					// double quote, add one
					if (reader.Peek() == '"')
					{
						sb.Append('"');
						reader.Read();
						continue;
					}

					// single, stop quote, add token
					quote = false;
					list.Add(sb.ToString());
					sb.Length = 0;
					continue;
				}

				// start quote
				quote = true;
				continue;
			}

			// add the char
			sb.Append((char)n);
		}
	}

	// 2025-02-09-1643
	public static string? ResolveName(string part, string[] names)
	{
		var matches = names.Where(name => name.StartsWith(part, StringComparison.OrdinalIgnoreCase)).ToList();
		return matches.Count switch
		{
			1 => matches[0],
			0 => null,
			_ => throw new ModuleException("Cannot resolve name: " + part)
		};
	}
}
