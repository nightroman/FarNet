
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Switching between editor and viewer.
/// Used by editor <see cref="IEditor.Switching"/> and viewer <see cref="IViewer.Switching"/>.
/// </summary>
public enum Switching
{
	/// <summary>
	/// Switching is disabled if editor <see cref="IEditor.DeleteSource"/> or viewer <see cref="IViewer.DeleteSource"/> is set
	/// or there are any event handlers added to an editor or viewer.
	/// </summary>
	Auto,

	/// <summary>
	/// Switching is enabled. If you use it together with events or <c>DeleteSource</c> take into account possible side effects.
	/// </summary>
	Enabled,

	/// <summary>
	/// Switching is disabled.
	/// </summary>
	Disabled
}
