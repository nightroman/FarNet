/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class SelectionCollection : public ILineCollection
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
	virtual property ILine^ Item[int] { ILine^ get(int index); void set(int, ILine^) { throw gcnew NotSupportedException; } }
	virtual property int Count { int get(); }
	virtual property Object^ SyncRoot { Object^ get(); }
public:
	virtual bool Contains(ILine^) { throw gcnew NotSupportedException; }
	virtual bool Remove(ILine^)  { throw gcnew NotSupportedException; }
	virtual IEnumerator<ILine^>^ GetEnumerator() = IEnumerable<ILine^>::GetEnumerator;
	virtual int IndexOf(ILine^) { throw gcnew NotSupportedException; }
	virtual System::Collections::IEnumerator^ GetEnumeratorObject() = System::Collections::IEnumerable::GetEnumerator;
	virtual void Add(ILine^) { throw gcnew NotSupportedException("Use AddText()."); }
	virtual void Clear();
	virtual void CopyTo(array<ILine^>^, int) { throw gcnew NotSupportedException; }
	virtual void Insert(int, ILine^) { throw gcnew NotSupportedException("Use InsertText()."); }
	virtual void RemoveAt(int index);
internal:
	SelectionCollection(IEditor^ editor, bool ignoreEmptyLast);
private:
	IEditor^ const _editor;
	const bool IgnoreEmptyLast;
};
}
