
namespace Vessel;

/// <summary>
/// Tracked item or far history item.
/// </summary>
public class Record
{
	internal const string AGED = "aged";
	internal const string USED = "used";

	/// <summary>
	/// Last used time.
	/// </summary>
	public DateTime Time { get; set; }

	/// <summary>
	/// Item kind: "used", "aged", or empty if not tracked.
	/// </summary>
	public string What { get; set; }

	/// <summary>
	/// Item path or text.
	/// </summary>
	public string Path { get; }

	internal bool IsRecent { get; set; }

	internal Record(DateTime time, string what, string path)
	{
		Time = time;
		What = what;
		Path = path;
	}

	public bool IsTracked => What.Length > 0;

	/// <summary>
	/// Compares by time.
	/// </summary>
	public class TimeComparer : IComparer<Record>
	{
		public int Compare(Record left, Record right)
		{
			return left.Time.CompareTo(right.Time);
		}
	}

	/// <summary>
	/// Compares by rank.
	/// </summary>
	public class RankComparer : IComparer<Record>
	{
		public int Compare(Record left, Record right)
		{
			// recent
			var recent1 = left.IsRecent;
			var recent2 = right.IsRecent;
			if (recent1 && !recent2)
				return 1;
			if (!recent1 && recent2)
				return -1;
			if (recent1)
				return left.Time.CompareTo(right.Time);

			// used
			var used1 = left.IsTracked && left.What != AGED;
			var used2 = right.IsTracked && right.What != AGED;
			if (used1 && !used2)
				return 1;
			if (!used1 && used2)
				return -1;

			// time
			return left.Time.CompareTo(right.Time);
		}
	}
}
