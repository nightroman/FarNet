
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet.Forms;

/// <summary>
/// Base class of dialog and control event arguments.
/// </summary>
public class AnyEventArgs : EventArgs
{
	/// <param name="control">Control involved into this event or null.</param>
	public AnyEventArgs(IControl control)
	{
		Control = control;
	}

	/// <summary>
	/// Event's control or null. See the constructor for details.
	/// </summary>
	public IControl Control { get; }
}
