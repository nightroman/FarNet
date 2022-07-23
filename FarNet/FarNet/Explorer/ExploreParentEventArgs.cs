
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Arguments of <see cref="Explorer.ExploreParent"/>.
/// </summary>
public sealed class ExploreParentEventArgs : ExploreEventArgs
{
	/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
	public ExploreParentEventArgs(ExplorerModes mode) : base(mode)
	{
	}
}
