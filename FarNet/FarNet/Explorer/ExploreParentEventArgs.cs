
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Arguments of <see cref="Explorer.ExploreParent"/>.
/// </summary>
/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
public sealed class ExploreParentEventArgs(ExplorerModes mode) : ExploreEventArgs(mode)
{
}
