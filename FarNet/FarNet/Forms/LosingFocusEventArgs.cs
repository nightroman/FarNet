
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// <see cref="IControl.LosingFocus"/> event arguments.
/// </summary>
public sealed class LosingFocusEventArgs : AnyEventArgs
{
	/// <param name="losing">Control losing focus.</param>
	public LosingFocusEventArgs(IControl losing) : base(losing)
	{
	}

	/// <summary>
	/// Control you want to pass focus to or leave it null to allow losing focus.
	/// </summary>
	public IControl Focused { get; set; }
}
