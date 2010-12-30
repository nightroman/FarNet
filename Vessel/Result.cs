
/*
FarNet module Vessel
Copyright (c) 2010 Roman Kuzmin
*/

namespace FarNet.Vessel
{
	public class Result
	{
		public int Factor1 { get; set; }
		public int Factor2 { get; set; }
		public int UpCount { get; set; }
		public int DownCount { get; set; }
		public int SameCount { get; set; }
		public int UpSum { get; set; }
		public int DownSum { get; set; }

		public int TotalSum
		{
			get { return UpSum - DownSum; }
		}

		public float AverageGain
		{
			get
			{
				int count = UpCount + DownCount + SameCount;
				return count == 0 ? 0 : (float)(UpSum - DownSum) / count;
			}
		}

		// Keep it float for experiments with float targets
		public float Target
		{
			get { return TotalSum; }
		}

	}
}
