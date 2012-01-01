
/*
FarNet module CopyColor
Copyright (c) 2011-2012 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace FarNet.CopyColor
{
	[System.Runtime.InteropServices.Guid("e9a7fa32-15e0-4f19-b004-9148df346794")]
	[ModuleTool(Name = TheTool.ModuleName, Options = ModuleToolOptions.Editor)]
	public class TheTool : ModuleTool
	{
		const string ModuleName = "CopyColor";
		readonly string[] Colors = { "#000000", "#000080", "#008000", "#008080", "#800000", "#800080", "#808000", "#c0c0c0", "#808080", "#0000ff", "#00ff00", "#00ffff", "#ff0000", "#ff00ff", "#ffff00", "#ffffff", };
		static string EncodeHtml(string html)
		{
			return html.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
		}
		public override void Invoke(object sender, ModuleToolEventArgs e)
		{
			var editor = Far.Net.Editor;

			int iLine1, iLine2;
			if (editor.SelectionExists)
			{
				var rect = editor.SelectionPlace;
				iLine1 = rect.First.Y;
				iLine2 = rect.Last.Y;
				if (rect.Last.X < 0)
					--iLine2;
			}
			else
			{
				iLine1 = editor.Caret.Y;
				iLine2 = iLine1;
			}

			var linetexts = new List<string>();
			var linespans = new List<ColorSpan[]>();
			var bgcount = new int[16];

			for (int line = iLine1; line <= iLine2; ++line)
			{
				var text = editor[line].Text;
				linetexts.Add(text);

				var colors = new ColorSpan[text.Length];
				linespans.Add(colors);

				var spans = editor.GetColors(line);
				if (spans == null || spans.Count == 0 || spans[0].Start > 0 || spans[0].End < text.Length)
				{
					Far.Net.Message(@"
Cannot copy: part of the selected test has no colors.
Try to scroll the text. Long lines are not supported.
", ModuleName);
					return;
				}

				foreach (var span in spans)
				{
					for (int ch = span.Start; ch < span.End; ++ch)
					{
						++bgcount[(int)span.Background];
						if (ch < text.Length)
							colors[ch] = span;
					}
				}
			}

			int bgindex = Array.IndexOf(bgcount, bgcount.Max());
			var bgcolor = (ConsoleColor)bgindex;

			var sb = new StringBuilder();
			sb.AppendFormat(null, "<div style='background:{0}'><pre>", Colors[bgindex]);
			sb.AppendLine();

			for (int line = 0; line < linetexts.Count; ++line)
			{
				var text = linetexts[line];
				var colors = linespans[line];

				for (int start = 0; start < text.Length; )
				{
					var color = colors[start];

					int end = start + 1;
					while (end < text.Length && colors[end].Background == color.Background && colors[end].Foreground == color.Foreground)
						++end;

					var html = EncodeHtml(text.Substring(start, end - start));
					if (color.Background == bgcolor)
						sb.AppendFormat(null, "<span style='color:{0}'>{1}</span>",
							Colors[(int)color.Foreground], html);
					else
						sb.AppendFormat(null, "<span style='color:{0}; background:{1}'>{2}</span>",
							Colors[(int)color.Foreground], Colors[(int)color.Background], html);

					start = end;
				}

				sb.AppendLine();
			}

			sb.AppendLine("</pre></div>");

			ClipboardHelper.SetHtml(sb.ToString());
		}
	}
}
