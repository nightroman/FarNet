
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Line or column span, for example span of selected text in an editor line.
/// </summary>
public struct Span
{
	/// <summary>
	/// Start position, included into the span.
	/// </summary>
	public int Start { get; set; }

	/// <summary>
	/// End position, excluded from the span.
	/// </summary>
	public int End { get; set; }

	/// <summary>
	/// Gets length of the span or a negative value if the span does not exist.
	/// </summary>
	public int Length { get { return End - Start; } }

	/// <include file='doc.xml' path='doc/OpEqual/*'/>
	public static bool operator ==(Span left, Span right)
	{
		return left.Start == right.Start && left.End == right.End;
	}

	/// <include file='doc.xml' path='doc/OpNotEqual/*'/>
	public static bool operator !=(Span left, Span right)
	{
		return left.Start != right.Start || left.End != right.End;
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj)
	{
		return obj != null && obj.GetType() == typeof(Span) && this == (Span)obj;
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		return Start | (End << 16);
	}

	/// <summary>
	/// Returns the string "Empty" or "{0} from {1} to {2}", Length, Start, End.
	/// </summary>
	public override string ToString()
	{
		return Length < 0 ? "Empty" : string.Format(null, "{0} from {1} to {2}", Length, Start, End);
	}
}
