/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Diagnostics;

namespace FarNet
{
	/// <summary>
	/// Ordered pair of integer X and Y coordinates that defines a point in a two-dimensional plane.
	/// </summary>
	public struct Point
	{
		int x;
		int y;
		/// <summary>
		/// Initializes a point with the same x and y.
		/// </summary>
		public Point(int coordinate)
		{
			x = y = coordinate;
		}
		/// <summary>
		/// Initializes a new instance with the specified coordinates.
		/// </summary>
		/// <param name="column">The horizontal position of the point.</param>
		/// <param name="row">The vertical position of the point.</param>
		public Point(int column, int row)
		{
			this.x = column;
			this.y = row;
		}
		/// <summary>
		/// Gets or sets the x-coordinate.
		/// </summary>
		public int X
		{
			get { return x; }
			set { x = value; }
		}
		/// <summary>
		/// Gets or sets the y-coordinate.
		/// </summary>
		public int Y
		{
			get { return y; }
			set { y = value; }
		}
		///
		public static bool operator ==(Point left, Point right)
		{
			return left.x == right.x && left.y == right.y;
		}
		///
		public static bool operator !=(Point left, Point right)
		{
			return left.x != right.x || left.y != right.y;
		}
		///
		public override bool Equals(Object obj)
		{
			return obj is Point && this == (Point)obj;
		}
		///
		public override int GetHashCode()
		{
			return x | (y << 16);
		}
		///
		public override string ToString()
		{
			return "(" + x + ", " + y + ")";
		}
	}

	/// <summary>
	/// Ordered pair of two points defining a rectangle or a stream region.
	/// </summary>
	public struct Place
	{
		Point _first;
		Point _last;
		/// <param name="value">Value used for all coordinates.</param>
		public Place(int value)
		{
			_first = new Point(value);
			_last = new Point(value);
		}
		/// <param name="first">First point.</param>
		/// <param name="last">Last Point.</param>
		public Place(Point first, Point last)
		{
			_first = first;
			_last = last;
		}
		/// <include file='doc.xml' path='docs/pp[@name="LTRB"]/*'/>
		public Place(int left, int top, int right, int bottom)
		{
			_first = new Point(left, top);
			_last = new Point(right, bottom);
		}
		/// <summary>
		/// First point.
		/// </summary>
		public Point First
		{
			get { return _first; }
			set { _first = value; }
		}
		/// <summary>
		/// Last point.
		/// </summary>
		public Point Last
		{
			get { return _last; }
			set { _last = value; }
		}
		/// <summary>
		/// Size as (<c>Width</c>, <c>Height</c>) pair.
		/// </summary>
		public Point Size
		{
			get { return new Point(Width, Height); }
		}
		/// <summary>
		/// Top line.
		/// </summary>
		public int Top
		{
			get { return _first.Y; }
			set { _first.Y = value; }
		}
		/// <summary>
		/// Left position.
		/// </summary>
		public int Left
		{
			get { return _first.X; }
			set { _first.X = value; }
		}
		/// <summary>
		/// Bottom line.
		/// </summary>
		public int Bottom
		{
			get { return _last.Y; }
			set { _last.Y = value; }
		}
		/// <summary>
		/// Right position.
		/// </summary>
		public int Right
		{
			get { return _last.X; }
			set { _last.X = value; }
		}
		/// <summary>
		/// Horizontal size.
		/// </summary>
		public int Width
		{
			get { return this.Right - this.Left + 1; }
			set { this.Right = (this.Left + value - 1); }
		}
		/// <summary>
		/// Vertical size.
		/// </summary>
		public int Height
		{
			get { return this.Bottom - this.Top + 1; }
			set { this.Bottom = (this.Top + value - 1); }
		}
		///
		public static bool operator ==(Place left, Place right)
		{
			return left.First == right.First && left.Last == right.Last;
		}
		///
		public static bool operator !=(Place left, Place right)
		{
			return left.First != right.First || left.Last != right.Last;
		}
		///
		public override bool Equals(Object obj)
		{
			return obj is Place && this == (Place)obj;
		}
		///
		public override int GetHashCode()
		{
			return First.GetHashCode() ^ Last.GetHashCode();
		}
		///
		public override string ToString()
		{
			return "(" + First + ", " + Last + ")";
		}
		/// <summary>
		/// Returns true if the rectangular contains the point.
		/// </summary>
		public bool RectContains(Point point)
		{
			return point.X >= First.X && point.Y >= First.Y && point.X <= Last.X && point.Y <= Last.Y;
		}
	}

	/// <summary>
	/// Kinds of regions of text, screen, and etc.
	/// </summary>
	/// <remarks>
	/// A standard region is completely defined if its kind and coordinates (<see cref="Place"/>) are known.
	/// </remarks>
	public enum RegionKind
	{
		///
		None = 0,
		/// <summary>
		/// Stream region. Example: classic editor text selection.
		/// </summary>
		Stream = 1,
		/// <summary>
		/// Rectangle region. Example: column block editor text selection.
		/// </summary>
		Rect = 2,
	}
}
