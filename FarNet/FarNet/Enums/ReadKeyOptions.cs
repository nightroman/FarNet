
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Read key options.
/// </summary>
[Flags]
public enum ReadKeyOptions
{
	/// <summary>
	/// Allow the CTRL+C key to be processed as a keystroke, as opposed to causing a break event.
	/// </summary>
	AllowCtrlC = 1,

	/// <summary>
	/// Do not display the character in the window when the key is pressed.
	/// </summary>
	NoEcho = 2,

	/// <summary>
	/// Allow key-down events.
	/// </summary>
	IncludeKeyDown = 4,

	/// <summary>
	/// Allow key-up events.
	/// </summary>
	IncludeKeyUp = 8,
}
