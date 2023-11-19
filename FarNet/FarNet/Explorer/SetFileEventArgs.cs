
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Update file from file arguments.
/// </summary>
/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
/// <param name="file">See <see cref="ExplorerFileEventArgs.File"/></param>
/// <param name="fileName">See <see cref="FileName"/></param>
public class SetFileEventArgs(ExplorerModes mode, FarFile file, string fileName) : ExplorerFileEventArgs(mode, file)
{
	/// <summary>
	/// Gets the source file path.
	/// </summary>
	public string FileName { get; } = fileName;
}
