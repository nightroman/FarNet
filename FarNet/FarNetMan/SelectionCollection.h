/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once
#include "Collections.h"

namespace FarNet
{;
ref class SelectionCollection : public ISelection
{
public:
	virtual property bool Exists { bool get(); }
	virtual property bool IsFixedSize { bool get(); }
	virtual property bool IsReadOnly { bool get(); }
	virtual property bool IsSynchronized { bool get(); }
	virtual property ILine^ First { ILine^ get(); }
	virtual property ILine^ Item[int] {ILine^ get(int index); void set(int, ILine^) { throw gcnew NotSupportedException; } }
	virtual property ILine^ Last { ILine^ get(); }
	virtual property int Count { int get(); }
	virtual property IStrings^ Strings { IStrings^ get(); }
	virtual property Place Shape { Place get(); }
	virtual property Object^ SyncRoot { Object^ get(); }
	virtual property RegionKind Kind { RegionKind get(); }
public:
	virtual IEnumerator<ILine^>^ GetEnumerator() = IEnumerable<ILine^>::GetEnumerator;
	virtual String^ GetText() { return GetText(CV::CRLF); }
	virtual String^ GetText(String^ separator);
	virtual System::Collections::IEnumerator^ GetEnumeratorObject() = System::Collections::IEnumerable::GetEnumerator;
	virtual void Add(String^ item);
	virtual void Clear();
	virtual void Insert(int, String^ item);
	virtual void RemoveAt(int index);
	virtual void SetText(String^ text);
public:
	virtual void Select(RegionKind kind, int pos1, int line1, int pos2, int line2);
	virtual void SelectAll();
	virtual void Unselect();
public:
	virtual bool Contains(ILine^) { throw gcnew NotSupportedException; }
	virtual bool Remove(ILine^)  { throw gcnew NotSupportedException; }
	virtual int IndexOf(ILine^) { throw gcnew NotSupportedException; }
	virtual void Add(ILine^) { throw gcnew NotSupportedException("Use Add(string) method."); }
	virtual void CopyTo(array<ILine^>^, int) { throw gcnew NotSupportedException; }
	virtual void Insert(int, ILine^) { throw gcnew NotSupportedException("Use Insert(string) method."); }
internal:
	SelectionCollection(IEditor^ editor, bool trueLines);
private:
	IEditor^ _editor;
	bool _trueLines;
	EditorStringCollection^ _strings;
};
}
