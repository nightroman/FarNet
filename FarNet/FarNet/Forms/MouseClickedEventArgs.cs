
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// <c>MouseClicked</c> event arguments for <see cref="IDialog"/> and <see cref="IControl"/>.
/// </summary>
/// <param name="control">Current control.</param>
/// <param name="mouse">Mouse info.</param>
public sealed class MouseClickedEventArgs(IControl control, MouseInfo mouse) : AnyEventArgs(control)
{
	/// <summary>
	/// Mouse info.
	/// </summary>
	public MouseInfo Mouse { get; set; } = mouse;

	/// <summary>
	/// Ignore further processing.
	/// </summary>
	public bool Ignore { get; set; }
}
