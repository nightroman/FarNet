
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Arguments of methods operating on a single file.
/// </summary>
/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
/// <param name="file">See <see cref="File"/></param>
public abstract class ExplorerFileEventArgs(ExplorerModes mode, FarFile file) : ExplorerEventArgs(mode)
{
	/// <summary>
	/// Gets the file to be processed.
	/// </summary>
	public FarFile File => file;
}
