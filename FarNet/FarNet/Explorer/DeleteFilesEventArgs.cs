
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Collections.Generic;

namespace FarNet;

/// <summary>
/// Delete files arguments.
/// </summary>
/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
/// <param name="files">See <see cref="ExplorerFilesEventArgs.Files"/></param>
/// <param name="force">See <see cref="Force"/></param>
public class DeleteFilesEventArgs(ExplorerModes mode, IList<FarFile> files, bool force) : ExplorerFilesEventArgs(mode, files)
{
	/// <summary>
	/// Gets the force mode, e.g. on [ShiftDel] instead of [Del].
	/// </summary>
	public bool Force { get; } = force;
}
