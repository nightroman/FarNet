
/*
FarNet module Explore
Copyright (c) 2010 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FarNet.Explore
{
	public static class Parser
	{
		public static IList<string> Tokenize(string text, params string[] lasts)
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
						foreach(var last in lasts)
						{
							if (string.Equals(token, last, StringComparison.OrdinalIgnoreCase))
							{
								list.Add(reader.ReadToEnd().Trim());
								return list;
							}
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
		public static string ResolveName(string part, string[] names)
		{
			for (int i = names.Length; --i >= 0; )
			{
				var name1 = names[i];
				if (!name1.StartsWith(part, StringComparison.OrdinalIgnoreCase))
					continue;

				if (name1.Length == part.Length)
					return name1;

				for (int j = i; --j >= 0; )
				{
					var name2 = names[j];
					if (!name2.StartsWith(part, StringComparison.OrdinalIgnoreCase))
						continue;
					
					if (name2.Length == part.Length)
						return name2;

					throw new InvalidOperationException("Cannot resolve the name: " + part);
				}

				return name1;
			}

			return null;
		}
	}
}
