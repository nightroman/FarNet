
/* Copyright 2012-2014 Roman Kuzmin
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

[assembly: AssemblyVersion("1.0.2")]
[assembly: AssemblyProduct("HtmlToFarHelp")]
[assembly: AssemblyTitle("HtmlToFarHelp")]
[assembly: AssemblyDescription("HtmlToFarHelp - converts HTML to Far Manager help")]
[assembly: AssemblyCompany("http://code.google.com/p/farnet/")]
[assembly: AssemblyCopyright("Copyright (c) 2012-2014 Roman Kuzmin")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

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
				var converter = new Converter();
				using (var reader = new XmlTextReader(from))
				{
					using (var writer = new StreamWriter(to, false, Encoding.UTF8))
					{
						converter.Reader = reader;
						converter.Writer = writer;
						converter.Run();
					}
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
