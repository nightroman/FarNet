
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// <c>KeyPressed</c> event arguments for <see cref="IDialog"/> and <see cref="IControl"/>.
/// </summary>
/// <param name="control">Current control.</param>
/// <param name="key">The key.</param>
public sealed class KeyPressedEventArgs(IControl control, KeyInfo key) : AnyEventArgs(control)
{
	/// <summary>
	/// The key.
	/// </summary>
	public KeyInfo Key { get; } = key;

	/// <summary>
	/// Ignore further processing.
	/// </summary>
	public bool Ignore { get; set; }
}
