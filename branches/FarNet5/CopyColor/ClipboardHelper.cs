
/*
FarNet module CopyColor
Copyright (c) 2011-2012 Roman Kuzmin
*/

// HTML formatting code comes from:
// http://blog.tcx.be/2005/08/copying-html-fragment-to-clipboard.html

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
namespace FarNet.CopyColor
{
	public static class ClipboardHelper
	{
		public static void CopyHtmlToClipboard(string html)
		{
			Encoding encoding = Encoding.UTF8;

			string begin = "Version:0.9\r\nStartHTML:{0:000000}\r\nEndHTML:{1:000000}"
			+ "\r\nStartFragment:{2:000000}\r\nEndFragment:{3:000000}\r\n";

			string html_begin = "<html>\r\n<head>\r\n"
			+ "<meta http-equiv=\"Content-Type\""
			+ " content=\"text/html; charset=" + encoding.WebName + "\">\r\n"
			+ "<title>HTML clipboard</title>\r\n</head>\r\n<body>\r\n"
			+ "<!--StartFragment-->";

			string html_end = "<!--EndFragment-->\r\n</body>\r\n</html>\r\n";

			string begin_sample = String.Format(begin, 0, 0, 0, 0);

			int count_begin = encoding.GetByteCount(begin_sample);
			int count_html_begin = encoding.GetByteCount(html_begin);
			int count_html = encoding.GetByteCount(html);
			int count_html_end = encoding.GetByteCount(html_end);

			string html_total = String.Format(
			begin
			, count_begin
			, count_begin + count_html_begin + count_html + count_html_end
			, count_begin + count_html_begin
			, count_begin + count_html_begin + count_html
			) + html_begin + html + html_end;

			using (var stream = new MemoryStream(encoding.GetBytes(html_total)))
			{
				DataObject data = new DataObject();
				data.SetData(DataFormats.Html, stream);
				Clipboard.SetDataObject(data, true);
			}
		}
		public static void SetHtml(string text)
		{
			Work(delegate
			{
				if (string.IsNullOrEmpty(text))
					Clipboard.Clear();
				else
					CopyHtmlToClipboard(text);
			});
		}
		static void Work(ThreadStart job)
		{
			var thread = new Thread(job);
			thread.SetApartmentState(ApartmentState.STA);

			thread.Start();
			thread.Join();
		}
	}
}
