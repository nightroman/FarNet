
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// Size event arguments, e.g. of <see cref="IDialog.ConsoleSizeChanged"/> event.
/// </summary>
public sealed class SizeEventArgs : AnyEventArgs
{
	/// <param name="control">It is null.</param>
	/// <param name="size">The size.</param>
	public SizeEventArgs(IControl control, Point size) : base(control)
	{
		Size = size;
	}

	/// <summary>
	/// The size.
	/// </summary>
	public Point Size { get; set; }
}
