
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace FarNet.Forms;

/// <summary>
/// Far dialog.
/// It is created by <see cref="IFar.CreateDialog"/>.
/// </summary>
/// <remarks>
/// After creation of a dialog by <see cref="IFar.CreateDialog"/> you have to:
/// <ul>
/// <li>set the dialog properties and add event handlers;</li>
/// <li>create and add controls using <c>Add*</c> methods;</li>
/// <li>set control properties and add event handlers;</li>
/// <li>show the dialog.</li>
/// </ul>
/// <para>
/// <see cref="Closing"/> may be used for input data validation without closing the dialog.
/// <see cref="Closed"/> may be used for cleaning up.
/// </para>
/// </remarks>
public abstract class IDialog
{
	/// <summary>
	/// Called when all dialog items are initialized and about to be shown.
	/// </summary>
	public abstract event EventHandler<InitializedEventArgs> Initialized;

	/// <summary>
	/// Called when the dialog is about to be closed (normally by a user).
	/// </summary>
	/// <remarks>
	/// <para>
	/// If the argument <see cref="AnyEventArgs.Control"/> is null then the dialog
	/// is about to be closed by [Esc] or [F10]. Handlers should not stop closing.
	/// </para>
	/// <para>
	/// This event can be used for input data validation before closing the dialog.
	/// If a user mistake is found you may show a message box (<see cref="IFar.Message(string)"/>),
	/// set focus to the control with a mistake (<see cref="Focused"/>) and finally set
	/// event argument <see cref="ClosingEventArgs.Ignore"/> to keep the dialog running,
	/// so that a user may correct the input or cancel the dialog.
	/// </para>
	/// <para>
	/// It is not recommended to change control states during this event.
	/// Doing so may trigger actions that may be unexpected on closing.
	/// </para>
	/// <para>
	/// If handlers do not stop closing then <see cref="Closed"/> is called and then the dialog stops.
	/// </para>
	/// </remarks>
	public abstract event EventHandler<ClosingEventArgs> Closing;

	/// <summary>
	/// Called after <see cref="Closing"/> which was not stopped.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The argument <see cref="AnyEventArgs.Control"/> is null if the dialog is canceled.
	/// </para>
	/// <para>
	/// Handlers can access the dialog controls for reading. Changes should be avoided.
	/// The dialog closes after this event for sure, unlike on <see cref="Closing"/>.
	/// </para>
	/// </remarks>
	public abstract event EventHandler<AnyEventArgs> Closed;

	/// <include file='doc.xml' path='doc/Timer/*'/>
	public abstract event EventHandler Timer;

	/// <include file='doc.xml' path='doc/TimerInterval/*'/>
	public abstract int TimerInterval { get; set; }

	/// <summary>
	/// Called on mouse clicks outside of the dialog and on not handled clicks on the controls.
	/// </summary>
	/// <remarks>
	/// Mouse coordinates are absolute screen coordinates.
	/// </remarks>
	public abstract event EventHandler<MouseClickedEventArgs> MouseClicked;

	/// <summary>
	/// Called when a key is pressed in the dialog and the active control does not handle the key.
	/// </summary>
	public abstract event EventHandler<KeyPressedEventArgs> KeyPressed;

	/// <summary>
	/// Called when the console window size has changed, e.g. on [AltF9].
	/// </summary>
	public abstract event EventHandler<SizeEventArgs> ConsoleSizeChanged;

	/// <summary>
	/// Called when the non-modal dialog has got focus.
	/// </summary>
	public abstract event EventHandler GotFocus;

	/// <summary>
	/// Called when the non-modal dialog is losing focus.
	/// </summary>
	public abstract event EventHandler LosingFocus;

	/// <summary>
	/// Gets or sets the control which has focus.
	/// </summary>
	public abstract IControl Focused { get; set; }

	/// <summary>
	/// Gets the selected dialog control.
	/// </summary>
	/// <remarks>
	/// Normally it is a closing button or the <see cref="Default"/> control.
	/// </remarks>
	public abstract IControl Selected { get; }

	/// <summary>
	/// Tells to use "Warning" dialog color scheme.
	/// </summary>
	public abstract bool IsWarning { get; set; }

	/// <summary>
	/// Tells to create the dialog with reduced border size.
	/// </summary>
	/// <remarks>
	/// In "small" dialogs there is no space between the border and the double box.
	/// </remarks>
	public abstract bool IsSmall { get; set; }

	/// <summary>
	/// Tells to keep the window title instead of setting it to the first control text.
	/// </summary>
	/// <remarks>
	/// Text of the first control added to the dialog is normally used as the main window title.
	/// Set this flag to true in order to keep the window title intact.
	/// </remarks>
	public abstract bool KeepWindowTitle { get; set; }

	/// <summary>
	/// Tells to create the dialog with no shadow.
	/// </summary>
	public abstract bool NoShadow { get; set; }

	/// <summary>
	/// Tells to create the dialog with no panel shown.
	/// </summary>
	public abstract bool NoPanel { get; set; }

	/// <summary>
	/// Tells to disable use of smart coordinates.
	/// </summary>
	/// <remarks>
	/// Smart coordinates mode: not positive <c>Top</c> is subtracted from the previous control <c>Top</c>:
	/// i.e. 0: the same line, -1: next line and so on; <c>Bottom</c> value, if any, should be relative to 0.
	/// Example: last <c>Top</c> is 5, then <c>AddBox(*, -1, *, 2, *)</c> is recalculated as <c>AddBox(*, 6, *, 8, *)</c>.
	/// </remarks>
	public abstract bool NoSmartCoordinates { get; set; }

	/// <summary>
	/// Tells the modeless fialog to stay on top.
	/// </summary>
	[Experimental("FarNet250326")]
	public abstract bool StayOnTop { get; set; }

	/// <include file='doc.xml' path='doc/HelpTopic/*'/>
	public abstract string HelpTopic { get; set; }

	/// <summary>
	/// Gets or sets the dialog window rectangular.
	/// </summary>
	public abstract Place Rect { get; set; }

	/// <summary>
	/// Gets the internal identifier.
	/// </summary>
	public abstract IntPtr Id { get; }

	/// <summary>
	/// Gets or sets the dialog type ID.
	/// </summary>
	/// <remarks>
	/// It is normally set by the dialog creator.
	/// It cannot be changed for running dialogs.
	/// </remarks>
	public abstract Guid TypeId { get; set; }

	/// <summary>
	/// Gets or sets the "Cancel" button.
	/// </summary>
	/// <remarks>
	/// If this button is clicked then <see cref="Show"/> returns false.
	/// NOTE: the opposite is not always true, see <see cref="IButton.ButtonClicked"/> remarks.
	/// </remarks>
	public abstract IButton Cancel { get; set; }

	/// <summary>
	/// Gets or sets the default control: it gets selected on [Enter] if the focus is not on a button.
	/// </summary>
	/// <remarks>
	/// NOTE: "selected" and "clicked" are different events, see <see cref="IButton.ButtonClicked"/> remarks.
	/// </remarks>
	public abstract IControl Default { get; set; }

	/// <summary>
	/// Shows the dialog.
	/// </summary>
	/// <returns>False if the user canceled the dialog or clicked the <see cref="Cancel"/> button.</returns>
	public abstract bool Show();

	/// <summary>
	/// Opens the dialog as a non-modal window.
	/// </summary>
	/// <remarks>
	/// <para>
	/// To prevent closing on mouse clicks outside the dialog, use <see cref="MouseClicked"/> and set
	/// <see cref="MouseClickedEventArgs.Ignore"/> to true if <see cref="AnyEventArgs.Control"/> is null.
	/// </para>
	/// <para>
	/// Use <see cref="Closing"/> for continuations. If <see cref="AnyEventArgs.Control"/> is null then
	/// the dialog was canceled. Otherwise, invoke some continuation using the current dialog data. Note
	/// that the dialog is not yet closed, so some continuations are better with <see cref="IFar.PostJob"/>.
	/// </para>
	/// </remarks>
	public abstract void Open();

	/// <summary>
	/// Redraws the dialog.
	/// </summary>
	public abstract void Redraw();

	/// <summary>
	/// Adds a double or single box control. See <see cref="NoSmartCoordinates"/>.
	/// </summary>
	/// <include file='doc.xml' path='doc/LTRB/*'/>
	/// <param name="text">Control text.</param>
	/// <remarks>
	/// If <c>right</c>\<c>bottom</c> is 0 then it is calculated.
	/// </remarks>
	public abstract IBox AddBox(int left, int top, int right, int bottom, string? text);

	/// <summary>
	/// Adds a button control. See <see cref="NoSmartCoordinates"/>.
	/// </summary>
	/// <include file='doc.xml' path='doc/LT/*'/>
	/// <param name="text">Control text.</param>
	public abstract IButton AddButton(int left, int top, string? text);

	/// <summary>
	/// Adds a check box control. See <see cref="NoSmartCoordinates"/>.
	/// </summary>
	/// <include file='doc.xml' path='doc/LT/*'/>
	/// <param name="text">Control text.</param>
	public abstract ICheckBox AddCheckBox(int left, int top, string? text);

	/// <summary>
	/// Adds a combo box control. See <see cref="NoSmartCoordinates"/>.
	/// </summary>
	/// <include file='doc.xml' path='doc/LTR/*'/>
	/// <param name="text">Control text.</param>
	public abstract IComboBox AddComboBox(int left, int top, int right, string? text);

	/// <summary>
	/// Adds a standard edit control. See <see cref="NoSmartCoordinates"/>.
	/// </summary>
	/// <include file='doc.xml' path='doc/LTR/*'/>
	/// <param name="text">Control text.</param>
	public abstract IEdit AddEdit(int left, int top, int right, string? text);

	/// <summary>
	/// Adds a fixed size edit control. See <see cref="NoSmartCoordinates"/>.
	/// </summary>
	/// <include file='doc.xml' path='doc/LTR/*'/>
	/// <param name="text">Control text.</param>
	public abstract IEdit AddEditFixed(int left, int top, int right, string? text);

	/// <summary>
	/// Adds a password edit control. See <see cref="NoSmartCoordinates"/>.
	/// </summary>
	/// <include file='doc.xml' path='doc/LTR/*'/>
	/// <param name="text">Control text.</param>
	public abstract IEdit AddEditPassword(int left, int top, int right, string? text);

	/// <summary>
	/// Adds a list box control. See <see cref="NoSmartCoordinates"/>.
	/// </summary>
	/// <include file='doc.xml' path='doc/LTRB/*'/>
	/// <param name="title">Title.</param>
	public abstract IListBox AddListBox(int left, int top, int right, int bottom, string? title);

	/// <summary>
	/// Adds a radio button. See <see cref="NoSmartCoordinates"/>.
	/// </summary>
	/// <include file='doc.xml' path='doc/LT/*'/>
	/// <param name="text">Control text.</param>
	public abstract IRadioButton AddRadioButton(int left, int top, string? text);

	/// <summary>
	/// Adds a text control. See <see cref="NoSmartCoordinates"/>.
	/// </summary>
	/// <include file='doc.xml' path='doc/LTR/*'/>
	/// <param name="text">Control text.</param>
	/// <remarks>
	/// For separators use of -1 as position tells to center the text (alternative to <see cref="IText.Centered"/>).
	/// </remarks>
	public abstract IText AddText(int left, int top, int right, string? text);

	/// <summary>
	/// Adds a vertical text control. See <see cref="NoSmartCoordinates"/>.
	/// </summary>
	/// <include file='doc.xml' path='doc/LTB/*'/>
	/// <param name="text">Control text.</param>
	public abstract IText AddVerticalText(int left, int top, int bottom, string? text);

	/// <summary>
	/// Adds a user control. See <see cref="NoSmartCoordinates"/>.
	/// </summary>
	/// <include file='doc.xml' path='doc/LTRB/*'/>
	public abstract IUserControl AddUserControl(int left, int top, int right, int bottom);

	/// <summary>
	/// Closes the dialog.
	/// </summary>
	public void Close() => Close(-1);

	/// <summary>
	/// Closes the dialog.
	/// </summary>
	/// <param name="id">
	/// Specifies the selected item ID.
	/// Use -1 for the focused item.
	/// </param>
	public abstract void Close(int id);

	/// <summary>
	/// Gets a control by its ID.
	/// </summary>
	/// <param name="id">Control ID.</param>
	/// <returns>Requested control or null if ID is not valid.</returns>
	/// <remarks>
	/// Control IDs are indexes in the dialog control collection.
	/// </remarks>
	public abstract IControl this[int id] { get; }

	/// <summary>
	/// Gets the dialog control collection.
	/// </summary>
	/// <remarks>
	/// It should be used only when control indexes are not known or not used.
	/// Otherwise <see cref="this"/> should be used.
	/// </remarks>
	public abstract IEnumerable<IControl> Controls { get; }

	/// <summary>
	/// Sets focus to the specified control.
	/// </summary>
	/// <param name="id">Control ID (index).</param>
	public abstract void SetFocus(int id);

	/// <summary>
	/// Moves the dialog window to a new position.
	/// </summary>
	/// <param name="point">Absolute point or relative shift.</param>
	/// <param name="absolute">true: point is absolute (use -1 to center the dialog); false: point is relative.</param>
	public abstract void Move(Point point, bool absolute);

	/// <summary>
	/// Resizes the dialog window.
	/// </summary>
	/// <param name="size">New size.</param>
	public abstract void Resize(Point size);

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
	public abstract void DisableRedraw();

	/// <summary>
	/// Enables redrawing of the dialog.
	/// </summary>
	/// <remarks>
	/// It decrements the internal redraw lock counter; when it is equal to 0 the dialog gets drawn.
	/// WARNING: it must be called after any call of <see cref="DisableRedraw"/>.
	/// </remarks>
	public abstract void EnableRedraw();

	/// <include file='doc.xml' path='doc/Data/*'/>
	public Hashtable Data => _Data ??= [];
	Hashtable? _Data;

	/// <summary>
	/// Makes the window current.
	/// </summary>
	public void Activate()
	{
		var myId = Id;
		for (int i = Far.Api.Window.Count; --i >= 0;)
		{
			if (Far.Api.Window.GetIdAt(i) == myId)
			{
				Far.Api.Window.SetCurrentAt(i);
				return;
			}
		}
	}
}
