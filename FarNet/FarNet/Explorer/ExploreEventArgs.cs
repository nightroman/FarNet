
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Arguments of ExploreX methods.
/// </summary>
public class ExploreEventArgs : ExplorerEventArgs
{
	/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
	public ExploreEventArgs(ExplorerModes mode) : base(mode)
	{
	}

	/// <summary>
	/// Tells to create a new panel even if the new explorer has the same type as the current.
	/// </summary>
	public bool NewPanel { get; set; }
}
