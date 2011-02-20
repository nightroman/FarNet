
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
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
		/// It implements <see cref="Explorer.ExploreLocation"/>, works with paths and files without <see cref="FarFile.Data"/>.
		/// </summary>
		ExploreLocation = 1 << 0,
		/// <summary>
		/// It implements <see cref="Explorer.ExportFile"/> and is able to export some files.
		/// </summary>
		ExportFile = 1 << 1,
		/// <summary>
		/// It implements <see cref="Explorer.ImportFile"/> and is able to import some files. See also <see cref="ExportFileArgs.CanImport"/>.
		/// </summary>
		ImportFile = 1 << 2,
		/// <summary>
		/// It implements <see cref="Explorer.DeleteFiles"/>.
		/// </summary>
		DeleteFiles = 1 << 3,
	}

	/// <summary>
	/// Explorer mode flags.
	/// </summary>
	[Flags]
	public enum ExplorerModes
	{
		/// <summary>
		/// Nothing special.
		/// </summary>
		None = 0,
		/// <summary>
		/// A job should minimize user requests because it is only a part of a more complex file operation.
		/// </summary>
		Silent = 0x0001,
		/// <summary>
		/// A job is called from the find file or another scanning command. Screen output has to be minimized.
		/// </summary>
		Find = 0x0002,
		/// <summary>
		/// A job is a part of the file view operation.
		/// If a file is opened in the quick view panel, than both <c>View</c> and <c>QuickView</c> are set.
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
		/// A job is called to export or import files with file descriptions.
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
		/// The job is done.
		/// </summary>
		Done,
		/// <summary>
		/// The job is not done and the core should not use default methods.
		/// Example: a method is supposed to process only special files.
		/// It is implemented and called but it ignores some files.
		/// </summary>
		Ignore,
		/// <summary>
		/// The job is not done and the core should do the job as if a method is not implemented.
		/// Example: a method is implemented but it is only a wrapper of an actual worker.
		/// If a worker is optional and not specified then the core should work itself.
		/// </summary>
		Default,
	}

	/// <summary>
	/// Common explorer method arguments.
	/// </summary>
	public class ExplorerArgs
	{
		///
		public ExplorerArgs(ExplorerModes mode) { Mode = mode; }
		/// <summary>
		/// Gets the explorer mode.
		/// </summary>
		public ExplorerModes Mode { get; private set; }
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
	}

	/// <summary>
	/// Explore location arguments.
	/// </summary>
	public sealed class ExploreLocationArgs : ExplorerArgs
	{
		///
		public ExploreLocationArgs(ExplorerModes mode, string location) : base(mode) { Location = location; }
		/// <summary>
		/// Gets the location.
		/// </summary>
		public string Location { get; private set; }
	}

	/// <summary>
	/// Arguments of methods operating on a single file.
	/// </summary>
	public abstract class ExplorerFileArgs : ExplorerArgs
	{
		///
		protected ExplorerFileArgs(ExplorerModes mode, FarFile file) : base(mode) { File = file; }
		/// <summary>
		/// Gets the file to be processed.
		/// </summary>
		public FarFile File { get; private set; }
	}

	/// <summary>
	/// Arguments of <see cref="Explorer.ExploreDirectory"/>
	/// </summary>
	public class ExploreDirectoryArgs : ExplorerFileArgs
	{
		///
		public ExploreDirectoryArgs(ExplorerModes mode, FarFile file) : base(mode, file) { }
	}

	/// <summary>
	/// Arguments of file IO methods.
	/// </summary>
	public abstract class IOFileArgs : ExplorerFileArgs
	{
		///
		protected IOFileArgs(ExplorerModes mode, FarFile file, string fileName) : base(mode, file) { FileName = fileName; }
		/// <summary>
		/// Gets the full file system name.
		/// </summary>
		public string FileName { get; private set; }
	}

	/// <summary>
	/// Export file arguments.
	/// </summary>
	public class ExportFileArgs : IOFileArgs
	{
		///
		public ExportFileArgs(ExplorerModes mode, FarFile file, string fileName) : base(mode, file, fileName) { }
		/// <summary>
		/// Tells that the file can be imported back.
		/// </summary>
		/// <remarks>
		/// Use case. The core opens the file in the editor. By default the editor is locked:
		/// the core assumes the changes will be lost. This flag tells to not lock the editor.
		/// </remarks>
		public bool CanImport { get; set; }
		/// <summary>
		/// Gets or sets the optional recommended extension.
		/// </summary>
		/// <remarks>
		/// It is used on opening the file in the editor.
		/// The extension may be useful in order to get proper syntax highlighting with the <i>Colorer</i> plugin.
		/// </remarks>
		public string FileNameExtension { get; set; }
	}

	/// <summary>
	/// Import file arguments.
	/// </summary>
	public class ImportFileArgs : IOFileArgs
	{
		///
		public ImportFileArgs(ExplorerModes mode, FarFile file, string fileName) : base(mode, file, fileName) { }
	}

	/// <summary>
	/// Common arguments of batch file methods.
	/// </summary>
	public abstract class ExplorerFilesArgs : ExplorerArgs
	{
		///
		protected ExplorerFilesArgs(ExplorerModes mode, IList<FarFile> files) : base(mode) { Files = files; }
		/// <summary>
		/// Gets the files to be processed.
		/// </summary>
		/// <remarks>
		/// Explorers must not change the list unless this is allowed.
		/// </remarks>
		public IList<FarFile> Files { get; private set; }
	}

	/// <summary>
	/// Delete files arguments.
	/// </summary>
	public class DeleteFilesArgs : ExplorerFilesArgs
	{
		///
		public DeleteFilesArgs(ExplorerModes mode, IList<FarFile> files) : base(mode, files) { }
	}
}
