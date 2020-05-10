
// FarNet module Vessel
// Copyright (c) Roman Kuzmin

using System.Collections.Generic;

namespace FarNet.Vessel
{
	class InfoComparer : IComparer<Info>
	{
		readonly int _group0;
		public InfoComparer(int group0)
		{
			_group0 = group0;
		}
		public int Compare(Info left, Info right)
		{
			// group or recent time
			{
				var x = left.Group(_group0);
				var y = right.Group(_group0);
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

			// times
			return left.Idle.CompareTo(right.Idle);
		}
	}
}
