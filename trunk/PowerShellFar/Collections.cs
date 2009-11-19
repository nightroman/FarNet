/*
PowerShellFar plugin for Far Manager
Copyright (C) 2006-2009 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace My
{
	/// <summary>
	/// Enumerator based on yet another one.
	/// </summary>
	/// <remarks>
	/// You have to override <see cref="MoveNext"/>.
	/// </remarks>
	/// <typeparam name="S">Source type.</typeparam>
	/// <typeparam name="T">Result type.</typeparam>
	abstract class Enumerator<T, S> : IEnumerator<T>
	{
		protected IEnumerator<S> _enumerator;
		protected T _current = default(T);

		public Enumerator(IEnumerable<S> enumerable)
		{
			_enumerator = enumerable.GetEnumerator();
		}

		/// <summary>
		/// Calls _enumerator.MoveNext() and set _current.
		/// </summary>
		abstract public bool MoveNext();

		public T Current
		{
			get { return _current; }
		}

		object IEnumerator.Current
		{
			get { return _current; }
		}

		public void Reset()
		{
			_enumerator.Reset();
		}

		public void Dispose()
		{
			_enumerator.Dispose();
		}
	}

	/// <summary>
	/// Void enumerator.
	/// </summary>
	struct VoidEnumerator<T> : IEnumerator<T>
	{
		public void Reset() { }
		public bool MoveNext() { return false; }
		public void Dispose() { }
		public T Current { get { throw new InvalidOperationException(); } }
		object IEnumerator.Current { get { throw new InvalidOperationException(); } }
	}

	abstract class SimpleCollection : System.Collections.ICollection
	{
		public bool IsSynchronized
		{
			get
			{
				return false;
			}
		}

		public object SyncRoot
		{
			get
			{
				return null;
			}
		}

		public void CopyTo(Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException("array");
			if (index < 0)
				throw new ArgumentOutOfRangeException("index");
			if (array.Length - index > Count)
				throw new ArgumentException("array, index");
			foreach (object v in this)
			{
				array.SetValue(v, index);
				++index;
			}
		}

		public abstract int Count { get; }

		public abstract IEnumerator GetEnumerator();
	}
}
