/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#pragma once
#include "Collections.h"

namespace FarNet
{;
public ref class EditorLineCollection : ILines
{
internal:
	EditorLineCollection(bool trueLines);
public:
	virtual property bool IsFixedSize { bool get(); }
	virtual property bool IsReadOnly { bool get(); }
	virtual property bool IsSynchronized { bool get(); }
	virtual property ILine^ default[int] { ILine^ get(int index); void set(int, ILine^) { throw gcnew NotSupportedException(); } }
	virtual property ILine^ First { ILine^ get(); }
	virtual property ILine^ Last { ILine^ get(); }
	virtual property int Count { int get(); }
	virtual property IStrings^ Strings { IStrings^ get(); }
	virtual property Object^ SyncRoot { Object^ get() { throw gcnew NotSupportedException(); } }
public:
	virtual bool Contains(ILine^) { throw gcnew NotSupportedException(); }
	virtual bool Remove(ILine^ item);
	virtual IEnumerator<ILine^>^ GetEnumerator();
	virtual int IndexOf(ILine^) { throw gcnew NotSupportedException(); }
	virtual void Add(ILine^ item);
	virtual void Add(String^ item);
	virtual void Clear();
	virtual void CopyTo(array<ILine^>^, int) { throw gcnew NotSupportedException(); }
	virtual void Insert(int index, ILine^ item);
	virtual void Insert(int index, String^ item);
	virtual void RemoveAt(int index);
private:
	virtual System::Collections::IEnumerator^ GetEnumerator2() sealed = System::Collections::IEnumerable::GetEnumerator;
private:
	bool _trueLines;
	EditorStringCollection^ _strings;
};
}
