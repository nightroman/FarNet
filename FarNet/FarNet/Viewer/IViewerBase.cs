
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Common viewer events.
/// </summary>
public abstract class IViewerBase
{
	/// <summary>
	/// Called when the viewer is closed.
	/// </summary>
	/// <remarks>
	/// This event is called once for the viewer instance, even if there were several files opened in it,
	/// e.g. on [Add], [Subtract] keys the <see cref="Opened"/> is called every time.
	/// <para>
	/// Don't operate on the viewer, it has really gone.
	/// </para>
	/// </remarks>
	public abstract event EventHandler Closed; // [_100117_101226]

	/// <summary>
	/// Called when a file is opened in the viewer.
	/// </summary>
	/// <remarks>
	/// This event can be called more than once for the same viewer instance,
	/// e.g. on [Add], [Subtract] keys.
	/// </remarks>
	public abstract event EventHandler Opened;

	/// <summary>
	/// Called when the viewer window has got focus.
	/// </summary>
	public abstract event EventHandler GotFocus;

	/// <summary>
	/// Called when the viewer window is losing focus.
	/// </summary>
	public abstract event EventHandler LosingFocus;
}
