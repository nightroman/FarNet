
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

		public int Target
		{
			get { return TotalSum; }
		}

		public float ChangeAverage
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

	static class Mat
	{
		public static int Factor(double value, int factor)
		{
			if (value < factor)
				return 0;

			int result = 1;
			int limit = factor * factor;
			while(value >= limit)
			{
				++result;
				limit *= factor;
			}
			
			return result;
		}
	}

}
