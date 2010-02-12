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
			_menu = Far.Host.CreateListMenu();
			A.Psf.Settings.ListMenu(_menu);

			_menu.HelpTopic = A.Psf.HelpTopic + "MenuCommandHistory";
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

		void ResetItems()
		{
			_menu.Items.Clear();
			foreach(string s in History.GetLines(0))
				_menu.Add(s);
		}

		void OnDelete(object sender, MenuEventArgs e)
		{
			int n1 = _menu.Items.Count;
			History.RemoveDupes();
			ResetItems();
			if (n1 == _menu.Items.Count)
			{
				e.Ignore = true;
			}
			else
			{
				e.Restart = true;
				_menu.Selected = -1;
			}
		}

		public string Show()
		{
			// fill
			ResetItems();

			// show
			if (!_menu.Show())
				return null;

			// selected
			Alternative = _menu.BreakKey == (KeyMode.Ctrl | KeyCode.Enter);
			return _menu.Items[_menu.Selected].Text;
		}
	}
}
