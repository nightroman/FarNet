
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Open file arguments.
/// </summary>
/// <param name="file">See <see cref="ExplorerFileEventArgs.File"/></param>
public sealed class OpenFileEventArgs(FarFile file) : ExplorerFileEventArgs(ExplorerModes.None, file)
{
}
