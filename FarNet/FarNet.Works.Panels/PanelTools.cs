
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FarNet;

namespace FarNet.Works
{
	public static class PanelTools
	{
		public static void ResizeColumn(Panel panel, bool right)
		{
			if (panel == null) throw new ArgumentNullException("panel");
			
			var view = panel.ViewMode;
			var plan = panel.GetPlan(view) ?? panel.ViewPlan;
			if (plan.Columns.Length != 2)
				return;

			int width = panel.Window.Width - 2;
			plan.Columns[0].Width = 0;
			if (plan.Columns[1].Width == 0)
				plan.Columns[1].Width = width / 2;

			int width2 = plan.Columns[1].Width;
			if (right)
			{
				--width2;
				if (width2 < 1)
					return;
			}
			else
			{
				++width2;
				if (width2 > width - 2)
					return;
			}
			
			plan.Columns[1].Width = width2;
			panel.SetPlan(view, plan);
			panel.Redraw();
		}
		public static void SwitchFullScreen(Panel panel)
		{
			if (panel == null) throw new ArgumentNullException("panel");
			
			// get/make the plan
			var iViewMode = panel.ViewMode;
			var plan = panel.GetPlan(iViewMode) ?? panel.ViewPlan;
	
			// drop widths of text columns
			foreach(var c in plan.Columns)
				if (c.Kind == "N" || c.Kind == "Z" || c.Kind == "O")
					c.Width = 0;

			// switch
			plan.IsFullScreen = !plan.IsFullScreen;

			// set
			panel.SetPlan(iViewMode, plan);
			panel.Redraw();
		}
		const string
			sPushShelveThePanel = "Push/Shelve panel",
			sSwitchFullScreen = "Switch full screen",
			sResizeColum1 = "Decrease left column",
			sResizeColum2 = "Increase left column",
			sClose = "Close panel";
		public static void ShowPanelsMenu()
		{
			var menu = Far.Net.CreateMenu();
			menu.AutoAssignHotkeys = true;
			menu.HelpTopic = "MenuPanels";
			menu.ShowAmpersands = true;
			menu.Title = "Panels";

			menu.AddKey(KeyCode.Delete);
			menu.AddKey(KeyCode.Spacebar);
	
			for(;; menu.Items.Clear())
			{
				IPanel panel = Far.Net.Panel;
				Panel module = null;
		
				// Push/Shelve
				if (panel.IsPlugin)
				{
					module = panel as Panel;
					if (module != null)
					{
						menu.Add(sPushShelveThePanel);
						menu.Add(sResizeColum1);
						menu.Add(sResizeColum2);
						menu.Add(sSwitchFullScreen);
					}

					menu.Add(sClose);
				}
				else if (panel.Kind == PanelKind.File)
				{
					menu.Add(sPushShelveThePanel);
				}

				// Pop/Unshelve
				if (ShelveInfo.Stack.Count > 0)
				{
					menu.Add("Pop/Unshelve").IsSeparator = true;

					foreach(var si in ShelveInfo.Stack)
						menu.Add(si.Title).Data = si;
				}

				if (!menu.Show())
					return;

				var mi = menu.Items[menu.Selected];

				// [Delete]:
				if (menu.Key.VirtualKeyCode == KeyCode.Delete)
				{
					// remove the shelved panel; do not remove module panels because of their shutdown bypassed
					var si = (ShelveInfo)mi.Data;
					if (si.CanRemove)
						ShelveInfo.Stack.Remove(si);

					continue;
				}

				// Push/Shelve
				if (mi.Text == sPushShelveThePanel)
				{
					panel.Push();
					return;
				}

				bool repeat = menu.Key.VirtualKeyCode == KeyCode.Spacebar;

				// Decrease/Increase column
				if (mi.Text == sResizeColum1 || mi.Text == sResizeColum2)
				{
					ResizeColumn(module, mi.Text == sResizeColum2);
					if (repeat)
						continue;
					else
						return;
				}

				// Full screen
				if (mi.Text == sSwitchFullScreen)
				{
					SwitchFullScreen(module);
					if (repeat)
						continue;
					else
						return;
				}

				// Close panel
				if (mi.Text == sClose)
				{
					// native plugin panel: go to the first item to work around "Far does not restore panel state",
					// this does not restore either but is still better than unexpected current item after exit.
					if (null == module)
						panel.Redraw(0, 0);

					panel.Close();
					return;
				}

				// Pop/Unshelve
				var shelve = (ShelveInfo)mi.Data;
				shelve.Pop();
				return;
			}
		}
	}
}
