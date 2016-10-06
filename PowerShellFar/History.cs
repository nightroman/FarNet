
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2016 Roman Kuzmin
*/

// Encoding: UTF8 with no BOM (same as in logging tools, BinaryFormatter, etc).
// Text line history files are rather logs, not text files for an editor.
// IO.File methods by default work in this way.

using FarNet;
using System.Collections.Generic;
using System.IO;

namespace PowerShellFar
{
	static class History
	{
		static string[] Cache;
		internal static void DropCache()
		{
			Cache = null;
		}
		/// <summary>
		/// History list current index.
		/// </summary>
		static int CacheIndex;
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
			for (int i = lines.Length; --i >= 0;)
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
					if (!(editor.Host is Interactive))
						break;
					editor.GoToEnd(true);
					editor.InsertText(code);
					editor.Redraw();
					return;
				case WindowKind.Dialog:
					var dialog = Far.Api.Dialog;
					var typeId = dialog.TypeId;
					if (typeId != UI.InputDialog.TypeId)
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

			if (Cache == null)
			{
				lastUsed = current;
				Cache = ReadLines();
				CacheIndex = Cache.Length;
			}
			else if (CacheIndex >= 0 && CacheIndex < Cache.Length)
			{
				lastUsed = Cache[CacheIndex];
			}

			if (up)
			{
				for (;;)
				{
					if (--CacheIndex < 0)
					{
						CacheIndex = -1;
						return string.Empty;
					}
					else
					{
						var command = Cache[CacheIndex];
						if (command != lastUsed)
							return command;
					}
				}
			}
			else
			{
				for (;;)
				{
					if (++CacheIndex >= Cache.Length)
					{
						CacheIndex = Cache.Length;
						return string.Empty;
					}
					else
					{
						var command = Cache[CacheIndex];
						if (command != lastUsed)
							return command;
					}
				}
			}
		}
	}
}
