
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
		/// <summary>
		/// The explorer type ID string.
		/// </summary>
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
				ExplorerFunctions.ExportFiles |
				ExplorerFunctions.GetContent |
				ExplorerFunctions.SetFile |
				ExplorerFunctions.SetText |
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
					return explorer.ExploreLocation(new ExploreLocationEventArgs(mode, file.Name));
				else
					return explorer.ExploreDirectory(new ExploreDirectoryEventArgs(mode, file));
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
		/// <param name="files">The files to be added.</param>
		public void AddFiles(IEnumerable<FarFile> files)
		{
			if (files == null)
				return;

			//! cast/check and add
			foreach (SuperFile file in files)
				_Cache.Add(file);
		}
		/// <inheritdoc/>
		public override Panel CreatePanel()
		{
			return new SuperPanel(this);
		}
		/// <inheritdoc/>
		public override IList<FarFile> GetFiles(GetFilesEventArgs args)
		{
			return _Cache;
		}
		/// <inheritdoc/>
		public override Explorer ExploreDirectory(ExploreDirectoryEventArgs args)
		{
			if (args == null) return null;

			var xfile = (SuperFile)args.File;
			return ExploreSuperDirectory(xfile.Explorer, args.Mode, xfile.File);
		}
		/// <inheritdoc/>
		public override Explorer OpenFile(OpenFileEventArgs args)
		{
			if (args == null) return null;

			// can?
			var xfile = (SuperFile)args.File;
			if (!xfile.Explorer.CanOpenFile)
			{
				args.Result = JobResult.Ignore;
				return null;
			}

			// call
			var args2 = new OpenFileEventArgs(xfile.File);
			var explorer = xfile.Explorer.OpenFile(args2);
			args.Result = args2.Result;
			return explorer;
		}
		/// <inheritdoc/>
		public override void AcceptFiles(AcceptFilesEventArgs args)
		{
			if (args == null) return;

			foreach (var file in args.Files)
			{
				var xfile = file as SuperFile;
				if (xfile == null)
					Cache.Add(new SuperFile(args.Explorer, file));
				else
					Cache.Add(xfile);
			}
		}
		/// <inheritdoc/>
		public override void GetContent(GetContentEventArgs args)
		{
			if (args == null) return;

			// check
			var xfile = (SuperFile)args.File;
			if (!xfile.Explorer.CanGetContent)
			{
				args.Result = JobResult.Default;
				return;
			}

			// call
			var argsExport = new GetContentEventArgs(args.Mode, xfile.File, args.FileName);
			xfile.Explorer.GetContent(argsExport);

			// results
			args.Result = argsExport.Result;
			args.UseText = argsExport.UseText;
			args.CanSet = argsExport.CanSet;
			args.UseFileName = argsExport.UseFileName;
			args.UseFileExtension = argsExport.UseFileExtension;
		}
		/// <inheritdoc/>
		public override void SetFile(SetFileEventArgs args)
		{
			if (args == null) return;

			// call
			var xfile = (SuperFile)args.File;
			if (!xfile.Explorer.CanSetFile)
			{
				args.Result = JobResult.Default;
				return;
			}
			var argsImport = new SetFileEventArgs(args.Mode, xfile.File, args.FileName);
			xfile.Explorer.SetFile(argsImport);

			// result
			args.Result = argsImport.Result;
		}
		/// <inheritdoc/>
		public override void SetText(SetTextEventArgs args)
		{
			if (args == null) return;

			// call
			var xfile = (SuperFile)args.File;
			if (!xfile.Explorer.CanSetText)
			{
				args.Result = JobResult.Default;
				return;
			}
			var argsImport = new SetTextEventArgs(args.Mode, xfile.File, args.Text);
			xfile.Explorer.SetText(argsImport);

			// result
			args.Result = argsImport.Result;
		}
		static Dictionary<Guid, Dictionary<Explorer, List<SuperFile>>> GroupFiles(IList<FarFile> files, ExplorerFunctions function)
		{
			var result = new Dictionary<Guid, Dictionary<Explorer, List<SuperFile>>>();
			foreach (SuperFile file in files)
			{
				if (function != ExplorerFunctions.None && 0 == (file.Explorer.Functions & function))
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
		internal void CommitFiles(SuperPanel source, Panel target, IList<FarFile> files, bool move)
		{
			var dicTypeId = GroupFiles(files, ExplorerFunctions.None);

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
					var argsAccept = new AcceptFilesEventArgs(ExplorerModes.None, filesToAccept, move, explorer);
					argsAccept.Data = codata;
					target.UIAcceptFiles(argsAccept);
					codata = argsAccept.Data;

					// info
					bool isIncomplete = argsAccept.Result == JobResult.Incomplete;
					bool isAllToStay = isIncomplete && argsAccept.FilesToStay.Count == 0;

					// Copy: do not update the source, files are the same
					if (!move)
					{
						// keep it as it is
						if (isAllToStay || !SelectionExists)
						{
							if (isAllToStay && SelectionExists)
								foreach(var file in xfiles)
									xfilesToStay.Add(file);
							continue;
						}

						// drop selection
						toUnselect = true;

						// recover
						if (isIncomplete)
							xfilesToStay.AddRange(SuperFile.SuperFilesOfExplorerFiles(xfiles, argsAccept.FilesToStay, explorer.FileComparer));

						continue;
					}

					// Move: no need to delete or all to stay or cannot delete
					if (!argsAccept.ToDeleteFiles || isAllToStay || !explorer.CanDeleteFiles)
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
			var argsDelete = new DeleteFilesEventArgs(mode, efilesToDelete, force);
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
						var filesAfterDelete = explorer.GetFiles(new GetFilesEventArgs(ExplorerModes.Silent));
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
		/// <inheritdoc/>
		public override void DeleteFiles(DeleteFilesEventArgs args)
		{
			if (args == null) return;

			var dicTypeId = GroupFiles(args.Files, ExplorerFunctions.DeleteFiles);

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
		/// <inheritdoc/>
		public override void ExportFiles(ExportFilesEventArgs args)
		{
			if (args == null) return;
			
			var dicTypeId = GroupFiles(args.Files, ExplorerFunctions.ExportFiles);

			foreach (var xTypeId in dicTypeId)
			{
				Log.Source.TraceInformation("ExportFiles TypeId='{0}'", xTypeId.Key);
				object codata = null;
				foreach (var kv in xTypeId.Value)
				{
					// explorer and its files
					var explorer = kv.Key;
					var xfiles = kv.Value;
					var filesToExport = new List<FarFile>(xfiles.Count);
					foreach (var file in xfiles)
						filesToExport.Add(file.File);

					// export, mind co-data
					Log.Source.TraceInformation("ExportFiles Count='{0}' Location='{1}' DirectoryName='{2}'", filesToExport.Count, explorer.Location, args.DirectoryName);
					var argsExport = new ExportFilesEventArgs(ExplorerModes.None, filesToExport, args.Move, args.DirectoryName);
					argsExport.Data = codata;
					explorer.ExportFiles(argsExport);
					codata = argsExport.Data;

					// info
					bool isIncomplete = argsExport.Result == JobResult.Incomplete;
					bool isAllToStay = isIncomplete && argsExport.FilesToStay.Count == 0;
					if (isIncomplete)
						args.Result = JobResult.Incomplete;

					// Copy: do not update the source, files are the same
					if (!args.Move)
					{
						// keep it as it is
						if (isAllToStay)
						{
							foreach (var file in xfiles)
								args.FilesToStay.Add(file);
							continue;
						}

						// recover
						if (isIncomplete)
							foreach(var file in SuperFile.SuperFilesOfExplorerFiles(xfiles, argsExport.FilesToStay, explorer.FileComparer))
								args.FilesToStay.Add(file);
						
						continue;
					}

					// Move: no need to delete or all to stay or cannot delete
					if (!argsExport.ToDeleteFiles || isAllToStay || !explorer.CanDeleteFiles)
					{
						// recover selection
						if (isIncomplete)
						{
							var filesToStay = isAllToStay ? argsExport.Files : argsExport.FilesToStay;
							foreach(var file in SuperFile.SuperFilesOfExplorerFiles(xfiles, filesToStay, explorer.FileComparer))
								args.FilesToStay.Add(file);
						}

						continue;
					}

					// Move: delete is requested, delete the source files

					// exclude this files to stay from to be deleted
					if (isIncomplete)
					{
						foreach (SuperFile xfile in SuperFile.SuperFilesOfExplorerFiles(xfiles, argsExport.FilesToStay, explorer.FileComparer))
						{
							xfiles.Remove(xfile);
							args.FilesToStay.Add(xfile);
						}
					}

					// call delete on remaining files
					object codata2 = null;
					var result = DeleteFilesOfExplorer(explorer, xfiles, args.FilesToStay, ExplorerModes.Silent, false, ref codata2);
					if (result == JobResult.Incomplete)
						args.Result = JobResult.Incomplete;
				}
			}
		}
	}
}
