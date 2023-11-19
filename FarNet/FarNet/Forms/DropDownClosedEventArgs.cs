
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// <see cref="IDropDown.DropDownClosed"/> event arguments.
/// </summary>
/// <param name="control">Control which drop down is closed.</param>
public sealed class DropDownClosedEventArgs(IControl control) : AnyEventArgs(control)
{
}
