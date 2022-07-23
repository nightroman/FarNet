
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Arguments of methods operating on a single file.
/// </summary>
public abstract class ExplorerFileEventArgs : ExplorerEventArgs
{
	/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
	/// <param name="file">See <see cref="File"/></param>
	protected ExplorerFileEventArgs(ExplorerModes mode, FarFile file) : base(mode)
	{
		File = file;
	}

	/// <summary>
	/// Gets the file to be processed.
	/// </summary>
	public FarFile File { get; }
}
