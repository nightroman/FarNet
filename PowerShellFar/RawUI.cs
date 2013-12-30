
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
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
			get { return Far.Api.UI.WindowTitle; }
			set { Far.Api.UI.WindowTitle = value; }
		}

		public override int CursorSize
		{
			get { return Far.Api.UI.CursorSize; }
			set { Far.Api.UI.CursorSize = value; }
		}

		public override Coordinates CursorPosition
		{
			get { return ToCoordinates(Far.Api.UI.BufferCursor); }
			set { Far.Api.UI.BufferCursor = ToPoint(value); }
		}

		public override ConsoleColor BackgroundColor
		{
			get { return Far.Api.UI.BackgroundColor; }
			set { Far.Api.UI.BackgroundColor = value; }
		}

		public override ConsoleColor ForegroundColor
		{
			get { return Far.Api.UI.ForegroundColor; }
			set { Far.Api.UI.ForegroundColor = value; }
		}

		public override Size BufferSize
		{
			get { return ToSize(Far.Api.UI.BufferSize); }
			set { Far.Api.UI.BufferSize = ToPoint(value); }
		}

		public override bool KeyAvailable
		{
			get { return Far.Api.UI.KeyAvailable; }
		}

		public override void FlushInputBuffer()
		{
			Far.Api.UI.FlushInputBuffer();
		}

		public override PS.KeyInfo ReadKey(PS.ReadKeyOptions options)
		{
			FarNet.KeyInfo k = Far.Api.UI.ReadKey((FarNet.ReadKeyOptions)options);
			return new PS.KeyInfo((int)k.VirtualKeyCode, k.Character, (PS.ControlKeyStates)k.ControlKeyState, k.KeyDown);
		}

		public override Size MaxPhysicalWindowSize
		{
			get { return ToSize(Far.Api.UI.MaxPhysicalWindowSize); }
		}

		public override Size MaxWindowSize
		{
			get { return ToSize(Far.Api.UI.MaxWindowSize); }
		}

		public override Coordinates WindowPosition
		{
			get { return ToCoordinates(Far.Api.UI.WindowPoint); }
			set { Far.Api.UI.WindowPoint = ToPoint(value); }
		}

		public override Size WindowSize
		{
			get { return ToSize(Far.Api.UI.WindowSize); }
			set { Far.Api.UI.WindowSize = ToPoint(value); }
		}

		public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, PS.BufferCell fill)
		{
			Far.Api.UI.ScrollBufferContents(ToPlace(source), ToPoint(destination), ToPlace(clip), ToBufferCell(fill));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
		public override PS.BufferCell[,] GetBufferContents(Rectangle rectangle)
		{
			FN.BufferCell[,] r1 = Far.Api.UI.GetBufferContents(ToPlace(rectangle));
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
			Far.Api.UI.SetBufferContents(ToPoint(origin), r);
		}

		public override void SetBufferContents(Rectangle rectangle, PS.BufferCell fill)
		{
			Far.Api.UI.SetBufferContents(ToPlace(rectangle), ToBufferCell(fill));
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
