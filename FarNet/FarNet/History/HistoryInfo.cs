
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// History information.
/// </summary>
public sealed class HistoryInfo
{
	/// <param name="name">See <see cref="Name"/></param>
	/// <param name="time">See <see cref="Time"/></param>
	/// <param name="isLocked">See <see cref="IsLocked"/></param>
	public HistoryInfo(string name, DateTime time, bool isLocked)
	{
		Name = name;
		Time = time;
		IsLocked = isLocked;
	}

	/// <summary>
	/// History information, text.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Last time.
	/// </summary>
	public DateTime Time { get; }

	/// <summary>
	/// Locked state.
	/// </summary>
	public bool IsLocked { get; }
}
