
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Any viewer operator.
/// Exposed as <see cref="IFar.AnyViewer"/>.
/// </summary>
public abstract class IAnyViewer : IViewerBase
{
	/// <summary>
	/// Opens a viewer to view some text.
	/// </summary>
	/// <param name="text">The text to view.</param>
	/// <param name="title">The viewer title.</param>
	/// <param name="mode">The open mode.</param>
	public abstract void ViewText(string text, string title, OpenMode mode);
}
