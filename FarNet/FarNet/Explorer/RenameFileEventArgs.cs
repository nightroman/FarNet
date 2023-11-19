
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Rename file arguments.
/// </summary>
/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
/// <param name="file">See <see cref="ExplorerFileEventArgs.File"/></param>
public sealed class RenameFileEventArgs(ExplorerModes mode, FarFile file) : ExplorerFileEventArgs(mode, file)
{
}
