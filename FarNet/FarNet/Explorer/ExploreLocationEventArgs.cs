
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Explore location arguments.
/// </summary>
public sealed class ExploreLocationEventArgs : ExploreEventArgs
{
	/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
	/// <param name="location">See <see cref="Location"/></param>
	public ExploreLocationEventArgs(ExplorerModes mode, string location) : base(mode)
	{
		Location = location;
	}

	/// <summary>
	/// Gets the location.
	/// </summary>
	public string Location { get; }
}
