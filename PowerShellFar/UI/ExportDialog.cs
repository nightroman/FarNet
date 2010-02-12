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

			UIDialog = Far.Host.CreateDialog(-1, -1, 77, h);
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
				using (PowerShell p = A.Psf.CreatePipeline())
				{
					string filePath = ui.UIFile.Text;
					if (!string.IsNullOrEmpty(directory) && filePath.IndexOfAny(new char[] { '\\', '/', ':' }) < 0)
						filePath = My.PathEx.Combine(directory, filePath);
					if (File.Exists(filePath))
					{
						if (Far.Host.Message("File " + filePath + " exists. Continue?", "Confirm", MsgOptions.YesNo) != 0)
							return;
					}

					Command c = new Command("Export-Clixml");
					c.Parameters.Add("Path", filePath);
					c.Parameters.Add("Encoding", ui.UIEncoding.Text);
					if (ui.UIDepth.Text.Length > 0)
						c.Parameters.Add("Depth", int.Parse(ui.UIDepth.Text, CultureInfo.InstalledUICulture));
					c.Parameters.Add(Prm.Force);
					c.Parameters.Add(Prm.EAStop);
					p.Commands.AddCommand(c);
					p.Invoke(items);
				}
			}
			catch (RuntimeException ex)
			{
				A.Msg(ex);
			}
		}
	}
}
