
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
		int _factor1;
		int _factor2;

		public InfoComparer(int factor1, int factor2)
		{
			_factor1 = factor1;
			_factor2 = factor2;
		}

		public int Compare(Info left, Info right)
		{
			if (left == null) throw new ArgumentNullException("left");
			if (right == null) throw new ArgumentNullException("right");
			
			// recency
			var recency1 = left.RecentRank(_factor1, _factor2);
			var recency2 = right.RecentRank(_factor1, _factor2);
			if (recency1 < recency2)
				return -1;
			if (recency1 > recency2)
				return 1;

			// recent times
			if (recency1 == 0)
				return left.Idle.CompareTo(right.Idle);

			// activity
			{
				int x = left.Activity;
				int y = right.Activity;
				if (x > y)
					return -1;
				if (x < y)
					return 1;
			}

			// day counts
			{
				int x = left.DayCount;
				int y = right.DayCount;
				if (x > y)
					return -1;
				if (x < y)
					return 1;
			}

			// frequency
			{
				int x = left.Frequency > 0 ? 1 : 0;
				int y = right.Frequency > 0 ? 1 : 0;
				if (x > y)
					return -1;
				if (x < y)
					return 1;
			}

			// key counts
			{
				int x = Mat.Factor(left.KeyCount, 2);
				int y = Mat.Factor(right.KeyCount, 2);
				if (x > y)
					return -1;
				if (x < y)
					return 1;
			}

			// times
			return left.Idle.CompareTo(right.Idle);
		}
	}
}
