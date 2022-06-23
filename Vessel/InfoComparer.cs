
// FarNet module Vessel
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;

namespace Vessel;

class InfoComparer : IComparer<Info>
{
	readonly DateTime _old;

	public InfoComparer(DateTime old)
	{
		_old = old;
	}

	public int Compare(Info left, Info right)
	{
		// recently used
		var recent1 = left.TimeN > _old;
		var recent2 = right.TimeN > _old;
		if (recent1 && !recent2)
			return -1;
		if (!recent1 && recent2)
			return 1;
		if (recent1)
			return right.TimeN.CompareTo(left.TimeN);

		// evidence
		var evidence1 = left.Evidence;
		var evidence2 = right.Evidence;
		if (evidence1 > evidence2)
			return -1;
		if (evidence1 < evidence2)
			return 1;

		// times
		return right.TimeN.CompareTo(left.TimeN);
	}
}
