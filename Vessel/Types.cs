
/*
FarNet module Vessel
Copyright (c) 2011 Roman Kuzmin
*/

namespace FarNet.Vessel
{
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

	enum TrainingState
	{
		None,
		Started,
		Completed
	}
}
