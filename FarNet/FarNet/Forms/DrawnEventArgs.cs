
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// <see cref="IControl.Drawn"/> event arguments.
/// </summary>
/// <param name="control">Control that is drawn.</param>
public sealed class DrawnEventArgs(IControl control) : AnyEventArgs(control)
{
}
