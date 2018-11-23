
// FarNet module Vessel
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet.Vessel
{
	/// <summary>
	/// Collected file summary information.
	/// </summary>
	public class Info
	{
		// base 2: 10 ~ 42 days
		public const int SpanCount = 11;
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
		/// Idle span since the last use.
		/// </summary>
		public TimeSpan Idle { get; set; }
		/// <summary>
		/// Kind of probability.
		/// </summary>
		public int Evidence { get; set; }
		/// <summary>
		/// Recency group: 0 is the most recent to be sorted by time.
		/// </summary>
		public int Group(int group0, int group1)
		{
			var hours = Idle.TotalHours;

			if (hours < group0)
				return 0;

			if (hours < group1)
				return 1;

			return 2;
		}
	}
}
