
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Clone file arguments.
/// </summary>
public sealed class CloneFileEventArgs : ExplorerFileEventArgs
{
	/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
	/// <param name="file">See <see cref="ExplorerFileEventArgs.File"/></param>
	public CloneFileEventArgs(ExplorerModes mode, FarFile file) : base(mode, file)
	{
	}
}
