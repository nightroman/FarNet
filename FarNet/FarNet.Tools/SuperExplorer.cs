
/*
FarNet.Tools library for FarNet
Copyright (c) 2010 Roman Kuzmin
*/

using System;
using System.Collections.Generic;

namespace FarNet.Tools
{
	/// <summary>
	/// Explorer of other explorers files.
	/// </summary>
	public class SuperExplorer : Explorer
	{
		///
		public const string TypeIdString = "7d503b37-23a0-4ebd-878b-226e972b0b9d";
		readonly List<FarFile> _Cache;
		/// <summary>
		/// Gets the cache of files.
		/// </summary>
		public IList<FarFile> Cache { get { return _Cache; } }
		/// <summary>
		/// New search explorer with the search root.
		/// </summary>
		public SuperExplorer()
			: base(new System.Guid(TypeIdString))
		{
			// base
			FileComparer = new FileFileComparer();
			Functions =
				ExplorerFunctions.AcceptFiles |
				ExplorerFunctions.DeleteFiles |
				ExplorerFunctions.ExportFile |
				ExplorerFunctions.ImportFile |
				ExplorerFunctions.ImportText |
				ExplorerFunctions.OpenFile;

			// this
			_Cache = new List<FarFile>();
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		internal static Explorer ExploreSuperDirectory(Explorer explorer, ExplorerModes mode, FarFile file)
		{
			try
			{
				if (explorer.CanExploreLocation)
					return explorer.ExploreLocation(new ExploreLocationEventArgs(null, mode, file.Name));
				else
					return explorer.ExploreDirectory(new ExploreDirectoryEventArgs(null, mode, file));
			}
			catch (Exception ex)
			{
				FarNet.Log.TraceException(ex);
				return null;
			}
		}
		/// <summary>
		/// Adds <see cref="SuperFile"/> files.
		/// </summary>
		public void AddFiles(IEnumerable<FarFile> files)
		{
			if (files == null)
				return;

			//! cast/check and add
			foreach (SuperFile file in files)
				_Cache.Add(file);
		}
		///
		public override Panel CreatePanel()
		{
			return new SuperPanel(this);
		}
		///
		public override IList<FarFile> GetFiles(GetFilesEventArgs args)
		{
			return _Cache;
		}
		///
		public override Explorer ExploreDirectory(ExploreDirectoryEventArgs args)
		{
			if (args == null) return null;

			var xfile = (SuperFile)args.File;
			return ExploreSuperDirectory(xfile.Explorer, args.Mode, xfile.File);
		}
		///
		public override void OpenFile(OpenFileEventArgs args)
		{
			if (args == null) return;

			var xfile = (SuperFile)args.File;
			if (!xfile.Explorer.CanOpenFile)
			{
				args.Result = JobResult.Ignore;
				return;
			}

			var argsOpen = new OpenFileEventArgs(args.Panel, ExplorerModes.None, xfile.File);
			xfile.Explorer.OpenFile(argsOpen);

			args.Result = argsOpen.Result;
		}
		///
		public override void AcceptFiles(AcceptFilesEventArgs args)
		{
			if (args == null) return;

			foreach (var file in args.Files)
			{
				var efile = file as SuperFile;
				if (efile == null)
					Cache.Add(new SuperFile(args.Explorer, file));
				else
					Cache.Add(efile);
			}
		}
		///
		public override void ExportFile(ExportFileEventArgs args)
		{
			if (args == null) return;

			// check
			var file2 = (SuperFile)args.File;
			if (!file2.Explorer.CanExportFile)
			{
				args.Result = JobResult.Default;
				return;
			}

			// call
			var argsExport = new ExportFileEventArgs(null, args.Mode, file2.File, args.FileName);
			file2.Explorer.ExportFile(argsExport);

			// results
			args.Result = argsExport.Result;
			args.UseText = argsExport.UseText;
			args.CanImport = argsExport.CanImport;
			args.UseFileName = argsExport.UseFileName;
			args.UseFileExtension = argsExport.UseFileExtension;
		}
		///
		public override void ImportFile(ImportFileEventArgs args)
		{
			if (args == null) return;

			// call
			var file2 = (SuperFile)args.File;
			if (!file2.Explorer.CanImportFile)
			{
				args.Result = JobResult.Default;
				return;
			}
			var argsImport = new ImportFileEventArgs(null, args.Mode, file2.File, args.FileName);
			file2.Explorer.ImportFile(argsImport);

			// result
			args.Result = argsImport.Result;
		}
		///
		public override void ImportText(ImportTextEventArgs args)
		{
			if (args == null) return;

			// call
			var file2 = (SuperFile)args.File;
			if (!file2.Explorer.CanImportText)
			{
				args.Result = JobResult.Default;
				return;
			}
			var argsImport = new ImportTextEventArgs(null, args.Mode, file2.File, args.Text);
			file2.Explorer.ImportText(argsImport);

			// result
			args.Result = argsImport.Result;
		}
		static Dictionary<Guid, Dictionary<Explorer, List<SuperFile>>> GroupFiles(IList<FarFile> files, bool toDelete)
		{
			var result = new Dictionary<Guid, Dictionary<Explorer, List<SuperFile>>>();
			foreach (SuperFile file in files)
			{
				if (toDelete && !file.Explorer.CanDeleteFiles)
					continue;

				Dictionary<Explorer, List<SuperFile>> dicExplorer;
				if (!result.TryGetValue(file.Explorer.TypeId, out dicExplorer))
				{
					dicExplorer = new Dictionary<Explorer, List<SuperFile>>();
					result.Add(file.Explorer.TypeId, dicExplorer);
				}

				List<SuperFile> efiles;
				if (!dicExplorer.TryGetValue(file.Explorer, out efiles))
				{
					efiles = new List<SuperFile>();
					dicExplorer.Add(file.Explorer, efiles);
				}
				efiles.Add(file);
			}
			return result;
		}
		internal void RemoveFiles(IList<FarFile> files)
		{
			foreach (var file in files)
				_Cache.Remove(file);
		}
		///
		public override void DeleteFiles(DeleteFilesEventArgs args)
		{
			if (args == null) return;

			var dicTypeId = GroupFiles(args.Files, true);

			int nDone = 0;
			int nIncomplete = 0;
			foreach (var xTypeId in dicTypeId)
			{
				Log.Source.TraceInformation("DeleteFiles TypeId='{0}'", xTypeId.Key);
				object codata = null;
				foreach (var kv in xTypeId.Value)
				{
					var result = DeleteFilesOfExplorer(kv.Key, kv.Value, args.FilesToStay, args.Mode, args.Force, ref codata);
					if (result == JobResult.Done)
						++nDone;
					else if (result == JobResult.Incomplete)
						++nIncomplete;
				}
			}

			// my result
			if (nIncomplete > 0)
				args.Result = JobResult.Incomplete;
			else if (nDone == 0)
				args.Result = JobResult.Ignore;
		}
		///
		internal void CommitFiles(SuperPanel source, Panel target, IList<FarFile> files, bool move)
		{
			var dicTypeId = GroupFiles(files, false);

			bool SelectionExists = source.SelectionExists;
			var xfilesToStay = new List<FarFile>();
			bool toUnselect = false;
			bool toUpdate = false;
			foreach (var xTypeId in dicTypeId)
			{
				Log.Source.TraceInformation("AcceptFiles TypeId='{0}'", xTypeId.Key);
				object codata = null;
				foreach (var kv in xTypeId.Value)
				{
					// explorer and its files
					var explorer = kv.Key;
					var xfiles = kv.Value;
					var filesToAccept = new List<FarFile>(xfiles.Count);
					foreach (var file in xfiles)
						filesToAccept.Add(file.File);

					// accept, mind co-data
					Log.Source.TraceInformation("AcceptFiles Count='{0}' Location='{1}'", filesToAccept.Count, explorer.Location);
					var argsAccept = new AcceptFilesEventArgs(null, ExplorerModes.None, filesToAccept, explorer, move);
					argsAccept.Data = codata;
					target.Explorer.AcceptFiles(argsAccept);
					codata = argsAccept.Data;

					// info
					bool isIncomplete = argsAccept.Result == JobResult.Incomplete;
					bool isAllToStay = isIncomplete && argsAccept.FilesToStay.Count == 0;

					// Copy: do not update the source, files are the same
					if (!move)
					{
						// keep it as it is
						if (isAllToStay || !SelectionExists)
							continue;

						// drop selection
						toUnselect = true;

						// recover
						if (isIncomplete)
							xfilesToStay.AddRange(SuperFile.SuperFilesOfExplorerFiles(xfiles, argsAccept.FilesToStay, explorer.FileComparer));

						continue;
					}

					// Move: no need to delete or all to stay or cannot delete
					if (!argsAccept.Delete || isAllToStay || !explorer.CanDeleteFiles)
					{
						// the source may have some files deleted, update
						toUpdate = true;

						// recover selection
						if (isIncomplete)
							xfilesToStay.AddRange(SuperFile.SuperFilesOfExplorerFiles(
								xfiles, isAllToStay ? argsAccept.Files : argsAccept.FilesToStay, explorer.FileComparer));

						continue;
					}

					// Move: delete is requested, delete the source files

					// exclude this files to stay from to be deleted
					if (isIncomplete)
					{
						foreach (SuperFile xfile in SuperFile.SuperFilesOfExplorerFiles(xfiles, argsAccept.FilesToStay, explorer.FileComparer))
						{
							xfiles.Remove(xfile);
							xfilesToStay.Add(xfile);
						}
					}

					// call delete on remaining files
					object codata2 = null;
					var result = DeleteFilesOfExplorer(explorer, xfiles, xfilesToStay, ExplorerModes.Silent, false, ref codata2);
					if (result == JobResult.Done || result == JobResult.Incomplete)
						toUpdate = true;
				}
			}

			// update the target panel
			target.Update(true);
			target.Redraw();

			// update/recover the source

			if (toUpdate)
				source.Update(false);
			else if (toUnselect)
				source.UnselectAll();

			if (xfilesToStay.Count > 0)
				source.SelectFiles(xfilesToStay, null);

			source.Redraw();
		}
		JobResult DeleteFilesOfExplorer(Explorer explorer, List<SuperFile> xfilesToDelete, IList<FarFile> xfilesToStay, ExplorerModes mode, bool force, ref object codata)
		{
			// explorer files
			var efilesToDelete = new List<FarFile>(xfilesToDelete.Count);
			foreach (var file in xfilesToDelete)
				efilesToDelete.Add(file.File);

			// delete, mind co-data
			Log.Source.TraceInformation("DeleteFiles Count='{0}' Location='{1}'", efilesToDelete.Count, explorer.Location);
			var argsDelete = new DeleteFilesEventArgs(null, mode, efilesToDelete, force);
			argsDelete.Data = codata;
			explorer.DeleteFiles(argsDelete);
			codata = argsDelete.Data;

			// result: break to delete
			switch (argsDelete.Result)
			{
				default:
					return JobResult.Ignore;

				case JobResult.Done:
					break;

				case JobResult.Incomplete:
					
					// recover that files to stay
					if (argsDelete.FilesToStay.Count == 0)
					{
						var filesAfterDelete = explorer.GetFiles(new GetFilesEventArgs(null, ExplorerModes.Silent));
						var hashAfterDelete = Works.Kit.HashFiles(filesAfterDelete, explorer.FileComparer);
						foreach (var file in efilesToDelete)
							if (hashAfterDelete.ContainsKey(file))
								argsDelete.FilesToStay.Add(file);
					}

					// convert that files to this files to stay
					foreach (SuperFile xfile in SuperFile.SuperFilesOfExplorerFiles(xfilesToDelete, argsDelete.FilesToStay, explorer.FileComparer))
					{
						xfilesToDelete.Remove(xfile);
						xfilesToStay.Add(xfile);
					}

					break;
			}

			// remove remaining super files
			foreach (var file in xfilesToDelete)
				_Cache.Remove(file);

			return argsDelete.Result;
		}
	}
}
