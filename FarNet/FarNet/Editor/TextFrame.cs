
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Information about the text frame and the caret position.
/// </summary>
public struct TextFrame
{
	/// <param name="value">The same value assigned to all properties.</param>
	public TextFrame(int value)
		: this()
	{
		CaretLine = value;
		CaretColumn = value;
		CaretScreenColumn = value;
		VisibleLine = value;
		VisibleChar = value;
	}

	/// <summary>
	/// Gets or sets the caret line index.
	/// </summary>
	public int CaretLine { get; set; }

	/// <summary>
	/// Gets or sets the caret character index.
	/// </summary>
	public int CaretColumn { get; set; }

	/// <summary>
	/// Gets or sets the caret screen column index.
	/// </summary>
	public int CaretScreenColumn { get; set; }

	/// <summary>
	/// Gets or sets the first visible line index.
	/// </summary>
	public int VisibleLine { get; set; }

	/// <summary>
	/// Gets or sets the first visible character index.
	/// </summary>
	public int VisibleChar { get; set; }

	/// <include file='doc.xml' path='doc/OpEqual/*'/>
	public static bool operator ==(TextFrame left, TextFrame right)
	{
		return
			left.CaretLine == right.CaretLine &&
			left.CaretColumn == right.CaretColumn &&
			left.CaretScreenColumn == right.CaretScreenColumn &&
			left.VisibleLine == right.VisibleLine &&
			left.VisibleChar == right.VisibleChar;
	}

	/// <include file='doc.xml' path='doc/OpNotEqual/*'/>
	public static bool operator !=(TextFrame left, TextFrame right)
	{
		return !(left == right);
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj)
	{
		return obj != null && obj.GetType() == typeof(TextFrame) && this == (TextFrame)obj;
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		return CaretLine | (CaretColumn << 16);
	}

	/// <summary>
	/// Returns "(({CaretColumn}/{CaretScreenColumn}, {CaretLine})({VisibleChar}, {VisibleLine}))".
	/// </summary>
	public override string ToString()
	{
		return $"(({CaretColumn}/{CaretScreenColumn}, {CaretLine})({VisibleChar}, {VisibleLine}))";
	}
}
