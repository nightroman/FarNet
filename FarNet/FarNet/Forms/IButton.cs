
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet.Forms
{
	/// <summary>
	/// Button control.
	/// It is created and added to a dialog by <see cref="IDialog.AddButton"/>.
	/// </summary>
	/// <remarks>
	/// When a button is clicked then <see cref="ButtonClicked"/> event is called and the dialog normally closes.
	/// <para>
	/// There are a few ways to keep the dialog running: set the button property <see cref="NoClose"/> or
	/// set the event property <see cref="ButtonClickedEventArgs.Ignore"/>.
	/// </para>
	/// </remarks>
	/// <seealso cref="IDialog.Cancel"/>
	public interface IButton : IControl
	{
		/// <include file='doc.xml' path='doc/ButtonClicked/*'/>
		event EventHandler<ButtonClickedEventArgs> ButtonClicked;

		/// <summary>
		/// Tells to not close the dialog on using this button.
		/// </summary>
		bool NoClose { get; set; }

		/// <include file='doc.xml' path='doc/CenterGroup/*'/>
		bool CenterGroup { get; set; }

		/// <summary>
		/// Tells to display the button without brackets.
		/// </summary>
		bool NoBrackets { get; set; }

		/// <include file='doc.xml' path='doc/NoFocus/*'/>
		bool NoFocus { get; set; }

		/// <include file='doc.xml' path='doc/ShowAmpersand/*'/>
		bool ShowAmpersand { get; set; }
	}
}
