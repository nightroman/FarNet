
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet.Forms;

/// <summary>
/// An editable dialog item.
/// </summary>
public interface IEditable
{
	/// <summary>
	/// Called when the text has changed (say, on typing).
	/// </summary>
	event EventHandler<TextChangedEventArgs> TextChanged;

	/// <summary>
	/// Gets the editor line operator.
	/// </summary>
	ILine Line { get; }

	/// <summary>
	/// Tells to disable text changes for a user.
	/// </summary>
	bool ReadOnly { get; set; }

	/// <summary>
	/// Tells to select the text when the control gets focus.
	/// </summary>
	bool SelectOnEntry { get; set; }

	/// <summary>
	/// Gets or sets the touched state.
	/// </summary>
	bool IsTouched { get; set; }

	/// <include file='doc.xml' path='doc/ExpandEnvironmentVariables/*'/>
	bool ExpandEnvironmentVariables { get; set; }
}
