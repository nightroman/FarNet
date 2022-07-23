
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Arguments of <see cref="Explorer.ExploreDirectory"/>
/// </summary>
public sealed class ExploreDirectoryEventArgs : ExploreEventArgs
{
	/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
	/// <param name="file">See <see cref="File"/></param>
	public ExploreDirectoryEventArgs(ExplorerModes mode, FarFile file) : base(mode)
	{
		File = file;
	}

	/// <summary>
	/// Gets the directory file to explore.
	/// </summary>
	public FarFile File { get; }
}
