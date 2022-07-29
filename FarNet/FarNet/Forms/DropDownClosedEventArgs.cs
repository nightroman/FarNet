
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// <see cref="IDropDown.DropDownClosed"/> event arguments.
/// </summary>
public sealed class DropDownClosedEventArgs : AnyEventArgs
{
	/// <param name="control">Control which drop down is closed.</param>
	public DropDownClosedEventArgs(IControl control) : base(control)
	{
	}
}
