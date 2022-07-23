
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Collections.Generic;

namespace FarNet;

/// <summary>
/// Delete files arguments.
/// </summary>
public class DeleteFilesEventArgs : ExplorerFilesEventArgs
{
	/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
	/// <param name="files">See <see cref="ExplorerFilesEventArgs.Files"/></param>
	/// <param name="force">See <see cref="Force"/></param>
	public DeleteFilesEventArgs(ExplorerModes mode, IList<FarFile> files, bool force) : base(mode, files)
	{
		Force = force;
	}

	/// <summary>
	/// Gets the force mode, e.g. on [ShiftDel] instead of [Del].
	/// </summary>
	public bool Force { get; }
}
