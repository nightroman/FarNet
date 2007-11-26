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
		public KeyInfo(int virtualKeyCode, char character, ControlKeyStates controlKeyState, bool keyDown)
		{
			_virtualKeyCode = virtualKeyCode;
			_character = character;
			_controlKeyState = controlKeyState;
			_keyDown = keyDown;
		}
		/// <summary>
		/// <see cref="VKeyCode"/> code.
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

	/// <summary>
	/// Virtual key codes.
	/// <c>Add{Control|Alt|Shift}</c> are not virtual key codes but helpers, e.g. to join menu break codes.
	/// </summary>
	public static class VKeyCode
	{
		/// <summary></summary>
		public const int
		AddControl = 1 << 16,
		AddAlt = 2 << 16,
		AddShift = 4 << 16,

		LButton = 0x01,
		RButton = 0x02,
		Cancel = 0x03,
		MButton = 0x04,
		XButton1 = 0x05,
		XButton2 = 0x06,
			// 07 Undefined  
		Backspace = 0x08,
		Tab = 0x09,
			// 0A-0B Reserved 
		Clear = 0x0C,
		Return = 0x0D,
			// 0E-0F Undefined  
		Shift = 0x10,
		Control = 0x11,
		Menu = 0x12,
		Pause = 0x13,
		Capital = 0x14,
		Kana = 0x15,
		Hanguel = 0x15,
		Hangul = 0x15,
			// 16 Undefined 
		Junja = 0x17,
		Final = 0x18,
		Hanja = 0x19,
		Kanji = 0x19,
			// 1A Undefined  
		Escape = 0x1B,
		Convert = 0x1C,
		NonConvert = 0x1D,
		Accept = 0x1E,
		ModeChange = 0x1F,
		Space = 0x20,
		Prior = 0x21,
		Next = 0x22,
		End = 0x23,
		Home = 0x24,
		Left = 0x25,
		Up = 0x26,
		Right = 0x27,
		Down = 0x28,
		Select = 0x29,
		Print = 0x2A,
		Execute = 0x2B,
		Snapshot = 0x2C,
		Insert = 0x2D,
		Delete = 0x2E,
		Help = 0x2F,
		K0 = 0x30,
		K1 = 0x31,
		K2 = 0x32,
		K3 = 0x33,
		K4 = 0x34,
		K5 = 0x35,
		K6 = 0x36,
		K7 = 0x37,
		K8 = 0x38,
		K9 = 0x39,
			// 3A-40 Undefined  
		KA = 0x41,
		KB = 0x42,
		KC = 0x43,
		KD = 0x44,
		KE = 0x45,
		KF = 0x46,
		KG = 0x47,
		KH = 0x48,
		KI = 0x49,
		KJ = 0x4A,
		KK = 0x4B,
		KL = 0x4C,
		KM = 0x4D,
		KN = 0x4E,
		KO = 0x4F,
		KP = 0x50,
		KQ = 0x51,
		KR = 0x52,
		KS = 0x53,
		KT = 0x54,
		KU = 0x55,
		KV = 0x56,
		KW = 0x57,
		KX = 0x58,
		KY = 0x59,
		KZ = 0x5A,
		LWin = 0x5B,
		RWin = 0x5C,
		Apps = 0x5D,
			// 5E Reserved 
		Sleep = 0x5F,
		Numpad0 = 0x60,
		Numpad1 = 0x61,
		Numpad2 = 0x62,
		Numpad3 = 0x63,
		Numpad4 = 0x64,
		Numpad5 = 0x65,
		Numpad6 = 0x66,
		Numpad7 = 0x67,
		Numpad8 = 0x68,
		Numpad9 = 0x69,
		Multiply = 0x6A,
		Add = 0x6B,
		Separator = 0x6C,
		Subtract = 0x6D,
		Decimal = 0x6E,
		Divide = 0x6F,
		F1 = 0x70,
		F2 = 0x71,
		F3 = 0x72,
		F4 = 0x73,
		F5 = 0x74,
		F6 = 0x75,
		F7 = 0x76,
		F8 = 0x77,
		F9 = 0x78,
		F10 = 0x79,
		F11 = 0x7A,
		F12 = 0x7B,
		F13 = 0x7C,
		F14 = 0x7D,
		F15 = 0x7E,
		F16 = 0x7F,
		F17 = 0x80,
		F18 = 0x81,
		F19 = 0x82,
		F20 = 0x83,
		F21 = 0x84,
		F22 = 0x85,
		F23 = 0x86,
		F24 = 0x87,
			// 88-8F Unassigned  
		Numlock = 0x90,
		Scroll = 0x91,
			// 97-9F Unassigned 
		LShift = 0xA0,
		RShift = 0xA1,
		LControl = 0xA2,
		RControl = 0xA3,
		LMenu = 0xA4,
		RMenu = 0xA5,
		BrowserBack = 0xA6,
		BrowserForward = 0xA7,
		BrowserRefresh = 0xA8,
		BrowserStop = 0xA9,
		BrowserSearch = 0xAA,
		BrowserFavorites = 0xAB,
		BrowserHome = 0xAC,
		VolumeMute = 0xAD,
		VolumeDown = 0xAE,
		VolumeUp = 0xAF,
		MediaNextTrack = 0xB0,
		MediaPrevTrack = 0xB1,
		MediaStop = 0xB2,
		MediaPlayPause = 0xB3,
		LaunchMail = 0xB4,
		LaunchMediaSelect = 0xB5,
		LaunchApp1 = 0xB6,
		LaunchApp2 = 0xB7,
			// B8-B9 Reserved 
		Oem1 = 0xBA,
		OemPlus = 0xBB,
		OemComma = 0xBC,
		OemMinus = 0xBD,
		OemPeriod = 0xBE,
		Oem2 = 0xBF,
		Oem3 = 0xC0,
			// C1-D7 Reserved  
			// D8-DA Unassigned 
		Oem4 = 0xDB,
		Oem5 = 0xDC,
		Oem6 = 0xDD,
		Oem7 = 0xDE,
		Oem8 = 0xDF,
			// E0 Reserved 
			// E1 OEM specific 
		Oem102 = 0xE2,
			// E3-E4 OEM specific  
		ProcessKey = 0xE5,
			// E6 OEM specific  
		Packet = 0xE7,
			// E8 Unassigned  
		Attn = 0xF6,
		CRSel = 0xF7,
		Exsel = 0xF8,
		EraseEof = 0xF9,
		Play = 0xFA,
		Zoom = 0xFB,
		NoName = 0xFC,
		PA1 = 0xFD,
		OemClear = 0xFE;
	}
}
