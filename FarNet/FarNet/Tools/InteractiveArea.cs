
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Tools;

/// <summary>
/// Current interactive area info.
/// </summary>
public class InteractiveArea
{
	/// <summary>
	/// The first not empty line.
	/// </summary>
	public int FirstLineIndex { get; set; }

	/// <summary>
	/// The last not empty line.
	/// </summary>
	public int LastLineIndex { get; set; }

	/// <summary>
	/// The caret point.
	/// </summary>
	public Point Caret { get; set; }

	/// <summary>
	/// Tells if the area is active.
	/// </summary>
	public bool Active { get; set; }
}
