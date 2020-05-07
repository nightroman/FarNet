
// FarNet module Vessel
// Copyright (c) Roman Kuzmin

using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace FarNet.Vessel
{
	public class Record
	{
		internal const string NOOP = "";
		internal const string AGED = "aged";
		internal const string EDIT = "edit";
		internal const string GOTO = "goto";
		internal const string OPEN = "open";
		internal const string VIEW = "view";
		public DateTime Time { get; private set; }
		public string What { get; private set; }
		public string Path { get; private set; }
		internal Record(DateTime time, string what, string path)
		{
			Time = time;
			What = what;
			Path = path;
		}
		public void SetAged()
		{
			What = AGED;
		}
	}
	public class Result
	{
		public int UpCount { get; set; }
		public int DownCount { get; set; }
		public int SameCount { get; set; }
		public int UpSum { get; set; }
		public int DownSum { get; set; }
		public float Average
		{
			get
			{
				int count = UpCount + DownCount + SameCount;
				return count == 0 ? 0 : (float)(UpSum - DownSum) / count;
			}
		}
	}
	public static class Logger
	{
		public static TraceSource Source { get { return _Source; } }
		static readonly TraceSource _Source = new TraceSource("Vessel", SourceLevels.All);
	}
	static class Mat
	{
		/// <summary>
		/// Gets the logarithm span of the value.
		/// </summary>
		public static int Span(double value)
		{
			if (value < 2) // base
				return 0;

			int result = 1;
			int limit = 4; // base * base
			while (value >= limit)
			{
				++result;
				limit *= 2; // base
			}

			return result;
		}
	}
	public class SpanSet
	{
		readonly int[] _Spans = new int[Info.SpanCount];
		public IList<int> Spans { get { return _Spans; } }
		internal DateTime Time { get; set; }
	}
}
