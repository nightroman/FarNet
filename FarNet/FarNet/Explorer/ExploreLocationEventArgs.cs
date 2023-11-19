
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Explore location arguments.
/// </summary>
/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
/// <param name="location">See <see cref="Location"/></param>
public sealed class ExploreLocationEventArgs(ExplorerModes mode, string location) : ExploreEventArgs(mode)
{
	/// <summary>
	/// Gets the location.
	/// </summary>
	public string Location { get; } = location;
}
