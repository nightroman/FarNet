
/*
FarNet module Vessel
Copyright (c) 2011 Roman Kuzmin
*/

using System;

namespace FarNet.Vessel
{
	static class Mat
	{
		/// <summary>
		/// Gets the logarithm span of the value.
		/// </summary>
		public static int Span(double value, int scale)
		{
			if (value < scale)
				return 0;

			int result = 1;
			int limit = scale * scale;
			while (value >= limit)
			{
				++result;
				limit *= scale;
			}

			return result;
		}

		public static int EvidenceSpan(double value, int scale)
		{
			return Span(value, scale);
		}
	}

	public class SpanSet
	{
		public readonly int[] Spans = new int[Info.SpanCount];
		internal DateTime Time { get; set; }
	}

	enum TrainingState
	{
		None,
		Started,
		Completed
	}

}
