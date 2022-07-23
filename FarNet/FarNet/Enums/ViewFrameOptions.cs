
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Options for <see cref="IViewer.SetFrame"/>.
/// </summary>
[Flags]
public enum ViewFrameOptions
{
	///
	None,

	/// <summary>
	/// Don't redraw.
	/// </summary>
	NoRedraw = 1,

	/// <summary>
	/// Offset is defined in percents.
	/// </summary>
	Percent = 2,

	/// <summary>
	/// Offset is relative to the current (and can be negative).
	/// </summary>
	Relative = 4,
}
