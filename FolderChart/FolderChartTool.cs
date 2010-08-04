
/*
FarNet module FolderChart
Copyright (c) 2010 Roman Kuzmin
*/

using System.IO;
using System.Text;
using FarNet;

namespace FolderChart
{
	[System.Runtime.InteropServices.Guid("192d8c20-ab65-4807-9654-9387881dd70b")]
	[ModuleTool(Name = "FolderChart", Options = ModuleToolOptions.Panels)]
	public class FolderChartTool : ModuleTool
	{
		static void Message(string body)
		{
			Far.Net.Message(body, "Forder Chart", MsgOptions.LeftAligned);
		}

		public override void Invoke(object sender, ModuleToolEventArgs e)
		{
			var panel = Far.Net.Panel;
			if (panel.IsPlugin || panel.Kind != PanelKind.File)
				return;

			var path = panel.Path;

			var run = new SizeRun();
			if (!run.Run(Directory.GetDirectories(path), Directory.GetFiles(path)))
				return;

			{
				var sb = new StringBuilder();
				foreach (var it in run.Errors)
					sb.AppendLine(it.Message);
				if (sb.Length > 0)
					Message(sb.ToString());
			}

			var result = FolderChartForm.Show(path, run.Result, new WindowWrapper(Far.Net.UI.MainWindowHandle));
			if (result == null)
				return;

			var path2 = Path.Combine(path, result);
			if (Directory.Exists(path2))
			{
				panel.Path = path2;
			}
			else if (File.Exists(path2))
			{
				bool ok = panel.GoToName(result, false);
				if (!ok)
					Message(path2 + " is not found on the panel, perhaps it is hidden.");
			}
			else
			{
				Message(path2 + " does not exist.");
			}
			
			panel.Redraw();
		}
	}
}
