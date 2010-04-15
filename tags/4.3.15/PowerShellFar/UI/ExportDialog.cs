/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System.Collections;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using FarNet;
using FarNet.Forms;

namespace PowerShellFar.UI
{
	class ExportDialog
	{
		IDialog UIDialog;
		IEdit UIFile;
		IComboBox UIEncoding;
		IEdit UIDepth;

		ExportDialog(string title, string filePath, bool useDepth)
		{
			int h = 8;
			if (useDepth)
				++h;

			UIDialog = Far.Net.CreateDialog(-1, -1, 77, h);
			UIDialog.AddBox(3, 1, 0, 0, title);
			const int x = 16;
			int y = 1;

			UIDialog.AddText(5, ++y, 0, "&File name");
			UIFile = UIDialog.AddEdit(x, y, 71, string.Empty);
			UIFile.History = "NewEdit";
			UIFile.IsPath = true;
			UIFile.UseLastHistory = true;
			if (filePath != null)
				UIFile.Text = filePath;

			UIDialog.AddText(5, ++y, 0, "&Encoding");
			UIEncoding = UIDialog.AddComboBox(x, y, 71, string.Empty);
			UIEncoding.DropDownList = true;
			UIEncoding.Text = "Unicode";
			UIEncoding.Add("Default");
			UIEncoding.Add("Unicode");
			UIEncoding.Add("UTF8");
			UIEncoding.Add("BigEndianUnicode");
			UIEncoding.Add("ASCII");
			UIEncoding.Add("UTF7");
			UIEncoding.Add("UTF32");
			UIEncoding.Add("OEM");

			if (useDepth)
			{
				UIDialog.AddText(5, ++y, 0, "&Depth");
				UIDepth = UIDialog.AddEdit(x, y, 71, string.Empty);
			}

			UIDialog.AddText(5, ++y, 0, string.Empty).Separator = 1;

			IButton buttonOK = UIDialog.AddButton(0, ++y, "Ok");
			buttonOK.CenterGroup = true;

			IButton buttonCancel = UIDialog.AddButton(0, y, Res.Cancel);
			buttonCancel.CenterGroup = true;
		}

		internal static void ExportClixml(IEnumerable items, string directory)
		{
			ExportDialog ui = new ExportDialog("Export-Clixml", null, true);
			for (; ; )
			{
				if (!ui.UIDialog.Show())
					return;
				if (ui.UIDepth.Text.Length != 0)
				{
					int r;
					if (!int.TryParse(ui.UIDepth.Text, out r) || r <= 0)
					{
						A.Msg("Invalid depth value");
						ui.UIDialog.Focused = ui.UIDepth;
						continue;
					}
				}
				break;
			}
			string ext = Path.GetExtension(ui.UIFile.Text);
			if (ext.Length == 0)
				ui.UIFile.Text += ".clixml";

			try
			{
				string filePath = ui.UIFile.Text;
				if (!string.IsNullOrEmpty(directory) && filePath.IndexOfAny(new char[] { '\\', '/', ':' }) < 0)
					filePath = My.PathEx.Combine(directory, filePath);

				if (File.Exists(filePath))
					if (Far.Net.Message("File " + filePath + " exists. Continue?", "Confirm", MsgOptions.YesNo) != 0)
						return;

				const string code = "$args[0] | Export-Clixml -Path $args[1] -Encoding $args[2] -Force -ErrorAction Stop";
				if (ui.UIDepth.Text.Length > 0)
					A.Psf.InvokeCode(code + " -Depth $args[3]",
						items, filePath, ui.UIEncoding.Text, int.Parse(ui.UIDepth.Text, CultureInfo.InvariantCulture));
				else
					A.Psf.InvokeCode(code,
						items, filePath, ui.UIEncoding.Text);
			}
			catch (RuntimeException ex)
			{
				A.Msg(ex);
			}
		}
	
	}
}
