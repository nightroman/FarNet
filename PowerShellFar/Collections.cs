
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
abstract class Enumerator<T, S>(IEnumerable<S> enumerable) : IEnumerator<T>
{
	protected IEnumerator<S> _enumerator = enumerable.GetEnumerator();
	protected T _current = default!;

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
	public readonly void Reset() { }

	public readonly bool MoveNext() => false;

	public readonly void Dispose() { }

	public readonly T Current => throw new InvalidOperationException();

	readonly object IEnumerator.Current => throw new InvalidOperationException();
}

abstract class SimpleCollection : ICollection
{
	public bool IsSynchronized => false;

	public object SyncRoot => this;

	public void CopyTo(Array array, int index)
	{
		ArgumentNullException.ThrowIfNull(array);
		ArgumentOutOfRangeException.ThrowIfNegative(index);
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
