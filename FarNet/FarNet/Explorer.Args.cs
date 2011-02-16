
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;

namespace FarNet
{
	/// <summary>
	/// Result of a job.
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
	/// Common arguments of explorer methods.
	/// </summary>
	public class ExplorerArgs
	{
		/// <summary>
		/// Operation mode.
		/// </summary>
		public OperationModes Mode { get; set; }
		/// <summary>
		/// Method call result.
		/// </summary>
		public JobResult Result { get; set; }
	}

	/// <summary>
	/// Arguments of <see cref="Explorer.ExploreFile"/>
	/// </summary>
	public class ExploreFileArgs : ExplorerArgs
	{
		/// <summary>
		/// The file to explore.
		/// </summary>
		public FarFile File { get; set; }
		/// <summary>
		/// Test only.
		/// </summary>
		public bool ToPostName { get; set; }
	}

	/// <summary>
	/// Arguments of file IO methods.
	/// </summary>
	public abstract class IOFileArgs : ExplorerArgs
	{
		/// <summary>
		/// The virtual file or null.
		/// </summary>
		public FarFile File { get; set; }
		/// <summary>
		/// Full path of the system file.
		/// </summary>
		public string FileName { get; set; }
	}

	/// <summary>
	/// Export file arguments.
	/// </summary>
	public class ExportFileArgs : IOFileArgs
	{ }

	/// <summary>
	/// Import file arguments.
	/// </summary>
	public class ImportFileArgs : IOFileArgs
	{ }

	/// <summary>
	/// Common arguments of panel maker methods.
	/// </summary>
	public class PanelMakerArgs : ExplorerArgs
	{
		/// <summary>
		/// The target panel.
		/// </summary>
		public Panel Panel { get; set; }
	}

	/// <summary>
	/// Common arguments of batch file methods.
	/// </summary>
	public abstract class FilesArgs : ExplorerArgs
	{
		/// <summary>
		/// Files to be processed.
		/// </summary>
		/// <remarks>
		/// The list is not read only but it should not be changed unless this is allowed.
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public IList<FarFile> Files { get; set; }
	}

	/// <summary>
	/// Delete files arguments.
	/// </summary>
	public class DeleteFilesArgs : FilesArgs
	{ }

}
