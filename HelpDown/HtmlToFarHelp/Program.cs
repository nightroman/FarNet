
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
		const string Usage = @"
Input: {0}
Error: {1}

Usage:
    HtmlToFarHelp key=value ...
    HtmlToFarHelp ""key = value; ...""

Input:
    from = input HTML file
    to = output HLF file
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
						default: throw new Exception($"Unknown key: '{it.Key}'.");
					}
				}

				if (from == null) throw new Exception("Missing required `from=<input-file>`");
				if (to == null) throw new Exception("Missing required `to=<output-file>`");
			}
			catch (Exception exn)
			{
				Console.Error.WriteLine(string.Format(null, Usage, parameters, exn.Message));
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
					var converter = new Converter(from, reader, writer);
					converter.Run();
				}

				return 0;
			}
			catch (Exception exn)
			{
				Console.Error.WriteLine(exn.Message);
				return 1;
			}
		}
	}
}
