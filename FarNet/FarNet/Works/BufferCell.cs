
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet.Works;
#pragma warning disable 1591

public struct BufferCell(char character, ConsoleColor foreground, ConsoleColor background, BufferCellType bufferCellType)
{
	public char Character { get; set; } = character;

	public ConsoleColor ForegroundColor { get; set; } = foreground;

	public ConsoleColor BackgroundColor { get; set; } = background;

	public BufferCellType BufferCellType { get; set; } = bufferCellType;

	public static bool operator ==(BufferCell first, BufferCell second) => (first.Character == second.Character) && (first.BackgroundColor == second.BackgroundColor) && (first.ForegroundColor == second.ForegroundColor) && (first.BufferCellType == second.BufferCellType);

	public static bool operator !=(BufferCell first, BufferCell second) => !(first == second);

	public override readonly bool Equals(object? obj)
	{
		bool flag = false;
		if (obj is BufferCell cell)
			flag = this == cell;
		return flag;
	}

	public override readonly int GetHashCode()
	{
		uint num = ((uint)(ForegroundColor ^ BackgroundColor)) << 0x10;
		num |= Character;
		return num.GetHashCode();
	}

	public override readonly string ToString()
	{
		return $"'{Character}' {ForegroundColor} {BackgroundColor} {BufferCellType}";
	}
}
