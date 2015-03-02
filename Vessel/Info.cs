
/*
FarNet module Vessel
Copyright (c) 2011-2015 Roman Kuzmin
*/

using System;

namespace FarNet.Vessel
{
	/// <summary>
	/// Collected file summary information.
	/// </summary>
	public class Info
	{
		// scale 2: 10 ~ 42 days
		public const int SpanCount = 11;
		public const int SpanScale = 2;
		/// <summary>
		/// File path.
		/// </summary>
		public string Path { get; set; }
		/// <summary>
		/// The first recorded time.
		/// </summary>
		public DateTime Head { get; set; }
		/// <summary>
		/// The last recorded time.
		/// </summary>
		public DateTime Tail { get; set; }
		/// <summary>
		/// Count of records.
		/// </summary>
		public int UseCount { get; set; }
		/// <summary>
		/// Recent activity rank.
		/// </summary>
		public int Activity { get; set; }
		/// <summary>
		/// Count of days of use.
		/// </summary>
		public int DayCount { get; set; }
		/// <summary>
		/// Count of typed keys.
		/// </summary>
		public int KeyCount { get; set; }
		/// <summary>
		/// Idle span since the last use.
		/// </summary>
		public TimeSpan Idle { get; set; }
		/// <summary>
		/// Kind of probability in percents.
		/// </summary>
		public int Evidence { get; set; }
		/// <summary>
		/// Recency group: 0 is the most recent to be sorted by time.
		/// </summary>
		public int Group(int limit0, int factor1, int factor2)
		{
			var hours = Idle.TotalHours;

			if (hours < limit0)
				return 0;

			if (hours < factor1)
				return 1;

			if (Idle.TotalDays < factor2)
				return 2;

			return 3;
		}
	}
}
