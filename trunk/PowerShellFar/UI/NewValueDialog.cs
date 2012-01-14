
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
*/

using System;
using FarNet;
using FarNet.Forms;

namespace PowerShellFar.UI
{
	class NewValueDialog
	{
		public IDialog Dialog;
		public IEdit Name;
		public IEdit Type;
		public IEdit Value;

		public NewValueDialog(string title)
		{
			Dialog = Far.Net.CreateDialog(-1, -1, 77, 9);
			Dialog.AddBox(3, 1, 0, 0, title);
			int x = 11;

			// use last history, it is useful
			Dialog.AddText(5, -1, 0, "&Name");
			Name = Dialog.AddEdit(x, 0, 71, string.Empty);
			Name.History = "PowerPanelNames";
			Name.UseLastHistory = true;

			// do not use last history!
			Dialog.AddText(5, -1, 0, "&Type");
			Type = Dialog.AddEdit(x, 0, 71, string.Empty);
			Type.History = "PowerPanelTypes";

			// do not use last history!
			Dialog.AddText(5, -1, 0, "&Value");
			Value = Dialog.AddEdit(x, 0, 71, string.Empty);
			Value.History = "PowerPanelValues";

			Dialog.AddText(5, -1, 0, string.Empty).Separator = 1;

			IButton buttonOK = Dialog.AddButton(0, -1, "Ok");
			buttonOK.CenterGroup = true;

			IButton buttonCancel = Dialog.AddButton(0, 0, Res.Cancel);
			buttonCancel.CenterGroup = true;
		}
	}
}
