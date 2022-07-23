
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Collections.Generic;

namespace FarNet;

/// <summary>
/// Export files arguments.
/// </summary>
public sealed class ExportFilesEventArgs : CopyFilesEventArgs
{
	/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
	/// <param name="files">See <see cref="ExplorerFilesEventArgs.Files"/></param>
	/// <param name="move">See <see cref="CopyFilesEventArgs.Move"/></param>
	/// <param name="directoryName">See <see cref="DirectoryName"/></param>
	public ExportFilesEventArgs(ExplorerModes mode, IList<FarFile> files, bool move, string directoryName)
		: base(mode, files, move)
	{
		DirectoryName = directoryName;
	}

	/// <summary>
	/// The target directory name.
	/// </summary>
	public string DirectoryName { get; }
}
