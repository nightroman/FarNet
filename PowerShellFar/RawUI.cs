/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Management.Automation.Host;
using FarNet;
using Host = System.Management.Automation.Host;

// _091007_034112
// Getting Console.Title throws an exception internally caught by PowerShell. Usually in MT scenarios.
// It does not make problems but it is noisy. So we use a native call with no exceptions.

namespace PowerShellFar
{
	/// <summary>
	/// Implementation of the PSHostRawUserInterface for FarHost
	/// </summary>
	class RawUI : PSHostRawUserInterface
	{
		public RawUI()
		{ }

		#region Managed

		/// <summary>
		/// Get and set the background color of text ro be written.
		/// This maps directly onto the corresponding .NET Console property.
		/// </summary>
		public override ConsoleColor BackgroundColor
		{
			get { return Console.BackgroundColor; }
			set { Console.BackgroundColor = value; }
		}

		/// <summary>
		/// Return the host buffer size adapted from on the .NET Console buffer size.
		/// </summary>
		public override Size BufferSize
		{
			get { return new Size(Console.BufferWidth, Console.BufferHeight); }
			set { Console.SetBufferSize(value.Width, value.Height); }
		}

		/// <summary>
		/// Cursor position.
		/// </summary>
		public override Coordinates CursorPosition
		{
			get { return new Coordinates(Console.CursorLeft, Console.CursorTop); }
			set { Console.SetCursorPosition(value.X, value.Y); }
		}

		/// <summary>
		/// Return the cursor size taken directly from the .NET Console cursor size.
		/// </summary>
		public override int CursorSize
		{
			get { return Console.CursorSize; }
			set { Console.CursorSize = value; }
		}

		/// <summary>
		/// Get and set the foreground color of text ro be written.
		/// This maps directly onto the corresponding .NET Console property.
		/// </summary>
		public override ConsoleColor ForegroundColor
		{
			get { return Console.ForegroundColor; }
			set { Console.ForegroundColor = value; }
		}

		/// <summary>
		/// Map directly to the corresponding .NET Console property.
		/// </summary>
		public override bool KeyAvailable
		{
			get { return Console.KeyAvailable; }
		}

		/// <summary>
		/// Return the MaxPhysicalWindowSize size adapted from the .NET Console
		/// </summary>
		public override Size MaxPhysicalWindowSize
		{
			get { return new Size(Console.LargestWindowWidth, Console.LargestWindowHeight); }
		}

		/// <summary>
		/// Return the MaxWindowSize size adapted from the .NET Console
		/// </summary>
		public override Size MaxWindowSize
		{
			get { return new Size(Console.LargestWindowWidth, Console.LargestWindowHeight); }
		}

		/// <summary>
		/// Return the window position adapted from the Console window position information.
		/// </summary>
		public override Coordinates WindowPosition
		{
			get { return new Coordinates(Console.WindowLeft, Console.WindowTop); }
			set { Console.SetWindowPosition(value.X, value.Y); }
		}

		/// <summary>
		/// Return the window size adapted from the corresponding .NET Console calls.
		/// </summary>
		public override Size WindowSize
		{
			get { return new Size(Console.WindowWidth, Console.WindowHeight); }
			set { Console.SetWindowSize(value.Width, value.Height); }
		}

		/// <summary>
		/// Console.Title property.
		/// </summary>
		// _091007_034112
		public override string WindowTitle
		{
			get { return Far.Net.Zoo.ConsoleTitle; }
			set { Console.Title = value; }
		}

		#endregion

		#region Native

		// converter
		static Place PlaceOf(Rectangle rectangle)
		{
			return new Place(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
		}

		// converter
		static Point PointOf(Coordinates coordinates)
		{
			return new Point(coordinates.X, coordinates.Y);
		}

		// converter
		static FarNet.Support.BufferCell BufferCellOf(Host.BufferCell cell)
		{
			return new FarNet.Support.BufferCell(cell.Character, cell.ForegroundColor, cell.BackgroundColor, (FarNet.Support.BufferCellType)cell.BufferCellType);
		}

		// converter
		static Host.BufferCell BufferCellOf(FarNet.Support.BufferCell cell)
		{
			return new Host.BufferCell(cell.Character, cell.ForegroundColor, cell.BackgroundColor, (Host.BufferCellType)cell.BufferCellType);
		}

		public override void FlushInputBuffer()
		{
			Far.Net.Zoo.FlushInputBuffer();
		}

		public override Host.KeyInfo ReadKey(Host.ReadKeyOptions options)
		{
			FarNet.KeyInfo k = Far.Net.Zoo.ReadKey((FarNet.Support.ReadKeyOptions)options);
			return new Host.KeyInfo(k.VirtualKeyCode, k.Character, (Host.ControlKeyStates)k.ControlKeyState, k.KeyDown);
		}

		public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, Host.BufferCell fill)
		{
			Far.Net.Zoo.ScrollBufferContents(PlaceOf(source), PointOf(destination), PlaceOf(clip), BufferCellOf(fill));
		}

		public override Host.BufferCell[,] GetBufferContents(Rectangle rectangle)
		{
			FarNet.Support.BufferCell[,] r1 = Far.Net.Zoo.GetBufferContents(PlaceOf(rectangle));
			Host.BufferCell[,] r2 = new Host.BufferCell[r1.GetLength(0), r1.GetLength(1)];
			for (int i = 0; i < r1.GetLength(0); ++i)
				for (int j = 0; j < r1.GetLength(1); ++j)
					r2[i, j] = BufferCellOf(r1[i, j]);
			return r2;
		}

		public override void SetBufferContents(Coordinates origin, Host.BufferCell[,] contents)
		{
			FarNet.Support.BufferCell[,] r = new FarNet.Support.BufferCell[contents.GetLength(0), contents.GetLength(1)];
			for (int i = 0; i < contents.GetLength(0); ++i)
				for (int j = 0; j < contents.GetLength(1); ++j)
					r[i, j] = BufferCellOf(contents[i, j]);
			Far.Net.Zoo.SetBufferContents(PointOf(origin), r);
		}

		public override void SetBufferContents(Rectangle rectangle, Host.BufferCell fill)
		{
			Far.Net.Zoo.SetBufferContents(PlaceOf(rectangle), BufferCellOf(fill));
		}

		#endregion
	}
}
