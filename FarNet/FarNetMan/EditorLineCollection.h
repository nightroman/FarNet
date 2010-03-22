/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class EditorLineCollection : ILineCollection
{
public:
	virtual property ILine^ First { ILine^ get(); }
	virtual property ILine^ Last { ILine^ get(); }
public:
	virtual void AddText(String^ item);
	virtual void InsertText(int, String^ item);
public:
	virtual property bool IsFixedSize { bool get(); }
	virtual property bool IsReadOnly { bool get(); }
	virtual property bool IsSynchronized { bool get(); }
	virtual property ILine^ default[int] { ILine^ get(int index); void set(int, ILine^) { throw gcnew NotSupportedException; } }
	virtual property int Count { int get(); }
	virtual property Object^ SyncRoot { Object^ get() { throw gcnew NotSupportedException; } }
public:
	virtual bool Contains(ILine^) { throw gcnew NotSupportedException; }
	virtual bool Remove(ILine^ item);
	virtual IEnumerator<ILine^>^ GetEnumerator();
	virtual int IndexOf(ILine^) { throw gcnew NotSupportedException; }
	virtual void Add(ILine^ item);
	virtual void Clear();
	virtual void CopyTo(array<ILine^>^, int) { throw gcnew NotSupportedException; }
	virtual void Insert(int index, ILine^ item);
	virtual void RemoveAt(int index);
internal:
	EditorLineCollection(bool ignoreEmptyLast);
private:
	virtual System::Collections::IEnumerator^ GetEnumerator2() sealed = System::Collections::IEnumerable::GetEnumerator;
private:
	const bool IgnoreEmptyLast;
};
}
