
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet.Works;

/// <summary>
/// INTERNAL
/// </summary>
public struct BufferCell
{

	///
	public BufferCell(char character, ConsoleColor foreground, ConsoleColor background, BufferCellType bufferCellType)
	{
		Character = character;
		ForegroundColor = foreground;
		BackgroundColor = background;
		BufferCellType = bufferCellType;
	}

	///
	public char Character { get; set; }

	///
	public ConsoleColor ForegroundColor { get; set; }

	///
	public ConsoleColor BackgroundColor { get; set; }

	///
	public BufferCellType BufferCellType { get; set; }

	///
	public static bool operator ==(BufferCell first, BufferCell second) => (first.Character == second.Character) && (first.BackgroundColor == second.BackgroundColor) && (first.ForegroundColor == second.ForegroundColor) && (first.BufferCellType == second.BufferCellType);

	///
	public static bool operator !=(BufferCell first, BufferCell second) => !(first == second);

	/// <inheritdoc/>
	public override bool Equals(object obj)
	{
		bool flag = false;
		if (obj is BufferCell cell)
			flag = this == cell;
		return flag;
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		uint num = ((uint)(ForegroundColor ^ BackgroundColor)) << 0x10;
		num |= Character;
		return num.GetHashCode();
	}

	///
	public override string ToString()
	{
		return string.Format(null, "'{0}' {1} {2} {3}", new object[] { Character, ForegroundColor, BackgroundColor, BufferCellType });
	}
}
