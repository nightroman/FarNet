
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Filter pattern options.
/// </summary>
[Flags]
public enum PatternOptions
{
	/// <summary>
	/// No filter.
	/// </summary>
	None,

	/// <summary>
	/// Prefix pattern.
	/// </summary>
	Prefix = 2,

	/// <summary>
	/// Substring pattern.
	/// </summary>
	Substring = 4,

	/// <summary>
	/// Used with <see cref="Prefix"/> or <see cref="Substring"/>
	/// to treat all filter symbols literally, including '*'.
	/// </summary>
	Literal = 8
}
