using FarManager;
using System.Collections.Generic;
using System;

namespace FarManager.Impl
{
	/// <summary>
	/// Base class for VisibleEditorLineCollection è Selection 
	/// </summary>
	abstract public class EditorLineCollection : ILines
	{
		EditorStringCollection _strings;

		protected EditorLineCollection()
		{
			_strings = new EditorStringCollection(this, false);
		}

		public abstract void Add(string item);
		public abstract void Insert(int index, string item);

		#region ILines Members

		public ILine First
		{
			get { return this[0]; }
		}

		public ILine Last
		{
			get { return this[Count - 1]; }
		}

		public string Text
		{
			get { return _strings.Text; }
			set { _strings.Text = value; }
		}

		public IStrings Strings
		{
			get { return _strings; }
		}

		#endregion

		#region IList Members

		abstract public ILine this[int index] { get; set; }

		public void Add(ILine item)
		{
			if (item == null)
				throw new ArgumentNullException("item");
			Add(item.Text);
		}

		public abstract void Clear();

		public bool Contains(ILine item)
		{
			throw new NotSupportedException();
		}

		public int IndexOf(ILine item)
		{
			throw new NotSupportedException();
		}

		public void Insert(int index, ILine item)
		{
			if (item == null)
				throw new ArgumentNullException("item");
			Insert(index, item.Text);
		}

		public bool IsFixedSize
		{
			get { return false; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(ILine item)
		{
			if (item == null)
				throw new ArgumentNullException("item");
			RemoveAt(item.No);
			return true;
		}

		public abstract void RemoveAt(int index);

		#endregion

		#region ICollection Members

		public abstract int Count { get; }

		public void CopyTo(ILine[] array, int arrayIndex)
		{
			throw new NotSupportedException();
		}

		public bool IsSynchronized
		{
			get { return false; }
		}

		public object SyncRoot
		{
			get { return this; }
		}

		#endregion

		#region IEnumerable Members

		protected abstract IEnumerator<ILine> EnumeratorImpl { get; }

		public IEnumerator<ILine> GetEnumerator()
		{
			return EnumeratorImpl;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return EnumeratorImpl;
		}

		#endregion
	}
}
