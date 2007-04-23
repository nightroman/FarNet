/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#pragma once
#include "Collections.h"

namespace FarManagerImpl
{;
public ref class SelectionCollection : public ISelection
{
public:
	virtual property bool Exists { bool get(); }
	virtual property bool IsFixedSize { bool get(); }
	virtual property bool IsReadOnly { bool get(); }
	virtual property bool IsSynchronized { bool get(); }
	virtual property ILine^ First { ILine^ get(); }
	virtual property ILine^ Item[int] {ILine^ get(int index); void set(int index, ILine^ value); }
	virtual property ILine^ Last { ILine^ get(); }
	virtual property int Count { int get(); }
	virtual property IStrings^ Strings { IStrings^ get(); }
	virtual property Place Shape { Place get(); }
	virtual property Object^ SyncRoot { Object^ get(); }
	virtual property SelectionType Type { SelectionType get(); }
	virtual property String^ Text { String^ get(); void set(String^ value); }
public:
	virtual bool Contains(ILine^ item);
	virtual bool Remove(ILine^ item);
	virtual IEnumerator<ILine^>^ GetEnumerator() = IEnumerable<ILine^>::GetEnumerator;
	virtual int IndexOf(ILine^ item);
	virtual System::Collections::IEnumerator^ GetEnumeratorObject() = System::Collections::IEnumerable::GetEnumerator;
	virtual void Add(ILine^ item);
	virtual void Add(String^ item);
	virtual void Clear();
	virtual void CopyTo(array<ILine^>^, int);
	virtual void Insert(int, ILine^ item);
	virtual void Insert(int, String^ item);
	virtual void RemoveAt(int index);
public:
	virtual void Select(SelectionType type, int pos1, int line1, int pos2, int line2);
	virtual void Unselect();
internal:
	SelectionCollection(IEditor^ editor);
private:
	IEditor^ _editor;
	EditorStringCollection^ _strings;
};
}
