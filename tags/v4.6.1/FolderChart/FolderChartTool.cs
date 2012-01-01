
/*
FarNet module FolderChart
Copyright (c) 2010-2012 Roman Kuzmin
*/

using System.IO;
using System.Linq;
using System.Text;
using FarNet;

namespace FolderChart
{
	[System.Runtime.InteropServices.Guid("192d8c20-ab65-4807-9654-9387881dd70b")]
	[ModuleTool(Name = "FolderChart", Options = ModuleToolOptions.Panels)]
	public class FolderChartTool : ModuleTool
	{
		const int HIDDEN_FACTOR = 100;
		const int MAX_ITEM_COUNT = 40;

		static void Message(string body)
		{
			Far.Net.Message(body, "Forder Chart", MsgOptions.LeftAligned);
		}

		public override void Invoke(object sender, ModuleToolEventArgs e)
		{
			var panel = Far.Net.Panel;
			if (panel.IsPlugin || panel.Kind != PanelKind.File)
				return;

			var path = panel.CurrentDirectory;

			var run = new SizeRun();
			if (!run.Run(Directory.GetDirectories(path), Directory.GetFiles(path)))
				return;

			var sb = new StringBuilder();
			foreach (var it in run.Errors)
				sb.AppendLine(it.Message);
			if (sb.Length > 0)
				Message(sb.ToString());

			var sorted = run.Result.OrderBy(x => x.Size).ToList();
			if (sorted.Count == 0)
				return;

			var totalSize = run.Result.Sum(x => x.Size);
			var title = Kit.FormatSize(totalSize, path);

			var maxSizeToShow = sorted[sorted.Count - 1].Size / HIDDEN_FACTOR;
			long sumHiddenSizes = 0;
			int index = 0;
			for (; index < sorted.Count; ++index)
			{
				if (sorted[index].Size < maxSizeToShow || sorted.Count - index > MAX_ITEM_COUNT)
					sumHiddenSizes += sorted[index].Size;
				else
					break;
			}
			if (index > 0)
				sorted.RemoveRange(0, index);
			if (sumHiddenSizes > 0)
				sorted.Insert(0, new FolderItem() { Name = string.Empty, Size = sumHiddenSizes });

			var result = FolderChartForm.Show(title, sorted, new WindowWrapper(Far.Net.UI.MainWindowHandle));
			if (result == null)
				return;

			var path2 = Path.Combine(path, result);
			if (Directory.Exists(path2))
			{
				panel.CurrentDirectory = path2;
			}
			else if (File.Exists(path2))
			{
				bool ok = panel.GoToName(result, false);
				if (!ok)
					Message(path2 + " exists but it is not in the panel.");
			}
			else
			{
				Message(path2 + " does not exist.");
			}

			panel.Redraw();
		}
	}
}
