/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;

namespace FarNet.Works
{
	public sealed class SelectionCollection : IList<ILine>
	{
		readonly IEditor _Editor;

		public SelectionCollection(IEditor editor)
		{
			_Editor = editor;
		}

		public int Count
		{
			get
			{
				Place pp = _Editor.SelectionPlace;
				if (pp.Top < 0)
					return 0;
				else
					return pp.Height;
			}
		}

		public ILine this[int index]
		{
			get
			{
				Point pt = _Editor.SelectionPoint;
				if (pt.Y < 0)
					throw new InvalidOperationException();

				return _Editor[pt.Y + index];
			}
			set
			{
				throw new NotSupportedException();
			}
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
			_Editor.DeleteText();
		}

		public IEnumerator<ILine> GetEnumerator()
		{
			Place pp = _Editor.SelectionPlace;
			if (pp.Top < 0)
				return EditorTools.EnumerateLines(_Editor, 0, 0).GetEnumerator();
			else
				return EditorTools.EnumerateLines(_Editor, pp.Top, pp.Bottom + 1).GetEnumerator();
		}

		public void RemoveAt(int index)
		{
			Point pt = _Editor.SelectionPoint;
			if (pt.Y < 0)
				throw new InvalidOperationException();

			_Editor.RemoveAt(pt.Y + index);
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
