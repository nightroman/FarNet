
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;

namespace FarNet.Works;
#pragma warning disable CS1591, CA1822

public sealed class LineCollection(IEditor editor, int start, int count) : IList<ILine>
{
	readonly IEditor _Editor = editor;
	readonly int _Start = start;
	int _Count = count;

	public int Count => _Count;

	public ILine this[int index]
	{
		get
		{
			if (index < 0 || index >= _Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			return _Editor[_Start + index];
		}
		set => throw new NotSupportedException();
	}

	public void RemoveAt(int index)
	{
		if (index < 0 || index >= _Count)
			throw new ArgumentOutOfRangeException(nameof(index));

		_Editor.RemoveAt(_Start + index);

		--_Count;
	}

	public IEnumerator<ILine> GetEnumerator() => EditorTools.EnumerateLines(_Editor, _Start, _Start + _Count).GetEnumerator();

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

	public bool IsFixedSize => false;

	public bool IsReadOnly => false;

	public bool IsSynchronized => false;

	public object SyncRoot => this;

	public void Add(ILine item) => throw new NotSupportedException();

	public void Clear() => throw new NotSupportedException();

	public bool Contains(ILine item) => throw new NotSupportedException();

	public void CopyTo(ILine[] array, int arrayIndex) => throw new NotSupportedException();

	public int IndexOf(ILine item) => throw new NotSupportedException();

	public void Insert(int index, ILine item) => throw new NotSupportedException();

	public bool Remove(ILine item) => throw new NotSupportedException();
}
