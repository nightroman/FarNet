
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System.Collections;
using System.IO;
using System.Management.Automation;
using FarNet;
using FarNet.Forms;

namespace PowerShellFar.UI
{
	class ExportDialog
	{
		readonly IDialog _Dialog;
		readonly IEdit _File;
		readonly IComboBox _Encoding;
		readonly IEdit _Depth;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "BigEndianUnicode")]
		ExportDialog(string title, string filePath, bool useDepth)
		{
			int h = 8;
			if (useDepth)
				++h;

			_Dialog = Far.Api.CreateDialog(-1, -1, 77, h);
			_Dialog.AddBox(3, 1, 0, 0, title);
			const int x = 16;
			int y = 1;

			_Dialog.AddText(5, ++y, 0, "&File name");
			_File = _Dialog.AddEdit(x, y, 71, string.Empty);
			_File.History = "NewEdit";
			_File.IsPath = true;
			_File.UseLastHistory = true;
			if (filePath != null)
				_File.Text = filePath;

			_Dialog.AddText(5, ++y, 0, "&Encoding");
			_Encoding = _Dialog.AddComboBox(x, y, 71, string.Empty);
			_Encoding.DropDownList = true;
			_Encoding.Text = "Unicode";
			_Encoding.Add("Default");
			_Encoding.Add("Unicode");
			_Encoding.Add("UTF8");
			_Encoding.Add("BigEndianUnicode");
			_Encoding.Add("ASCII");
			_Encoding.Add("UTF7");
			_Encoding.Add("UTF32");
			_Encoding.Add("OEM");

			if (useDepth)
			{
				_Dialog.AddText(5, ++y, 0, "&Depth");
				_Depth = _Dialog.AddEdit(x, y, 71, string.Empty);
			}

			_Dialog.AddText(5, ++y, 0, string.Empty).Separator = 1;

			IButton buttonOK = _Dialog.AddButton(0, ++y, "Ok");
			buttonOK.CenterGroup = true;
			_Dialog.Default = buttonOK;

			IButton buttonCancel = _Dialog.AddButton(0, y, Res.Cancel);
			buttonCancel.CenterGroup = true;
			_Dialog.Cancel = buttonCancel;
		}
		internal static void ExportClixml(IEnumerable items, string directory)
		{
			ExportDialog ui = new ExportDialog("Export-Clixml", null, true);
			for (; ; )
			{
				if (!ui._Dialog.Show())
					return;
				if (ui._Depth.Text.Length != 0)
				{
					int r;
					if (!int.TryParse(ui._Depth.Text, out r) || r <= 0)
					{
						A.Message("Invalid depth value");
						ui._Dialog.Focused = ui._Depth;
						continue;
					}
				}
				break;
			}
			string ext = Path.GetExtension(ui._File.Text);
			if (ext.Length == 0)
				ui._File.Text += ".clixml";

			try
			{
				string filePath = ui._File.Text;
				if (!string.IsNullOrEmpty(directory) && filePath.IndexOfAny(new char[] { '\\', '/', ':' }) < 0)
					filePath = My.PathEx.Combine(directory, filePath);

				if (File.Exists(filePath))
					if (Far.Api.Message("File " + filePath + " exists. Continue?", "Confirm", MessageOptions.YesNo) != 0)
						return;

				const string code = "$args[0] | Export-Clixml -Path $args[1] -Encoding $args[2] -Force -ErrorAction Stop";
				if (ui._Depth.Text.Length > 0)
					A.InvokeCode(code + " -Depth $args[3]",
						items, filePath, ui._Encoding.Text, int.Parse(ui._Depth.Text, null));
				else
					A.InvokeCode(code,
						items, filePath, ui._Encoding.Text);
			}
			catch (RuntimeException ex)
			{
				A.Msg(ex);
			}
		}
	}
}
