using System;

namespace FarNet;

/// <summary>
/// Arguments of key events.
/// </summary>
/// <param name="key">Key data.</param>
public sealed class KeyEventArgs(KeyInfo key) : EventArgs
{
	/// <summary>
	/// Key data.
	/// </summary>
	public KeyInfo Key { get; } = key;

	/// <summary>
	/// Ignore event.
	/// </summary>
	public bool Ignore { get; set; }
}
