
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// States of macro processing.
/// </summary>
public enum MacroState
{
	/// <summary>
	/// No processing.
	/// </summary>
	None,

	/// <summary>
	/// Executing with plugins excluded.
	/// </summary>
	Executing,

	/// <summary>
	/// Executing with plugins included.
	/// </summary>
	ExecutingCommon,

	/// <summary>
	/// Recording with plugins excluded.
	/// </summary>
	Recording,

	/// <summary>
	/// Recording with plugins included.
	/// </summary>
	RecordingCommon
}
