/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System.Collections;

namespace FarNet.Works
{
	public class EnumerableReader
	{
		readonly IEnumerator Enumerator;

		public EnumerableReader(IEnumerable enumerable)
		{
			Enumerator = enumerable.GetEnumerator();
		}

		public string Read()
		{
			if (!Enumerator.MoveNext())
				throw new ModuleException("Unexpected end of the data sequence.");

			return Enumerator.Current.ToString();
		}

		public string TryRead()
		{
			if (!Enumerator.MoveNext())
				return null;

			return Enumerator.Current.ToString();
		}
	}

}
