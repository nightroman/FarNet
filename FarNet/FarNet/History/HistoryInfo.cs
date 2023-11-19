
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// History information.
/// </summary>
/// <param name="name">See <see cref="Name"/></param>
/// <param name="time">See <see cref="Time"/></param>
/// <param name="isLocked">See <see cref="IsLocked"/></param>
public sealed class HistoryInfo(string name, DateTime time, bool isLocked)
{
	/// <summary>
	/// History information, text.
	/// </summary>
	public string Name { get; } = name;

	/// <summary>
	/// Last time.
	/// </summary>
	public DateTime Time { get; } = time;

	/// <summary>
	/// Locked state.
	/// </summary>
	public bool IsLocked { get; } = isLocked;
}
