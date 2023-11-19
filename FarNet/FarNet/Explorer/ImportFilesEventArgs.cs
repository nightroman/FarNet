
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Collections.Generic;

namespace FarNet;

/// <summary>
/// Import files arguments.
/// </summary>
/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
/// <param name="files">See <see cref="ExplorerFilesEventArgs.Files"/></param>
/// <param name="move">See <see cref="Move"/></param>
/// <param name="directoryName">See <see cref="DirectoryName"/></param>
public sealed class ImportFilesEventArgs(ExplorerModes mode, IList<FarFile> files, bool move, string directoryName) : ExplorerFilesEventArgs(mode, files)
{
	/// <summary>
	/// Tells that the files are moved.
	/// </summary>
	public bool Move { get; } = move;

	/// <summary>
	/// The source directory name.
	/// </summary>
	public string DirectoryName { get; } = directoryName;
}
