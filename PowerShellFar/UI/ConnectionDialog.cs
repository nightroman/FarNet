
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using FarNet.Forms;

namespace PowerShellFar.UI;

class ConnectionDialog
{
	const int x = 15;
	readonly IDialog _Dialog;
	readonly IEdit _ComputerName;
	readonly IEdit _UserName;

	public string ComputerName => _ComputerName.Text.TrimEnd();

	public string UserName => _UserName.Text.TrimEnd();

	public bool Show() => _Dialog.Show();

	public ConnectionDialog(string title)
	{
		_Dialog = Far.Api.CreateDialog(-1, -1, 77, 8);
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
		IButton buttonOK = _Dialog.AddButton(0, -1, "OK");
		buttonOK.CenterGroup = true;
		_Dialog.Default = buttonOK;
		_Dialog.Cancel = _Dialog.AddButton(0, 0, Res.Cancel);
		_Dialog.Cancel.CenterGroup = true;
	}
}
