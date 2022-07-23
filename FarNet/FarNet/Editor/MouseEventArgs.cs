
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Arguments of mouse events.
/// </summary>
public sealed class MouseEventArgs : EventArgs
{
	/// <param name="mouse">Mouse data.</param>
	public MouseEventArgs(MouseInfo mouse)
	{
		Mouse = mouse;
	}

	/// <summary>
	/// Mouse data.
	/// </summary>
	public MouseInfo Mouse { get; }

	/// <summary>
	/// Ignore event.
	/// </summary>
	public bool Ignore { get; set; }
}
