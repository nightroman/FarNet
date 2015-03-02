
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System;

namespace FarNet
{
	/// <summary>
	/// Read key options.
	/// </summary>
	[Flags]
	public enum ReadKeyOptions
	{
		/// <summary>
		/// Allow the CTRL+C key to be processed as a keystroke, as opposed to causing a break event.
		/// </summary>
		AllowCtrlC = 1,
		/// <summary>
		/// Do not display the character in the window when the key is pressed.
		/// </summary>
		NoEcho = 2,
		/// <summary>
		/// Allow key-down events.
		/// </summary>
		IncludeKeyDown = 4,
		/// <summary>
		/// Allow key-up events.
		/// </summary>
		IncludeKeyUp = 8,
	}

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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
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
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
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
	/// Virtual key codes.
	/// </summary>
	/// <remarks>
	/// They are similar to <c>System.ConsoleKey</c> and <c>System.Windows.Forms.Keys</c>.
	/// <para>
	/// [enum]::GetNames([ConsoleKey]) | %{ '{0} = {1},' -f $_, [int][ConsoleKey]$_ }
	/// </para>
	/// </remarks>
	public static class KeyCode
	{
		/// <summary>
		/// .
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
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
	/// <seealso cref="IUserInterface.DrawColor"/>
	/// <seealso cref="IUserInterface.DrawPalette"/>
	/// <seealso cref="IUserInterface.GetPaletteForeground"/>
	/// <seealso cref="IUserInterface.GetPaletteBackground"/>
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
	/// Base class for keyboard related classes.
	/// </summary>
	public abstract class KeyBase
	{
		ControlKeyStates _ControlKeyState;
		///
		protected KeyBase()
		{ }
		/// <param name="controlKeyState">See <see cref="ControlKeyState"/></param>
		protected KeyBase(ControlKeyStates controlKeyState)
		{
			_ControlKeyState = controlKeyState;
		}
		/// <summary>
		/// Gets all control key states including special flags.
		/// </summary>
		public ControlKeyStates ControlKeyState { get { return _ControlKeyState; } }
		/// <summary>
		/// Tests no Ctrl, Alt, or Shift.
		/// </summary>
		public bool Is()
		{
			return 0 == (_ControlKeyState & ControlKeyStates.CtrlAltShift);
		}
		/// <summary>
		/// Tests Alt state.
		/// </summary>
		public bool IsAlt()
		{
			var value = _ControlKeyState & ControlKeyStates.CtrlAltShift;
			return value == ControlKeyStates.LeftAltPressed || value == ControlKeyStates.RightAltPressed;
		}
		/// <summary>
		/// Tests Ctrl state.
		/// </summary>
		public bool IsCtrl()
		{
			var value = _ControlKeyState & ControlKeyStates.CtrlAltShift;
			return value == ControlKeyStates.LeftCtrlPressed || value == ControlKeyStates.RightCtrlPressed;
		}
		/// <summary>
		/// Tests Shift state.
		/// </summary>
		public bool IsShift()
		{
			return (_ControlKeyState & ControlKeyStates.CtrlAltShift) == ControlKeyStates.ShiftPressed;
		}
		/// <summary>
		/// Tests AltShift state.
		/// </summary>
		public bool IsAltShift()
		{
			var value = _ControlKeyState & ControlKeyStates.CtrlAltShift;
			return
				value == (ControlKeyStates.ShiftPressed | ControlKeyStates.LeftAltPressed) ||
				value == (ControlKeyStates.ShiftPressed | ControlKeyStates.RightAltPressed);
		}
		/// <summary>
		/// Tests CtrlAlt state.
		/// </summary>
		public bool IsCtrlAlt()
		{
			var value = _ControlKeyState & ControlKeyStates.CtrlAltShift;
			return
				value == (ControlKeyStates.LeftCtrlPressed | ControlKeyStates.LeftAltPressed) ||
				value == (ControlKeyStates.RightCtrlPressed | ControlKeyStates.RightAltPressed);
		}
		/// <summary>
		/// Tests CtrlShift state.
		/// </summary>
		public bool IsCtrlShift()
		{
			var value = _ControlKeyState & ControlKeyStates.CtrlAltShift;
			return
				value == (ControlKeyStates.ShiftPressed | ControlKeyStates.LeftCtrlPressed) ||
				value == (ControlKeyStates.ShiftPressed | ControlKeyStates.RightCtrlPressed);
		}
		/// <summary>
		/// Gets only Ctrl, Alt, and Shift states excluding special flags.
		/// </summary>
		public ControlKeyStates CtrlAltShift()
		{
			return _ControlKeyState & ControlKeyStates.CtrlAltShift;
		}
		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			var that = obj as KeyBase;
			return that != null &&
				_ControlKeyState == that._ControlKeyState;
		}
		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return (int)_ControlKeyState;
		}
		/// <summary>
		/// Returns the string "ControlKeyState".
		/// </summary>
		public override string ToString()
		{
			return _ControlKeyState.ToString();
		}
	}

	/// <summary>
	/// Minimal key data.
	/// </summary>
	public class KeyData : KeyBase
	{
		static readonly KeyData _Empty = new KeyData(0);
		int _VirtualKeyCode;
		/// <param name="virtualKeyCode">See <see cref="VirtualKeyCode"/></param>
		public KeyData(int virtualKeyCode)
		{
			_VirtualKeyCode = virtualKeyCode;
		}
		/// <param name="virtualKeyCode">See <see cref="VirtualKeyCode"/></param>
		/// <param name="controlKeyState">See <see cref="KeyBase.ControlKeyState"/></param>
		public KeyData(int virtualKeyCode, ControlKeyStates controlKeyState)
			: base(controlKeyState)
		{
			_VirtualKeyCode = virtualKeyCode;
		}
		/// <summary>
		/// Gets the empty key instance.
		/// </summary>
		public static KeyData Empty { get { return _Empty; } }
		/// <summary>
		/// Gets the <see cref="KeyCode"/> code.
		/// </summary>
		public int VirtualKeyCode { get { return _VirtualKeyCode; } }
		/// <summary>
		/// Tests a key code with no Ctrl, Alt, or Shift.
		/// </summary>
		/// <param name="virtualKeyCode">The key code to test.</param>
		public bool Is(int virtualKeyCode)
		{
			return _VirtualKeyCode == virtualKeyCode && Is();
		}
		/// <summary>
		/// Tests a key code with Alt.
		/// </summary>
		/// <param name="virtualKeyCode">The key code to test.</param>
		public bool IsAlt(int virtualKeyCode)
		{
			return _VirtualKeyCode == virtualKeyCode && IsAlt();
		}
		/// <summary>
		/// Tests a key code with Ctrl.
		/// </summary>
		/// <param name="virtualKeyCode">The key code to test.</param>
		public bool IsCtrl(int virtualKeyCode)
		{
			return _VirtualKeyCode == virtualKeyCode && IsCtrl();
		}
		/// <summary>
		/// Tests a key code with Shift.
		/// </summary>
		/// <param name="virtualKeyCode">The key code to test.</param>
		public bool IsShift(int virtualKeyCode)
		{
			return _VirtualKeyCode == virtualKeyCode && IsShift();
		}
		/// <summary>
		/// Tests a key with AltShift.
		/// </summary>
		/// <param name="virtualKeyCode">The key code to test.</param>
		public bool IsAltShift(int virtualKeyCode)
		{
			return _VirtualKeyCode == virtualKeyCode && IsAltShift();
		}
		/// <summary>
		/// Tests a key with CtrlAlt.
		/// </summary>
		/// <param name="virtualKeyCode">The key code to test.</param>
		public bool IsCtrlAlt(int virtualKeyCode)
		{
			return _VirtualKeyCode == virtualKeyCode && IsCtrlAlt();
		}
		/// <summary>
		/// Tests a key with CtrlShift.
		/// </summary>
		/// <param name="virtualKeyCode">The key code to test.</param>
		public bool IsCtrlShift(int virtualKeyCode)
		{
			return _VirtualKeyCode == virtualKeyCode && IsCtrlShift();
		}
		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			var that = obj as KeyData;
			return that != null &&
				_VirtualKeyCode == that._VirtualKeyCode &&
				ControlKeyState == that.ControlKeyState;
		}
		/// <inheritdoc/>
		public override int GetHashCode()
		{
			uint num = ((uint)ControlKeyState) << 0x10 | (uint)_VirtualKeyCode;
			return num.GetHashCode();
		}
		/// <summary>
		/// Returns the string "(ControlKeyState)VirtualKeyCode".
		/// </summary>
		public override string ToString()
		{
			return "(" + ControlKeyState + ")" + _VirtualKeyCode;
		}
	}

	/// <summary>
	/// Full key information.
	/// </summary>
	public sealed class KeyInfo : KeyData
	{
		char _Character;
		bool _KeyDown;
		/// <param name="virtualKeyCode">See <see cref="KeyData.VirtualKeyCode"/></param>
		/// <param name="character">See <see cref="Character"/></param>
		/// <param name="controlKeyState">See <see cref="KeyBase.ControlKeyState"/></param>
		/// <param name="keyDown">See <see cref="KeyDown"/></param>
		public KeyInfo(int virtualKeyCode, char character, ControlKeyStates controlKeyState, bool keyDown)
			: base(virtualKeyCode, controlKeyState)
		{
			_Character = character;
			_KeyDown = keyDown;
		}
		/// <summary>
		/// Gets the character of the key.
		/// </summary>
		public char Character { get { return _Character; } }
		/// <summary>
		/// Gets true for the key down event.
		/// </summary>
		public bool KeyDown { get { return _KeyDown; } }
		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			var that = obj as KeyInfo;
			return that != null &&
				VirtualKeyCode == that.VirtualKeyCode &&
				ControlKeyState == that.ControlKeyState &&
				_Character == that._Character &&
				_KeyDown == that._KeyDown;
		}
		/// <inheritdoc/>
		public override int GetHashCode()
		{
			uint num = _KeyDown ? 0x10000000u : 0;
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

	/// <summary>
	/// Mouse event information.
	/// </summary>
	public sealed class MouseInfo : KeyBase
	{
		Point _Where;
		MouseAction _Action;
		MouseButtons _Buttons;
		int _Value;
		/// <param name="where">Position.</param>
		/// <param name="action">Action.</param>
		/// <param name="buttons">Buttons.</param>
		/// <param name="controls">Control keys.</param>
		/// <param name="value">Wheel value.</param>
		public MouseInfo(Point where, MouseAction action, MouseButtons buttons, ControlKeyStates controls, int value)
			: base(controls)
		{
			_Where = where;
			_Buttons = buttons;
			_Action = action;
			_Value = value;
		}
		/// <summary>
		/// Mouse positon.
		/// </summary>
		public Point Where { get { return _Where; } }
		/// <summary>
		/// Action.
		/// </summary>
		public MouseAction Action { get { return _Action; } }
		/// <summary>
		/// Buttons.
		/// </summary>
		public MouseButtons Buttons { get { return _Buttons; } }
		/// <summary>
		/// Wheel value.
		/// </summary>
		/// <remarks>
		/// It is positive or negative depending on the wheel direction.
		/// The value is normally 120*X but it depends on the mouse driver.
		/// </remarks>
		public int Value { get { return _Value; } }
		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			var that = obj as MouseInfo;
			return that != null &&
				_Action == that._Action &&
				_Buttons == that._Buttons &&
				ControlKeyState == that.ControlKeyState &&
				_Where == that._Where;
		}
		/// <inheritdoc/>
		public override int GetHashCode()
		{
			uint num = (uint)_Action + ((uint)_Buttons << 8) + ((uint)ControlKeyState << 16);
			return num.GetHashCode() ^ _Where.GetHashCode();
		}
		/// <summary>
		/// Returns the string "{0} {1} ({2}) ({3})", Where, Action, Buttons, ControlKeyState.
		/// </summary>
		public override string ToString()
		{
			return string.Format(null, "{0} {1} ({2}) ({3})", Where, Action, Buttons, ControlKeyState);
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
