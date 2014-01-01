
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

using System;
using System.Collections.Generic;

namespace FarNet.Works
{
	public sealed class StringCollection : IList<string>
	{
		readonly IEditor _Editor;

		public StringCollection(IEditor editor)
		{
			_Editor = editor;
		}

		public int Count
		{
			get { return _Editor.Count; }
		}

		public string this[int index]
		{
			get { return _Editor[index].Text; }
			set { _Editor[index].Text = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public bool IsFixedSize
		{
			get { return false; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
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

		public IEnumerator<string> GetEnumerator()
		{
			return EditorTools.EnumerateStrings(_Editor, 0, _Editor.Count).GetEnumerator();
		}

		public void RemoveAt(int index)
		{
			_Editor.RemoveAt(index);
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void CopyTo(string[] array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			foreach (string it in EditorTools.EnumerateStrings(_Editor, 0, _Editor.Count))
				array[arrayIndex++] = it;
		}

		public void Add(string item)
		{
			_Editor.Add(item);
		}

		public void Insert(int index, string item)
		{
			_Editor.Insert(index, item);
		}

		public bool Contains(string item) { throw new NotSupportedException(); }
		public int IndexOf(string item) { throw new NotSupportedException(); }
		public bool Remove(string item) { throw new NotSupportedException(); }
	}
}
