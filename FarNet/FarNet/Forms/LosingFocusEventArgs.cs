
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// <see cref="IControl.LosingFocus"/> event arguments.
/// </summary>
/// <param name="losing">Control losing focus.</param>
public sealed class LosingFocusEventArgs(IControl losing) : AnyEventArgs(losing)
{
	/// <summary>
	/// Control you want to pass focus to or leave it null to allow losing focus.
	/// </summary>
	public IControl? Focused { get; set; }
}
