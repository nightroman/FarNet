
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

using System;
using System.Collections.Generic;

namespace FarNet.Works
{
	public sealed class LineCollection : IList<ILine>
	{
		readonly IEditor _Editor;
		readonly int _Start;
		int _Count;

		public LineCollection(IEditor editor, int start, int count)
		{
			_Editor = editor;
			_Start = start;
			_Count = count;
		}

		public int Count
		{
			get { return _Count; }
		}

		public ILine this[int index]
		{
			get
			{
				if (index < 0 || index >= _Count)
					throw new ArgumentOutOfRangeException("index");

				return _Editor[_Start + index];
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public void RemoveAt(int index)
		{
			if (index < 0 || index >= _Count)
				throw new ArgumentOutOfRangeException("index");

			_Editor.RemoveAt(_Start + index);

			--_Count;
		}

		public IEnumerator<ILine> GetEnumerator() { return EditorTools.EnumerateLines(_Editor, _Start, _Start + _Count).GetEnumerator(); }
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public bool IsFixedSize { get { return false; } }

		public bool IsReadOnly { get { return false; } }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public bool IsSynchronized { get { return false; } }

		public object SyncRoot { get { return this; } }

		public void Add(ILine item) { throw new NotSupportedException(); }

		public void Clear() { throw new NotSupportedException(); }

		public bool Contains(ILine item) { throw new NotSupportedException(); }

		public void CopyTo(ILine[] array, int arrayIndex) { throw new NotSupportedException(); }

		public int IndexOf(ILine item) { throw new NotSupportedException(); }

		public void Insert(int index, ILine item) { throw new NotSupportedException(); }

		public bool Remove(ILine item) { throw new NotSupportedException(); }
	}
}
