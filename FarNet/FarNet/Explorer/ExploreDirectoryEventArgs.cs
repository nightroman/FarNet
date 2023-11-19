
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Arguments of <see cref="Explorer.ExploreDirectory"/>
/// </summary>
/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
/// <param name="file">See <see cref="File"/></param>
public sealed class ExploreDirectoryEventArgs(ExplorerModes mode, FarFile file) : ExploreEventArgs(mode)
{
	/// <summary>
	/// Gets the directory file to explore.
	/// </summary>
	public FarFile File { get; } = file;
}
