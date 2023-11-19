
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Collections.Generic;

namespace FarNet;

/// <summary>
/// Accept files arguments.
/// </summary>
/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
/// <param name="files">See <see cref="ExplorerFilesEventArgs.Files"/></param>
/// <param name="move">See <see cref="CopyFilesEventArgs.Move"/></param>
/// <param name="explorer">See <see cref="Explorer"/></param>
public sealed class AcceptFilesEventArgs(ExplorerModes mode, IList<FarFile> files, bool move, Explorer explorer) : CopyFilesEventArgs(mode, files, move)
{
	/// <summary>
	/// Gets the source file explorer.
	/// </summary>
	public Explorer Explorer { get; } = explorer;
}
