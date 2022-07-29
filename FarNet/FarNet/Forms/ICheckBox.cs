
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet.Forms;

/// <summary>
/// Check box control.
/// It is created and added to a dialog by <see cref="IDialog.AddCheckBox"/>.
/// </summary>
public interface ICheckBox : IControl
{
	/// <include file='doc.xml' path='doc/ButtonClicked/*'/>
	event EventHandler<ButtonClickedEventArgs> ButtonClicked;

	/// <summary>
	/// Selected state.
	/// Standard: 0: off; 1: on.
	/// ThreeState: 0: off; 1: on; 2: undefined.
	/// </summary>
	int Selected { get; set; }

	/// <summary>
	/// Tells to use three possible states: "off", "on", "undefined".
	/// </summary>
	bool ThreeState { get; set; }

	/// <include file='doc.xml' path='doc/CenterGroup/*'/>
	bool CenterGroup { get; set; }

	/// <include file='doc.xml' path='doc/NoFocus/*'/>
	bool NoFocus { get; set; }

	/// <include file='doc.xml' path='doc/ShowAmpersand/*'/>
	bool ShowAmpersand { get; set; }
}
