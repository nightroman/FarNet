
/*
FarNet module Vessel
Copyright (c) 2011-2012 Roman Kuzmin
*/

using System;
using System.Collections.Generic;

namespace FarNet.Vessel
{
	class InfoComparer : IComparer<Info>
	{
		int _limit0;
		int _factor1;
		int _factor2;

		public InfoComparer(int limit0, int factor1, int factor2)
		{
			_limit0 = limit0;
			_factor1 = factor1;
			_factor2 = factor2;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
		public int Compare(Info left, Info right)
		{
			// group or recent time
			{
				var x = left.Group(_limit0, _factor1, _factor2);
				var y = right.Group(_limit0, _factor1, _factor2);
				if (x < y)
					return -1;
				if (x > y)
					return 1;
				if (x == 0)
					return left.Idle.CompareTo(right.Idle);
			}

			// evidence
			{
				var x = left.Evidence;
				var y = right.Evidence;
				if (x > y)
					return -1;
				if (x < y)
					return 1;
			}

			// activity
			{
				int x = left.Activity;
				int y = right.Activity;
				if (x > y)
					return -1;
				if (x < y)
					return 1;
			}

			// days
			{
				int x = left.DayCount;
				int y = right.DayCount;
				if (x > y)
					return -1;
				if (x < y)
					return 1;
			}

			// keys
			{
				int x = Mat.Span(left.KeyCount, 2);
				int y = Mat.Span(right.KeyCount, 2);
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
