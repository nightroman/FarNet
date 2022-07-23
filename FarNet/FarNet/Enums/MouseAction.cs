
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Mouse action.
/// </summary>
public enum MouseAction
{
	/// <summary>
	/// Regular click.
	/// </summary>
	Click = 0,

	/// <summary>
	/// A change in mouse position occurred.
	/// </summary>
	Moved = 0x0001,

	/// <summary>
	/// The second click (button press) of a double-click occurred.
	/// The first click is returned as a regular button-press event.
	/// </summary>
	DoubleClick = 0x0002,

	/// <summary>
	/// The mouse wheel was rolled.
	/// </summary>
	Wheeled = 0x0004,

	/// Masks all flags.
	All = Moved | DoubleClick | Wheeled
}
