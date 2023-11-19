
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;

namespace FarNet.Works;
#pragma warning disable CS1591, CA1822

public sealed class StringCollection(IEditor editor) : IList<string>
{
	readonly IEditor _Editor = editor;

	public int Count => _Editor.Count;

	public string this[int index]
	{
		get => _Editor[index].Text;
		set => _Editor[index].Text = value;
	}

	public bool IsFixedSize => false;

	public bool IsReadOnly => false;

	public bool IsSynchronized => false;

	public object SyncRoot => this;

	public void Clear() => _Editor.Clear();

	public IEnumerator<string> GetEnumerator() => EditorTools.EnumerateStrings(_Editor, 0, _Editor.Count).GetEnumerator();

	public void RemoveAt(int index) => _Editor.RemoveAt(index);

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

	public void CopyTo(string[] array, int arrayIndex)
	{
		ArgumentNullException.ThrowIfNull(array);

		foreach (string it in EditorTools.EnumerateStrings(_Editor, 0, _Editor.Count))
			array[arrayIndex++] = it;
	}

	public void Add(string item) => _Editor.Add(item);

	public void Insert(int index, string item) => _Editor.Insert(index, item);

	public bool Contains(string item) => throw new NotSupportedException();

	public int IndexOf(string item) => throw new NotSupportedException();

	public bool Remove(string item) => throw new NotSupportedException();
}
