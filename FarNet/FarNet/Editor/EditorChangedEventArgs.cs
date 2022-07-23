
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Arguments of editor changed event.
/// </summary>
public sealed class EditorChangedEventArgs : EventArgs
{
	/// <param name="kind">See <see cref="Kind"/></param>
	/// <param name="line">See <see cref="Line"/></param>
	public EditorChangedEventArgs(EditorChangeKind kind, int line)
	{
		Kind = kind;
		Line = line;
	}

	/// <summary>
	/// Gets the editor change kind.
	/// </summary>
	public EditorChangeKind Kind { get; }

	/// <summary>
	/// Gets the changed line index.
	/// </summary>
	public int Line { get; }
}
