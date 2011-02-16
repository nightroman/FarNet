
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System.Collections.Generic;

namespace FarNet
{
	class SearchData
	{
		public Explorer Explorer { get; set; }
		public FarFile File { get; set; }
	}
	class SearchExplorer : Explorer
	{
		public static void Start(Panel panel)
		{
			if (panel.Explorer == null)
				return;

			var queue = new Queue<Explorer>();
			queue.Enqueue(panel.Explorer);

			var results = new List<FarFile>();
			while (queue.Count > 0)
			{
				var explorer = queue.Dequeue();
				var args = new ExplorerArgs();
				foreach (var file in explorer.Explore(args))
				{
					var data = new SearchData() { Explorer = explorer, File = file };
					var file2 = new SetFile(file) { Data = data, IsDirectory = true };
					results.Add(file2);

					var args2 = new ExploreFileArgs() { File = file };
					var explorer2 = explorer.ExploreFile(args2);
					if (explorer2 != null)
						queue.Enqueue(explorer2);
				}
			}

			var newPanel = new Panel();
			newPanel.Explorer = new SearchExplorer(results);
			newPanel.Title = "Search Results of " + panel.Explorer.Location;
			newPanel.OpenChild(panel);
		}
		readonly IList<FarFile> _Files;
		public SearchExplorer(IList<FarFile> results)
			: base(null)
		{
			_Files = results;
		}
		public override IList<FarFile> Explore(ExplorerArgs args)
		{
			return _Files;
		}
		public override Explorer ExploreFile(ExploreFileArgs args)
		{
			var data = (SearchData)args.File.Data;
			var args2 = new ExploreFileArgs();
			args2.File = data.File;
			var newExplorer = data.Explorer.ExploreFile(args2);
			if (newExplorer != null)
				return newExplorer;

			args.ToPostName = true;
			return data.Explorer;
		}
		public override bool CanExportFile(FarFile file)
		{
			var data = (SearchData)file.Data;
			return data.Explorer.CanExportFile(data.File);
		}
		public override void ExportFile(ExportFileArgs args)
		{
			var data = (SearchData)args.File.Data;
			var args2 = new ExportFileArgs();
			args2.File = data.File;
			args2.FileName = args.FileName;
			data.Explorer.ExportFile(args2);
		}
		public override bool CanImportFile(FarFile file)
		{
			var data = (SearchData)file.Data;
			return data.Explorer.CanImportFile(data.File);
		}
		public override void ImportFile(ImportFileArgs args)
		{
			var data = (SearchData)args.File.Data;
			var args2 = new ImportFileArgs();
			args2.File = data.File;
			args2.FileName = args.FileName;
			data.Explorer.ImportFile(args2);
		}
	}
}
