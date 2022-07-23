
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Arguments of <see cref="Explorer.ExploreRoot"/>.
/// </summary>
public sealed class ExploreRootEventArgs : ExploreEventArgs
{
	/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
	public ExploreRootEventArgs(ExplorerModes mode) : base(mode)
	{
	}
}
