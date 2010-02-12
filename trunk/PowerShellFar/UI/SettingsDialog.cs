/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using FarNet;
using FarNet.Forms;

namespace PowerShellFar.UI
{
	class SettingsDialog
	{
		const int x = 25;
		IDialog _Dialog;
		IEdit _StartupCode;
		IEdit _StartupEdit;

		public SettingsDialog()
		{
			_Dialog = Far.Host.CreateDialog(-1, -1, 77, 8);
			_Dialog.HelpTopic = A.Psf.HelpTopic + "Settings";
			_Dialog.AddBox(3, 1, 0, 0, Res.Me);

			_Dialog.AddText(5, -1, 0, "&Main startup code");
			_StartupCode = _Dialog.AddEdit(x, 0, 71, A.Psf.Settings.StartupCode);

			_Dialog.AddText(5, -1, 0, "&Editor startup code");
			_StartupEdit = _Dialog.AddEdit(x, 0, 71, A.Psf.Settings.StartupEdit);

			_Dialog.AddText(5, -1, 0, string.Empty).Separator = 1;
			IButton buttonOK = _Dialog.AddButton(0, -1, "Ok");
			buttonOK.CenterGroup = true;
			_Dialog.Default = buttonOK;
			_Dialog.Cancel = _Dialog.AddButton(0, 0, Res.Cancel);
			_Dialog.Cancel.CenterGroup = true;
		}

		public bool Show()
		{
			while (_Dialog.Show())
			{
				_StartupCode.Text = _StartupCode.Text.TrimEnd();
				_StartupEdit.Text = _StartupEdit.Text.TrimEnd();

				bool needRestart =
					A.Psf.Settings.StartupCode != _StartupCode.Text ||
					A.Psf.Settings.StartupEdit != _StartupEdit.Text;

				A.Psf.Settings.StartupCode = _StartupCode.Text;
				A.Psf.Settings.StartupEdit = _StartupEdit.Text;
				A.Psf.Settings.Save();

				if (needRestart)
					Far.Host.Message("Some settings will take effect only when Far restarts.");

				return true;
			}
			return false;
		}
	}
}
