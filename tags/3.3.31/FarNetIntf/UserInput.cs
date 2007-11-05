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
		bool _keyDown;
		char _character;
		ControlKeyStates _controlKeyState;
		int _virtualKeyCode;

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
		///
		public override string ToString()
		{
			return "Down = " + _keyDown + "; Code = " + _virtualKeyCode + "; Char = " + _character + " (" + _controlKeyState + ")";
		}
		///
		public static bool operator ==(KeyInfo left, KeyInfo right)
		{
			return
				left._character == right._character &&
				left._controlKeyState == right._controlKeyState &&
				left._keyDown == right._keyDown &&
				left._virtualKeyCode == right._virtualKeyCode;
		}
		///
		public static bool operator !=(KeyInfo left, KeyInfo right)
		{
			return !(left == right);
		}
		///
		public override bool Equals(Object obj)
		{
			return obj is KeyInfo && this == (KeyInfo)obj;
		}
		///
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
		///
		public override string ToString()
		{
			return _where.ToString() + " " + _action + " (" + _buttons + ") (" + _controlKeyState + ")";
		}
		///
		public static bool operator ==(MouseInfo left, MouseInfo right)
		{
			return
				left._action == right._action &&
				left._buttons == right._buttons &&
				left._controlKeyState == right._controlKeyState &&
				left._where == right._where;
		}
		///
		public static bool operator !=(MouseInfo left, MouseInfo right)
		{
			return !(left == right);
		}
		///
		public override bool Equals(Object obj)
		{
			return obj is MouseInfo && this == (MouseInfo)obj;
		}
		///
		public override int GetHashCode()
		{
			uint num = (uint)_action + ((uint)_buttons << 8) + ((uint)_controlKeyState << 16);
			return num.GetHashCode() ^ _where.GetHashCode();
		}
	}

	/// <summary>
	/// Internal key codes. [farkeys.hpp]
	/// </summary>
	public static class KeyCode
	{
		/// <summary></summary>
		public const long CtrlMask = 0xFF000000;

		/// <summary></summary>
		public const int

		Ctrl = 0x01000000,
		Alt = 0x02000000,
		Shift = 0x04000000,
		RCtrl = 0x10000000,
		RAlt = 0x20000000,

		LBracket = '[',
		RBracket = ']',
		Comma = ',',
		Quote = '"',
		Dot = '.',
		Slash = '/',
		Colon = ':',
		Semicolon = ';',
		Backslash = '\\',

		Backspace = 0x00000008,
		Tab = 0x00000009,
		Enter = 0x0000000D,
		Escape = 0x0000001B,
		Space = 0x00000020,

		FKeyMask = 0x00000FFF,
		FKeyBegin = 0x00000100,

		Break = 0x00000103,

		PageUp = 0x00000121,
		PageDown = 0x00000122,
		End = 0x00000123,
		Home = 0x00000124,
		Left = 0x00000125,
		Up = 0x00000126,
		Right = 0x00000127,
		Down = 0x00000128,
		Insert = 0x0000012D,
		Delete = 0x0000012E,

		LWin = 0x0000015B,
		RWin = 0x0000015C,
		Apps = 0x0000015D,

		Numpad0 = 0x00000160,
		Numpad1 = 0x00000161,
		Numpad2 = 0x00000162,
		Numpad3 = 0x00000163,
		Numpad4 = 0x00000164,
		Numpad5 = 0x00000165,
		Clear = Numpad5,
		Numpad6 = 0x00000166,
		Numpad7 = 0x00000167,
		Numpad8 = 0x00000168,
		Numpad9 = 0x00000169,

		Multiply = 0x0000016A,
		Add = 0x0000016B,
		Subtract = 0x0000016D,
		Divide = 0x0000016F,

		F1 = 0x00000170,
		F2 = 0x00000171,
		F3 = 0x00000172,
		F4 = 0x00000173,
		F5 = 0x00000174,
		F6 = 0x00000175,
		F7 = 0x00000176,
		F8 = 0x00000177,
		F9 = 0x00000178,
		F10 = 0x00000179,
		F11 = 0x0000017A,
		F12 = 0x0000017B,

		F13 = 0x0000017C,
		F14 = 0x0000017D,
		F15 = 0x0000017E,
		F16 = 0x0000017F,
		F17 = 0x00000180,
		F18 = 0x00000181,
		F19 = 0x00000182,
		F20 = 0x00000183,
		F21 = 0x00000184,
		F22 = 0x00000185,
		F23 = 0x00000186,
		F24 = 0x00000187,

		BrowserBack = 0x000001A6,
		BrowserForward = 0x000001A7,
		BrowserRefresh = 0x000001A8,
		BrowserStop = 0x000001A9,
		BrowserSearch = 0x000001AA,
		BrowserFavorites = 0x000001AB,
		BrowserHome = 0x000001AC,
		VolumeMute = 0x000001AD,
		VolumeDown = 0x000001AE,
		VolumeUp = 0x000001AF,
		MediaNextTrack = 0x000001B0,
		MediaPrevTrack = 0x000001B1,
		MediaStop = 0x000001B2,
		MediaPlayPause = 0x000001B3,
		LaunchMail = 0x000001B4,
		LaunchMediaSelect = 0x000001B5,
		LaunchApp1 = 0x000001B6,
		LaunchApp2 = 0x000001B7,

		CtrlAltShiftPress = 0x00000201,
		CtrlAltShiftRelease = 0x00000202,

		MouseWheelUp = 0x00000203,
		MouseWheelDown = 0x00000204,
		NumpadDelete = 0x00000209,
		Decimal = 0x0000020A,
		NumpadEnter = 0x0000020B,

		FKeyEnd = 0x00000FFF,

		None = 0x00001001,
		Idle = 0x00001002,

		SKeyEnd = 0x0000FFFF,
		LastBase = SKeyEnd;
	}
}
