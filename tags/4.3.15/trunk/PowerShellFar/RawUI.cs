
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Management.Automation.Host;
using FarNet;
using FN = FarNet.Works;
using PS = System.Management.Automation.Host;

namespace PowerShellFar
{
	/// <summary>
	/// Implements PSHostRawUserInterface.
	/// </summary>
	class RawUI : PSHostRawUserInterface
	{
		public override string WindowTitle
		{
			get { return Far.Net.UI.WindowTitle; }
			set { Far.Net.UI.WindowTitle = value; }
		}

		public override int CursorSize
		{
			get { return Far.Net.UI.CursorSize; }
			set { Far.Net.UI.CursorSize = value; }
		}

		public override Coordinates CursorPosition
		{
			get { return ToCoordinates(Far.Net.UI.BufferCursor); }
			set { Far.Net.UI.BufferCursor = ToPoint(value); }
		}

		public override ConsoleColor BackgroundColor
		{
			get { return Far.Net.UI.BackgroundColor; }
			set { Far.Net.UI.BackgroundColor = value; }
		}

		public override ConsoleColor ForegroundColor
		{
			get { return Far.Net.UI.ForegroundColor; }
			set { Far.Net.UI.ForegroundColor = value; }
		}

		public override Size BufferSize
		{
			get { return ToSize(Far.Net.UI.BufferSize); }
			set { Far.Net.UI.BufferSize = ToPoint(value); }
		}

		public override bool KeyAvailable
		{
			get { return Far.Net.UI.KeyAvailable; }
		}

		public override void FlushInputBuffer()
		{
			Far.Net.UI.FlushInputBuffer();
		}

		public override PS.KeyInfo ReadKey(PS.ReadKeyOptions options)
		{
			FarNet.KeyInfo k = Far.Net.UI.ReadKey((FN.ReadKeyOptions)options);
			return new PS.KeyInfo(k.VirtualKeyCode, k.Character, (PS.ControlKeyStates)k.ControlKeyState, k.KeyDown);
		}

		public override Size MaxPhysicalWindowSize
		{
			get { return ToSize(Far.Net.UI.MaxPhysicalWindowSize); }
		}

		public override Size MaxWindowSize
		{
			get { return ToSize(Far.Net.UI.MaxWindowSize); }
		}

		public override Coordinates WindowPosition
		{
			get { return ToCoordinates(Far.Net.UI.WindowPoint); }
			set { Far.Net.UI.WindowPoint = ToPoint(value); }
		}

		public override Size WindowSize
		{
			get { return ToSize(Far.Net.UI.WindowSize); }
			set { Far.Net.UI.WindowSize = ToPoint(value); }
		}

		public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, PS.BufferCell fill)
		{
			Far.Net.UI.ScrollBufferContents(ToPlace(source), ToPoint(destination), ToPlace(clip), ToBufferCell(fill));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
		public override PS.BufferCell[,] GetBufferContents(Rectangle rectangle)
		{
			FN.BufferCell[,] r1 = Far.Net.UI.GetBufferContents(ToPlace(rectangle));
			PS.BufferCell[,] r2 = new PS.BufferCell[r1.GetLength(0), r1.GetLength(1)];
			for (int i = 0; i < r1.GetLength(0); ++i)
				for (int j = 0; j < r1.GetLength(1); ++j)
					r2[i, j] = ToBufferCell(r1[i, j]);
			return r2;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
		public override void SetBufferContents(Coordinates origin, PS.BufferCell[,] contents)
		{
			if (contents == null)
				throw new ArgumentNullException("contents");

			FN.BufferCell[,] r = new FN.BufferCell[contents.GetLength(0), contents.GetLength(1)];
			for (int i = 0; i < contents.GetLength(0); ++i)
				for (int j = 0; j < contents.GetLength(1); ++j)
					r[i, j] = ToBufferCell(contents[i, j]);
			Far.Net.UI.SetBufferContents(ToPoint(origin), r);
		}

		public override void SetBufferContents(Rectangle rectangle, PS.BufferCell fill)
		{
			Far.Net.UI.SetBufferContents(ToPlace(rectangle), ToBufferCell(fill));
		}

		#region Converters

		static Coordinates ToCoordinates(Point point)
		{
			return new Coordinates(point.X, point.Y);
		}

		static FN.BufferCell ToBufferCell(PS.BufferCell cell)
		{
			return new FN.BufferCell(cell.Character, cell.ForegroundColor, cell.BackgroundColor, (FN.BufferCellType)cell.BufferCellType);
		}

		static PS.BufferCell ToBufferCell(FN.BufferCell cell)
		{
			return new PS.BufferCell(cell.Character, cell.ForegroundColor, cell.BackgroundColor, (PS.BufferCellType)cell.BufferCellType);
		}

		static Place ToPlace(Rectangle rectangle)
		{
			return new Place(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
		}

		static Point ToPoint(Coordinates coordinates)
		{
			return new Point(coordinates.X, coordinates.Y);
		}

		static Point ToPoint(Size size)
		{
			return new Point(size.Width, size.Height);
		}

		static Size ToSize(Point point)
		{
			return new Size(point.X, point.Y);
		}

		#endregion
	}
}
