
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Arguments of <see cref="Panel.InvokingCommand"/>.
/// Set <see cref="PanelEventArgs.Ignore"/> = true to tell that command has been processed internally.
/// </summary>
/// <param name="command">See <see cref="Command"/></param>
public sealed class CommandLineEventArgs(string command) : PanelEventArgs
{
	/// <summary>
	/// Gets the command to be processed.
	/// </summary>
	public string Command { get; } = command;
}
