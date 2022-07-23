
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Base class for keyboard related classes.
/// </summary>
public abstract class KeyBase
{
	///
	protected KeyBase()
	{ }

	/// <param name="controlKeyState">See <see cref="ControlKeyState"/></param>
	protected KeyBase(ControlKeyStates controlKeyState)
	{
		ControlKeyState = controlKeyState;
	}

	/// <summary>
	/// Gets all control key states including special flags.
	/// </summary>
	public ControlKeyStates ControlKeyState { get; }

	/// <summary>
	/// Tests no Ctrl, Alt, or Shift.
	/// </summary>
	public bool Is()
	{
		return 0 == (ControlKeyState & ControlKeyStates.CtrlAltShift);
	}

	/// <summary>
	/// Tests Alt state.
	/// </summary>
	public bool IsAlt()
	{
		var value = ControlKeyState & ControlKeyStates.CtrlAltShift;
		return value == ControlKeyStates.LeftAltPressed || value == ControlKeyStates.RightAltPressed;
	}

	/// <summary>
	/// Tests Ctrl state.
	/// </summary>
	public bool IsCtrl()
	{
		var value = ControlKeyState & ControlKeyStates.CtrlAltShift;
		return value == ControlKeyStates.LeftCtrlPressed || value == ControlKeyStates.RightCtrlPressed;
	}

	/// <summary>
	/// Tests Shift state.
	/// </summary>
	public bool IsShift()
	{
		return (ControlKeyState & ControlKeyStates.CtrlAltShift) == ControlKeyStates.ShiftPressed;
	}

	/// <summary>
	/// Tests AltShift state.
	/// </summary>
	public bool IsAltShift()
	{
		var value = ControlKeyState & ControlKeyStates.CtrlAltShift;
		return
			value == (ControlKeyStates.ShiftPressed | ControlKeyStates.LeftAltPressed) ||
			value == (ControlKeyStates.ShiftPressed | ControlKeyStates.RightAltPressed);
	}

	/// <summary>
	/// Tests CtrlAlt state.
	/// </summary>
	public bool IsCtrlAlt()
	{
		var value = ControlKeyState & ControlKeyStates.CtrlAltShift;
		return
			value == (ControlKeyStates.LeftCtrlPressed | ControlKeyStates.LeftAltPressed) ||
			value == (ControlKeyStates.RightCtrlPressed | ControlKeyStates.RightAltPressed);
	}

	/// <summary>
	/// Tests CtrlShift state.
	/// </summary>
	public bool IsCtrlShift()
	{
		var value = ControlKeyState & ControlKeyStates.CtrlAltShift;
		return
			value == (ControlKeyStates.ShiftPressed | ControlKeyStates.LeftCtrlPressed) ||
			value == (ControlKeyStates.ShiftPressed | ControlKeyStates.RightCtrlPressed);
	}

	/// <summary>
	/// Gets only Ctrl, Alt, and Shift states excluding special flags.
	/// </summary>
	public ControlKeyStates CtrlAltShift()
	{
		return ControlKeyState & ControlKeyStates.CtrlAltShift;
	}

	/// <inheritdoc/>
	public override bool Equals(object obj)
	{
		return obj is KeyBase that && ControlKeyState == that.ControlKeyState;
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		return (int)ControlKeyState;
	}

	/// <summary>
	/// Returns the string "ControlKeyState".
	/// </summary>
	public override string ToString()
	{
		return ControlKeyState.ToString();
	}
}
