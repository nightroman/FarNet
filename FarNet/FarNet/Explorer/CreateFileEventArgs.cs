
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Create file arguments.
/// </summary>
/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
public sealed class CreateFileEventArgs(ExplorerModes mode) : ExplorerEventArgs(mode)
{
}
