
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// <see cref="IDropDown.DropDownOpening"/> event arguments.
/// </summary>
public sealed class DropDownOpeningEventArgs : AnyEventArgs
{
	/// <param name="control">Control which drop down is opening.</param>
	public DropDownOpeningEventArgs(IControl control) : base(control)
	{
	}
}
