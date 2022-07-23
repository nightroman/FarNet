
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Represents the thumbnail progress bar state.
/// </summary>
public enum TaskbarProgressBarState
{
	/// <summary>
	/// No progress is displayed.
	/// </summary>
	NoProgress = 0,

	/// <summary>
	/// The progress is indeterminate (marquee).
	/// </summary>
	Indeterminate = 1,

	/// <summary>
	/// Normal progress is displayed.
	/// </summary>
	Normal = 2,

	/// <summary>
	/// An error occurred (red).
	/// </summary>
	Error = 4,

	/// <summary>
	/// The operation is paused (yellow).
	/// </summary>
	Paused = 8
}
