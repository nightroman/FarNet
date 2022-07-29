
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// <c>MouseClicked</c> event arguments for <see cref="IDialog"/> and <see cref="IControl"/>.
/// </summary>
public sealed class MouseClickedEventArgs : AnyEventArgs
{
	/// <param name="control">Current control.</param>
	/// <param name="mouse">Mouse info.</param>
	public MouseClickedEventArgs(IControl control, MouseInfo mouse) : base(control)
	{
		Mouse = mouse;
	}

	/// <summary>
	/// Mouse info.
	/// </summary>
	public MouseInfo Mouse { get; set; }

	/// <summary>
	/// Ignore further processing.
	/// </summary>
	public bool Ignore { get; set; }
}
