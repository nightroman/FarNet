/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;

namespace FarNet.Works
{
	public sealed class LineCollection : IList<ILine>
	{
		readonly IEditor _Editor;

		public LineCollection(IEditor editor)
		{
			_Editor = editor;
		}

		public int Count
		{
			get { return _Editor.Count; }
		}

		public ILine this[int index]
		{
			get { return _Editor[index]; }
			set { throw new NotSupportedException(); }
		}

		public bool IsFixedSize
		{
			get { return false; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool IsSynchronized
		{
			get { return false; }
		}

		public object SyncRoot
		{
			get { return this; }
		}

		public void Clear()
		{
			_Editor.Clear();
		}

		public IEnumerator<ILine> GetEnumerator()
		{
			return EditorTools.Enumerate(_Editor, 0, _Editor.Count).GetEnumerator();
		}

		public void RemoveAt(int index)
		{
			_Editor.RemoveAt(index);
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(ILine item) { throw new NotSupportedException(); }
		public bool Contains(ILine item) { throw new NotSupportedException(); }
		public void CopyTo(ILine[] array, int start) { throw new NotSupportedException(); }
		public int IndexOf(ILine item) { throw new NotSupportedException(); }
		public void Insert(int index, ILine item) { throw new NotSupportedException(); }
		public bool Remove(ILine item) { throw new NotSupportedException(); }
	}
}
