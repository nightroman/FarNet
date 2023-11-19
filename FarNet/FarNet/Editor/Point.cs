
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Ordered pair of integer X and Y coordinates that defines a point in a two-dimensional plane.
/// </summary>
public struct Point
{
	/// <summary>
	/// Initializes a point with the same X (column) and Y (row) value.
	/// </summary>
	/// <param name="coordinate">The same X (column) and Y (row) value.</param>
	public Point(int coordinate)
		: this()
	{
		X = coordinate;
		Y = coordinate;
	}

	/// <summary>
	/// Initializes a point with the specified coordinates.
	/// </summary>
	/// <param name="column">The horizontal position of the point.</param>
	/// <param name="row">The vertical position of the point.</param>
	public Point(int column, int row)
		: this()
	{
		X = column;
		Y = row;
	}

	/// <summary>
	/// Gets or sets the X coordinate (column).
	/// </summary>
	public int X { get; set; }

	/// <summary>
	/// Gets or sets the Y coordinate (row).
	/// </summary>
	public int Y { get; set; }

	/// <include file='doc.xml' path='doc/OpEqual/*'/>
	public static bool operator ==(Point left, Point right)
	{
		return left.X == right.X && left.Y == right.Y;
	}

	/// <include file='doc.xml' path='doc/OpNotEqual/*'/>
	public static bool operator !=(Point left, Point right)
	{
		return left.X != right.X || left.Y != right.Y;
	}

	/// <inheritdoc/>
	public override readonly bool Equals(object? obj)
	{
		return obj != null && obj.GetType() == typeof(Point) && this == (Point)obj;
	}

	/// <inheritdoc/>
	public override readonly int GetHashCode()
	{
		return X | (Y << 16);
	}

	/// <summary>
	/// Returns the string "(X, Y)".
	/// </summary>
	public override readonly string ToString()
	{
		return "(" + X + ", " + Y + ")";
	}
}
