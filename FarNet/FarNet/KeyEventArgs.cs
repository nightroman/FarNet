
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Arguments of key events.
/// </summary>
public sealed class KeyEventArgs : EventArgs
{
	/// <param name="key">Key data.</param>
	public KeyEventArgs(KeyInfo key)
	{
		Key = key;
	}

	/// <summary>
	/// Key data.
	/// </summary>
	public KeyInfo Key { get; }

	/// <summary>
	/// Ignore event.
	/// </summary>
	public bool Ignore { get; set; }
}
