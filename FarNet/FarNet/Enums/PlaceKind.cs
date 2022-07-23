
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Kinds of screen or text places.
/// </summary>
/// <remarks>
/// A place is completely defined by its kind and coordinates (<see cref="Place"/>).
/// </remarks>
public enum PlaceKind
{
	///
	None = 0,

	/// <summary>
	/// Continuous stream place. Example: classic editor selection.
	/// </summary>
	Stream = 1,

	/// <summary>
	/// Rectangular place, block of columns. Example: column editor selection.
	/// </summary>
	Column = 2,
}
