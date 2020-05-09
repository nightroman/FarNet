
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
		public const int SpanCount = 10;
		static readonly int[] _spanLen = new int[SpanCount] { 2, 2, 4, 8, 16, 32, 64, 128, 256, 1024 };
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
		public float Evidence { get; private set; }
		/// <summary>
		/// Recency group: 0 is the most recent to be sorted by time.
		/// </summary>
		public int Group(int group0)
		{
			return Idle.TotalHours < group0 ? 0 : 1;
		}
		public void SetEvidence(int count, int span)
		{
			Evidence = (float)count / _spanLen[span];
		}
	}
}
