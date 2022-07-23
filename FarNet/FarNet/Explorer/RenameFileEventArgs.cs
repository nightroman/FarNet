
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Rename file arguments.
/// </summary>
public sealed class RenameFileEventArgs : ExplorerFileEventArgs
{
	/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
	/// <param name="file">See <see cref="ExplorerFileEventArgs.File"/></param>
	public RenameFileEventArgs(ExplorerModes mode, FarFile file) : base(mode, file)
	{
	}
}
