
/*
FarNet module Vessel
Copyright (c) 2010 Roman Kuzmin
*/

using System;

namespace FarNet.Vessel
{
	/// <summary>
	/// File summary information.
	/// </summary>
	public class Info
	{
		/// <summary>
		/// File path.
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// The head time.
		/// </summary>
		public DateTime Head { get; set; }

		/// <summary>
		/// The tail time.
		/// </summary>
		public DateTime Tail { get; set; }

		/// <summary>
		/// Count of use cases.
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
		/// Idle time from the last use.
		/// </summary>
		public TimeSpan Idle { get; set; }

		/// <summary>
		/// Frequency rank with 0 as the least used.
		/// </summary>
		public int Frequency { get; set; }

		/// <summary>
		/// Recency rank: 0 is the most recent to be sorted by time.
		/// </summary>
		public int RecentRank(int factor1, int factor2)
		{
			var hours = Idle.TotalHours;
			
			if (hours < VesselHost.Limit0)
				return 0;

			if (hours < factor1)
				return 1;

			if (Idle.TotalDays < factor2)
				return 2;

			return 3;
		}

	}
}
