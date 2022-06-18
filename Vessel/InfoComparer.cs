
// FarNet module Vessel
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;

namespace Vessel;

class InfoComparer : IComparer<Info>
{
	readonly TimeSpan _limit0;
	public InfoComparer(TimeSpan limit0)
	{
		_limit0 = limit0;
	}
	public int Compare(Info left, Info right)
	{
		// recently used
		var recent1 = left.Idle < _limit0;
		var recent2 = right.Idle < _limit0;
		if (recent1 && !recent2)
			return -1;
		if (!recent1 && recent2)
			return 1;
		if (recent1)
			return left.Idle.CompareTo(right.Idle);

		// evidence
		var evidence1 = left.Evidence;
		var evidence2 = right.Evidence;
		if (evidence1 > evidence2)
			return -1;
		if (evidence1 < evidence2)
			return 1;

		// times
		return left.Idle.CompareTo(right.Idle);
	}
}
