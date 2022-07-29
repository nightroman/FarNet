
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// <c>KeyPressed</c> event arguments for <see cref="IDialog"/> and <see cref="IControl"/>.
/// </summary>
public sealed class KeyPressedEventArgs : AnyEventArgs
{
	/// <param name="control">Current control.</param>
	/// <param name="key">The key.</param>
	public KeyPressedEventArgs(IControl control, KeyInfo key) : base(control)
	{
		Key = key;
	}

	/// <summary>
	/// The key.
	/// </summary>
	public KeyInfo Key { get; }

	/// <summary>
	/// Ignore further processing.
	/// </summary>
	public bool Ignore { get; set; }
}
