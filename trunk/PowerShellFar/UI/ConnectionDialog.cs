/*
PowerShellFar plugin for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using FarNet.Forms;

namespace PowerShellFar.UI
{
	class ConnectionDialog
	{
		const int x = 15;
		IDialog _Dialog;
		IEdit _ComputerName;
		IEdit _UserName;

		public string ComputerName { get { return _ComputerName.Text.TrimEnd(); } }

		public string UserName { get { return _UserName.Text.TrimEnd(); } }

		public ConnectionDialog(string title)
		{
			_Dialog = A.Far.CreateDialog(-1, -1, 77, 8);
			_Dialog.AddBox(3, 1, 0, 0, title);

			_Dialog.AddText(5, -1, 0, "&Computer");
			_ComputerName = _Dialog.AddEdit(x, 0, 71, string.Empty);
			_ComputerName.History = "ComputerName";
			_ComputerName.UseLastHistory = true;

			_Dialog.AddText(5, -1, 0, "&User name");
			_UserName = _Dialog.AddEdit(x, 0, 71, string.Empty);
			_UserName.History = "UserName";
			_UserName.UseLastHistory = true;

			_Dialog.AddText(5, -1, 0, string.Empty).Separator = 1;
			IButton buttonOK = _Dialog.AddButton(0, -1, "Ok");
			buttonOK.CenterGroup = true;
			_Dialog.Default = buttonOK;
			_Dialog.Cancel = _Dialog.AddButton(0, 0, Res.Cancel);
			_Dialog.Cancel.CenterGroup = true;
		}

		public bool Show()
		{
			return _Dialog.Show();
		}
	}
}
