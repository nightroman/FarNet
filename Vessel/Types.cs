
/*
FarNet module Vessel
Copyright (c) 2010 Roman Kuzmin
*/

namespace FarNet.Vessel
{
	public class Stat
	{
		public double Factor { get; set; }
		public int UpCount { get; set; }
		public int DownCount { get; set; }
		public int SameCount { get; set; }
		public int UpSum { get; set; }
		public int DownSum { get; set; }
		public int TotalSum { get; set; }

		public float UpAverage
		{
			get
			{
				return UpCount == 0 ? 0 : (float)UpSum / UpCount;
			}
		}

		public float DownAverage
		{
			get
			{
				return DownCount == 0 ? 0 : (float)DownSum / DownCount;
			}
		}

		public float TotalAverage
		{
			get
			{
				int count = UpCount + DownCount;
				return count == 0 ? 0 : (float)(UpSum - DownSum) / count;
			}
		}

		public float GlobalAverage
		{
			get
			{
				int count = UpCount + DownCount + SameCount;
				return count == 0 ? 0 : (float)(UpSum - DownSum) / count;
			}
		}
	}
}
