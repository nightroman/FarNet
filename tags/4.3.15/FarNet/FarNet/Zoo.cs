/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;

namespace FarNet.Works
{
	/// <summary>
	/// For internal use.
	/// </summary>
	public enum BufferCellType
	{
		///
		Complete = 0,
		///
		Leading = 1,
		///
		Trailing = 2,
	}

	/// <summary>
	/// For internal use.
	/// </summary>
	[Flags]
	public enum ReadKeyOptions
	{
		///
		AllowCtrlC = 1,
		///
		NoEcho = 2,
		///
		IncludeKeyDown = 4,
		///
		IncludeKeyUp = 8,
	}

	/// <summary>
	/// For internal use.
	/// </summary>
	public struct BufferCell
	{
		private char character;
		private ConsoleColor foregroundColor;
		private ConsoleColor backgroundColor;
		private BufferCellType bufferCellType;

		///
		public BufferCell(char character, ConsoleColor foreground, ConsoleColor background, BufferCellType bufferCellType)
		{
			this.character = character;
			this.foregroundColor = foreground;
			this.backgroundColor = background;
			this.bufferCellType = bufferCellType;
		}

		///
		public static bool operator==(BufferCell first, BufferCell second)
		{
			return ((((first.Character == second.Character) && (first.BackgroundColor == second.BackgroundColor)) && (first.ForegroundColor == second.ForegroundColor)) && (first.BufferCellType == second.BufferCellType));
		}

		///
		public static bool operator!=(BufferCell first, BufferCell second)
		{
			return !(first == second);
		}

		///
		public char Character
		{
			get
			{
				return this.character;
			}
			set
			{
				this.character = value;
			}
		}

		///
		public ConsoleColor ForegroundColor
		{
			get
			{
				return this.foregroundColor;
			}
			set
			{
				this.foregroundColor = value;
			}
		}

		///
		public ConsoleColor BackgroundColor
		{
			get
			{
				return this.backgroundColor;
			}
			set
			{
				this.backgroundColor = value;
			}
		}

		///
		public BufferCellType BufferCellType
		{
			get
			{
				return this.bufferCellType;
			}
			set
			{
				this.bufferCellType = value;
			}
		}

		///
		public override bool Equals(object obj)
		{
			bool flag = false;
			if (obj is BufferCell)
				flag = this == ((BufferCell)obj);
			return flag;
		}

		///
		public override int GetHashCode()
		{
			uint num = ((uint)(this.ForegroundColor ^ this.BackgroundColor)) << 0x10;
			num |= this.Character;
			return num.GetHashCode();
		}

		///
		public override string ToString()
		{
			return Invariant.Format("'{0}' {1} {2} {3}", new object[] { this.Character, this.ForegroundColor, this.BackgroundColor, this.BufferCellType });
		}
	}

	/// <summary>
	/// For internal use.
	/// </summary>
	public abstract class IZoo
	{
		///
		public abstract void FlushInputBuffer();
		///
		public abstract KeyInfo ReadKey(ReadKeyOptions options);
		///
		public abstract BufferCell[,] GetBufferContents(Place rectangle);
		///
		public abstract string ConsoleTitle { get; }
		///
		public abstract void ScrollBufferContents(Place source, Point destination, Place clip, BufferCell fill);
		///
		public abstract void SetBufferContents(Point origin, BufferCell[,] contents);
		///
		public abstract void SetBufferContents(Place rectangle, BufferCell fill);
		///
		public abstract void Break();
	}
}
