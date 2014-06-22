
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

// Encoding: UTF8 with no BOM (same as in logging tools, BinaryFormatter, etc).
// Text line history files are rather logs, not text files for an editor.
// IO.File methods by default work in this way.

using System.Collections.Generic;
using System.IO;
using FarNet;

namespace PowerShellFar
{
	static class History
	{
		/// <summary>
		/// History list used for getting commands by Up/Down.
		/// </summary>
		public static string[] Cache { get; set; }
		/// <summary>
		/// History list current index.
		/// </summary>
		public static int CacheIndex { get; set; }
		static string GetFileName(bool create)
		{
			return A.Psf.Manager.GetFolderPath(SpecialFolder.LocalData, create) + @"\PowerShellFarHistory.log";
		}
		static void WriteLines(string[] lines)
		{
			File.WriteAllLines(GetFileName(true), lines);
		}
		/// <summary>
		/// Gets history lines.
		/// </summary>
		public static string[] ReadLines()
		{
			// get lines
			try
			{
				var lines = File.ReadAllLines(GetFileName(false));
				if (lines.Length > Settings.Default.MaximumHistoryCount + Settings.Default.MaximumHistoryCount / 10)
					return Update(lines);
				else
					return lines;
			}
			catch (FileNotFoundException)
			{
				return new string[0];
			}
		}
		/// <summary>
		/// Removes dupes and extra lines.
		/// </summary>
		public static string[] Update(string[] lines)
		{
			// ensure lines
			if (lines == null)
				lines = ReadLines();

			// copy lines
			var list = new List<string>(lines);

			// remove dupes
			var hash = new HashSet<string>();
			for (int i = lines.Length; --i >= 0; )
			{
				var line = lines[i];
				if (!hash.Add(line))
					list.RemoveAt(i);
			}

			// remove lines above the limit
			int removeCount = list.Count - Settings.Default.MaximumHistoryCount;
			if (removeCount > 0)
				list.RemoveRange(0, removeCount);

			// return the same lines
			if (lines.Length == list.Count)
				return lines;

			// write and return new lines
			lines = list.ToArray();
			WriteLines(lines);
			return lines;
		}
		/// <summary>
		/// Add a new history line.
		/// </summary>
		public static void AddLine(string value)
		{
			using (var writer = File.AppendText(GetFileName(true)))
				writer.WriteLine(value);
		}
		/// <summary>
		/// For Actor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "PowerShellFar")]
		public static void ShowHistory()
		{
			var m = new UI.CommandHistoryMenu(string.Empty);
			string code = m.Show();
			if (code == null)
				return;

			// insert to command lines
			switch (Far.Api.Window.Kind)
			{
				case WindowKind.Panels:
					Far.Api.CommandLine.Text = Entry.CommandInvoke1.Prefix + ": " + code;
					return;
				case WindowKind.Editor:
					var editor = Far.Api.Editor;
					if (!(editor.Host is EditorConsole))
						break;
					editor.GoToEnd(true);
					editor.InsertText(code);
					editor.Redraw();
					return;
				case WindowKind.Dialog:
					var dialog = Far.Api.Dialog;
					var typeId = dialog.TypeId;
					if (typeId != UI.InputConsole.TypeId && typeId != UI.InputDialog.TypeId)
						break;
					var line = Far.Api.Line;
					if (line == null || line.IsReadOnly)
						break;
					line.Text = code;
					return;
			}

			// show "Invoke commands"
			var ui = new UI.InputDialog() { Caption = Res.Me, History = Res.History, Prompt = new string[] { Res.InvokeCommands }, Text = code };
			if (!ui.Show())
				return;

			// invoke input
			A.Psf.Act(ui.Text, null, true);
		}
		public static string GetNextCommand(bool up, string current)
		{
			string lastUsed = null;

			if (History.Cache == null)
			{
				lastUsed = current;
				History.Cache = History.ReadLines();
				History.CacheIndex = History.Cache.Length;
			}
			else if (History.CacheIndex >= 0 && History.CacheIndex < History.Cache.Length)
			{
				lastUsed = History.Cache[History.CacheIndex];
			}

			if (up)
			{
				for (; ; )
				{
					if (--History.CacheIndex < 0)
					{
						History.CacheIndex = -1;
						return string.Empty;
					}
					else
					{
						var command = History.Cache[History.CacheIndex];
						if (command != lastUsed)
							return command;
					}
				}
			}
			else
			{
				for (; ; )
				{
					if (++History.CacheIndex >= History.Cache.Length)
					{
						History.CacheIndex = History.Cache.Length;
						return string.Empty;
					}
					else
					{
						var command = History.Cache[History.CacheIndex];
						if (command != lastUsed)
							return command;
					}
				}
			}
		}
	}
}
