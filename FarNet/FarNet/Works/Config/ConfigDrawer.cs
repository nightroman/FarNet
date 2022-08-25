
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Collections.Generic;

namespace FarNet.Works;
#pragma warning disable 1591

public static class ConfigDrawer
{
	internal const string HelpTopic = "configure-drawers";

	public static void Show(List<IModuleDrawer> drawers)
	{
		var menu = Far.Api.CreateMenu();
		menu.AutoAssignHotkeys = true;
		menu.HelpTopic = HelpTopic;
		menu.Title = "Drawers";
		menu.AddSimpleConfigItems(drawers);

		while (menu.Show())
		{
			var drawer = (IModuleDrawer)menu.SelectedData;

			var dialog = new ConfigDrawerDialog(drawer);
			while (dialog.Dialog.Show())
			{
				var mask = ConfigTool.ValidateMask(dialog.Mask.Text);
				if (mask == null)
					continue;

				string priorityText = dialog.Priority.Text.Trim();
				if (!int.TryParse(priorityText, out int priority))
				{
					Far.Api.Message("Invalid Priority.");
					continue;
				}

				drawer.Mask = mask;
				drawer.Priority = priority;
				drawer.Manager.SaveConfig();
				break;
			}
		}
	}
}
