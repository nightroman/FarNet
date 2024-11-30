
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FarNet;

/// <summary>
/// Common arguments of batch file methods.
/// </summary>
/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
/// <param name="files">See <see cref="Files"/></param>
public abstract class ExplorerFilesEventArgs(ExplorerModes mode, IList<FarFile> files) : ExplorerEventArgs(mode)
{
	/// <summary>
	/// Gets the files to be processed.
	/// </summary>
	/// <remarks>
	/// Explorers must not change the list unless this is allowed.
	/// </remarks>
	public IList<FarFile> Files => files;

	/// <summary>
	/// Gets data attached to <see cref="Files"/>.
	/// </summary>
	public IEnumerable FilesData => Files.Select(x => x.Data);

	/// <summary>
	/// Gets the list of source files to stay selected and not deleted on move if the job is incomplete.
	/// </summary>
	/// <remarks>
	/// If the job is <see cref="JobResult.Incomplete"/> then not processed files should normally stay selected
	/// and not deleted on move if the core is told to delete files. Such files have to added to this list.
	/// <para>
	/// If the list is empty and the job is incomplete then all input files
	/// that still exist in the source stay selected and not deleted.
	/// </para>
	/// <para>
	/// It is important that the files added to this list must be taken from the input file list.
	/// </para>
	/// <para>
	/// Choose a proper <see cref="FarNet.Explorer.FileComparer"/> otherwise source files that should stay
	/// may lose selection or even may be deleted because the comparer cannot find them.
	/// </para>
	/// </remarks>
	public IList<FarFile> FilesToStay => _FilesToStay;
	readonly List<FarFile> _FilesToStay = [];
}
