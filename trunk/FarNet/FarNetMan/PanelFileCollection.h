
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

#pragma once
#include "Wrappers.h"

namespace FarNet
{;
ref class Panel1;

ref class PanelFileEnumerator : IEnumerator<FarFile^>
{
internal:
	PanelFileEnumerator(Panel1^ panel, FileType type, int count);
public:
	~PanelFileEnumerator() {}
	virtual property FarFile^ Current { FarFile^ get(); }
	virtual bool MoveNext();
	virtual void Reset();
private:
	virtual property Object^ CurrentObject { Object^ get() sealed = System::Collections::IEnumerator::Current::get { return Current; } }
protected:
	Panel1^ _Panel;
	FarFile^ _File;
	FileType _Type;
	int _Count;
	int _Index;
};

ref class PanelFileCollection : IList<FarFile^>
{
internal:
	PanelFileCollection(Panel1^ panel, FileType type);
public:
	virtual bool Contains(FarFile^) { throw gcnew NotSupportedException; }
	virtual bool Remove(FarFile^) { throw gcnew NotSupportedException; }
	virtual IEnumerator<FarFile^>^ GetEnumerator() { return gcnew PanelFileEnumerator(_Panel, _Type, _Count); }
	virtual int IndexOf(FarFile^) { throw gcnew NotSupportedException; }
	virtual property bool IsReadOnly { bool get() { return true; } }
	virtual property FarFile^ default[int] { FarFile^ get(int index); void set(int, FarFile^) { throw gcnew NotSupportedException; } }
	virtual property int Count { int get() { return _Count; } }
	virtual void Add(FarFile^) { throw gcnew NotSupportedException; }
	virtual void Clear() { throw gcnew NotSupportedException; }
	virtual void CopyTo(array<FarFile^>^ array, int arrayIndex);
	virtual void Insert(int, FarFile^) { throw gcnew NotSupportedException; }
	virtual void RemoveAt(int) { throw gcnew NotSupportedException; }
private:
	virtual System::Collections::IEnumerator^ GetEnumerator2() sealed = System::Collections::IEnumerable::GetEnumerator { return GetEnumerator(); }
protected:
	Panel1^ _Panel;
	FileType _Type;
	int _Count;
};
}
