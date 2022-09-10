
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet.Forms;

/// <summary>
/// Base class of dialog and control event arguments.
/// </summary>
public class AnyEventArgs : EventArgs
{
	/// <param name="control">The control related to this event.</param>
	public AnyEventArgs(IControl? control)
	{
		Control = control;
	}

	/// <summary>
	/// Gets the event control. See the constructor for details.
	/// </summary>
	public IControl? Control { get; }
}
