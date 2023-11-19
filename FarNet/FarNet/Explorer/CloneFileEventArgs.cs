
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Clone file arguments.
/// </summary>
/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
/// <param name="file">See <see cref="ExplorerFileEventArgs.File"/></param>
public sealed class CloneFileEventArgs(ExplorerModes mode, FarFile file) : ExplorerFileEventArgs(mode, file)
{
}
