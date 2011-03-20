
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace FarNet
{
	/// <summary>
	/// Defines the functions that are either implemented or supported by the explorer.
	/// </summary>
	[Flags]
	public enum ExplorerFunctions
	{
		///
		None,
		/// <summary>
		/// It implements <see cref="Explorer.ExploreLocation"/>, works with pure paths, i.e. files without <see cref="FarFile.Data"/>.
		/// </summary>
		ExploreLocation = 1 << 0,
		/// <summary>
		/// It implements <see cref="Explorer.AcceptFiles"/>.
		/// </summary>
		AcceptFiles = 1 << 1,
		/// <summary>
		/// It implements <see cref="Explorer.ImportFiles"/>.
		/// </summary>
		ImportFiles = 1 << 2,
		/// <summary>
		/// It implements <see cref="Explorer.ExportFiles"/>.
		/// </summary>
		ExportFiles = 1 << 3,
		/// <summary>
		/// It implements <see cref="Explorer.DeleteFiles"/>.
		/// </summary>
		DeleteFiles = 1 << 4,
		/// <summary>
		/// It implements <see cref="Explorer.CreateFile"/>.
		/// </summary>
		CreateFile = 1 << 5,
		/// <summary>
		/// It implements <see cref="Explorer.GetContent"/>.
		/// </summary>
		GetContent = 1 << 6,
		/// <summary>
		/// It implements <see cref="Explorer.SetFile"/>.
		/// </summary>
		SetFile = 1 << 7,
		/// <summary>
		/// It implements <see cref="Explorer.SetText"/>.
		/// </summary>
		SetText = 1 << 8,
		/// <summary>
		/// It implements <see cref="Explorer.OpenFile"/>.
		/// </summary>
		OpenFile = 1 << 9,
		/// <summary>
		/// It implements <see cref="Explorer.RenameFile"/>.
		/// </summary>
		RenameFile = 1 << 10,
	}

	/// <summary>
	/// Explorer call mode flags.
	/// </summary>
	[Flags]
	public enum ExplorerModes
	{
		///
		None = 0,
		/// <summary>
		/// A job should not interact with a user because it is a part of another job that does not want this.
		/// </summary>
		Silent = 0x0001,
		/// <summary>
		/// A job is called from a search or scan operation; screen output and user interaction should be avoided.
		/// </summary>
		Find = 0x0002,
		/// <summary>
		/// A job is a part of the file view operation.
		/// If a file is opened in the quick view panel, than the <c>View</c> and <c>QuickView</c> flags are both set.
		/// </summary>
		View = 0x0004,
		/// <summary>
		/// A job is a part of a file edit operation.
		/// </summary>
		Edit = 0x0008,
		/// <summary>
		/// All files in a host file of file based panel should be processed.
		/// This flag is set on [ShiftF2], [ShiftF3] commands outside of a host file.
		/// Passed in an operation file list also contains all necessary information,
		/// so that this flag can be either ignored or used to speed up processing.
		/// </summary>
		TopLevel = 0x0010,
		/// <summary>
		/// A job is called for files with file descriptions.
		/// </summary>
		Description = 0x0020,
		/// <summary>
		/// A job is a part of a file view operation in the quick view panel ([CtrlQ]).
		/// </summary>
		QuickView = 0x0040,
	}

	/// <summary>
	/// Job results.
	/// </summary>
	public enum JobResult
	{
		/// <summary>
		/// A job is done.
		/// </summary>
		Done,
		/// <summary>
		/// A job is not done and the core should not use default methods.
		/// Example: a method is supposed to process only special files.
		/// It is implemented and called but it ignores some files.
		/// </summary>
		Ignore,
		/// <summary>
		/// A job is not done and the core should do the job as if a method is not implemented.
		/// Example: a method is implemented but it is only a wrapper of an actual worker.
		/// If a worker is optional and not specified then the core should work itself.
		/// </summary>
		Default,
		/// <summary>
		/// A job is done but not completely. The core should try to recover if possible.
		/// </summary>
		Incomplete,
	}

	/// <summary>
	/// Common explorer method arguments.
	/// </summary>
	public abstract class ExplorerEventArgs : EventArgs
	{
		///
		protected ExplorerEventArgs(ExplorerModes mode) { Mode = mode; }
		/// <summary>
		/// Gets the explorer mode.
		/// </summary>
		public ExplorerModes Mode { get; private set; }
		/// <summary>
		/// Gets or sets the parameter to be used by the explorer.
		/// </summary>
		public object Parameter { get; set; }
		/// <summary>
		/// Gets or sets the job result.
		/// </summary>
		public JobResult Result { get; set; }
		/// <summary>
		/// Gets or sets any co-explorer data (not used by the core).
		/// </summary>
		/// <remarks>
		/// Use case. Some complex operation has to be performed by several calls to one or more co-explorers.
		/// On the first call an explorer may ask a user for extra options and keep this information in here.
		/// On the next calls this or another co-explorer uses this information.
		/// </remarks>
		public object Data { get; set; }
		/// <summary>
		/// To be set current.
		/// </summary>
		public object PostData { get; set; }
		/// <summary>
		/// To be set current.
		/// </summary>
		public FarFile PostFile { get; set; }
		/// <summary>
		/// To be set current.
		/// </summary>
		public string PostName { get; set; }
		/// <summary>
		/// Tells whether user interaction is allowed.
		/// </summary>
		public bool UI { get { return 0 == (Mode & (ExplorerModes.Find | ExplorerModes.Silent)); } }
		/// <summary>
		/// Casts the not null <see cref="Parameter"/> to the specified type or creates and returns a new instance.
		/// </summary>
		/// <typeparam name="T">The type of the parameter to be created or converted to.</typeparam>
		/// <exception cref="InvalidCastException">The parameter cannot be converted to the specified type.</exception>
		public T ParameterOrDefault<T>() where T : class, new()
		{
			return Parameter == null ? new T() : (T)Parameter;
		}
	}

	/// <summary>
	/// <see cref="Explorer.GetFiles"/> arguments.
	/// </summary>
	public class GetFilesEventArgs : ExplorerEventArgs
	{
		///
		public GetFilesEventArgs(ExplorerModes mode) : base(mode) { }
	}

	/// <summary>
	/// <see cref="Explorer.CreateFile"/> arguments.
	/// </summary>
	public class CreateFileEventArgs : ExplorerEventArgs
	{
		///
		public CreateFileEventArgs(ExplorerModes mode) : base(mode) { }
	}

	/// <summary>
	/// Arguments of ExploreX methods.
	/// </summary>
	public class ExploreEventArgs : ExplorerEventArgs
	{
		///
		public ExploreEventArgs(ExplorerModes mode) : base(mode) { }
		/// <summary>
		/// Tells to create a new panel even if the new explorer has the same type as the current.
		/// </summary>
		public bool NewPanel { get; set; }
	}

	/// <summary>
	/// Arguments of <see cref="Explorer.ExploreDirectory"/>
	/// </summary>
	public sealed class ExploreDirectoryEventArgs : ExploreEventArgs
	{
		///
		public ExploreDirectoryEventArgs(ExplorerModes mode, FarFile file) : base(mode) { File = file; }
		/// <summary>
		/// Gets the directory file to explore.
		/// </summary>
		public FarFile File { get; private set; }
	}

	/// <summary>
	/// Explore location arguments.
	/// </summary>
	public sealed class ExploreLocationEventArgs : ExploreEventArgs
	{
		///
		public ExploreLocationEventArgs(ExplorerModes mode, string location) : base(mode) { Location = location; }
		/// <summary>
		/// Gets the location.
		/// </summary>
		public string Location { get; private set; }
	}

	/// <summary>
	/// Arguments of <see cref="Explorer.ExploreParent"/>.
	/// </summary>
	public sealed class ExploreParentEventArgs : ExploreEventArgs
	{
		///
		public ExploreParentEventArgs(ExplorerModes mode) : base(mode) { }
	}

	/// <summary>
	/// Arguments of <see cref="Explorer.ExploreRoot"/>.
	/// </summary>
	public sealed class ExploreRootEventArgs : ExploreEventArgs
	{
		///
		public ExploreRootEventArgs(ExplorerModes mode) : base(mode) { }
	}

	/// <summary>
	/// Arguments of methods operating on a single file.
	/// </summary>
	public abstract class ExplorerFileEventArgs : ExplorerEventArgs
	{
		///
		protected ExplorerFileEventArgs(ExplorerModes mode, FarFile file) : base(mode) { File = file; }
		/// <summary>
		/// Gets the file to be processed.
		/// </summary>
		public FarFile File { get; private set; }
	}

	/// <summary>
	/// Open file arguments.
	/// </summary>
	public sealed class OpenFileEventArgs : ExplorerFileEventArgs
	{
		///
		public OpenFileEventArgs(FarFile file) : base(ExplorerModes.None, file) { }
	}

	/// <summary>
	/// Rename file arguments.
	/// </summary>
	public sealed class RenameFileEventArgs : ExplorerFileEventArgs
	{
		///
		public RenameFileEventArgs(ExplorerModes mode, FarFile file, string newName) : base(mode, file) { NewName = newName; }
		/// <summary>
		/// Gets the new name.
		/// </summary>
		public string NewName { get; private set; }
	}

	/// <summary>
	/// Export file arguments.
	/// </summary>
	public class GetContentEventArgs : ExplorerFileEventArgs
	{
		///
		public GetContentEventArgs(ExplorerModes mode, FarFile file, string fileName) : base(mode, file) { FileName = fileName; }
		/// <summary>
		/// Gets the destination file path.
		/// </summary>
		public string FileName { get; private set; }
		/// <summary>
		/// Tells that the file can be updated.
		/// </summary>
		/// <remarks>
		/// Use case. The core opens the file in the editor. By default the editor is locked:
		/// the core assumes the changes will be lost. This flag tells to not lock the editor.
		/// </remarks>
		public bool CanSet { get; set; }
		/// <summary>
		/// Gets or set the exported text.
		/// </summary>
		/// <remarks>
		/// It can be a string or an object to be converted by <c>ToString</c>
		/// or a collection of objects to be converted to lines by <c>ToString</c>.
		/// </remarks>
		public object UseText { get; set; }
		/// <summary>
		/// Gets or set the actual source file name to be used instead.
		/// </summary>
		public string UseFileName { get; set; }
		/// <summary>
		/// Gets or sets the file extension to use.
		/// </summary>
		/// <remarks>
		/// It is used on opening the file in the editor.
		/// The extension may be useful in order to get proper syntax highlighting with the <i>Colorer</i> plugin.
		/// </remarks>
		public string UseFileExtension { get; set; }
	}

	/// <summary>
	/// Update file from file arguments.
	/// </summary>
	public class SetFileEventArgs : ExplorerFileEventArgs
	{
		///
		public SetFileEventArgs(ExplorerModes mode, FarFile file, string fileName) : base(mode, file) { FileName = fileName; }
		/// <summary>
		/// Gets the source file path.
		/// </summary>
		public string FileName { get; private set; }
	}

	/// <summary>
	/// Update file from text arguments.
	/// </summary>
	public class SetTextEventArgs : ExplorerFileEventArgs
	{
		///
		public SetTextEventArgs(ExplorerModes mode, FarFile file, string text) : base(mode, file) { Text = text; }
		/// <summary>
		/// Gets the text to be imported.
		/// </summary>
		public string Text { get; private set; }
	}

	/// <summary>
	/// Common arguments of batch file methods.
	/// </summary>
	public abstract class ExplorerFilesEventArgs : ExplorerEventArgs
	{
		///
		protected ExplorerFilesEventArgs(ExplorerModes mode, IList<FarFile> files) : base(mode) { Files = files; }
		/// <summary>
		/// Gets the files to be processed.
		/// </summary>
		/// <remarks>
		/// Explorers must not change the list unless this is allowed.
		/// </remarks>
		public IList<FarFile> Files { get; private set; }
		/// <summary>
		/// Gets data attached to <see cref="Files"/>.
		/// </summary>
		public IEnumerable FilesData { get { foreach (var it in Files) yield return it.Data; } }
	}

	/// <summary>
	/// Delete files arguments.
	/// </summary>
	public class DeleteFilesEventArgs : ExplorerFilesEventArgs
	{
		///
		public DeleteFilesEventArgs(ExplorerModes mode, IList<FarFile> files, bool force) : base(mode, files) { Force = force; }
		/// <summary>
		/// Gets the force mode, e.g. on [ShiftDel] instead of [Del].
		/// </summary>
		public bool Force { get; private set; }
		/// <summary>
		/// Gets the list of files from <see cref="ExplorerFilesEventArgs.Files"/> to stay selected if the job is incomplete.
		/// </summary>
		/// <remarks>
		/// If the job is incomplete and this list is empty then the core recovers selection itself but this is less effective.
		/// </remarks>
		public IList<FarFile> FilesToStay { get { return _FailedFiles; } }
		readonly List<FarFile> _FailedFiles = new List<FarFile>();
	}

	/// <summary>
	/// Import files arguments.
	/// </summary>
	public sealed class ImportFilesEventArgs : ExplorerFilesEventArgs
	{
		///
		public ImportFilesEventArgs(ExplorerModes mode, IList<FarFile> files, bool move, string directoryName)
			: base(mode, files)
		{
			Move = move;
			DirectoryName = directoryName;
		}
		/// <summary>
		/// Tells that the files are moved.
		/// </summary>
		public bool Move { get; private set; }
		/// <summary>
		/// The source directory name.
		/// </summary>
		public string DirectoryName { get; private set; }
	}

	/// <summary>
	/// Copy files arguments.
	/// </summary>
	public abstract class CopyFilesEventArgs : ExplorerFilesEventArgs
	{
		///
		public CopyFilesEventArgs(ExplorerModes mode, IList<FarFile> files, bool move)
			: base(mode, files)
		{
			Move = move;
		}
		/// <summary>
		/// Tells that the files are moved.
		/// </summary>
		/// <remarks>
		/// On Move an explorer may do only the Copy part of the action and set the <see cref="ToDeleteFiles"/> flag.
		/// In that case the core calls <see cref="FarNet.Explorer.DeleteFiles"/> of the source explorer.
		/// </remarks>
		public bool Move { get; private set; }
		/// <summary>
		/// Tells the core to delete the source files on move.
		/// </summary>
		/// <remarks>
		/// On move the explorer may only copy files and tell the core to delete the source files.
		/// The core does not delete itself, it calls <see cref="FarNet.Explorer.DeleteFiles"/> of the source explorer.
		/// </remarks>
		public bool ToDeleteFiles { get; set; }
		/// <summary>
		/// Gets the list of source files to stay selected and not deleted on move if the job is incomplete.
		/// </summary>
		/// <remarks>
		/// If the job is <see cref="JobResult.Incomplete"/> then not processed files should normally stay selected
		/// and not deleted on Move if <see cref="ToDeleteFiles"/> flag is set. Such files have to added to this list.
		/// <para>
		/// If the list is empty and the job is incomplete then all input files
		/// that still exist in the source stay selected and not deleted.
		/// </para>
		/// <para>
		/// It is important that the files added to this list must be taken from the input file list.
		/// </para>
		/// <para>
		/// Choose a proper <see cref="FarNet.Explorer.FileComparer"/> otherwise source files that should stay
		/// may lose selection or even may be deleted because the comparer does not help to find or exclude them.
		/// </para>
		/// </remarks>
		public IList<FarFile> FilesToStay { get { return _FailedFiles; } }
		readonly List<FarFile> _FailedFiles = new List<FarFile>();
	}

	/// <summary>
	/// Accept files arguments.
	/// </summary>
	public sealed class AcceptFilesEventArgs : CopyFilesEventArgs
	{
		///
		public AcceptFilesEventArgs(ExplorerModes mode, IList<FarFile> files, bool move, Explorer explorer)
			: base(mode, files, move)
		{
			Explorer = explorer;
		}
		/// <summary>
		/// Gets the source file explorer.
		/// </summary>
		public Explorer Explorer { get; private set; }
	}

	/// <summary>
	/// Export files arguments.
	/// </summary>
	public sealed class ExportFilesEventArgs : CopyFilesEventArgs
	{
		///
		public ExportFilesEventArgs(ExplorerModes mode, IList<FarFile> files, bool move, string directoryName)
			: base(mode, files, move)
		{
			DirectoryName = directoryName;
		}
		/// <summary>
		/// The target directory name.
		/// </summary>
		public string DirectoryName { get; private set; }
	}

	/// <summary>
	/// Arguments of the <see cref="Panel.UIExplorerEntered"/> method and the <see cref="Panel.ExplorerEntered"/> event.
	/// </summary>
	public sealed class ExplorerEnteredEventArgs : EventArgs
	{
		///
		public ExplorerEnteredEventArgs(Explorer explorer) { Explorer = explorer; }
		/// <summary>
		/// The old explorer replaced by the new just entered <see cref="Panel.Explorer"/>.
		/// </summary>
		public Explorer Explorer { get; private set; }
	}

}
