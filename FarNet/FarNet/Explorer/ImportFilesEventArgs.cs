
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Collections.Generic;

namespace FarNet;

/// <summary>
/// Import files arguments.
/// </summary>
public sealed class ImportFilesEventArgs : ExplorerFilesEventArgs
{
	/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
	/// <param name="files">See <see cref="ExplorerFilesEventArgs.Files"/></param>
	/// <param name="move">See <see cref="Move"/></param>
	/// <param name="directoryName">See <see cref="DirectoryName"/></param>
	public ImportFilesEventArgs(ExplorerModes mode, IList<FarFile> files, bool move, string directoryName)
		: base(mode, files)
	{
		Move = move;
		DirectoryName = directoryName;
	}

	/// <summary>
	/// Tells that the files are moved.
	/// </summary>
	public bool Move { get; }

	/// <summary>
	/// The source directory name.
	/// </summary>
	public string DirectoryName { get; }
}
