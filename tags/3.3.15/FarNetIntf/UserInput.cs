/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

using System;

namespace FarManager
{
	/// <summary>
	/// Represents Control key states.
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
		/// <summary>Alt, Ctrl and Shift states.</summary>
		AltCtrlShift = RightAltPressed | LeftAltPressed | RightCtrlPressed | LeftCtrlPressed | ShiftPressed,
		/// <summary>All states.</summary>
		All = RightAltPressed | LeftAltPressed | RightCtrlPressed | LeftCtrlPressed | ShiftPressed | NumLockOn | ScrollLockOn | CapsLockOn | EnhancedKey
	}

	/// <summary>
	/// Represents Control, Alt and Shift states.
	/// </summary>
	[Flags]
	public enum KeyStates
	{
		/// <summary>None.</summary>
		None = 0,
		/// <summary>Control pressed.</summary>
		Control = 0x1,
		/// <summary>Alt pressed.</summary>
		Alt = 0x2,
		/// <summary>Shift pressed.</summary>
		Shift = 0x4,
	}

	/// <summary>
	/// Keyboard event information.
	/// </summary>
	public struct KeyInfo
	{
		private bool _keyDown;
		private char _character;
		private ControlKeyStates _controlKeyState;
		private int _virtualKeyCode;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="virtualKeyCode"></param>
		/// <param name="character"></param>
		/// <param name="controlKeyState"></param>
		/// <param name="keyDown"></param>
		public KeyInfo(int virtualKeyCode, char character, ControlKeyStates controlKeyState, bool keyDown)
		{
			_virtualKeyCode = virtualKeyCode;
			_character = character;
			_controlKeyState = controlKeyState;
			_keyDown = keyDown;
		}
		/// <summary>
		/// Virtual key code.
		/// </summary>
		public int VirtualKeyCode { get { return _virtualKeyCode; } set { _virtualKeyCode = value; } }
		/// <summary>
		/// Character.
		/// </summary>
		public char Character { get { return _character; } set { _character = value; } }
		/// <summary>
		/// Control key states.
		/// </summary>
		public ControlKeyStates ControlKeyState { get { return _controlKeyState; } set { _controlKeyState = value; } }
		/// <summary>
		/// Key down event.
		/// </summary>
		public bool KeyDown { get { return _keyDown; } set { _keyDown = value; } }
		/// <summary>
		/// Gets only Alt, Ctrl and Shift states.
		/// </summary>
		public ControlKeyStates AltCtrlShift { get { return _controlKeyState & ControlKeyStates.AltCtrlShift; } }
		/// <summary>
		/// ToString()
		/// </summary>
		public override string ToString()
		{
			return "Down = " + _keyDown + "; Code = " + _virtualKeyCode + "; Char = " + _character + " (" + _controlKeyState + ")";
		}
		/// <summary>
		/// Compares two objects.
		/// </summary>
		public static bool operator ==(KeyInfo left, KeyInfo right)
		{
			return
				left._character == right._character &&
				left._controlKeyState == right._controlKeyState &&
				left._keyDown == right._keyDown &&
				left._virtualKeyCode == right._virtualKeyCode;
		}
		/// <summary>
		/// Compares two objects.
		/// </summary>
		public static bool operator !=(KeyInfo left, KeyInfo right)
		{
			return !(left == right);
		}
		/// <summary>
		/// Equals()
		/// </summary>
		public override bool Equals(Object obj)
		{
			return obj is KeyInfo && this == (KeyInfo)obj;
		}
		/// <summary>
		/// GetHashCode()
		/// </summary>
		public override int GetHashCode()
		{
			uint num = _keyDown ? 0x10000000u : 0;
			num |= ((uint)_controlKeyState) << 0x10;
			num |= (uint)_virtualKeyCode;
			return num.GetHashCode();
		}
	}

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
		/// <summary></summary>
		XButton1 = 0x0008,
		/// <summary></summary>
		XButton2 = 0x0010,
		/// <summary></summary>
		All = Left | Right | Middle | XButton1 | XButton2
	}

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
		/// <summary>
		/// All.
		/// </summary>
		All = Moved | DoubleClick | Wheeled
	}

	/// <summary>
	/// Mouse event information.
	/// </summary>
	public struct MouseInfo
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="where">Position.</param>
		/// <param name="action">Mouse action.</param>
		/// <param name="buttons">Mouse buttons.</param>
		/// <param name="controlKeyState">Control keys.</param>
		public MouseInfo(Point where, MouseAction action, MouseButtons buttons, ControlKeyStates controlKeyState)
		{
			_where = where;
			_buttons = buttons;
			_action = action;
			_controlKeyState = controlKeyState;
		}
		/// <summary>
		/// Mouse location.
		/// </summary>
		public Point Where { get { return _where; } set { _where = value; } }
		Point _where;
		/// <summary>
		/// Buttons.
		/// </summary>
		public MouseButtons Buttons { get { return _buttons; } set { _buttons = value; } }
		MouseButtons _buttons;
		/// <summary>
		/// Action.
		/// </summary>
		public MouseAction Action { get { return _action; } set { _action = value; } }
		MouseAction _action;
		/// <summary>
		/// Control key states.
		/// </summary>
		public ControlKeyStates ControlKeyState { get { return _controlKeyState; } set { _controlKeyState = value; } }
		ControlKeyStates _controlKeyState;
		/// <summary>
		/// Gets only Alt, Ctrl and Shift states.
		/// </summary>
		public ControlKeyStates AltCtrlShift { get { return _controlKeyState & ControlKeyStates.AltCtrlShift; } }
		/// <summary>
		/// ToString()
		/// </summary>
		public override string ToString()
		{
			return _where.ToString() + " " + _action + " (" + _buttons + ") (" + _controlKeyState + ")";
		}
		/// <summary>
		/// Compares two objects.
		/// </summary>
		public static bool operator ==(MouseInfo left, MouseInfo right)
		{
			return
				left._action == right._action &&
				left._buttons == right._buttons &&
				left._controlKeyState == right._controlKeyState &&
				left._where == right._where;
		}
		/// <summary>
		/// Compares two objects.
		/// </summary>
		public static bool operator !=(MouseInfo left, MouseInfo right)
		{
			return !(left == right);
		}
		/// <summary>
		/// Equals()
		/// </summary>
		public override bool Equals(Object obj)
		{
			return obj is MouseInfo && this == (MouseInfo)obj;
		}
		/// <summary>
		/// GetHashCode()
		/// </summary>
		public override int GetHashCode()
		{
			uint num = (uint)_action + ((uint)_buttons << 8) + ((uint)_controlKeyState << 16);
			return num.GetHashCode() ^ _where.GetHashCode();
		}
	}
}
