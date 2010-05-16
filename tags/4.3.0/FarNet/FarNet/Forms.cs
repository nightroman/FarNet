/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;

namespace FarNet.Forms
{
	/// <summary>
	/// Base interface for dialog controls.
	/// </summary>
	public interface IControl
	{
		/// <summary>
		/// Event is sent to draw the control.
		/// </summary>
		event EventHandler<DrawingEventArgs> Drawing;
		/// <summary>
		/// Event is sent when the control has received keyboard focus.
		/// </summary>
		event EventHandler<AnyEventArgs> GotFocus;
		/// <summary>
		/// Event is sent before the control loses the focus.
		/// </summary>
		event EventHandler<LosingFocusEventArgs> LosingFocus;
		/// <summary>
		/// Event is sent when the mouse has clicked on the control.
		/// </summary>
		/// <remarks>
		/// For <see cref="IUserControl"/> coordinates are relative to its left top;
		/// for other controls coordinates are absolute screen coordinates.
		/// </remarks>
		event EventHandler<MouseClickedEventArgs> MouseClicked;
		/// <summary>
		/// Event is sent when a key has been pressed in the dialog.
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
		/// Control rectangular.
		/// </summary>
		Place Rect { get; set; }
	}

	/// <summary>
	/// Double line or single line box control.
	/// It is created and added to a dialog by <see cref="IDialog.AddBox"/>.
	/// </summary>
	/// <remarks>
	/// If this item is added first to the dialog then its <see cref="IControl.Text"/> is copied into the Far console title.
	/// </remarks>
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
	/// <remarks>
	/// When a button is clicked then <see cref="ButtonClicked"/> event is triggered and normally the dialog closes.
	/// <para>
	/// There are a few ways to keep the dialog running: set the button property <see cref="NoClose"/> or
	/// set the event property <see cref="ButtonClickedEventArgs.Ignore"/>.
	/// </para>
	/// </remarks>
	/// <seealso cref="IDialog.Cancel"/>
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
		/// Cannot get keyboard focus, but can handle other events.
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
		/// Cannot get keyboard focus, but can handle other events.
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
	/// It is created and added to a dialog by:
	/// <see cref="IDialog.AddEdit"/>, <see cref="IDialog.AddEditFixed"/>, <see cref="IDialog.AddEditPassword"/>.
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
		/// Tells that it is used for file system path input.
		/// </summary>
		/// <remarks>
		/// Setting this to true enables some extras, e.g. on typing: a dropdown list of matching available paths.
		/// </remarks>
		bool IsPath { get; set; }
		/// <summary>
		/// Gets true if it is used for password input.
		/// </summary>
		/// <remarks>
		/// It is true if it is created by <see cref="IDialog.AddEditPassword"/>.
		/// </remarks>
		bool IsPassword { get; }
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
		/// Cannot get keyboard focus, but can handle other events.
		/// </summary>
		bool NoFocus { get; set; }
		/// <summary>
		/// Disables auto completion from history.
		/// </summary>
		bool NoAutoComplete { get; set; }
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
		/// (example: Far color selection dialog).
		/// </summary>
		bool MoveSelect { get; set; }
		/// <summary>
		/// Sequential items having this flag set and equal lines are horizontally centered.
		/// Their horizontal coordinates are ignored.
		/// </summary>
		bool CenterGroup { get; set; }
		/// <summary>
		/// Cannot get keyboard focus, but can handle other events.
		/// </summary>
		bool NoFocus { get; set; }
		/// <summary>
		/// Show ampersand symbol in caption instead of using it for defining hotkeys.
		/// </summary>
		bool ShowAmpersand { get; set; }
	}

	/// <summary>
	/// Static text label.
	/// It is created and added to a dialog by
	/// <see cref="IDialog.AddText"/>, <see cref="IDialog.AddVerticalText"/>.
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
		/// Draws a single-line (1) or double-line (2) separator including text if any.
		/// </summary>
		int Separator { get; set; }
		/// <summary>
		/// Show ampersand symbol in caption instead of using it for defining hotkeys.
		/// </summary>
		bool ShowAmpersand { get; set; }
		/// <summary>
		/// Centers the text (horizontally or vertically).
		/// </summary>
		bool Centered { get; set; }
		/// <summary>
		/// Text is vertical.
		/// </summary>
		bool Vertical { get; }
	}

	/// <summary>
	/// User control.
	/// It is created and added to a dialog by <see cref="IDialog.AddUserControl"/>.
	/// </summary>
	/// <remarks>
	/// Use <see cref="IControl.Drawing"/> event to draw this control by
	/// <see cref="IFar.WritePalette"/> or <see cref="IFar.WriteText"/>
	/// with <see cref="IFar.GetPaletteForeground"/> and <see cref="IFar.GetPaletteBackground"/>.
	/// Also usually you should at first calculate absolute coordinates using
	/// absolute dialog <see cref="IDialog.Rect"/> and relative control <see cref="IControl.Rect"/>.
	/// <para>
	/// User control can be used to emulate <c>MouseClicked</c> event for a dialog:
	/// add this control so that it covers all the dialog area and use its event handler.
	/// </para>
	/// </remarks>
	public interface IUserControl : IControl
	{
		/// <summary>
		/// Cannot get keyboard focus, but can handle other events.
		/// </summary>
		bool NoFocus { get; set; }
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
		/// <remarks>
		/// This is the simplest way to setup items before opening a dialog.
		/// After opening it is better to create and add items directly to <see cref="Items"/>.
		/// </remarks>
		FarItem Add(string text);
		/// <summary>
		/// The selected item index.
		/// </summary>
		int Selected { get; set; }
		/// <summary>
		/// Assigns hotkeys for the items automatically, starting with the first item.
		/// </summary>
		bool AutoAssignHotkeys { get; set; }
		/// <summary>
		/// Shows hotkeys instead of showing ampersands.
		/// </summary>
		bool NoAmpersands { get; set; }
		/// <summary>
		/// Try to move the cursor up from the first element or down from the last element
		/// will move the cursor to the bottom or the top of the list.
		/// </summary>
		bool WrapCursor { get; set; }
		/// <summary>
		/// Cannot get keyboard focus, but can handle other events.
		/// </summary>
		bool NoFocus { get; set; }
		/// <summary>
		/// Do not close the dialog after an item selection.
		/// Default behaviour is to end the dialog processing.
		/// </summary>
		bool NoClose { get; set; }
		/// <summary>
		/// Tells to select the last item if <see cref="Selected"/> is not set.
		/// </summary>
		bool SelectLast { get; set; }
		/// <summary>
		/// Attaches previously detached items.
		/// </summary>
		/// <seealso cref="Items"/>
		void AttachItems();
		/// <summary>
		/// Detaches the items before large changes for better performance.
		/// You have to call <see cref="AttachItems"/> when changes are done.
		/// <seealso cref="Items"/>
		/// </summary>
		void DetachItems();
		/// <include file='doc.xml' path='docs/pp[@name="BaseListItems"]/*'/>
		IList<FarItem> Items { get; }
		/// <summary>
		/// STOP: this is workaround, use it only if <see cref="Items"/>.<b>Clear()</b> fails. Bug [_090208_042536]
		/// </summary>
		[Obsolete("Use Items.Clear() if it works fine.")]
		void Clear();
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
		/// <summary>
		/// Title line text.
		/// </summary>
		string Title { get; set; }
		/// <summary>
		/// Bottom line text.
		/// </summary>
		string Bottom { get; set; }
		/// <summary>
		/// Sets both cursor and top position. It should be called when a dialog is shown.
		/// </summary>
		void SetFrame(int selected, int top);
	}

	/// <summary>
	/// Far dialog.
	/// It is created by <see cref="IFar.CreateDialog"/>.
	/// </summary>
	/// <remarks>
	/// After creation of a dialog by <see cref="IFar.CreateDialog"/> you have to:
	/// *) create and add controls using <c>Add*</c> methods;
	/// *) set control and dialog properties and add event handlers;
	/// *) call <see cref="IDialog.Show"/>.
	/// <para>
	/// Event <see cref="Closing"/> can be used for input data validation without closing the dialog.
	/// </para>
	/// </remarks>
	public interface IDialog
	{
		/// <summary>
		/// Called when all dialog items are initialized and about to be shown.
		/// </summary>
		event EventHandler<InitializedEventArgs> Initialized;
		/// <summary>
		/// Called when the dialog is about to be closed (normally a user closes it).
		/// </summary>
		/// <remarks>
		/// This event can be used for example for input data validation before the dialog is closed.
		/// If event argument <see cref="AnyEventArgs.Control"/> is null then the dialog is about to
		/// be closed by [Esc] or [F10]; in this case normally you should not stop closing.
		/// Otherwise you may check validity of input data.
		/// If a user mistake is found you may show a message box (<see cref="IFar.Msg(string)"/>),
		/// set focus to the control with a mistake (<see cref="Focused"/>) and finally set
		/// event argument <see cref="ClosingEventArgs.Ignore"/> to keep the dialog running,
		/// so that a user may correct the input or cancel the dialog.
		/// <para>
		/// It is not recommended to change control states during this event.
		/// Doing so may trigger actions that may be unexpected on closing.
		/// </para>
		/// </remarks>
		event EventHandler<ClosingEventArgs> Closing;
		/// <summary>
		/// Called periodically when a user is idle.
		/// </summary>
		/// <seealso cref="IdledHandler"/>
		event EventHandler Idled;
		/// <summary>
		/// Called on mouse clicks outside of the dialog and on not handled clicks on the controls.
		/// </summary>
		/// <remarks>
		/// Coordinates are absolute screen coordinates.
		/// </remarks>
		event EventHandler<MouseClickedEventArgs> MouseClicked;
		/// <summary>
		/// Called when a key is pressed in the dialog and the active control does not handle the key.
		/// </summary>
		event EventHandler<KeyPressedEventArgs> KeyPressed;
		/// <summary>
		/// Called when the console window size has changed, e.g. on [AltF9].
		/// </summary>
		event EventHandler<SizeEventArgs> ConsoleSizeChanged;
		/// <summary>
		/// Gets or sets the "default control" which gets selected on [Enter] if the focus is not on a button.
		/// </summary>
		IControl Default { get; set; }
		/// <summary>
		/// Gets or sets the control which has focus.
		/// </summary>
		IControl Focused { get; set; }
		/// <summary>
		/// Gets the selected dialog control.
		/// </summary>
		/// <remarks>
		/// Normally it is a closing button or the <see cref="Default"/> control.
		/// </remarks>
		IControl Selected { get; }
		/// <summary>
		/// Tells to use "Warning" dialog color scheme.
		/// </summary>
		bool IsWarning { get; set; }
		/// <summary>
		/// Tells to create the dialog with reduced border size.
		/// </summary>
		/// <remarks>
		/// In "small" dialogs there is no space between the border and the double box.
		/// </remarks>
		bool IsSmall { get; set; }
		/// <summary>
		/// Tells to create the dialog with no shadow.
		/// </summary>
		bool NoShadow { get; set; }
		/// <summary>
		/// Tells to create the dialog with no panel shown.
		/// </summary>
		bool NoPanel { get; set; }
		/// <summary>
		/// Tells to disable use of smart coordinates.
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
		/// Gets or sets any user data.
		/// </summary>
		object Data { get; set; }
		/// <summary>
		/// Gets or sets the dialog window rectangular.
		/// </summary>
		Place Rect { get; set; }
		/// <summary>
		/// Gets or sets the dialog type ID.
		/// </summary>
		/// <remarks>
		/// It is normally set by the dialog creator.
		/// It cannot be changed for running dialogs.
		/// </remarks>
		Guid TypeId { get; set; }
		/// <summary>
		/// Gets or sets the "Cancel" button.
		/// </summary>
		/// <remarks>
		/// If this button is clicked then <see cref="Show"/> returns false.
		/// </remarks>
		IButton Cancel { get; set; }
		/// <summary>
		/// Shows the dialog.
		/// </summary>
		/// <returns>false if the user cancelled the dialog or clicked the <see cref="Cancel"/> button.</returns>
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
		/// <param name="title">Title.</param>
		IListBox AddListBox(int left, int top, int right, int bottom, string title);
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
		/// <summary>
		/// Adds a user control. See <see cref="NoSmartCoords"/>.
		/// </summary>
		/// <include file='doc.xml' path='docs/pp[@name="LTRB"]/*'/>
		IUserControl AddUserControl(int left, int top, int right, int bottom);
		/// <summary>
		/// Closes the dialog.
		/// </summary>
		void Close();
		/// <summary>
		/// Gets a control by the ID (index).
		/// </summary>
		/// <param name="id">Control ID (index).</param>
		/// <returns>Requested control or null if ID is not valid.</returns>
		IControl GetControl(int id);
		/// <summary>
		/// Sets focus to the specified control.
		/// </summary>
		/// <param name="id">Control ID (index).</param>
		void SetFocus(int id);
		/// <summary>
		/// Moves the dialog window to a new position.
		/// </summary>
		/// <param name="point">Absolute point or relative shift.</param>
		/// <param name="absolute">true: point is absolute (use -1 to center the dialog); false: point is relative.</param>
		void Move(Point point, bool absolute);
		/// <summary>
		/// Resizes the dialog window.
		/// </summary>
		/// <param name="size">New size.</param>
		void Resize(Point size);
		/// <summary>
		/// Disables redrawing of the dialog.
		/// </summary>
		/// <remarks>
		/// This method is used to prevent excessive dialog redraws when modifying multiple dialog items. 
		/// <para>
		/// It increments the internal redraw lock counter.
		/// WARNING: you must call <see cref="EnableRedraw"/> (normally when dialog changes are done).
		/// </para>
		/// </remarks>
		void DisableRedraw();
		/// <summary>
		/// Enables redrawing of the dialog.
		/// </summary>
		/// <remarks>
		/// It decrements the internal redraw lock counter; when it is equal to 0 the dialog gets drawn.
		/// WARNING: it must be called after any call of <see cref="DisableRedraw"/>.
		/// </remarks>
		void EnableRedraw();
	}

	/// <summary>
	/// Specialized dialog.
	/// </summary>
	public interface IForm
	{
		/// <summary>
		/// Form title.
		/// </summary>
		string Title { get; set; }
		/// <summary>
		/// Shows the form.
		/// </summary>
		/// <returns>False if the form was cancelled.</returns>
		bool Show();
	}

	/// <summary>
	/// Form for selecting an ordered subset of a given set of items.
	/// </summary>
	/// <remarks>
	/// Create it by <see cref="IFar.CreateSubsetForm"/>, set its <see cref="Items"/>
	/// and initial <see cref="Indexes"/>, call <see cref="IForm.Show"/>, get result <see cref="Indexes"/>.
	/// </remarks>
	public interface ISubsetForm : IForm
	{
		/// <summary>
		/// Items to select from.
		/// </summary>
		object[] Items { get; set; }
		/// <summary>
		/// Indexes of selected items.
		/// </summary>
		int[] Indexes { get; set; }
		/// <summary>
		/// Optional converter of items to strings.
		/// </summary>
		Converter<object, string> ItemToString { get; set; }
	}
}