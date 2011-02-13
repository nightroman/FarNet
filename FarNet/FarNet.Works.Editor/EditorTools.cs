
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FarNet.Works
{
	public static class EditorTools
	{
		public static IEnumerable<ILine> EnumerateLines(IEditor editor, int start, int end)
		{
			for (int i = start; i < end; ++i)
				yield return editor[i];
		}

		public static IEnumerable<string> EnumerateStrings(IEditor editor, int start, int end)
		{
			for (int i = start; i < end; ++i)
				yield return editor[i].Text;
		}

		public static string EditText(string text, string title)
		{
			var file = Far.Net.TempName();
			try
			{
				if (!string.IsNullOrEmpty(text))
					File.WriteAllText(file, text, Encoding.Unicode);

				var edit = Far.Net.CreateEditor();
				edit.FileName = file;
				edit.DisableHistory = true;
				if (!string.IsNullOrEmpty(title))
					edit.Title = title;
				edit.Open(OpenMode.Modal);

				if (File.Exists(file))
				{
					// read and delete
					var r = File.ReadAllText(file, Encoding.Default);
					try
					{
						File.Delete(file);
					}
					catch (IOException e)
					{
						Log.TraceException(e);
					}
					return r;
				}
				else
				{
					// no file, e.g. the text was empty and user exits without saving; case 080502
					return string.Empty;
				}
			}
			finally
			{
				File.Delete(file);
			}
		}

		/*
		Issue [_090219_121638] On switching to editor the temp file is not deleted;
		?? Editor in READ event can check existing viewer with DeleteSource::File,
		drop the flag for editor and propagate this option to itself.
		*/
		public static void ViewText(string text, string title, OpenMode mode)
		{
			string tmpfile = Far.Net.TempName();
			File.WriteAllText(tmpfile, text, Encoding.Unicode);

			var viewer = Far.Net.CreateViewer();
			viewer.DeleteSource = DeleteSource.File; // yes, File - we can control it
			viewer.DisableHistory = true;
			viewer.FileName = tmpfile;
			viewer.Switching = Switching.Enabled;
			viewer.Title = title;
			viewer.Open(mode);
		}
	}
}
