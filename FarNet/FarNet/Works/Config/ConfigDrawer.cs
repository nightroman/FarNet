using FarNet.Forms;

namespace FarNet.Works;

static class ConfigDrawer
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
			var drawer = (IModuleDrawer)menu.SelectedData!;

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

file class ConfigDrawerDialog
{
	public IDialog Dialog;
	public IEdit Mask;
	public IEdit Priority;

	public ConfigDrawerDialog(IModuleDrawer drawer)
	{
		Dialog = Far.Api.CreateDialog(-1, -1, 77, 8);
		Dialog.HelpTopic = ConfigDrawer.HelpTopic;

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

		var buttonOK = Dialog.AddButton(0, -1, "OK");
		buttonOK.CenterGroup = true;

		var buttonCancel = Dialog.AddButton(0, 0, "Cancel");
		buttonCancel.CenterGroup = true;
	}
}
