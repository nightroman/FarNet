
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Open file arguments.
/// </summary>
public sealed class OpenFileEventArgs : ExplorerFileEventArgs
{
	/// <param name="file">See <see cref="ExplorerFileEventArgs.File"/></param>
	public OpenFileEventArgs(FarFile file) : base(ExplorerModes.None, file)
	{
	}
}
