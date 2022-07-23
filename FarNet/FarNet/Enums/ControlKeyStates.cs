
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Represents control key states.
/// </summary>
[Flags]
public enum ControlKeyStates
{
	/// <summary>None.</summary>
	None = 0,

	/// <summary>Right Alt.</summary>
	RightAltPressed = 0x0001,

	/// <summary>Left Alt.</summary>
	LeftAltPressed = 0x0002,

	/// <summary>Right control.</summary>
	RightCtrlPressed = 0x0004,

	/// <summary>Left Cotrol.</summary>
	LeftCtrlPressed = 0x0008,

	/// <summary>Shift.</summary>
	ShiftPressed = 0x0010,

	/// <summary>NumLock.</summary>
	NumLockOn = 0x0020,

	/// <summary>ScrollLock.</summary>
	ScrollLockOn = 0x0040,

	/// <summary>CapsLock.</summary>
	CapsLockOn = 0x0080,

	/// <summary>Enhanced key.</summary>
	EnhancedKey = 0x0100,

	/// <summary>Ctrl, Alt and Shift states.</summary>
	CtrlAltShift = RightAltPressed | LeftAltPressed | RightCtrlPressed | LeftCtrlPressed | ShiftPressed,

	/// <summary>All states.</summary>
	All = RightAltPressed | LeftAltPressed | RightCtrlPressed | LeftCtrlPressed | ShiftPressed | NumLockOn | ScrollLockOn | CapsLockOn | EnhancedKey
}
