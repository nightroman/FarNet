
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// <see cref="IDropDown.DropDownOpening"/> event arguments.
/// </summary>
/// <param name="control">Control which drop down is opening.</param>
public sealed class DropDownOpeningEventArgs(IControl control) : AnyEventArgs(control)
{
}
