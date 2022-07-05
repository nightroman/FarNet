
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet.Forms
{
	/// <summary>
	/// Base dialog control.
	/// </summary>
	public interface IControl
	{
		/// <summary>
		/// Called to draw the control.
		/// </summary>
		/// <remarks>
		/// Event handlers of <see cref="IUserControl"/> controls use this event to draw them.
		/// </remarks>
		event EventHandler<DrawingEventArgs> Drawing;

		/// <summary>
		/// Called after drawing the control.
		/// </summary>
		event EventHandler<DrawnEventArgs> Drawn;

		/// <summary>
		/// Called to color the control.
		/// </summary>
		/// <remarks>
		/// Event handlers change the default colors provided by the event arguments.
		/// </remarks>
		event EventHandler<ColoringEventArgs> Coloring; //! Think twice before changes (to properties), this way has advantages.

		/// <summary>
		/// Called when the control has got focus.
		/// </summary>
		event EventHandler<AnyEventArgs> GotFocus;

		/// <summary>
		/// Called when the control is losing focus.
		/// </summary>
		event EventHandler<LosingFocusEventArgs> LosingFocus;

		/// <summary>
		/// Called when the mouse has clicked on the control.
		/// </summary>
		/// <remarks>
		/// For a <see cref="IUserControl"/> mouse coordinates are relative to its left top;
		/// for other controls mouse coordinates are absolute screen coordinates.
		/// </remarks>
		event EventHandler<MouseClickedEventArgs> MouseClicked;

		/// <summary>
		/// Called when a key has been pressed.
		/// </summary>
		event EventHandler<KeyPressedEventArgs> KeyPressed;

		/// <summary>
		/// Gets the control by its ID.
		/// </summary>
		int Id { get; }

		/// <summary>
		/// Gets or sets the control text.
		/// </summary>
		string Text { get; set; }

		/// <summary>
		/// Gets or sets the disabled state flag.
		/// </summary>
		bool Disabled { get; set; }

		/// <summary>
		/// Gets or sets the hidden state flag.
		/// </summary>
		bool Hidden { get; set; }

		/// <summary>
		/// Gets or sets the control rectangular.
		/// </summary>
		Place Rect { get; set; }

		/// <summary>
		/// Gets or set any data (not used by the core).
		/// It is settable in FarNet dialogs.
		/// </summary>
		object Data { get; set; }

		/// <summary>
		/// Gets or sets a name (not used by the core).
		/// It is settable in FarNet dialogs.
		/// </summary>
		string Name { get; set; }
	}
}
