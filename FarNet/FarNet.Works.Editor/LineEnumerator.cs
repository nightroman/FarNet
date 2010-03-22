/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;

namespace FarNet.Works
{
	public sealed class LineEnumerator : IEnumerator<ILine>
	{
		IList<ILine> List;
		int Start;
		int End;

		ILine Value;
		int Index;

		public LineEnumerator(IList<ILine> list, int start, int end)
		{
			List = list;
			Start = start;
			End = end;
			Index = start;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (Index >= End)
			{
				Value = null;
				return false;
			}

			Value = List[Index++];
			return true;
		}

		public void Reset()
		{
			Index = Start;
			Value = null;
		}

		public ILine Current
		{
			get
			{
				return Value;
			}
		}

		object System.Collections.IEnumerator.Current
		{
			get
			{
				return Value;
			}
		}
	}
}
