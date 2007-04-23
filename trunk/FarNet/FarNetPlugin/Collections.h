/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#pragma once

namespace FarManagerImpl
{;
/// <summary>
/// Enumerator of ILine items in IList.
/// </summary>
public ref class LineListEnumerator : public IEnumerator<ILine^>
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
public ref class EditorStringCollection : IStrings
{
	ILines^ _lines;
	bool _selected;
public:
	EditorStringCollection(ILines^ lines, bool selected)
	{
		_lines = lines;
		_selected = selected;
	}
	virtual property String^ Text { String^ get(); void set(String^ value); }
	virtual property String^ default[int]
	{
		String^ get(int index)
		{
			if (_selected)
				return _lines[index]->Selection->Text;
			else
				return _lines[index]->Text;
		}
		void set(int index, String^ value)
		{
			if (_selected)
				_lines[index]->Selection->Text = value;
			else
				_lines[index]->Text = value;
		}
	}
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
		throw gcnew NotSupportedException();
	}
	virtual int IndexOf(String^)
	{
		throw gcnew NotSupportedException();
	}
	virtual bool Remove(String^)
	{
		throw gcnew NotSupportedException();
	}
public:
	// ICollection Members
	virtual property bool IsSynchronized
	{
		bool get() { return false; }
	}
	virtual void CopyTo(array<String^>^ arrayObject, int arrayIndex)
	{
		if (arrayObject == nullptr)
			throw gcnew ArgumentNullException("array");
		if (arrayIndex < 0)
			throw gcnew ArgumentOutOfRangeException("arrayIndex");
		if (arrayObject->Length - arrayIndex > Count)
			throw gcnew ArgumentException("array, arrayIndex");
		for each(String^ s in this)
			arrayObject[++arrayIndex] = s;
	}
	virtual property int Count
	{
		int get() { return _lines->Count; }
	}
	virtual property Object^ SyncRoot
	{
		Object^ get() { return this; }
	}
public:
	// IEnumerable Members
	virtual IEnumerator<String^>^ GetEnumerator()
	{
		return gcnew EditorStringEnumerator(_lines, 0, Count - 1);
	}
	virtual System::Collections::IEnumerator^ GetEnumerator2() = System::Collections::IEnumerable::GetEnumerator
	{
		return gcnew EditorStringEnumerator(_lines, 0, Count - 1);
	}
};

/// <summary>
/// Base class for VisibleEditorLineCollection è Selection 
/// </summary>
public ref class EditorLineCollection abstract : ILines
{
	EditorStringCollection^ _strings;
protected:
	EditorLineCollection()
	{
		_strings = gcnew EditorStringCollection(this, false);
	}
public:
	virtual void Add(String^ item) = 0;
	virtual void Insert(int index, String^ item) = 0;
public:
	// ILines Members
	virtual property ILine^ First
	{
		ILine^ get() { return this[0]; }
	}
	virtual property ILine^ Last
	{
		ILine^ get() { return this[Count - 1]; }
	}
	virtual property String^ Text
	{
		String^ get()
		{
			return _strings->Text;
		}
		void set(String^ value)
		{
			_strings->Text = value;
		}
	}
	virtual property IStrings^ Strings
	{
		IStrings^ get() { return _strings; }
	}
public:
	// IList Members
	virtual property ILine^ default[int]
	{
		ILine^ get(int) = 0;
		void set(int, ILine^) = 0;
	}
	virtual void Add(ILine^ item)
	{
		if (item == nullptr)
			throw gcnew ArgumentNullException("item");
		Add(item->Text);
	}
	virtual void Clear() = 0;
	virtual bool Contains(ILine^)
	{
		throw gcnew NotSupportedException();
	}
	virtual int IndexOf(ILine^)
	{
		throw gcnew NotSupportedException();
	}
	virtual void Insert(int index, ILine^ item)
	{
		if (item == nullptr)
			throw gcnew ArgumentNullException("item");
		Insert(index, item->Text);
	}
	virtual property bool IsFixedSize
	{
		bool get() { return false; }
	}
	virtual property bool IsReadOnly
	{
		bool get() { return false; }
	}
	virtual bool Remove(ILine^ item)
	{
		if (item == nullptr)
			throw gcnew ArgumentNullException("item");
		RemoveAt(item->No);
		return true;
	}
	virtual void RemoveAt(int index) = 0;
public:
	// ICollection Members
	virtual property int Count
	{
		int get() = 0;
	}
	virtual void CopyTo(array<ILine^>^, int)
	{
		throw gcnew NotSupportedException();
	}
	virtual property bool IsSynchronized
	{
		bool get() { return false; }
	}
	virtual property Object^ SyncRoot
	{
		Object^ get() { return this; }
	}
public:
	// IEnumerable Members
	virtual IEnumerator<ILine^>^ GetEnumerator()
	{
		return gcnew LineListEnumerator(this, 0, Count);
	}
private:
	virtual System::Collections::IEnumerator^ GetEnumerator2() sealed = System::Collections::IEnumerable::GetEnumerator
	{
		return gcnew LineListEnumerator(this, 0, Count);
	}
};
}
