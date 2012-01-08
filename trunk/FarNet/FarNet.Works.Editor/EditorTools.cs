
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
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
			return EditText(text, title, false);
		}
		public static string EditText(string text, string title, bool locked)
		{
			var file = Far.Net.TempName();
			try
			{
				if (!string.IsNullOrEmpty(text))
					File.WriteAllText(file, text, Encoding.Unicode);

				var editor = Far.Net.CreateEditor();
				editor.FileName = file;
				editor.DisableHistory = true;
				if (!string.IsNullOrEmpty(title))
					editor.Title = title;
				if (locked)
					editor.IsLocked = true;
				
				editor.Open(OpenMode.Modal);
				if (locked)
				{
					File.Delete(file);
					return null;
				}

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
		#region Menus
		const string MenuHotkeys = "0123456789abcdefghijklmnopqrstuvwxyz";
		const string MenuItemFormat = "&{0}. {1}";
		public static void ShowEditorsMenu()
		{
			var menu = Far.Net.CreateMenu();
			menu.HelpTopic = "MenuEditors";
			menu.Title = "Editors";

			int index = -1;
			foreach (var it in Far.Net.Editors())
			{
				++index;
				var name = string.Format(null, MenuItemFormat, (index < MenuHotkeys.Length ? MenuHotkeys.Substring(index, 1) : " "), it.FileName);
				menu.Add(name).Data = it;
			}

			if (menu.Show())
				((IEditor)menu.SelectedData).Activate();
		}
		public static void ShowViewersMenu()
		{
			var menu = Far.Net.CreateMenu();
			menu.HelpTopic = "MenuViewers";
			menu.Title = "Viewers";

			int index = -1;
			foreach (var it in Far.Net.Viewers())
			{
				++index;
				var name = string.Format(null, MenuItemFormat, (index < MenuHotkeys.Length ? MenuHotkeys.Substring(index, 1) : " "), it.FileName);
				menu.Add(name).Data = it;
			}

			if (menu.Show())
				((IViewer)menu.SelectedData).Activate();
		}
		#endregion
	}
}
