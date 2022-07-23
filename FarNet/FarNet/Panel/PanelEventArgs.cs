
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Panel event arguments.
/// </summary>
public class PanelEventArgs : EventArgs
{
	/// <summary>
	/// Tells that a job is done or an action has to be ignored, it depends on the event.
	/// </summary>
	public bool Ignore { get; set; }
}
