
// Copyright (c) Roman Kuzmin
// http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections;
using System.Data.Common;

namespace HtmlToFarHelp
{
	struct Options
	{
		public bool CenterHeading;
		public bool PlainCode;
		public bool PlainHeading;
		public int IndentCode;
		public int IndentList;
		public int IndentPara;
		public int IndentQuote;
		public int Margin;
		public string Language;
		public string PluginContents;
		public string TopicHeading;

		public static Options New()
		{
			return new Options()
			{
				Margin = 1,
				IndentCode = 4,
				IndentList = 2,
				IndentQuote = 4,
				Language = "English,English",
				TopicHeading = "h6"
			};
		}

		static string ParseTopicHeading(string value)
		{
			if (value.Length != 2 || value[0] != 'h' || value[1] < '1' || value[1] > '6')
				throw new InvalidOperationException("TopicHeading must be h1 - h6");
			return value;
		}

		public static Options Parse(Options options, string optionString)
		{
			try
			{
				var builder = new DbConnectionStringBuilder
				{
					ConnectionString = optionString
				};

				foreach (DictionaryEntry it in (IDictionary)builder)
				{
					var value = it.Value.ToString();
					switch (it.Key.ToString())
					{
						case "centerheading": options.CenterHeading = bool.Parse(value); break;
						case "indentcode": options.IndentCode = int.Parse(value); break;
						case "indentlist": options.IndentList = int.Parse(value); break;
						case "indentpara": options.IndentPara = int.Parse(value); break;
						case "indentquote": options.IndentQuote = int.Parse(value); break;
						case "language": options.Language = value; break;
						case "margin": options.Margin = int.Parse(value); break;
						case "plaincode": options.PlainCode = bool.Parse(value); break;
						case "plainheading": options.PlainHeading = bool.Parse(value); break;
						case "plugincontents": options.PluginContents = value; break;
						case "topicheading": options.TopicHeading = ParseTopicHeading(value); break;
						default: throw new ArgumentException("Unknown option: " + it.Key);
					}
				}
			}
			catch (Exception e)
			{
				throw new FormatException("Error on parsing HLF options: " + e.Message, e);
			}

			return options;
		}
	}
}
