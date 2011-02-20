
/*
FarNet module Explore
Copyright (c) 2010 Roman Kuzmin
*/

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace FarNet.Tools
{
	/// <summary>
	/// File search processor.
	/// </summary>
	/// <param name="explorer">The explorer providing the file.</param>
	/// <param name="file">The file to be processed.</param>
	public delegate bool FileSearchProcess(Explorer explorer, FarFile file);

	/// <summary>
	/// File search explorer.
	/// </summary>
	public class FileSearchExplorer : Explorer
	{
		readonly Explorer _RootExplorer;
		readonly List<FarFile> _Files;
		/// <summary>
		/// Tells to include directories into the search process and results.
		/// </summary>
		public bool Directory { get; set; }
		/// <summary>
		/// Tells to search through all directories and sub-directories.
		/// </summary>
		public bool Recurse { get; set; }
		/// <summary>
		/// Gets or sets the file search processor.
		/// </summary>
		public FileSearchProcess Process { get; set; }
		/// <summary>
		/// Gets the collection of result files (<see cref="ExplorerFile"/>).
		/// </summary>
		public ReadOnlyCollection<FarFile> ResultFiles { get { return new ReadOnlyCollection<FarFile>(_Files); } }
		/// <summary>
		/// New search explorer with the search root.
		/// </summary>
		public FileSearchExplorer(Explorer root)
			: base(new System.Guid("7d503b37-23a0-4ebd-878b-226e972b0b9d"))
		{
			// base
			Function = ExplorerFunctions.DeleteFiles | ExplorerFunctions.ExportFile | ExplorerFunctions.ImportFile;
			FileComparer = new FileFileComparer();

			// this
			_RootExplorer = root;
			_Files = new List<FarFile>();
		}
		/// <summary>
		/// Starts the search.
		/// </summary>
		public void Invoke()
		{
			if (_RootExplorer == null) throw new InvalidOperationException("Root explorer is null.");

			var queue = new Queue<Explorer>();
			queue.Enqueue(_RootExplorer);

			_Files.Clear();
			while (queue.Count > 0)
			{
				var explorer = queue.Dequeue();
				var args = new ExplorerArgs(ExplorerModes.None);
				foreach (var file in explorer.Explore(args))
				{
					// process and add
					bool add = Directory || !file.IsDirectory;
					if (add && Process != null)
						add = Process(explorer, file);
					if (add)
						_Files.Add(new ExplorerFile(explorer, file));

					// skip if flat or leaf
					if (!Recurse || !file.IsDirectory)
						continue;

					Explorer explorer2 = ExploreExplorerFile(explorer, file);
					if (explorer2 != null)
						queue.Enqueue(explorer2);
				}
			}
		}
		static Explorer ExploreExplorerFile(Explorer explorer, FarFile file)
		{
			if ((explorer.Function & ExplorerFunctions.ExploreLocation) != 0)
				return explorer.ExploreLocation(new ExploreLocationArgs(ExplorerModes.None, file.Name));
			else
				return explorer.ExploreDirectory(new ExploreDirectoryArgs(ExplorerModes.None, file));
		}
		/// <summary>
		/// Creates the search result panel.
		/// </summary>
		/// <returns></returns>
		public override Panel CreatePanel()
		{
			var panel = new Panel();
			panel.Explorer = this;
			panel.PanelDirectory = "*";
			panel.Title = "Found in " + _RootExplorer.Location;

			panel.KeyPressed += OnKeyPressed;

			return panel;
		}
		void OnKeyPressed(object sender, PanelKeyEventArgs e)
		{
			switch (e.Code)
			{
				case VKeyCode.PageUp:
					if (e.State == KeyStates.Control)
					{
						var file = (ExplorerFile)((Panel)sender).CurrentFile;
						if (file == null)
							return;

						e.Ignore = true;

						var panel = file.Explorer.CreatePanel() ?? new Panel();
						file.Explorer.UpdatePanel(panel);

						panel.Explorer = file.Explorer; //????? this has to be denied (need a method)
						panel.PostFile(file);

						if (string.IsNullOrEmpty(panel.PanelDirectory))
							panel.PanelDirectory = "*"; //?????

						panel.OpenChild(((Panel)sender));
					}
					return;
			}
		}
		///
		public override IList<FarFile> Explore(ExplorerArgs args)
		{
			return _Files;
		}
		///
		public override Explorer ExploreDirectory(ExploreDirectoryArgs args)
		{
			var xFile = (ExplorerFile)args.File;
			return ExploreExplorerFile(xFile.Explorer, xFile.File);
		}
		///
		public override void ExportFile(ExportFileArgs args)
		{
			// call
			var file2 = (ExplorerFile)args.File;
			var argsExport = new ExportFileArgs(args.Mode, file2.File, args.FileName);
			file2.Explorer.ExportFile(argsExport);

			// results
			args.Result = argsExport.Result;
			args.CanImport = argsExport.CanImport;
			args.FileNameExtension = argsExport.FileNameExtension;
		}
		///
		public override void ImportFile(ImportFileArgs args)
		{
			var file2 = (ExplorerFile)args.File;
			var argsImport = new ImportFileArgs(args.Mode, file2.File, args.FileName);
			file2.Explorer.ImportFile(argsImport);

			args.Result = argsImport.Result;
		}
		///
		public override void DeleteFiles(DeleteFilesArgs args)
		{
			var dicTypeId = new Dictionary<Guid, Dictionary<Explorer, List<ExplorerFile>>>();
			foreach (ExplorerFile file in args.Files)
			{
				Dictionary<Explorer, List<ExplorerFile>> dicExplorer;
				if (!dicTypeId.TryGetValue(file.Explorer.TypeId, out dicExplorer))
				{
					dicExplorer = new Dictionary<Explorer, List<ExplorerFile>>();
					dicTypeId.Add(file.Explorer.TypeId, dicExplorer);
				}

				List<ExplorerFile> files;
				if (!dicExplorer.TryGetValue(file.Explorer, out files))
				{
					files = new List<ExplorerFile>();
					dicExplorer.Add(file.Explorer, files);
				}
				files.Add(file);
			}

			foreach (var xTypeId in dicTypeId)
			{
				Log.Source.TraceInformation("DeleteFiles TypeId='{0}'", xTypeId.Key);
				object dataForTypeId = null;
				foreach (var xExplorer in xTypeId.Value)
				{
					// collect explorer files
					var files = new List<FarFile>(xExplorer.Value.Count);
					foreach (var file in xExplorer.Value)
						files.Add(file.File);

					// delete, mind co-data
					Log.Source.TraceInformation("DeleteFiles Count='{0}' Location='{1}'", files.Count, xExplorer.Key.Location);
					var argsDelete = new DeleteFilesArgs(args.Mode, files);
					argsDelete.Data = dataForTypeId;
					xExplorer.Key.DeleteFiles(argsDelete);
					dataForTypeId = argsDelete.Data;

					// remove
					foreach (var file in xExplorer.Value)
						_Files.Remove(file);
				}
			}
		}
	}
}
