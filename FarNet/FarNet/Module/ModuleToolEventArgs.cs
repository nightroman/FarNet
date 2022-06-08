
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Arguments of a module tool event.
/// </summary>
/// <remarks>
/// This event is called from plugin, disk or configuration menus.
/// </remarks>
public sealed class ModuleToolEventArgs : EventArgs
{
	/// <summary>
	/// Where it is called from.
	/// </summary>
	public ModuleToolOptions From { get; set; }
	/// <summary>
	/// Tells to ignore results, for example when a configuration dialog is canceled.
	/// </summary>
	public bool Ignore { get; set; }
	/// <summary>
	/// Gets true if the event is called from the left disk menu.
	/// </summary>
	public bool IsLeft { get; set; }
}
