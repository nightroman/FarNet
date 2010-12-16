
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
		/// Count of typed keys.
		/// </summary>
		public int KeyCount { get; set; }

		/// <summary>
		/// Idle time from the last use.
		/// </summary>
		public TimeSpan Idle { get; set; }

		/// <summary>
		/// Recent activity rank.
		/// </summary>
		public int Activity { get; set; }

		/// <summary>
		/// Count of days of use.
		/// </summary>
		public int DayCount { get; set; }

		/// <summary>
		/// Frequency rank with 0 as the least used.
		/// </summary>
		public int Frequency { get; set; }

		/// <summary>
		/// Recency rank (the least is the most recent).
		/// </summary>
		public int Recency(double factor)
		{
			var h = Idle.TotalHours;
			if (h >= factor)
				return (int)(Math.Log(h, factor));
			if (h > factor / 1.5) //??
				return 0;
			return -1;
		}

	}
}
