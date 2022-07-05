
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet.Forms
{
	/// <summary>
	/// Radio button control.
	/// It is created and added to a dialog by <see cref="IDialog.AddRadioButton"/>.
	/// </summary>
	public interface IRadioButton : IControl
	{
		/// <include file='doc.xml' path='doc/ButtonClicked/*'/>
		event EventHandler<ButtonClickedEventArgs> ButtonClicked;

		/// <summary>
		/// Selected state.
		/// </summary>
		bool Selected { get; set; }

		/// <summary>
		/// Tells to use this as the first radio button item in the following group.
		/// </summary>
		bool Group { get; set; }

		/// <summary>
		/// Tells to change selection in the radio button group when focus is moved.
		/// </summary>
		/// <remarks>
		/// Radio buttons with this flag set are also drawn without parentheses around the selection mark
		/// (example: Far color selection dialog).
		/// </remarks>
		bool MoveSelect { get; set; }

		/// <include file='doc.xml' path='doc/CenterGroup/*'/>
		bool CenterGroup { get; set; }

		/// <include file='doc.xml' path='doc/NoFocus/*'/>
		bool NoFocus { get; set; }

		/// <include file='doc.xml' path='doc/ShowAmpersand/*'/>
		bool ShowAmpersand { get; set; }
	}
}
