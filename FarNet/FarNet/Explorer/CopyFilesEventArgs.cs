
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Collections.Generic;

namespace FarNet;

/// <summary>
/// Copy files arguments.
/// </summary>
/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
/// <param name="files">See <see cref="ExplorerFilesEventArgs.Files"/></param>
/// <param name="move">See <see cref="Move"/></param>
public abstract class CopyFilesEventArgs(ExplorerModes mode, IList<FarFile> files, bool move) : ExplorerFilesEventArgs(mode, files)
{
	/// <summary>
	/// Tells that the files are moved.
	/// </summary>
	/// <remarks>
	/// On Move an explorer may do only the Copy part of the action and set the <see cref="ToDeleteFiles"/> flag.
	/// In that case the core calls <see cref="FarNet.Explorer.DeleteFiles"/> of the source explorer.
	/// </remarks>
	public bool Move => move;

	/// <summary>
	/// Tells the core to delete the source files on move.
	/// </summary>
	/// <remarks>
	/// On move the explorer may only copy files and tell the core to delete the source files.
	/// The core does not delete itself, it calls <see cref="FarNet.Explorer.DeleteFiles"/> of the source explorer.
	/// </remarks>
	public bool ToDeleteFiles { get; set; }
}
