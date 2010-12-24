
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
		float _factor;

		public InfoComparer(float factor)
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
				int x = (int)Math.Log((float)left.KeyCount + 1, 2);
				int y = (int)Math.Log((float)right.KeyCount + 1, 2);
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
