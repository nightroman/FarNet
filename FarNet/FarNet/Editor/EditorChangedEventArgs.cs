
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Arguments of editor changed event.
/// </summary>
/// <param name="kind">See <see cref="Kind"/></param>
/// <param name="line">See <see cref="Line"/></param>
public sealed class EditorChangedEventArgs(EditorChangeKind kind, int line) : EventArgs
{
	/// <summary>
	/// Gets the editor change kind.
	/// </summary>
	public EditorChangeKind Kind { get; } = kind;

	/// <summary>
	/// Gets the changed line index.
	/// </summary>
	public int Line { get; } = line;
}
