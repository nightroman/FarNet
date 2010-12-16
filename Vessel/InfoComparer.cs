
/*
FarNet module Vessel
Copyright (c) 2010 Roman Kuzmin
*/

using System;
using System.Collections.Generic;

namespace FarNet.Vessel
{
	class InfoComparer : IComparer<Info>
	{
		double _factor;

		public InfoComparer(double factor)
		{
			_factor = factor;
		}

		public int Compare(Info left, Info right)
		{
			// recency
			var recency1 = left.Recency(_factor);
			var recency2 = right.Recency(_factor);
			if (recency1 < recency2)
				return -1;
			if (recency1 > recency2)
				return 1;

			// both fresh, compare times
			if (recency1 < 1)
			{
				if (recency1 == 0)
				{
					int activity1 = left.Activity;
					int activity2 = right.Activity;
					if (activity1 < activity2)
						return 1;
					if (activity1 > activity2)
						return -1;
				}
				return left.Idle.CompareTo(right.Idle);
			}

			{
				int activity1 = left.Activity;
				int activity2 = right.Activity;
				if (activity1 < activity2)
					return 1;
				if (activity1 > activity2)
					return -1;
			}

			// days
			int days1 = left.DayCount;
			int days2 = right.DayCount;
			if (days1 < days2)
				return 1;
			if (days1 > days2)
				return -1;

			// frequency
			int frequency1 = left.Frequency;
			int frequency2 = right.Frequency;
			if (frequency1 < frequency2)
				return 1;
			if (frequency1 > frequency2)
				return -1;

			// compare times
			return left.Idle.CompareTo(right.Idle);
		}
	}
}
