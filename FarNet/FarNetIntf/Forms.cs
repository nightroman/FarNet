/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

using System.Collections.Generic;
using System;

namespace FarManager.Forms
{
	/// <summary>
	/// Base interface for dialog controls.
	/// </summary>
	public interface IControl
	{
		/// <summary>
		/// Event is sent after a dialog item has received keyboard focus.
		/// </summary>
		event EventHandler<AnyEventArgs> GotFocus;
		/// <summary>
		/// Event is sent before a dialog item loses the focus.
		/// </summary>
		event EventHandler<LosingFocusEventArgs> LosingFocus;
		/// <summary>
		/// Event is sent after the user clicks the mouse on one of the dialog items.
		/// </summary>
		event EventHandler<MouseClickedEventArgs> MouseClicked;
		/// <summary>
		/// Event is sent after the user presses a key in the dialog. 
		/// </summary>
		event EventHandler<KeyPressedEventArgs> KeyPressed;
		/// <summary>
		/// Internal control ID.
		/// </summary>
		int Id { get; }
		/// <summary>
		/// Control text.
		/// </summary>
		string Text { get; set; }
		/// <summary>
		/// Disables user access to the control.
		/// </summary>
		bool Disabled { get; set; }
		/// <summary>
		/// Hides a dialog item. 
		/// </summary>
		bool Hidden { get; set; }
		/// <summary>
		/// Gets a rectangular.
		/// </summary>
		Place Rect { get; }
	}

	/// <summary>
	/// Double line or single line box control.
	/// It is created and added to a dialog by <see cref="IDialog.AddBox"/>.
	/// </summary>
	public interface IBox : IControl
	{
		/// <summary>
		/// Single box control.
		/// </summary>
		bool Single { get; set; }
		/// <summary>
		/// The caption of the frame will be left aligned.
		/// </summary>
		bool LeftText { get; set; }
		/// <summary>
		/// Show ampersand symbol in caption instead of using it for defining hotkeys.
		/// </summary>
		bool ShowAmpersand { get; set; }
	}

	/// <summary>
	/// Button control.
	/// It is created and added to a dialog by <see cref="IDialog.AddButton"/>.
	/// </summary>
	public interface IButton : IControl
	{
		/// <summary>
		/// Event is sent on clicking or similar action.
		/// </summary>
		event EventHandler<ButtonClickedEventArgs> ButtonClicked;
		/// <summary>
		/// Disables dialog closing after pressing the button.
		/// </summary>
		bool NoClose { get; set; }
		/// <summary>
		/// Sequential items having this flag set and equal lines are horizontally centered.
		/// Their horizontal coordinates are ignored.
		/// </summary>
		bool CenterGroup { get; set; }
		/// <summary>
		/// Display button titles without brackets.
		/// </summary>
		bool NoBrackets { get; set; }
		/// <summary>
		/// The dialog item cannot receive keyboard focus, but can handle other user events.
		/// </summary>
		bool NoFocus { get; set; }
		/// <summary>
		/// Show ampersand symbol in caption instead of using it for defining hotkeys.
		/// </summary>
		bool ShowAmpersand { get; set; }
	}

	/// <summary>
	/// Check box control.
	/// It is created and added to a dialog by <see cref="IDialog.AddCheckBox"/>.
	/// </summary>
	public interface ICheckBox : IControl
	{
		/// <summary>
		/// Event is sent on clicking or similar action.
		/// </summary>
		event EventHandler<ButtonClickedEventArgs> ButtonClicked;
		/// <summary>
		/// Selected state.
		/// Standard: 0: off; 1: on.
		/// ThreeState: 0: off; 1: on; 2: undefined.
		/// </summary>
		int Selected { get; set; }
		/// <summary>
		/// Sequential items having this flag set and equal lines are horizontally centered.
		/// Their horizontal coordinates are ignored.
		/// </summary>
		bool CenterGroup { get; set; }
		/// <summary>
		/// The dialog item cannot receive keyboard focus, but can handle other user events.
		/// </summary>
		bool NoFocus { get; set; }
		/// <summary>
		/// The checkbox will have 3 possible states: "off", "on", "undefined".
		/// </summary>
		bool ThreeState { get; set; }
		/// <summary>
		/// Show ampersand symbol in caption instead of using it for defining hotkeys.
		/// </summary>
		bool ShowAmpersand { get; set; }
	}

	/// <summary>
	/// Edit control.
	/// It is created and added to a dialog by <see cref="IDialog.AddEdit"/>.
	/// </summary>
	public interface IEdit : IControl
	{
		/// <summary>
		/// Event is sent when an edit item has changed (for example, a character has been entered).
		/// </summary>
		event EventHandler<TextChangedEventArgs> TextChanged;
		/// <summary>
		/// Fixed size edit control.
		/// </summary>
		bool Fixed { get; }
		/// <summary>
		/// Password edit control.
		/// </summary>
		bool Password { get; }
		/// <summary>
		/// History name to be used. It overrides <see cref="Mask"/> text if any.
		/// </summary>
		string History { get; set; }
		/// <summary>
		/// Mask for fixed size mode. It overrides <see cref="History"/> text if any.
		/// </summary>
		string Mask { get; set; }
		/// <summary>
		/// Sequential edit controls with this flag are grouped into a simple editor with the ability to insert and delete lines.
		/// </summary>
		bool Editor { get; set; }
		/// <summary>
		/// Specifies that items will be added to the history list of an edit box only manually, not automatically.
		/// Must be used together with <see cref="History"/>.
		/// </summary>
		bool ManualAddHistory { get; set; }
		/// <summary>
		/// The initial value will be set to the last history element.
		/// </summary>
		bool UseLastHistory { get; set; }
		/// <summary>
		/// Expand environment variables.
		/// </summary>
		bool EnvExpanded { get; set; }
		/// <summary>
		/// Sets read-only state for the edit control.
		/// </summary>
		bool ReadOnly { get; set; }
		/// <summary>
		/// Makes the edit control always select the text when it receives the focus.
		/// </summary>
		bool SelectOnEntry { get; set; }
		/// <summary>
		/// The dialog item cannot receive keyboard focus, but can handle other user events.
		/// </summary>
		bool NoFocus { get; set; }
		/// <summary>
		/// <see cref="ILine"/> interface.
		/// </summary>
		ILine Line { get; }
	}

	/// <summary>
	/// Radio button control.
	/// It is created and added to a dialog by <see cref="IDialog.AddRadioButton"/>.
	/// </summary>
	public interface IRadioButton : IControl
	{
		/// <summary>
		/// Event is sent on clicking or similar action.
		/// </summary>
		event EventHandler<ButtonClickedEventArgs> ButtonClicked;
		/// <summary>
		/// Selected state.
		/// </summary>
		bool Selected { get; set; }
		/// <summary>
		/// This flag should be set for the first radio button item in a group.
		/// </summary>
		bool Group { get; set; }
		/// <summary>
		/// Change selection in a radio button group when focus is moved.
		/// Radio buttons with this flag set are also drawn without parentheses around the selection mark
		/// (example: FAR color selection dialog).
		/// </summary>
		bool MoveSelect { get; set; }
		/// <summary>
		/// Sequential items having this flag set and equal lines are horizontally centered.
		/// Their horizontal coordinates are ignored.
		/// </summary>
		bool CenterGroup { get; set; }
		/// <summary>
		/// The dialog item cannot receive keyboard focus, but can handle other user events.
		/// </summary>
		bool NoFocus { get; set; }
		/// <summary>
		/// Show ampersand symbol in caption instead of using it for defining hotkeys.
		/// </summary>
		bool ShowAmpersand { get; set; }
	}

	/// <summary>
	/// Static text label.
	/// It is created and added to a dialog by <see cref="IDialog.AddText"/>.
	/// </summary>
	public interface IText : IControl
	{
		/// <summary>
		/// The text item will be displayed using frame color.
		/// </summary>
		bool BoxColor { get; set; }
		/// <summary>
		/// Sequential items having this flag set and equal lines are horizontally centered.
		/// Their horizontal coordinates are ignored.
		/// </summary>
		bool CenterGroup { get; set; }
		/// <summary>
		/// Draws a single-line separator including text if any.
		/// </summary>
		bool Separator { get; set; }
		/// <summary>
		/// Draws a double-line separator including text if any.
		/// </summary>
		bool Separator2 { get; set; }
		/// <summary>
		/// Show ampersand symbol in caption instead of using it for defining hotkeys.
		/// </summary>
		bool ShowAmpersand { get; set; }
		/// <summary>
		/// Centers the text (horizontally or vertically).
		/// </summary>
		bool Centered { get; set; }
	}

	/// <summary>
	/// Item of <see cref="IComboBox"/> and <see cref="IListBox"/>.
	/// </summary>
	public interface IListItem
	{
		/// <summary>
		/// Item text.
		/// </summary>
		string Text { get; set; }
		/// <summary>
		/// Is it checked?
		/// </summary>
		bool Checked { get; set; }
		/// <summary>
		/// Is it disabled?
		/// </summary>
		bool Disabled { get; set; }
		/// <summary>
		/// Is it separator?
		/// </summary>
		bool IsSeparator { get; set; }
		/// <summary>
		/// Any user data.
		/// </summary>
		object Data { get; set; }
	}

	/// <summary>
	/// Base interface for <see cref="IComboBox"/> and <see cref="IListBox"/>.
	/// </summary>
	public interface IBaseList : IControl
	{
		/// <summary>
		/// Adds and returns a new item.
		/// </summary>
		/// <param name="text">Item text.</param>
		IListItem Add(string text);
		/// <summary>
		/// Selected item.
		/// </summary>
		int Selected { get; set; }
		/// <summary>
		/// Assigns hotkeys for the list elements automatically, starting with the first item.
		/// </summary>
		bool AutoAssignHotkeys { get; set; }
		/// <summary>
		/// Shows a hotkey instead of showing the ampersand itself.
		/// </summary>
		bool NoAmpersands { get; set; }
		/// <summary>
		/// Try to move the cursor up from the first element or down from the last element
		/// will move the cursor to the bottom or the top of the list, respectively.
		/// </summary>
		bool WrapCursor { get; set; }
		/// <summary>
		/// The dialog item cannot receive keyboard focus, but can handle other user events.
		/// </summary>
		bool NoFocus { get; set; }
		/// <summary>
		/// Don't not to close the dialog after item selection.
		/// Default list behavior after item selection is to end dialog processing.
		/// </summary>
		bool NoClose { get; set; }
	}

	/// <summary>
	/// Combo box control.
	/// It is created and added to a dialog by <see cref="IDialog.AddComboBox"/>.
	/// </summary>
	public interface IComboBox : IBaseList
	{
		/// <summary>
		/// Event is sent when an edit item has changed (for example, a character has been entered).
		/// </summary>
		event EventHandler<TextChangedEventArgs> TextChanged;
		/// <summary>
		/// Shows non-editable drop-down list instead of a common combo box.
		/// </summary>
		bool DropDownList { get; set; }
		/// <summary>
		/// Expand environment variables.
		/// </summary>
		bool EnvExpanded { get; set; }
		/// <summary>
		/// Sets read-only state for the edit control.
		/// </summary>
		bool ReadOnly { get; set; }
		/// <summary>
		/// Makes the edit control always select the text when it receives the focus.
		/// </summary>
		bool SelectOnEntry { get; set; }
		/// <summary>
		/// <see cref="ILine"/> interface.
		/// </summary>
		ILine Line { get; }
	}

	/// <summary>
	/// List box control.
	/// It is created and added to a dialog by <see cref="IDialog.AddListBox"/>.
	/// </summary>
	public interface IListBox : IBaseList
	{
		/// <summary>
		/// Disables the drawing of a frame around the list.
		/// </summary>
		bool NoBox { get; set; }
	}

	/**
<summary>
FAR dialog.
It is created by <see cref="IFar.CreateDialog"/>.
</summary>
<remarks>
After creation of a dialog by <see cref="IFar.CreateDialog"/> you have to:
*) create and add controls using <c>Add*</c> methods;
*) set control and dialog properties and|or add event handlers;
*) call <see cref="IDialog.Show"/>.
</remarks>
*/
	public interface IDialog
	{
		/// <summary>
		/// Event is sent after all dialog items are initialized, but before they are displayed.
		/// </summary>
		event EventHandler<InitializedEventArgs> Initialized;
		/// <summary>
		/// Event is sent as a notification before the dialog is closed - the user wants to close the dialog.
		/// </summary>
		event EventHandler<ClosingEventArgs> Closing;
		/// <summary>
		/// Event is sent to the dialog when the dialog enters the idle state.
		/// </summary>
		event EventHandler<AnyEventArgs> Idled;
		/// <summary>
		/// Event is sent after the user clicks the mouse outside the dialog.
		/// </summary>
		event EventHandler<MouseClickedEventArgs> MouseClicked;
		/// <summary>
		/// Event is sent after the user presses a key in the dialog. 
		/// </summary>
		event EventHandler<KeyPressedEventArgs> KeyPressed;
		/// <summary>
		/// "Default control" which is selected on [Enter] if the focus is not set on a button.
		/// </summary>
		IControl Default { get; set; }
		/// <summary>
		/// Control which has focus.
		/// </summary>
		IControl Focused { get; set; }
		/// <summary>
		/// Selected dialog item.
		/// </summary>
		IControl Selected { get; }
		/// <summary>
		/// Sets "Warning" color scheme for the dialog. 
		/// </summary>
		bool IsWarning { get; set; }
		/// <summary>
		/// Allows to create dialogs with reduced border size:
		/// there is no shadow and for separators there is no space between dialog border and dialog double box.
		/// </summary>
		bool IsSmall { get; set; }
		/// <summary>
		/// Don't draw shadow under the dialog.
		/// </summary>
		bool NoShadow { get; set; }
		/// <summary>
		/// Don't draw dialog panel. 
		/// </summary>
		bool NoPanel { get; set; }
		/// <summary>
		/// Disable smart coordinates.
		/// </summary>
		/// <remarks>
		/// Smart coordinates mode: not positive <c>Top</c> is subtracted from the previous control <c>Top</c>:
		/// i.e. 0: the same line, -1: next line and so on; <c>Bottom</c> value, if any, should be relative to 0.
		/// Example: last <c>Top</c> is 5, then <c>AddBox(*, -1, *, 2, *)</c> is recalculated as <c>AddBox(*, 6, *, 8, *)</c>.
		/// </remarks>
		bool NoSmartCoords { get; set; }
		/// <include file='doc.xml' path='docs/pp[@name="HelpTopic"]/*'/>
		string HelpTopic { get; set; }
		/// <summary>
		/// Any user data.
		/// </summary>
		object Data { get; set; }
		/// <summary>
		/// Gets a rectangular.
		/// </summary>
		Place Rect { get; }
		/// <summary>
		/// If it is set and the button is pushed <see cref="Show"/> returns <c>false</c>.
		/// </summary>
		IButton Cancel { get; set; }
		/// <summary>
		/// Shows a dialog.
		/// </summary>
		/// <returns>false if the user cancelled the dialog or pushed <see cref="Cancel"/> button.</returns>
		bool Show();
		/// <summary>
		/// Adds a double or single box control. See <see cref="NoSmartCoords"/>.
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="LTRB"]/*'/>
		/// <param name="text">Control text.</param>
		/// <remarks>
		/// If <c>right</c>\<c>bottom</c> is 0 then it is calculated.
		/// </remarks>
		IBox AddBox(int left, int top, int right, int bottom, string text);
		/// <summary>
		/// Adds a button control. See <see cref="NoSmartCoords"/>.
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="LT"]/*'/>
		/// <param name="text">Control text.</param>
		IButton AddButton(int left, int top, string text);
		/// <summary>
		/// Adds a check box control. See <see cref="NoSmartCoords"/>.
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="LT"]/*'/>
		/// <param name="text">Control text.</param>
		ICheckBox AddCheckBox(int left, int top, string text);
		/// <summary>
		/// Adds a combo box control. See <see cref="NoSmartCoords"/>.
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="LTR"]/*'/>
		/// <param name="text">Control text.</param>
		IComboBox AddComboBox(int left, int top, int right, string text);
		/// <summary>
		/// Adds a standard edit control. See <see cref="NoSmartCoords"/>.
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="LTR"]/*'/>
		/// <param name="text">Control text.</param>
		IEdit AddEdit(int left, int top, int right, string text);
		/// <summary>
		/// Adds a fixed size edit control. See <see cref="NoSmartCoords"/>.
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="LTR"]/*'/>
		/// <param name="text">Control text.</param>
		IEdit AddEditFixed(int left, int top, int right, string text);
		/// <summary>
		/// Adds a password edit control. See <see cref="NoSmartCoords"/>.
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="LTR"]/*'/>
		/// <param name="text">Control text.</param>
		IEdit AddEditPassword(int left, int top, int right, string text);
		/// <summary>
		/// Adds a list box control. See <see cref="NoSmartCoords"/>.
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="LTRB"]/*'/>
		/// <param name="text">Control text.</param>
		IListBox AddListBox(int left, int top, int right, int bottom, string text);
		/// <summary>
		/// Adds a radio button. See <see cref="NoSmartCoords"/>.
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="LT"]/*'/>
		/// <param name="text">Control text.</param>
		IRadioButton AddRadioButton(int left, int top, string text);
		/// <summary>
		/// Adds a text control. See <see cref="NoSmartCoords"/>.
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="LTR"]/*'/>
		/// <param name="text">Control text.</param>
		IText AddText(int left, int top, int right, string text);
		/// <summary>
		/// Adds a vertical text control. See <see cref="NoSmartCoords"/>.
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="LTB"]/*'/>
		/// <param name="text">Control text.</param>
		IText AddVerticalText(int left, int top, int bottom, string text);
	}
}
