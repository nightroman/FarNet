
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Arguments of <see cref="Explorer.ExploreRoot"/>.
/// </summary>
/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
public sealed class ExploreRootEventArgs(ExplorerModes mode) : ExploreEventArgs(mode)
{
}
