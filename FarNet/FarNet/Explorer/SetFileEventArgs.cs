
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Update file from file arguments.
/// </summary>
public class SetFileEventArgs : ExplorerFileEventArgs
{
	/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
	/// <param name="file">See <see cref="ExplorerFileEventArgs.File"/></param>
	/// <param name="fileName">See <see cref="FileName"/></param>
	public SetFileEventArgs(ExplorerModes mode, FarFile file, string fileName) : base(mode, file)
	{
		FileName = fileName;
	}

	/// <summary>
	/// Gets the source file path.
	/// </summary>
	public string FileName { get; }
}
