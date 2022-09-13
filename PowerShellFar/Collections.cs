
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections;
using System.Collections.Generic;

namespace My;

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
	protected T _current = default!;

	public Enumerator(IEnumerable<S> enumerable)
	{
		_enumerator = enumerable.GetEnumerator();
	}

	/// <summary>
	/// Calls _enumerator.MoveNext() and set _current.
	/// </summary>
	abstract public bool MoveNext();

	public T Current => _current;

	object IEnumerator.Current => _current!;

	public void Reset() => _enumerator.Reset();

	public void Dispose()
	{
		_enumerator.Dispose();
		GC.SuppressFinalize(this);
	}
}

/// <summary>
/// Void enumerator.
/// </summary>
struct VoidEnumerator<T> : IEnumerator<T>
{
	public void Reset() { }

	public bool MoveNext() => false;

	public void Dispose() { }

	public T Current => throw new InvalidOperationException();

	object IEnumerator.Current => throw new InvalidOperationException();
}

abstract class SimpleCollection : ICollection
{
	public bool IsSynchronized => false;

	public object SyncRoot => this;

	public void CopyTo(Array array, int index)
	{
		if (array == null)
			throw new ArgumentNullException(nameof(array));
		if (index < 0)
			throw new ArgumentOutOfRangeException(nameof(index));
		if (Count > array.Length - index)
			throw new ArgumentException("Not enough space in the array.");

		foreach (var value in this)
		{
			array.SetValue(value, index);
			++index;
		}
	}

	public abstract int Count { get; }

	public abstract IEnumerator GetEnumerator();
}
