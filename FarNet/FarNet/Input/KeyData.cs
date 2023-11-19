
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Minimal key data.
/// </summary>
public class KeyData : KeyBase
{
	static readonly KeyData _Empty = new(0);

	/// <param name="virtualKeyCode">See <see cref="VirtualKeyCode"/></param>
	public KeyData(int virtualKeyCode)
	{
		VirtualKeyCode = virtualKeyCode;
	}

	/// <param name="virtualKeyCode">See <see cref="VirtualKeyCode"/></param>
	/// <param name="controlKeyState">See <see cref="KeyBase.ControlKeyState"/></param>
	public KeyData(int virtualKeyCode, ControlKeyStates controlKeyState)
		: base(controlKeyState)
	{
		VirtualKeyCode = virtualKeyCode;
	}

	/// <summary>
	/// Gets the empty key instance.
	/// </summary>
	public static KeyData Empty => _Empty;

	/// <summary>
	/// Gets the <see cref="KeyCode"/> code.
	/// </summary>
	public int VirtualKeyCode { get; }

	/// <summary>
	/// Tests a key code with no Ctrl, Alt, or Shift.
	/// </summary>
	/// <param name="virtualKeyCode">The key code to test.</param>
	public bool Is(int virtualKeyCode)
	{
		return VirtualKeyCode == virtualKeyCode && Is();
	}

	/// <summary>
	/// Tests a key code with Alt.
	/// </summary>
	/// <param name="virtualKeyCode">The key code to test.</param>
	public bool IsAlt(int virtualKeyCode)
	{
		return VirtualKeyCode == virtualKeyCode && IsAlt();
	}

	/// <summary>
	/// Tests a key code with Ctrl.
	/// </summary>
	/// <param name="virtualKeyCode">The key code to test.</param>
	public bool IsCtrl(int virtualKeyCode)
	{
		return VirtualKeyCode == virtualKeyCode && IsCtrl();
	}

	/// <summary>
	/// Tests a key code with Shift.
	/// </summary>
	/// <param name="virtualKeyCode">The key code to test.</param>
	public bool IsShift(int virtualKeyCode)
	{
		return VirtualKeyCode == virtualKeyCode && IsShift();
	}

	/// <summary>
	/// Tests a key with AltShift.
	/// </summary>
	/// <param name="virtualKeyCode">The key code to test.</param>
	public bool IsAltShift(int virtualKeyCode)
	{
		return VirtualKeyCode == virtualKeyCode && IsAltShift();
	}

	/// <summary>
	/// Tests a key with CtrlAlt.
	/// </summary>
	/// <param name="virtualKeyCode">The key code to test.</param>
	public bool IsCtrlAlt(int virtualKeyCode)
	{
		return VirtualKeyCode == virtualKeyCode && IsCtrlAlt();
	}

	/// <summary>
	/// Tests a key with CtrlShift.
	/// </summary>
	/// <param name="virtualKeyCode">The key code to test.</param>
	public bool IsCtrlShift(int virtualKeyCode)
	{
		return VirtualKeyCode == virtualKeyCode && IsCtrlShift();
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj)
	{
		return obj is KeyData that && VirtualKeyCode == that.VirtualKeyCode && ControlKeyState == that.ControlKeyState;
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		uint num = ((uint)ControlKeyState) << 0x10 | (uint)VirtualKeyCode;
		return num.GetHashCode();
	}

	/// <summary>
	/// Returns the string "(ControlKeyState)VirtualKeyCode".
	/// </summary>
	public override string ToString()
	{
		return "(" + ControlKeyState + ")" + VirtualKeyCode;
	}
}
