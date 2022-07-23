
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Update file from text arguments.
/// </summary>
public class SetTextEventArgs : ExplorerFileEventArgs
{
	/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
	/// <param name="file">See <see cref="ExplorerFileEventArgs.File"/></param>
	/// <param name="text">See <see cref="Text"/></param>
	public SetTextEventArgs(ExplorerModes mode, FarFile file, string text) : base(mode, file)
	{
		Text = text;
	}

	/// <summary>
	/// Gets the text to be imported.
	/// </summary>
	public string Text { get; }
}
