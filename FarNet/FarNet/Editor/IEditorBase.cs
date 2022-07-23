
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Common editor events.
/// </summary>
public abstract class IEditorBase
{
	/// <summary>
	/// Called when the editor is closed.
	/// </summary>
	public abstract event EventHandler Closed;

	/// <summary>
	/// Called when the editor is opened.
	/// </summary>
	public abstract event EventHandler Opened;

	/// <summary>
	/// Called before saving.
	/// </summary>
	public abstract event EventHandler<EditorSavingEventArgs> Saving;

	/// <summary>
	/// Called on a key pressed.
	/// </summary>
	public abstract event EventHandler<KeyEventArgs> KeyDown;

	/// <summary>
	/// Called on a key pressed.
	/// </summary>
	public abstract event EventHandler<KeyEventArgs> KeyUp;

	/// <summary>
	/// Occurs when a mouse button is clicked.
	/// </summary>
	public abstract event EventHandler<MouseEventArgs> MouseClick;

	/// <summary>
	/// Occurs when a mouse button is clicked two times.
	/// </summary>
	public abstract event EventHandler<MouseEventArgs> MouseDoubleClick;

	/// <summary>
	/// Occurs when the mouse pointer moves.
	/// </summary>
	public abstract event EventHandler<MouseEventArgs> MouseMove;

	/// <summary>
	/// Occurs when the mouse wheel is rotated.
	/// </summary>
	public abstract event EventHandler<MouseEventArgs> MouseWheel;

	/// <summary>
	/// Called when the editor has got focus.
	/// </summary>
	public abstract event EventHandler GotFocus;

	/// <summary>
	/// Called when the editor is losing focus.
	/// </summary>
	public abstract event EventHandler LosingFocus;

	/// <summary>
	/// Called on [CtrlC] in asynchronous mode, see <see cref="IEditor.BeginAsync"/>.
	/// </summary>
	public abstract event EventHandler CtrlCPressed;

	/// <summary>
	/// Called on redrawing.
	/// </summary>
	public abstract event EventHandler Redrawing;

	/// <summary>
	/// Called on changes.
	/// </summary>
	public abstract event EventHandler<EditorChangedEventArgs> Changed;
}
