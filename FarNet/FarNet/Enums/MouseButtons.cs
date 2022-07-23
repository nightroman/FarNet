
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Specifies constants that define which mouse button was pressed.
/// </summary>
[Flags]
public enum MouseButtons
{
	/// <summary>No mouse button was pressed.</summary>
	None = 0,

	/// <summary>The left mouse button was pressed.</summary>
	Left = 0x0001,

	/// <summary>The right mouse button was pressed.</summary>
	Right = 0x0002,

	/// <summary>The middle mouse button was pressed.</summary>
	Middle = 0x0004,

	///
	XButton1 = 0x0008,

	///
	XButton2 = 0x0010,

	///
	All = Left | Right | Middle | XButton1 | XButton2
}
