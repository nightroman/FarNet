/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "Window.h"

namespace FarNet
{;
ref class FarWindowInfo : public IWindowInfo
{
public:
	FarWindowInfo(int index, bool full)
	{
		WindowInfo wi;
		wi.Pos = index;

		if (full)
		{
#pragma push_macro("ACTL_GETWINDOWINFO")
#undef ACTL_GETWINDOWINFO
			wi.Name = wi.TypeName = NULL;
			wi.NameSize = wi.TypeNameSize = 0;
			if (!Info.AdvControl(Info.ModuleNumber, ACTL_GETWINDOWINFO, &wi))
				throw gcnew InvalidOperationException("GetWindowInfo:" + index + " failed.");

			CBox name(wi.NameSize), typeName(wi.TypeNameSize);
			wi.Name = name;
			wi.TypeName = typeName;
			if (!Info.AdvControl(Info.ModuleNumber, ACTL_GETWINDOWINFO, &wi))
				throw gcnew InvalidOperationException("GetWindowInfo:" + index + " failed.");

			_Name = gcnew String(name);
			_KindName = gcnew String(typeName);
#pragma pop_macro("ACTL_GETWINDOWINFO")
		}
		else
		{
			if (!Info.AdvControl(Info.ModuleNumber, ACTL_GETSHORTWINDOWINFO, &wi))
				throw gcnew InvalidOperationException("GetWindowInfo:" + index + " failed.");
		}

		_Current = wi.Current != 0;
		_Modified = wi.Modified != 0;
		_Kind = (WindowKind)wi.Type;
	}
	virtual property bool Current { bool get() { return _Current; } }
	virtual property bool Modified { bool get() { return _Modified; } }
	virtual property String^ Name { String^ get() { return _Name; } }
	virtual property String^ KindName { String^ get() { return _KindName; } }
	virtual property WindowKind Kind { WindowKind get() { return _Kind; } }
private:
	bool _Current;
	bool _Modified;
	String^ _Name;
	String^ _KindName;
	WindowKind _Kind;
};

int Window::Count::get()
{
	return (int)Info.AdvControl(Info.ModuleNumber, ACTL_GETWINDOWCOUNT, 0);
}

IWindowInfo^ Window::GetInfoAt(int index, bool full)
{
	return gcnew FarWindowInfo(index, full);
}

WindowKind Window::Kind::get()
{
	WindowInfo wi;
	wi.Pos = -1;
	return Info.AdvControl(Info.ModuleNumber, ACTL_GETSHORTWINDOWINFO, &wi) ? (FarNet::WindowKind)wi.Type : FarNet::WindowKind::None;
}

WindowKind Window::GetKindAt(int index)
{
	WindowInfo wi;
	wi.Pos = index;
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_GETSHORTWINDOWINFO, &wi))
		throw gcnew InvalidOperationException("GetWindowType:" + index + " failed.");
	
	return (FarNet::WindowKind)wi.Type;
}

void Window::SetCurrentAt(int index)
{
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_SETCURRENTWINDOW, (void*)(INT_PTR)index))
		throw gcnew InvalidOperationException("SetCurrentWindow:" + index + " failed.");
}

bool Window::Commit()
{
	return Info.AdvControl(Info.ModuleNumber, ACTL_COMMIT, 0) != 0;
}

}
