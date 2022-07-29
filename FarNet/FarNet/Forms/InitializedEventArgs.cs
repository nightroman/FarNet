
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// <see cref="IDialog.Initialized"/> event arguments.
/// </summary>
public sealed class InitializedEventArgs : AnyEventArgs
{
	/// <param name="focused">Control that will initially receive focus.</param>
	public InitializedEventArgs(IControl focused) : base(focused)
	{
	}

	/// <summary>
	/// Ingore changes.
	/// </summary>
	public bool Ignore { get; set; }
}
