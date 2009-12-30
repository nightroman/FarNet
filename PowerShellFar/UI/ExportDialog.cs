/*
PowerShellFar plugin for Far Manager
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
		IDialog Dialog;
		IEdit Name;
		IComboBox Encoding;
		IEdit Depth;

		ExportDialog(string title, string filePath, bool useDepth)
		{
			int h = 8;
			if (useDepth)
				++h;

			Dialog = A.Far.CreateDialog(-1, -1, 77, h);
			Dialog.AddBox(3, 1, 0, 0, title);
			const int x = 16;
			int y = 1;

			Dialog.AddText(5, ++y, 0, "&File name");
			Name = Dialog.AddEdit(x, y, 71, string.Empty);
			Name.History = "NewEdit";
			Name.UseLastHistory = true;
			if (filePath != null)
				Name.Text = filePath;

			Dialog.AddText(5, ++y, 0, "&Encoding");
			Encoding = Dialog.AddComboBox(x, y, 71, string.Empty);
			Encoding.DropDownList = true;
			Encoding.Text = "Unicode";
			Encoding.Add("Default");
			Encoding.Add("Unicode");
			Encoding.Add("UTF8");
			Encoding.Add("BigEndianUnicode");
			Encoding.Add("ASCII");
			Encoding.Add("UTF7");
			Encoding.Add("UTF32");
			Encoding.Add("OEM");

			if (useDepth)
			{
				Dialog.AddText(5, ++y, 0, "&Depth");
				Depth = Dialog.AddEdit(x, y, 71, string.Empty);
			}

			Dialog.AddText(5, ++y, 0, string.Empty).Separator = 1;

			IButton buttonOK = Dialog.AddButton(0, ++y, "Ok");
			buttonOK.CenterGroup = true;

			IButton buttonCancel = Dialog.AddButton(0, y, Res.Cancel);
			buttonCancel.CenterGroup = true;
		}

		internal static void ExportClixml(IEnumerable items, string directory)
		{
			ExportDialog ui = new ExportDialog("Export-Clixml", null, true);
			for (; ; )
			{
				if (!ui.Dialog.Show())
					return;
				if (ui.Depth.Text.Length != 0)
				{
					int r;
					if (!int.TryParse(ui.Depth.Text, out r) || r <= 0)
					{
						A.Msg("Invalid depth value");
						ui.Dialog.Focused = ui.Depth;
						continue;
					}
				}
				break;
			}
			string ext = Path.GetExtension(ui.Name.Text);
			if (ext.Length == 0)
				ui.Name.Text += ".clixml";

			try
			{
				using (PowerShell p = A.Psf.CreatePipeline())
				{
					string filePath = ui.Name.Text;
					if (!string.IsNullOrEmpty(directory) && filePath.IndexOfAny(new char[] { '\\', '/', ':' }) < 0)
						filePath = My.PathEx.Combine(directory, filePath);
					if (File.Exists(filePath))
					{
						if (A.Far.Msg("File " + filePath + " exists. Continue?", "Confirm", MsgOptions.YesNo) != 0)
							return;
					}

					Command c = new Command("Export-Clixml");
					c.Parameters.Add("Path", filePath);
					c.Parameters.Add("Encoding", ui.Encoding.Text);
					if (ui.Depth.Text.Length > 0)
						c.Parameters.Add("Depth", int.Parse(ui.Depth.Text, CultureInfo.InstalledUICulture));
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
