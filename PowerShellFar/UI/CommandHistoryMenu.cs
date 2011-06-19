
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using FarNet;

namespace PowerShellFar.UI
{
	class CommandHistoryMenu
	{
		IListMenu _menu;
		internal bool Alternative;

		public CommandHistoryMenu(string filter)
		{
			_menu = Far.Net.CreateListMenu();
			Settings.Default.ListMenu(_menu);

			_menu.HelpTopic = Far.Net.GetHelpTopic("MenuCommandHistory");
			_menu.SelectLast = true;
			_menu.Title = "PowerShellFar History";

			_menu.Filter = filter;
			_menu.FilterHistory = "PowerShellFarFilterHistory";
			_menu.FilterRestore = true;
			_menu.IncrementalOptions = PatternOptions.Substring;

			_menu.AddKey(KeyMode.Ctrl | KeyCode.Enter);
			_menu.AddKey(KeyMode.Ctrl | 'R', OnDelete);
			_menu.AddKey(KeyCode.Del, OnDelete);
		}

		void ResetItems(string[] lines)
		{
			_menu.Items.Clear();
			foreach(string s in lines)
				_menu.Add(s);
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
			Alternative = _menu.BreakKey == (KeyMode.Ctrl | KeyCode.Enter);
			return _menu.Items[_menu.Selected].Text;
		}
	}
}
