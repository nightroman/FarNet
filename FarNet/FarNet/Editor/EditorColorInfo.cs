
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Editor line color info.
/// </summary>
/// <param name="line">See <see cref="EditorColor.Line"/></param>
/// <param name="start">See <see cref="EditorColor.Start"/></param>
/// <param name="end">See <see cref="EditorColor.End"/></param>
/// <param name="foreground">See <see cref="EditorColor.Foreground"/></param>
/// <param name="background">See <see cref="EditorColor.Background"/></param>
/// <param name="owner">See <see cref="Owner"/></param>
/// <param name="priority">See <see cref="Priority"/></param>
public class EditorColorInfo(
	int line,
	int start,
	int end,
	ConsoleColor foreground,
	ConsoleColor background,
	Guid owner,
	int priority)
	:
	EditorColor(line, start, end, foreground, background)
{
	/// <summary>
	/// Color owner ID.
	/// </summary>
	public Guid Owner { get; } = owner;

	/// <summary>
	/// Color priority.
	/// </summary>
	public int Priority { get; } = priority;

	/// <summary>
	/// Returns "{Priority} {Owner} {Line} ({Start}, {End}) {Foreground}/{Background}".
	/// </summary>
	public override string ToString()
	{
		return $"{Priority} {Owner} {Line} ({Start}, {End}) {Foreground}/{Background}";
	}
}
