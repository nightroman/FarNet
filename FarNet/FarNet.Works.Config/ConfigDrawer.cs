
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
*/

using System.Collections.Generic;
using FarNet.Forms;

namespace FarNet.Works
{
	public static class ConfigDrawer
	{
		public static void Show(IList<IModuleDrawer> drawers, string helpTopic)
		{
			if (drawers == null)
				return;

			IMenu menu = Far.Api.CreateMenu();
			menu.AutoAssignHotkeys = true;
			menu.HelpTopic = helpTopic;
			menu.Title = Res.ModuleDrawers;

			foreach (IModuleDrawer it in drawers)
				menu.Add(Utility.FormatConfigMenu(it)).Data = it;

			while (menu.Show())
			{
				FarItem mi = menu.Items[menu.Selected];
				IModuleDrawer drawer = (IModuleDrawer)mi.Data;

				var dialog = new ConfigDrawerDialog(drawer, helpTopic);
				while (dialog.Dialog.Show())
				{
					var mask = ConfigTool.ValidateMask(dialog.Mask.Text);
					if (mask == null)
						continue;

					int priority;
					string priorityText = dialog.Priority.Text.Trim();
					if (!int.TryParse(priorityText, out priority))
					{
						Far.Api.Message("Invalid Priority.");
						continue;
					}

					// set
					drawer.Mask = mask;
					drawer.Priority = priority;
					drawer.Manager.SaveSettings();
					break;
				}
			}
		}
	}

	class ConfigDrawerDialog
	{
		public IDialog Dialog;
		public IEdit Mask;
		public IEdit Priority;
		public ConfigDrawerDialog(IModuleDrawer drawer, string helpTopic)
		{
			Dialog = Far.Api.CreateDialog(-1, -1, 77, 8);
			Dialog.HelpTopic = helpTopic;

			// Box
			Dialog.AddBox(3, 1, 0, 0, drawer.Name);
			int x = 14;

			// Mask
			Dialog.AddText(5, -1, 0, "&Mask");
			Mask = Dialog.AddEdit(x, 0, 71, drawer.Mask);

			// Priority
			Dialog.AddText(5, -1, 0, "&Priority");
			Priority = Dialog.AddEdit(x, 0, 71, drawer.Priority.ToString());

			Dialog.AddText(5, -1, 0, string.Empty).Separator = 1;

			IButton buttonOK = Dialog.AddButton(0, -1, "Ok");
			buttonOK.CenterGroup = true;

			IButton buttonCancel = Dialog.AddButton(0, 0, "Cancel");
			buttonCancel.CenterGroup = true;
		}
	}
}
