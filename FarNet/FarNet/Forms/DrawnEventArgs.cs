
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// <see cref="IControl.Drawn"/> event arguments.
/// </summary>
public sealed class DrawnEventArgs : AnyEventArgs
{
	/// <param name="control">Control that is drawn.</param>
	public DrawnEventArgs(IControl control) : base(control)
	{
	}
}
