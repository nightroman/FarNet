
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

using System;
using System.Collections;

namespace FarNet.Works
{
	public class EnumerableReader
	{
		readonly IEnumerator Enumerator;
		public EnumerableReader(IEnumerable enumerable)
		{
			if (enumerable == null)
				throw new ArgumentNullException("enumerable");

			Enumerator = enumerable.GetEnumerator();
		}
		public object Read()
		{
			if (!Enumerator.MoveNext())
				throw new ModuleException("Unexpected end of the data sequence.");

			return Enumerator.Current;
		}
		public object TryRead()
		{
			if (!Enumerator.MoveNext())
				return null;

			return Enumerator.Current;
		}
	}
}
