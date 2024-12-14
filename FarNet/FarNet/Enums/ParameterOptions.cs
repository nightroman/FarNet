
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Parameter processing options.
/// </summary>
[Flags]
public enum ParameterOptions
{
	/// <summary>None</summary>
	None = 0,

	/// <summary>Tells to expand environment variables.</summary>
	ExpandVariables = 1,

	/// <summary>Tells to get absolute path using <see cref="IFar.CurrentDirectory"/>.</summary>
	GetFullPath = 2,

	/// <summary>With omitted parameters tells to use <see cref="FSContext.CursorPath"/>.</summary>
	UseCursorPath = 4,

	/// <summary>With omitted parameters tells to use <see cref="FSContext.CursorFile"/>.</summary>
	UseCursorFile = 8,

	/// <summary>With omitted parameters tells to use <see cref="FSContext.CursorDirectory"/>.</summary>
	UseCursorDirectory = 16,
}
