
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Full key information.
/// </summary>
public sealed class KeyInfo : KeyData
{
	/// <param name="virtualKeyCode">See <see cref="KeyData.VirtualKeyCode"/></param>
	/// <param name="character">See <see cref="Character"/></param>
	/// <param name="controlKeyState">See <see cref="KeyBase.ControlKeyState"/></param>
	/// <param name="keyDown">See <see cref="KeyDown"/></param>
	public KeyInfo(int virtualKeyCode, char character, ControlKeyStates controlKeyState, bool keyDown)
		: base(virtualKeyCode, controlKeyState)
	{
		Character = character;
		KeyDown = keyDown;
	}

	/// <summary>
	/// Gets the character of the key.
	/// </summary>
	public char Character { get; }

	/// <summary>
	/// Gets true for the key down event.
	/// </summary>
	public bool KeyDown { get; }

	/// <inheritdoc/>
	public override bool Equals(object? obj)
	{
		return obj is KeyInfo that &&
			VirtualKeyCode == that.VirtualKeyCode &&
			ControlKeyState == that.ControlKeyState &&
			Character == that.Character &&
			KeyDown == that.KeyDown;
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		uint num = KeyDown ? 0x10000000u : 0;
		num |= ((uint)ControlKeyState) << 0x10;
		num |= (uint)VirtualKeyCode;
		return num.GetHashCode();
	}

	/// <summary>
	/// Returns the string "Down = {0}; Code = {1}; Char = {2} ({3})", KeyDown, VirtualKeyCode, Character, ControlKeyState.
	/// </summary>
	public override string ToString()
	{
		return string.Format(null, "Down = {0}; Code = {1}; Char = {2} ({3})", KeyDown, VirtualKeyCode, Character, ControlKeyState);
	}
}
