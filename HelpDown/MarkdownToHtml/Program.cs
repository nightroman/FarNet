
// Copyright 2012-2016 Roman Kuzmin
// http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using MarkdownDeep;

[assembly: AssemblyVersion("1.0.2")]
[assembly: AssemblyProduct("MarkdownToHtml")]
[assembly: AssemblyTitle("MarkdownToHtml")]
[assembly: AssemblyDescription("MarkdownToHtml - converts markdown to HTML")]
[assembly: AssemblyCompany("https://github.com/nightroman/FarNet")]
[assembly: AssemblyCopyright("Copyright (c) 2012-2016 Roman Kuzmin")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

namespace MarkdownToHtml
{
	class Program
	{
		const string Usage = @"Error: {0}
{1}

Usage:
  MarkdownToHtml.exe key=value ...
  MarkdownToHtml.exe ""key = value; ...""

Keys:
  From  = Input markdown file
  To    = Output HTML file
  Title = Optional HTML title
";
		static int Main(string[] args)
		{
			var parameters = string.Join("; ", args);
			string from = null;
			string to = null;
			string title = null;
			try
			{
				var builder = new DbConnectionStringBuilder() { ConnectionString = parameters };
				foreach (DictionaryEntry it in (IDictionary)builder)
				{
					switch (it.Key.ToString())
					{
						case "from": from = it.Value.ToString(); break;
						case "to": to = it.Value.ToString(); break;
						case "title": title = it.Value.ToString(); break;
						default: throw new ArgumentException("Unknown key: " + it.Key);
					}
				}

				if (from == null) throw new ArgumentException("Missing key 'From'.");
				if (to == null) throw new ArgumentException("Missing key 'To'.");
				if (title == null) title = Path.GetFileNameWithoutExtension(from);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(string.Format(null, Usage, e.Message, parameters));
				return 1;
			}

			try
			{
				var text = File.ReadAllText(from).Replace("\r\n", "\n");

				var markdown = new Markdown();
				markdown.ExtraMode = true;

				var html = markdown.Transform(text);

				using (var writer = new StreamWriter(to, false, Encoding.UTF8))
				{
					writer.Write("<html><title>{0}</title><body>\n", title);
					writer.Write(html);
					writer.Write("</body></html>\n");
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
