
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System;
using FarNet;

namespace PowerShellFar.UI
{
	class CommandHistoryMenu
	{
		IListMenu _menu;

		public CommandHistoryMenu(string prefix)
		{
			_menu = Far.Api.CreateListMenu();
			Settings.Default.ListMenu(_menu);

			_menu.HelpTopic = Far.Api.GetHelpTopic("MenuCommandHistory");
			_menu.SelectLast = true;
			_menu.Title = "PowerShell history";

			_menu.Incremental = prefix;
			_menu.IncrementalOptions = PatternOptions.Substring;

			_menu.AddKey(KeyCode.R, ControlKeyStates.LeftCtrlPressed, OnDelete);
			_menu.AddKey(KeyCode.Delete, ControlKeyStates.None, OnDelete);
		}
		void ResetItems(string[] lines)
		{
			_menu.Items.Clear();
			foreach (string s in lines)
			{
				if (string.IsNullOrEmpty(_menu.Incremental) || s.StartsWith(_menu.Incremental, StringComparison.OrdinalIgnoreCase))
					_menu.Add(s);
			}
		}
		void OnDelete(object sender, MenuEventArgs e)
		{
			var lines = History.Update(null);
			if (lines.Length == _menu.Items.Count)
			{
				e.Ignore = true;
			}
			else
			{
				e.Restart = true;
				_menu.Selected = -1;

				ResetItems(lines);
			}
		}
		public string Show()
		{
			// fill
			ResetItems(History.ReadLines());

			// show
			if (!_menu.Show())
				return null;

			// selected
			return _menu.Items[_menu.Selected].Text;
		}
	}
}
