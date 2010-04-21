/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Diagnostics.CodeAnalysis;

namespace FarNet
{
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

	/// <summary>
	/// Mouse action.
	/// </summary>
	[SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
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

	/// <summary>
	/// Key modes and helper masks used with <see cref="KeyCode"/>. [farkeys.hpp]
	/// </summary>
	/// <remarks>
	/// Use modes to make a key combination with [Ctrl], [Alt], [Shift] and etc.
	/// </remarks>
	/// <example>
	/// Use (KeyMode.Ctrl | 'R') for [CtrlR], (KeyMode.Ctrl | KeyCode.Enter) for [CtrlEnter] and etc.
	/// </example>
	public static class KeyMode
	{
		/// <summary>
		/// Code mask, i.e. (Key &amp; CodeMask) gets key code without modes.
		/// </summary>
		public const long CodeMask = 0x00FFFFFF;

		/// <summary>
		/// Mode mask, i.e. (Key &amp; ModeMask) gets key modes without code.
		/// </summary>
		public const long ModeMask = 0xFF000000;

		///
		public const int

		Ctrl = 0x01000000,
		Alt = 0x02000000,
		Shift = 0x04000000,
		RCtrl = 0x10000000,
		RAlt = 0x20000000,

		AltShift = Alt | Shift,
		CtrlAlt = Ctrl | Alt,
		CtrlShift = Ctrl | Shift,
		CtrlAltShift = Ctrl | Alt | Shift;
	}

	/// <summary>
	/// Internal key codes. Use <see cref="KeyMode"/> to make key combinations. [farkeys.hpp]
	/// </summary>
	/// <remarks>
	/// Key names and codes are Far key macro names and codes.
	/// <para>
	/// This PowerShellFar code gets names and codes from BS to 131086 (Oem* and Spec* are excluded):
	/// </para>
	/// <para>
	/// .{for($r = 0; $r -le 131086; ++$r) {$r}} | %{$e = $Far.KeyToName($_); if ($e.Length -ge 2 -and $e -notmatch '^Oem|^Spec' ) {"$e = $r,"}}
	/// </para>
	/// </remarks>
	/// <example>
	/// Use KeyMode.Ctrl | 'R' for CtrlR, KeyMode.Ctrl | KeyCode.Enter for CtrlEnter and etc.
	/// </example>
	public static class KeyCode
	{
		///
		public const int
BS = 8,
Tab = 9,
Enter = 13,
Esc = 27,
Space = 32,
BackSlash = 92,
Break = 65539,
Pause = 65555,
CapsLock = 65556,
PgUp = 65569,
PgDn = 65570,
End = 65571,
Home = 65572,
Left = 65573,
Up = 65574,
Right = 65575,
Down = 65576,
PrntScrn = 65580,
Ins = 65581,
Del = 65582,
LWin = 65627,
RWin = 65628,
Apps = 65629,
Standby = 65631,
Num0 = 65632,
Num1 = 65633,
Num2 = 65634,
Num3 = 65635,
Num4 = 65636,
Clear = 65637,
Num6 = 65638,
Num7 = 65639,
Num8 = 65640,
Num9 = 65641,
Multiply = 65642,
Add = 65643,
Subtract = 65645,
Decimal = 65646,
Divide = 65647,
F1 = 65648,
F2 = 65649,
F3 = 65650,
F4 = 65651,
F5 = 65652,
F6 = 65653,
F7 = 65654,
F8 = 65655,
F9 = 65656,
F10 = 65657,
F11 = 65658,
F12 = 65659,
F13 = 65660,
F14 = 65661,
F15 = 65662,
F16 = 65663,
F17 = 65664,
F18 = 65665,
F19 = 65666,
F20 = 65667,
F21 = 65668,
F22 = 65669,
F23 = 65670,
F24 = 65671,
NumLock = 65680,
ScrollLock = 65681,
BrowserBack = 65702,
BrowserForward = 65703,
BrowserRefresh = 65704,
BrowserStop = 65705,
BrowserSearch = 65706,
BrowserFavorites = 65707,
BrowserHome = 65708,
VolumeMute = 65709,
VolumeDown = 65710,
VolumeUp = 65711,
MediaNextTrack = 65712,
MediaPrevTrack = 65713,
MediaStop = 65714,
MediaPlayPause = 65715,
LaunchMail = 65716,
LaunchMediaSelect = 65717,
LaunchApp1 = 65718,
LaunchApp2 = 65719,
CtrlAltShiftPress = 131073,
CtrlAltShiftRelease = 131074,
MsWheelUp = 131075,
MsWheelDown = 131076,
RightCtrlAltShiftPress = 131079,
RightCtrlAltShiftRelease = 131080,
NumDel = 131081,
NumEnter = 131083,
MsWheelLeft = 131084,
MsWheelRight = 131085;
	}

	/// <summary>
	/// Virtual key modes for key combinations.
	/// </summary>
	public static class VKeyMode
	{
		///
		public const int
Ctrl = 1 << 16,
Alt = 2 << 16,
Shift = 4 << 16;
	}

	/// <summary>
	/// Virtual key codes.
	/// Use <see cref="VKeyMode"/> for combinations.
	/// </summary>
	/// <remarks>
	/// They are similar to <c>System.ConsoleKey</c>, <c>System.Windows.Forms.Keys</c>.
	/// <para>
	/// [enum]::GetNames([consolekey]) | %{ '{0} = {1},' -f $_, [int][consolekey]$_ }
	/// </para>
	/// </remarks>
	public static class VKeyCode
	{
		///
		public const int
Backspace = 8,
Tab = 9,
Clear = 12,
Enter = 13,
Pause = 19,
Escape = 27,
Spacebar = 32,
PageUp = 33,
PageDown = 34,
End = 35,
Home = 36,
LeftArrow = 37,
UpArrow = 38,
RightArrow = 39,
DownArrow = 40,
Select = 41,
Print = 42,
Execute = 43,
PrintScreen = 44,
Insert = 45,
Delete = 46,
Help = 47,
D0 = 48,
D1 = 49,
D2 = 50,
D3 = 51,
D4 = 52,
D5 = 53,
D6 = 54,
D7 = 55,
D8 = 56,
D9 = 57,
A = 65,
B = 66,
C = 67,
D = 68,
E = 69,
F = 70,
G = 71,
H = 72,
I = 73,
J = 74,
K = 75,
L = 76,
M = 77,
N = 78,
O = 79,
P = 80,
Q = 81,
R = 82,
S = 83,
T = 84,
U = 85,
V = 86,
W = 87,
X = 88,
Y = 89,
Z = 90,
LeftWindows = 91,
RightWindows = 92,
Applications = 93,
Sleep = 95,
NumPad0 = 96,
NumPad1 = 97,
NumPad2 = 98,
NumPad3 = 99,
NumPad4 = 100,
NumPad5 = 101,
NumPad6 = 102,
NumPad7 = 103,
NumPad8 = 104,
NumPad9 = 105,
Multiply = 106,
Add = 107,
Separator = 108,
Subtract = 109,
Decimal = 110,
Divide = 111,
F1 = 112,
F2 = 113,
F3 = 114,
F4 = 115,
F5 = 116,
F6 = 117,
F7 = 118,
F8 = 119,
F9 = 120,
F10 = 121,
F11 = 122,
F12 = 123,
F13 = 124,
F14 = 125,
F15 = 126,
F16 = 127,
F17 = 128,
F18 = 129,
F19 = 130,
F20 = 131,
F21 = 132,
F22 = 133,
F23 = 134,
F24 = 135,
BrowserBack = 166,
BrowserForward = 167,
BrowserRefresh = 168,
BrowserStop = 169,
BrowserSearch = 170,
BrowserFavorites = 171,
BrowserHome = 172,
VolumeMute = 173,
VolumeDown = 174,
VolumeUp = 175,
MediaNext = 176,
MediaPrevious = 177,
MediaStop = 178,
MediaPlay = 179,
LaunchMail = 180,
LaunchMediaSelect = 181,
LaunchApp1 = 182,
LaunchApp2 = 183,
Oem1 = 186,
OemPlus = 187,
OemComma = 188,
OemMinus = 189,
OemPeriod = 190,
Oem2 = 191,
Oem3 = 192,
Oem4 = 219,
Oem5 = 220,
Oem6 = 221,
Oem7 = 222,
Oem8 = 223,
Oem102 = 226,
Process = 229,
Packet = 231,
Attention = 246,
CrSel = 247,
ExSel = 248,
EraseEndOfFile = 249,
Play = 250,
Zoom = 251,
NoName = 252,
Pa1 = 253,
OemClear = 254;
	}

	/// <summary>
	/// Palette colors used in UI.
	/// </summary>
	/// <seealso cref="IFar.WritePalette"/>
	/// <seealso cref="IFar.WriteText"/>
	/// <seealso cref="IFar.GetPaletteForeground"/>
	/// <seealso cref="IFar.GetPaletteBackground"/>
	public enum PaletteColor
	{
		///
		MenuText,
		///
		MenuSelectedText,
		///
		MenuHighlight,
		///
		MenuSelectedHighlight,
		///
		MenuBox,
		///
		MenuTitle,

		///
		HMenuText,
		///
		HMenuSelectedText,
		///
		HMenuHighlight,
		///
		HMenuSelectedHighlight,

		///
		PanelText,
		///
		PanelSelectedText,
		///
		PanelHighlightText,
		///
		PanelInfoText,
		///
		PanelCursor,
		///
		PanelSelectedCursor,
		///
		PanelTitle,
		///
		PanelSelectedTitle,
		///
		PanelColumnTitle,
		///
		PanelTotalInfo,
		///
		PanelSelectedInfo,

		///
		DialogText,
		///
		DialogHighlightText,
		///
		DialogBox,
		///
		DialogBoxTitle,
		///
		DialogHighlightBoxTitle,
		///
		DialogEdit,
		///
		DialogButton,
		///
		DialogSelectedButton,
		///
		DialogHighlightButton,
		///
		DialogSelectedHighlightButton,

		///
		DialogListText,
		///
		DialogListSelectedText,
		///
		DialogListHighlight,
		///
		DialogListSelectedHighlight,

		///
		WarnDialogText,
		///
		WarnDialogHighlightText,
		///
		WarnDialogBox,
		///
		WarnDialogBoxTitle,
		///
		WarnDialogHighlightBoxTitle,
		///
		WarnDialogEdit,
		///
		WarnDialogButton,
		///
		WarnDialogSelectedButton,
		///
		WarnDialogHighlightButton,
		///
		WarnDialogSelectedHighlightButton,

		///
		KeyBarNumber,
		///
		KeyBarText,
		///
		KeyBarBackground,

		///
		CommandLine,

		///
		Clock,

		///
		ViewerText,
		///
		ViewerSelectedText,
		///
		ViewerStatus,

		///
		EditorText,
		///
		EditorSelectedText,
		///
		EditorStatus,

		///
		HelpText,
		///
		HelpHighlightText,
		///
		HelpTopic,
		///
		HelpSelectedTopic,
		///
		HelpBox,
		///
		HelpBoxTitle,

		///
		PanelDragText,
		///
		DialogEditUnchanged,
		///
		PanelScrollBar,
		///
		HelpScrollBar,
		///
		PanelBox,
		///
		PanelScreensNumber,
		///
		DialogEditSelected,
		///
		CommandLineSelected,
		///
		ViewerArrows,

		///
		NotUsed0,

		///
		DialogListScrollBar,
		///
		MenuScrollBar,
		///
		ViewerScrollBar,
		///
		CommandLinePrefix,
		///
		DialogDisabled,
		///
		DialogEditDisabled,
		///
		DialogListDisabled,
		///
		WarnDialogDisabled,
		///
		WarnDialogEditDisabled,
		///
		WarnDialogListDisabled,

		///
		MenuDisabledText,

		///
		EditorClock,
		///
		ViewerClock,

		///
		DialogListTitle,
		///
		DialogListBox,

		///
		WarnDialogEditSelected,
		///
		WarnDialogEditUnchanged,

		///
		DialogComboText,
		///
		DialogComboSelectedText,
		///
		DialogComboHighlight,
		///
		DialogComboSelectedHighlight,
		///
		DialogComboBox,
		///
		DialogComboTitle,
		///
		DialogComboDisabled,
		///
		DialogComboScrollBar,

		///
		WarnDialogListText,
		///
		WarnDialogListSelectedText,
		///
		WarnDialogListHighlight,
		///
		WarnDialogListSelectedHighlight,
		///
		WarnDialogListBox,
		///
		WarnDialogListTitle,
		///
		WarnDialogListScrollBar,

		///
		WarnDialogComboText,
		///
		WarnDialogComboSelectedText,
		///
		WarnDialogComboHighlight,
		///
		WarnDialogComboSelectedHighlight,
		///
		WarnDialogComboBox,
		///
		WarnDialogComboTitle,
		///
		WarnDialogComboDisabled,
		///
		WarnDialogComboScrollBar,

		///
		DialogListArrows,
		///
		DialogListArrowsDisabled,
		///
		DialogListArrowsSelected,
		///
		DialogComboArrows,
		///
		DialogComboArrowsDisabled,
		///
		DialogComboArrowsSelected,
		///
		WarnDialogListArrows,
		///
		WarnDialogListArrowsDisabled,
		///
		WarnDialogListArrowsSelected,
		///
		WarnDialogComboArrows,
		///
		WarnDialogComboArrowsDisabled,
		///
		WarnDialogComboArrowsSelected,
		///
		MenuArrows,
		///
		MenuArrowsDisabled,
		///
		MenuArrowsSelected,

		///
		MenuGrayText,
		///
		MenuSelectedGrayText,
		///
		DialogComboGray,
		///
		DialogComboSelectedGrayText,
		///
		DialogListGray,
		///
		DialogListSelectedGrayText,
		///
		WarnDialogComboGray,
		///
		WarnDialogComboSelectedGrayText,
		///
		WarnDialogListGray,
		///
		WarnDialogListSelectedGrayText,

		///
		LastPaletteColor
	}

	/// <summary>
	/// Switching between editor and viewer.
	/// Used by editor <see cref="IEditor.Switching"/> and viewer <see cref="IViewer.Switching"/>.
	/// </summary>
	public enum Switching
	{
		/// <summary>
		/// Switching is disabled if editor <see cref="IEditor.DeleteSource"/> or viewer <see cref="IViewer.DeleteSource"/> is set
		/// or there are any event handlers added to an editor or viewer.
		/// </summary>
		Auto,
		/// <summary>
		/// Switching is enabled. If you use it together with events or <c>DeleteSource</c> take into account possible side effects.
		/// </summary>
		Enabled,
		/// <summary>
		/// Switching is disabled.
		/// </summary>
		Disabled
	}

	/// <summary>
	/// Keyboard event information.
	/// </summary>
	public struct KeyInfo
	{
		bool _KeyDown;
		char _Character;
		int _VirtualKeyCode;
		ControlKeyStates _ControlKeyState;

		///
		public KeyInfo(int virtualKeyCode, char character, ControlKeyStates controlKeyState, bool keyDown)
		{
			_VirtualKeyCode = virtualKeyCode;
			_Character = character;
			_ControlKeyState = controlKeyState;
			_KeyDown = keyDown;
		}

		/// <summary>
		/// <see cref="VKeyCode"/> code.
		/// </summary>
		public int VirtualKeyCode { get { return _VirtualKeyCode; } set { _VirtualKeyCode = value; } }

		/// <summary>
		/// Character.
		/// </summary>
		public char Character { get { return _Character; } set { _Character = value; } }

		/// <summary>
		/// Gets all control key states.
		/// </summary>
		public ControlKeyStates ControlKeyState { get { return _ControlKeyState; } set { _ControlKeyState = value; } }

		/// <summary>
		/// Key down event.
		/// </summary>
		public bool KeyDown { get { return _KeyDown; } set { _KeyDown = value; } }

		/// <summary>
		/// Gets only Ctrl, Alt and Shift states.
		/// </summary>
		public ControlKeyStates CtrlAltShift { get { return _ControlKeyState & ControlKeyStates.CtrlAltShift; } }

		///
		public static bool operator ==(KeyInfo left, KeyInfo right)
		{
			return
				left._Character == right._Character &&
				left._ControlKeyState == right._ControlKeyState &&
				left._KeyDown == right._KeyDown &&
				left._VirtualKeyCode == right._VirtualKeyCode;
		}

		///
		public static bool operator !=(KeyInfo left, KeyInfo right)
		{
			return !(left == right);
		}

		///
		public override bool Equals(object obj)
		{
			return obj != null && obj.GetType() == typeof(KeyInfo) && this == (KeyInfo)obj;
		}

		///
		public override int GetHashCode()
		{
			uint num = _KeyDown ? 0x10000000u : 0;
			num |= ((uint)_ControlKeyState) << 0x10;
			num |= (uint)_VirtualKeyCode;
			return num.GetHashCode();
		}

		///
		public override string ToString()
		{
			return "Down = " + _KeyDown + "; Code = " + _VirtualKeyCode + "; Char = " + _Character + " (" + _ControlKeyState + ")";
		}

	}

	/// <summary>
	/// Mouse event information.
	/// </summary>
	public struct MouseInfo
	{
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
		/// Gets all control key states.
		/// </summary>
		public ControlKeyStates ControlKeyState { get { return _controlKeyState; } set { _controlKeyState = value; } }
		ControlKeyStates _controlKeyState;

		/// <summary>
		/// Gets only Ctrl, Alt and Shift states.
		/// </summary>
		public ControlKeyStates CtrlAltShift { get { return _controlKeyState & ControlKeyStates.CtrlAltShift; } }

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
		public override bool Equals(object obj)
		{
			return obj != null && obj.GetType() == typeof(MouseInfo) && this == (MouseInfo)obj;
		}

		///
		public override int GetHashCode()
		{
			uint num = (uint)_action + ((uint)_buttons << 8) + ((uint)_controlKeyState << 16);
			return num.GetHashCode() ^ _where.GetHashCode();
		}

		///
		public override string ToString()
		{
			return _where.ToString() + " " + _action + " (" + _buttons + ") (" + _controlKeyState + ")";
		}

	}

	/// <summary>
	/// Helper for <c>Idled</c> events with a custom frequency.
	/// </summary>
	/// <remarks>
	/// It is a helper for <c>Idled</c> events. These events may be called too frequently for
	/// a particular task. In this case use <see cref="Create"/> to get a handler with a
	/// custom call frequency.
	/// </remarks>
	public sealed class IdledHandler
	{
		DateTime _Time;
		double _Seconds;
		EventHandler _Handler;

		/// <summary>
		/// Creates a handler with a custom frequency.
		/// </summary>
		/// <param name="seconds">Time interval in seconds.</param>
		/// <param name="handler">Wrapped handler to be invoked.</param>
		public static EventHandler Create(double seconds, EventHandler handler)
		{
			if (handler == null)
				throw new ArgumentNullException("handler");

			return (new IdledHandler(seconds, handler)).Invoke;
		}

		IdledHandler(double seconds, EventHandler handler)
		{
			_Seconds = seconds;
			_Handler = handler;
		}

		void Invoke(object sender, EventArgs e)
		{
			DateTime now = DateTime.Now;
			if ((now - _Time).TotalSeconds >= _Seconds)
			{
				_Time = now;
				_Handler(sender, e);
			}
		}
	}

}
