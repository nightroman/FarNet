
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
using System.Data.Common;
using System.IO;
using System.Text;
using MarkdownDeep;

namespace MarkdownToHtml
{
	class Program
	{
		const string argFrom = "FROM";
		const string argTo = "TO";
		const string argTitle = "TITLE";
		static int Main(string[] args)
		{
			string from = null;
			string to = null;
			string title = null;
			try
			{
				var builder = new DbConnectionStringBuilder();
				builder.ConnectionString = string.Join(";", args);

				foreach (string key in builder.Keys)
				{
					switch (key.ToUpper())
					{
						case argFrom: from = builder[argFrom].ToString(); break;
						case argTo: to = builder[argTo].ToString(); break;
						case argTitle: title = builder[argTitle].ToString(); break;
						default: throw new ArgumentException("Unknown key: " + key);
					}
				}

				if (from == null) throw new ArgumentException("Missing argument: From=<Markdown file>");
				if (to == null) throw new ArgumentException("Missing argument: To=<HTML file>");
				if (title == null) title = Path.GetFileNameWithoutExtension(from);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("Invalid command line. " + e.Message);
				return 1;
			}

			try
			{
				var text = File.ReadAllText(from);

				var markdown = new Markdown();
				markdown.ExtraMode = true;

				var html = markdown.Transform(text);

				using (var writer = new StreamWriter(to, false, Encoding.UTF8))
				{
					writer.WriteLine("<html><title>{0}</title><body>", title);
					writer.Write(html);
					writer.WriteLine("</body></html>");
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
