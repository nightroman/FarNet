
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

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

	/// <include file='doc.xml' path='doc/LTRB/*'/>
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
		readonly get => _first;
		set => _first = value;
	}

	/// <summary>
	/// Last point.
	/// </summary>
	public Point Last
	{
		readonly get => _last;
		set => _last = value;
	}

	/// <summary>
	/// Size as (<c>Width</c>, <c>Height</c>) pair.
	/// </summary>
	public readonly Point Size => new(Width, Height);

	/// <summary>
	/// Top line.
	/// </summary>
	public int Top
	{
		readonly get => _first.Y;
		set => _first.Y = value;
	}

	/// <summary>
	/// Left position.
	/// </summary>
	public int Left
	{
		readonly get => _first.X;
		set => _first.X = value;
	}

	/// <summary>
	/// Bottom line.
	/// </summary>
	public int Bottom
	{
		readonly get => _last.Y;
		set => _last.Y = value;
	}

	/// <summary>
	/// Right position.
	/// </summary>
	public int Right
	{
		readonly get => _last.X;
		set => _last.X = value;
	}

	/// <summary>
	/// Horizontal size.
	/// </summary>
	public int Width
	{
		readonly get => Right - Left + 1;
		set => Right = Left + value - 1;
	}

	/// <summary>
	/// Vertical size.
	/// </summary>
	public int Height
	{
		readonly get => Bottom - Top + 1;
		set => Bottom = Top + value - 1;
	}

	/// <include file='doc.xml' path='doc/OpEqual/*'/>
	public static bool operator ==(Place left, Place right)
	{
		return left.First == right.First && left.Last == right.Last;
	}

	/// <include file='doc.xml' path='doc/OpNotEqual/*'/>
	public static bool operator !=(Place left, Place right)
	{
		return left.First != right.First || left.Last != right.Last;
	}

	/// <inheritdoc/>
	public override readonly bool Equals(object? obj)
	{
		return obj != null && obj.GetType() == typeof(Place) && this == (Place)obj;
	}

	/// <inheritdoc/>
	public override readonly int GetHashCode()
	{
		return First.GetHashCode() ^ Last.GetHashCode();
	}

	/// <summary>
	/// Gets "({First}, {Last})".
	/// </summary>
	/// <returns>"({First}, {Last})"</returns>
	public override readonly string ToString()
	{
		return $"({First}, {Last})";
	}

	/// <summary>
	/// Gets true if the rectangular contains the point.
	/// </summary>
	/// <param name="point">The point to test.</param>
	/// <returns>True if the rectangular contains the point.</returns>
	public readonly bool RectContains(Point point)
	{
		return point.X >= First.X && point.Y >= First.Y && point.X <= Last.X && point.Y <= Last.Y;
	}
}
