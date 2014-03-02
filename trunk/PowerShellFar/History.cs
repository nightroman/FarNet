
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
			UI.CommandHistoryMenu m = new UI.CommandHistoryMenu(string.Empty);
			string code = m.Show();
			if (code == null)
				return;

			switch (Far.Api.Window.Kind)
			{
				case WindowKind.Panels:
					{
						Far.Api.CommandLine.Text = Entry.CommandInvoke1.Prefix + ": " + code;
						if (!m.Alternative)
							Far.Api.PostMacro("Keys('Enter')", true, false);
						return;
					}
				case WindowKind.Editor:
					{
						IEditor editor = A.Psf.Editor();

						// case: usual editor
						EditorConsole console = editor.Host as EditorConsole;
						if (console == null)
							goto default;

						// case: psfconsole
						editor.GoToEnd(true);
						editor.InsertText(code);
						if (m.Alternative)
							return;

						console.Invoke();
						return;
					}
				default:
					{
						if (Far.Api.UI.IsCommandMode)
						{
							var line = Far.Api.Line;
							if (line != null)
							{
								line.Text = code;
								line.Caret = -1;
							}
							return;
						}

						if (m.Alternative)
						{
							UI.InputDialog ui = new UI.InputDialog(Res.Me, Res.History, "PowerShell code");
							ui.UIEdit.Text = code;
							if (!ui.UIDialog.Show())
								return;
							code = ui.UIEdit.Text;
						}

						A.Psf.Act(code, null, true);
						return;
					}
			}
		}
	}
}
