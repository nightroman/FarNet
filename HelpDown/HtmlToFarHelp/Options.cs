
// Copyright (c) Roman Kuzmin
// http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections;
using System.Data.Common;

namespace HtmlToFarHelp
{
	struct Options
	{
		public bool CenterHeading { get; private set; }
		public bool HighlightListBullet { get; private set; }
		public bool HighlightListNumber { get; private set; }
		public bool ListItemEmptyLine { get; private set; }
		public bool PlainCode { get; private set; }
		public bool PlainHeading { get; private set; }
		public int EmptyLinesAfterHeading { get; private set; }
		public int EmptyLinesBeforeHeading { get; private set; }
		public int EmptyLinesBeforeTopic { get; private set; }
		public int IndentCode { get; private set; }
		public int IndentList { get; private set; }
		public int IndentPara { get; private set; }
		public int IndentQuote { get; private set; }
		public int Margin { get; private set; }
		public string Language { get; private set; }
		public string PluginContents { get; private set; }
		public string TopicHeading { get; private set; }
		public string[] ListBullet { get; private set; }

		public const string DefaultBullet = "•";
		static readonly char[] BulletSeparator = new char[] { ' ' };

		public static Options CreateDefault()
		{
			return new Options()
			{
				EmptyLinesAfterHeading = 1,
				EmptyLinesBeforeHeading = 1,
				EmptyLinesBeforeTopic = 1,
				IndentCode = 4,
				IndentList = 2,
				IndentQuote = 4,
				Language = "English,English",
				Margin = 1,
				TopicHeading = "h6",
			};
		}

		static string ParseTopicHeading(string value)
		{
			if (value.Length != 2 || value[0] != 'h' || value[1] < '1' || value[1] > '6')
				throw new InvalidOperationException("TopicHeading must be h1 - h6");
			return value;
		}

		static string[] ParseListBullet(string value)
		{
			var parts = value.Split(BulletSeparator, StringSplitOptions.RemoveEmptyEntries);
			return parts.Length > 0 ? parts : null;
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
						case "emptylinesafterheading": options.EmptyLinesAfterHeading = int.Parse(value); break;
						case "emptylinesbeforeheading": options.EmptyLinesBeforeHeading = int.Parse(value); break;
						case "emptylinesbeforetopic": options.EmptyLinesBeforeTopic = int.Parse(value); break;
						case "highlightlistbullet": options.HighlightListBullet = bool.Parse(value); break;
						case "highlightlistnumber": options.HighlightListNumber = bool.Parse(value); break;
						case "indentcode": options.IndentCode = int.Parse(value); break;
						case "indentlist": options.IndentList = int.Parse(value); break;
						case "indentpara": options.IndentPara = int.Parse(value); break;
						case "indentquote": options.IndentQuote = int.Parse(value); break;
						case "language": options.Language = value; break;
						case "listbullet": options.ListBullet = ParseListBullet(value); break;
						case "listitememptyline": options.ListItemEmptyLine = bool.Parse(value); break;
						case "margin": options.Margin = int.Parse(value); break;
						case "plaincode": options.PlainCode = bool.Parse(value); break;
						case "plainheading": options.PlainHeading = bool.Parse(value); break;
						case "plugincontents": options.PluginContents = value; break;
						case "topicheading": options.TopicHeading = ParseTopicHeading(value); break;
						default: throw new FormatException($"Unknown option: '{it.Key}'.");
					}
				}
			}
			catch (Exception exn)
			{
				throw new FormatException($"Error on parsing HLF options: {exn.Message}", exn);
			}

			return options;
		}
	}
}
