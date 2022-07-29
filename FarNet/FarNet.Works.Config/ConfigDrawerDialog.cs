
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet.Forms;

namespace FarNet.Works;

class ConfigDrawerDialog
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

		var buttonOK = Dialog.AddButton(0, -1, "Ok");
		buttonOK.CenterGroup = true;

		var buttonCancel = Dialog.AddButton(0, 0, "Cancel");
		buttonCancel.CenterGroup = true;
	}
}
