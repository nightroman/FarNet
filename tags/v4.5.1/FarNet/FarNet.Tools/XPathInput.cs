
/*
FarNet.Tools library for FarNet
Copyright (c) 2010 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace FarNet.Tools
{
	/// <summary>
	/// XPath input helper.
	/// </summary>
	public class XPathInput
	{
		/// <summary>
		/// Gets the XPath expression.
		/// </summary>
		public string Expression { get; private set; }
		/// <summary>
		/// Gets the XPath variables.
		/// </summary>
		public Dictionary<string, object> Variables { get; private set; }
		/// <summary>
		/// Parses the XPath file.
		/// </summary>
		public static XPathInput ParseFile(string path)
		{
			return Parse(File.ReadAllLines(path, Encoding.Default));
		}
		/// <summary>
		/// Parses the XPath text.
		/// </summary>
		public static XPathInput ParseText(string text)
		{
			return Parse(Works.Kit.SplitLines(text));
		}
		static XPathInput Parse(string[] lines)
		{
			var result = new XPathInput();
			result.Variables = new Dictionary<string, object>();

			var regex1 = new Regex(@"^declare\s+variable\s+\$(\w+)\s+(.*)");
			var regex2 = new Regex(@"^external[;\s]*$");
			var regex3 = new Regex(@"^:=\s*(.*?)[;\s]*$");
			int i;
			bool comment = false;
			for (i = 0; i < lines.Length; ++i)
			{
				var line = lines[i].Trim();

			repeat:

				// empty line
				if (line.Length == 0)
					continue;

				// comment
				if (comment)
				{
					int index = line.IndexOf(":)", StringComparison.Ordinal);
					if (index < 0)
						continue;
					
					line = line.Substring(index + 2).Trim();
					comment = false;
					goto repeat;
				}
				else if (line.StartsWith("(:", StringComparison.Ordinal))
				{
					line = line.Substring(2);
					comment = true;
					goto repeat;
				}

				var match = regex1.Match(line);
				if (!match.Success)
					break;

				var name = match.Groups[1].Value;
				var text = match.Groups[2].Value;
				match = regex2.Match(text);
				if (match.Success)
				{
					// prompt
					text = Far.Net.Input("Variable: " + name, "XPathVariable", "Input variable");
					if (text == null)
					{
						result.Variables.Add(name, string.Empty);
						continue;
					}

					double adouble;
					if (double.TryParse(text, out adouble))
						result.Variables.Add(name, adouble);
					else
						result.Variables.Add(name, text);
					continue;
				}

				match = regex3.Match(text);
				if (!match.Success)
					throw new InvalidOperationException("declare variable: expected 'external' or ':='");

				text = match.Groups[1].Value;
				if (text.StartsWith("'", StringComparison.Ordinal) && text.EndsWith("'", StringComparison.Ordinal) ||
					text.StartsWith("\"", StringComparison.Ordinal) && text.EndsWith("\"", StringComparison.Ordinal))
				{
					result.Variables.Add(name, text.Substring(1, text.Length - 2));
				}
				else
				{
					double adouble;
					if (!double.TryParse(text, out adouble))
						throw new InvalidOperationException("Not supported variable value.");
					result.Variables.Add(name, adouble);
				}
			}

			result.Expression = string.Join(Environment.NewLine, lines, i, lines.Length - i);
			return result;
		}
	}
}
