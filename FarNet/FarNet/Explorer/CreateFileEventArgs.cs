
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Create file arguments.
/// </summary>
public sealed class CreateFileEventArgs : ExplorerEventArgs
{
	/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
	public CreateFileEventArgs(ExplorerModes mode) : base(mode)
	{
	}
}
