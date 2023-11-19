
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Collections.Generic;

namespace FarNet;

/// <summary>
/// Export files arguments.
/// </summary>
/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
/// <param name="files">See <see cref="ExplorerFilesEventArgs.Files"/></param>
/// <param name="move">See <see cref="CopyFilesEventArgs.Move"/></param>
/// <param name="directoryName">See <see cref="DirectoryName"/></param>
public sealed class ExportFilesEventArgs(ExplorerModes mode, IList<FarFile> files, bool move, string directoryName) : CopyFilesEventArgs(mode, files, move)
{
	/// <summary>
	/// The target directory name.
	/// </summary>
	public string DirectoryName { get; } = directoryName;
}
