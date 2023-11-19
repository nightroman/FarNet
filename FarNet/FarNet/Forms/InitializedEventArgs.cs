
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// <see cref="IDialog.Initialized"/> event arguments.
/// </summary>
/// <param name="focused">Control that will initially receive focus.</param>
public sealed class InitializedEventArgs(IControl focused) : AnyEventArgs(focused)
{
	/// <summary>
	/// Ingore changes.
	/// </summary>
	public bool Ignore { get; set; }
}
