
// Copyright (c) Roman Kuzmin
// http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Xml;

namespace HtmlToFarHelp
{
	class Program
	{
		const string Usage = @"Error: {0}
{1}

Usage:
  HtmlToFarHelp.exe key=value ...
  HtmlToFarHelp.exe ""key = value; ...""

Keys:
  From = Input HTML file
  To   = Output HLF file
";
		static int Main(string[] args)
		{
			var parameters = string.Join("; ", args);
			string from = null;
			string to = null;
			try
			{
				var builder = new DbConnectionStringBuilder() { ConnectionString = parameters };
				foreach (DictionaryEntry it in (IDictionary)builder)
				{
					switch (it.Key.ToString())
					{
						case "from": from = it.Value.ToString(); break;
						case "to": to = it.Value.ToString(); break;
						default: throw new ArgumentException("Unknown key: " + it.Key);
					}
				}

				if (from == null) throw new ArgumentException("Missing key 'From'.");
				if (to == null) throw new ArgumentException("Missing key 'To'.");
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(string.Format(null, Usage, e.Message, parameters));
				return 1;
			}

			try
			{
				var settings = new XmlReaderSettings
				{
					ConformanceLevel = ConformanceLevel.Auto,
					// net40
					DtdProcessing = DtdProcessing.Ignore,
					//! pandoc
					XmlResolver = null
				};

				using (var reader = XmlReader.Create(from, settings))
				using (var writer = new StreamWriter(to, false, Encoding.UTF8))
				{
					var converter = new Converter
					{
						Reader = reader,
						Writer = writer
					};
					converter.Run();
				}

				return 0;
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e.Message);
				return 1;
			}
		}
	}
}
