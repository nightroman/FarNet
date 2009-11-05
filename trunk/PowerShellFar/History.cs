/*
PowerShellFar plugin for Far Manager
Copyright (C) 2006-2009 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using FarNet;
using Microsoft.Win32;

namespace PowerShellFar
{
	static class History
	{
		static string[] _Cache;
		/// <summary>
		/// History list used for getting commands by Up/Down.
		/// </summary>
		public static string[] Cache
		{
			get { return History._Cache; }
			set { History._Cache = value; }
		}

		static int _CacheIndex;
		/// <summary>
		/// History list current index.
		/// </summary>
		public static int CacheIndex
		{
			get { return History._CacheIndex; }
			set { History._CacheIndex = value; }
		}

		/// <summary>
		/// Gets history lines.
		/// </summary>
		public static string[] GetLines(int count)
		{
			using (RegistryKey key = Open(false))
			{
				string[] names = key.GetValueNames();
				if (count <= 0 || count > names.Length)
					count = names.Length;

				string[] lines = new string[count];
				for(int s = names.Length - count, d = 0; s < names.Length; ++s, ++d)
					lines[d] = key.GetValue(names[s]).ToString();

				return lines;
			}
		}

		/// <summary>
		/// Add a new history line.
		/// </summary>
		public static void AddLine(string value)
		{
			using (RegistryKey key = Open(true))
				key.SetValue(Kit.ToString(DateTime.Now.Ticks), value);
		}

		/// <summary>
		/// For Actor.
		/// </summary>
		public static void ShowHistory()
		{
			UI.CommandHistoryMenu m = new UI.CommandHistoryMenu(string.Empty);
			string code = m.Show();
			if (code == null)
				return;

			switch (A.Far.WindowType)
			{
				case WindowType.Panels:
					{
						A.Far.CommandLine.Text = Entry.Prefix1 + ": " + code;
						if (!m.Alternative)
							A.Far.PostKeys("Enter", false);
						return;
					}
				case WindowType.Editor:
					{
						IEditor editor = A.Psf.Editor();

						// case: usual editor
						EditorConsole console = editor.Host as EditorConsole;
						if (console == null)
							goto default;

						// case: psfconsole
						editor.GoEnd(true);
						editor.Insert(code);
						if (m.Alternative)
							return;

						console.Invoke();
						return;
					}
				default:
					{
						if (m.Alternative)
						{
							UI.InputDialog ui = new UI.InputDialog(Res.Name, Res.Name, "PowerShell code");
							ui.Edit.Text = code;
							if (!ui.Dialog.Show())
								return;
							code = ui.Edit.Text;
						}

						A.Psf.InvokePipeline(code, null, true);
						return;
					}
			}
		}

		/// <summary>
		/// Removes duplicated history lines.
		/// </summary>
		public static void RemoveDupes()
		{
			using (RegistryKey key = Open(true))
				RemoveDupes(key);
		}

		/// <summary>
		/// Removes duplicated history lines.
		/// </summary>
		static void RemoveDupes(RegistryKey key)
		{
			string[] names = key.GetValueNames();
			Dictionary<string, object> map = new Dictionary<string, object>();
			for (int i = names.Length; --i >= 0; )
			{
				string s = key.GetValue(names[i]).ToString();
				if (map.ContainsKey(s))
					key.DeleteValue(names[i]);
				else
					map.Add(s, null);
			}
		}

		static string _path_;
		/// <summary>
		/// Opens the history key for using.
		/// </summary>
		static RegistryKey Open(bool writable)
		{
			//! 'Console.Title' was used for progress but it can throw IOException; we better do not use it
			if (_path_ == null)
			{
				// get path once
				_path_ = A.Far.RootFar + "\\SavedDialogHistory\\PowerShellFarHistory";

				// ensure the key and reduce the history once
				using (RegistryKey key = Registry.CurrentUser.CreateSubKey(_path_))
				{
					// remove old if the count is "well above" the limit
					int nKill = key.ValueCount - A.Psf.Settings.MaxHistoryCount;
					if (nKill > A.Psf.Settings.MaxHistoryCount / 10)
					{
						RemoveDupes(key);
						nKill = key.ValueCount - A.Psf.Settings.MaxHistoryCount;
						if (nKill > 0)
						{
							foreach (string s in key.GetValueNames())
							{
								key.DeleteValue(s);
								if (--nKill <= 0)
									break;
							}
						}
					}
				}
			}
			
			// the opened key
			return Registry.CurrentUser.OpenSubKey(_path_, writable);
		}

	}
}
