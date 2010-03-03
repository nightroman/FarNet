/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class LineListEnumerator sealed : IEnumerator<ILine^>
{
public:
	LineListEnumerator(IList<ILine^>^ list, int start, int end)
		: _list(list), _start(_start), _end(end), _index(start - 1)
	{}
	~LineListEnumerator()
	{}
	virtual property ILine^ Current
	{
		ILine^ get() = IEnumerator<ILine^>::Current::get
		{
			return _list[_index];
		}
	}
	virtual bool MoveNext()
	{
		return ++_index < _end;
	}
	virtual void Reset()
	{
		_index = _start;
	}
private:
	virtual property Object^ CurrentObject
	{
		Object^ get() sealed = System::Collections::IEnumerator::Current::get
		{
			return _list[_index];
		}
	}
private:
	IList<ILine^>^ _list;
	int _start;
	int _end;
	int _index;
};

/// <summary>
/// Enumerator of EditorStringCollection.
/// </summary>
ref class EditorStringEnumerator : IEnumerator<String^>
{
	ILines^ _lines;
	int _first;
	int _last;
	int _index;
public:
	EditorStringEnumerator(ILines^ lines, int first, int last)
	{
		_lines = lines;
		_first = first;
		_last = last;
		_index = first - 1;
	}
	~EditorStringEnumerator()
	{
	}
	virtual bool MoveNext()
	{
		return ++_index <= _last;
	}
	virtual void Reset()
	{
		_index = _first;
	}
	virtual property String^ Current
	{
		String^ get() = IEnumerator<String^>::Current::get
		{
			return _lines[_index]->Text;
		}
	}
private:
	virtual property Object^ Current2
	{
		Object^ get() sealed = System::Collections::IEnumerator::Current::get
		{
			return _lines[_index]->Text;
		}
	}
};

/// <summary>
/// Implements IStrings editor lines as strings.
/// </summary>
ref class EditorStringCollection : IStrings
{
	ILines^ _lines;
	bool _selected;
public:
	EditorStringCollection(ILines^ lines, bool selected);
	virtual property String^ default[int] { String^ get(int index); void set(int index, String^ value); }
	virtual void Add(String^ item)
	{
		_lines->Add(item);
	}
	virtual void Clear()
	{
		_lines->Clear();
	}
	virtual void Insert(int index, String^ item)
	{
		_lines->Insert(index, item);
	}
	virtual property bool IsFixedSize
	{
		bool get() { return false; }
	}
	virtual property bool IsReadOnly
	{
		bool get() { return false; }
	}
	virtual void RemoveAt(int index)
	{
		_lines->RemoveAt(index);
	}
	virtual bool Contains(String^)
	{
		throw gcnew NotSupportedException;
	}
	virtual int IndexOf(String^)
	{
		throw gcnew NotSupportedException;
	}
	virtual bool Remove(String^)
	{
		throw gcnew NotSupportedException;
	}
public:
	virtual property bool IsSynchronized
	{
		bool get() { return false; }
	}
	virtual void CopyTo(array<String^>^ arrayObject, int arrayIndex);
	virtual property int Count
	{
		int get() { return _lines->Count; }
	}
	virtual property Object^ SyncRoot
	{
		Object^ get() { return this; }
	}
public:
	virtual IEnumerator<String^>^ GetEnumerator()
	{
		return gcnew EditorStringEnumerator(_lines, 0, Count - 1);
	}
	virtual System::Collections::IEnumerator^ GetEnumerator2() = System::Collections::IEnumerable::GetEnumerator
	{
		return gcnew EditorStringEnumerator(_lines, 0, Count - 1);
	}
};
}
