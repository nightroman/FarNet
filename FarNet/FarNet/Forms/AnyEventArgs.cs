
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet.Forms;

/// <summary>
/// Base class of dialog and control event arguments.
/// </summary>
/// <param name="control">The control related to this event.</param>
public class AnyEventArgs(IControl? control) : EventArgs
{
	/// <summary>
	/// Gets the event control. See the constructor for details.
	/// </summary>
	public IControl? Control { get; } = control;
}
