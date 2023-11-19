
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Update file from text arguments.
/// </summary>
/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
/// <param name="file">See <see cref="ExplorerFileEventArgs.File"/></param>
/// <param name="text">See <see cref="Text"/></param>
public class SetTextEventArgs(ExplorerModes mode, FarFile file, string text) : ExplorerFileEventArgs(mode, file)
{
	/// <summary>
	/// Gets the text to be imported.
	/// </summary>
	public string Text { get; } = text;
}
