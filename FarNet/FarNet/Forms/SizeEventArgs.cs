
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// Size event arguments, e.g. of <see cref="IDialog.ConsoleSizeChanged"/> event.
/// </summary>
/// <param name="control">It is null.</param>
/// <param name="size">The size.</param>
public sealed class SizeEventArgs(IControl control, Point size) : AnyEventArgs(control)
{
	/// <summary>
	/// The size.
	/// </summary>
	public Point Size { get; set; } = size;
}
