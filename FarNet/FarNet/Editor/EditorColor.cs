
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Editor line color span.
/// </summary>
/// <param name="line">See <see cref="Line"/></param>
/// <param name="start">See <see cref="Start"/></param>
/// <param name="end">See <see cref="End"/></param>
/// <param name="foreground">See <see cref="Foreground"/></param>
/// <param name="background">See <see cref="Background"/></param>
public class EditorColor(
	int line,
	int start,
	int end,
	ConsoleColor foreground,
	ConsoleColor background)
{
	/// <summary>
	/// Line index.
	/// </summary>
	public int Line { get; } = line;

	/// <summary>
	/// Start position.
	/// </summary>
	public int Start { get; } = start;

	/// <summary>
	/// End position, not included into the span, <c>End - Start</c> is the span length.
	/// </summary>
	public int End { get; } = end;

	/// <summary>
	/// Foreground color. Black on black is the special case.
	/// </summary>
	public ConsoleColor Foreground { get; } = foreground;

	/// <summary>
	/// Background color. Black on black is the special case.
	/// </summary>
	public ConsoleColor Background { get; } = background;

	/// <summary>
	/// Returns "({Start}, {End}) {Foreground}/{Background}".
	/// </summary>
	public override string ToString()
	{
		return $"({Start}, {End}) {Foreground}/{Background}";
	}
}
