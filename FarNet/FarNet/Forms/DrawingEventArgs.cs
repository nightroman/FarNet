
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// <see cref="IControl.Drawing"/> event arguments.
/// </summary>
/// <param name="control">Control that is about to be drawn.</param>
public sealed class DrawingEventArgs(IControl control) : AnyEventArgs(control)
{
	/// <summary>
	/// Ingore and don't draw the control.
	/// </summary>
	public bool Ignore { get; set; }
}
