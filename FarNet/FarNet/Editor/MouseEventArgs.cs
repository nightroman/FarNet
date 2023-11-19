
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Arguments of mouse events.
/// </summary>
/// <param name="mouse">Mouse data.</param>
public sealed class MouseEventArgs(MouseInfo mouse) : EventArgs
{
	/// <summary>
	/// Mouse data.
	/// </summary>
	public MouseInfo Mouse { get; } = mouse;

	/// <summary>
	/// Ignore event.
	/// </summary>
	public bool Ignore { get; set; }
}
